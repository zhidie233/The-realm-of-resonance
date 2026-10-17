using UnityEngine;

namespace Game.Runtime.Level
{
    public class MedicalKit : NetworkItem
    {
        [GameToggle] public bool autoRespawn = false;
        [Range(0, 100)]
        public int health = 25;

        void OnTriggerEnter(Collider m_other)
        {
            if (!m_other.isLocalPlayerCollider()) return;

            var pdm = m_other.transform.root.GetComponent<PlayerHealthManagerBase>();
            if (pdm == null) { return; }

            //don't pickup if the player has max health
            if (pdm.GetHealth() >= pdm.GetMaxHealth()) return;

            GameEvents.DispatchPickUpHealth(health);

            //should this health kit respawn after certain time?
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