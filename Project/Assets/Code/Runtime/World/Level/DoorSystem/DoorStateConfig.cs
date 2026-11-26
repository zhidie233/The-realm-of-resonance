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

    [CreateAssetMenu(fileName = "Door Setting", menuName = "项目/Level/Door Setting")]
    public class DoorStateConfig : ScriptableObject
    {
        public float TransitionDuration = 1;
        [Space]
        public Vector3 CloseRotation;
        public Vector3 OpenInsideRotation;
        public Vector3 OpenOutsideRotation;
        [Space]
        public AudioClip OpenSound;
        public AudioClip CloseSound;
        [Space]
        public AnimationCurve TransitionEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}
