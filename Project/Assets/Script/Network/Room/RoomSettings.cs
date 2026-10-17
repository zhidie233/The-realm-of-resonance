using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Game.Runtime.Settings;
using System.Collections.Generic;

/// <summary>
/// This script handle all the room properties, setup the required local player data containers
/// and handle the auto team selection logic.
/// </summary>
public class RoomSettings : MonoBehaviourBase
{
    [GameToggle] public bool canSuicide = true;

    #region Public properties
    public GameMode CurrentGameMode => CurrentRoomInfo.gameMode;
    public int GameGoal => CurrentRoomInfo.goal;
    public bool AutoTeamSelection => CurrentRoomInfo.autoTeamSelection;
    public GameRoomInfo CurrentRoomInfo { get; set; }
    public bool RoomInfoFetched { get; set; } = false;
    #endregion

    private Dictionary<string, object> matchPersistData = new Dictionary<string, object>();

    protected override void Awake()
    {
        base.Awake();
        if ((!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) && !GameConfig.Instance.offlineMode)
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
    /// Reset all the room properties to it's default values
    /// </summary>
    public void ResetRoom()
    {
        Hashtable table = new Hashtable();
        //Initialize new properties where the information will stay Room
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            table.Add(PropertiesKeys.Team1Score, 0);
            table.Add(PropertiesKeys.Team2Score, 0);
            PhotonNetworkWrapper.CurrentRoom.SetCustomProperties(table);
        }
        table.Clear();
        //Initialize new properties where the information will stay Players
        if (!GameCore.GameData.UsingWaitingRoom() || PhotonNetwork.OfflineMode)
        {
            table.Add(PropertiesKeys.TeamKey, Team.None.ToString());
        }
        table.Add(PropertiesKeys.KillsKey, 0);
        table.Add(PropertiesKeys.DeathsKey, 0);
        table.Add(PropertiesKeys.ScoreKey, 0);
        table.Add(PropertiesKeys.UserRole, GameConfig.Instance.RolePrefix);
        table.Add(PropertiesKeys.PlayerTotalScore, GameCore.LocalPlayer.Stats.GetAllTimeScore());

        LocalPlayer.SetCustomProperties(table);

        matchPersistData = new Dictionary<string, object>();

        GameEvents.DispatchRoomPropertiesReset();
    }

    /// <summary>
    /// Get the custom room properties of this room
    /// These properties was set by the room creator (Host)
    /// </summary>
    void GetRoomInfo()
    {
        CurrentRoomInfo = PhotonNetwork.CurrentRoom.GetRoomInfo();
        CheckAutoSpawn();
        RoomInfoFetched = true;
    }

    /// <summary>
    /// Check if the local player should spawn automatically
    /// or should let him decide when spawn.
    /// </summary>
    public void CheckAutoSpawn()
    {
        if (CurrentRoomInfo.autoTeamSelection && !GameCore.GameData.UsingWaitingRoom())
        {
            UIReferences.Instance.AutoTeam(true);
            UIReferences.Instance.ShowMenu(false);
            Invoke(nameof(SelectTeamAutomatically), 3);
        }
        else if (GameCore.GameData.UsingWaitingRoom() && LocalPlayer.GetPlayerTeam() == Team.None)
        {
            if (PhotonNetwork.OfflineMode && !CurrentRoomInfo.autoTeamSelection)
            {
                UIReferences.Instance.SetUpUI();
                UIReferences.Instance.SetUpJoinButtons(true);
                return;
            }

            if (!SpectatorModeBase.EnterAsSpectator && RoomCameraBase.Instance.GetCameraMode() != RoomCameraBase.CameraMode.Spectator)
            {
                UIReferences.Instance.AutoTeam(true);
                UIReferences.Instance.ShowMenu(false);
                Invoke(nameof(SelectTeamAutomatically), 3);
            }
            else
            {
                // if the player joined as spectator from the waiting room
            }
        }
    }

    /// <summary>
    /// Set the player that just join to the room to the team with less players on it automatically
    /// </summary>
    void SelectTeamAutomatically()
    {
        string joinText = isOneTeamMode ? GameTexts.JoinedInMatch.Localized(17) : GameTexts.JoinIn.Localized(23);
        int teamDelta = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
        int teamRecon = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
        Team team = Team.All;

        if (!isOneTeamMode)
        {
            if (teamDelta > teamRecon)
            {
                team = Team.Team2;
            }
            else if (teamDelta < teamRecon)
            {
                team = Team.Team1;
            }
            else if (teamDelta == teamRecon)
            {
                team = Team.Team1;
            }

            string jt = string.Format("{0} {1}", joinText, team.GetTeamName());
            KillFeedBase.Instance.SendTeamHighlightMessage(PhotonNetwork.NickName, jt, team);
        }
        else
        {
            KillFeedBase.Instance.SendMessageEvent(string.Format("{0} {1}", PhotonNetwork.NickName, joinText));
        }
        RoomMenu.Instance.OnAutoTeam();
        UIReferences.Instance.AutoTeam(false);

        if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn == GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish && GameController.Instance.GameMatchState == MatchState.Playing)
        {
            if (RoomMenu.Instance.onWaitUntilRoundFinish != null) { RoomMenu.Instance.onWaitUntilRoundFinish.Invoke(team); }
            GameController.Instance.SetLocalPlayerToTeam(team);
            return;
        }

        //spawn player
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