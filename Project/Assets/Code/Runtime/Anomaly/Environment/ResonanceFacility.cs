using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.UI;
namespace Resonance.Anomaly
{

    public class ResonanceFacility : MonoBehaviour, IGameDamageable
    {
        [Header("设施配置")]
        public string resonanceKey;

        public virtual void ReceiveDamage(DamageInfo damageData)
        {

        }
    }

}
