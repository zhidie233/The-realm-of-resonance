using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using GFWK.Core.Motion;
using UnityEngine.Serialization;
using GFWK.Audio;
using Photon.Pun;

public class bl_ExplosionDamage : bl_ExplosionBase
{
    [FormerlySerializedAs("m_Type")]
    public ExplosionType explosionType = ExplosionType.Normal;
    [GFWorksToogle] public bool CheckRootsOnly = false;
    public float explosionDamage = 50f;
    public float explosionRadius = 50f;
    public float DisappearIn = 3f;
    public LayerMask detectLayers;
    public ShakerPresent shakerPresent;
    public string shakerKey = "explosion";

    private RaycastHit hitInfo;
    private BulletData cachedData;
    private GFWKPlayer creator;

    // is not remote take damage
    void Start()
    {
        if (cachedData == null)
        {
            if (explosionType == ExplosionType.Level)
            {
                cachedData = new BulletData()
                {
                    GFWKActor = bl_GameManager.Instance.LocalActor,
                    isNetwork = false,
                    Damage = explosionDamage,
                    Position = transform.position,

                };
                creator = bl_GFWK.LocalPlayer.GFWKActor;
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

    public override void InitExplosion(BulletData bulletData, GFWKPlayer fromPlayer)
    {
        cachedData = bulletData;
        creator = fromPlayer;
        if (cachedData.Damage > 0) explosionDamage = cachedData.Damage;

        SetupAudio();
    }

    // <param name="radius"></param>
    public override void SetRadius(float radius)
    {
        explosionRadius = radius;
    }

    // applying impact damage from the explosion to enemies
    private void DoDamage()
    {
        if (explosionType == ExplosionType.Shake && !bl_GameData.Instance.ArriveKitsCauseDamage)
            return;

        DoPlayersDamage();
        DoCollisionDamage();
    }

    // Apply damage to the real players The splash calculation for players is due by distance instead of detect colliders due to its simpler and performs better.
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
            //check if there is an obstacle between player and explosion
            if (!ExplosionCanHitTarget(pt, new Vector3(0, 0.6f, 0), pp) && !ExplosionCanHitTarget(pt, new Vector3(0, 0.15f, 0), pp)) continue;

            var pdm = p.transform.GetComponentInParent<bl_PlayerHealthManagerBase>();

            var odi = new DamageData();
            odi.Damage = CalculatePlayerDamage(p.transform, player);
            odi.Direction = transform.position;
            odi.From = creator.Name;
            odi.isHeadShot = false;
            odi.Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion;
            odi.GunID = cachedData.WeaponID;
            odi.Actor = bl_PhotonNetwork.LocalPlayer;
            
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

    // Apply damage to objects, items, bots, etc... if the collider is inside the explosion radius
    void DoCollisionDamage()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, explosionRadius, detectLayers, QueryTriggerInteraction.Ignore);
        List<string> Hited = new List<string>();

        foreach (Collider c in colls)
        {
            //the damage to real players is handled separately
            if (c.isLocalPlayerCollider() || c.CompareTag("Untagged")) continue;

            var damageable = c.transform.GetComponent<IGFWKDamageable>();
            if (damageable == null) continue;

            if (c.CompareTag(bl_GFWK.HITBOX_TAG) || c.CompareTag(bl_GFWK.AI_TAG))
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
            DamageData damageData = new DamageData()
            {
                Damage = (int)damage,
                Direction = transform.position,
                GFWKActor = creator,
                ActorViewID = creator.ActorViewID,
                GunID = cachedData.WeaponID,
                From = creator.Name,
            };
            damageData.Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion;

            if(damageData.GFWKActor == null)
            {
                Debug.Log($"Explosion actor '{creator.ActorViewID}' was not found in the scene, maybe left the match?");
                return;
            }
            damageable.ReceiveDamage(damageData);
        }
    }

    // When Explosion is local, and take player hit Send only shake movement
    void ApplyShake()
    {
        if (isMyInRange() == true)
        {
            bl_EventHandler.DoPlayerCameraShake(shakerPresent, "shakerKey");
        }
    }

    // calculate the damage it generates, based on the distance between the player and the explosion
    private int CalculatePlayerDamage(Transform trans, Player p)
    {
        if (p != null)
        {
            if (!isOneTeamMode)
            {
                if (bl_GameData.Instance.SelfGrenadeDamage && p == bl_PhotonNetwork.LocalPlayer)
                {

                }
                else
                {
                    if ((string)p.CustomProperties[PropertiesKeys.TeamKey] == myTeam)
                    {
                        return 0;
                    }
                }
            }
        }
        float distance = bl_UtilityHelper.Distance(transform.position, trans.position);
        return Mathf.Clamp((int)(explosionDamage * ((explosionRadius - distance) / explosionRadius)), 0, (int)explosionDamage);
    }

    // Do a simple check to see if there's anything between the explosion and the collider target if that is the case, the explosion should not make any effect to the target.
    // <param name="target"></param>
    // <param name="offset"></param>
    // <returns></returns>
    private bool ExplosionCanHitTarget(Transform target, Vector3 offset, bool rootOnly = true)
    {
        return ExplosionCanHitTarget(target, offset, target.position, rootOnly);
    }

    // Do a simple check to see if there's anything between the explosion and the collider target if that is the case, the explosion should not make any effect to the target.
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
                // If the collider has a rigidbody in the transform root, it will always hinder the raycast
                // so lets ignore it.
                if (target.IsChildOf(hitInfo.transform)) return true;

                if (hitInfo.transform == target) { return true; }
            }
        }
        return result;
    }

    // get players who are within the range of the explosion
    // <returns></returns>
    private List<Player> GetPlayersInRange()
    {
        List<Player> list = new List<Player>();
        foreach (Player p in bl_PhotonNetwork.PlayerList)
        {
            GameObject player = FindPhotonPlayer(p);
            if (player == null)
                return null;

            float distance = bl_UtilityHelper.Distance(transform.position, player.transform.position);
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
                    if (p != bl_PhotonNetwork.LocalPlayer)
                    {
                        if (p.GetPlayerTeam() != bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() && (distance <= explosionRadius))
                        {
                            list.Add(p);
                        }
                    }
                    else
                    {
                        if (bl_GameData.Instance.SelfGrenadeDamage)
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
                if (p != bl_PhotonNetwork.LocalPlayer)
                {
                    if (distance <= explosionRadius)
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (bl_GameData.Instance.SelfGrenadeDamage)
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

    // Calculate if player local in explosion radius
    // <returns></returns>
    private bool isMyInRange()
    {
        GameObject p = bl_GameManager.Instance.LocalPlayer;

        if (p == null)
        {
            return false;
        }
        if ((bl_UtilityHelper.Distance(this.transform.position, p.transform.position) <= this.explosionRadius))
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
        Source.maxDistance = bl_AudioController.Instance.maxExplosionDistance;
        Source.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
        Source.minDistance = bl_AudioController.Instance.maxExplosionDistance * 0.09f;
        Source.spatialize = true;
    }

    // <returns></returns>
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