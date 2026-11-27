using System;
using UnityEngine;
using Resonance.Domain.Actors;

namespace Resonance.Actors
{
    /// <summary>
    /// Tracks vertical apex, fall duration, and computes fall damage on landing.
    /// </summary>
    public sealed class AirborneMotionTracker
    {
        public bool IsFalling { get; set; }
        public int JumpDirection { get; set; }
        public float PostGroundVerticalPos { get; set; }
        public float HigherPointOnJump { get; set; }
        public float FallingTime { get; set; }
        public bool HasTouchedGround { get; set; }
        public bool HasLanded { get; set; }
        public bool JumpImmune { get; set; }

        public void ResetOnLand(float currentY)
        {
            IsFalling = false;
            HasLanded = true;
            JumpDirection = 0;
            FallingTime = 0f;
            HigherPointOnJump = currentY;
        }

        public void UpdateVerticalDirection(float currentY)
        {
            if (Mathf.Approximately(currentY, PostGroundVerticalPos)) return;
            if (JumpDirection == 0)
            {
                JumpDirection = currentY > PostGroundVerticalPos ? 1 : -1;
            }
            else if (JumpDirection == 1)
            {
                if (currentY < PostGroundVerticalPos) HigherPointOnJump = PostGroundVerticalPos;
                else PostGroundVerticalPos = currentY;
            }
        }

        public float EvaluateFall(
            float currentY,
            bool fallDamageEnabled,
            float safeFallDistance,
            float deathFallDistance,
            PlayerHealthManagerBase healthManager,
            Action<float> playLandingSound,
            Action<float> dispatchLandImpact)
        {
            var input = new FallEvaluationInput(
                currentY,
                HigherPointOnJump,
                PostGroundVerticalPos,
                JumpDirection,
                fallDamageEnabled,
                HasTouchedGround,
                HasLanded,
                JumpImmune,
                Time.time,
                FallingTime,
                safeFallDistance,
                deathFallDistance);

            var result = FallDamageCalculator.Evaluate(in input);

            if (result.ShouldConsumeJumpImmunity)
            {
                JumpImmune = false;
                return result.FallDistance;
            }

            if (result.ShouldDispatchEarlyLandImpact)
            {
                dispatchLandImpact?.Invoke(result.EarlyLandImpactAmount);
                return result.FallDistance;
            }

            if (result.ShouldApplyDamage && healthManager != null)
            {
                healthManager.DoFallDamage(result.Damage);
            }

            if (result.LandingSoundVolume > 0f || result.ShouldUpdateFallingTime || !fallDamageEnabled || !HasTouchedGround || !HasLanded)
            {
                playLandingSound?.Invoke(result.LandingSoundVolume);
            }

            if (result.ShouldUpdateFallingTime)
            {
                FallingTime = Time.time;
            }

            return result.FallDistance;
        }
    }
}
