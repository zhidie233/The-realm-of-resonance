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
    /// 玩家进入但需等待当前回合结束时的事件。
    /// </summary>
    public Action<Team> onWaitUntilRoundFinish;

    /// <summary>
    /// 当前是否锁定光标？
    /// </summary>
    public bool isCursorLocked
    {
        get;
        private set;
    }

    /// <summary>
    /// 返回当前哪部分游戏正在使用输入/控制/键盘
    /// </summary>
    public ControlFocus CurrentControlFocus
    {
        get;
        set;
    } = ControlFocus.None;

    /// <summary>
    /// 游戏当前是否显示暂停菜单？
    /// </summary>
    public bool isPaused
    {
        get => PauseMenuBase.IsMenuOpen;
    }

    /// <summary>
    /// 游戏是否正在退出？
    /// </summary>
    public bool isApplicationQuitting
    {
        get;
        private set;
    } = false;

    protected override void Awake()
    {
        if (!isConnected)
            return;

        base.Awake();
        UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
        InputHandler.CheckGamePadRequired();
    }

    protected override void OnEnable()
    {
        GameEvents.onLocalPlayerSpawn += OnPlayerSpawn;
        GameEvents.onLocalPlayerDeath += OnPlayerLocalDeath;
        PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    protected override void OnDisable()
    {
        GameEvents.onLocalPlayerSpawn -= OnPlayerSpawn;
        GameEvents.onLocalPlayerDeath -= OnPlayerLocalDeath;
        PhotonCallbacks.LeftRoom -= OnLeftRoom;
    }

    void OnPlayerSpawn()
    {
        UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = true;
        GameController.Instance.IsLocalPlaying = true;
    }

    void OnPlayerLocalDeath()
    {
        UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
        GameController.Instance.IsLocalPlaying = false;
    }

    public override void OnUpdate()
    {
        isCursorLocked = UtilityHelper.GetCursorState;
        PauseControll();
        ScoreboardInput();
    }

    /// <summary>
    /// 检测打开/隐藏暂停菜单的输入
    /// </summary>
    void PauseControll()
    {

        // 按下左 Alt + 鼠标左键时强制锁定光标
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
    /// 切换当前暂停状态
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
    /// 玩家将被自动分配到队伍时触发
    /// </summary>
    public void OnAutoTeam()
    {
        UtilityHelper.LockCursor(true);
        GameController.Instance.IsLocalPlaying = true;
    }

    public void JoinTeam(int id)
    {
        Team team = (Team)id;
        string tn = team.GetTeamName();
        string joinText = isOneTeamMode ? GameTexts.JoinedInMatch : GameTexts.JoinIn;

        if (isOneTeamMode)
        {
            KillFeedBase.Instance.SendMessageEvent(string.Format("{0} {1}", PhotonNetworkWrapper.NickName, joinText));
        }
        else
        {
            string jt = string.Format("{0} {1}", joinText, tn);
            KillFeedBase.Instance.SendTeamHighlightMessage(PhotonNetworkWrapper.NickName, jt, team);
        }
        UtilityHelper.LockCursor(true);
        //若玩家只在新回合开始时出生
        if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn == GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish && GameController.Instance.GameMatchState == MatchState.Playing)
        {
            //订阅回合开始事件
            if (onWaitUntilRoundFinish != null) { onWaitUntilRoundFinish.Invoke(team); }
            GameController.Instance.SetLocalPlayerToTeam(team);//set the player to the selected team but not spawn yet.
            return;
        }
        //将玩家设为所选队伍并出生
        GameController.Instance.SpawnPlayer(team);
    }

    /// <summary>
    /// 离开当前房间（若存在）并返回大厅
    /// </summary>
    public void LeaveRoom(bool save = true)
    {
        //适合在重置统计数据前保存信息
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
    /// 服务器收到离开房间请求时调用。
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