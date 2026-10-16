﻿using UnityEngine;
using System.Collections;

namespace Game.Runtime.Level
{
    public class AmmoBox : NetworkItem
    {
        [GameToggle] public bool isGlobal = true;
        [GameToggle] public bool autoRespawn = false;
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

            if (GameEvents.OnAmmo(Bullets, Projectiles, gunID))
            {
                if (PickSound) AudioSource.PlayClipAtPoint(PickSound, transform.position, 1.0f);
                //should this ammo reaper after certain time?
                if (autoRespawn)
                {
                    ItemManagerBase.Instance.RespawnAfter(this);
                }
                else
                {
                    DestroySync();
                }
            }
        }
    }
}