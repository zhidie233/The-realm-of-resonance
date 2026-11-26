using System;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Actors
{

    [Serializable]
    public class HitBoxInfo
    {
        public HumanBodyBones Bone;
        [Range(0.5f, 10)] public float DamageMultiplier = 1.0f;
        public Collider collider;
    }
}
