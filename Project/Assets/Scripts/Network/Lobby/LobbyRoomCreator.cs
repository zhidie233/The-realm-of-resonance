using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using GFWK.Internal.Structures;

public class LobbyRoomCreator : MonoBehaviour, IConnectionCallbacks, ILobbyCallbacks
{
    public string defaultRoomPrefix = "Room";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GFWKRoomInfo BuildRoomInfo()
    {
        var mode = GameModeInfo;
        var room = new GFWKRoomInfo();

        room.roomName = RoomName;
        room.mapName = Map.ShowName;
        room.sceneName = Map.RealSceneName;
        room.maxPlayers = MaxPlayers;
        room.maxPing = MaxPing;
        room.goal = Goal;
        room.gameMode = mode.gameMode;
        room.time = TimeLimit;
        room.password = Password;
        room.friendlyFire = FriendlyFire;
        room.autoTeamSelection = AutoTeamSelection;
        room.withBots = BotsActive;
        room.roundStyle = PerRoundGame;
        return room;
    }

    #region Photon Callbacks

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
    }

    public void OnDisconnected(DisconnectCause cause)
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    public void OnJoinedLobby()
    {
    }

    public void OnLeftLobby()
    {
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }
    #endregion

    #region Getters
    public MapInfo Map => MapList[LobbyRoomCreatorUI.Instance.Map];
    public int MaxPlayers => GameModeInfo.maxPlayers[LobbyRoomCreatorUI.Instance.MaxPlayer];
    public int MaxPing => MatchLobby.Instance.MaxPing[LobbyRoomCreatorUI.Instance.MaxPing];
    public int Goal => GameModeInfo.GetGoalValue(LobbyRoomCreatorUI.Instance.Goal);
    public int TimeLimit => GameModeInfo.timeLimits[LobbyRoomCreatorUI.Instance.TimeLimit];
    public GameModeSettings GameModeInfo => LobbyRoomCreatorUI.Instance.CurrentGameMode;
    public bool IsPrivate => LobbyRoomCreatorUI.Instance.IsPrivate;
    public bool FriendlyFire => LobbyRoomCreatorUI.Instance.FriendlyFire;
    public bool BotsActive => GameModeInfo.supportBots ? LobbyRoomCreatorUI.Instance.BotsActive : false;
    public bool AutoTeamSelection => GameModeInfo.AutoTeamSelection ? true :  LobbyRoomCreatorUI.Instance.AutoTeamSelection;
    public RoundStyle PerRoundGame => GameModeInfo.HasForcedRoundMode() ? GameModeInfo.GetAllowedRoundMode() : LobbyRoomCreatorUI.Instance.GamePerRound;
    public string Password => LobbyRoomCreatorUI.Instance.RoomPassword;
    public string RoomName => LobbyRoomCreatorUI.Instance.RoomName;

    private List<MapInfo> m_mapList;
    public List<MapInfo> MapList
    {
        get
        {
            if (m_mapList == null) m_mapList = GameConfig.Instance.AllScenes;
            return m_mapList;
        }
    }
    #endregion

    private static LobbyRoomCreator _instance;
    public static LobbyRoomCreator Instance
    {
        get
        {
            if (_instance == null)_instance = FindObjectOfType<LobbyRoomCreator>();
            return _instance;
        }
    }
}