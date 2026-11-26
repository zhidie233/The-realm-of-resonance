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

            EventBus.Subscribe<ResonanceValueChangedEvent>(OnEvt_onResonanceChanged_OnResonanceChanged);
            currentResonance = ResonanceManager.Instance.GetCurrentResonance();
            UpdateEffect();
        }

        protected virtual void OnDestroy()
        {
            EventBus.Unsubscribe<ResonanceValueChangedEvent>(OnEvt_onResonanceChanged_OnResonanceChanged);
        }

        void OnResonanceChanged(float newResonance)
        {
            currentResonance = newResonance;
            UpdateEffect();
        }

        public abstract void UpdateEffect();

        private void OnEvt_onResonanceChanged_OnResonanceChanged(ResonanceValueChangedEvent e) => OnResonanceChanged(e.Value);
    }
}
