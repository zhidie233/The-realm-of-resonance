using NUnit.Framework;
using Resonance.Domain.Actors;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class FallDamageCalculatorTests
    {
        [Test]
        public void Evaluate_ComputesDamageAboveSafeDistanceAndGuardsZeroDeathDistance()
        {
            var input = new FallEvaluationInput(
                currentY: 0f,
                higherPointOnJump: 10f,
                postGroundVerticalPos: 10f,
                jumpDirection: 1,
                fallDamageEnabled: true,
                hasTouchedGround: true,
                hasLanded: true,
                jumpImmune: false,
                currentTime: 5f,
                lastFallingTime: 1f,
                safeFallDistance: 3f,
                deathFallDistance: 20f);

            var res = FallDamageCalculator.Evaluate(in input);
            Assert.AreEqual(10f, res.FallDistance, 0.001f);
            Assert.IsTrue(res.ShouldApplyDamage);
            Assert.AreEqual(50, res.Damage);
            Assert.IsTrue(res.ShouldUpdateFallingTime);

            var zeroDeath = new FallEvaluationInput(0f, 10f, 10f, -1, true, true, true, false, 5f, 1f, 3f, 0f);
            var zeroRes = FallDamageCalculator.Evaluate(in zeroDeath);
            Assert.AreEqual(0, zeroRes.Damage);
        }

        [Test]
        public void Evaluate_ConsumesJumpImmunityAndHandlesEarlyLandingCooldown()
        {
            var immune = new FallEvaluationInput(0f, 10f, 10f, 1, true, true, true, true, 5f, 1f, 3f, 15f);
            var immuneRes = FallDamageCalculator.Evaluate(in immune);
            Assert.IsTrue(immuneRes.ShouldConsumeJumpImmunity);
            Assert.IsFalse(immuneRes.ShouldApplyDamage);

            var early = new FallEvaluationInput(0f, 2f, 2f, 1, true, true, true, false, 1.2f, 1.0f, 3f, 15f);
            var earlyRes = FallDamageCalculator.Evaluate(in early);
            Assert.IsTrue(earlyRes.ShouldDispatchEarlyLandImpact);
            Assert.AreEqual(0.2f, earlyRes.EarlyLandImpactAmount, 0.001f);
            Assert.IsFalse(earlyRes.ShouldApplyDamage);
        }
    }
}
