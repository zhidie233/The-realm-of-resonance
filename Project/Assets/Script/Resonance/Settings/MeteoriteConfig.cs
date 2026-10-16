using UnityEngine;

[CreateAssetMenu(fileName = "Meteorite", menuName = "Resonance/Meteorite")]
public class MeteoriteConfig : ScriptableObject
{
    [Header("触发概率")]
    [Range(0, 1)] public float triggerProbability = 0.3f;
    
    [Header("陨石设置")]
    public GameObject meteoritePrefab;
    public float meteoriteCountdown = 1f;
    public float meteoriteFallSpeed = 20f;
    
    [Header("伤害设置")]
    public static int minResonanceValue = 0;
    public static int maxResonanceValue= 5000;
    public static float minDamageValue=0;
    public static float maxDamageValue=50;
    public AnimationCurve damageCurve = AnimationCurve.Linear(minResonanceValue, minDamageValue, maxResonanceValue, maxDamageValue);
    public int baseDamage = 100;
    
    [Header("标记设置")]
    public GameObject arrowMarkerPrefab;
    public float markerDisplayTime = 5f;
}