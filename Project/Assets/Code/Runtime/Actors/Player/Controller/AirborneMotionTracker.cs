using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
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
                JumpDirection = (currentY > PostGroundVerticalPos) ? 1 : -1;
            }
            else if (JumpDirection == 1)
            {
                if (currentY < PostGroundVerticalPos)
                    HigherPointOnJump = PostGroundVerticalPos;
                else
                    PostGroundVerticalPos = currentY;
            }
        }

        public float EvaluateFall(
            float currentY,
            bool fallDamageEnabled,
            float safeFallDistance,
            float deathFallDistance,
            PlayerHealthManagerBase healthManager,
            System.Action<float> playLandingSound,
            System.Action<float> dispatchLandImpact)
        {
            float fallDistance = HigherPointOnJump - currentY;
            if (JumpDirection == -1)
            {
                fallDistance = Mathf.Abs(PostGroundVerticalPos - currentY);
            }

            if (fallDamageEnabled && HasTouchedGround && HasLanded)
            {
                if (JumpImmune)
                {
                    JumpImmune = false;
                    return fallDistance;
                }
                if ((Time.time - FallingTime) <= 0.4f)
                {
                    if (fallDistance > 0.05f) dispatchLandImpact?.Invoke(0.2f);
                    return fallDistance;
                }
                float ratio = fallDistance / deathFallDistance;
                if (fallDistance > safeFallDistance && healthManager != null)
                {
                    int damage = Mathf.FloorToInt(ratio * 100f);
                    healthManager.DoFallDamage(damage);
                }
                playLandingSound?.Invoke(ratio);
                FallingTime = Time.time;
            }
            else
            {
                playLandingSound?.Invoke(1f);
            }
            return fallDistance;
        }
    }
}
