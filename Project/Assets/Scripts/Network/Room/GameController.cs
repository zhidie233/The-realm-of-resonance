using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// 处理玩家生成、游戏模式选择及通用游戏工具函数
public class GameController : PhotonHelper, IInRoomCallbacks, IConnectionCallbacks
{
    // 当前游戏状态
    public MatchState GameMatchState;

    // Event called once all the players required to start have joined This is only called when GameData.JoinMethod is set to DirectToMap
    public Action onAllPlayersRequiredIn;

    // 所有已连接的真实玩家列表
    public List<Player> connectedPlayerList = new List<Player>();

    // List with references to all instanced players (bots and real players) in the scene. This list included all but the local player.
    public List<GamePlayer> OthersActorsInScene = new List<GamePlayer>();

    // 本地玩家 GamePlayer 数据
    public GamePlayer LocalActor { get; set; } = new GamePlayer();

    // 玩家等待生成时为 true
    public bool IsLocalWaitingForSpawn { get; set; }

    // 本地玩家本局爆头数
    public int Headshots { get; set; }

    // 当前比赛游戏模式主脚本
    public IGameMode GameModeLogic { get; private set; }

    // 游戏是否已结束
    public bool GameFinish { get; set; }

    // 本地玩家是否正在游戏中
    public bool IsLocalPlaying { get; set; }

    // 本地玩家游戏对象引用
    public new GameObject LocalPlayer { get; set; }

    // 本地玩家脚本引用
    public PlayerReferencesHandler LocalPlayerReferences { get; set; }

    // 覆盖 GameData 中玩家预制体
    public PrefabOverride OverridePlayerPrefab { get; set; }

    // The name of the player prefab with which the local player prefab spawned
    public string LastInstancedPlayerPrefabName { get; private set; }

    public static int LocalPlayerViewID = -1;
    public static int SuicideCount = 0;
    public static bool Joined = false;

    private int WaitingPlayersAmount = 1;
    private float StartPlayTime;
    private bool registered = false;
    // In case you need to filter the player spawn This function will be invoked right before the instanced the player prefab if the function return true, the player will not be spawned, otherwise it won't spawn and you will have to handled the instantiation in your custom function.
    private Queue<Func<GameObject, Vector3, Quaternion, Team, bool>> spawnFilters;

    void Awake()
    {
        CheckViewAllocation();
        if (!registered) { PhotonNetwork.AddCallbackTarget(this); registered = true; }

        PhotonNetwork.IsMessageQueueRunning = true;
        UtilityHelper.BlockCursorForUser = false;
        NamePlateBase.BlockDraw = false;
        Joined = false;
        SuicideCount = 0;
        StartPlayTime = Time.time;

        LocalActor.isRealPlayer = true;
        LocalActor.Name = PhotonNetwork.NickName;

        CrosshairBase.Instance.Show(false);
        if (GameCore.GameData.UsingWaitingRoom() && PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam() != Team.None)
        {
            Invoke(nameof(SpawnPlayerWithCurrentTeam), 2);
        }
        Debug.Log($"Game Started, Online: {!PhotonNetworkWrapper.OfflineMode} Master: {PhotonNetworkWrapper.IsMasterClient} State: {GameMatchState.ToString()} TimeState: {MatchTimeManagerBase.Instance.TimeState.ToString()}");
    }

    void Start()
    {
        if (GameModeLogic == null)// check on start because game mode should be assigned on awake
        {
            Debug.LogWarning("No Game Mode has been assigned yet!");
        }
    }

    private void OnEnable()
    {
        GameEvents.onRemoteActorChange += OnRemoteActorChange;
    }

    private void OnDisable()
    {
        GameEvents.onRemoteActorChange -= OnRemoteActorChange;
        if (registered)
            PhotonNetwork.RemoveCallbackTarget(this);
        if (GameModeLogic != null)
        {
            PhotonCallbacks.PlayerEnteredRoom -= GameModeLogic.OnOtherPlayerEnter;
            PhotonCallbacks.RoomPropertiesUpdate -= GameModeLogic.OnRoomPropertiesUpdate;
            PhotonCallbacks.PlayerLeftRoom -= GameModeLogic.OnOtherPlayerLeave;
            GameEvents.onLocalPlayerDeath -= GameModeLogic.OnLocalPlayerDeath;
        }
    }

    // Spawn the local player in the give team If a player instanced already exist, it will be automatically destroyed and replaced with a new one.
    // team in which the player will spawn
    // <returns></returns>
    public bool SpawnPlayer(Team playerTeam)
    {
        if (IsLocalWaitingForSpawn) return false;//there's a reserved spawn incoming, don't spawn before that

        //if there is a local player already instance
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);
        }

        //if the game finish
        if (GameFinish)
        {
            RoomCameraBase.Instance?.SetActive(false);
            return false;
        }

        //set the player team to the player properties
        var PlayerTeam = new Hashtable
        {
            { PropertiesKeys.TeamKey, playerTeam.ToString() }
        };
        PhotonNetworkWrapper.LocalPlayer.SetCustomProperties(PlayerTeam);
        GameCore.LocalPlayer.Team = playerTeam;

        //spawn the player model
#if !PSELECTOR
        SpawnPlayerModel(playerTeam);
#else
        if (PlayerSelector.InMatch)
        {
            if(PlayerSelector.Instance == null)
            {
                Debug.LogWarning("Player Selector is enabled but is not integrated in this map.");
                return false;
            }
            if (!PlayerSelector.Instance.TrySpawnSelectedPlayer(playerTeam)) return false;
        }
        else
        {
            PlayerSelector.SpawnPreSelectedPlayer(playerTeam);
        }
#endif
        return true;
    }

    // Spawn player based in the team
    // <returns></returns>
    public void SpawnPlayerModel(Team playerTeam)
    {
        Vector3 pos;
        Quaternion rot;
        SpawnPointManager.Instance.GetPlayerSpawnPosition(playerTeam, out pos, out rot);

        GameObject playerPrefab = GameConfig.Instance.Player1.gameObject;
        if (OverridePlayerPrefab == null)
        {
            if (playerTeam == Team.Team2) playerPrefab = GameConfig.Instance.Player2.gameObject;
        }
        else
        {
            playerPrefab = OverridePlayerPrefab.GetPlayerForTeam(playerTeam);
        }

        if (!InstancePlayer(playerPrefab, pos, rot, playerTeam))
        {
            // if the player was not instanced
            return;
        }

        OnPostSpawn();
    }

    // Spawn the local player in the given position and rotation.
    // Player prefab
    // <param name="position"></param>
    // <param name="rotation"></param>
    // <param name="team"></param>
    // 返回: Succeed instancing the player.
    public bool InstancePlayer(GameObject prefab, Vector3 position, Quaternion rotation, Team team)
    {
        if (!PhotonNetworkWrapper.InRoom) return false;

        // Check if there're spawn filters
        if (VerifySpawnFilter(prefab, position, rotation, team, out bool filterResult))
        {
            // if there's a filter and it returns True
            if (filterResult)
            {
                // That means we can spawn the player, so lets start this function from the begin (to check any other pending filter)
                InstancePlayer(prefab, position, rotation, team);
            }
            // if the filter returns False, this mean the spawn in canceled here and therefore
            // the spawn will be handle elsewhere (by the filter)
            else return false;
        }

        //set the some common data that will be sync right after the player is instanced in the other clients
        var commonData = new object[1];
        commonData[0] = team;

        LastInstancedPlayerPrefabName = prefab.name;

        //instantiate the player prefab
        LocalPlayer = PhotonNetwork.Instantiate(prefab.name, position, rotation, 0, commonData);

        LocalPlayerReferences = LocalPlayer.GetComponent<PlayerReferencesHandler>();
        LocalActor.Actor = LocalPlayer.transform;
        LocalActor.ActorView = LocalPlayer.GetComponent<PhotonView>();
        LocalActor.Team = team;
        LocalActor.AimPosition = LocalPlayer.GetComponent<PlayerConfig>().AimPositionReference.transform;
        RegisterFirstPersonController();
        if (!Joined)
        {
            Debug.Log($"Spawned with the player prefab: {prefab.name}");
            
        }

        GameEvents.DispatchPlayerLocalSpawnEvent();
        return true;
    }

    public void RegisterFirstPersonController()
    {
        Debug.Log("Registering first person controller");
        GravityManager gravityManager = GetComponentInChildren<GravityManager>();
        gravityManager.SetBlFirstPersonController(LocalPlayer.GetComponent<FirstPersonController>());
        gravityManager.UpdateEffect();
    }

    // Check if there's any spawn filter pending
    // 返回: True if a filter exist, false if not filters availables
    public static bool VerifySpawnFilter(GameObject prefab, Vector3 position, Quaternion rotation, Team team, out bool filterResult)
    {
        filterResult = false;
        if (Instance.spawnFilters == null || Instance.spawnFilters.Count == 0) return false;

        var filter = Instance.spawnFilters.Dequeue();
        filterResult = filter.Invoke(prefab, position, rotation, team);
        return true;
    }

    // <param name="filter"></param>
    public static void AddSpawnFilter(Func<GameObject, Vector3, Quaternion, Team, bool> filter)
    {
        if (Instance == null) return;
        if (Instance.spawnFilters == null) Instance.spawnFilters = new Queue<Func<GameObject, Vector3, Quaternion, Team, bool>>();
        Instance.spawnFilters.Enqueue(filter);
    }

    // Assign a player to a team but not instance it
    public void SetLocalPlayerToTeam(Team team)
    {
        Hashtable PlayerTeam = new Hashtable
        {
            { PropertiesKeys.TeamKey, team.ToString() }
        };
        PhotonNetworkWrapper.LocalPlayer.SetCustomProperties(PlayerTeam, null);
        GameCore.LocalPlayer.Team = team;
        Joined = true;
    }

    // Instance a Player only if already has been instanced and is alive
    public void SpawnPlayerIfAlreadyInstanced()
    {
        if (LocalPlayer == null)
            return;

        Team t = PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam();
        SpawnPlayer(t);
    }

    // Respawn the local player after the respawn time delay
    // define a custom delay or -1 to use the GameData respawn fixed delay
    public void RespawnLocalPlayerAfter(float respawnTime = -1, bool doFadeIn = true)
    {
        if (respawnTime < 0) respawnTime = GameConfig.Instance.PlayerRespawnTime;

        StartCoroutine(DoWait());
        IEnumerator DoWait()
        {
            float fadeDuration = Mathf.Min(0.4f, respawnTime / 3);
            float wait = Mathf.Max(0.5f, (respawnTime - fadeDuration) - 0.5f);

            yield return new WaitForSeconds(wait);
            NamePlateBase.BlockDraw = true;
            if (!GameFinish && doFadeIn)
            {
                UIReferences.Instance.blackScreen.FadeIn(fadeDuration);
            }
            yield return new WaitForSeconds(fadeDuration);
            if (SpawnPlayer(PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam()))
            {
                KillCamBase.Instance.SetActive(false);
            }
            UIReferences.Instance.blackScreen.FadeOut(fadeDuration);
            NamePlateBase.BlockDraw = false;
        }
    }

    // Called after each player spawn.
    public static void OnPostSpawn(bool skipFade = false)
    {
        if (Instance == null) return;

        Instance.AfterSpawnSetup(skipFade);

        if (!Instance.FirstSpawnDone && MatchInformationDisplay.Instance != null) { MatchInformationDisplay.Instance.DisplayInfo(); }
        Instance.FirstSpawnDone = true;
        CrosshairBase.Instance.Show(true);
    }

    // If Player exist, them destroy
    public void DestroyPlayer(bool ActiveCamera)
    {
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);
        }
        RoomCameraBase.Instance?.SetActive(ActiveCamera);
    }

    // Called when the room round time finish
    public void OnGameTimeFinish(bool gameOver)
    {
        GameFinish = true;
        if (GameModeLogic != null) { GameModeLogic.OnFinishTime(gameOver); }
        SetGameState(MatchState.Finishing);
    }

    // Make the local player spawn in a random spawn point of the current team
    public void SpawnPlayerWithCurrentTeam()
    {
        if (SpawnPlayer(PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam()))
            RoomMenu.Instance.OnAutoTeam();
    }

    // Called after the local player spawn
    public void AfterSpawnSetup(bool skipFade = false)
    {
        RoomCameraBase.Instance?.SetActive(false);
        if (!skipFade) StartCoroutine(UIReferences.Instance.FinalFade(false, false, 0));
        UtilityHelper.LockCursor(true);
        if (!Joined) { StartPlayTime = Time.time; }
        Joined = true;
    }

    public bool IsGameMode(GameMode mode, IGameMode logic)
    {
        bool isIt = GetGameMode == mode;
        if (isIt)
        {
            if (GameModeLogic != null)
            {
                Debug.LogError("A GameMode has been assigned before, only 1 game mode can be assigned per match.");
                return false;
            }
            GameModeLogic = logic;
            PhotonCallbacks.PlayerEnteredRoom += logic.OnOtherPlayerEnter;
            PhotonCallbacks.RoomPropertiesUpdate += logic.OnRoomPropertiesUpdate;
            PhotonCallbacks.PlayerLeftRoom += logic.OnOtherPlayerLeave;
            GameEvents.onLocalPlayerDeath += GameModeLogic.OnLocalPlayerDeath;
            Debug.Log("Game Mode: " + mode.GetName());
        }
        return isIt;
    }

    // Should called when the local player score in game
    public void SetPointFromLocalPlayer(int points, GameMode mode)
    {
        if (GameModeLogic == null) return;
        if (mode != GetGameMode) return;

        GameModeLogic.OnLocalPoint(points, PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam());
    }

    // Should called when a non player object score in game, ex: AI This should be called by master client only
    public void SetPoint(int points, GameMode mode, Team team)
    {
        if (GameModeLogic == null) return;
        if (mode != GetGameMode) return;

        GameModeLogic.OnLocalPoint(points, team);
    }

    public void OnLocalPlayerKill()
    {
        if (GameModeLogic == null) return;

        GameModeLogic.OnLocalPlayerKill();
    }

    // <returns></returns>
    public bool isLocalPlayerWinner()
    {
        if (GameModeLogic == null) return false;
        return GameModeLogic.isLocalPlayerWinner;
    }

    public bool WaitForPlayers(int MinPlayers)
    {
        if (MinPlayers > 1)
        {
            if (isOneTeamMode)
            {
                if (PhotonNetwork.PlayerList.Length >= MinPlayers) return false;
            }
            else
            {
                if (PhotonNetwork.PlayerList.Length >= MinPlayers)
                {
                    if (PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length > 0 && PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length > 0)
                    {
                        if (onAllPlayersRequiredIn != null) { onAllPlayersRequiredIn.Invoke(); }
                        return false;
                    }
                }
            }
        }
        WaitingPlayersAmount = MinPlayers;
        SetGameState(MatchState.Waiting);
        return true;
    }

    // This is a event callback here is caching all 'actors' in the scene (players and bots)
    public void OnRemoteActorChange(GameEvents.PlayerChangeData data)
    {
        int id = OthersActorsInScene.FindIndex(x => x.Name == data.PlayerName);
        if (id != -1)
        {
            if (data.IsAlive)
            {
                if (data.GameActor != null)
                {
                    OthersActorsInScene[id] = data.GameActor;
                }
                else
                {
                    OthersActorsInScene[id].isAlive = data.IsAlive;
                }
                OthersActorsInScene[id].ActorView = data.NetworkView;
            }
            else
            {
                if (OthersActorsInScene[id].Actor == null)
                {
                    OthersActorsInScene[id].isAlive = false;
                }
            }
        }
        else
        {
            if (data.IsAlive)
            {
                if (data.GameActor == null) { Debug.LogWarning($"Actor data for {data.PlayerName} has not been build yet."); return; }
                if (data.GameActor.ActorView == null) { data.GameActor.ActorView = data.GameActor.Actor?.GetComponent<PhotonView>(); }
                OthersActorsInScene.Add(data.GameActor);
            }
        }
    }

    // Add a new player info in the <see cref="OthersActorsInScene"/> list
    // The new player to register
    // Replace the information if record already exist for this player
    public void RegisterGamePlayer(GamePlayer newPlayer, bool force = false)
    {
        if (OthersActorsInScene.Exists(x => x.Name == newPlayer.Name) && !force) return;

        int index = OthersActorsInScene.FindIndex(x => x.Name == newPlayer.Name);

        if (index == -1)
        {
            OthersActorsInScene.Add(newPlayer);
        }
        else
        {
            OthersActorsInScene[index] = newPlayer;
        }
    }

    // Find a player or bot by their PhotonView ID
    // <returns></returns>
    public Transform FindActor(int ViewID)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.ViewID == ViewID)
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().ViewID == ViewID) { return LocalPlayer.transform; }
        return null;
    }

    // Find a player or bot by their PhotonPlayer
    // <returns></returns>
    public Transform FindActor(Player player)
    {
        if (player == null) return null;
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.Owner != null && OthersActorsInScene[i].ActorView.Owner.ActorNumber == player.ActorNumber)
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().Owner.ActorNumber == player.ActorNumber) { return LocalPlayer.transform; }
        return null;
    }

    // Find a player or bot by their PhotonPlayer
    // <returns></returns>
    public GamePlayer FindActor(string actorName)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].Actor.name == actorName)
            {
                return OthersActorsInScene[i];
            }
        }
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().Owner.NickName == actorName) { return LocalActor; }
        return null;
    }

    // Find a player or bot by their ViewID
    // <returns></returns>
    public GamePlayer GetGameActor(int viewID)
    {
        if (LocalActor.ActorViewID == viewID) { return LocalActor; }
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorViewID == viewID)
            {
                return OthersActorsInScene[i];
            }
        }
        return null;
    }

    public GamePlayer GetGamePlayer(string nickName)
    {
        GamePlayer player = OthersActorsInScene.Find(x => x.Name == nickName);
        if (player == null && nickName == LocalName)
        {
            player = LocalActor;
        }
        return player;
    }

    // Get the list of Game players that are joined in the given team
    // Team from where the players have to be part of.
    // Fetch only players that has been registered in the <see cref="OthersActorsInScene"/> list.
    // <returns></returns>
    public GamePlayer[] GetGamePlayerInTeam(Team team, bool registeredActorsOnly = true)
    {
        List<GamePlayer> list = new List<GamePlayer>();
        if (registeredActorsOnly)
        {
            for (int i = 0; i < OthersActorsInScene.Count; i++)
            {
                if (OthersActorsInScene[i].Team == team)
                {
                    list.Add(OthersActorsInScene[i]);
                }
            }
            if (LocalActor.Team == team) list.Add(LocalActor);
        }
        else
        {

        }
        return list.ToArray();
    }

    // Get the list of Game actors that are not in the same team of the local player
    // <param name="includeBots"></param>
    // <returns></returns>
    public List<GamePlayer> GetNonTeamMatePlayers(bool includeBots = true)
    {
        Team playerTeam = PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam();
        List<GamePlayer> list = new List<GamePlayer>();
        bool oneTeamMode = isOneTeamMode;
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (oneTeamMode)
            {
                if (OthersActorsInScene[i].Team == Team.All && OthersActorsInScene[i].Actor != LocalActor.Actor)
                {
                    list.Add(OthersActorsInScene[i]);
                }
            }
            else
            {
                if (OthersActorsInScene[i].Team != playerTeam)
                {
                    if (OthersActorsInScene[i].isRealPlayer) { list.Add(OthersActorsInScene[i]); }
                    else if (includeBots) { list.Add(OthersActorsInScene[i]); }
                }
            }
        }
        return list;
    }

    [PunRPC]
    void RPCSyncGame(MatchState state)
    {
        Debug.Log("Game sync by master, match state: " + state.ToString());
        GameMatchState = state;
        if (!PhotonNetworkWrapper.IsMasterClient)
        {
            MatchTimeManagerBase.Instance.Init();
        }
    }

    // <param name="state"></param>
    public void SetGameState(MatchState state)
    {
        if (!PhotonNetworkWrapper.IsMasterClient)
        {
            return;
        }
        else
        {
            if (state == MatchState.Starting)
            {
                // Unlist the room if the game mode is set to unlist after started
                if (GetGameMode.GetGameModeInfo().UnlistGameAfterStarted)
                {
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            }
        }

        CheckViewAllocation();
        PhotonNetwork.RegisterPhotonView(photonView);

        photonView.RPC(nameof(RPCMatchState), RpcTarget.All, state);
    }

    [PunRPC]
    void RPCMatchState(MatchState state)
    {
        GameMatchState = state;
        GameEvents.DispatchMatchStateChange(state);
        CheckPlayersInMatch();
    }

    // Check the minimum required players are meet
    void CheckPlayersInMatch()
    {
        //if still waiting
        if (!MatchTimeManagerBase.Instance.IsInitialized && GameMatchState == MatchState.Waiting)
        {
            bool ready = false;
            if (isOneTeamMode)
            {
                ready = PhotonNetwork.PlayerList.Length >= WaitingPlayersAmount;
            }
            else
            {
                int team1Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
                int team2Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
                int totalPlayers = PhotonNetwork.PlayerList.Length;

                if (AIMananger.Instance.BotsActive)
                {
                    totalPlayers += AIMananger.Instance.BotsStatistics.Count;
                    team1Count += AIMananger.Instance.GetAllBotsInTeam(Team.Team1).Count;
                    team2Count += AIMananger.Instance.GetAllBotsInTeam(Team.Team2).Count;
                }

                //if the minimum amount of players are in the game
                if (totalPlayers >= WaitingPlayersAmount)
                {
                    //and they are split in both teams
                    if ((team1Count > 0 && team2Count > 0) || WaitingPlayersAmount <= 1)
                    {
                        //we are ready to start
                        ready = true;
                    }
                    else
                    {
                        //otherwise wait until player split in both teams
#if LOCALIZATION
                        UIReferences.Instance.SetWaitingPlayersText(LocalizationTable.Instance.GetText(128), true);
#else
                        UIReferences.Instance.SetWaitingPlayersText(GameTexts.WaitingTeamBalance, true);
#endif
                        return;
                    }
                }
            }
            if (ready)//all needed players in game
            {
                //master set the call to start the match
                if (PhotonNetworkWrapper.IsMasterClient)
                {
                    MatchTimeManagerBase.Instance.InitAfterWaiting();
                }
                SetGameState(MatchState.Starting);
                MatchTimeManagerBase.Instance.SetTimeState(RoomTimeState.Started, true);
                onAllPlayersRequiredIn?.Invoke();
                UIReferences.Instance.SetWaitingPlayersText("", false);
            }
            else
            {
                UIReferences.Instance.SetWaitingPlayersText(string.Format(GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, 2), true);
            }
        }
        else
        {
            UIReferences.Instance.SetWaitingPlayersText("", false);
        }
    }

    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }

    //PLAYER EVENTS
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player connected: " + newPlayer.NickName);
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            //master sync the require match info to be sure all players have the same info at the start
            photonView.RPC(nameof(RPCSyncGame), newPlayer, GameMatchState);
        }

        // if the new player has a team, it means it came from the waiting room
        if (newPlayer.GetPlayerTeam() != Team.None)
        {
            // Try register the player info right away
            // This is important to do since in game modes where the player have to wait until a round finish before he can spawn
            // the player record won't be added until the second time he spawn
            var playerData = new GamePlayer()
            {
                Name = newPlayer.NickName,
                Team = newPlayer.GetPlayerTeam(),
                isRealPlayer = true,
                isAlive = false,
            };
            RegisterGamePlayer(playerData);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player disconnected: " + otherPlayer.NickName);
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {

    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //when a player has join to a team
        if (changedProps.ContainsKey(PropertiesKeys.TeamKey))
        {
            //make sure has join to a team
            if ((string)changedProps[PropertiesKeys.TeamKey] != Team.None.ToString())
            {
                CheckPlayersInMatch();
            }
            else
            {
                if (GameMatchState == MatchState.Waiting)
                {
                    UIReferences.Instance.SetWaitingPlayersText(string.Format(GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, WaitingPlayersAmount), true);
                }
            }
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("The old masterclient left, we have a new masterclient: " + newMasterClient.NickName);
        RoomChatBase.Instance?.SetChatLocally($"We have a new MasterClient: {newMasterClient.NickName}");
    }

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
    }

    public void OnDisconnected(DisconnectCause cause)
    {
#if UNITY_EDITOR
        if (RoomMenu.Instance.isApplicationQuitting) { return; }
#endif
        Debug.Log("Clean up a bit after server quit, cause: " + cause.ToString());
        PhotonNetwork.IsMessageQueueRunning = false;
        UtilityHelper.LoadLevel(GameConfig.Instance.OnDisconnectScene);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    private Camera cameraRender = null;
    public Camera CameraRendered
    {
        get
        {
            if (cameraRender == null)
            {
                // Debug.Log("Not Camera has been setup.");
                return Camera.current;
            }
            return cameraRender;
        }
        set
        {
            if (cameraRender != null && cameraRender.isActiveAndEnabled)
            {
                //if the current render over the set camera, keep it as renderer camera
                if (cameraRender.depth >= value.depth) return;
            }
            cameraRender = value;
        }
    }

    private bool _enterInGame = false;
    public bool FirstSpawnDone
    {
        get
        {
            return _enterInGame;
        }
        set
        {
            _enterInGame = value;
        }
    }

    public float PlayedTime => (Time.time - StartPlayTime);

    private static GameController _instance;
    public static GameController Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<GameController>(); }
            return _instance;
        }
    }
}