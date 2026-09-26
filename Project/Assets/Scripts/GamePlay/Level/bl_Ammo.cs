using UnityEngine;
using System.Collections;

namespace GFWK.Runtime.Level
{
    public class bl_Ammo : bl_NetworkItem
    {
        [GFWorksToogle] public bool isGlobal = true;
        [GFWorksToogle] public bool autoRespawn = false;
        [GunID] public int ForGun = 0;

        public int Bullets = 30;
        public int Projectiles = 2;
        public AudioClip PickSound;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_other"></param>
        void OnTriggerEnter(Collider m_other)
        {
            if (!m_other.isLocalPlayerCollider()) return;

            int gunID = isGlobal ? -1 : ForGun;

            if (bl_EventHandler.OnAmmo(Bullets, Projectiles, gunID))
            {
                if (PickSound) AudioSource.PlayClipAtPoint(PickSound, transform.position, 1.0f);
                //should this ammo reaper after certain time?
                if (autoRespawn)
                {
                    bl_ItemManagerBase.Instance.RespawnAfter(this);
                }
                else
                {
                    DestroySync();
                }
            }
        }
    }
}