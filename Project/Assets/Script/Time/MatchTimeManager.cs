using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;

/// <summary>
/// Default Time Management script, handled the match time
/// For modifications inherited a new script from <see cref="MatchTimeManagerBase"/>
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

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (!PhotonNetworkWrapper.IsConnected)
            return;

        //only master client initialized by default, other players will wait until master sync match information after they join.
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            Init();
        }
        else
        {
            RemoteInit();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitch;
        PhotonCallbacks.RoomPropertiesUpdate += OnRoomPropertiesChange;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitch;
        PhotonCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesChange;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        isThisActive = active;
    }

    /// <summary>
    /// Master Client Initialize the room time
    /// </summary>
    public override void Init()
    {
        //if the match is waiting for a minimum amount of players to start
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

    /// <summary>
    /// 
    /// </summary>
    public override void InitAfterWaiting()
    {
        GetTime(true);
        photonView.RPC(nameof(RpcStartTime), RpcTarget.AllBuffered, 3);
    }

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
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
    /// get the current time and verify if it is correct
    /// </summary>
    public void GetTime(bool ResetReference)
    {
        if (PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            //get the time duration from the room properties 
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

    /// <summary>
    /// 
    /// </summary>
    public override void SetTimeState(RoomTimeState state, bool sync = false)
    {
        // Debug.Log($"time state changed from {TimeState.ToString()} to {state.ToString()}");
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

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        UpdateTime();
        DisplayTime();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateTime()
    {
        if (!IsInitialized || IsPaused || m_timeIsUp || !isThisActive)
            return;

        //calculate seconds from the reference time and the current server time
        float seconds = RoundDuration - ((float)PhotonNetworkWrapper.Time - m_serverTimeReference);
        if (seconds > 0.0001f)
        {
            CurrentTime = seconds;
            //if the game is about to finish, close the room so it will not be listed in the lobby anymore
            if (CurrentTime <= 30 && !roomClose && PhotonNetworkWrapper.IsMasterClient)
            {
                roomClose = true;
                if (RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht)
                {
                   // PhotonNetwork.CurrentRoom.IsOpen = false;
                   // PhotonNetwork.CurrentRoom.IsVisible = false;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doPause"></param>
    public override void Pause(bool doPause)
    {
        IsPaused = doPause;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void RestartTime()
    {
        //get the room time duration from hash tables
        if (PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            RoundDuration = (int)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        }
        //cause everyone is already in the room, all will get the reference locally from the server
        m_serverTimeReference = (float)PhotonNetworkWrapper.Time;
        //Master Client will take care of store the reference time for future players
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            PhotonNetworkWrapper.CurrentRoom.SetCustomProperties(startTimeProp);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="finishRoundHandler"></param>
    public override void SetFinishRoundHandler(Action<RoundFinishCause> finishRoundHandler)
    {
        finishRoundCallback = finishRoundHandler;
    }

    /// <summary>
    /// Call this in order to finish the match
    /// if the room is setup as one match room -> the game resume screen will appear.
    /// </summary>
    public override void FinishRound(RoundFinishCause cause)
    {
        // if the room time if finished but the countdown is still running, do not finish the match.
        if (TimeState == RoomTimeState.Countdown) return;
        
        if(finishRoundCallback != null && cause != RoundFinishCause.GameFinish)
        {
            // handle the finish round logic in the custom callback
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

            // Save the gained XP in this game
            UIReferences.Instance.ResumeScreen.CollectData();

            finalCountDown = GameConfig.Instance.afterMatchCountdown;
            RoundFinishScreenBase.Instance?.SetCountdown(finalCountDown);
            InvokeRepeating(nameof(FinalCountdown), 1, 1);
            CrosshairBase.Instance.Show(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DisplayTime()
    {
        if (CurrentTime == 0 || !isThisActive) return;
        if (TimeText == null) return;

        TimeText.text = StringUtility.GetTimeFormat(Mathf.FloorToInt(CurrentTime / SECOND), Mathf.FloorToInt(CurrentTime % SECOND));
    }

    /// <summary>
    /// with this fixed the problem of the time lag in the Photon
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
    /// Count down before leave the room after finish the game
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

    /// <summary>
    /// 
    /// </summary>
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
    /// Start a new round and keep the player and team scores
    /// The time will be reset to the initial value
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

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeText"></param>
    /// <param name="disableCurrent"></param>
    public override void SetTimeTextRender(TextMeshProUGUI timeText, bool disableCurrent = true)
    {
        if (disableCurrent)
        {
            if (TimeText != null) TimeText.gameObject.SetActive(false);
            else UIReferences.Instance.PlayerUI.TimeText.gameObject.SetActive(false);                            
        }
        TimeText = timeText;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsTimeUp() => m_timeIsUp;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newMaster"></param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="props"></param>
    void OnRoomPropertiesChange(Hashtable props)
    {
        if (props.ContainsKey(StartTimeKey) && !PhotonNetworkWrapper.IsMasterClient)
        {
            RemoteInit();
        }
    }

    private bool GetTimeServed => Time.timeSinceLevelLoad > 7;
}