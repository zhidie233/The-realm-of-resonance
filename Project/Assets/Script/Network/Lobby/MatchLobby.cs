using Game.Audio;
using Game.Internal.Structures;
using Game.Runtime.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-998)]
public class MatchLobby : NetworkHelper, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks
{
    #region Public members
    [Header("Photon")]
    public ServerRegionCode DefaultServer = ServerRegionCode.usw;
    [GameToggle] public bool ShowPhotonStatistics;

    [Header("Room Options")]
    //房间最大 Ping
    public int[] MaxPing = new int[] { 100, 200, 500, 1000 };
    #endregion

    #region Public properties
    public LobbyConnectionState connectionState { get; private set; } = LobbyConnectionState.Disconnected;
    public int CurrentMaxPing { get; set; }
    public int ServerRegionID { get; set; } = -1;
    public GameModeSettings[] GameModes { get; set; }
    public bool rememberMe { get; set; }
    public string justCreatedRoomName { get; set; }
    public string CachePlayerName { get; set; }
    public bool JoinedWithPassword { get; set; } = false;
    public Action onShowMenu;
    #endregion

    #region Private members
    private RoomInfo checkingRoom;
    private bool quittingApp = false;
    private bool isSeekingMatch = false;
    bool alreadyLoadHome = false;
    private bool autoRejoinLobby = false;
    #endregion

    void Awake()
    {
        SetupPhotonSettings();
        CommonUtil.BlockCursorForUser = false;
        CommonUtil.LockCursor(false);

        // 加载流程
        StartCoroutine(InitialLoadProcess());

        if (AudioManager.Instance != null) { AudioManager.Instance.PlayBackground(); }
        LobbyUI.Instance.InitialSetup();
    }

    /// <summary>
    /// 初始化 Photon
    /// </summary>
    void SetupPhotonSettings()
    {
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        NetworkWrapper.OfflineMode = false;
        NetworkWrapper.IsMessageQueueRunning = true;
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = DefaultServer.ToString();
    }

    IEnumerator InitialLoadProcess()
    {
        LobbyLoadingScreenBase.Instance
            .SetActive(true)
            .SetText(GameTexts.LoadingLocalContent.Localized(39));

        if (!GameConfig.isDataCached)
        {
            //异步加载 GameData
            yield return StartCoroutine(GameConfig.AsyncLoadData());
            yield return new WaitForEndOfFrame();
        }

        SetUpGameModes();
        LobbyUI.Instance.Setup();

        yield return LobbyUI.Instance.blackScreenFader.FadeOut(1);

        LobbyLoadingScreenBase.Instance
            .SetActive(true);

        GetPlayerName();
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void ConnectPhoton()
    {
        if (NetworkWrapper.IsConnected) return;

        connectionState = LobbyConnectionState.Connecting;
        PhotonNetwork.GameVersion = GameConfig.Instance.GameVersion;
        AuthenticationValues authValues = new AuthenticationValues(NetworkWrapper.NickName);


        PhotonNetwork.AuthValues = authValues;

        //如果没有自定义的连接区域，或者用的是自建服务器
        if (ServerRegionID == -1 || !PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            //使用默认的 PhotonServerSettings 连接
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            //切换云服务器区域
            ConnectToServerRegion(ServerRegionID, true);
        }

        LobbyLoadingScreenBase.Instance
            .SetActive(true)
            .SetText(GameTexts.ConnectingToGameServer.Localized(40));
    }

    /// <summary>
    /// 切换服务器区域（仅 Photon PUN）
    /// </summary>
    public void ConnectToServerRegion(int id, bool initialConnection = false)
    {
        if (PhotonNetwork.IsConnected && !initialConnection)
        {
            // 玩家手动切换区域时
            connectionState = LobbyConnectionState.ChangingRegion;
            LobbyLoadingScreenBase.Instance.SetActive(true)
           .SetText(GameTexts.ConnectingToGameServer.Localized(40));

            ServerRegionID = id;
            Invoke(nameof(DelayDisconnect), 0.2f);
            return;
        }

        if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            connectionState = LobbyConnectionState.Connecting;
            var code = ServerRegionCode.usw;
            if (id > 3) { id++; } // servers ids jumps from 3 to 5
            code = (ServerRegionCode)id;
            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            PhotonNetwork.ConnectToRegion(code.ToString());
            PlayerPrefs.SetString(NetworkKeys.GetUniqueKey("preferredregion"), code.ToString());
        }
        else
        {
            Debug.LogWarning("Need your AppId for change server, please add it in PhotonServerSettings");
        }
    }

    public void Disconect()
    {
        SetLobbyChat(false);
        if (NetworkWrapper.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    void DelayDisconnect() => PhotonNetwork.Disconnect();

    /// <summary>
    /// 服务器每次更新房间列表时都会调用
    /// </summary>
    public void ServerList(List<RoomInfo> roomList)
    {
        LobbyRoomListBase.Instance.SetRoomList(roomList);
    }

    public void SetNickName(string nick)
    {
        nick = nick.Replace("\n", "").Trim();
        if (GameConfig.Instance.verifySingleSession)
        {
            if (string.IsNullOrEmpty(NetworkWrapper.NickName))
            {
                // 需要会话验证，验证在连接服务器之后进行。
                CachePlayerName = nick;
            }
            else NetworkWrapper.NickName = nick; // in case a nick name change happens, make sure to update it.
        }
        else NetworkWrapper.NickName = nick;

        if (rememberMe)
        {
            PlayerPrefs.SetString(NetworkKeys.GetUniqueKey("remembernick"), nick);
        }
    }

    void GetPlayerName()
    {
        if (string.IsNullOrEmpty(NetworkWrapper.NickName))
        {
            GeneratePlayerName();
        }
        else
        {
            GameEvents.DispatchCoinUpdate(null);
            GoToMainMenu();
        }
    }

    void GeneratePlayerName()
    {
        connectionState = LobbyConnectionState.WaitingForUserName;
        if (!rememberMe)
        {
            string storedNick = NetworkKeys.GetUniqueKey("playername");
            if (!PlayerPrefs.HasKey(storedNick) || !GameConfig.Instance.RememberPlayerName)
            {
                CachePlayerName = string.Format(GameConfig.Instance.guestNameFormat, Random.Range(1, 9999));
            }
            else if (GameConfig.Instance.RememberPlayerName)
            {
                CachePlayerName = PlayerPrefs.GetString(storedNick, string.Format(GameConfig.Instance.guestNameFormat, Random.Range(1, 9999)));
            }
            LobbyUI.Instance.PlayerNameField.text = CachePlayerName;
            SetNickName(CachePlayerName);
            LobbyUI.Instance.ChangeWindow("player name");
            LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
        }
        else
        {
            //自动填入已保存的昵称
            CachePlayerName = PlayerPrefs.GetString(NetworkKeys.GetUniqueKey("remembernick"));
            if (string.IsNullOrEmpty(CachePlayerName))
            {
                rememberMe = false;
                GeneratePlayerName();
                return;
            }
            SetNickName(CachePlayerName);
            GoToMainMenu();
        }
    }

    void GoToMainMenu()
    {
        if (!NetworkWrapper.IsConnected)
        {
            ConnectPhoton();
        }
        else
        {
            if (!PhotonNetwork.InLobby)
            {
                if (PhotonNetwork.NetworkClientState != ClientState.JoiningLobby)
                {
                    PhotonNetwork.JoinLobby();
                }
            }
            else
            {

                if (string.IsNullOrEmpty(NetworkWrapper.NickName))
                {
                    Debug.LogError("Player nick name has not been authenticated!");
                }

                connectionState = LobbyConnectionState.Connected;
                if (!alreadyLoadHome) { LobbyUI.Instance.Home(); alreadyLoadHome = true; }
                SetLobbyChat(true);
                ResetValues();
                LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);

                //修复：玩家已在 Lobby 时房间列表不刷新
                autoRejoinLobby = true;
                PhotonNetwork.LeaveLobby();
            }
        }
    }

    public void SetPlayerName(string InputName)
    {
        CachePlayerName = InputName;
        PlayerPrefs.SetString(NetworkKeys.GetUniqueKey("playername"), CachePlayerName);
        SetNickName(CachePlayerName);
        ConnectPhoton();
    }

    /// <summary>
    /// 特殊账号的密码
    /// 更多信息见文档中的 'Game Staff'
    /// </summary>
    public bool EnterPassword(string password)
    {
        if (GameConfig.Instance.CheckPasswordUse(CachePlayerName, password))
        {
            SetNickName(CachePlayerName);
            ConnectPhoton();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AutoMatch()
    {
        if (isSeekingMatch)
            return;

        isSeekingMatch = true;
        StartCoroutine(DoSearch());
        IEnumerator DoSearch()
        {
            //激活搜索比赛 UI
            LobbyUI.Instance.SeekingMatchUI.SetActive(true);
            // 这里其实并不需要 :)
            yield return new WaitForSeconds(3);
            PhotonNetwork.JoinRandomRoom();
            isSeekingMatch = false;
            LobbyUI.Instance.SeekingMatchUI.SetActive(false);
        }
    }

    /// <summary>
    /// 点击 Play 按钮但当前没有可加入的房间时调用
    /// </summary>
    public void OnNoRoomsToJoin(short returnCode, string message)
    {
        Debug.Log("No games to join found on matchmaking, creating one.");
        //创建随机房间属性
        int scid = Random.Range(0, GameConfig.Instance.AllScenes.Count);
        var mapInfo = GameConfig.Instance.AllScenes[scid];

        var allModes = mapInfo.GetAllowedGameModes(GameModes);
        int modeRandom = Random.Range(0, allModes.Length);
        var gameMode = allModes[modeRandom];

        int maxPlayersRandom = Random.Range(0, gameMode.maxPlayers.Length);
        int timeRandom = Random.Range(0, gameMode.timeLimits.Length);
        int randomGoal = Random.Range(0, gameMode.GameGoalsOptions.Length);

        var roomInfo = new GameRoomInfo();
        roomInfo.roomName = string.Format("[PUBLIC] {0}{1}", NetworkWrapper.NickName.Substring(0, 2), Random.Range(0, 9999));
        roomInfo.gameMode = gameMode.gameMode;
        roomInfo.time = gameMode.timeLimits[timeRandom];
        roomInfo.sceneName = mapInfo.RealSceneName;
        roomInfo.roundStyle = gameMode.GetAllowedRoundMode();
        roomInfo.autoTeamSelection = gameMode.AutoTeamSelection;
        roomInfo.mapName = mapInfo.ShowName;
        roomInfo.goal = gameMode.GetGoalValue(randomGoal);
        roomInfo.friendlyFire = true;
        roomInfo.maxPing = MaxPing[CurrentMaxPing];
        roomInfo.password = string.Empty;
        roomInfo.withBots = gameMode.supportBots;
        roomInfo.maxPlayers = gameMode.maxPlayers[maxPlayersRandom];

        CreateRoom(roomInfo);
    }

    /// <summary>
    /// 使用 Room Creator 菜单中指定的设置创建房间
    /// </summary>
    public void CreateRoom()
    {
        var roomInfo = LobbyRoomCreator.Instance.BuildRoomInfo();
        CreateRoom(roomInfo);
    }

    /// <summary>
    /// 用给定信息创建并加入房间
    /// </summary>
    /// <param name="roomInfo"></param>
    public void CreateRoom(GameRoomInfo roomInfo)
    {
        SetLobbyChat(false);
        justCreatedRoomName = roomInfo.roomName;
        JoinedWithPassword = true;

        //保存房间属性，供进入房间时加载
        var roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[NetworkKeys.TimeRoomKey] = roomInfo.time;
        roomOption[NetworkKeys.GameModeKey] = roomInfo.gameMode.ToString();
        roomOption[NetworkKeys.SceneNameKey] = roomInfo.sceneName;
        roomOption[NetworkKeys.RoomRoundKey] = roomInfo.roundStyle;
        roomOption[NetworkKeys.TeamSelectionKey] = roomInfo.autoTeamSelection;
        roomOption[NetworkKeys.CustomSceneName] = roomInfo.mapName;
        roomOption[NetworkKeys.RoomGoal] = roomInfo.goal;
        roomOption[NetworkKeys.RoomFriendlyFire] = roomInfo.friendlyFire;
        roomOption[NetworkKeys.MaxPing] = roomInfo.maxPing;
        roomOption[NetworkKeys.RoomPassword] = roomInfo.password;
        roomOption[NetworkKeys.WithBotsKey] = roomInfo.withBots;

        PhotonNetwork.CreateRoom(roomInfo.roomName, new RoomOptions()
        {
            MaxPlayers = (byte)roomInfo.maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = roomOption.Keys.Select(x => x.ToString()).ToArray(),
            PublishUserId = true,
            EmptyRoomTtl = 0,
            BroadcastPropsChangeToAll = true,
        }, null);

        LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        if (AudioManager.Instance != null) { AudioManager.Instance.StopBackground(); }
    }

    public void SignOut()
    {
        PlayerPrefs.SetString(NetworkKeys.GetUniqueKey("remembernick"), string.Empty);
        Disconect();
        LobbyUI.Instance.ChangeWindow("player name");
    }

    #region UGUI
    public void SetRememberMe(bool value)
    {
        rememberMe = value;
    }

    public void CheckRoomPassword(RoomInfo room)
    {
        checkingRoom = room;
    }

    public bool SetRoomPassworld(string pass)
    {
        if (checkingRoom == null)
        {
            Debug.Log("Checking room is not assigned more!");
            return false;
        }

        if ((string)checkingRoom.CustomProperties[NetworkKeys.RoomPassword] == pass && checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
        {
            JoinedWithPassword = true;
            if (PhotonNetwork.GetPing() < (int)checkingRoom.CustomProperties[NetworkKeys.MaxPing])
            {
                LobbyUI.Instance.blackScreenFader.FadeIn(1);
                if (checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(checkingRoom.Name);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void LoadLocalLevel(string level)
    {
        LobbyUI.Instance.blackScreenFader.FadeIn(0.75f, () =>
         {
             CommonUtil.LoadLevel(level);
         });
    }
    #endregion

    private IEnumerator MoveToGameScene()
    {
        //等待检查
        SetLobbyChat(false);
        if (AudioManager.Instance != null) { AudioManager.Instance.StopBackground(); }
        while (!NetworkWrapper.InRoom)
        {
            yield return null;
        }
        NetworkWrapper.IsMessageQueueRunning = false;
        CommonUtil.LoadLevel((string)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.SceneNameKey]);
    }

    void SetUpGameModes()
    {
        var gameModes = new List<GameModeSettings>();
        for (int i = 0; i < GameConfig.Instance.gameModes.Count; i++)
        {
            if (GameConfig.Instance.gameModes[i].isEnabled)
            {
                gameModes.Add(GameConfig.Instance.gameModes[i]);
            }
        }
        GameModes = gameModes.ToArray();
    }

    private void OnApplicationQuit()
    {
        quittingApp = true;
        GameConfig.isDataCached = false;
        Disconect();
    }

    void SetLobbyChat(bool connect)
    {
        if (LobbyChat.Instance == null) return;
        if (!GameConfig.Instance.UseLobbyChat) return;

        if (connect) { LobbyChat.Instance.Connect(GameConfig.Instance.GameVersion); }
        else { LobbyChat.Instance.Disconnect(); }
    }

    #region Photon Callbacks

    /// <summary>
    /// 与游戏服务器连接成功后调用
    /// </summary>
    public void OnConnected()
    {
        if (PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            Debug.Log("Server connection established to: " + PhotonNetwork.CloudRegion);
        }
        else
        {
            Debug.Log($"Server connection established to: {PhotonNetwork.ServerAddress} ({PhotonNetwork.Server.ToString()})");
        }
    }

    /// <summary>
    /// 与主服务器连接成功后调用
    /// </summary>
    public void OnConnectedToMaster()
    {
        // 游戏会立即连接到一个 'common' lobby
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master.");
    }

    /// <summary>
    /// 玩家成功连接到公共 Lobby 时调用
    /// </summary>
    public void OnJoinedLobby()
    {
        if (autoRejoinLobby)
        {
            autoRejoinLobby = false;
            return;
        }

        LobbyUI.Instance.Home();

        // 需要会话验证
        if (string.IsNullOrEmpty(NetworkWrapper.NickName))
        {
            connectionState = LobbyConnectionState.Authenticating;
            Debug.Log($"Player has joined to the lobby, now verifying session...");
            RequestSessionVerification();
            LobbyLoadingScreenBase.Instance.SetText(GameTexts.VerifyingSession.Localized(199));
        }
        else
        {
            connectionState = LobbyConnectionState.Connected;
            Debug.Log($"Player <b>{NetworkWrapper.LocalPlayer.NickName}</b> joined to the lobby, UserId: {NetworkWrapper.LocalPlayer.UserId}");
            LobbyLoadingScreenBase.Instance.SetActive(true).HideIn(2, true);
        }

        SetLobbyChat(true);
        ResetValues();
    }

    private void RequestSessionVerification()
    {
        var user = new string[] { CachePlayerName };
        PhotonNetwork.FindFriends(user);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            if (quittingApp)
            {
                Debug.Log("Disconnect from Server!");
                return;
            }
            if (ServerRegionID == -1 && connectionState != LobbyConnectionState.Authenticating && PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
            {
                Debug.Log("Disconnect from cloud!");
            }
            else if (connectionState == LobbyConnectionState.ChangingRegion)
            {
                Debug.Log($"Changing server! region to: {ServerRegionID}");
                ConnectToServerRegion(ServerRegionID);
                LobbyRoomListBase.Instance?.ClearList();
                return;
            }
            else if (connectionState == LobbyConnectionState.Authenticating)
            {
                Debug.Log($"Authentication complete, finishing connection...");
                LobbyLoadingScreenBase.Instance.SetText(GameTexts.FinishingUp.Localized(198));
                if (PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
                {
                    PhotonNetwork.AuthValues = new AuthenticationValues(NetworkWrapper.NickName);
                    if (ServerRegionID == -1)
                    {
                        ConnectPhoton();
                    }
                    else
                    {
                        ConnectToServerRegion(ServerRegionID);
                    }
                }
                else // if photon server is used
                {
                    ConnectPhoton();
                }
                return;
            }
            else
            {
                Debug.Log("Disconnect from Server.");
            }
        }
        else
        {
            LobbyUI.Instance.blackScreenFader.FadeOut(1);
            LobbyUI.Instance.connectionPopupMessage.SetConfirmationText(GameTexts.TryAgain.ToUpper());

            // CustomAuthenticationFailed 在 OnCustomAuthenticationFailed(...) 中处理
            if (cause != DisconnectCause.CustomAuthenticationFailed)
            {
                LobbyUI.Instance.connectionPopupMessage.AskConfirmation(string.Format(GameTexts.DisconnectCause.Localized(41), cause.ToString()), () =>
                {
                    ConnectPhoton();
                    Debug.LogWarning("Failed to connect to server, cause: " + cause);
                });
            }
        }
        connectionState = LobbyConnectionState.Disconnected;
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list updated, total rooms: " + roomList.Count);
        ServerList(roomList);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void ResetValues()
    {
        if (LobbyRoomCreatorUI.Instance == null) { LobbyRoomCreatorUI.Instance = transform.GetComponentInChildren<LobbyRoomCreatorUI>(true); }
        //为玩家以后创建的房间生成一个随机名字
        LobbyRoomCreatorUI.Instance.SetupSelectors();
        NetworkWrapper.IsMessageQueueRunning = true;
    }

    public void OnJoinedRoom()
    {
        if (!GameCore.GameData.UsingWaitingRoom())
        {
            Debug.Log($"Local client joined to the room '{NetworkWrapper.CurrentRoom.Name}'");
            StartCoroutine(MoveToGameScene());
        }
        else
        {
            SetLobbyChat(false);
        }
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.LogWarning($"Custom authentication response");
        foreach (var item in data)
        {
            Debug.Log($"CA Pair: {item.Key}: {item.Value}");
        }
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        LobbyUI.Instance.connectionPopupMessage.AskConfirmation($"Custom Authentication Failed: {debugMessage}", () =>
        {
            ConnectPhoton();
        });

        Debug.LogWarning($"Custom Authentication Failed: {debugMessage}");
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    public void OnLeftLobby()
    {
        if (autoRejoinLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        if (string.IsNullOrEmpty(NetworkWrapper.NickName))
        {
            foreach (var player in friendList)
            {
                if (player.UserId == CachePlayerName && player.IsOnline)
                {
                    // 该账号已有其他会话打开
                    Debug.Log("This account has a session open.");
                    LobbyUI.Instance.connectionPopupMessage.SetConfirmationText(GameTexts.Ok.ToUpper());
                    LobbyUI.Instance.connectionPopupMessage.AskConfirmation(GameTexts.AnotherSessionOpen.Localized(197), () =>
                    {
                        connectionState = LobbyConnectionState.Disconnected;
                        Disconect();
                        LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
                        LobbyUI.Instance.ChangeWindow("player name");
                    });
                    return;
                }
            }

            // 该账号没有打开的会话
            connectionState = LobbyConnectionState.Authenticating;
            NetworkWrapper.NickName = CachePlayerName;
            GameEvents.DispatchCoinUpdate(null);
            Disconect();
        }
    }

    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        OnNoRoomsToJoin(returnCode, message);
    }

    public void OnLeftRoom()
    {
    }
    #endregion

    private static MatchLobby _instance;
    public static MatchLobby Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MatchLobby>();
            }
            return _instance;
        }
    }
}