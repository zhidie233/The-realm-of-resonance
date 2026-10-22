using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-1000)]
public class SoloRoom : MonoBehaviour, IConnectionCallbacks
{
    [Header("Offline Room")]
    public GameMode gameMode = GameMode.FreeMatch;
    [GameToggle] public bool forceOffline = false;
    [GameToggle] public bool withBots = false;
    [GameToggle] public bool autoAssignTeams = true;
    [GameToggle] public bool friendlyFire = false;
    [Range(1, 64)] public int maxPlayers = 1;
    public int MatchTime = 9989;
    public int gameModeGoal = 100;
    public RoundStyle roundStyle = RoundStyle.OneMacht;
    [Header("References")]
    public GameObject PhotonObject;

    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (!NetworkWrapper.IsConnectedInRoom)
        {
            if (GameConfig.Instance.directConnectMode || forceOffline)
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
        roomOption[NetworkKeys.TimeRoomKey] = MatchTime;
        roomOption[NetworkKeys.GameModeKey] = gameMode.ToString();
        roomOption[NetworkKeys.SceneNameKey] = GameConfig.Instance.SceneTable[0].RealSceneName;
        roomOption[NetworkKeys.RoomRoundKey] = roundStyle;
        roomOption[NetworkKeys.TeamSelectionKey] = autoAssignTeams;
        roomOption[NetworkKeys.CustomSceneName] = GameConfig.Instance.SceneTable[0].ShowName;
        roomOption[NetworkKeys.RoomGoal] = gameModeGoal;
        roomOption[NetworkKeys.RoomFriendlyFire] = friendlyFire;
        roomOption[NetworkKeys.MaxPing] = 1000;
        roomOption[NetworkKeys.RoomPassword] = "";
        roomOption[NetworkKeys.WithBotsKey] = withBots;
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
