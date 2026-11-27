using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Actors;

namespace Resonance.Combat
{
    /// <summary>
    /// Manages weapon ADS transform interpolation, camera FOV blending, and sway state.
    /// </summary>
    public sealed class WeaponAimPresenter
    {
        public Vector3 DefaultPosition { get; private set; }
        public Quaternion DefaultRotation { get; private set; }
        public bool LastAimState { get; private set; }

        public void CaptureDefaults(Transform weaponTransform)
        {
            DefaultPosition = weaponTransform.localPosition;
            DefaultRotation = weaponTransform.localRotation;
        }

        public void ResetAimState()
        {
            LastAimState = false;
        }

        public void SnapToAim(Transform weaponTransform, Vector3 aimPosition, Vector3 aimRotation)
        {
            weaponTransform.localPosition = aimPosition;
            weaponTransform.localRotation = Quaternion.Euler(aimRotation);
        }

        public bool UpdateAim(
            Transform weaponTransform,
            Camera playerCamera,
            PlayerRig playerRefs,
            PlayerLocomotionController controller,
            bool isAiming,
            bool isReloading,
            bool blockAimFov,
            bool useSmooth,
            float aimSpeed,
            Vector3 aimPosition,
            Vector3 aimRotation,
            float zoomFov,
            float deltaTime)
        {
            bool activeAim = isAiming && !isReloading;
            Vector3 targetPos;
            Quaternion targetRot;
            float targetFov;

            if (activeAim)
            {
                targetPos = aimPosition;
                targetRot = Quaternion.Euler(aimRotation);
                targetFov = zoomFov;
                playerRefs.weaponSway.UseAimSettings();
            }
            else
            {
                targetPos = DefaultPosition;
                targetRot = DefaultRotation;
                targetFov = playerRefs.DefaultCameraFOV;
                playerRefs.weaponSway.ResetSettings();
            }

            weaponTransform.localPosition = useSmooth
                ? Vector3.Lerp(weaponTransform.localPosition, targetPos, deltaTime * aimSpeed)
                : Vector3.MoveTowards(weaponTransform.localPosition, targetPos, deltaTime * aimSpeed);
            weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, targetRot, deltaTime * aimSpeed);

            if (playerCamera != null && !blockAimFov)
            {
                playerCamera.fieldOfView = Mathf.Lerp(
                    playerCamera.fieldOfView,
                    targetFov + controller.GetSprintFov(),
                    deltaTime * aimSpeed);
            }

            if (LastAimState != isAiming)
            {
                EventBus.Publish(new LocalAimChangedEvent(isAiming));
                LastAimState = isAiming;
            }

            return activeAim;
        }
    }
}
