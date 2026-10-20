using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CaptureOfFlagController : NetworkHelper, IGameMode
{
    [Header("Settings")]
    public int scorePerCapture = 500;
    public int scorePerRecover = 100;
    public float captureAreaRange = 3;
    [GameToggle] public bool moveFlagWithCarrierMotion = true;

    [Header("Events (For Local Only)")]
    public GameEvents.UEvent onPickUp;
    public GameEvents.UEvent onCapture;
    public GameEvents.UEvent onRecover;

    [Header("References")]
    public GameObject Content;
    public FlagPoint Team1Flag;
    public FlagPoint Team2Flag;

    private bool isActive = false;
    public int CapturesToWin { get; set; } = 3;

    void Awake()
    {
        if (!NetworkWrapper.IsConnected)
            return;

        Initialize();
    }

    void OnDisable()
    {
        if (isActive)
        {
            NetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
        }
    }

    void LateUpdate()
    {
        if (Team1Flag) Team1Flag.HandleFlagCapture();
        if (Team2Flag) Team2Flag.HandleFlagCapture();
    }

    void CheckScores()
    {
        if (!RoomSettings.Instance.RoomInfoFetched) return;

        int team1 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team2);

        if (team1 >= CapturesToWin || team2 >= CapturesToWin)
        {
            MatchTimeManagerBase.Instance.FinishRound();
        }
    }

    void OnNetworkMessage(Hashtable data)
    {
        var command = (int)data["cmd"];
        switch (command)
        {
            case 0: // Change Flag State
                OnFlagStateChanged(data);
                break;
            case 1: // Drop a flag
                var flag = (Team)data["team"] == Team.Team1 ? Team1Flag : Team2Flag;
                flag.DropFlag(data);
                break;
            case 2: // Return flag to origin position
                flag = (Team)data["team"] == Team.Team1 ? Team1Flag : Team2Flag;
                flag.SetFlagToOrigin();
                break;
            case 3: // Sync flags states
                SyncFlags(data);
                break;
            default:
                Debug.Log("Not defined command: " + command);
                return;
        }
    }

    void OnFlagStateChanged(Hashtable data)
    {
        var flagTeam = (Team)data["team"];
        var newState = (FlagPoint.FlagState)data["state"];
        var flag = flagTeam == Team.Team1 ? Team1Flag : Team2Flag;
        var player = (Player)data["player"];

        flag.State = newState;
        switch (newState)
        {
            case FlagPoint.FlagState.Captured:
                flag.OnCapture(player);
                break;
            case FlagPoint.FlagState.PickUp:
                flag.OnPickup(player, (int)data["viewID"]);
                break;
            case FlagPoint.FlagState.InHome:
                flag.Recover(player);
                break;
        }

        // 夺旗模式共鸣值事件（ChangeResonance 内部仅房主生效，天然防止多端重复累加）
        switch (newState)
        {
            case FlagPoint.FlagState.PickUp:
                // 敌方夺走旗帜，战场激化，提升共鸣值
                AddResonance("FlagPickUp");
                break;
            case FlagPoint.FlagState.Captured:
                // 成功交旗，大幅提升共鸣值
                AddResonance("FlagCapture");
                break;
            case FlagPoint.FlagState.InHome:
                // 夺回己方旗帜，缓解战场激化，降低共鸣值
                AddResonance("FlagRecover");
                break;
        }
    }

    /// <summary>
    /// 由旗帜事件触发共鸣值变化（数值在 ResonanceConfig 中配置）
    /// </summary>
    private void AddResonance(string resonanceKey)
    {
        if (ResonanceManager.Instance == null) return;
        ResonanceManager.Instance.ChangeResonance(resonanceKey);
    }

    void SyncFlags(Hashtable data)
    {
        Team1Flag.State = (FlagPoint.FlagState)data["f1s"];
        Team2Flag.State = (FlagPoint.FlagState)data["f2s"];

        if (Team1Flag.State == FlagPoint.FlagState.PickUp)
        {
            var player = GameController.Instance.FindActor((int)data["f1ca"]);
            if (player != null) Team1Flag.SetFlagToCarrier(player.GetComponent<PlayerConfig>());
        }

        if (Team2Flag.State == FlagPoint.FlagState.PickUp)
        {
            var player = GameController.Instance.FindActor((int)data["f2ca"]);
            if (player != null) Team2Flag.SetFlagToCarrier(player.GetComponent<PlayerConfig>());
        }
    }

    #region GameMode Interface
    public void Initialize()
    {
        //检查这是否是当前房间的游戏模式
        if (GameController.Instance.IsGameMode(GameMode.CTF, this))
        {
            Content.SetActive(true);
            NetworkWrapper.Instance.AddCallback(NetworkKeys.CaptureOfFlagMode, OnNetworkMessage);
            GameController.Instance.WaitForPlayers(GameMode.CTF.GetGameModeInfo().RequiredPlayersToStart);
            CaptureOfFlagUI.Instance.ShowUp();
            CapturesToWin = (int)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.RoomGoal];
            isActive = true;
        }
        else
        {
            Content.SetActive(false);
            CaptureOfFlagUI.Instance.Hide();
        }
    }

    public void OnFinishTime(bool gameOver)
    {
        //判定胜者
        string finalText = "";
        if (GetWinnerTeam() != Team.None)
        {
            finalText = GetWinnerTeam().GetTeamName();
        }
        else
        {
            finalText = GameTexts.NoOneWonName;
        }
        RoundFinishScreenBase.Instance?.Show(finalText);
    }

    public void OnLocalPlayerDeath()
    {

    }

    public void OnLocalPlayerKill()
    {

    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
        NetworkWrapper.CurrentRoom.SetTeamScore(teamToAddPoint);
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
        if (NetworkWrapper.IsMasterClient)
        {
            var data = CommonUtil.CreatePhotonHashTable();
            data.Add("cmd", 3);
            data.Add("f1s", Team1Flag.State);
            data.Add("f2s", Team2Flag.State);
            if (Team1Flag.State == FlagPoint.FlagState.PickUp)
            {
                data.Add("f1ca", Team1Flag.carriyingPlayer.View.ViewID);
            }
            if (Team2Flag.State == FlagPoint.FlagState.PickUp)
            {
                data.Add("f2ca", Team1Flag.carriyingPlayer.View.ViewID);
            }
            NetworkWrapper.Instance.SendDataOverNetworkToPlayer(NetworkKeys.CaptureOfFlagMode, data, newPlayer);
        }
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {

    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(NetworkKeys.Team1Score) || propertiesThatChanged.ContainsKey(NetworkKeys.Team2Score))
        {
            int team1 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team1);
            int team2 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team2);
            CaptureOfFlagUI.Instance.SetScores(team1, team2);
            CheckScores();
        }
    }

    public bool isLocalPlayerWinner
    {
        get
        {
            return (NetworkWrapper.LocalPlayer.GetPlayerTeam() == GetWinnerTeam());
        }
    }
    #endregion

    public FlagPoint GetFlag(Team team)
    {
        if (team == Team.Team1)
        {
            return Team1Flag;
        }

        return Team2Flag;
    }

    public Team GetWinnerTeam()
    {
        int team1 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team2);

        Team winner = Team.None;
        if (team1 > team2) { winner = Team.Team1; }
        else if (team1 < team2) { winner = Team.Team2; }
        else { winner = Team.None; }
        return winner;
    }

    public static Team GetOppositeTeam(Team team)
    {
        if (team == Team.Team1)
        {
            return Team.Team2;
        }

        return Team.Team1;
    }

    private static CaptureOfFlagController _instance;
    public static CaptureOfFlagController Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<CaptureOfFlagController>(); }
            return _instance;
        }
    }
}