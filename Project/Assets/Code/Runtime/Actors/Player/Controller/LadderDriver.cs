using System;
using UnityEngine;
using Resonance.World;
namespace Resonance.Actors
{

    /// <summary>
    /// Handles ladder attachment, vertical climbing, and detachment motion.
    /// </summary>
    public sealed class LadderDriver
    {
        public bool IsClimbing { get; private set; }
        public ClimbableLadder CurrentLadder { get; private set; }

        public void Attach(ClimbableLadder ladder)
        {
            CurrentLadder = ladder;
            IsClimbing = true;
        }

        public void Detach()
        {
            IsClimbing = false;
            CurrentLadder = null;
        }

        public bool TryStepClimb(
            Vector2 input,
            float climbSpeed,
            float jumpSpeed,
            bool jumpPressed,
            Transform playerTransform,
            CharacterController controller,
            ref Vector3 targetDirection,
            Action onStartExit,
            Action<ClimbableLadder> onJumpOff)
        {
            if (CurrentLadder == null) return false;

            if (CurrentLadder.Status == ClimbableLadder.LadderStatus.Detaching)
            {
                if (!CurrentLadder.Exiting)
                {
                    CurrentLadder.Exiting = true;
                    onStartExit?.Invoke();
                }
                return true;
            }

            Vector3 desired = CurrentLadder.transform.rotation * Vector3.forward * input.y;
            targetDirection = desired * climbSpeed;

            if (jumpPressed)
            {
                var exited = CurrentLadder;
                Detach();
                onJumpOff?.Invoke(exited);
            }

            controller.Move(targetDirection * Time.smoothDeltaTime);
            if (CurrentLadder != null) CurrentLadder.WatchLimits();
            return true;
        }
    }
}
