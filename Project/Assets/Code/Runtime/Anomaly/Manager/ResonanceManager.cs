using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.UI;
namespace Resonance.Anomaly
{

    public class ResonanceManager : NetworkActorBehaviour
    {
        public static ResonanceManager Instance { get; private set; }

        [Header("当前共鸣值")]
        [SerializeField] private float currentResonance = 0f;

        private PhotonView _photonView;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _photonView = GetComponent<PhotonView>();
        }

        protected virtual void OnEnable()
        {
            NetworkCallbacks.RoomPropertiesUpdate += OnRoomPropertiesUpdate;
        }

        protected virtual void OnDisable()
        {
            NetworkCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesUpdate;
        }

        private void Start()
        {
            if (ProjectConfigRoot.Instance.resonanceConfig != null)
                ProjectConfigRoot.Instance.resonanceConfig.Initialize();

            currentResonance = ReadRoomResonance();
            EventBus.Publish(new ResonanceValueChangedEvent(currentResonance));
        }

        /// 修改共鸣值，仅房主生效，结果写入房间属性
        public void ChangeResonance(string changeKey, float multiplier = 1f)
        {
            if (!NetworkWrapper.IsMasterClient) return;
            if (!ProjectConfigRoot.Instance.resonanceEnabled) return;

            currentResonance += ProjectConfigRoot.Instance.resonanceConfig.GetResonanceChangeValue(changeKey) * multiplier;

            Hashtable properties = new Hashtable();
            properties[NetProtocol.ResonanceValueKey] = currentResonance;
            NetworkWrapper.CurrentRoom.SetCustomProperties(properties);

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
            if (NetworkWrapper.IsMasterClient)
            {
                ChangeResonance(changeKey, damage);
                return;
            }

            if (_photonView == null || !NetworkWrapper.InRoom) return;
            _photonView.RPC(nameof(RPC_RequestChange), RpcTarget.MasterClient, changeKey, damage);
        }

        [PunRPC]
        void RPC_RequestChange(string changeKey, float damage)
        {
            ChangeResonance(changeKey, damage);
        }

        void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (!propertiesThatChanged.ContainsKey(NetProtocol.ResonanceValueKey)) return;

            currentResonance = (float)propertiesThatChanged[NetProtocol.ResonanceValueKey];
            EventBus.Publish(new ResonanceValueChangedEvent(currentResonance));
        }

        float ReadRoomResonance()
        {
            if (!NetworkWrapper.InRoom) return 0f;
            object value;
            if (NetworkWrapper.CurrentRoom.CustomProperties.TryGetValue(NetProtocol.ResonanceValueKey, out value))
                return (float)value;
            return 0f;
        }

        public float GetCurrentResonance()
        {
            return currentResonance;
        }
    }
}
