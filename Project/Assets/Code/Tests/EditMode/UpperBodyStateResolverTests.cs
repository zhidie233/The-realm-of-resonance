using NUnit.Framework;
using Resonance.Domain.Combat;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class UpperBodyStateResolverTests
    {
        [Test]
        public void Resolve_RespectsStatePrecedence()
        {
            Assert.AreEqual(UpperBodyActionState.Reloading, UpperBodyStateResolver.Resolve(true, true, true, true));
            Assert.AreEqual(UpperBodyActionState.FireAiming, UpperBodyStateResolver.Resolve(true, true, false, true));
            Assert.AreEqual(UpperBodyActionState.Firing, UpperBodyStateResolver.Resolve(true, false, false, true));
            Assert.AreEqual(UpperBodyActionState.Aiming, UpperBodyStateResolver.Resolve(false, true, false, true));
            Assert.AreEqual(UpperBodyActionState.Running, UpperBodyStateResolver.Resolve(false, false, false, true));
            Assert.AreEqual(UpperBodyActionState.Idle, UpperBodyStateResolver.Resolve(false, false, false, false));
        }
    }
}
