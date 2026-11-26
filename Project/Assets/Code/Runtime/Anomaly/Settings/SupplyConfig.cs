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

    [CreateAssetMenu(fileName = "SupplyConfig", menuName = "项目/共振/SupplyConfig")]
    public class SupplyConfig : ScriptableObject
    {
        public int supplyInterval = 50;
        [Header("掉落包裹")]
        [Tooltip("对应 LootDispatcher 中 AvailablePacks 的索引")]
        public int[] supplyPackIDs = new int[] { 0, 1 };
    }
}
