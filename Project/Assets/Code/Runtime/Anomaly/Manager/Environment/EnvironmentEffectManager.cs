using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;

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

            EventBus.Subscribe<ResonanceValueChangedEvent>(HandleResonanceChanged);
            EventBus.Subscribe<RoundEndedEvent>(HandleRoundEnded);
            EventBus.Subscribe<RoomPropertiesResetEvent>(HandleRoomReset);
            currentResonance = ResonanceManager.Instance.GetCurrentResonance();
            UpdateEffect();
        }

        protected virtual void OnDestroy()
        {
            EventBus.Unsubscribe<ResonanceValueChangedEvent>(HandleResonanceChanged);
            EventBus.Unsubscribe<RoundEndedEvent>(HandleRoundEnded);
            EventBus.Unsubscribe<RoomPropertiesResetEvent>(HandleRoomReset);
        }

        public abstract void UpdateEffect();

        protected virtual void OnMatchReset() { }

        private void HandleResonanceChanged(ResonanceValueChangedEvent e)
        {
            currentResonance = e.Value;
            UpdateEffect();
        }

        private void HandleRoundEnded(RoundEndedEvent e) => OnMatchReset();

        private void HandleRoomReset(RoomPropertiesResetEvent e)
        {
            currentResonance = ResonanceManager.Instance != null ? ResonanceManager.Instance.GetCurrentResonance() : 0f;
            OnMatchReset();
        }
    }
}
