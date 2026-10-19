using UnityEngine;

namespace Game.Runtime.Level
{
    public class MedicalKit : NetworkedItem
    {
        [GameToggle] public bool autoRespawn = false;
        [Range(0, 100)]
        public int health = 25;

        void OnTriggerEnter(Collider m_other)
        {
            if (!m_other.isLocalPlayerCollider()) return;

            var pdm = m_other.transform.root.GetComponent<PlayerHealthManagerBase>();
            if (pdm == null) { return; }

            //玩家生命值满时不拾取
            if (pdm.GetHealth() >= pdm.GetMaxHealth()) return;

            GameEvents.DispatchPickUpHealth(health);

            //此医疗包是否在一段时间后重生？
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