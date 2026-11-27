using UnityEngine;
using Resonance.Foundation;
using Resonance.Domain.Anomaly;
using Resonance.Core;
using Resonance.Multiplayer;
using Resonance.World;

namespace Resonance.Anomaly
{
    public class SupplyManager : EnvironmentEffectManager
    {
        [Header("掉落区域")]
        private BoxCollider dropArea;

        private readonly ThresholdIntervalTracker _intervalTracker = new ThresholdIntervalTracker();
        private int lastIndex = 0;

        protected override void Start()
        {
            base.Start();
            if (!enabled) return;

            dropArea = WorldDropZone.FindDropArea();
            if (dropArea == null)
            {
                enabled = false;
                return;
            }

            _intervalTracker.SyncBaseline(currentResonance, ProjectConfigRoot.Instance.supplyConfig.supplyInterval);
        }

        public override void UpdateEffect()
        {
            if (!NetworkWrapper.IsMasterClient) return;

            int interval = ProjectConfigRoot.Instance.supplyConfig.supplyInterval;
            if (!_intervalTracker.TryConsumeNextMilestone(currentResonance, interval)) return;

            DropItem();
        }

        protected override void OnMatchReset()
        {
            _intervalTracker.SyncBaseline(currentResonance, ProjectConfigRoot.Instance.supplyConfig.supplyInterval);
            lastIndex = 0;
        }

        private void DropItem()
        {
            if (dropArea == null) return;

            int[] packIDs = ProjectConfigRoot.Instance.supplyConfig.supplyPackIDs;
            if (packIDs == null || packIDs.Length == 0) return;

            EventBus.Publish(new AirSupplyRequestedEvent(WorldDropZone.SampleWorldPoint(dropArea, 1f), packIDs[lastIndex]));

            lastIndex++;
            if (lastIndex >= packIDs.Length)
                lastIndex = 0;
        }
    }
}
