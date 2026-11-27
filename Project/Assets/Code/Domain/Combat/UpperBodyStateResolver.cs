namespace Resonance.Domain.Combat
{
    public enum UpperBodyActionState : byte
    {
        Idle = 0,
        Firing = 1,
        Reloading = 2,
        Aiming = 3,
        Running = 4,
        FireAiming = 9
    }

    /// <summary>
    /// Pure C# resolver for first-person upper-body animation state.
    /// </summary>
    public static class UpperBodyStateResolver
    {
        public static UpperBodyActionState Resolve(bool isFiring, bool isAiming, bool isReloading, bool isRunning)
        {
            if (isReloading) return UpperBodyActionState.Reloading;
            if (isFiring) return isAiming ? UpperBodyActionState.FireAiming : UpperBodyActionState.Firing;
            if (isAiming) return UpperBodyActionState.Aiming;
            if (isRunning) return UpperBodyActionState.Running;
            return UpperBodyActionState.Idle;
        }
    }
}
