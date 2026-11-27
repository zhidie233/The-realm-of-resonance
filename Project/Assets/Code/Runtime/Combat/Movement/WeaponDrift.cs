using UnityEngine;
using System.Collections;
using Resonance.Foundation;
using Resonance.Core;
namespace Resonance.Combat
{

    public class WeaponDrift : MonoBehaviour
    {
        private float maxAmount = 0.05F;
        [Header("Movements")]
        [Range(0.2f, 5)] public float delayAmplitude = 2;
        [Range(0.01f, 0.5f)] public float pushAmplitude = 0.2f;
        [Range(1, 7)] public float sideAngleAmplitude = 4;
        public float Smoothness = 3.0F;
        public float aimMultiplier = 0.05f;

        [Header("FallEffect")]
        [Range(0.01f, 1.0f)]
        public float m_time = 0.2f;
        public float m_ReturnSpeed = 5;
        public float SliderAmount = 12;
        public float DownAmount = 13;
        public AnimationCurve downCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public float Amount { get; set; }

        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        private Quaternion delayRotation;
        private Vector3 targetVector = Vector3.zero;
        private Transform m_Transform;
        private float factorX, factorY, factorZ = 0;
        private Vector3 defaultEuler;
        private float deltaTime;
        private bool isAiming = false;
        private float amplitudeMultiplier = 1;

        protected virtual void Awake()
        {
            m_Transform = transform;
            defaultPosition = m_Transform.localPosition;
            defaultRotation = m_Transform.localRotation;
            Amount = delayAmplitude;
            defaultEuler = m_Transform.localEulerAngles;
        }

        protected virtual void Update()
        {     
            deltaTime = Time.smoothDeltaTime;
            DelayMovement();
            SideMovement();

            m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, defaultRotation * delayRotation, deltaTime * m_ReturnSpeed);
        }
        /// 用鼠标移动相机时的延迟跟随运动。
        void DelayMovement()
        {
            factorX = (-GameInput.MouseX * deltaTime * Amount) * amplitudeMultiplier;
            factorY = (-GameInput.MouseY * deltaTime * Amount) * amplitudeMultiplier;
            factorZ = (-GameInput.Vertical * (isAiming ? pushAmplitude * 0.1f : pushAmplitude)) * amplitudeMultiplier;
            factorX = Mathf.Clamp(factorX, -maxAmount, maxAmount);
            factorY = Mathf.Clamp(factorY, -maxAmount, maxAmount);
            targetVector.Set(defaultPosition.x + factorX, defaultPosition.y + factorY, defaultPosition.z + factorZ);
            delayRotation = Quaternion.Euler((factorY * 100), (-factorX * 100), 0);
            m_Transform.localPosition = Vector3.Slerp(m_Transform.localPosition, targetVector, deltaTime * Smoothness);
        }
        /// 玩家横向移动时的角度摆动
        void SideMovement()
        {
            factorX = GameInput.Horizontal;
            defaultEuler.z = (factorX * sideAngleAmplitude) * amplitudeMultiplier;
            defaultEuler.z = -defaultEuler.z;
            defaultRotation = Quaternion.Euler(defaultEuler);
            defaultRotation = Quaternion.Slerp(defaultRotation, Quaternion.identity, deltaTime * Smoothness);
        }

        protected virtual void OnEnable()
        {
            EventBus.Subscribe<PlayerLandedEvent>(OnEvt_onPlayerLand_OnSmallImpact);
            EventBus.Subscribe<LocalAimChangedEvent>(OnEvt_onLocalAimChanged_OnLocalAimChanged);
            EventBus.Subscribe<WeaponSwitchedEvent>(OnEvt_onChangeWeapon_OnLocalWeaponChanged);
            EventBus.Subscribe<LocalPlayerStateChangedEvent>(OnEvt_onLocalPlayerStateChanged_OnLocalStateChange);
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe<PlayerLandedEvent>(OnEvt_onPlayerLand_OnSmallImpact);
            EventBus.Unsubscribe<LocalAimChangedEvent>(OnEvt_onLocalAimChanged_OnLocalAimChanged);
            EventBus.Unsubscribe<WeaponSwitchedEvent>(OnEvt_onChangeWeapon_OnLocalWeaponChanged);
            EventBus.Unsubscribe<LocalPlayerStateChangedEvent>(OnEvt_onLocalPlayerStateChanged_OnLocalStateChange);
        }
        /// 碰撞事件
        void OnSmallImpact(float impactAmount)
        {
            StartCoroutine(FallEffect(impactAmount));
        }

        void OnLocalAimChanged(bool aim)
        {
            isAiming = aim;
        }

        void OnLocalWeaponChanged(int newWeapon)
        {
            isAiming = false;
        }

        void OnLocalStateChange(PlayerState from, PlayerState to)
        {
            if(to == PlayerState.Jumping)
            {
            }
        }

        public void SetMotionMultiplier(float multiplier)
        {
            amplitudeMultiplier = multiplier;
        }

        public void UseAimSettings()
        {
            amplitudeMultiplier = aimMultiplier;
        }
        /// 创建轻微的碰撞效果
        public IEnumerator FallEffect(float amount)
        {
            float mul = Mathf.Clamp01(amount / 3.3f);
            float side = SliderAmount * mul;
            Quaternion m_default = m_Transform.localRotation;
            Quaternion m_final = m_Transform.localRotation * Quaternion.Euler(new Vector3(DownAmount * mul, Random.Range(-side, side), m_default.z));
            float t_rate = 1.0f / m_time;
            float t_time = 0.0f;
            float t;

            while (t_time < 1.0f)
            {
                t = downCurve.Evaluate(t_time);
                t_time += Time.deltaTime * t_rate;
                m_Transform.localRotation = Quaternion.Slerp(m_default, m_final, t);
                yield return t_rate;
            }
        }

        /// 把所有设置重置为初始值
        public void ResetSettings()
        {
            Amount = delayAmplitude;
            amplitudeMultiplier = 1;
        }

        private void OnEvt_onPlayerLand_OnSmallImpact(PlayerLandedEvent e) => OnSmallImpact(e.ImpactAmount);
        private void OnEvt_onLocalAimChanged_OnLocalAimChanged(LocalAimChangedEvent e) => OnLocalAimChanged(e.IsAiming);
        private void OnEvt_onChangeWeapon_OnLocalWeaponChanged(WeaponSwitchedEvent e) => OnLocalWeaponChanged(e.WeaponId);
        private void OnEvt_onLocalPlayerStateChanged_OnLocalStateChange(LocalPlayerStateChangedEvent e) => OnLocalStateChange(e.From, e.To);
    }
}
