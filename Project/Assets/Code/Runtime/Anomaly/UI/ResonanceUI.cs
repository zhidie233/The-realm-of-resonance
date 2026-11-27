using TMPro;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;

namespace Resonance.Anomaly
{
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

            EventBus.Subscribe<ResonanceValueChangedEvent>(HandleResonanceChanged);
            UpdateValue(ResonanceManager.Instance.GetCurrentResonance());
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ResonanceValueChangedEvent>(HandleResonanceChanged);
        }

        private void HandleResonanceChanged(ResonanceValueChangedEvent e)
        {
            UpdateValue(e.Value);
        }

        private void UpdateValue(float newResonance)
        {
            currentResonance = newResonance;
            if (resonanceValueText != null)
            {
                resonanceValueText.text = currentResonance.ToString();
            }
        }
    }
}
