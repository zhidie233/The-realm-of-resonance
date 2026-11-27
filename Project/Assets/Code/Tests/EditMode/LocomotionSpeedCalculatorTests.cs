using NUnit.Framework;
using Resonance.Domain.Actors;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class LocomotionSpeedCalculatorTests
    {
        [Test]
        public void ComputeEffectiveSpeed_AppliesWeightAndFloor()
        {
            Assert.AreEqual(6f, LocomotionSpeedCalculator.ComputeEffectiveSpeed(8f, 2f, false), 0.001f);
            Assert.AreEqual(1.75f, LocomotionSpeedCalculator.ComputeEffectiveSpeed(2f, 2f, false), 0.001f);
            Assert.AreEqual(1f, LocomotionSpeedCalculator.ComputeEffectiveSpeed(1f, 2f, true), 0.001f);
        }

        [Test]
        public void ComputeSprintFovAndJumpOverride_WorkNonDestructively()
        {
            Assert.AreEqual(8f, LocomotionSpeedCalculator.ComputeSprintFovTarget(true, true, 8f), 0.001f);
            Assert.AreEqual(0f, LocomotionSpeedCalculator.ComputeSprintFovTarget(true, false, 8f), 0.001f);

            Assert.AreEqual(7.5f, LocomotionSpeedCalculator.ResolveEffectiveJumpSpeed(7.5f, null), 0.001f);
            Assert.AreEqual(12f, LocomotionSpeedCalculator.ResolveEffectiveJumpSpeed(7.5f, 12f), 0.001f);
        }
    }
}
