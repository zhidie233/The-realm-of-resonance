using NUnit.Framework;
using Resonance.Domain.Anomaly;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class ResonanceContributionRouterTests
    {
        [Test]
        public void RouteCombatDamage_HandlesFfaTeamEnemyFriendlyFireAndZeroDamage()
        {
            var zero = ResonanceContributionRouter.RouteCombatDamage(1, 2, 0f, false);
            Assert.IsFalse(zero.IsValid);

            var ffaSameTeam = ResonanceContributionRouter.RouteCombatDamage(1, 1, 25f, true);
            Assert.IsTrue(ffaSameTeam.IsValid);
            Assert.AreEqual(ResonanceContributionRouter.EnemyHitKey, ffaSameTeam.Key);
            Assert.AreEqual(25f, ffaSameTeam.Multiplier, 0.001f);
            Assert.IsFalse(ffaSameTeam.AuthorityOnly);

            var friendly = ResonanceContributionRouter.RouteCombatDamage(1, 1, 30f, false);
            Assert.AreEqual(ResonanceContributionRouter.TeammateHitKey, friendly.Key);
            Assert.AreEqual(30f, friendly.Multiplier, 0.001f);

            var enemy = ResonanceContributionRouter.RouteCombatDamage(1, 2, 40f, false);
            Assert.AreEqual(ResonanceContributionRouter.EnemyHitKey, enemy.Key);
            Assert.AreEqual(40f, enemy.Multiplier, 0.001f);
        }

        [Test]
        public void RouteObjective_MapsAllFlagEventsAsAuthorityOnly()
        {
            var pickUp = ResonanceContributionRouter.RouteObjective(ResonanceObjectiveKind.FlagPickedUp);
            Assert.IsTrue(pickUp.IsValid && pickUp.AuthorityOnly);
            Assert.AreEqual(ResonanceContributionRouter.FlagPickUpKey, pickUp.Key);

            var capture = ResonanceContributionRouter.RouteObjective(ResonanceObjectiveKind.FlagCaptured);
            Assert.IsTrue(capture.IsValid && capture.AuthorityOnly);
            Assert.AreEqual(ResonanceContributionRouter.FlagCaptureKey, capture.Key);

            var recover = ResonanceContributionRouter.RouteObjective(ResonanceObjectiveKind.FlagRecovered);
            Assert.IsTrue(recover.IsValid && recover.AuthorityOnly);
            Assert.AreEqual(ResonanceContributionRouter.FlagRecoverKey, recover.Key);
        }

        [Test]
        public void RouteDirectImpulse_RejectsEmptyKey()
        {
            Assert.IsFalse(ResonanceContributionRouter.RouteDirectImpulse("", 10f).IsValid);
            var valid = ResonanceContributionRouter.RouteDirectImpulse("FacilityAmplifier", 50f);
            Assert.IsTrue(valid.IsValid);
            Assert.AreEqual("FacilityAmplifier", valid.Key);
            Assert.AreEqual(50f, valid.Multiplier, 0.001f);
        }
    }
}
