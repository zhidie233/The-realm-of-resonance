using TMPro;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Multiplayer
{

    public class PingMonitor : MonoBehaviour
    {
        public int fetchPingEach = 5;
        public int kickAfter = 10;
        public string displayFormat = "PING: {0}";

        [SerializeField] private GameObject highPingIndicator = null;
        [SerializeField] private TextMeshProUGUI pingText = null;

        public int CurrentPing { get; private set; } = 0;

        private int maxPingAllowed = 1000;
        private bool isWarned = false;
        private bool isShowingPing = false;

        private void Start()
        {
            InvokeRepeating(nameof(FetchPing), 0, fetchPingEach);
            maxPingAllowed = RoomSettings.Instance.CurrentRoomInfo.maxPing;
            OnSettingsChanged();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameSettingsChangedEvent>(OnEvt_onGameSettingsChange_OnSettingsChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameSettingsChangedEvent>(OnEvt_onGameSettingsChange_OnSettingsChanged);
        }

        public void FetchPing()
        {
            CurrentPing = NetworkWrapper.GetPing();
            PingCheck();
        }

        private void PingCheck()
        {
            if (pingText != null && isShowingPing) pingText.text = string.Format(displayFormat, CurrentPing);
            if (CurrentPing >= maxPingAllowed)
            {
                if (highPingIndicator != null) highPingIndicator.SetActive(true);

                if (!isWarned)
                {
                    Invoke(nameof(KickDuePing), kickAfter);
                    isWarned = true;
                }
            }
            else
            {
                if (highPingIndicator != null) highPingIndicator.SetActive(false);
                if (isWarned)
                {
                    CancelInvoke();
                    isWarned = false;
                }
            }
        }

        private void KickDuePing()
        {
            NetworkWrapper.Instance.OnPingKick();
            NetworkWrapper.LeaveRoom();
        }

        void OnSettingsChanged()
        {
            if (pingText != null)
            {
                isShowingPing = (bool)RuntimeServices.Settings.GetSettingOf("显示ping");
                pingText.gameObject.SetActive(isShowingPing);
            }
        }

        private void OnEvt_onGameSettingsChange_OnSettingsChanged(GameSettingsChangedEvent e) => OnSettingsChanged();
    }
}
