using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GravityConfig", menuName = "Resonance/Settings/GravityConfig")]
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