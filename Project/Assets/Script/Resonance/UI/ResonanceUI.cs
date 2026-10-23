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
            Debug.LogWarning("[ResonanceUI] 场景中不存在 ResonanceManager，共振 UI 已禁用。");
            enabled = false;
            return;
        }

        GameEvents.onResonanceChanged += OnResonanceChanged;
        OnResonanceChanged(ResonanceManager.Instance.GetCurrentResonance());
    }

    void OnDestroy()
    {
        GameEvents.onResonanceChanged -= OnResonanceChanged;
    }

    void OnResonanceChanged(float newResonance)
    {
        currentResonance = newResonance;
        UpdateCurrentResonanceValue();
    }

    void UpdateCurrentResonanceValue()
    {
        resonanceValueText.text = currentResonance.ToString();
    }
}
