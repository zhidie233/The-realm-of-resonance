using UnityEngine;
using System;

[DefaultExecutionOrder(-800)]
public class RoomMenu : MonoBehaviourBase
{
    public enum ControlFocus
    {
        None,
        Chat,
        InputBinding
    }

    [Range(0.0f, 5)]
    public float DelayLeave = 1.5f;

    /// <summary>
    /// event called when a player enter but have to wait until current round finish.
    /// </summary>
    public Action<Team> onWaitUntilRoundFinish;

    /// <summary>
    /// Is currently the cursor locked?
    /// </summary>
    public bool isCursorLocked
    {
        get;
        private set;
    }

    /// <summary>
    /// Return which part of the game is currently using the Input/Control/Keyboard
    /// </summary>
    public ControlFocus CurrentControlFocus
    {
        get;
        set;
    } = ControlFocus.None;

    /// <summary>
    /// Is currently the game showing the pause menu?
    /// </summary>
    public bool isPaused
    {
        get => PauseMenuBase.IsMenuOpen;
    }

    /// <summary>
    /// Is the game quitting?
    /// </summary>
    public bool isApplicationQuitting
    {
        get;
        private set;
    } = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!isConnected)
            return;

        base.Awake();
#if ULSP
        if (DataBase.IsUserLogged) { DataBase.Instance.RecordTime(); }
#endif
        UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
        InputHandler.CheckGamePadRequired();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        GameEvents.onLocalPlayerSpawn += OnPlayerSpawn;
        GameEvents.onLocalPlayerDeath += OnPlayerLocalDeath;
#if GameM
        TouchHelper.OnPause += TogglePause;
#endif
        PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        GameEvents.onLocalPlayerSpawn -= OnPlayerSpawn;
        GameEvents.onLocalPlayerDeath -= OnPlayerLocalDeath;
#if GameM
        TouchHelper.OnPause -= TogglePause;
#endif
        PhotonCallbacks.LeftRoom -= OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerSpawn()
    {
        UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = true;
        GameController.Instance.IsLocalPlaying = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerLocalDeath()
    {
        UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
        GameController.Instance.IsLocalPlaying = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        isCursorLocked = UtilityHelper.GetCursorState;
        PauseControll();
        ScoreboardInput();
    }

    /// <summary>
    /// Check the input for open/hide the pause menu
    /// </summary>
    void PauseControll()
    {

        // Force lock cursor when press left Alt + left mouse button
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetMouseButtonDown(0))
            {
                UtilityHelper.LockCursor(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UtilityHelper.LockCursor(false);
        }

        if (CurrentControlFocus == ControlFocus.InputBinding) return;

        if (GameInput.Pause()) TogglePause();
    }

    /// <summary>
    /// Toggle the current paused state
    /// </summary>
    public void TogglePause()
    {
        if (!GameController.Instance.FirstSpawnDone || GameController.Instance.GameFinish) return;

        bool paused = isPaused;
        paused = !paused;
        UIReferences.Instance.ShowMenu(paused);
        UtilityHelper.LockCursor(!paused);
        CrosshairBase.Instance?.Show(!paused);
        GameEvents.DispatchGamePauseEvent(paused);
    }

    /// <summary>
    /// 
    /// </summary>
    void ScoreboardInput()
    {
        if (GameController.Instance.GameFinish || PauseMenuBase.IsMenuOpen) return;

        if (GameInput.Scoreboard())
        {
            PauseMenuBase.Instance.SetActiveLayouts(PauseMenuBase.LayoutPart.Body);
            PauseMenuBase.Instance.OpenWindow("scoreboard");
        }
        else if (GameInput.Scoreboard(GameInputType.Up))
        {
            PauseMenuBase.Instance.CloseWindow("scoreboard");
        }
    }

    /// <summary>
    /// Event called when the player will be automatically assigned to a team
    /// </summary>
    public void OnAutoTeam()
    {
        UtilityHelper.LockCursor(true);
        GameController.Instance.IsLocalPlaying = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void JoinTeam(int id)
    {
        Team team = (Team)id;
        string tn = team.GetTeamName();
        string joinText = isOneTeamMode ? GameTexts.JoinedInMatch : GameTexts.JoinIn;

#if LOCALIZATION
        joinText = isOneTeamMode ? LocalizationTable.Instance.GetText(17) : LocalizationTable.Instance.GetText(23);
#endif
        if (isOneTeamMode)
        {
            KillFeedBase.Instance.SendMessageEvent(string.Format("{0} {1}", PhotonNetworkWrapper.NickName, joinText));
        }
        else
        {
            string jt = string.Format("{0} {1}", joinText, tn);
            KillFeedBase.Instance.SendTeamHighlightMessage(PhotonNetworkWrapper.NickName, jt, team);
        }
#if !PSELECTOR
        UtilityHelper.LockCursor(true);
#else
        if (!PlayerSelector.InMatch)
        {
            UtilityHelper.LockCursor(true);
        }
#endif
        //if player only spawn when a new round start
        if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn == GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish && GameController.Instance.GameMatchState == MatchState.Playing)
        {
            //subscribe to the start round event
            if (onWaitUntilRoundFinish != null) { onWaitUntilRoundFinish.Invoke(team); }
            GameController.Instance.SetLocalPlayerToTeam(team);//set the player to the selected team but not spawn yet.
            return;
        }
        //set the player to the selected team and spawn the player
        GameController.Instance.SpawnPlayer(team);
    }

    /// <summary>
    /// Leave the current room (if exist) and return to the lobby
    /// </summary>
    public void LeaveRoom(bool save = true)
    {
#if ULSP
        if (DataBase.IsUserLogged && save)
        {
            var p = PhotonNetworkWrapper.LocalPlayer;
            ULoginGame.SaveLocalPlayerKDS();
            DataBase.Instance.StopAndSaveTime();
        }
#endif
        //Good place to save info before reset statistics
        if (PhotonNetworkWrapper.IsConnected && PhotonNetworkWrapper.InRoom)
        {
            PhotonNetworkWrapper.CleanBuffer();
            PhotonNetworkWrapper.LeaveRoom();
        }
        else
        {
#if UNITY_EDITOR
            if (isApplicationQuitting) { return; }
#endif
            UtilityHelper.LoadLevel(GameConfig.Instance.MainMenuScene);
        }
    }

    /// <summary>
    /// Called from the server when the left room request was retrieved.
    /// </summary>
    public void OnLeftRoom()
    {
        Debug.Log("Local client left the room");
        RoomCameraBase.Instance?.SetActive(true);
        PhotonNetworkWrapper.IsMessageQueueRunning = false;
        MatchTimeManagerBase.Instance.enabled = false;
        if (UIReferences.Instance != null)
            StartCoroutine(UIReferences.Instance.FinalFade(true));
    }

    /// <summary>
    /// 
    /// </summary>
    void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private static RoomMenu _instance;
    public static RoomMenu Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<RoomMenu>(); }
            return _instance;
        }
    }
}