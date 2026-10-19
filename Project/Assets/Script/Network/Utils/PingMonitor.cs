using TMPro;
using UnityEngine;

namespace Game.Runtime.Network
{
    public class PingMonitor : MonoBehaviour
    {
        public int fetchPingEach = 5;
        public int kickAfter = 10; // Timeout after detect high ping limit, before kick the player.
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
            GameEvents.onGameSettingsChange += OnSettingsChanged;
        }

        private void OnDisable()
        {
            GameEvents.onGameSettingsChange -= OnSettingsChanged;
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
                isShowingPing = (bool)GameCore.Settings.GetSettingOf("显示ping");
                pingText.gameObject.SetActive(isShowingPing);
            }
        }
    }
}