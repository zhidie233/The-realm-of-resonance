using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
namespace Resonance.UI
{

    public class ModifiableLayoutWrapper : MonoBehaviour
    {
        public bool allowModifySize = true;
        public Vector2 allowedSizeRange = new Vector2(0.5f, 1.7f);
        public bool allowModifyOpacity = true;
        public Vector2 allowedOpacity = new Vector2(0.02f, 1);
    }
}
