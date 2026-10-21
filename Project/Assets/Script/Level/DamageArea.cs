using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public AreaType m_Type = AreaType.OneTime;
    [Range(1, 100)] public int Damage = 5;

    private bool isPlayerCaused = false;
    private DamageInfo cacheInformation;

    private List<PlayerHealthManagerBase> AllHitted = new List<PlayerHealthManagerBase>();
    private bool isNetwork = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isLocalPlayerCollider())
        {
            var pdm = other.transform.GetComponent<PlayerHealthManagerBase>();
            if (pdm != null)
            {
                if (m_Type == AreaType.Repeat)
                {
                    if (isPlayerCaused)
                    {
                        cacheInformation.Direction = transform.position;
                        int gi = GameConfig.Instance.GetWeaponID("Molotov");
                        if (gi > 0)
                        {
                            cacheInformation.GunID = gi;
                        }

                        pdm.DoRepeatHitDamage(new PlayerHealthManagerBase.RepeatHitDamage()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                            DamageInfo = cacheInformation
                        });
                        AllHitted.Add(pdm);
                    }
                    else
                    {
                        pdm.DoRepeatHitDamage(new PlayerHealthManagerBase.RepeatHitDamage()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                        });
                    }
                }
                else if (m_Type == AreaType.OneTime)
                {
                    DamageInfo info = new DamageInfo();
                    info.Damage = Damage;
                    info.Direction = transform.position;
                    info.Cause = DamageCause.Fire;
                    pdm.DoDamage(info);
                }
            }
        }
        else if (other.transform.CompareTag("AI") && !isNetwork)
        {
            if (isPlayerCaused)
            {
                var ash = other.transform.root.GetComponent<PlayerHealthManagerBase>();
                if (ash != null)
                {
                    if (!AllHitted.Contains(ash))
                    {
                        cacheInformation.Direction = transform.position;
                        int gi = GameConfig.Instance.GetWeaponID("Molotov");
                        if (gi > 0)
                        {
                            cacheInformation.GunID = gi;
                        }
                        ash.DoRepeatHitDamage(new PlayerHealthManagerBase.RepeatHitDamage()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                            DamageInfo = cacheInformation
                        });
                        AllHitted.Add(ash);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isLocalPlayerCollider())
        {
            var pdm = other.transform.GetComponent<PlayerHealthManagerBase>();
            if (pdm != null)
            {
                if (m_Type == AreaType.Repeat)
                {
                    pdm.CancelRepeatHit();
                    AllHitted.Remove(pdm);
                }
            }
        }
        else if (other.transform.CompareTag("AI") && !isNetwork)
        {
            var ash = other.transform.root.GetComponent<PlayerHealthManagerBase>();
            if (ash != null)
            {
                if (AllHitted.Contains(ash)) { AllHitted.Remove(ash); }
                if (m_Type == AreaType.Repeat)
                {
                    ash.CancelRepeatHit();
                }
            }
        }
    }

    private void OnDisable()
    {
        cacheInformation = null;
        foreach(var p in AllHitted)
        {
            if (p == null) continue;
            p.CancelRepeatHit();
        }
    }

    public void SetInfo(string from, bool isNetwork)
    {
        isPlayerCaused = true;
        this.isNetwork = isNetwork;
        cacheInformation = new DamageInfo();
        cacheInformation.Actor = NetworkWrapper.LocalPlayer;
        cacheInformation.Cause = DamageCause.Player;
        cacheInformation.From = from;
    }

    private void OnDrawGizmosSelected()
    {
        SphereCollider c = GetComponent<SphereCollider>();
        if (c == null) return;
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(c.center), c.radius);
        Gizmos.DrawWireSphere(transform.TransformPoint(c.center), c.radius);
    }

    [System.Serializable]
    public enum AreaType
    {
        Repeat,
        OneTime,
    }
}