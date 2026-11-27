using UnityEngine;

namespace Resonance.Anomaly
{
    public abstract class EnvironmentEffectManager : MonoBehaviour, IResonanceEffect
    {
        protected float currentResonance;
        private bool _isInitialized;

        protected virtual void OnEnable()
        {
            if (ResonanceManager.Instance != null)
            {
                ResonanceManager.Instance.RegisterEffect(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (ResonanceManager.Instance != null)
            {
                ResonanceManager.Instance.UnregisterEffect(this);
            }
        }

        public void Initialize(float initialResonance)
        {
            if (_isInitialized) return;
            _isInitialized = true;
            currentResonance = initialResonance;
            OnInitialize();
            if (enabled)
            {
                UpdateEffect();
            }
        }

        public void ApplyResonance(float resonance)
        {
            currentResonance = resonance;
            if (enabled)
            {
                UpdateEffect();
            }
        }

        public void ResetEffect(float baselineResonance)
        {
            currentResonance = baselineResonance;
            OnMatchReset();
        }

        protected virtual void OnInitialize() { }
        public abstract void UpdateEffect();
        protected virtual void OnMatchReset() { }
    }
}
