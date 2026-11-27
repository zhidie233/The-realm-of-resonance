using NUnit.Framework;
using Resonance.Domain.Combat;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class AmmoInventoryModelTests
    {
        private AmmoInventoryModel _model;

        [SetUp]
        public void SetUp() => _model = new AmmoInventoryModel();

        [Test]
        public void Configure_BulletsMode_SetsReserveOnlyWhenNotInitial()
        {
            _model.Configure(AmmoStorageMode.Bullets, 30, 4, true);
            Assert.AreEqual(30, _model.CurrentMagazine);
            Assert.AreEqual(0, _model.ReserveCount);

            _model.Configure(AmmoStorageMode.Bullets, 30, 4, false);
            Assert.AreEqual(30, _model.CurrentMagazine);
            Assert.AreEqual(120, _model.ReserveCount);
        }

        [Test]
        public void FillMagazine_BulletsMode_TransfersOnlyNeededAndRespectsInfinite()
        {
            _model.Configure(AmmoStorageMode.Bullets, 30, 2, false);
            _model.CurrentMagazine = 20;
            _model.FillMagazine(30);
            Assert.AreEqual(30, _model.CurrentMagazine);
            Assert.AreEqual(50, _model.ReserveCount);

            _model.SetInfiniteAmmo(true, 30);
            _model.CurrentMagazine = 10;
            int beforeReserve = _model.ReserveCount;
            _model.FillMagazine(30);
            Assert.AreEqual(30, _model.CurrentMagazine);
            Assert.AreEqual(beforeReserve, _model.ReserveCount);
        }

        [Test]
        public void TryPickUpAmmo_FiltersWeaponIdAndKnifeAndClampsAtCap()
        {
            _model.Configure(AmmoStorageMode.Bullets, 10, 1, false);

            var wrongWeapon = _model.TryPickUpAmmo(10, 0, 2, 1, false, false, 10, 3);
            Assert.IsFalse(wrongWeapon.Consumed);

            var knife = _model.TryPickUpAmmo(10, 0, -1, 1, true, false, 10, 3);
            Assert.IsFalse(knife.Consumed);

            var clamped = _model.TryPickUpAmmo(25, 0, -1, 1, false, false, 10, 3);
            Assert.IsTrue(clamped.Consumed);
            Assert.AreEqual(20, clamped.AddedAmount);
            Assert.AreEqual(30, _model.ReserveCount);
            Assert.IsTrue(clamped.ShouldNotify);
        }

        [Test]
        public void TryPickUpAmmo_Grenade_AddsProjectiles()
        {
            _model.Configure(AmmoStorageMode.Bullets, 1, 2, false);
            var res = _model.TryPickUpAmmo(0, 3, -1, 4, false, true, 1, 5);
            Assert.IsTrue(res.Consumed);
            Assert.IsTrue(res.IsGrenade);
            Assert.AreEqual(3, res.AddedAmount);
            Assert.AreEqual(5, _model.ReserveCount);
        }

        [Test]
        public void TryPickUpAmmo_ClipsMode_GuardsZeroCapacityAndClamps()
        {
            _model.Configure(AmmoStorageMode.Clips, 30, 2, false);
            Assert.IsFalse(_model.TryPickUpAmmo(30, 0, -1, 1, false, false, 0, 5).Consumed);

            var res = _model.TryPickUpAmmo(150, 0, -1, 1, false, false, 30, 4);
            Assert.IsTrue(res.Consumed);
            Assert.IsFalse(res.ShouldNotify);
            Assert.AreEqual(4, _model.ReserveCount);
        }

        [Test]
        public void QueryHelpers_WorkAcrossBoundaryStates()
        {
            _model.Configure(AmmoStorageMode.Bullets, 30, 2, false);
            _model.CurrentMagazine = 10;
            Assert.IsTrue(_model.CanReload(30, false, false, false, true));
            Assert.IsFalse(_model.CanReload(30, true, false, false, true));
            Assert.IsFalse(_model.CanReload(30, false, false, true, true));
            Assert.AreEqual(20, _model.CalculateSplitReloadRounds(30));
            Assert.AreEqual(2, _model.GetCompactClips(30));
            Assert.AreEqual(0, _model.GetCompactClips(0));

            _model.CurrentMagazine = 0;
            _model.ReserveCount = 0;
            Assert.IsTrue(_model.IsOutOfAmmo());
            Assert.IsFalse(_model.CanBeTaken(false));
            Assert.IsTrue(_model.CanBeTaken(true));
        }
    }
}
