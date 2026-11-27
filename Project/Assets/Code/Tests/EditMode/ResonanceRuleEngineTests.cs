using System.Collections.Generic;
using NUnit.Framework;
using Resonance.Domain.Anomaly;

namespace Resonance.Tests.EditMode
{
    [TestFixture]
    public sealed class ResonanceRuleEngineTests
    {
        private ResonanceRuleEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new ResonanceRuleEngine();
            _engine.LoadRules(new[]
            {
                new KeyValuePair<string, float>("Enemy", 0.5f),
                new KeyValuePair<string, float>("Teammate", -1.0f),
                new KeyValuePair<string, float>("Enemy", 99f),
                new KeyValuePair<string, float>(null, 10f)
            });
        }

        [Test]
        public void LoadRules_IgnoresDuplicatesAndNullKeys()
        {
            Assert.AreEqual(0.5f, _engine.GetMagnification("Enemy"), 0.001f);
            Assert.AreEqual(-1.0f, _engine.GetMagnification("Teammate"), 0.001f);
            Assert.AreEqual(0f, _engine.GetMagnification("Unknown"), 0.001f);
        }

        [Test]
        public void TryApplyChange_AccumulatesPositiveAndNegativeValues()
        {
            Assert.IsTrue(_engine.TryApplyChange("Enemy", 40f, out float v1));
            Assert.AreEqual(20f, v1, 0.001f);

            Assert.IsTrue(_engine.TryApplyChange("Teammate", 5f, out float v2));
            Assert.AreEqual(15f, v2, 0.001f);

            Assert.IsTrue(_engine.TryApplyChange("MissingKey", 100f, out float v3));
            Assert.AreEqual(15f, v3, 0.001f);
        }

        [Test]
        public void TryApplyChange_DoesNothingWhenDisabled()
        {
            _engine.SetValue(12f);
            _engine.IsEnabled = false;
            Assert.IsFalse(_engine.TryApplyChange("Enemy", 20f, out float v));
            Assert.AreEqual(12f, v, 0.001f);
        }
    }
}
