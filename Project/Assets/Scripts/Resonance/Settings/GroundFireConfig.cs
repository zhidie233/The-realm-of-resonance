using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GroundFireConfig", menuName = "Resonance/Settings/GroundFireConfig")]
public class GroundFireConfig : ScriptableObject
{
    [Header("激活阈值")]
    public float activationThreshold = 150f;
    
    [Header("伤害配置")]
    public static int minResonanceValue = 0;
    public static int maxResonanceValue= 5000;
    public static float minDamageValue=5;
    public static float maxDamageValue=20;
    public AnimationCurve fireDamageCurve = AnimationCurve.Linear(minResonanceValue, minDamageValue, maxResonanceValue, maxDamageValue);
    
    [Header("视觉效果")]
    public GameObject fireEffectPrefab;
    public float effectScaleMultiplier = 0.5f;
}