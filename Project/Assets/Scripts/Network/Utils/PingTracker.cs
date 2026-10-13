using TMPro;
using UnityEngine;

namespace GFWK.Runtime.Network
{
    public class PingTracker : MonoBehaviour
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

        /// <summary>
        ///
        /// </summary>
        private void Start()
        {
            InvokeRepeating(nameof(FetchPing), 0, fetchPingEach);
            maxPingAllowed = RoomSettings.Instance.CurrentRoomInfo.maxPing;
            OnSettingsChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            GameEvents.onGameSettingsChange += OnSettingsChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            GameEvents.onGameSettingsChange -= OnSettingsChanged;
        }

        /// <summary>
        ///
        /// </summary>
        public void FetchPing()
        {
            CurrentPing = PhotonNetworkWrapper.GetPing();
            PingCheck();
        }

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
        private void KickDuePing()
        {
            PhotonNetworkWrapper.Instance.OnPingKick();
            PhotonNetworkWrapper.LeaveRoom();
        }

        /// <summary>
        /// 
        /// </summary>
        void OnSettingsChanged()
        {
            if (pingText != null)
            {
                isShowingPing = (bool)GFWKCore.Settings.GetSettingOf("显示ping");
                pingText.gameObject.SetActive(isShowingPing);
            }
        }
    }
}