using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentEffectManager : MonoBehaviour
{
    protected float currentResonance;
    
    protected virtual void Start()
    {
        ResonanceManager.Instance.OnResonanceChanged += OnResonanceChanged;
        currentResonance = ResonanceManager.Instance.GetCurrentResonance();
        UpdateEffect();
    }
    
    protected virtual void OnDestroy()
    {
        if (ResonanceManager.Instance != null)
            ResonanceManager.Instance.OnResonanceChanged -= OnResonanceChanged;
    }
    
    protected void OnResonanceChanged(float newResonance)
    {
        currentResonance = newResonance;
        UpdateEffect();
    }
    
    public abstract void UpdateEffect();
}