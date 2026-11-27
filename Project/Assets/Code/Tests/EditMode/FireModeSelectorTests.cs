using NUnit.Framework;
using Resonance.Domain.Combat;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class FireModeSelectorTests
    {
        [Test]
        public void ResolveInitialMode_FallsBackWhenPreferredDisabled()
        {
            Assert.AreEqual(FireModeOption.Burst, FireModeSelector.ResolveInitialMode(FireModeOption.Auto, false, true, true));
            Assert.AreEqual(FireModeOption.Single, FireModeSelector.ResolveInitialMode(FireModeOption.Auto, false, false, true));
            Assert.AreEqual(FireModeOption.Auto, FireModeSelector.ResolveInitialMode(FireModeOption.Burst, false, false, false).Equals(FireModeOption.Burst) ? FireModeOption.Auto : FireModeOption.Burst);
            Assert.AreEqual(FireModeOption.Burst, FireModeSelector.ResolveInitialMode(FireModeOption.Single, true, true, false));
        }

        [Test]
        public void CycleNextMode_SkipsDisabledModesAndKeepsCurrentIfAlone()
        {
            Assert.AreEqual(FireModeOption.Burst, FireModeSelector.CycleNextMode(FireModeOption.Auto, true, true, true));
            Assert.AreEqual(FireModeOption.Single, FireModeSelector.CycleNextMode(FireModeOption.Burst, true, true, true));
            Assert.AreEqual(FireModeOption.Auto, FireModeSelector.CycleNextMode(FireModeOption.Single, true, true, true));

            Assert.AreEqual(FireModeOption.Single, FireModeSelector.CycleNextMode(FireModeOption.Auto, true, false, true));
            Assert.AreEqual(FireModeOption.Auto, FireModeSelector.CycleNextMode(FireModeOption.Auto, true, false, false));
        }
    }
}
