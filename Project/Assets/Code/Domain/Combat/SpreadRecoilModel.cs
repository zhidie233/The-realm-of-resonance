namespace Resonance.Domain.Combat
{
    /// <summary>
    /// Pure C# weapon spread and inaccuracy calculator.
    /// </summary>
    public sealed class SpreadRecoilModel
    {
        public float BaseMin { get; private set; }
        public float BaseMax { get; private set; }
        public float CurrentMin { get; private set; }
        public float CurrentMax { get; private set; }
        public float CurrentSpread { get; set; }

        public void SetBaseRange(float min, float max)
        {
            BaseMin = min;
            BaseMax = max;
            CurrentMin = min;
            CurrentMax = max;
        }

        public void ApplyAimState(bool isAiming, float aimMultiplier)
        {
            if (isAiming)
            {
                CurrentMin = BaseMin * aimMultiplier;
                CurrentMax = BaseMax * aimMultiplier;
            }
            else
            {
                CurrentMin = BaseMin;
                CurrentMax = BaseMax;
            }
        }

        public float Step(bool isFiring, bool isCrouching, float spreadPerSecond, float decreasePerSecond, float extraSpread)
        {
            if (isFiring)
            {
                CurrentSpread += isCrouching ? spreadPerSecond * 0.5f : spreadPerSecond;
            }
            else
            {
                CurrentSpread -= decreasePerSecond;
            }

            float effectiveBase = isCrouching ? CurrentMin * 0.5f : CurrentMin;
            float min = effectiveBase + extraSpread;
            float max = CurrentMax + extraSpread;
            if (CurrentSpread < min) CurrentSpread = min;
            if (CurrentSpread > max) CurrentSpread = max;
            return CurrentSpread;
        }

        public void ComputeInaccuracy(bool isSniper, bool isAiming, out float spread, out float maxSpread)
        {
            if (isSniper && !isAiming)
            {
                spread = CurrentSpread * 3f;
                maxSpread = CurrentMax * 3f;
            }
            else
            {
                spread = CurrentSpread;
                maxSpread = CurrentMax;
            }
        }
    }
}
