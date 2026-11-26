using UnityEngine;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{

    [CreateAssetMenu(fileName = "ResonanceSuite", menuName = "项目/Config/Resonance Suite")]
    public class ResonanceSuiteConfig : ScriptableObject
    {
        [GameToggle] public bool resonanceEnabled = true;
        [ScriptableDrawer] public ResonanceConfig resonanceConfig;
        [ScriptableDrawer] public GravityConfig gravityConfig;
        [ScriptableDrawer] public GroundFireConfig groundFireConfig;
        [ScriptableDrawer] public MeteoriteConfig meteoriteConfig;
        [ScriptableDrawer] public RobotConfig robotConfig;
        [ScriptableDrawer] public SupplyConfig supplyConfig;
    }
}
