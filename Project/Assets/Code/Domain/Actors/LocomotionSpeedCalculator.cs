using System;

namespace Resonance.Domain.Actors
{
    /// <summary>
    /// Pure C# locomotion speed, sprint FOV, and jump modifier calculations.
    /// </summary>
    public static class LocomotionSpeedCalculator
    {
        public const float DefaultMinSpeed = 1.75f;
        public const float StealthMinSpeed = 1f;

        public static float ComputeEffectiveSpeed(float baseSpeed, float weaponWeight, bool isStealth)
        {
            float min = isStealth ? StealthMinSpeed : DefaultMinSpeed;
            return Math.Max(baseSpeed - weaponWeight, min);
        }

        public static float ComputeSprintFovTarget(bool isRunning, bool isGrounded, float runFovAmount)
        {
            return isRunning && isGrounded ? runFovAmount : 0f;
        }

        public static float ResolveEffectiveJumpSpeed(float baseJumpSpeed, float? overrideJumpSpeed)
        {
            return overrideJumpSpeed ?? baseJumpSpeed;
        }
    }
}
