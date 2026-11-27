using UnityEngine;

namespace Resonance.Anomaly
{
    [CreateAssetMenu(fileName = "GravityConfig", menuName = "项目/共振/GravityConfig")]
    public class GravityConfig : ScriptableObject
    {
        public const int minResonanceValue = -5000;
        public const int maxResonanceValue = 5000;
        public const float minGravityValue = -25f;
        public const float maxGravityValue = 25f;

        [Header("重力变化配置")]
        public AnimationCurve jumpForce = AnimationCurve.Linear(
            minResonanceValue, minGravityValue,
            maxResonanceValue, maxGravityValue);

        public int GetMaxResonanceValue() => maxResonanceValue;
    }
}
