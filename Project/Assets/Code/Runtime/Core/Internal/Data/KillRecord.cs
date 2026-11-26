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
    [System.Serializable]
    public class KillRecord
    {
        public string Killer;
        public string Killed;
        public string KillMethod;
        public bool byHeadShot = false;
    }
}
