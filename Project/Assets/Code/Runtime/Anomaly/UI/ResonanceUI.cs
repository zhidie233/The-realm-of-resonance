using TMPro;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.UI;
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

            EventBus.Subscribe<ResonanceValueChangedEvent>(OnEvt_onResonanceChanged_OnResonanceChanged);
            OnResonanceChanged(ResonanceManager.Instance.GetCurrentResonance());
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe<ResonanceValueChangedEvent>(OnEvt_onResonanceChanged_OnResonanceChanged);
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

        private void OnEvt_onResonanceChanged_OnResonanceChanged(ResonanceValueChangedEvent e) => OnResonanceChanged(e.Value);
    }
}
