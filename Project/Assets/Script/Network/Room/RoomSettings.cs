using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// 该脚本处理所有房间属性，初始化所需的本地玩家数据容器
/// 并处理自动选队逻辑。
/// </summary>
public class RoomSettings : MonoBehaviourBase
{
    [GameToggle] public bool canSuicide = true;

    #region Public properties
    public GameMode CurrentGameMode => CurrentRoomInfo.gameMode;
    public int GameGoal => CurrentRoomInfo.goal;
    public bool AutoAssignTeams => CurrentRoomInfo.autoAssignTeams;
    public GameRoomInfo CurrentRoomInfo { get; set; }
    public bool RoomInfoFetched { get; set; } = false;
    #endregion

    private Dictionary<string, object> matchPersistData = new Dictionary<string, object>();

    protected override void Awake()
    {
        base.Awake();
        if ((!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) && !GameConfig.Instance.directConnectMode)
            return;

        ResetRoom();
        GetRoomInfo();
    }

    IEnumerator Start()
    {
        while (!GameConfig.isDataCached) yield return null;
        if (GameCore.Settings != null) GameCore.Settings.ApplySettings(RuntimeSettingsProfile.ResolutionApplication.NoApply, true);
    }

    /// <summary>
    /// 把所有房间属性重置为默认值
    /// </summary>
    public void ResetRoom()
    {
        Hashtable table = new Hashtable();
        //初始化存放房间级信息的新属性
        if (NetworkWrapper.IsMasterClient)
        {
            table.Add(NetworkKeys.Team1Score, 0);
            table.Add(NetworkKeys.Team2Score, 0);
            NetworkWrapper.CurrentRoom.SetCustomProperties(table);
        }
        table.Clear();
        //初始化存放玩家级信息的新属性
        if (!GameCore.GameData.UsingReadyRoom() || PhotonNetwork.OfflineMode)
        {
            table.Add(NetworkKeys.TeamKey, Team.None.ToString());
        }
        table.Add(NetworkKeys.KillsKey, 0);
        table.Add(NetworkKeys.DeathsKey, 0);
        table.Add(NetworkKeys.ScoreKey, 0);
        table.Add(NetworkKeys.UserRole, GameConfig.Instance.RolePrefix);
        table.Add(NetworkKeys.PlayerTotalScore, GameCore.LocalPlayer.Stats.GetAllTimeScore());

        LocalPlayer.SetCustomProperties(table);

        matchPersistData = new Dictionary<string, object>();

        GameEvents.DispatchRoomPropertiesReset();
    }

    /// <summary>
    /// 获取本房间的自定义房间属性
    /// 这些属性由房间创建者（Host）设置
    /// </summary>
    void GetRoomInfo()
    {
        CurrentRoomInfo = PhotonNetwork.CurrentRoom.GetRoomInfo();
        CheckAutoSpawn();
        RoomInfoFetched = true;
    }

    /// <summary>
    /// 检查本机玩家是应自动出生
    /// 还是由他自己决定何时出生。
    /// </summary>
    public void CheckAutoSpawn()
    {
        if (CurrentRoomInfo.autoAssignTeams && !GameCore.GameData.UsingReadyRoom())
        {
            UIReferences.Instance.AutoTeam(true);
            UIReferences.Instance.ShowMenu(false);
            Invoke(nameof(SelectTeamAutomatically), 3);
        }
        else if (GameCore.GameData.UsingReadyRoom() && LocalPlayer.GetPlayerTeam() == Team.None)
        {
            if (PhotonNetwork.OfflineMode && !CurrentRoomInfo.autoAssignTeams)
            {
                UIReferences.Instance.SetUpUI();
                UIReferences.Instance.SetUpJoinButtons(true);
                return;
            }

            if (!ObserverModeBase.EnterAsSpectator && LobbyCameraBase.Instance.GetCameraMode() != LobbyCameraBase.CameraMode.Spectator)
            {
                UIReferences.Instance.AutoTeam(true);
                UIReferences.Instance.ShowMenu(false);
                Invoke(nameof(SelectTeamAutomatically), 3);
            }
            else
            {
                // 如果玩家以观战身份从等待房间加入
            }
        }
    }

    /// <summary>
    /// 把刚进房间的玩家自动分配到人数较少的队伍
    /// </summary>
    void SelectTeamAutomatically()
    {
        string joinText = isOneTeamMode ? GameTexts.JoinedInMatch.Localized(17) : GameTexts.JoinIn.Localized(23);
        int teamDelta = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
        int teamTwoCount = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
        Team team = Team.All;

        if (!isOneTeamMode)
        {
            if (teamDelta > teamTwoCount)
            {
                team = Team.Team2;
            }
            else if (teamDelta < teamTwoCount)
            {
                team = Team.Team1;
            }
            else if (teamDelta == teamTwoCount)
            {
                team = Team.Team1;
            }

            string jt = string.Format("{0} {1}", joinText, team.GetTeamName());
            KillReportEntry.Instance.SendTeamHighlightMessage(PhotonNetwork.NickName, jt, team);
        }
        else
        {
            KillReportEntry.Instance.SendMessageEvent(string.Format("{0} {1}", PhotonNetwork.NickName, joinText));
        }
        LobbyMenu.Instance.OnAutoTeam();
        UIReferences.Instance.AutoTeam(false);

        if (GetGameMode.GetGameModeInfo().spawnOnRoundBegin == GameModeSettings.OnRoundBegin.WaitUntilRoundFinish && GameController.Instance.GameMatchState == MatchState.Playing)
        {
            if (LobbyMenu.Instance.onWaitUntilRoundFinish != null) { LobbyMenu.Instance.onWaitUntilRoundFinish.Invoke(team); }
            GameController.Instance.SetLocalPlayerToTeam(team);
            return;
        }

        //生成玩家
        GameController.Instance.SpawnPlayer(team);
    }

    public static void AddMatchPersistData(string key, object value)
    {
        if (Instance == null) return;

        if (!Instance.matchPersistData.ContainsKey(key))
        {
            Instance.matchPersistData.Add(key, value);
            return;
        }

        Instance.matchPersistData[key] = value;
    }

    public static float IncreaseMatchPersistData(string key, float value)
    {
        if (Instance == null) return 0;

        if (!Instance.matchPersistData.ContainsKey(key))
        {
            Instance.matchPersistData.Add(key, value);
            return value;
        }

        float current = (float)Instance.matchPersistData[key];
        current += value;
        Instance.matchPersistData[key] = current;
        return current;
    }

    public static bool TryGetMatchPersistData(string key, out object value)
    {
        value = null;
        if (Instance == null || !Instance.matchPersistData.ContainsKey(key)) return false;

        value = Instance.matchPersistData[key];
        return true;
    }

    private static RoomSettings _instance;
    public static RoomSettings Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<RoomSettings>(); }
            return _instance;
        }
    }
}