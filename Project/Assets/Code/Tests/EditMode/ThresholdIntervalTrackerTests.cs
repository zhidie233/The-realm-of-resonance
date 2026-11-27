using NUnit.Framework;
using Resonance.Domain.Anomaly;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class ThresholdIntervalTrackerTests
    {
        [Test]
        public void TryConsumeNextMilestone_TriggersOnlyOnNewPositiveMultiples()
        {
            var tracker = new ThresholdIntervalTracker();
            tracker.SyncBaseline(0f, 50);

            Assert.IsFalse(tracker.TryConsumeNextMilestone(-100f, 50));
            Assert.IsFalse(tracker.TryConsumeNextMilestone(49.9f, 50));
            Assert.IsTrue(tracker.TryConsumeNextMilestone(50f, 50));
            Assert.IsFalse(tracker.TryConsumeNextMilestone(75f, 50));
            Assert.IsTrue(tracker.TryConsumeNextMilestone(160f, 50));
            Assert.AreEqual(3, tracker.LastTriggeredMultiple);
            Assert.IsFalse(tracker.TryConsumeNextMilestone(150f, 50));
        }

        [Test]
        public void GuardsAgainstNonPositiveInterval()
        {
            var tracker = new ThresholdIntervalTracker();
            tracker.SyncBaseline(100f, 0);
            Assert.AreEqual(-1, tracker.LastTriggeredMultiple);
            Assert.IsFalse(tracker.TryConsumeNextMilestone(100f, 0));
            Assert.IsFalse(tracker.TryConsumeNextMilestone(100f, -10));
        }
    }
}
