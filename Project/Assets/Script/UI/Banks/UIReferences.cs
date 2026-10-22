using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class UIReferences : NetworkHelper, IInRoomCallbacks
{
    [FlagEnum, SerializeField] private RoomUILayers m_uiMask = 0;
    public RoomUILayers UIMask
    {
        get => m_uiMask;
        set => m_uiMask = value;
    }

    public IGameResumeScreen ResumeScreen { get; set; }

    [Header("References")]
    public PlayerScoreboardBase playerScoreboards;
    [SerializeField] private GameObject SuicideButton = null;
    [SerializeField] private GameObject AutoTeamUI = null;
    [SerializeField] private GameObject SpectatorButton = null;
    [SerializeField] private GameObject ChangeTeamButton = null;
    [SerializeField] private TextMeshProUGUI SpawnProtectionText = null;
    [SerializeField] private TextMeshProUGUI RoomNameText = null;
    [SerializeField] private TextMeshProUGUI GameModeText = null;
    [SerializeField] private TextMeshProUGUI MaxPlayerText = null;
    [SerializeField] private TextMeshProUGUI MaxKillsText = null;
    [SerializeField] private TextMeshProUGUI ReadyPlayersText = null;
    public CanvasGroupFader blackScreen;
    public ConfirmationWindow leaveRoomConfirmation;

    private bool inTeam = false;
    private int ChangeTeamTimes = 0;

    private void Awake()
    {
        if (!NetworkWrapper.IsConnected || !NetworkWrapper.InRoom)
            return;

        PhotonNetwork.AddCallbackTarget(this);
        blackScreen.FadeOut(1);

        GetRoomInfo();
    }

    void OnEnable()
    {
        SetUpUI();

        GameEvents.onLocalPlayerSpawn += OnPlayerSpawn;
        GameEvents.onPickUpHealth += OnPicUpMedKit;
        GameEvents.onLocalPlayerDeath += OnLocalPlayerDeath;
        GameEvents.onRoundEnd += OnRoundFinish;
        NetworkCallbacks.LeftRoom += OnLeftRoom;
    }

    public void SetUpJoinButtons(bool forced = false)
    {
        if (!inTeam && (CanSelectTeamToJoin() || forced))
        {
            playerScoreboards.SetActiveByTeamMode(true);
        }
        else { playerScoreboards.SetActiveJoinButtons(false); }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        GameEvents.onLocalPlayerSpawn -= OnPlayerSpawn;
        GameEvents.onPickUpHealth -= OnPicUpMedKit;
        GameEvents.onLocalPlayerDeath -= OnLocalPlayerDeath;
        GameEvents.onRoundEnd -= OnRoundFinish;
        NetworkCallbacks.LeftRoom -= OnLeftRoom;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void OnLocalPlayerDeath()
    {
    }

    void OnRoundFinish()
    {
        playerScoreboards.SetActiveByTeamMode();
        StopAllCoroutines();
        blackScreen.SetAlpha(0);
    }

    void GetRoomInfo()
    {
        GameMode mode = GetGameMode;
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name.ToUpper();
        GameModeText.text = mode.GetName().ToUpper();
        int vs = (!isOneTeamMode) ? PhotonNetwork.CurrentRoom.MaxPlayers / 2 : PhotonNetwork.CurrentRoom.MaxPlayers - 1;
        MaxPlayerText.text = (!isOneTeamMode) ? string.Format("{0} VS {1}", vs, vs) : string.Format("1 VS {0}", vs);       
    }

    public void SetUpUI()
    {
        if (!RoomSettings.Instance.CurrentRoomInfo.autoAssignTeams)
        {
            PauseMenuBase.Instance.OpenMenu();
        }
       
        playerScoreboards.gameObject.SetActive(true);
        SetActiveChangeTeamButton(false);
        SetUpJoinButtons();
        UpdateDisplayUI();
        SuicideButton.SetActive(RoomSettings.Instance.canSuicide && GameController.Instance.IsLocalPlaying);

        if (NetworkWrapper.IsConnected)
        {
            if (MaxKillsText != null)
            {
                int MaxKills = (int)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.RoomGoal];
            MaxKillsText.text = string.Format("{0} 击杀", MaxKills);
            }
        }
        if (GameCore.GameData.UsingReadyRoom())
        {
            if (NetworkWrapper.OfflineMode && !NetworkWrapper.CurrentRoom.GetRoomInfo().autoAssignTeams) return;

            SetUpJoinButtons(true);
            SpectatorButton.SetActive(false);
            ShowMenu(false);
        }
    }

    public void ShowMenu(bool active)
    {
        if (active) PauseMenuBase.Instance.OpenMenu();
        else PauseMenuBase.Instance.CloseMenu();

        SetActiveChangeTeamButton(GameConfig.Instance.allowTeamSwitch && !isOneTeamMode && ChangeTeamTimes <= GameConfig.Instance.teamSwitchLimit 
            && !RoomSettings.Instance.AutoAssignTeams && MatchTimeManagerBase.HaveTimeStarted() && ObserverModeBase.Instance.CanSelectTeam());

        SuicideButton.SetActive(RoomSettings.Instance.canSuicide && GameController.Instance.IsLocalPlaying);

        if (!active)
        {
        }

        if (InputHandler.isGamePad) GamePadPointer.Instance?.SetActive(active);
    }

    public void SetUIMask(RoomUILayers newMask)
    {
        UIMask = newMask;
        UpdateDisplayUI();
        GameEvents.DispatchUIMaskChange(newMask);
    }

    public void Resume()
    {
        if (!GameController.Instance.FirstSpawnDone) return;

        CommonUtil.LockCursor(true);
        ShowMenu(false);
        ReticleBase.Instance.Show(true);
        GameEvents.DispatchGamePauseEvent(false);
    }

    public void UpdateDisplayUI()
    {
        PlayerUI?.UpdateUIDisplay();
        if (playerScoreboards != null)
        {
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards));
            playerScoreboards.BlockScoreboards = !UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards);
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards), CanSelectTeamToJoin());
        }
    }

    public void AutoTeam(bool v)
    {
        AutoTeamUI.SetActive(v);
        if (!v)
        {
            PauseMenuBase.Instance.CloseMenu();
            ObserverModeBase.Instance.SetActiveUI(false);
            SpectatorButton.SetActive(false);
            inTeam = true;
        }
        else
        {
            if (NetworkWrapper.OfflineMode)
            {
                AutoTeamUI.GetComponentInChildren<TextMeshProUGUI>().text = GameTexts.StartingOfflineRoom;
            }
        }
    }

    public void LeftRoom(bool quit)
    {
        if (!quit)
        {
            CommonUtil.LockCursor(false);
            leaveRoomConfirmation.AskConfirmation(GameTexts.LeaveMatchWarning, () => { LeftRoom(true); }, () => {  });
        }
        else
        {
            LobbyMenu.Instance.LeaveRoom();
        }
    }

    public void Suicide()
    {
        if (!GameCore.LocalPlayer.Suicide())
        {
            GameEvents.DispatchGamePauseEvent(false);
        }
        CommonUtil.LockCursor(true);
        ShowMenu(false);
    }

    public void JoinTeam(int id)
    {
        LobbyMenu.Instance.JoinTeam(id);
        PauseMenuBase.Instance.CloseMenu();
        inTeam = true;
        AutoTeamUI.SetActive(false);
        ObserverModeBase.Instance.SetActiveUI(false);
        SpectatorButton.SetActive(false);

        if (GameController.Joined) { ChangeTeamTimes++; }
        if (GameConfig.Instance.allowTeamSwitch && !isOneTeamMode && ChangeTeamTimes <= GameConfig.Instance.teamSwitchLimit && MatchTimeManagerBase.HaveTimeStarted())
        {
            SetActiveChangeTeamButton(true);
        }
    }

    /// <summary>
    /// 由 Change Team 按钮调用
    /// </summary>
    public void ActiveChangeTeam()
    {
        playerScoreboards.SetActive(false, true);
        SetActiveChangeTeamButton(false);
    }

    public void SetActiveChangeTeamButton(bool active)
    {
        if (ChangeTeamButton == null) return;

        ChangeTeamButton.SetActive(active);
    }

    public void ShowScoreboard(bool active)
    {
        playerScoreboards.SetActive(active);
    }

    public void OnSpawnCount(int count)
    {
        SpawnProtectionText.text = string.Format("出生保护倒计时: <b>{0}</b>", count);
        SpawnProtectionText.gameObject.SetActive(count > 0);
    }

    void OnPlayerSpawn()
    {
        if (InputHandler.isGamePad)
        {
            GamePadPointer.Instance?.SetActive(false);
        }
    }

    void OnPicUpMedKit(int Amount) => new GameLocalNotification(string.Format("+{0} 血量", Amount));

    public void SetReadyPlayersText(string text = "", bool show = false)
    {
        ReadyPlayersText.text = text;
        ReadyPlayersText.transform.parent.gameObject.SetActive(show);
    }

    public void ResetRound()
    {
        RoundFinishScreenBase.Instance?.Hide();
        inTeam = false;
        blackScreen.FadeOut(1);
        SetUpUI();
        ShowMenu(true);
        if (!RoomSettings.Instance.AutoAssignTeams)
        {
            playerScoreboards.ResetJoinButtons();
            SetUpJoinButtons();
        }
    }

    public void SetActiveTimeUI(bool active)
    {
        PlayerUI.TimeUIRoot.SetActive(active);
    }

    public void SetActiveMaxKillsUI(bool active) => PlayerUI.MaxKillsUI.SetActive(active);

    public IEnumerator FinalFade(bool fadein, bool goToLobby = true, float delay = 1)
    {
        if (!goToLobby)
        {
            if (GameController.Instance.GameFinish) { blackScreen.SetAlpha(0); yield break; }
        }

        if (fadein)
        {
            yield return new WaitForSeconds(delay);
            yield return blackScreen.FadeIn(1);
            if (goToLobby)
            {
                CommonUtil.LoadLevel(GameConfig.Instance.MainMenuScene);
            }
        }
        else
        {
            yield return new WaitForSeconds(delay);
            blackScreen.FadeOut(1);
        }
    }

    private bool CanSelectTeamToJoin()
    {
        return !inTeam && (!RoomSettings.Instance.AutoAssignTeams || !GameCore.GameData.UsingReadyRoom());
    }

    #region Photon Callbacks
    public void OnLeftRoom()
    {
        ShowMenu(false);
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    } 
    #endregion

    #region Getters
    private static UIReferences _instance;
    public static UIReferences Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<UIReferences>(); }
            return _instance;
        }
    }

    private PlayerUIBank _PlayerBank;
    public PlayerUIBank PlayerUI
    {
        get
        {
            if (_PlayerBank == null) { _PlayerBank = transform.GetComponentInChildren<PlayerUIBank>(true); }
            return _PlayerBank;
        }
    }
    #endregion
}