using System;

namespace Resonance.Domain.Anomaly
{
    /// <summary>
    /// Tracks positive integer interval crossings without re-triggering already passed milestones.
    /// </summary>
    public sealed class ThresholdIntervalTracker
    {
        public int LastTriggeredMultiple { get; private set; } = -1;

        public void SyncBaseline(float currentValue, int interval)
        {
            LastTriggeredMultiple = interval <= 0 ? -1 : (int)Math.Floor(currentValue / interval);
        }

        public bool TryConsumeNextMilestone(float currentValue, int interval)
        {
            if (interval <= 0) return false;

            int currentMultiple = (int)Math.Floor(currentValue / interval);
            if (currentMultiple <= 0 || currentMultiple <= LastTriggeredMultiple)
            {
                return false;
            }

            LastTriggeredMultiple = currentMultiple;
            return true;
        }

        public void Reset()
        {
            LastTriggeredMultiple = -1;
        }
    }
}
