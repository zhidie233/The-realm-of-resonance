using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Domain.Anomaly;
using Resonance.Core;
using Resonance.Multiplayer;

namespace Resonance.Anomaly
{
    public class ResonanceManager : NetworkActorBehaviour
    {
        public static ResonanceManager Instance { get; private set; }

        [Header("当前共鸣值")]
        [SerializeField] private float currentResonance = 0f;

        private readonly ResonanceRuleEngine _ruleEngine = new ResonanceRuleEngine();
        private PhotonView _photonView;
        private IResonanceStateSync _stateSync;

        private IResonanceStateSync StateSync => _stateSync ?? (_stateSync = new PhotonResonanceSync(this));

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _photonView = GetComponent<PhotonView>();
            _stateSync = new PhotonResonanceSync(this);
        }

        public void ConfigureSync(IResonanceStateSync sync)
        {
            _stateSync = sync ?? new PhotonResonanceSync(this);
        }

        protected virtual void OnEnable()
        {
            NetworkCallbacks.RoomPropertiesUpdate += OnRoomPropertiesUpdate;
            EventBus.Subscribe<CombatDamageDealtEvent>(HandleCombatDamageDealt);
            EventBus.Subscribe<ObjectiveStateChangedEvent>(HandleObjectiveStateChanged);
            EventBus.Subscribe<ResonanceImpulseEvent>(HandleResonanceImpulse);
            EventBus.Subscribe<RoomPropertiesResetEvent>(HandleRoomPropertiesReset);
        }

        protected virtual void OnDisable()
        {
            NetworkCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesUpdate;
            EventBus.Unsubscribe<CombatDamageDealtEvent>(HandleCombatDamageDealt);
            EventBus.Unsubscribe<ObjectiveStateChangedEvent>(HandleObjectiveStateChanged);
            EventBus.Unsubscribe<ResonanceImpulseEvent>(HandleResonanceImpulse);
            EventBus.Unsubscribe<RoomPropertiesResetEvent>(HandleRoomPropertiesReset);
        }

        private void Start()
        {
            var config = ProjectConfigRoot.Instance.resonanceConfig;
            if (config != null)
            {
                config.Initialize();
                _ruleEngine.LoadRules(config.EnumerateRules());
            }
            _ruleEngine.IsEnabled = ProjectConfigRoot.Instance.resonanceEnabled;

            float initial = StateSync.TryReadCurrentValue(out float roomValue) ? roomValue : 0f;
            SetCurrentValue(initial);
            EventBus.Publish(new ResonanceValueChangedEvent(currentResonance));
        }

        /// 修改共鸣值，仅房主生效，结果写入房间属性
        public void ChangeResonance(string changeKey, float multiplier = 1f)
        {
            if (!StateSync.IsAuthority) return;
            _ruleEngine.IsEnabled = ProjectConfigRoot.Instance.resonanceEnabled;

            if (!_ruleEngine.TryApplyChange(changeKey, multiplier, out float updated)) return;

            currentResonance = updated;
            StateSync.PublishValue(currentResonance);
            EventBus.Publish(new ResonanceValueChangedEvent(currentResonance));
        }

        /// 任意端请求修改共鸣值：房主直接结算，其余端转交房主
        public static void RequestChange(string changeKey, float damage)
        {
            if (Instance == null) return;
            Instance.SendChangeRequest(changeKey, damage);
        }

        void SendChangeRequest(string changeKey, float damage)
        {
            if (StateSync.IsAuthority)
            {
                ChangeResonance(changeKey, damage);
                return;
            }
            StateSync.RequestAuthorityChange(changeKey, damage);
        }

        [PunRPC]
        void RPC_RequestChange(string changeKey, float damage)
        {
            ChangeResonance(changeKey, damage);
        }

        void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (!propertiesThatChanged.ContainsKey(NetProtocol.ResonanceValueKey)) return;

            SetCurrentValue((float)propertiesThatChanged[NetProtocol.ResonanceValueKey]);
            EventBus.Publish(new ResonanceValueChangedEvent(currentResonance));
        }

        public float GetCurrentResonance() => currentResonance;

        private void SetCurrentValue(float value)
        {
            _ruleEngine.SetValue(value);
            currentResonance = _ruleEngine.CurrentValue;
        }

        private void HandleCombatDamageDealt(CombatDamageDealtEvent e)
        {
            var contribution = ResonanceContributionRouter.RouteCombatDamage(
                (int)e.AttackerTeam,
                (int)e.VictimTeam,
                e.Damage,
                e.IsOneTeamMode);
            ApplyContribution(contribution);
        }

        private void HandleObjectiveStateChanged(ObjectiveStateChangedEvent e)
        {
            ApplyContribution(ResonanceContributionRouter.RouteObjective(e.Kind));
        }

        private void HandleResonanceImpulse(ResonanceImpulseEvent e)
        {
            ApplyContribution(ResonanceContributionRouter.RouteDirectImpulse(e.Key, e.Multiplier));
        }

        private void HandleRoomPropertiesReset(RoomPropertiesResetEvent e)
        {
            _ruleEngine.Reset(0f);
            currentResonance = _ruleEngine.CurrentValue;
            if (StateSync.IsAuthority)
            {
                StateSync.PublishValue(currentResonance);
            }
            EventBus.Publish(new ResonanceValueChangedEvent(currentResonance));
        }

        private void ApplyContribution(ResonanceContribution contribution)
        {
            if (!contribution.IsValid) return;

            if (contribution.AuthorityOnly)
            {
                ChangeResonance(contribution.Key, contribution.Multiplier);
            }
            else
            {
                SendChangeRequest(contribution.Key, contribution.Multiplier);
            }
        }

        private sealed class PhotonResonanceSync : IResonanceStateSync
        {
            private readonly ResonanceManager _owner;

            public PhotonResonanceSync(ResonanceManager owner)
            {
                _owner = owner;
            }

            public bool IsAuthority => NetworkWrapper.IsMasterClient;

            public bool TryReadCurrentValue(out float value)
            {
                if (!NetworkWrapper.InRoom)
                {
                    value = 0f;
                    return false;
                }

                if (NetworkWrapper.CurrentRoom.CustomProperties.TryGetValue(NetProtocol.ResonanceValueKey, out object raw)
                    && raw is float f)
                {
                    value = f;
                    return true;
                }

                value = 0f;
                return false;
            }

            public void PublishValue(float value)
            {
                if (!NetworkWrapper.InRoom) return;
                Hashtable properties = new Hashtable();
                properties[NetProtocol.ResonanceValueKey] = value;
                NetworkWrapper.CurrentRoom.SetCustomProperties(properties);
            }

            public void RequestAuthorityChange(string changeKey, float multiplier)
            {
                if (_owner._photonView == null || !NetworkWrapper.InRoom) return;
                _owner._photonView.RPC(nameof(RPC_RequestChange), RpcTarget.MasterClient, changeKey, multiplier);
            }
        }
    }
}
