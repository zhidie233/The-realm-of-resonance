using NUnit.Framework;
using Resonance.Domain.Anomaly;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class GroundFireRuleTests
    {
        [Test]
        public void Evaluate_TransitionsCleanlyAcrossThreshold()
        {
            var below = GroundFireRule.Evaluate(100f, 150f, 0.5f, false);
            Assert.IsFalse(below.ShouldActivate);
            Assert.IsFalse(below.ShouldDeactivate);
            Assert.IsFalse(below.IsActive);

            var activate = GroundFireRule.Evaluate(150f, 150f, 0.5f, false);
            Assert.IsTrue(activate.ShouldActivate);
            Assert.IsTrue(activate.ShouldUpdateIntensity);
            Assert.IsTrue(activate.IsActive);
            Assert.AreEqual(1.25f, activate.VerticalScale, 0.001f);

            var stayActive = GroundFireRule.Evaluate(200f, 150f, 0.5f, true);
            Assert.IsFalse(stayActive.ShouldActivate);
            Assert.IsTrue(stayActive.ShouldUpdateIntensity);
            Assert.AreEqual(1.5f, stayActive.VerticalScale, 0.001f);

            var deactivate = GroundFireRule.Evaluate(149f, 150f, 0.5f, true);
            Assert.IsTrue(deactivate.ShouldDeactivate);
            Assert.IsFalse(deactivate.IsActive);
        }
    }
}
