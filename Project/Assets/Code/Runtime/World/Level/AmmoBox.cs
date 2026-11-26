using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.World
{

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

            int weaponID = isGlobal ? -1 : ForGun;

            var ammoQuery = new AmmoPickupQuery(Bullets, Projectiles, weaponID); EventBus.Publish(ammoQuery); if (ammoQuery.Consumed)
            {
                if (PickSound) AudioSource.PlayClipAtPoint(PickSound, transform.position, 1.0f);
                //此弹药是否在一定时间后重新出现？
                if (autoRespawn)
                {
                    PickupSpawner.Instance.RespawnAfter(this);
                }
                else
                {
                    DestroySync();
                }
            }
        }
    }
}
