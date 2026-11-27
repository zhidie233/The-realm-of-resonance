namespace Resonance.Domain.Anomaly
{
    public readonly struct GroundFireDecision
    {
        public bool ShouldActivate { get; }
        public bool ShouldDeactivate { get; }
        public bool ShouldUpdateIntensity { get; }
        public bool IsActive { get; }
        public float VerticalScale { get; }

        public GroundFireDecision(
            bool shouldActivate,
            bool shouldDeactivate,
            bool shouldUpdateIntensity,
            bool isActive,
            float verticalScale)
        {
            ShouldActivate = shouldActivate;
            ShouldDeactivate = shouldDeactivate;
            ShouldUpdateIntensity = shouldUpdateIntensity;
            IsActive = isActive;
            VerticalScale = verticalScale;
        }
    }

    /// <summary>
    /// Pure C# evaluation rule for ground fire activation and vertical scale.
    /// </summary>
    public static class GroundFireRule
    {
        public static GroundFireDecision Evaluate(
            float currentResonance,
            float activationThreshold,
            float scaleMultiplier,
            bool currentlyActive)
        {
            if (currentResonance >= activationThreshold)
            {
                bool shouldActivate = !currentlyActive;
                float scale = (1f + currentResonance * 0.01f) * scaleMultiplier;
                return new GroundFireDecision(shouldActivate, false, true, true, scale);
            }

            if (currentlyActive)
            {
                return new GroundFireDecision(false, true, false, false, 0f);
            }

            return new GroundFireDecision(false, false, false, false, 0f);
        }
    }
}
