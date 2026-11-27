using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
namespace Resonance.Anomaly
{

    public class ResonanceFacility : MonoBehaviour, IGameDamageable
    {
        [Header("设施配置")]
        public string resonanceKey;

        public virtual void ReceiveDamage(DamageInfo damageData)
        {
            if (damageData == null || string.IsNullOrEmpty(resonanceKey)) return;
            EventBus.Publish(new ResonanceImpulseEvent(resonanceKey, damageData.Damage));
        }
    }

}
