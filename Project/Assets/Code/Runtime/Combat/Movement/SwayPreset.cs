using UnityEngine;
using Resonance.Foundation;
using Resonance.Actors;
namespace Resonance.Combat
{
    /// 玩家冲刺时控制第一人称武器的旋转
    /// 此类没有直接引用，可以换成自己的脚本。
    public class SwayPreset : MonoBehaviour
    {

        [Header("Weapon On Run Position")]
        [Tooltip("武器常规位置与奔跑位置")]
        public Vector3 moveTo;
        [Tooltip("武器常规旋转与奔跑位置")]
        public Vector3 rotateTo;
        [Space(5)]
        [Header("Weapon On Run and Reload Position")]
        [Tooltip("武器常规位置、奔跑位置与换弹位置")]
        public Vector3 moveToReload;
        [Tooltip("武器常规旋转、奔跑位置与换弹位置")]
        public Vector3 rotateToReload;
        [Space(5)]
        public float accelerationMultiplier = 2.5f;
        public float InSpeed = 15;
        public float OutSpeed = 12;
        public float rotationSpeedMultiplier = 1;
        public AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [HideInInspector] public float _previewWeight = 0;

        private float vel;
        private Quaternion DefaultRot;
        private Quaternion sprintRot, sprintReloadRot;
        private Vector3 DefaultPos;
        private FirearmController Gun;
        private PlayerLocomotionController controller;
        private float acceleration = 1;
        private bool defaultState = true;

        protected virtual void Awake()
        {
            DefaultRot = transform.localRotation;
            DefaultPos = transform.localPosition;
            Gun = transform.parent.GetComponent<FirearmController>();
            controller = Gun.PlayerReferences.PlayerLocomotionController;
            sprintRot = Quaternion.Euler(rotateTo);
            sprintReloadRot = Quaternion.Euler(rotateToReload);
        }

        protected virtual void LateUpdate()
        {
            if (controller == null)
                return;

            vel = controller.VelocityMagnitude;
            RotateControl();
        }

        void RotateControl()
        {
            float delta = Time.smoothDeltaTime;
            acceleration = Mathf.Lerp(acceleration, 1, delta * accelerationMultiplier);
            float acc = accelerationCurve.Evaluate(acceleration);

            if ((vel > 1f && controller.isGrounded) && controller.State == PlayerState.Running && !Gun.isFiring && !Gun.isAiming)
            {
                if (defaultState)
                {
                    acceleration = 0f;
                    acc = 0;
                    defaultState = false;
                }

                if (Gun.isReloading)
                {
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, sprintReloadRot, (delta * (InSpeed * rotationSpeedMultiplier)) * acc);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, moveToReload, delta * InSpeed * acc);
                }
                else
                {
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, sprintRot, (delta * (InSpeed * rotationSpeedMultiplier)) * acc);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, moveTo, (delta * InSpeed) * acc);
                }
            }
            else
            {
                defaultState = true;

                transform.localRotation = Quaternion.Slerp(transform.localRotation, DefaultRot, delta * (OutSpeed * rotationSpeedMultiplier));
                transform.localPosition = Vector3.Lerp(transform.localPosition, DefaultPos, delta * OutSpeed);
            }
        }
    }
}
