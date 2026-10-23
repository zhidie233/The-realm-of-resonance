using UnityEngine;

public abstract class EnvironmentEffectManager : MonoBehaviour
{
    protected float currentResonance;

    protected virtual void Start()
    {
        if (ResonanceManager.Instance == null)
        {
            Debug.LogWarning($"[{GetType().Name}] 场景中不存在 ResonanceManager，共鸣环境效果已禁用。");
            enabled = false;
            return;
        }

        GameEvents.onResonanceChanged += OnResonanceChanged;
        currentResonance = ResonanceManager.Instance.GetCurrentResonance();
        UpdateEffect();
    }

    protected virtual void OnDestroy()
    {
        GameEvents.onResonanceChanged -= OnResonanceChanged;
    }

    void OnResonanceChanged(float newResonance)
    {
        currentResonance = newResonance;
        UpdateEffect();
    }

    public abstract void UpdateEffect();
}
