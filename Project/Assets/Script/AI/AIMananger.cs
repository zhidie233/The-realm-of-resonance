using Game.Runtime.AI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables

public class AIMananger : PhotonHelper
{
    [Header("设置")]
    public int updateBotsLookAtEach = 50;
    //场景中当前所有已实例化机器人的引用
    [HideInInspector] public List<Transform> AllBotsTransforms = new List<Transform>();

    //当前所有在局机器人的信息和战绩
    [HideInInspector] public List<GameBotProperties> BotsStatistics = new List<GameBotProperties>();

    //本局是否使用机器人？
    public bool BotsActive { get; set; }

    //机器人信息是否已由主客户端同步？
    public bool HasMasterInfo { get; set; } = false;

    public delegate void EEvent(List<GameBotProperties> stats);
    public static EEvent OnMaterStatsReceived;
    public delegate void StatEvent(GameBotProperties stat);
    public static StatEvent OnBotStatUpdate;

    private GameController GameManager;
    private List<PlayersSlots> Team1PlayersSlots = new List<PlayersSlots>();
    private List<PlayersSlots> Team2PlayersSlots = new List<PlayersSlots>();
    private List<string> BotsNames = new List<string>();
    private List<AIShooter> SpawningBots = new List<AIShooter>();
    private List<string> lastLifeBots = new List<string>();
    private int NumberOfBots = 5;
    private List<AIShooter> AllBots = new List<AIShooter>();
    private List<PlayerReferencesCommon> targetsLists = new List<PlayerReferencesCommon>();
    private bool isMasterAlredyInTeam = false;

    private void Awake()
    {
        GameManager = GameController.Instance;
        BotsNames.AddRange(GameTexts.RandomNames);
        PhotonCallbacks.PlayerPropertiesUpdate += OnPhotonPlayerPropertiesChanged;
        PhotonCallbacks.PlayerEnteredRoom += OnPlayerEnter;
        PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitched;
        PhotonCallbacks.PlayerLeftRoom += OnPlayerLeft;

        if (!PhotonNetworkWrapper.IsConnected)
            return;

        CheckViewAllocation();
        BotsActive = (bool)PhotonNetworkWrapper.CurrentRoom.CustomProperties[PropertiesKeys.WithBotsKey];
        NumberOfBots = PhotonNetworkWrapper.CurrentRoom.MaxPlayers;

        GameEvents.onRemoteActorChange += OnRemotePlayerChange;
        GameEvents.onLocalPlayerDeath += OnLocalDeath;
        GameEvents.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    private void Start()
    {
        FirstSpawn();
        if (GameCore.GameData.UsingWaitingRoom() && PhotonNetworkWrapper.IsMasterClient)
        {
            this.InvokeAfter(2, SyncBotsDataToAllOthers);
        }
    }

    //首次实例化所有机器人
    void FirstSpawn()
    {
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            SetUpSlots(true);
            if (BotsActive)
            {
                if (isOneTeamMode)
                {
                    int requiredBots = EmptySlotsCount(Team.All);
                    for (int i = 0; i < requiredBots; i++)
                    {
                        SpawnBot(null, Team.All);
                    }
                }
                else
                {
                    int half = EmptySlotsCount(Team.Team1);
                    for (int i = 0; i < half; i++)
                    {
                        SpawnBot(null, Team.Team1);
                    }
                    half = EmptySlotsCount(Team.Team2);
                    for (int i = 0; i < half; i++)
                    {
                        SpawnBot(null, Team.Team2);
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        PhotonCallbacks.PlayerPropertiesUpdate -= OnPhotonPlayerPropertiesChanged;
        PhotonCallbacks.PlayerEnteredRoom -= OnPlayerEnter;
        PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitched;
        PhotonCallbacks.PlayerLeftRoom -= OnPlayerLeft;
        GameEvents.onRemoteActorChange -= OnRemotePlayerChange;
        GameEvents.onLocalPlayerDeath -= OnLocalDeath;
        GameEvents.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    //把机器人数据发给房间内其他所有客户端，新玩家会自动收到这些数据
    void SyncBotsDataToAllOthers()
    {
        if (!PhotonNetworkWrapper.IsMasterClient) return;

        Player[] players = PhotonNetworkWrapper.PlayerList;
        string line = GetCompiledBotsData();
        //再发给新玩家，让他拿到数据在本地更新。
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 0);

        //同时发送槽位数据，保证主客户端退出后所有玩家的列表一致
        line = GetCompiledSlotsData();
        //再发给新玩家，让他拿到数据在本地更新。
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 1);
        GameEvents.onBotsInitializated?.Invoke();
    }

    //把机器人数据取成一行字符串
    // <returns></returns>
    public string GetCompiledBotsData()
    {
        //先从主客户端收集全部战绩，拼成一行字符串
        string line = string.Empty;
        for (int i = 0; i < BotsStatistics.Count; i++)
        {
            GameBotProperties b = BotsStatistics[i];
            line += string.Format("{0},{1},{2},{3},{4},{5}|", b.Name, b.Kills, b.Deaths, b.Score, (int)b.Team, b.ViewID);
        }
        return line;
    }

    //把槽位列表取成一行字符串
    // <returns></returns>
    public string GetCompiledSlotsData()
    {
        string line = string.Empty;
        for (int i = 0; i < Team1PlayersSlots.Count; i++)
        {
            var d = Team1PlayersSlots[i];
            line += string.Format("{0},{1}|", d.Player, d.Bot);
        }
        line += "&";
        if (!isOneTeamMode)
        {
            for (int i = 0; i < Team2PlayersSlots.Count; i++)
            {
                var d = Team2PlayersSlots[i];
                line += string.Format("{0},{1}|", d.Player, d.Bot);
            }
        }
        return line;
    }

    //设置队伍槽位，用来分配玩家和机器人。
    void SetUpSlots(bool addExistingPlayers)
    {
        Team1PlayersSlots.Clear();
        Team2PlayersSlots.Clear();
        var team1Players = PhotonNetworkWrapper.PlayerList.GetPlayersInTeam(isOneTeamMode ? Team.All : Team.Team1).ToList();

        if (!isOneTeamMode)
        {
            var team2Players = PhotonNetworkWrapper.PlayerList.GetPlayersInTeam(Team.Team2).ToList();

            int ptp = NumberOfBots / 2;
            for (int i = 0; i < ptp; i++)
            {
                PlayersSlots s = new PlayersSlots();
                s.Bot = string.Empty;
                if (addExistingPlayers && team1Players.Count > 0)
                {
                    s.Player = team1Players[0].NickName;
                    team1Players.RemoveAt(0);
                }
                else
                {
                    s.Player = string.Empty;
                }
                Team1PlayersSlots.Add(s);
            }
            for (int i = 0; i < ptp; i++)
            {
                PlayersSlots s = new PlayersSlots();
                s.Bot = string.Empty;
                if (addExistingPlayers && team2Players.Count > 0)
                {
                    s.Player = team2Players[0].NickName;
                    team2Players.RemoveAt(0);
                }
                else
                {
                    s.Player = string.Empty;
                }
                Team2PlayersSlots.Add(s);
            }
        }
        else
        {
            for (int i = 0; i < NumberOfBots; i++)
            {
                PlayersSlots s = new PlayersSlots();
                s.Bot = string.Empty;
                if (addExistingPlayers && team1Players.Count > 0)
                {
                    s.Player = team1Players[0].NickName;
                    team1Players.RemoveAt(0);
                    Debug.Log("Set default player in slot: " + s.Player);
                }
                else
                {
                    s.Player = string.Empty;
                }
                Team1PlayersSlots.Add(s);
            }
        }
    }

    //在指定队伍出生一个机器人，首次实例化时 agent 传 null
    public AIShooter SpawnBot(AIShooter agent = null, Team _team = Team.None)
    {
        var spawnPoint = SpawnPointManager.Instance.GetSingleRandom();
        string AiName = GameConfig.Instance.BotTeam1.name;
        if (agent != null)//if is a already instanced bot
        {
            AiName = (agent.AITeam == Team.Team2) ? GameConfig.Instance.BotTeam2.name : GameConfig.Instance.BotTeam1.name;


            if (agent.AITeam == Team.None) { Debug.LogError($"bot {agent.AIName} has not team"); }

            //检查机器人是否已分配队伍，没有就看看有没有空位给它
            if (VerifyTeamAffiliation(agent, agent.AITeam))
            {
                spawnPoint = SpawnPointManager.Instance.GetSpawnPointForTeam(agent.AITeam, SpawnPointManager.SpawnMode.Sequential);
            }
            else // there's not space in the team for this bot
            {
                //检查机器人之前是否注册过队伍
                int ind = BotsStatistics.FindIndex(x => x.Name == agent.AIName);
                if (ind != -1 && ind <= BotsStatistics.Count - 1)
                {
                    //机器人不再参战，删除它的数据。
                    BotsStatistics.RemoveAt(ind);
                }
                return null;
            }
        }
        else
        {
            switch (_team)
            {
                case Team.Team2:
                    AiName = GameConfig.Instance.BotTeam2.name;
                    break;
                case Team.Team1:
                    AiName = GameConfig.Instance.BotTeam1.name;
                    break;
                case Team.Team3:
                    AiName = GameConfig.Instance.BotTeam3.name;
                    break;
            }
            if (!isOneTeamMode)//if team mode, spawn bots in the respective team spawn points.
            {
                spawnPoint = SpawnPointManager.Instance.GetSpawnPointForTeam(_team, SpawnPointManager.SpawnMode.Sequential);
            }
        }

        int rbn = Random.Range(0, BotsNames.Count);
        string AIName = agent == null ? string.Format(GameConfig.Instance.botsNameFormat, BotsNames[rbn]) : agent.AIName;
        Team AITeam = agent == null ? _team : agent.AITeam;

        spawnPoint.GetSpawnPosition(out Vector3 spawnPosition, out Quaternion spawnRot);

        object[] botEssentialData = new object[] { AIName, AITeam };
        //用 InstantiateSceneObject 让机器人由主客户端控制，主客户端离开房间时也不会销毁它们。
        GameObject bot = PhotonNetwork.InstantiateRoomObject(AiName, spawnPosition, spawnRot, 0, botEssentialData);

        AIShooter newAgent = bot.GetComponent<AIShooter>();
        //如果这个机器人已在局中
        if (agent != null)
        {
            newAgent.AIName = agent.AIName;
            newAgent.AITeam = agent.AITeam;
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, agent.AIName, bot.GetComponent<PhotonView>().ViewID, (byte)3);
        }
        else//if this is the first time instancing this bot
        {
            newAgent.AIName = AIName;
            newAgent.AITeam = _team;
            BotsNames.RemoveAt(rbn);
            //写入机器人战绩
            var bs = new GameBotProperties();
            bs.Name = newAgent.AIName;
            bs.Team = _team;
            bs.ViewID = bot.GetComponent<PhotonView>().ViewID;
            BotsStatistics.Add(bs);
            //在队伍里为这个机器人预留一个空位
            VerifyTeamAffiliation(newAgent, _team);
        }
        newAgent.Init();

        //构建玩家数据
        GamePlayer playerData = new GamePlayer()
        {
            Name = newAgent.AIName,
            Team = newAgent.AITeam,
            Actor = newAgent.transform,
            AimPosition = newAgent.AimTarget,
            isRealPlayer = false,
            isAlive = true,
        };

        GameEvents.DispatchRemoteActorChange(new GameEvents.PlayerChangeData()
        {
            PlayerName = newAgent.AIName,
            GameActor = playerData,
            IsAlive = true,
            NetworkView = newAgent.GetComponent<PhotonView>()
        });

        AllBots.Add(newAgent);
        AllBotsTransforms.Add(newAgent.AimTarget);

        return newAgent;
    }

    //检查机器人是否已分配到队伍槽位
    // <returns></returns>
    private bool VerifyTeamAffiliation(AIShooter agent, Team team)
    {
        var playerSlots = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        //检查机器人是否已分配到该队伍
        if (playerSlots.Exists(x => x.Bot == agent.AIName)) return true;
        else
        {
            //未分配的话，看看能不能把它加进去
            if (hasSpaceInTeamForBot(team))
            {
                //把机器人分配到该队伍
                int index = playerSlots.FindIndex(x => x.Player == string.Empty && x.Bot == string.Empty);
                playerSlots[index].Bot = agent.AIName;
                return true;
            }
            else { return false; }//bot can't be assigned in team
        }
    }

    //获取地图上所有可用的（存活）玩家。
    private void UpdateTargetList()
    {
        if (!PhotonNetworkWrapper.IsMasterClient || GameController.Instance == null) return;

        targetsLists.Clear();
        var all = GameController.Instance.OthersActorsInScene;

        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].Actor == null) continue;
            targetsLists.Add(all[i].Actor.GetComponent<PlayerReferencesCommon>());
        }

        if (GameCore.LocalPlayerReferences != null)
        {
            targetsLists.Add(GameCore.LocalPlayerReferences);
        }

        //刷新每个机器人的目标
        for (int i = 0; i < AllBots.Count; i++)
        {
            if (AllBots[i] == null) continue;

            AllBots[i].UpdateTargetList();
        }
    }

    public void GetTargetsFor(AIShooter bot, ref List<Transform> list)
    {
        list.Clear();
        PlayerReferencesCommon t;
        for (int i = 0; i < targetsLists.Count; i++)
        {
            t = targetsLists[i];
            if (t == null || t.name == bot.AIName) continue;

            if (isOneTeamMode)
            {
                list.Add(t.BotAimTarget);
            }
            else
            {
                if (t.PlayerTeam != Team.None && t.PlayerTeam == bot.AITeam) continue;

                list.Add(t.BotAimTarget);
            }
        }
    }

    //把死亡的机器人从可用列表移出，放进等待重生列表
    public void OnBotDeath(AIShooter agent, AIShooter killer)
    {
        if (!PhotonNetworkWrapper.IsMasterClient)
            return;

        AllBots.Remove(agent);
        AllBotsTransforms.Remove(agent.AimTarget);
        for (int i = 0; i < AllBots.Count; i++)
        {
            AllBots[i].CheckTargets();
        }

        AddBotToRespawn(agent);

        UpdateTargetList();
    }

    public void RespawnAllBots(bool forcedRespawn = false)
    {
        if (!PhotonNetworkWrapper.IsMasterClient) return;

        for (int i = AllBots.Count - 1; i >= 0; i--)
        {
            if (forcedRespawn)
            {
                //如果机器人仍然存活
                if (AllBots[i].gameObject != null)
                {
                    AllBots[i].Respawn();
                }
            }

            //如果机器人已死亡
            if (AllBots[i].gameObject == null)
            {
                SpawnBot(AllBots[i]);
                if (AllBots[i] != null)
                {
                    AllBots[i].GetComponent<AIShooterHealth>().DestroyEntity();
                }
            }
        }

        for (int i = SpawningBots.Count - 1; i >= 0; i--)
        {
            if (SpawningBots[i] == null || SpawningBots[i].gameObject == null) continue;

            SpawnBot(SpawningBots[i]);
            SpawningBots[i].GetComponent<AIShooterHealth>().DestroyEntity();
            SpawningBots.RemoveAt(i);
        }
    }

    //把机器人放入待重生列表，等最短重生时间过去后重生。
    public void AddBotToRespawn(AIShooter bot)
    {
        SpawningBots.Add(bot);
        //重生时间到后自动出生
        if (GetGameMode.GetGameModeInfo().onPlayerDie == GameModeSettings.OnPlayerDie.SpawnAfterDelay)
        {
            Invoke(nameof(SpawnPendingBot), GameConfig.Instance.PlayerRespawnTime);
        }
    }

    void SpawnPendingBot()
    {
        if (SpawningBots == null || SpawningBots.Count <= 0) return;
        if (SpawningBots[0] != null)
        {
            SpawnBot(SpawningBots[0]);
            //修复主客户端重新进入房间时 PhotonView ID 重复的问题。
            PhotonNetwork.Destroy(SpawningBots[0].gameObject);
            SpawningBots.RemoveAt(0);
        }
    }

    //更新凶手机器人的击杀数并同步给所有人
    public void SetBotKill(string botName)
    {
        var stats = GetBotStatistics(botName);
        if (stats == null) return;

        photonView.RPC(nameof(SyncBotStat), RpcTarget.All, stats.Name, 0, (byte)0);

        GameController.Instance.SetPoint(1, GameMode.FFA, Team.All);
        GameController.Instance.SetPoint(1, GameMode.TDM, stats.Team);
    }

    public void SetBotScore(string botName, int score)
    {
        var stat = GetBotStatistics(botName);
        if (stat == null) return;

        stat.Score += score;
    }

    //机器人死亡时在所有客户端调用，更新其死亡数并同步给所有人。
    //死亡的机器人
    public void SetBotDeath(string killed)
    {
        //如果这个机器人已被真人玩家替换
        if (lastLifeBots.Contains(killed))
        {
            //这是它最后一条命，既然死了就删除它的数据
            //因为已被玩家替换，这是最后一条命，不会再重生。
            int bi = GameController.Instance.OthersActorsInScene.FindIndex(x => x.Name == killed);
            if (bi != -1)
            {
                GameController.Instance.OthersActorsInScene.RemoveAt(bi);
                lastLifeBots.Remove(killed);
                RemoveBotInfo(killed);
                return;
            }
        }
        int index = BotsStatistics.FindIndex(x => x.Name == killed);
        if (index <= -1) return;

        GameEvents.EventBotDeath(killed);

        //这里难道不该只由主客户端调用？
        if (PhotonNetworkWrapper.IsMasterClient) photonView.RPC(nameof(SyncBotStat), RpcTarget.All, BotsStatistics[index].Name, 0, (byte)1);
    }

    public static void UpdateBotView(AIShooter bot, int viewID)
    {
        var stat = Instance.GetBotStatistics(bot.AIName);
        if (stat == null) return;

        stat.ViewID = viewID;
    }

    public static void SetBotGameState(AIShooter bot, BotGameState gameState)
    {
        var stat = Instance.GetBotStatistics(bot.AIName);
        if (stat == null) return;

        stat.GameState = gameState;
    }

    //新玩家进入对局时触发的事件
    void OnPlayerEnter(Player player)
    {
        if (player.ActorNumber == PhotonNetworkWrapper.LocalPlayer.ActorNumber) return;

        //机器人战绩不像玩家数据那样用 Hashtable 同步，所以要用 RPC 同步
        //只需同步一次（之后由本地客户端更新），因此在新玩家进入时发送（只发给新玩家）
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            //先从主客户端收集全部战绩，拼成一行字符串
            string line = GetCompiledBotsData();
            //再发给新玩家，让他拿到数据在本地更新。
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 0);

            //同时发送槽位数据，保证主客户端退出后所有玩家的列表一致
            line = GetCompiledSlotsData();
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 1);
        }
    }

    //玩家修改属性时触发的事件，用来监听玩家换队
    public void OnPhotonPlayerPropertiesChanged(Player player, Hashtable changedProps)
    {
        if (!BotsActive)
            return;
        if (!changedProps.ContainsKey(PropertiesKeys.TeamKey)) return;

        string teamName = (string)changedProps[PropertiesKeys.TeamKey];
        var team = teamName == Team.Team2.ToString() ? Team.Team2 : Team.Team1;

        if (teamName == Team.None.ToString()) return;

        ReplaceBotWithPlayer(player, team);
    }

    //用指定玩家替换指定队伍里的一个机器人。玩家不会立刻替换，而是在他死亡且不再重生后才替换。
    // <param name="newPlayer"></param>
    // <param name="playerTeam"></param>
    public void ReplaceBotWithPlayer(Player newPlayer, Team playerTeam)
    {
        if (!BotsActive) return;

        string remplaceBot = string.Empty;
        var slotList = playerTeam == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;

        //检查这个玩家是否已分配过（可能只是换了队伍）
        if (slotList.Exists(x => x.Player == newPlayer.NickName)) return;
        //在队伍里找一个空位
        int index = slotList.FindIndex(x => x.Player == string.Empty);
        if (index != -1)
        {
            //用新玩家替换该机器人的槽位
            remplaceBot = slotList[index].Bot;
            DeleteBot(remplaceBot);
            slotList[index].Player = newPlayer.NickName;
            slotList[index].Bot = string.Empty;

            //把槽位变更同步给其他玩家
            if (PhotonNetworkWrapper.IsMasterClient)
            {
                int teamCmdID = playerTeam == Team.Team2 ? 2 : 1;
                photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{teamCmdID}|{newPlayer.NickName}", index, (byte)4);
            }
            Debug.Log($"<color=yellow>Bot {remplaceBot} was replaced by {newPlayer.NickName}</color>");
        }

        //删除被主客户端替换掉的那个机器人
        if (newPlayer.IsMasterClient && PhotonNetworkWrapper.IsMasterClient && !isMasterAlredyInTeam && !string.IsNullOrEmpty(remplaceBot))
        {
            AIShooter bot = AllBots.Find(x => x.AIName == remplaceBot);
            if (bot != null)
            {
                PhotonView bv = bot.GetComponent<PhotonView>();
                bot.References.shooterHealth.DestroyEntity();//destroy on remote clients
                AllBots.Remove(bot);
                AllBotsTransforms.Remove(bot.AimTarget);
                PhotonNetwork.Destroy(bv.gameObject);
            }
            isMasterAlredyInTeam = true;
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        //如果新的主客户端就是本地客户端
        if (newMasterClient.ActorNumber == PhotonNetworkWrapper.LocalPlayer.ActorNumber)
        {
            if (Team1PlayersSlots == null || Team1PlayersSlots.Count <= 0)
                SetUpSlots(false);

            //机器人没有在新主客户端上收集，这里手动取出来
            AIShooter[] allBots = FindObjectsOfType<AIShooter>();
            foreach (var bot in allBots)
            {
                if (bot.isDeath)//if the bot was death when master client leave the game
                {
                    AddBotToRespawn(bot);
                    continue;
                }
                AllBots.Add(bot);
                AllBotsTransforms.Add(bot.transform);
                bot.Init();
            }
        }
    }

    public void OnPlayerLeft(Player player)
    {
        if (!BotsActive || GameController.Instance.GameFinish) return;

        //检查玩家是否占用了某个槽位
        Team team = player.GetPlayerTeam();
        var slotList = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        int index = slotList.FindIndex(x => x.Player == player.NickName);
        //清空被占用的槽位
        if (index != -1) { slotList[index].Player = ""; }

        //让主客户端实例化一个新机器人，补上刚离开的玩家
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            //尝试实例化新机器人
            var newAgent = SpawnBot(null, team);
            if (newAgent == null) return;

            //找到机器人被分配到的槽位 ID
            int botIndex = slotList.FindIndex(x => x.Bot == newAgent.AIName);
            //把新槽位同步给其他所有玩家
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{(int)team}|{newAgent.AIName}|{newAgent.photonView.ViewID}", botIndex, (byte)5);
            //向所有玩家显示新机器人的名字通知
            KillFeedBase.Instance.SendMessageEvent($"{newAgent.AIName} has joined to {team.GetTeamName()}");
            Debug.Log($"<color=blue>Bot {newAgent.AIName} has replace the player {player.NickName}.</color>");
        }
    }

    [PunRPC]
    public void SyncBotStat(string data, int value, byte cmd)
    {
        GameBotProperties bs = BotsStatistics.Find(x => x.Name == data);
        if (bs == null && cmd != 5) return;
        if (cmd == 0)//add kill
        {
            bs.Kills++;
            bs.Score += GameConfig.Instance.ScoreReward.ScorePerKill;
        }
        else if (cmd == 1)//death
        {
            bs.Deaths++;
            bs.GameState = BotGameState.Death;
        }
        else if (cmd == 2)//remove bot
        {
            RemoveBotInfo(data);
        }
        else if (cmd == 3)//update view id
        {
            bs.ViewID = value;
            OnBotStatUpdate?.Invoke(bs);
        }
        else if (cmd == 4)//replace bot slot with a player
        {
            string[] dataSplit = data.Split('|');
            var list = int.Parse(dataSplit[0]) == 1 ? Team1PlayersSlots : Team2PlayersSlots;
            if (list.Count <= 0) { Debug.LogWarning("Team slots has not been setup yet."); return; }
            list[value].Player = dataSplit[1];
            list[value].Bot = "";
        }
        else if (cmd == 5)//add single new bot
        {
            string[] dataSplit = data.Split('|');
            Team team = (Team)int.Parse(dataSplit[0]);
            var list = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
            if (list.Count <= 0) { Debug.LogWarning("Team slots has not been setup yet."); return; }
            list[value].Player = "";
            list[value].Bot = dataSplit[1];

            //添加机器人战绩
            bs = new GameBotProperties();
            bs.Name = dataSplit[1];
            bs.Team = team;
            bs.ViewID = int.Parse(dataSplit[2]);
            BotsStatistics.Add(bs);
        }
    }

    [PunRPC]
    void SyncAllBotsStats(string data, int cmd)
    {
        if (cmd == 0)//bots statistics
        {
            BotsStatistics.Clear();
            string[] split = data.Split("|"[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                string[] info = split[i].Split(","[0]);
                GameBotProperties bs = new GameBotProperties();
                bs.Name = info[0];
                bs.Kills = int.Parse(info[1]);
                bs.Deaths = int.Parse(info[2]);
                bs.Score = int.Parse(info[3]);
                bs.Team = (Team)int.Parse(info[4]);
                bs.ViewID = int.Parse(info[5]);
                BotsStatistics.Add(bs);

                if (!GameController.Instance.OthersActorsInScene.Exists(x => x.Name == bs.Name))
                {
                    GameController.Instance.OthersActorsInScene.Add(new GamePlayer()
                    {
                        Name = bs.Name,
                        isRealPlayer = false,
                        Team = bs.Team,
                    });
                }
            }
            OnMaterStatsReceived?.Invoke(BotsStatistics);
            HasMasterInfo = true;
        }
        else if (cmd == 1)//team slots info
        {
            SetUpSlots(false);
            string[] teams = data.Split('&');
            string[] teamInfo = teams[0].Split('|');//get the first team slots
            for (int i = 0; i < teamInfo.Length; i++)
            {
                if (string.IsNullOrEmpty(teamInfo[i])) continue;

                string[] slot = teamInfo[i].Split(',');
                Team1PlayersSlots[i].Player = slot[0];
                Team1PlayersSlots[i].Bot = slot[1];
            }
            if (!isOneTeamMode)
            {
                teamInfo = teams[1].Split('|');//get the second team slots
                for (int i = 0; i < teamInfo.Length; i++)
                {
                    if (string.IsNullOrEmpty(teamInfo[i])) continue;

                    string[] slot = teamInfo[i].Split(',');
                    Team2PlayersSlots[i].Player = slot[0];
                    Team2PlayersSlots[i].Bot = slot[1];
                }
            }
            GameEvents.onBotsInitializated?.Invoke();
        }
    }

    void RemoveBotInfo(string botName)
    {
        int bi = BotsStatistics.FindIndex(x => x.Name == botName);
        if (bi != -1) BotsStatistics.RemoveAt(bi);

        bi = GameController.Instance.OthersActorsInScene.FindIndex(x => x.Name == botName);
        if (bi != -1)
        {
            GameController.Instance.OthersActorsInScene.RemoveAt(bi);
        }

        lastLifeBots.Add(botName);
    }

    void DeleteBot(string Name)
    {
        if (BotsStatistics.Exists(x => x.Name == Name))
        {
            photonView.RPC(nameof(SyncBotStat), RpcTarget.All, Name, 0, (byte)2);
        }
    }

    private void OnRemotePlayerChange(GameEvents.PlayerChangeData changeData)
    {
        UpdateTargetList();
    }

    private void OnLocalDeath()
    {
        UpdateTargetList();
    }

    private void OnLocalPlayerSpawn()
    {
        UpdateTargetList();
    }

    public List<GamePlayer> GetAllBotsInTeam(Team team)
    {
        List<GamePlayer> list = new List<GamePlayer>();
        for (int i = 0; i < GameController.Instance.OthersActorsInScene.Count; i++)
        {
            if (GameController.Instance.OthersActorsInScene[i].isRealPlayer) continue;

            if (GameController.Instance.OthersActorsInScene[i].Team == team)
            {
                list.Add(GameController.Instance.OthersActorsInScene[i]);
            }
        }
        return list;
    }

    public List<Transform> GetOtherBots(Transform bot, Team _team)
    {
        List<Transform> all = new List<Transform>();
        if (isOneTeamMode)
        {
            all.AddRange(AllBotsTransforms);
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == null) continue;
                if (all[i].transform.root.name.Contains("(die)") || all[i].transform.root == bot.root)
                {
                    all.RemoveAt(i);
                }
            }
        }
        else //if TDM game mode
        {
            for (int i = 0; i < AllBotsTransforms.Count; i++)
            {
                if (AllBotsTransforms[i] == null) continue;

                Transform t = AllBotsTransforms[i].root;
                AIShooter asa = t.GetComponent<AIShooter>();
                if (t.name.Contains("(die)") || asa.isDeath) continue;

                if (asa.AITeam != _team && asa.AITeam != Team.None && t != bot.root)
                {
                    all.Add(AllBotsTransforms[i]);
                }
            }
        }
        if (all.Contains(bot))
        {
            all.Remove(bot);
        }
        return all;
    }

    public GameBotProperties GetBotStatistics(string botName)
    {
        var id = BotsStatistics.FindIndex(x => x.Name == botName);
        if (id < 0) return null;
        return BotsStatistics[id];
    }

    //统计空槽位数量（没有被真人玩家占用的）
    // <returns></returns>
    public int EmptySlotsCount(Team team)
    {
        int count = 0;
        var list = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        for (int i = 0; i < list.Count; i++)
        {
            if (string.IsNullOrEmpty(list[i].Player)) count++;
        }
        return count;
    }

    private bool hasSpaceInTeam(Team team)
    {
        if (team == Team.Team2)
        {
            return Team2PlayersSlots.Exists(x => x.Player == string.Empty);
        }
        else
        {
            return Team1PlayersSlots.Exists(x => x.Player == string.Empty);
        }
    }

    private bool hasSpaceInTeamForBot(Team team)
    {
        if (team == Team.Team2)
        {
            return Team2PlayersSlots.Exists(x => x.Player == string.Empty && x.Bot == string.Empty);
        }
        else
        {
            return Team1PlayersSlots.Exists(x => x.Player == string.Empty && x.Bot == string.Empty);
        }
    }

    public AIShooter GetBot(int viewID)
    {
        foreach (AIShooter agent in AllBots)
        {
            if (agent.photonView.ViewID == viewID)
            {
                return agent;
            }
        }
        return null;
    }

    public GameBotProperties GetBotWithMoreKills()
    {
        if (BotsStatistics == null || BotsStatistics.Count <= 0)
        {
            GameBotProperties bs = new GameBotProperties()
            {
                Name = "None",
                Kills = 0,
                Team = Team.None,
                Score = 0,
            };
            return bs;
        }
        int high = 0;
        int id = 0;
        for (int i = 0; i < BotsStatistics.Count; i++)
        {
            if (BotsStatistics[i].Kills > high)
            {
                high = BotsStatistics[i].Kills;
                id = i;
            }
        }
        return BotsStatistics[id];
    }

    [System.Serializable]
    public class PlayersSlots
    {
        public string Player;
        public string Bot;
    }

    private static AIMananger _instance;
    public static AIMananger Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<AIMananger>(); }
            return _instance;
        }
    }
}