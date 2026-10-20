using UnityEngine;
using System.Collections;

   public class AmmoBox : NetworkedItem
   {
       [GameToggle] public bool isGlobal = true;
       [GameToggle] public bool autoRespawn = false;
       [FirearmID] public int ForGun = 0;

       public int Bullets = 30;
       public int Projectiles = 2;
       public AudioClip PickSound;

       void OnTriggerEnter(Collider m_other)
       {
           if (!m_other.isLocalPlayerCollider()) return;

           int gunID = isGlobal ? -1 : ForGun;

           if (GameEvents.OnAmmo(Bullets, Projectiles, gunID))
           {
               if (PickSound) AudioSource.PlayClipAtPoint(PickSound, transform.position, 1.0f);
               //此弹药是否在一定时间后重新出现？
               if (autoRespawn)
               {
                   ItemRespawnerBase.Instance.RespawnAfter(this);
               }
               else
               {
                   DestroySync();
               }
           }
       }
   }