using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.AI
{
    [CreateAssetMenu(fileName = "AI Behavior Settings", menuName = "Game/AI/Behavior Settings")]
    public class AIBehaviorSettings : ScriptableObject
    {
        [Header("Settings")]
        public AIAgentBehave agentBehave = AIAgentBehave.Agressive;
        public AIWeaponAccuracy weaponAccuracy = AIWeaponAccuracy.Casual;
        [GameToggle] public bool GetRandomTargetOnStart = true;
        [GameToggle] public bool forceFollowAtHalfHealth = true;
        [GameToggle] public bool checkEnemysWhenHaveATarget = true;
        public AITargetOutRangeBehave targetOutRangeBehave = AITargetOutRangeBehave.KeepFollowingBasedOnState;

        [Header("Cover")]
        public float maxCoverTime = 10;
        public float coverColdDown = 15;
        [Tooltip("probability of get a cover point as random destination")]
        [Range(0, 1)] public float randomCoverProbability = 0.1f;
    }
}