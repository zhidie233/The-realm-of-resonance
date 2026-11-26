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

    [CreateAssetMenu(fileName = "RobotConfig", menuName = "项目/共振/RobotConfig")]
    public class RobotConfig : ScriptableObject
    {
        public int robotSpawnInterval=1000;
    }
}
