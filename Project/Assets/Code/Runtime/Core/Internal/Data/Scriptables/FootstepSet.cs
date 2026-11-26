using System;
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

    public class FootstepSet : ScriptableObject
    {
        public AudioGroup[] Groups;
        public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

        public AudioGroup GetGroupFor(string tag)
        {
            for (int i = 0; i < Groups.Length; i++)
            {
                if (Groups[i].Tag.Equals(tag))
                {
                    return Groups[i];
                }
            }
            return Groups[0];
        }

        [Serializable]
        public class AudioGroup
        {
            public string Tag;
            public AudioClip[] StepSounds;

            public AudioClip GetRandomClip()
            {
                return StepSounds[UnityEngine.Random.Range(0, StepSounds.Length)];
            }
        }
    }
}
