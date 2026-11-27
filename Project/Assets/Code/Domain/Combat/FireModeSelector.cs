namespace Resonance.Domain.Combat
{
    public enum FireModeOption
    {
        Auto = 0,
        Burst = 1,
        Single = 2
    }

    /// <summary>
    /// Pure C# fire-mode fallback and cycling rules.
    /// </summary>
    public static class FireModeSelector
    {
        public static FireModeOption ResolveInitialMode(FireModeOption preferred, bool canAuto, bool canBurst, bool canSingle)
        {
            switch (preferred)
            {
                case FireModeOption.Auto:
                    if (canAuto) return FireModeOption.Auto;
                    if (canBurst) return FireModeOption.Burst;
                    if (canSingle) return FireModeOption.Single;
                    return preferred;
                case FireModeOption.Single:
                    if (canSingle) return FireModeOption.Single;
                    if (canBurst) return FireModeOption.Burst;
                    if (canAuto) return FireModeOption.Auto;
                    return preferred;
                case FireModeOption.Burst:
                    if (canBurst) return FireModeOption.Burst;
                    if (canAuto) return FireModeOption.Auto;
                    if (canSingle) return FireModeOption.Single;
                    return preferred;
                default:
                    return preferred;
            }
        }

        public static FireModeOption CycleNextMode(FireModeOption current, bool canAuto, bool canBurst, bool canSingle)
        {
            switch (current)
            {
                case FireModeOption.Auto:
                    if (canBurst) return FireModeOption.Burst;
                    if (canSingle) return FireModeOption.Single;
                    return current;
                case FireModeOption.Burst:
                    if (canSingle) return FireModeOption.Single;
                    if (canAuto) return FireModeOption.Auto;
                    return current;
                case FireModeOption.Single:
                    if (canAuto) return FireModeOption.Auto;
                    if (canBurst) return FireModeOption.Burst;
                    return current;
                default:
                    return current;
            }
        }
    }
}
