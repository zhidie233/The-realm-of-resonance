using TMPro;
using UnityEngine;

public class ResonanceUI : MonoBehaviour
{
    private float currentResonance;
    public TextMeshProUGUI resonanceValueText;

    private void Start()
    {
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
