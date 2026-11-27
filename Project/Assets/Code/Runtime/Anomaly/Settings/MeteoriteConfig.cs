using UnityEngine;

namespace Resonance.Anomaly
{
    [CreateAssetMenu(fileName = "Meteorite", menuName = "项目/共振/Meteorite")]
    public class MeteoriteConfig : ScriptableObject
    {
        public const int minResonanceValue = 0;
        public const int maxResonanceValue = 5000;
        public const float minDamageValue = 0f;
        public const float maxDamageValue = 50f;

        [Header("触发概率")]
        [Range(0, 1)] public float triggerProbability = 0.3f;

        [Header("陨石设置")]
        public GameObject meteoritePrefab;
        public float meteoriteCountdown = 1f;
        public float meteoriteFallSpeed = 20f;

        [Header("伤害设置")]
        public AnimationCurve damageCurve = AnimationCurve.Linear(
            minResonanceValue, minDamageValue,
            maxResonanceValue, maxDamageValue);
        public int baseDamage = 100;

        [Header("标记设置")]
        public GameObject arrowMarkerPrefab;
        public float markerDisplayTime = 5f;
    }
}
