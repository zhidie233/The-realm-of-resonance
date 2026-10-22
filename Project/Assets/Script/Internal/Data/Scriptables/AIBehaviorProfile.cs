using System.Collections.Generic;
using UnityEngine;
   [CreateAssetMenu(fileName = "AI Behavior Settings", menuName = "项目/AI/Behavior Settings")]
   public class AIBehaviorProfile : ScriptableObject
   {
       [Header("Settings")]
       public AIBrainAction agentBehave = AIBrainAction.Agressive;
       public AIBotWeaponSpread weaponAccuracy = AIBotWeaponSpread.Casual;
       [GameToggle] public bool GetRandomTargetOnStart = true;
       [GameToggle] public bool forceFollowAtHalfHealth = true;
       [GameToggle] public bool checkEnemysWhenHaveATarget = true;
       public AIOutOfRangeAction targetOutRangeBehave = AIOutOfRangeAction.KeepFollowingBasedOnState;

       [Header("Cover")]
       public float maxCoverTime = 10;
       public float coverColdDown = 15;
       [Tooltip("随机选择掩体点作为目标的概率")]
       [Range(0, 1)] public float randomCoverProbability = 0.1f;
   }
