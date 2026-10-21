using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.Serialization;
using Photon.Pun;

public class ExplosionDamage : ExplosionBase
{
    [FormerlySerializedAs("m_Type")]
    public ExplosionType explosionType = ExplosionType.Normal;
    [GameToggle] public bool CheckRootsOnly = false;
    public float explosionDamage = 50f;
    public float explosionRadius = 50f;
    public float DisappearIn = 3f;
    public LayerMask detectLayers;
    public ShakePreset shakerPresent;
    public string shakerKey = "explosion";

    private RaycastHit hitInfo;
    private ShotData cachedData;
    private GamePlayer creator;

    // 不是远程受击
    void Start()
    {
        if (cachedData == null)
        {
            if (explosionType == ExplosionType.Level)
            {
                cachedData = new ShotData()
                {
                    GameActor = GameController.Instance.LocalActor,
                    isNetwork = false,
                    Damage = explosionDamage,
                    Position = transform.position,

                };
                creator = GameCore.LocalPlayer.GameActor;
            }
            else
            {
                Debug.LogWarning("Explosion has not been initialized.");
                return;
            }
        }

        if (!cachedData.isNetwork)
        {
            DoDamage();
            ApplyShake();
        }

        StartCoroutine(Init());
    }

    public override void InitExplosion(ShotData bulletData, GamePlayer fromPlayer)
    {
        cachedData = bulletData;
        creator = fromPlayer;
        if (cachedData.Damage > 0) explosionDamage = cachedData.Damage;

        SetupAudio();
    }

    public override void SetRadius(float radius)
    {
        explosionRadius = radius;
    }

    // 对敌人施加爆炸的冲击伤害
    private void DoDamage()
    {
        if (explosionType == ExplosionType.Shake && !GameConfig.Instance.AirSupplyCausesDamage)
            return;

        DoPlayersDamage();
        DoCollisionDamage();
    }

    // 对真人玩家施加伤害。玩家的波及范围按距离计算而不检测碰撞体，实现更简单、性能更好。
    void DoPlayersDamage()
    {
        List<Player> playersInRange = this.GetPlayersInRange();

        if (playersInRange == null || playersInRange.Count <= 0) return;

        foreach (Player player in playersInRange)
        {
            if (player == null)
            {
                Debug.LogError("Player " + player.NickName + " not found in this room.");
                continue;
            }

            GameObject p = FindPhotonPlayer(player);
            if (p == null) continue;

            var pt = p.transform;
            Vector3 pp = pt.position + Vector3.up;
            //检查玩家与爆炸点之间是否有障碍物
            if (!ExplosionCanHitTarget(pt, new Vector3(0, 0.6f, 0), pp) && !ExplosionCanHitTarget(pt, new Vector3(0, 0.15f, 0), pp)) continue;

            var pdm = p.transform.GetComponentInParent<PlayerHealthManagerBase>();

            var odi = new DamageInfo();
            odi.Damage = CalculatePlayerDamage(p.transform, player);
            odi.Direction = transform.position;
            odi.From = creator.Name;
            odi.isHeadShot = false;
            odi.Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion;
            odi.GunID = cachedData.WeaponID;
            odi.Actor = NetworkWrapper.LocalPlayer;
            
            // 检查团队关系并调整共鸣值
            if (!isOneTeamMode && player.GetPlayerTeam() == creator.Team)
            {
                // 爆炸伤害友军，降低共鸣值
                NotifyMasterClientResonanceChange("Teammate", odi.Damage);
            }
            else
            {
                // 爆炸伤害敌人，升高共鸣值
                NotifyMasterClientResonanceChange("Enemy", odi.Damage);
            }
            pdm?.DoDamage(odi);
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
            PhotonView shooterPhotonView = creator.m_actorView; // 获取发射者的PhotonView
            shooterPhotonView.RPC("RPC_RequestResonanceChange", RpcTarget.MasterClient, 
                resonanceKey, damage);
        }
    }

    // 对物体、物品、机器人等施加伤害，条件是碰撞体在爆炸半径内
    void DoCollisionDamage()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, explosionRadius, detectLayers, QueryTriggerInteraction.Ignore);
        List<string> Hited = new List<string>();

        foreach (Collider c in colls)
        {
            //对真人玩家的伤害单独处理
            if (c.isLocalPlayerCollider() || c.CompareTag("Untagged")) continue;

            var damageable = c.transform.GetComponent<IGameDamageable>();
            if (damageable == null) continue;

            if (c.CompareTag(GameCore.HITBOX_TAG) || c.CompareTag(GameCore.AI_TAG))
            {
                if (Hited.Contains(c.transform.root.name)) continue;
                if (!ExplosionCanHitTarget(c.transform.root, new Vector3(0, 0.6f, 0)) && !ExplosionCanHitTarget(c.transform.root, new Vector3(0, 0.15f, 0))) continue;

                Hited.Add(c.transform.root.name);
            }
            else
            {
                if (!ExplosionCanHitTarget(c.transform, new Vector3(0, 0.1f, 0), CheckRootsOnly)) continue;
            }

            int damage = CalculatePlayerDamage(c.transform, null);
            DamageInfo damageData = new DamageInfo()
            {
                Damage = (int)damage,
                Direction = transform.position,
                GameActor = creator,
                ActorViewID = creator.ActorViewID,
                GunID = cachedData.WeaponID,
                From = creator.Name,
            };
            damageData.Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion;

            if(damageData.GameActor == null)
            {
                Debug.Log($"Explosion actor '{creator.ActorViewID}' was not found in the scene, maybe left the match?");
                return;
            }
            damageable.ReceiveDamage(damageData);
        }
    }

    // 爆炸发生在本地且玩家被击中时，只发送震动
    void ApplyShake()
    {
        if (isMyInRange() == true)
        {
            GameEvents.DoPlayerCameraShake(shakerPresent, "shakerKey");
        }
    }

    // 根据玩家与爆炸点的距离计算产生的伤害
    private int CalculatePlayerDamage(Transform trans, Player p)
    {
        if (p != null)
        {
            if (!isOneTeamMode)
            {
                if (GameConfig.Instance.selfExplosionDamage && p == NetworkWrapper.LocalPlayer)
                {

                }
                else
                {
                    if ((string)p.CustomProperties[NetworkKeys.TeamKey] == myTeam)
                    {
                        return 0;
                    }
                }
            }
        }
        float distance = CommonUtil.Distance(transform.position, trans.position);
        return Mathf.Clamp((int)(explosionDamage * ((explosionRadius - distance) / explosionRadius)), 0, (int)explosionDamage);
    }

    // 简单检查爆炸点与目标碰撞体之间是否有遮挡，若有则爆炸不应影响目标。
    // <param name="target"></param>
    // <param name="offset"></param>
    // <returns></returns>
    private bool ExplosionCanHitTarget(Transform target, Vector3 offset, bool rootOnly = true)
    {
        return ExplosionCanHitTarget(target, offset, target.position, rootOnly);
    }

    // 简单检查爆炸点与目标碰撞体之间是否有遮挡，若有则爆炸不应影响目标。
    // <param name="target"></param>
    // <param name="offset"></param>
    // <param name="targetPosition"></param>
    // <param name="rootOnly"></param>
    // <returns></returns>
    private bool ExplosionCanHitTarget(Transform target, Vector3 offset, Vector3 targetPosition, bool rootOnly = true)
    {
        bool result = false;
        Vector3 rhs = transform.position + offset;
        Vector3 normalized = ((targetPosition + offset) - rhs).normalized;

        if (Physics.Raycast(rhs, normalized, out hitInfo, explosionRadius))
        {
            if (rootOnly)
            {
                if (hitInfo.transform.root == target) { return true; }
            }
            else
            {
                // 若碰撞体的 transform 根节点上有 rigidbody，它会一直挡住射线检测
                // 所以忽略它。
                if (target.IsChildOf(hitInfo.transform)) return true;

                if (hitInfo.transform == target) { return true; }
            }
        }
        return result;
    }

    // 获取爆炸范围内的玩家
    // <returns></returns>
    private List<Player> GetPlayersInRange()
    {
        List<Player> list = new List<Player>();
        foreach (Player p in NetworkWrapper.PlayerList)
        {
            GameObject player = FindPhotonPlayer(p);
            if (player == null)
                return null;

            float distance = CommonUtil.Distance(transform.position, player.transform.position);
            if (!isOneTeamMode)
            {
                if (!creator.isRealPlayer)
                {
                    if (p.GetPlayerTeam() != creator.Team && (distance <= explosionRadius))
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (p != NetworkWrapper.LocalPlayer)
                    {
                        if (p.GetPlayerTeam() != NetworkWrapper.LocalPlayer.GetPlayerTeam() && (distance <= explosionRadius))
                        {
                            list.Add(p);
                        }
                    }
                    else
                    {
                        if (GameConfig.Instance.selfExplosionDamage)
                        {
                            if (distance <= explosionRadius)
                            {
                                list.Add(p);
                            }
                        }
                    }
                }
            }
            else
            {
                if (p != NetworkWrapper.LocalPlayer)
                {
                    if (distance <= explosionRadius)
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (GameConfig.Instance.selfExplosionDamage)
                    {
                        if (distance <= explosionRadius)
                        {
                            list.Add(p);
                        }
                    }
                }
            }
        }
        return list;
    }

    // 计算本地玩家是否在爆炸半径内
    // <returns></returns>
    private bool isMyInRange()
    {
        GameObject p = GameController.Instance.LocalPlayer;

        if (p == null)
        {
            return false;
        }
        if ((CommonUtil.Distance(this.transform.position, p.transform.position) <= this.explosionRadius))
        {
            return true;
        }
        return false;
    }

    private void SetupAudio()
    {
        var Source = GetComponent<AudioSource>();
        if (Source == null) return;

        Source.spatialBlend = 1;
        Source.maxDistance = AudioManager.Instance.maxExplosionDistance;
        Source.rolloffMode = AudioManager.Instance.audioRolloffMode;
        Source.minDistance = AudioManager.Instance.maxExplosionDistance * 0.09f;
        Source.spatialize = true;
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(DisappearIn / 2);
        Destroy(gameObject);
    }

    [System.Serializable]
    public enum ExplosionType
    {
        Normal,
        Shake,
        Level
    }
}