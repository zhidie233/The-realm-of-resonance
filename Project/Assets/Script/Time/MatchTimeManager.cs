using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;
/// 默认时间管理脚本，负责对局时间
/// 需要修改时，从 <see cref="MatchTimeManagerBase"/> 继承新脚本
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
        if (!NetworkWrapper.IsConnected && !GameConfig.Instance.directConnectMode && !SoloRoom.Instance.forceOffline)
        {
            CommonUtil.LoadLevel(GameConfig.Instance.MainMenuScene);
            return;
        }
        base.Awake();
        if(TimeText == null) TimeText = UIReferences.Instance.PlayerUI.TimeText;
        IsPaused = false;
    }

    private void Start()
    {
        if (!NetworkWrapper.IsConnected)
            return;
        //默认只有主客户端会初始化，其他玩家加入后等主客户端同步对局信息。
        if (NetworkWrapper.IsMasterClient)
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
        NetworkCallbacks.MasterClientSwitched += OnMasterClientSwitch;
        NetworkCallbacks.RoomPropertiesUpdate += OnRoomPropertiesChange;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        NetworkCallbacks.MasterClientSwitched -= OnMasterClientSwitch;
        NetworkCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesChange;
    }
    public override void SetActive(bool active)
    {
        isThisActive = active;
    }
    /// 主客户端初始化房间时间
    public override void Init()
    {
        //如果对局正在等待最少玩家数才能开始
        if (GameController.Instance.GameMatchState == MatchState.Waiting && !isThisActive)
        {
            UIReferences.Instance.SetReadyPlayersText(string.Format(GameTexts.ReadyPlayers, NetworkWrapper.PlayerList.Length, 2), true);
            return;
        }
        if (GameConfig.Instance.prematchCountdownEnabled)
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
        if (NetworkWrapper.CurrentRoom.CustomProperties.ContainsKey(NetworkKeys.TimeState))
        {
            TimeState = (RoomTimeState)(int)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.TimeState];
            if (TimeState == RoomTimeState.Started)
            {
                GetTime(false);
            }
            SetTimeState(TimeState);
        }
    }
    public override void InitAfterCountdown()
    {
        RoundDuration = (int)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.TimeRoomKey];
        m_serverTimeReference = (float)NetworkWrapper.Time;
        if (NetworkWrapper.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            NetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
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
    /// 获取当前时间并校验是否正确
    public void GetTime(bool ResetReference)
    {
        if (NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.TimeRoomKey] != null)
        {
            //从房间属性获取时长
            RoundDuration = (int)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.TimeRoomKey];
        }
        if (NetworkWrapper.IsMasterClient)
        {
            if (ResetReference)
            {
                m_serverTimeReference = (float)NetworkWrapper.Time;
                Hashtable startTimeProp = new Hashtable();
                startTimeProp.Add(StartTimeKey, m_serverTimeReference);
                NetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
            }
        }
        else
        {
            if (NetworkWrapper.CurrentRoom.CustomProperties[StartTimeKey] != null)
            {
                m_serverTimeReference = (float)NetworkWrapper.CurrentRoom.CustomProperties[StartTimeKey];
            }
        }
        if (!IsInitialized) { GameController.Instance.SetGameState(MatchState.Playing); }
        IsInitialized = true;
    }
    public override void SetTimeState(RoomTimeState state, bool sync = false)
    {
        if (sync && NetworkWrapper.IsMasterClient)
        {
            Hashtable table = new Hashtable();
            table.Add(NetworkKeys.TimeState, (int)state);
            NetworkWrapper.CurrentRoom.SetCustomProperties(table);
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
        float seconds = RoundDuration - ((float)NetworkWrapper.Time - m_serverTimeReference);
        if (seconds > 0.0001f)
        {
            CurrentTime = seconds;
            //如果游戏即将结束，关闭房间使其不再显示在大厅列表
            if (CurrentTime <= 30 && !roomClose && NetworkWrapper.IsMasterClient)
            {
                roomClose = true;
                if (RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht)
                {
                }
            }
        }
        else if (seconds <= 0.001 && GetTimeServed == true)
        {
            CurrentTime = 0;
            FinishRound(RoundFinishCause.TimeUp);
        }
        else
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
        if (NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.TimeRoomKey] != null)
        {
            RoundDuration = (int)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.TimeRoomKey];
        }
        //因为所有人都已在房间内，大家会直接从服务器本地取得参考时间
        m_serverTimeReference = (float)NetworkWrapper.Time;
        //主客户端负责为后续加入的玩家保存参考时间
        if (NetworkWrapper.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            NetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
        }
    }
    public override void SetFinishRoundHandler(Action<RoundFinishCause> finishRoundHandler)
    {
        finishRoundCallback = finishRoundHandler;
    }
    /// 调用它以结束对局
    /// 如果房间设置为一局制 -> 会出现对局结算界面。
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
        if (!NetworkWrapper.IsConnected || !isThisActive)
            return;
        GameEvents.DispatchRoundEndEvent();
        if (!m_timeIsUp)
        {
            m_timeIsUp = true;
            GameController.Instance.OnGameTimeFinish(RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht || cause == RoundFinishCause.GameFinish);
            // 保存本局获得的经验值
            UIReferences.Instance.ResumeScreen.CollectData();
            finalCountDown = GameConfig.Instance.endMatchDelay;
            RoundFinishScreenBase.Instance?.SetCountdown(finalCountDown);
            InvokeRepeating(nameof(FinalCountdown), 1, 1);
            ReticleBase.Instance.Show(false);
        }
    }
    void DisplayTime()
    {
        if (CurrentTime == 0 || !isThisActive) return;
        if (TimeText == null) return;
        TimeText.text = StringHelpers.GetTimeFormat(Mathf.FloorToInt(CurrentTime / SECOND), Mathf.FloorToInt(CurrentTime % SECOND));
    }
    /// 用它修复了 Photon 时间延迟的问题
    void Refresh()
    {
        if (NetworkWrapper.CurrentRoom == null)
            return;
        if (NetworkWrapper.IsMasterClient)
        {
            m_serverTimeReference = (float)NetworkWrapper.Time;
            var startTimeProp = new Hashtable();
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            NetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
        }
        else
        {
            if (NetworkWrapper.CurrentRoom.CustomProperties.ContainsKey(StartTimeKey))
            {
                m_serverTimeReference = (float)NetworkWrapper.CurrentRoom.CustomProperties[StartTimeKey];
            }
        }
    }
    /// 游戏结束后离开房间前的倒计时
    void FinalCountdown()
    {
        finalCountDown--;
        RoundFinishScreenBase.Instance?.SetCountdown(finalCountDown);
        if (finalCountDown <= 0)
        {
            FinishGame();
            CancelInvoke(nameof(FinalCountdown));
            finalCountDown = GameConfig.Instance.endMatchDelay;
        }
    }
    public void FinishGame()
    {
        CommonUtil.LockCursor(false);
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
    /// 开始新回合并保留玩家和队伍得分
    /// 时间会重置为初始值
    public override void StartNewRound(NewRoundOptions options)
    {
        if (GameConfig.Instance.prematchCountdownEnabled)
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
        CommonUtil.LockCursor(false);
        DeathCamBase.Instance.SetActive(false);
        finalCountDown = GameConfig.Instance.endMatchDelay;
        NetworkWrapper.CurrentRoom.IsOpen = true;
        NetworkWrapper.CurrentRoom.IsVisible = true;
        AIManager.Instance?.RespawnAllBots(true);
        if (options.RespawnPlayer)
        {
            GameController.Instance.SpawnPlayer(NetworkWrapper.LocalPlayer.GetPlayerTeam());
        }
        if (!options.KeepData)
        {
            UIReferences.Instance.ResetRound();
            if (RoomSettings.Instance.AutoAssignTeams)
                RoomSettings.Instance.CheckAutoSpawn();
        }
    }
    [PunRPC]
    void RpcStartTime(int wait)
    {
        if (GameCore.LocalPlayer.Team == Team.None) return;
        SetTimeState(RoomTimeState.Started, NetworkWrapper.IsMasterClient);
        UIReferences.Instance.SetReadyPlayersText(GameTexts.StartingMatch, true);
        Invoke(nameof(StartTime), wait);
    }
    void StartTime()
    {
        if (GameConfig.Instance.prematchCountdownEnabled)
        {
            CountDownBase.Instance.StartCountDown();
        }
        else
        {
            GetTime(false);
        }
        IsPaused = false;
        UIReferences.Instance.SetReadyPlayersText();
        GameController.Instance.SpawnPlayer(NetworkWrapper.LocalPlayer.GetPlayerTeam());
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
        if (newMaster.ActorNumber == NetworkWrapper.LocalPlayer.ActorNumber)
        {
            if (TimeState == RoomTimeState.Countdown)
            {
                CountDownBase.Instance.StartCountDown();
            }
        }
    }
    void OnRoomPropertiesChange(Hashtable props)
    {
        if (props.ContainsKey(StartTimeKey) && !NetworkWrapper.IsMasterClient)
        {
            RemoteInit();
        }
    }
    private bool GetTimeServed => Time.timeSinceLevelLoad > 7;
}
