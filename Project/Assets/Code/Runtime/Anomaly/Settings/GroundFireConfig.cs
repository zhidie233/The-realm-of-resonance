using UnityEngine;

namespace Resonance.Anomaly
{
    [CreateAssetMenu(fileName = "GroundFireConfig", menuName = "项目/共振/GroundFireConfig")]
    public class GroundFireConfig : ScriptableObject
    {
        public const int minResonanceValue = 0;
        public const int maxResonanceValue = 5000;
        public const float minDamageValue = 5f;
        public const float maxDamageValue = 20f;

        [Header("激活阈值")]
        public float activationThreshold = 150f;

        [Header("伤害配置")]
        public AnimationCurve fireDamageCurve = AnimationCurve.Linear(
            minResonanceValue, minDamageValue,
            maxResonanceValue, maxDamageValue);

        [Header("视觉效果")]
        public GameObject fireEffectPrefab;
        public float effectScaleMultiplier = 0.5f;
    }
}
