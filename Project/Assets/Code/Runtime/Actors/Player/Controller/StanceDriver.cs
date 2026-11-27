using System.Collections;
using UnityEngine;
namespace Resonance.Actors
{

    /// <summary>
    /// Manages crouch capsule/camera interpolation, overhead clearance, and slide state.
    /// </summary>
    public sealed class StanceDriver
    {
        public bool IsCrouching { get; set; }
        public bool LastCrouchState { get; set; }
        public bool ForcedCrouch { get; set; }
        public float SlideForce { get; set; }
        public float LastSlideTime { get; set; }

        private Coroutine _crouchRoutine;

        public static bool IsHeadObstructed(Transform playerTransform, CharacterController controller)
        {
            Vector3 origin = playerTransform.localPosition + controller.center + Vector3.up * controller.height * 0.5f;
            float dist = 2.05f - controller.height;
            return Physics.Raycast(origin, Vector3.up, dist);
        }

        public void BeginCrouchTransition(MonoBehaviour host, CharacterController controller, Transform cameraRoot, Vector3 defaultCameraPos, float defaultHeight, float crouchHeight, float transitionSpeed, bool isSliding)
        {
            if (_crouchRoutine != null) host.StopCoroutine(_crouchRoutine);
            _crouchRoutine = host.StartCoroutine(InterpolateCrouch(controller, cameraRoot, defaultCameraPos, defaultHeight, crouchHeight, transitionSpeed, isSliding));
        }

        private IEnumerator InterpolateCrouch(CharacterController controller, Transform cameraRoot, Vector3 defaultCameraPos, float defaultHeight, float crouchHeight, float transitionSpeed, bool isSliding)
        {
            bool crouched = IsCrouching || isSliding;
            float targetHeight = crouched ? crouchHeight : defaultHeight;
            Vector3 targetCenter = new Vector3(0f, targetHeight * 0.5f, 0f);
            Vector3 camPos = cameraRoot.localPosition;
            Vector3 targetCamPos = crouched
                ? new Vector3(camPos.x, defaultCameraPos.y - (defaultHeight - targetHeight), camPos.z)
                : defaultCameraPos;

            float originHeight = controller.height;
            Vector3 originCenter = controller.center;
            Vector3 originCamPos = camPos;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / transitionSpeed;
                controller.height = Mathf.Lerp(originHeight, targetHeight, t);
                controller.center = Vector3.Lerp(originCenter, targetCenter, t);
                cameraRoot.localPosition = Vector3.Lerp(originCamPos, targetCamPos, t);
                yield return null;
            }
            _crouchRoutine = null;
        }
    }
}
