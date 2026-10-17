using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;

/// <summary>
/// 默认时间管理脚本，负责对局时间
/// 需要修改时，从 <see cref="MatchTimeManagerBase"/> 继承新脚本
/// </summary>
public class MatchTimeManager : MatchTimeManagerBase
{
    #region Public properties

    [SerializeField] private RoomTimeState m_timeState = RoomTimeState.None;
    public override RoomTimeState TimeState
    {
        get => m_timeState;
        set
        {
            m_timeState = value;
        }
    }

    public override int RoundDuration { get; set; }
    public override float CurrentTime { get; set; }

    public override bool IsInitialized 
    { 
        get; 
        set; 
    }

    public bool IsPaused
    {
        get;
        set;
    } = false;
    #endregion

    #region Private members
    private const string StartTimeKey = "RoomTime";
    private float m_serverTimeReference;
    private const int SECOND = 60;
    private bool roomClose = false;
    private bool isThisActive = true;
    private TextMeshProUGUI TimeText;
    private bool m_timeIsUp = false;
    private int finalCountDown = 10;
    private Action<RoundFinishCause> finishRoundCallback = null;
    #endregion

    protected override void Awake()
    {
        if (!PhotonNetworkWrapper.IsConnected && !GameConfig.Instance.offlineMode && !OfflineRoom.Instance.forceOffline)
        {
            UtilityHelper.LoadLevel(GameConfig.Instance.MainMenuScene);
            return;
        }
        base.Awake();
        if(TimeText == null) TimeText = UIReferences.Instance.PlayerUI.TimeText;
        IsPaused = false;
    }

    private void Start()
    {
        if (!PhotonNetworkWrapper.IsConnected)
            return;

        //默认只有主客户端会初始化，其他玩家加入后等主客户端同步对局信息。
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            Init();
        }
        else
        {
            RemoteInit();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitch;
        PhotonCallbacks.RoomPropertiesUpdate += OnRoomPropertiesChange;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitch;
        PhotonCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesChange;
    }

    public override void SetActive(bool active)
    {
        isThisActive = active;
    }

    /// <summary>
    /// 主客户端初始化房间时间
    /// </summary>
    public override void Init()
    {
        //如果对局正在等待最少玩家数才能开始
        if (GameController.Instance.GameMatchState == MatchState.Waiting && !isThisActive)
        {
            UIReferences.Instance.SetWaitingPlayersText(string.Format(GameTexts.WaitingPlayers, PhotonNetworkWrapper.PlayerList.Length, 2), true);
            return;
        }
        if (GameConfig.Instance.useCountDownOnStart)
        {
            CountDownBase.Instance.StartCountDown();
            return;
        }
        else
        {
            SetTimeState(RoomTimeState.Started, true);
        }
        GetTime(true);
    }

    public override void InitAfterWaiting()
    {
        GetTime(true);
        photonView.RPC(nameof(RpcStartTime), RpcTarget.AllBuffered, 3);
    }

    void RemoteInit()
    {
        if (TimeState == RoomTimeState.StartedAfterCountdown)
        {
            SetTimeState(RoomTimeState.Started);
            GetTime(false);
            return;
        }

        if (PhotonNetworkWrapper.CurrentRoom.CustomProperties.ContainsKey(PropertiesKeys.TimeState))
        {
            TimeState = (RoomTimeState)(int)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeState];
            if (TimeState == RoomTimeState.Started)
            {
                GetTime(false);
            }
            SetTimeState(TimeState);
        }
    }

    public override void InitAfterCountdown()
    {
        RoundDuration = (int)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        m_serverTimeReference = (float)PhotonNetworkWrapper.Time;
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            PhotonNetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
            SetTimeState(RoomTimeState.Started, true);
        }
        else
        {
            SetTimeState(RoomTimeState.StartedAfterCountdown);
            GetTime(false);
        }
        if (!IsInitialized) { GameController.Instance.SetGameState(MatchState.Playing); }
        IsInitialized = true;
        IsPaused = false;
    }

    /// <summary>
    /// 获取当前时间并校验是否正确
    /// </summary>
    public void GetTime(bool ResetReference)
    {
        if (PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            //从房间属性获取时长
            RoundDuration = (int)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        }

        if (PhotonNetworkWrapper.IsMasterClient)
        {
            if (ResetReference)//get the server time again?
            {
                m_serverTimeReference = (float)PhotonNetworkWrapper.Time;//get a reference time from the server

                Hashtable startTimeProp = new Hashtable();//create a property to store the reference time
                startTimeProp.Add(StartTimeKey, m_serverTimeReference);
                PhotonNetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);//send to the room hash tables so other clients can access to it
            }
        }
        else//Non master clients
        {
            if (PhotonNetworkWrapper.CurrentRoom.CustomProperties[StartTimeKey] != null)//if there's a reference time available
            {
                m_serverTimeReference = (float)PhotonNetworkWrapper.CurrentRoom.CustomProperties[StartTimeKey];//get it from the room hash tables
            }
        }
        if (!IsInitialized) { GameController.Instance.SetGameState(MatchState.Playing); }//is this the first round?
        IsInitialized = true;
    }

    public override void SetTimeState(RoomTimeState state, bool sync = false)
    {
        if (sync && PhotonNetworkWrapper.IsMasterClient)
        {
            Hashtable table = new Hashtable();
            table.Add(PropertiesKeys.TimeState, (int)state);
            PhotonNetworkWrapper.CurrentRoom.SetCustomProperties(table);
        }
        TimeState = state;
        if (TimeState == RoomTimeState.Started)
        {
            GameEvents.CallOnMatchStart();
        }
    }

    public override void OnUpdate()
    {
        UpdateTime();
        DisplayTime();
    }

    private void UpdateTime()
    {
        if (!IsInitialized || IsPaused || m_timeIsUp || !isThisActive)
            return;

        //根据参考时间和当前服务器时间计算秒数
        float seconds = RoundDuration - ((float)PhotonNetworkWrapper.Time - m_serverTimeReference);
        if (seconds > 0.0001f)
        {
            CurrentTime = seconds;
            //如果游戏即将结束，关闭房间使其不再显示在大厅列表
            if (CurrentTime <= 30 && !roomClose && PhotonNetworkWrapper.IsMasterClient)
            {
                roomClose = true;
                if (RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht)
                {
                }
            }
        }
        else if (seconds <= 0.001 && GetTimeServed == true)//Round Finished
        {
            CurrentTime = 0;
            FinishRound(RoundFinishCause.TimeUp);
        }
        else//if we cant get the server time yet
        {
            Refresh();
        }
    }

    public override void Pause(bool doPause)
    {
        IsPaused = doPause;
    }

    public override void RestartTime()
    {
        //从哈希表获取房间时长
        if (PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            RoundDuration = (int)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        }
        //因为所有人都已在房间内，大家会直接从服务器本地取得参考时间
        m_serverTimeReference = (float)PhotonNetworkWrapper.Time;
        //主客户端负责为后续加入的玩家保存参考时间
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            PhotonNetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
        }
    }

    public override void SetFinishRoundHandler(Action<RoundFinishCause> finishRoundHandler)
    {
        finishRoundCallback = finishRoundHandler;
    }

    /// <summary>
    /// 调用它以结束对局
    /// 如果房间设置为一局制 -> 会出现对局结算界面。
    /// </summary>
    public override void FinishRound(RoundFinishCause cause)
    {
        // 如果房间时间已到但倒计时还在跑，就不结束对局。
        if (TimeState == RoomTimeState.Countdown) return;
        
        if(finishRoundCallback != null && cause != RoundFinishCause.GameFinish)
        {
            // 在自定义回调里处理结束回合逻辑
            finishRoundCallback?.Invoke(cause);
            return;
        }

        if (!PhotonNetworkWrapper.IsConnected || !isThisActive)
            return;

        GameEvents.DispatchRoundEndEvent();
        Debug.Log($"Round Finish ({cause.ToString()}), mode: {RoomSettings.Instance.CurrentRoomInfo.roundStyle.ToString()}");
        if (!m_timeIsUp)
        {
            m_timeIsUp = true;
            GameController.Instance.OnGameTimeFinish(RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht || cause == RoundFinishCause.GameFinish);

            // 保存本局获得的经验值
            UIReferences.Instance.ResumeScreen.CollectData();

            finalCountDown = GameConfig.Instance.afterMatchCountdown;
            RoundFinishScreenBase.Instance?.SetCountdown(finalCountDown);
            InvokeRepeating(nameof(FinalCountdown), 1, 1);
            CrosshairBase.Instance.Show(false);
        }
    }

    void DisplayTime()
    {
        if (CurrentTime == 0 || !isThisActive) return;
        if (TimeText == null) return;

        TimeText.text = StringUtility.GetTimeFormat(Mathf.FloorToInt(CurrentTime / SECOND), Mathf.FloorToInt(CurrentTime % SECOND));
    }

    /// <summary>
    /// 用它修复了 Photon 时间延迟的问题
    /// </summary>
    void Refresh()
    {
        if (PhotonNetworkWrapper.CurrentRoom == null)
            return;

        if (PhotonNetworkWrapper.IsMasterClient)
        {
            m_serverTimeReference = (float)PhotonNetworkWrapper.Time;

            var startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            PhotonNetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
        }
        else
        {
            if (PhotonNetworkWrapper.CurrentRoom.CustomProperties.ContainsKey(StartTimeKey))
            {
                m_serverTimeReference = (float)PhotonNetworkWrapper.CurrentRoom.CustomProperties[StartTimeKey];
            }
        }
    }

    /// <summary>
    /// 游戏结束后离开房间前的倒计时
    /// </summary>
    void FinalCountdown()
    {
        finalCountDown--;
        RoundFinishScreenBase.Instance?.SetCountdown(finalCountDown);
        if (finalCountDown <= 0)
        {
            FinishGame();
            CancelInvoke(nameof(FinalCountdown));
            finalCountDown = GameConfig.Instance.afterMatchCountdown;
        }
    }

    public void FinishGame()
    {
        UtilityHelper.LockCursor(false);
        var roundStyle = RoomSettings.Instance.CurrentRoomInfo.roundStyle;
        if (roundStyle == RoundStyle.OneMacht || roundStyle == RoundStyle.RoundsOneMatch)
        {
            UIReferences.Instance.ResumeScreen.Show();
        }
        else if (roundStyle == RoundStyle.Rounds)
        {
            StartNewRound(new NewRoundOptions()
            {
                KeepData = false,
                RespawnPlayer = false
            });
        }
    }

    /// <summary>
    /// 开始新回合并保留玩家和队伍得分
    /// 时间会重置为初始值
    /// </summary>
    public override void StartNewRound(NewRoundOptions options)
    {
        if (GameConfig.Instance.useCountDownOnStart)
        {
            CountDownBase.Instance.StartCountDown();
        }
        else
        {
            GetTime(true);
            IsPaused = false;
        }

        if (!options.KeepData)
        {
            RoomSettings.Instance.ResetRoom();
            GameController.Instance.DestroyPlayer(true);
        }

        m_timeIsUp = false;
        GameController.Instance.GameFinish = false;
        GameController.Instance.IsLocalPlaying = false;
        UtilityHelper.LockCursor(false);
        KillCamBase.Instance.SetActive(false);
        finalCountDown = GameConfig.Instance.afterMatchCountdown;
        PhotonNetworkWrapper.CurrentRoom.IsOpen = true;
        PhotonNetworkWrapper.CurrentRoom.IsVisible = true;
        AIMananger.Instance?.RespawnAllBots(true);

        if (options.RespawnPlayer)
        {
            GameController.Instance.SpawnPlayer(PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam());
        }

        if (!options.KeepData)
        {
            UIReferences.Instance.ResetRound();

            if (RoomSettings.Instance.AutoTeamSelection)
                RoomSettings.Instance.CheckAutoSpawn();
        }
    }

    [PunRPC]
    void RpcStartTime(int wait)
    {
        if (GameCore.LocalPlayer.Team == Team.None) return;

        SetTimeState(RoomTimeState.Started, PhotonNetworkWrapper.IsMasterClient);
        UIReferences.Instance.SetWaitingPlayersText(GameTexts.StartingMatch, true);
        Invoke(nameof(StartTime), wait);
    }

    void StartTime()
    {
        if (GameConfig.Instance.useCountDownOnStart)
        {
            CountDownBase.Instance.StartCountDown();
        }
        else
        {
            GetTime(false);
        }
        IsPaused = false;
        UIReferences.Instance.SetWaitingPlayersText();
        GameController.Instance.SpawnPlayer(PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam());
    }

    public override void SetTimeTextRender(TextMeshProUGUI timeText, bool disableCurrent = true)
    {
        if (disableCurrent)
        {
            if (TimeText != null) TimeText.gameObject.SetActive(false);
            else UIReferences.Instance.PlayerUI.TimeText.gameObject.SetActive(false);                            
        }
        TimeText = timeText;
    }

    public override bool IsTimeUp() => m_timeIsUp;

    void OnMasterClientSwitch(Player newMaster)
    {
        if (newMaster.ActorNumber == PhotonNetworkWrapper.LocalPlayer.ActorNumber)
        {
            if (TimeState == RoomTimeState.Countdown)
            {
                CountDownBase.Instance.StartCountDown();
            }
        }
    }

    void OnRoomPropertiesChange(Hashtable props)
    {
        if (props.ContainsKey(StartTimeKey) && !PhotonNetworkWrapper.IsMasterClient)
        {
            RemoteInit();
        }
    }

    private bool GetTimeServed => Time.timeSinceLevelLoad > 7;
}