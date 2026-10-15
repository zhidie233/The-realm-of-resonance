using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Game.Internal;
using Game.Runtime.UI;
using Game.Internal.Interfaces;
using Game.Internal.Structures;
using TMPro;
using Game.InputManager;

public class UIReferences : PhotonHelper, IInRoomCallbacks
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
    [SerializeField] private TextMeshProUGUI WaitingPlayersText = null;
    public CanvasGroupFader blackScreen;
    public ConfirmationWindow leaveRoomConfirmation;
    public GameObject[] addonReferences;

    private bool inTeam = false;
    private int ChangeTeamTimes = 0;
#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 126, 22, 38, 32, 33, 34, 127 };
    private string[] LocaleStrings;
#endif

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!PhotonNetworkWrapper.IsConnected || !PhotonNetworkWrapper.InRoom)
            return;

        PhotonNetwork.AddCallbackTarget(this);
        blackScreen.FadeOut(1);

        GetRoomInfo();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        SetUpUI();

        GameEvents.onLocalPlayerSpawn += OnPlayerSpawn;
        GameEvents.onPickUpHealth += OnPicUpMedKit;
        GameEvents.onLocalPlayerDeath += OnLocalPlayerDeath;
        GameEvents.onRoundEnd += OnRoundFinish;
        PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpJoinButtons(bool forced = false)
    {
        if (!inTeam && (CanSelectTeamToJoin() || forced))
        {
            playerScoreboards.SetActiveByTeamMode(true);
        }
        else { playerScoreboards.SetActiveJoinButtons(false); }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        GameEvents.onLocalPlayerSpawn -= OnPlayerSpawn;
        GameEvents.onPickUpHealth -= OnPicUpMedKit;
        GameEvents.onLocalPlayerDeath -= OnLocalPlayerDeath;
        GameEvents.onRoundEnd -= OnRoundFinish;
        PhotonCallbacks.LeftRoom -= OnLeftRoom;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerDeath()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundFinish()
    {
        playerScoreboards.SetActiveByTeamMode();
        StopAllCoroutines();
        blackScreen.SetAlpha(0);
    }

    /// <summary>
    /// 
    /// </summary>
    void GetRoomInfo()
    {
        GameMode mode = GetGameMode;
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name.ToUpper();
        GameModeText.text = mode.GetName().ToUpper();
        int vs = (!isOneTeamMode) ? PhotonNetwork.CurrentRoom.MaxPlayers / 2 : PhotonNetwork.CurrentRoom.MaxPlayers - 1;
        MaxPlayerText.text = (!isOneTeamMode) ? string.Format("{0} VS {1}", vs, vs) : string.Format("1 VS {0}", vs);       
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpUI()
    {
#if LOCALIZATION
        if (LocaleStrings == null) LocaleStrings = LocalizationTable.Instance.GetTextArray(LocaleTextIDs);
#endif
        if (!RoomSettings.Instance.CurrentRoomInfo.autoTeamSelection)
        {
            PauseMenuBase.Instance.OpenMenu();
        }
       
        playerScoreboards.gameObject.SetActive(true);
        SetActiveChangeTeamButton(false);
        PlayerUI.SpeakerIcon.SetActive(false);
        SetUpJoinButtons();
        UpdateDisplayUI();
        SuicideButton.SetActive(RoomSettings.Instance.canSuicide && GameController.Instance.IsLocalPlaying);

        if (PhotonNetworkWrapper.IsConnected)
        {
            if (MaxKillsText != null)
            {
                int MaxKills = (int)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.RoomGoal];
#if LOCALIZATION
                MaxKillsText.text = string.Format("{0} {1}", MaxKills, LocaleStrings[0]);
#else
            MaxKillsText.text = string.Format("{0} 击杀", MaxKills);
#endif
            }
        }
#if LMS
        if (GetGameMode == GameMode.BR) ShowMenu(false);
#endif
        if (GameCore.GameData.UsingWaitingRoom())
        {
            if (PhotonNetworkWrapper.OfflineMode && !PhotonNetworkWrapper.CurrentRoom.GetRoomInfo().autoTeamSelection) return;

            SetUpJoinButtons(true);
            SpectatorButton.SetActive(false);
            ShowMenu(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowMenu(bool active)
    {
        if (active) PauseMenuBase.Instance.OpenMenu();
        else PauseMenuBase.Instance.CloseMenu();

        SetActiveChangeTeamButton(GameConfig.Instance.CanChangeTeam && !isOneTeamMode && ChangeTeamTimes <= GameConfig.Instance.MaxChangeTeamTimes 
            && !RoomSettings.Instance.AutoTeamSelection && MatchTimeManagerBase.HaveTimeStarted() && SpectatorModeBase.Instance.CanSelectTeam());

        SuicideButton.SetActive(RoomSettings.Instance.canSuicide && GameController.Instance.IsLocalPlaying);

        if (!active)
        {
#if PSELECTOR
            if (PlayerSelector.Instance != null)
            {
                PlayerSelector.Instance.isChangeOfTeam = false;
            }
#endif
        }

        if (InputHandler.isGamePad) GamePadPointer.Instance?.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUIMask(RoomUILayers newMask)
    {
        UIMask = newMask;
        UpdateDisplayUI();
        GameEvents.DispatchUIMaskChange(newMask);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Resume()
    {
        if (!GameController.Instance.FirstSpawnDone) return;

        UtilityHelper.LockCursor(true);
        ShowMenu(false);
        CrosshairBase.Instance.Show(true);
        GameEvents.DispatchGamePauseEvent(false);
    }

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    public void AutoTeam(bool v)
    {
        AutoTeamUI.SetActive(v);
        if (!v)
        {
            PauseMenuBase.Instance.CloseMenu();
            SpectatorModeBase.Instance.SetActiveUI(false);
            SpectatorButton.SetActive(false);
            inTeam = true;
        }
        else
        {
            if (PhotonNetworkWrapper.OfflineMode)
            {
                AutoTeamUI.GetComponentInChildren<TextMeshProUGUI>().text = GameTexts.StartingOfflineRoom;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeftRoom(bool quit)
    {
        if (!quit)
        {
            UtilityHelper.LockCursor(false);
            leaveRoomConfirmation.AskConfirmation(GameTexts.LeaveMatchWarning.Localized("areusulega"), () => { LeftRoom(true); }, () => {  });
        }
        else
        {
            RoomMenu.Instance.LeaveRoom();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        if (!GameCore.LocalPlayer.Suicide())
        {
            GameEvents.DispatchGamePauseEvent(false);
        }
        UtilityHelper.LockCursor(true);
        ShowMenu(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void JoinTeam(int id)
    {
#if ELIM
        if (GetGameMode == GameMode.ELIM)
        {
            if (GameController.Instance.GameMatchState == MatchState.Playing)
            {
                Elimination.Instance.WaitForRoundFinish();
            }
        }
#endif
        RoomMenu.Instance.JoinTeam(id);
        PauseMenuBase.Instance.CloseMenu();
        inTeam = true;
        AutoTeamUI.SetActive(false);
        SpectatorModeBase.Instance.SetActiveUI(false);
        SpectatorButton.SetActive(false);

        if (GameController.Joined) { ChangeTeamTimes++; }
        if (GameConfig.Instance.CanChangeTeam && !isOneTeamMode && ChangeTeamTimes <= GameConfig.Instance.MaxChangeTeamTimes && MatchTimeManagerBase.HaveTimeStarted())
        {
            SetActiveChangeTeamButton(true);
        }
    }

    /// <summary>
    /// Called from the Change Team button
    /// </summary>
    public void ActiveChangeTeam()
    {
#if PSELECTOR
        if (PlayerSelector.Instance != null)
        {
            PlayerSelector.Instance.isChangeOfTeam = true;
        }
#endif
        playerScoreboards.SetActive(false, true);
        SetActiveChangeTeamButton(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveChangeTeamButton(bool active)
    {
        if (ChangeTeamButton == null) return;

        ChangeTeamButton.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void ShowScoreboard(bool active)
    {
        playerScoreboards.SetActive(active);
    }

    public void OnSpawnCount(int count)
    {
#if LOCALIZATION
        SpawnProtectionText.text = string.Format(LocaleStrings[6].ToUpper(), count);
#else
        SpawnProtectionText.text = string.Format("出生保护倒计时: <b>{0}</b>", count);
#endif
        SpawnProtectionText.gameObject.SetActive(count > 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnPlayerSpawn()
    {
        if (InputHandler.isGamePad)
        {
            GamePadPointer.Instance?.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t_amount"></param>
    void OnPicUpMedKit(int Amount) => new GameLocalNotification(string.Format("+{0} 血量", Amount));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="show"></param>
    public void SetWaitingPlayersText(string text = "", bool show = false)
    {
        WaitingPlayersText.text = text;
        WaitingPlayersText.transform.parent.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetRound()
    {
        RoundFinishScreenBase.Instance?.Hide();
        inTeam = false;
        blackScreen.FadeOut(1);
        SetUpUI();
        ShowMenu(true);
        if (!RoomSettings.Instance.AutoTeamSelection)
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
#if ULSP
            if (DataBase.Instance != null && DataBase.Instance.IsRunningTask)
            {
                while (DataBase.Instance.IsRunningTask) { yield return null; }
            }
#endif
            if (goToLobby)
            {
                UtilityHelper.LoadLevel(GameConfig.Instance.MainMenuScene);
            }
        }
        else
        {
            yield return new WaitForSeconds(delay);
            blackScreen.FadeOut(1);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool CanSelectTeamToJoin()
    {
        return !inTeam && (!RoomSettings.Instance.AutoTeamSelection || !GameCore.GameData.UsingWaitingRoom());
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