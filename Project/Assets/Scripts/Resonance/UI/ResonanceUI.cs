using TMPro;
using UnityEngine;

public class ResonanceUI : MonoBehaviour
{
    private float currentResonance;
    public TextMeshProUGUI resonanceValueText;

    private void Start()
    {
        if (ResonanceManager.Instance == null)
        {
            Debug.LogWarning("[ResonanceUI] 场景中不存在 ResonanceManager，共鸣 UI 已禁用。");
            enabled = false;
            return;
        }

        ResonanceManager.Instance.OnResonanceChanged += OnResonanceChanged;
        currentResonance = ResonanceManager.Instance.GetCurrentResonance();
    }
    
    protected virtual void OnDestroy()
    {
        if (ResonanceManager.Instance != null)
            ResonanceManager.Instance.OnResonanceChanged -= OnResonanceChanged;
    }
    
    protected virtual void OnResonanceChanged(float newResonance)
    {
        currentResonance = newResonance;
        UpdateCurrentResonanceValue();
    }

    void UpdateCurrentResonanceValue()
    {
        resonanceValueText.text = currentResonance.ToString();
    }
}
