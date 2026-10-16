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
public class MatchLobby : PhotonHelper, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks
{
    #region Public members
    [Header("Photon")]
    public SeverRegionCode DefaultServer = SeverRegionCode.usw;
    [GameToggle] public bool ShowPhotonStatistics;

    [Header("Room Options")]
    //Room Max Ping
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

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
#if ULSP
        if (ULoginUtility.RedirectToLoginIfNeeded()) return;
#endif
        SetupPhotonSettings();
        UtilityHelper.BlockCursorForUser = false;
        UtilityHelper.LockCursor(false);

        // Loading process
        StartCoroutine(InitialLoadProcess());

        if (AudioManager.Instance != null) { AudioManager.Instance.PlayBackground(); }
        LobbyUI.Instance.InitialSetup();
    }

    /// <summary>
    /// Setup Photon
    /// </summary>
    void SetupPhotonSettings()
    {
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetworkWrapper.OfflineMode = false;
        PhotonNetworkWrapper.IsMessageQueueRunning = true;
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = DefaultServer.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator InitialLoadProcess()
    {
        LobbyLoadingScreenBase.Instance
            .SetActive(true)
            .SetText(GameTexts.LoadingLocalContent.Localized(39));

        if (!GameConfig.isDataCached)
        {
            // Load GameData asynchronously
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

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConnectPhoton()
    {
        if (PhotonNetworkWrapper.IsConnected) return;

        connectionState = LobbyConnectionState.Connecting;
        PhotonNetwork.GameVersion = GameConfig.Instance.GameVersion;
        AuthenticationValues authValues = new AuthenticationValues(PhotonNetworkWrapper.NickName);

#if ULSP  
        if (DataBase.IsUserLogged && LoginProDataBase.Instance.usePhotonAuthentication)
        {
            authValues.AuthType = CustomAuthenticationType.Custom;
            authValues.AddAuthParameter("type", "0");
            authValues.AddAuthParameter("photonId", PhotonNetworkWrapper.NickName);
            authValues.AddAuthParameter("userId", DataBase.LocalLoggedUser.ID.ToString());
            authValues.AddAuthParameter("ip", DataBase.LocalLoggedUser.IP);
            if (LoginProDataBase.UseDeviceId())
            {
                authValues.AddAuthParameter("deviceId", SystemInfo.deviceUniqueIdentifier);
            }
        }
#endif

        PhotonNetwork.AuthValues = authValues;

        //if we don't have a custom region to connect or we are using a self hosted server
        if (ServerRegionID == -1 || !PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            //connect using the default PhotonServerSettings
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            //Change the cloud server region
            ConnectToServerRegion(ServerRegionID, true);
        }

        LobbyLoadingScreenBase.Instance
            .SetActive(true)
            .SetText(GameTexts.ConnectingToGameServer.Localized(40));
    }

    /// <summary>
    /// Change the server region (Photon PUN Only)
    /// </summary>
    public void ConnectToServerRegion(int id, bool initialConnection = false)
    {
        if (PhotonNetwork.IsConnected && !initialConnection)
        {
            // When the player manually change of region
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
            var code = SeverRegionCode.usw;
            if (id > 3) { id++; } // servers ids jumps from 3 to 5
            code = (SeverRegionCode)id;
            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            PhotonNetwork.ConnectToRegion(code.ToString());
            PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("preferredregion"), code.ToString());
        }
        else
        {
            Debug.LogWarning("Need your AppId for change server, please add it in PhotonServerSettings");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disconect()
    {
        SetLobbyChat(false);
        if (PhotonNetworkWrapper.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DelayDisconnect() => PhotonNetwork.Disconnect();

    /// <summary>
    /// This is called from the server every time the room list is updated.
    /// </summary>
    public void ServerList(List<RoomInfo> roomList)
    {
        LobbyRoomListBase.Instance.SetRoomList(roomList);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetNickName(string nick)
    {
        nick = nick.Replace("\n", "").Trim();
        if (GameConfig.Instance.verifySingleSession)
        {
            if (string.IsNullOrEmpty(PhotonNetworkWrapper.NickName))
            {
                // Session verification is required, which will be done after connect to the server.
                CachePlayerName = nick;
            }
            else PhotonNetworkWrapper.NickName = nick; // in case a nick name change happens, make sure to update it.
        }
        else PhotonNetworkWrapper.NickName = nick;

        if (rememberMe)
        {
            PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("remembernick"), nick);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetPlayerName()
    {
        if (string.IsNullOrEmpty(PhotonNetworkWrapper.NickName))
        {
#if ULSP
            // if the user has log in with an account
            if (DataBase.IsUserLogged)
            {
                SetNickName(DataBase.Instance.LocalUser.NickName);
                GameEvents.DispatchCoinUpdate(null);
                GoToMainMenu();
            }
            else
            {
                GeneratePlayerName();
            }
#else
            GeneratePlayerName();
#endif
        }
        else
        {
            GameEvents.DispatchCoinUpdate(null);
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GeneratePlayerName()
    {
        connectionState = LobbyConnectionState.WaitingForUserName;
        if (!rememberMe)
        {
            string storedNick = PropertiesKeys.GetUniqueKey("playername");
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
            // Assign the saved nick name automatically
            CachePlayerName = PlayerPrefs.GetString(PropertiesKeys.GetUniqueKey("remembernick"));
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

    /// <summary>
    /// 
    /// </summary>
    void GoToMainMenu()
    {
        if (!PhotonNetworkWrapper.IsConnected)
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

                if (string.IsNullOrEmpty(PhotonNetworkWrapper.NickName))
                {
                    Debug.LogError("Player nick name has not been authenticated!");
                }

                connectionState = LobbyConnectionState.Connected;
                if (!alreadyLoadHome) { LobbyUI.Instance.Home(); alreadyLoadHome = true; }
                SetLobbyChat(true);
                ResetValues();
                LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);

                // Fix: room list doesn't update when the player is already in the lobby
                autoRejoinLobby = true;
                PhotonNetwork.LeaveLobby();
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public void SetPlayerName(string InputName)
    {
        CachePlayerName = InputName;
        PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("playername"), CachePlayerName);
        SetNickName(CachePlayerName);
        ConnectPhoton();
    }

    /// <summary>
    /// The password of the special accounts
    /// See 'Game Staff' in the documentation for more information
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

    /// <summary>
    /// 
    /// </summary>
    public void AutoMatch()
    {
        if (isSeekingMatch)
            return;

        isSeekingMatch = true;
        StartCoroutine(DoSearch());
        IEnumerator DoSearch()
        {
            //active the search match UI
            LobbyUI.Instance.SeekingMatchUI.SetActive(true);
            // This is not actually needed :)
            yield return new WaitForSeconds(3);
            PhotonNetwork.JoinRandomRoom();
            isSeekingMatch = false;
            LobbyUI.Instance.SeekingMatchUI.SetActive(false);
        }
    }

    /// <summary>
    /// This function is called when hit Play button but there is not rooms to join in.
    /// </summary>
    public void OnNoRoomsToJoin(short returnCode, string message)
    {
        Debug.Log("No games to join found on matchmaking, creating one.");
        //create random room properties
        int scid = Random.Range(0, GameConfig.Instance.AllScenes.Count);
        var mapInfo = GameConfig.Instance.AllScenes[scid];

        var allModes = mapInfo.GetAllowedGameModes(GameModes);
        int modeRandom = Random.Range(0, allModes.Length);
        var gameMode = allModes[modeRandom];

        int maxPlayersRandom = Random.Range(0, gameMode.maxPlayers.Length);
        int timeRandom = Random.Range(0, gameMode.timeLimits.Length);
        int randomGoal = Random.Range(0, gameMode.GameGoalsOptions.Length);

        var roomInfo = new GameRoomInfo();
        roomInfo.roomName = string.Format("[PUBLIC] {0}{1}", PhotonNetworkWrapper.NickName.Substring(0, 2), Random.Range(0, 9999));
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
    /// Create a room with the settings specified in the Room Creator menu.
    /// </summary>
    public void CreateRoom()
    {
        var roomInfo = LobbyRoomCreator.Instance.BuildRoomInfo();
        CreateRoom(roomInfo);
    }

    /// <summary>
    /// Create and Join in a room with the given info.
    /// </summary>
    /// <param name="roomInfo"></param>
    public void CreateRoom(GameRoomInfo roomInfo)
    {
        SetLobbyChat(false);
        justCreatedRoomName = roomInfo.roomName;
        JoinedWithPassword = true;

        //Save Room properties for load in room
        var roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = roomInfo.time;
        roomOption[PropertiesKeys.GameModeKey] = roomInfo.gameMode.ToString();
        roomOption[PropertiesKeys.SceneNameKey] = roomInfo.sceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = roomInfo.roundStyle;
        roomOption[PropertiesKeys.TeamSelectionKey] = roomInfo.autoTeamSelection;
        roomOption[PropertiesKeys.CustomSceneName] = roomInfo.mapName;
        roomOption[PropertiesKeys.RoomGoal] = roomInfo.goal;
        roomOption[PropertiesKeys.RoomFriendlyFire] = roomInfo.friendlyFire;
        roomOption[PropertiesKeys.MaxPing] = roomInfo.maxPing;
        roomOption[PropertiesKeys.RoomPassword] = roomInfo.password;
        roomOption[PropertiesKeys.WithBotsKey] = roomInfo.withBots;

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

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("remembernick"), string.Empty);
        Disconect();
        LobbyUI.Instance.ChangeWindow("player name");
#if ULSP
        if (PhotonNetworkWrapper.Instance != null) PhotonNetworkWrapper.LocalPlayer.NickName = string.Empty;
        LoginProDataBase.Instance.SignOut();
#endif
    }

    #region UGUI
    public void SetRememberMe(bool value)
    {
        rememberMe = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckRoomPassword(RoomInfo room)
    {
        checkingRoom = room;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool SetRoomPassworld(string pass)
    {
        if (checkingRoom == null)
        {
            Debug.Log("Checking room is not assigned more!");
            return false;
        }

        if ((string)checkingRoom.CustomProperties[PropertiesKeys.RoomPassword] == pass && checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
        {
            JoinedWithPassword = true;
            if (PhotonNetwork.GetPing() < (int)checkingRoom.CustomProperties[PropertiesKeys.MaxPing])
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

    /// <summary>
    /// 
    /// </summary>
    public void LoadLocalLevel(string level)
    {
        LobbyUI.Instance.blackScreenFader.FadeIn(0.75f, () =>
         {
             UtilityHelper.LoadLevel(level);
         });
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        //Wait for check
        SetLobbyChat(false);
        if (AudioManager.Instance != null) { AudioManager.Instance.StopBackground(); }
        while (!PhotonNetworkWrapper.InRoom)
        {
            yield return null;
        }
        PhotonNetworkWrapper.IsMessageQueueRunning = false;
        UtilityHelper.LoadLevel((string)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.SceneNameKey]);
    }

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        quittingApp = true;
        GameConfig.isDataCached = false;
        Disconect();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetLobbyChat(bool connect)
    {
        if (LobbyChat.Instance == null) return;
        if (!GameConfig.Instance.UseLobbyChat) return;

        if (connect) { LobbyChat.Instance.Connect(GameConfig.Instance.GameVersion); }
        else { LobbyChat.Instance.Disconnect(); }
    }

    #region Photon Callbacks

    /// <summary>
    /// Called once the game successfully establish connection with the game server
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
    /// Called once the game successfully establish connection with the master server
    /// </summary>
    public void OnConnectedToMaster()
    {
        // Game intermediately connect to a 'common' lobby.
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master.");
    }

    /// <summary>
    /// Called when the player successfully connect to the common lobby
    /// </summary>
    public void OnJoinedLobby()
    {
        if (autoRejoinLobby)
        {
            autoRejoinLobby = false;
            return;
        }

        LobbyUI.Instance.Home();

        // Require session verification
        if (string.IsNullOrEmpty(PhotonNetworkWrapper.NickName))
        {
            connectionState = LobbyConnectionState.Authenticating;
            Debug.Log($"Player has joined to the lobby, now verifying session...");
            RequestSessionVerification();
            LobbyLoadingScreenBase.Instance.SetText(GameTexts.VerifyingSession.Localized(199));
        }
        else
        {
            connectionState = LobbyConnectionState.Connected;
            Debug.Log($"Player <b>{PhotonNetworkWrapper.LocalPlayer.NickName}</b> joined to the lobby, UserId: {PhotonNetworkWrapper.LocalPlayer.UserId}");
            LobbyLoadingScreenBase.Instance.SetActive(true).HideIn(2, true);
        }

        SetLobbyChat(true);
        ResetValues();
    }

    /// <summary>
    /// 
    /// </summary>
    private void RequestSessionVerification()
    {
        var user = new string[] { CachePlayerName };
        PhotonNetwork.FindFriends(user);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cause"></param>
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
                    PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetworkWrapper.NickName);
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

            // CustomAuthenticationFailed is handled in OnCustomAuthenticationFailed(...)
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
        //Create a random name for a future room that player create
        LobbyRoomCreatorUI.Instance.SetupSelectors();
        PhotonNetworkWrapper.IsMessageQueueRunning = true;
    }

    public void OnJoinedRoom()
    {
        if (!GameCore.GameData.UsingWaitingRoom())
        {
            Debug.Log($"Local client joined to the room '{PhotonNetworkWrapper.CurrentRoom.Name}'");
            StartCoroutine(MoveToGameScene());
        }
        else
        {
            SetLobbyChat(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.LogWarning($"Custom authentication response");
        foreach (var item in data)
        {
            Debug.Log($"CA Pair: {item.Key}: {item.Value}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="debugMessage"></param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="friendList"></param>
    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        if (string.IsNullOrEmpty(PhotonNetworkWrapper.NickName))
        {
            foreach (var player in friendList)
            {
                if (player.UserId == CachePlayerName && player.IsOnline)
                {
                    // Another session is open
                    Debug.Log("This account has a session open.");
                    LobbyUI.Instance.connectionPopupMessage.SetConfirmationText(GameTexts.Ok.ToUpper());
                    LobbyUI.Instance.connectionPopupMessage.AskConfirmation(GameTexts.AnotherSessionOpen.Localized(197), () =>
                    {
                        connectionState = LobbyConnectionState.Disconnected;
                        Disconect();
                        LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
#if !ULSP
                        LobbyUI.Instance.ChangeWindow("player name");
#else
                        SignOut();
#endif
                    });
                    return;
                }
            }

            // No session open for this account
            connectionState = LobbyConnectionState.Authenticating;
            PhotonNetworkWrapper.NickName = CachePlayerName;
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