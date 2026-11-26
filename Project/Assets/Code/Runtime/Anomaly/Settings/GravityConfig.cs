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

    [CreateAssetMenu(fileName = "GravityConfig", menuName = "项目/共振/GravityConfig")]
    public class GravityConfig : ScriptableObject
    {
        [Header("重力变化配置")] 
        public static int minResonanceValue = -5000;
        public static int maxResonanceValue= 5000;
        public static float minGravityValue=-25;
        public static float maxGravityValue=25;
        public AnimationCurve jumpForce = AnimationCurve.Linear(minResonanceValue, minGravityValue, 
            maxResonanceValue, maxGravityValue);

        public int GetMaxResonanceValue()
        {
            return maxResonanceValue;
        }
    }
}
