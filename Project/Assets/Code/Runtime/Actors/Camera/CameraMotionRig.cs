using System.Collections;
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

    public class CameraMotionRig : MonoBehaviour
    {
        [ScriptableDrawer] public CameraMotionRigConfig motionSettings;

        private bool wiggleMotion = true;
        private bool breathingMotion = false;
        private float m_breathingAmplitude;
        private float m_startBreathingTime;
        private Vector3 breathRotation = Vector3.zero;
        private float sideTilt = 0;
        private Quaternion rotationMotion, extraRotation;
        private bool isSliding = false;
        private PlayerRig playerRefs;

        protected virtual void Awake()
        {
            playerRefs = GetComponentInParent<PlayerRig>();
            m_breathingAmplitude = motionSettings.breatheAmplitude;
            rotationMotion = transform.localRotation;
        }

        protected virtual void OnEnable()
        {
            EventBus.Subscribe<PlayerLandedEvent>(OnEvt_onPlayerLand_OnSmallImpact);
            wiggleMotion = ProjectConfigRoot.Instance.cameraSwayEnabled;
            EventBus.Subscribe<LocalPlayerStateChangedEvent>(OnEvt_onLocalPlayerStateChanged_OnPlayerStateChanged);
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe<PlayerLandedEvent>(OnEvt_onPlayerLand_OnSmallImpact);
            EventBus.Unsubscribe<LocalPlayerStateChangedEvent>(OnEvt_onLocalPlayerStateChanged_OnPlayerStateChanged);
        }

        protected virtual void Update()
        {
            if (!LobbyMenu.Instance.isCursorLocked) return;

            Breathe();

            Wiggle();

            transform.localRotation = rotationMotion * extraRotation;
        }

        void Wiggle()
        {
            if (!wiggleMotion) return;

            float rollAngle = -GameInput.MouseX * motionSettings.tiltAngle;
            if (isSliding) rollAngle = 0;
            rollAngle = Mathf.Clamp(rollAngle, -motionSettings.tiltAngle, motionSettings.tiltAngle);

            if (motionSettings.sideMoveEffector > 0)
            {
                sideTilt = Mathf.Lerp(sideTilt, -GameInput.Horizontal * motionSettings.sideMoveEffector, Time.deltaTime * (motionSettings.smooth * 2));
                rollAngle += sideTilt;
            }

            breathRotation.z = GameInput.Aim() ? rollAngle * motionSettings.aimMultiplier : rollAngle;
            rotationMotion = Quaternion.Slerp(rotationMotion, Quaternion.Euler(breathRotation), Time.smoothDeltaTime * motionSettings.smooth);
        }

        void Breathe()
        {
            if (!breathingMotion) return;

            float theta = (Time.time - m_startBreathingTime) / motionSettings.breathePeriod;
            float vertical = m_breathingAmplitude * Mathf.Sin(theta);
            float horizontal = m_breathingAmplitude * Mathf.Cos(theta * 0.5f);
            breathRotation = (Vector3.up * vertical) + (Vector3.right * horizontal);
        }

        /// 给相机运动额外增加一段旋转
        public void AddCameraRotation(Quaternion rotation)
        {
            extraRotation = rotation;
        }

        public void SetActiveBreathing(bool active, float amplitude = 0)
        {
            breathingMotion = active;

            if (!breathingMotion)
            {
                m_breathingAmplitude = motionSettings.breatheAmplitude;
                breathRotation = Vector3.zero;
            }
            else
            {
                m_breathingAmplitude = amplitude;
                m_startBreathingTime = Time.time;
            }
        }

        void OnSmallImpact(float impactAmount)
        {
            StartCoroutine(FallEffect());
        }

        IEnumerator FallEffect()
        {
            Vector3 target = Vector3.zero;
            Quaternion rot;
            float d = 0;

            while (d <= 1)
            {
                d += Time.deltaTime / motionSettings.downMotionDuration;

                target.x = (motionSettings.downPitchMovement.Evaluate(d) * motionSettings.DownAmount);
                rot = Quaternion.Euler(target);
                transform.localRotation = rotationMotion * extraRotation * rot;
                yield return null;
            }
        }

        void OnPlayerStateChanged(PlayerState from, PlayerState to)
        {
            isSliding = to == PlayerState.Sliding;
        }

        private void OnEvt_onPlayerLand_OnSmallImpact(PlayerLandedEvent e) => OnSmallImpact(e.ImpactAmount);
        private void OnEvt_onLocalPlayerStateChanged_OnPlayerStateChanged(LocalPlayerStateChangedEvent e) => OnPlayerStateChanged(e.From, e.To);
    }
}
