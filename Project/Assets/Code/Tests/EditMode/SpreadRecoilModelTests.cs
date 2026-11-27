using NUnit.Framework;
using Resonance.Domain.Combat;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class SpreadRecoilModelTests
    {
        private SpreadRecoilModel _spread;

        [SetUp]
        public void SetUp()
        {
            _spread = new SpreadRecoilModel();
            _spread.SetBaseRange(1f, 5f);
        }

        [Test]
        public void Step_FiringGrowsAndCrouchHalvesRate()
        {
            float standing = _spread.Step(true, false, 1f, 0.5f, 0f);
            Assert.AreEqual(1f, standing, 0.001f);

            standing = _spread.Step(true, false, 1f, 0.5f, 0f);
            Assert.AreEqual(2f, standing, 0.001f);

            float crouched = _spread.Step(true, true, 1f, 0.5f, 0f);
            Assert.AreEqual(2.5f, crouched, 0.001f);
        }

        [Test]
        public void Step_IdleDecaysAndClampsToRange()
        {
            _spread.CurrentSpread = 10f;
            float clampedMax = _spread.Step(true, false, 1f, 0.5f, 0f);
            Assert.AreEqual(5f, clampedMax, 0.001f);

            _spread.CurrentSpread = 0.2f;
            float clampedMin = _spread.Step(false, false, 1f, 0.5f, 0f);
            Assert.AreEqual(1f, clampedMin, 0.001f);
        }

        [Test]
        public void ApplyAimState_AndComputeInaccuracy_HandleSniperHipfirePenalty()
        {
            _spread.CurrentSpread = 2f;
            _spread.ApplyAimState(false, 0.5f);
            _spread.ComputeInaccuracy(true, false, out float hipSpread, out float hipMax);
            Assert.AreEqual(6f, hipSpread, 0.001f);
            Assert.AreEqual(15f, hipMax, 0.001f);

            _spread.ApplyAimState(true, 0.5f);
            _spread.ComputeInaccuracy(true, true, out float adsSpread, out float adsMax);
            Assert.AreEqual(2f, adsSpread, 0.001f);
            Assert.AreEqual(2.5f, adsMax, 0.001f);
        }
    }
}
