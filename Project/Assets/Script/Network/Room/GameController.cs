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

    // 开始所需的玩家全部加入后触发。仅在 GameData.JoinMethod 为 DirectToMap 时调用
    public Action onAllPlayersRequiredIn;

    // 所有已连接的真实玩家列表
    public List<Player> connectedPlayerList = new List<Player>();

    // 场景中所有已实例化玩家（机器人和真人）的引用列表，不包含本机玩家。
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

    // 本机玩家出生时所用玩家预制体的名字
    public string LastInstancedPlayerPrefabName { get; private set; }

    public static int LocalPlayerViewID = -1;
    public static int SuicideCount = 0;
    public static bool Joined = false;

    private int WaitingPlayersAmount = 1;
    private float StartPlayTime;
    private bool registered = false;
    // 需要过滤玩家出生时使用。该函数在实例化玩家预制体之前调用：返回 true 则不生成玩家，返回 false 同样不生成，需要你在自定义函数里完成实例化。
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

    // 让本机玩家在指定队伍出生。如果已存在玩家实例，会自动销毁并替换成新的实例。
    // 玩家出生的队伍
    // <returns></returns>
    public bool SpawnPlayer(Team playerTeam)
    {
        if (IsLocalWaitingForSpawn) return false;//there's a reserved spawn incoming, don't spawn before that

        //如果已存在本机玩家实例
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);
        }

        //如果游戏已结束
        if (GameFinish)
        {
            RoomCameraBase.Instance?.SetActive(false);
            return false;
        }

        //把玩家队伍写入玩家属性
        var PlayerTeam = new Hashtable
        {
            { PropertiesKeys.TeamKey, playerTeam.ToString() }
        };
        PhotonNetworkWrapper.LocalPlayer.SetCustomProperties(PlayerTeam);
        GameCore.LocalPlayer.Team = playerTeam;

        //生成玩家模型
        SpawnPlayerModel(playerTeam);
        return true;
    }

    // 按队伍生成玩家
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
            // 如果玩家尚未实例化
            return;
        }

        OnPostSpawn();
    }

    // 在指定位置和旋转处生成本机玩家
    // 玩家预制体
    // <param name="position"></param>
    // <param name="rotation"></param>
    // <param name="team"></param>
    // 返回: Succeed instancing the player.
    public bool InstancePlayer(GameObject prefab, Vector3 position, Quaternion rotation, Team team)
    {
        if (!PhotonNetworkWrapper.InRoom) return false;

        // 检查是否有出生过滤器
        if (VerifySpawnFilter(prefab, position, rotation, team, out bool filterResult))
        {
            // 如果有过滤器且返回 True
            if (filterResult)
            {
                // 说明可以生成玩家，于是重新从头执行本函数（检查其他待处理的过滤器）
                InstancePlayer(prefab, position, rotation, team);
            }
            // 过滤器返回 False 表示出生在这里被取消，
            // 出生将由其他地方（该过滤器）处理
            else return false;
        }

        //设置一些公共数据，玩家在其他客户端实例化后立即同步
        var commonData = new object[1];
        commonData[0] = team;

        LastInstancedPlayerPrefabName = prefab.name;

        //实例化玩家预制体
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

    // 检查是否还有待处理的出生过滤器
    // 返回: True if a filter exist, false if not filters availables
    public static bool VerifySpawnFilter(GameObject prefab, Vector3 position, Quaternion rotation, Team team, out bool filterResult)
    {
        filterResult = false;
        if (Instance.spawnFilters == null || Instance.spawnFilters.Count == 0) return false;

        var filter = Instance.spawnFilters.Dequeue();
        filterResult = filter.Invoke(prefab, position, rotation, team);
        return true;
    }

    public static void AddSpawnFilter(Func<GameObject, Vector3, Quaternion, Team, bool> filter)
    {
        if (Instance == null) return;
        if (Instance.spawnFilters == null) Instance.spawnFilters = new Queue<Func<GameObject, Vector3, Quaternion, Team, bool>>();
        Instance.spawnFilters.Enqueue(filter);
    }

    // 把玩家分配到队伍，但不实例化
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

    // 仅在玩家已实例化且存活时才出生该玩家
    public void SpawnPlayerIfAlreadyInstanced()
    {
        if (LocalPlayer == null)
            return;

        Team t = PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam();
        SpawnPlayer(t);
    }

    // 重生延迟结束后重生本机玩家
    // 自定义延迟，填 -1 则使用 GameData 中的固定重生延迟
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

    // 每个玩家出生后调用
    public static void OnPostSpawn(bool skipFade = false)
    {
        if (Instance == null) return;

        Instance.AfterSpawnSetup(skipFade);

        if (!Instance.FirstSpawnDone && MatchInformationDisplay.Instance != null) { MatchInformationDisplay.Instance.DisplayInfo(); }
        Instance.FirstSpawnDone = true;
        CrosshairBase.Instance.Show(true);
    }

    // 如果玩家已存在就销毁
    public void DestroyPlayer(bool ActiveCamera)
    {
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);
        }
        RoomCameraBase.Instance?.SetActive(ActiveCamera);
    }

    // 房间回合时间结束时调用
    public void OnGameTimeFinish(bool gameOver)
    {
        GameFinish = true;
        if (GameModeLogic != null) { GameModeLogic.OnFinishTime(gameOver); }
        SetGameState(MatchState.Finishing);
    }

    // 让本机玩家在当前队伍的随机出生点出生
    public void SpawnPlayerWithCurrentTeam()
    {
        if (SpawnPlayer(PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam()))
            RoomMenu.Instance.OnAutoTeam();
    }

    // 本机玩家出生后调用
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

    // 本机玩家在游戏中得分时应调用
    public void SetPointFromLocalPlayer(int points, GameMode mode)
    {
        if (GameModeLogic == null) return;
        if (mode != GetGameMode) return;

        GameModeLogic.OnLocalPoint(points, PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam());
    }

    // 非玩家对象（如 AI）得分时应调用，只能由主客户端调用
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

    // 这是事件回调，这里会缓存场景中所有 'actors'（玩家和机器人）
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

    // 向 <see cref="OthersActorsInScene"/> 列表中添加新的玩家信息
    // 要注册的新玩家
    // 该玩家已有记录时替换原信息
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

    // 按 PhotonView ID 查找玩家或机器人
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

    // 按 PhotonPlayer 查找玩家或机器人
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

    // 按 PhotonPlayer 查找玩家或机器人
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

    // 按 ViewID 查找玩家或机器人
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

    // 获取已加入指定队伍的 Game 玩家列表
    // 玩家所属的队伍。
    // 只取已登记到 <see cref="OthersActorsInScene"/> 列表中的玩家。
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

    // 获取与本机玩家不在同一队伍的 Game actors 列表
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
                // 游戏模式设置为开始后隐藏房间时，取消房间的公开列出
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

    // 检查是否满足最少玩家数
    void CheckPlayersInMatch()
    {
        //如果仍在等待
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

                //如果游戏内玩家数达到最少要求
                if (totalPlayers >= WaitingPlayersAmount)
                {
                    //并且两队都有人
                    if ((team1Count > 0 && team2Count > 0) || WaitingPlayersAmount <= 1)
                    {
                        //可以开始了
                        ready = true;
                    }
                    else
                    {
                        //否则等待玩家分到两队
                        UIReferences.Instance.SetWaitingPlayersText(GameTexts.WaitingTeamBalance, true);
                        return;
                    }
                }
            }
            if (ready)//all needed players in game
            {
                //主客户端调用开始比赛
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

    //玩家事件
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player connected: " + newPlayer.NickName);
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            //主客户端同步必要的比赛信息，确保开局时所有玩家信息一致
            photonView.RPC(nameof(RPCSyncGame), newPlayer, GameMatchState);
        }

        // 新玩家已有队伍，说明是从等待房间进来的
        if (newPlayer.GetPlayerTeam() != Team.None)
        {
            // 立即尝试注册玩家信息
            // 这一步很重要，因为有些模式里玩家要等到回合结束才能出生，
            // 否则玩家记录要到他第二次出生时才会添加
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
        //玩家加入队伍时
        if (changedProps.ContainsKey(PropertiesKeys.TeamKey))
        {
            //确保已加入队伍
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
                return Camera.current;
            }
            return cameraRender;
        }
        set
        {
            if (cameraRender != null && cameraRender.isActiveAndEnabled)
            {
                //如果当前渲染相机覆盖了所设置的相机，就保留它作为渲染相机
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