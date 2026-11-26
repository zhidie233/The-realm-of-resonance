using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
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

    [DefaultExecutionOrder(-1000)]
    public class SoloRoom : MonoBehaviour, IConnectionCallbacks
    {
        [Header("Offline Room")]
        public GameMode gameMode = GameMode.FreeMatch;
        [GameToggle] public bool forceOffline = false;
        [GameToggle] public bool withBots = false;
        [GameToggle] public bool autoAssignTeams = true;
        [Range(1, 64)] public int maxPlayers = 1;
        public int MatchTime = 9989;
        public int gameModeGoal = 100;
        public RoundStyle roundStyle = RoundStyle.OneMatch;
        [Header("References")]
        public GameObject PhotonObject;

        private void Awake()
        {
            PhotonNetwork.AddCallbackTarget(this);
            if (!NetworkWrapper.IsConnectedInRoom)
            {
                if (ProjectConfigRoot.Instance.directConnectMode || forceOffline)
                {
                    InputHandler.Initialize();
                    PhotonNetwork.OfflineMode = true;
                    PhotonNetwork.NickName = "Offline Player";
                    if(NetworkWrapper.Instance == null)
                    Instantiate(PhotonObject);
                }
                else
                {
                    PhotonNetwork.OfflineMode = false;
                }
            }
        }

        void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnConnectedToMaster()
        {
            Hashtable roomOption = new Hashtable();
            roomOption[NetProtocol.TimeRoomKey] = MatchTime;
            roomOption[NetProtocol.GameModeKey] = gameMode.ToString();
            roomOption[NetProtocol.SceneNameKey] = ProjectConfigRoot.Instance.SceneTable[0].RealSceneName;
            roomOption[NetProtocol.RoomRoundKey] = roundStyle;
            roomOption[NetProtocol.TeamSelectionKey] = autoAssignTeams;
            roomOption[NetProtocol.CustomSceneName] = ProjectConfigRoot.Instance.SceneTable[0].ShowName;
            roomOption[NetProtocol.RoomGoal] = gameModeGoal;
            roomOption[NetProtocol.MaxPing] = 1000;
            roomOption[NetProtocol.RoomPassword] = "";
            roomOption[NetProtocol.WithBotsKey] = withBots;
            PhotonNetwork.CreateRoom("Offline Room", new RoomOptions()
            {
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true,
                CustomRoomProperties = roomOption,
                CleanupCacheOnLeave = true,
                PublishUserId = true,
                EmptyRoomTtl = 0,
            }, null);
        }

        public void OnConnected()
        {
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {      
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {     
        }

        public void OnDisconnected(DisconnectCause cause)
        {     
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {      
        }

        private static SoloRoom _instance;
        public static SoloRoom Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<SoloRoom>(); }
                return _instance;
            }
        }
    }
}
