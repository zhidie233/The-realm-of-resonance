using Photon.Pun;
using UnityEngine;

// Game 默认的子弹处理脚本。想用自己的子弹脚本，就继承 ProjectileBase 类，实现要覆盖的函数，再挂到子弹预制体上。使用自定义子弹预制体请查看 Bullet 文档。可以参考本脚本了解如何对玩家和机器人施加伤害。
public class ShotController : ProjectileBase
{
    // 启用后，每次碰撞都会检查被击中的物体是否有 IGameDamageable 组件
    [GameToggle] public bool checkDamageables = false;
    public LayerMask HittableLayers;
    public TrailRenderer Trail = null;

    public int ActorViewID { get; set; }

    private Vector3 dir = Vector3.zero;
    private RaycastHit hit;
    private Transform _transform;
    private float distanceSinceLastCheck = 0;
    private float totalTraveledDistance = 0;
    private ShotData bulletData;
    private Vector3 velocity, newVelEuler = Vector3.zero; // bullet velocity
    private Vector3 newPos = Vector3.zero;   // bullet's new position
    private Vector3 oldPos = Vector3.zero;   // bullet's previous location
    private bool hasHit = false;             // has the bullet hit something?
    private Vector3 direction, gravity;               // direction bullet is travelling
    private float bulletRange, timeStep = 0;
    private bool realisticBallistics = false;
    private int fixedFrame = 0;
    private int detectionStep = 1;
    private LayerMask bulletMask;
    private int localLayer = -2;

    // 用武器传来的信息初始化子弹
    public override void InitProjectile(ShotData data)
    {
        // 子弹是池化的，必须把所有变量重置为初始值。
        ResetBullet();

        _transform = transform;
        bulletData = data;

        ActorViewID = data.ActorViewID;

        // 子弹飞行方向，按武器精度加入散布
        direction = _transform.TransformDirection(data.Inaccuracity);
        bulletRange = data.Range;
        newPos = _transform.position;
        oldPos = newPos;
        velocity = data.Speed * _transform.forward; // bullet's velocity determined by direction and bullet speed
        // 应用散布方向
        velocity += direction;
        gravity = MathHelpers.Gravity * data.DropFactor;

        realisticBallistics = GameConfig.Instance.realisticBullets;
        detectionStep = GameConfig.Instance.BulletHitDetectionStep;

        bulletMask = HittableLayers;
        if (data.IsLocalPlayer)
        {
            if (localLayer == -2) localLayer = GameConfig.TagsAndLayerSettings.GetLocalPlayerLayerIndex();
            bulletMask = HittableLayers & ~(1 << localLayer);
        }

        if (Trail != null) { if (!GameConfig.Instance.BulletTracer) { Destroy(Trail); } }
        // 子弹什么都没打中时安排销毁
        Invoke(nameof(Disable), 5);//TTL

    }

    // 把子弹所有属性设为初始值。
    void ResetBullet()
    {
        bulletData = null;
        hasHit = false;
        ActorViewID = 0;
        totalTraveledDistance = 0;
        fixedFrame = 0;
    }

    public override void OnFixedUpdate()
    {
        if (hasHit)
            return; // if bullet has already hit its max hits... exit

        Travel();
    }

    // 计算弹道运动并检测子弹飞行途中是否命中物体。
    void Travel()
    {
        timeStep = Time.fixedDeltaTime;
        if (!realisticBallistics)
        {
            // 计算直线弹道轨迹
            // 这里不施加外力或重力。
            newPos += velocity * timeStep;
        }
        else
        {
            //用 Heun 法计算新的速度和位置
            //公式：y_k+1 = y_k + timeStep * 0.5 * (f(t_k, y_k) + f(t_k+1, y_k+1))
            //其中 f(t_k+1, y_k+1) 用前向欧拉法计算：y_k+1 = y_k + timeStep * f(t_k, y_k)
            newVelEuler = velocity + timeStep * gravity;
            Vector3 newVel = velocity + timeStep * 0.5f * (gravity + gravity);

            newPos = newPos + timeStep * 0.5f * (velocity + newVelEuler);
            velocity = newVel;
        }

        if (fixedFrame % detectionStep == 0)
        {
            // 获取上次检测以来走过的轨迹
            dir = newPos - oldPos;
            distanceSinceLastCheck = dir.magnitude;
            totalTraveledDistance += distanceSinceLastCheck;

            if (distanceSinceLastCheck > 0)
            {

                // 检测途中是否命中物体
                dir /= distanceSinceLastCheck;
                if (Physics.Raycast(oldPos, dir, out hit, distanceSinceLastCheck, bulletMask, QueryTriggerInteraction.Ignore))
                {
                    newPos = hit.point;
                    if (OnHit(hit))
                    {
                        hasHit = true;
                        Disable();
                    }
                }
            }

            if (bulletRange > 0 && totalTraveledDistance > bulletRange)
            {
                Disable();
            }

            oldPos = _transform.position;  // set old position to current position
        }

        fixedFrame++;
        _transform.position = newPos;  // set current position to the new position
    }

    private void Disable()
    {
        ResetBullet();
        gameObject.SetActive(false);
    }

    private bool OnHit(RaycastHit hit)
    {
        Ray mRay = new Ray(_transform.position, _transform.forward);
        if (!bulletData.isNetwork)
        {
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic) // if we hit a rigi body... apply a force
            {
                float mAdjust = 1.0f / (Time.timeScale * (0.02f / Time.fixedDeltaTime));
                hit.rigidbody.AddForceAtPosition(((mRay.direction * bulletData.ImpactForce) / Time.timeScale) / mAdjust, hit.point);
            }
        }
        
        switch (hit.transform.tag) // decide what the bullet collided with and what to do with it
        {
            case "IgnoreBullet":
                return false;
            case "Projectile":
                // 两颗子弹相撞时不处理
                break;
            case GameCore.HITBOX_TAG://Send Damage for other players
                SendPlayerDamage(hit);
                break;
            case "AI":
                //子弹击中机器人碰撞体，检查是否需要施加伤害。
                SendBotDamage(hit);
                break;
            case "Player":
                //机器人击中了真人玩家
                SendPlayerDamageFromBot(hit);
                break;
            case "Facility":
                //攻击设施
                SendFacilityDamage(hit);
                break;
            case "Wood":
                InstanceHitParticle("decalw", hit);
                break;
            case "Concrete":
                InstanceHitParticle("decalc", hit);
                break;
            case "Metal":
                InstanceHitParticle("decalm", hit, true);
                break;
            case "Dirt":
                InstanceHitParticle("decals", hit);
                break;
            case "Water":
                InstanceHitParticle("decalwt", hit);
                break;
            default:
                InstanceHitParticle("decal", hit);
                break;
        }
        Disable();
        return true;
    }

    // 对真人玩家施加来自机器人或其他真人玩家的伤害
    void SendPlayerDamage(RaycastHit hit)
    {
        if (bulletData.isNetwork) return;
        
        IGameDamageable damageable = hit.transform.GetComponent<IGameDamageable>();
        if (damageable != null)
        {
            DamageInfo damageData = BuildBaseDamageData();
            // 检查是否是真人
            var playerRefs = hit.transform.root.GetComponent<PlayerReferencesHandler>();
            if (playerRefs != null)//是真人
            {
                if (isOneTeamMode)
                {
                    // 自由混战：所有玩家互为敌人，命中即提升共鸣值
                    NotifyMasterClientResonanceChange("Enemy", damageData.Damage);
                }
                else if (playerRefs.playerSettings.PlayerTeam == bulletData.GameActor.Team)
                {
                    if (!RoomSettings.Instance.CurrentRoomInfo.friendlyFire)//if hit a team mate player
                    {
                        Disable();
                        return;
                    }
                    else
                    {
                        // 攻击友军，降低共鸣值
                        NotifyMasterClientResonanceChange("Teammate", damageData.Damage);
                    }
                }
                else
                {
                    // 攻击敌人，升高共鸣值
                    NotifyMasterClientResonanceChange("Enemy", damageData.Damage);
                }
            }
            
            //不是真人
            var botRefs = hit.transform.root.GetComponent<AIBotReferences>();//判断是不是人机
            
            if (botRefs != null && !isOneTeamMode)//是人机
            {
                if (botRefs.PlayerTeam == bulletData.GameActor.Team)//判断是不是队友
                {
                    if (!RoomSettings.Instance.CurrentRoomInfo.friendlyFire)//if hit a team mate player
                    {
                        Disable();
                        return;
                    }
                    else
                    {
                        // 攻击友军，降低共鸣值
                        ResonanceManager.Instance.ChangeResonance("Teammate", damageData.Damage);
                    }
                }
                else
                {
                    // 攻击敌人，升高共鸣值
                    ResonanceManager.Instance.ChangeResonance("Enemy", damageData.Damage);
                }
            }
            
            //检查子弹来自机器人还是真人玩家。
            if (bulletData.GameActor != null)
            {
                damageData.Cause = bulletData.GameActor.isRealPlayer ? DamageCause.Player : DamageCause.Bot;
            }
            else
            {
                damageData.Cause = DamageCause.Player;
            }
            damageable.ReceiveDamage(damageData);
        }

        if (GameConfig.Instance.ShowBlood)
        {
            GameObject go = ObjectRecyclerBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            go.transform.parent = hit.transform;
        }
        Disable();
    }

    // 对机器人施加来自真人玩家或其他机器人的伤害
    void SendBotDamage(RaycastHit hit)
    {
        //只在本地玩家上施加伤害。也可以只在主客户端上处理以获得更"权威"的逻辑，但由于
        //主客户端的延迟会随其网络连接波动，可能影响房间内所有玩家。
        if (!bulletData.isNetwork)
        {
            if (bulletData.GameActor == null)
            {
                Debug.LogWarning($"Bullet ownerID {bulletData.ActorViewID} not found, ignore this if a player just enter in the match.");
            }
            if (hit.transform.root != null && bulletData.GameActor.Name == hit.transform.root.name) { return; }
            IGameDamageable damageable = hit.transform.GetComponent<IGameDamageable>();
            
            if (damageable != null)
            {
                //回调在 HitBox.cs 中
                var damageData = BuildBaseDamageData();
                //不是真人
                var botRefs = hit.transform.root.GetComponent<AIBotReferences>();//判断是不是人机
                if (botRefs != null)//是人机
                {
                    if (isOneTeamMode)
                    {
                        // 自由混战：所有人机均为敌人，命中即提升共鸣值
                        NotifyMasterClientResonanceChange("Enemy", damageData.Damage);
                    }
                    else if (botRefs.PlayerTeam == bulletData.GameActor.Team)//判断是不是队友
                    {
                        if (!RoomSettings.Instance.CurrentRoomInfo.friendlyFire)//if hit a team mate player
                        {
                            Disable();
                            return;
                        }
                        else
                        {
                            // 攻击友军，降低共鸣值
                            NotifyMasterClientResonanceChange("Teammate", damageData.Damage);
                        }
                    }
                    else
                    {
                        // 攻击敌人，升高共鸣值
                        NotifyMasterClientResonanceChange("Enemy", damageData.Damage);
                    }
                }
                damageable.ReceiveDamage(damageData);
            }
        }

        GameObject go = ObjectRecyclerBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
        go.transform.parent = hit.transform;
        Disable();
    }

    // 对真人玩家施加来自机器人的伤害。
    void SendPlayerDamageFromBot(RaycastHit hit)
    {
        if (bulletData.isNetwork) return;
        
        //机器人不像其他真人玩家那样击中玩家的判定盒，而是击中角色控制器碰撞体，
        //所以不通过判定盒脚本，而是直接和玩家生命值脚本通信
        var pdm = hit.transform.GetComponent<PlayerReferencesHandler>();
        if (pdm != null && bulletData.GameActor != null)
        {
            DamageInfo info = BuildBaseDamageData();
            if (isOneTeamMode)
            {
                // 自由混战：所有玩家互为敌人，被击中即提升共鸣值
                NotifyMasterClientResonanceChange("Enemy", info.Damage);
            }
            else if (pdm.playerSettings.PlayerTeam == bulletData.GameActor.Team)
            {
                if (!RoomSettings.Instance.CurrentRoomInfo.friendlyFire)//if hit a team mate player
                {
                    Disable();
                    return;
                }
                else
                {
                    // 攻击友军，降低共鸣值
                    NotifyMasterClientResonanceChange("Teammate", info.Damage);
                }
            }
            else
            {
                // 攻击敌人，升高共鸣值
                NotifyMasterClientResonanceChange("Enemy", info.Damage);
            }
            info.Actor = null;
            info.Cause = DamageCause.Bot;
            pdm.playerHealthManager.DoDamage(info);
            Disable();
        }
    }

    void SendFacilityDamage(RaycastHit hit)
    {
        if (bulletData.isNetwork) return;
        IGameDamageable damageable = hit.transform.GetComponent<IGameDamageable>();
        ResonanceFacility facility = hit.transform.GetComponent<ResonanceFacility>();
        if (damageable != null)
        {
            //回调在 HitBox.cs 中
            var damageData = BuildBaseDamageData();
            NotifyMasterClientResonanceChange(facility.resonanceKey, damageData.Damage);
            damageable.ReceiveDamage(damageData);
        }
    }
    
    void NotifyMasterClientResonanceChange(string resonanceKey, float damage)
    {
        if (ResonanceManager.Instance == null) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // 如果自己就是房主，直接修改
            ResonanceManager.Instance.ChangeResonance(resonanceKey, damage);
        }
        else
        {
            // 如果不是房主，通过RPC通知房主
            // 需要获取一个PhotonView来发送RPC
            PhotonView shooterPhotonView = bulletData.GameActor.m_actorView; // 获取发射者的PhotonView
            shooterPhotonView.RPC("RPC_RequestResonanceChange", RpcTarget.MasterClient, 
                resonanceKey, damage);
        }
    }

    void InstanceHitParticle(string poolPrefab, RaycastHit hit, bool overrideDamageable = false)
    {
        // 生成粒子
        GameObject go = ObjectRecyclerBase.Instance.Instantiate(poolPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        // 生成弹痕
        BulletMarkManagerBase.InstantiateDecal(hit);

        if (go != null)
            go.transform.parent = hit.transform;

        if (checkDamageables || overrideDamageable)
        {
            var damageable = hit.transform.GetComponent<IGameDamageable>();
            if (damageable == null) return;
            DamageInfo damageData = BuildBaseDamageData();
            damageData.Cause = DamageCause.Player;
            damageable.ReceiveDamage(damageData);
        }
    }

    DamageInfo BuildBaseDamageData()
    {
        DamageInfo data = new DamageInfo()
        {
            Damage = (int)bulletData.Damage,
            Direction = bulletData.Position,
            GameActor = bulletData.GameActor,
            ActorViewID = bulletData.ActorViewID,
            GunID = bulletData.WeaponID,
        };
        if (data.GameActor != null) { data.From = data.GameActor.Name; }
        return data;
    }   
}