using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
namespace Resonance.UI
{

    public class DamageIndicator : MonoBehaviour
    {
        private static DamageIndicator _instance;

        public static DamageIndicator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DamageIndicator>();
                }
                return _instance;
            }
        }

        [Range(1, 5)] public float FadeTime = 3;
        [Header("References")]
        public RectTransform indicatorPivot;
        public CanvasGroup indicatorAlpha; 

        private float alpha = 0.0f;
        private float rotationOffset;
        Vector3 eulerAngle = Vector3.zero;
        Vector3 forward;
        Vector3 rhs;
        private Vector3 attackDirection; 

        protected virtual void Awake()
        {
            enabled = ProjectConfigRoot.Instance.hitDirectionIndicator;
            EventBus.Subscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnLocalSpawn);
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnLocalSpawn);
            if (indicatorAlpha != null)
                indicatorAlpha.alpha = 0;
        }

        void OnLocalSpawn()
        {
            indicatorAlpha.alpha = 0;
            alpha = 0;
        }
        /// 用于发送新的受击方向
        public void SetHit(Vector3 direction)
        {
            if (direction == Vector3.zero)
                return;

            attackDirection = direction;
            alpha = 3f;
        }
        /// 可见时更新位置
        protected virtual void Update()
        {
            if (alpha <= 0) return;
            if (RuntimeServices.LocalPlayerReferences == null) return;

            alpha -= Time.deltaTime;
            UpdateDirection();
        }
        /// 按箭头指示更新方向
        void UpdateDirection()
        {
            rhs = attackDirection - RuntimeServices.LocalPlayerReferences.PlayerCameraTransform.position;
            rhs.y = 0;
            rhs.Normalize();
            if (GameController.Instance.CameraRendered != null)
            {
                forward = GameController.Instance.CameraRendered.transform.forward;
            }
            else
            {
                forward = transform.forward;
            }
            float GetPos = Vector3.Dot(forward, rhs);
            if (Vector3.Cross(forward, rhs).y > 0)
            {
                rotationOffset = (1f - GetPos) * 90;
            }
            else
            {
                rotationOffset = (1f - GetPos) * -90;
            }
            if (indicatorPivot != null)
            {
                indicatorAlpha.alpha = alpha;
                eulerAngle.z = -rotationOffset;
                indicatorPivot.eulerAngles = eulerAngle;
            }
        }

        private void OnEvt_onLocalPlayerSpawn_OnLocalSpawn(LocalPlayerSpawnedEvent e) => OnLocalSpawn();
    }
}
