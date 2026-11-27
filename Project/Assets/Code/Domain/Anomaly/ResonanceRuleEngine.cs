using System.Collections.Generic;

namespace Resonance.Domain.Anomaly
{
    /// <summary>
    /// Pure C# resonance value state and rule magnification engine.
    /// </summary>
    public sealed class ResonanceRuleEngine
    {
        private readonly Dictionary<string, float> _rules = new Dictionary<string, float>();

        public float CurrentValue { get; private set; }
        public bool IsEnabled { get; set; } = true;

        public void LoadRules(IEnumerable<KeyValuePair<string, float>> rules)
        {
            _rules.Clear();
            if (rules == null) return;

            foreach (var entry in rules)
            {
                if (string.IsNullOrEmpty(entry.Key)) continue;
                if (!_rules.ContainsKey(entry.Key))
                {
                    _rules.Add(entry.Key, entry.Value);
                }
            }
        }

        public float GetMagnification(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0f;
            return _rules.TryGetValue(key, out float value) ? value : 0f;
        }

        public bool TryApplyChange(string key, float multiplier, out float newValue)
        {
            if (!IsEnabled)
            {
                newValue = CurrentValue;
                return false;
            }

            CurrentValue += GetMagnification(key) * multiplier;
            newValue = CurrentValue;
            return true;
        }

        public void SetValue(float value)
        {
            CurrentValue = value;
        }

        public void Reset(float baseline = 0f)
        {
            CurrentValue = baseline;
        }
    }
}
