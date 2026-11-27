using System;

namespace Resonance.Domain.Actors
{
    public readonly struct FallEvaluationInput
    {
        public float CurrentY { get; }
        public float HigherPointOnJump { get; }
        public float PostGroundVerticalPos { get; }
        public int JumpDirection { get; }
        public bool FallDamageEnabled { get; }
        public bool HasTouchedGround { get; }
        public bool HasLanded { get; }
        public bool JumpImmune { get; }
        public float CurrentTime { get; }
        public float LastFallingTime { get; }
        public float SafeFallDistance { get; }
        public float DeathFallDistance { get; }

        public FallEvaluationInput(
            float currentY,
            float higherPointOnJump,
            float postGroundVerticalPos,
            int jumpDirection,
            bool fallDamageEnabled,
            bool hasTouchedGround,
            bool hasLanded,
            bool jumpImmune,
            float currentTime,
            float lastFallingTime,
            float safeFallDistance,
            float deathFallDistance)
        {
            CurrentY = currentY;
            HigherPointOnJump = higherPointOnJump;
            PostGroundVerticalPos = postGroundVerticalPos;
            JumpDirection = jumpDirection;
            FallDamageEnabled = fallDamageEnabled;
            HasTouchedGround = hasTouchedGround;
            HasLanded = hasLanded;
            JumpImmune = jumpImmune;
            CurrentTime = currentTime;
            LastFallingTime = lastFallingTime;
            SafeFallDistance = safeFallDistance;
            DeathFallDistance = deathFallDistance;
        }
    }

    public readonly struct FallEvaluationResult
    {
        public float FallDistance { get; }
        public int Damage { get; }
        public bool ShouldApplyDamage { get; }
        public float LandingSoundVolume { get; }
        public bool ShouldDispatchEarlyLandImpact { get; }
        public float EarlyLandImpactAmount { get; }
        public bool ShouldConsumeJumpImmunity { get; }
        public bool ShouldUpdateFallingTime { get; }

        public FallEvaluationResult(
            float fallDistance,
            int damage,
            bool shouldApplyDamage,
            float landingSoundVolume,
            bool shouldDispatchEarlyLandImpact,
            float earlyLandImpactAmount,
            bool shouldConsumeJumpImmunity,
            bool shouldUpdateFallingTime)
        {
            FallDistance = fallDistance;
            Damage = damage;
            ShouldApplyDamage = shouldApplyDamage;
            LandingSoundVolume = landingSoundVolume;
            ShouldDispatchEarlyLandImpact = shouldDispatchEarlyLandImpact;
            EarlyLandImpactAmount = earlyLandImpactAmount;
            ShouldConsumeJumpImmunity = shouldConsumeJumpImmunity;
            ShouldUpdateFallingTime = shouldUpdateFallingTime;
        }
    }

    /// <summary>
    /// Pure C# fall distance and damage evaluator.
    /// </summary>
    public static class FallDamageCalculator
    {
        private const float EarlyLandingThresholdSeconds = 0.4f;
        private const float EarlyImpactMinDistance = 0.05f;
        private const float EarlyImpactAmount = 0.2f;

        public static FallEvaluationResult Evaluate(in FallEvaluationInput input)
        {
            float fallDistance = input.JumpDirection == -1
                ? Math.Abs(input.PostGroundVerticalPos - input.CurrentY)
                : input.HigherPointOnJump - input.CurrentY;

            if (!input.FallDamageEnabled || !input.HasTouchedGround || !input.HasLanded)
            {
                return new FallEvaluationResult(fallDistance, 0, false, 1f, false, 0f, false, false);
            }

            if (input.JumpImmune)
            {
                return new FallEvaluationResult(fallDistance, 0, false, 0f, false, 0f, true, false);
            }

            if ((input.CurrentTime - input.LastFallingTime) <= EarlyLandingThresholdSeconds)
            {
                bool earlyImpact = fallDistance > EarlyImpactMinDistance;
                return new FallEvaluationResult(
                    fallDistance,
                    0,
                    false,
                    0f,
                    earlyImpact,
                    earlyImpact ? EarlyImpactAmount : 0f,
                    false,
                    false);
            }

            float ratio = input.DeathFallDistance > 0f ? fallDistance / input.DeathFallDistance : 0f;
            bool applyDamage = fallDistance > input.SafeFallDistance;
            int damage = applyDamage ? (int)Math.Floor(ratio * 100f) : 0;
            return new FallEvaluationResult(fallDistance, damage, applyDamage, ratio, false, 0f, false, true);
        }
    }
}
