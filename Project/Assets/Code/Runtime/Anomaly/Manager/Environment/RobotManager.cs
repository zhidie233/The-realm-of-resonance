using Resonance.Foundation;
using Resonance.Domain.Anomaly;
using Resonance.Core;
using Resonance.Bots;
using Resonance.Multiplayer;

namespace Resonance.Anomaly
{
    public class RobotManager : EnvironmentEffectManager
    {
        private readonly ThresholdIntervalTracker _intervalTracker = new ThresholdIntervalTracker();

        protected override void Start()
        {
            base.Start();
            if (!enabled) return;
            _intervalTracker.SyncBaseline(currentResonance, ProjectConfigRoot.Instance.robotConfig.robotSpawnInterval);
        }

        public override void UpdateEffect()
        {
            if (!NetworkWrapper.IsMasterClient) return;

            int interval = ProjectConfigRoot.Instance.robotConfig.robotSpawnInterval;
            if (!_intervalTracker.TryConsumeNextMilestone(currentResonance, interval)) return;

            AIManager.Instance.SpawnBot(null, Team.Team3);
        }

        protected override void OnMatchReset()
        {
            _intervalTracker.SyncBaseline(currentResonance, ProjectConfigRoot.Instance.robotConfig.robotSpawnInterval);
        }
    }
}
