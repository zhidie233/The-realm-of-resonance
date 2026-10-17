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
    // References to all the currently instanced bots in the scene
    [HideInInspector] public List<Transform> AllBotsTransforms = new List<Transform>();

    // Information and stats of all the bots currently playing
    [HideInInspector] public List<GameBotProperties> BotsStatistics = new List<GameBotProperties>();

    // Is this game using bots?
    public bool BotsActive { get; set; }

    // Is the bots information already synced by the Mater client?
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

    // Instance all bots for the first time
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

    // Send the bots data to all other clients in the room This data will automatically send to new players
    void SyncBotsDataToAllOthers()
    {
        if (!PhotonNetworkWrapper.IsMasterClient) return;

        Player[] players = PhotonNetworkWrapper.PlayerList;
        string line = GetCompiledBotsData();
        //and send to the new player so him can have the data and update locally.
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 0);

        //also send the slots data so all player have the same list in case the Master Client leave the game
        line = GetCompiledSlotsData();
        //and send to the new player so him can have the data and update locally.
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 1);
        GameEvents.onBotsInitializated?.Invoke();
    }

    // Gets the bots data as a string line
    // <returns></returns>
    public string GetCompiledBotsData()
    {
        //so first we recollect all the stats from the master client and join it in a string line
        string line = string.Empty;
        for (int i = 0; i < BotsStatistics.Count; i++)
        {
            GameBotProperties b = BotsStatistics[i];
            line += string.Format("{0},{1},{2},{3},{4},{5}|", b.Name, b.Kills, b.Deaths, b.Score, (int)b.Team, b.ViewID);
        }
        return line;
    }

    // Get the slots list in a string line
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

    // Setup the team slots where players and bots can be assigned.
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

    // Spawn the a bot in the selected team You can set the agent as null to instantiate it for the first time
    public AIShooter SpawnBot(AIShooter agent = null, Team _team = Team.None)
    {
        var spawnPoint = SpawnPointManager.Instance.GetSingleRandom();
        string AiName = GameConfig.Instance.BotTeam1.name;
        if (agent != null)//if is a already instanced bot
        {
            AiName = (agent.AITeam == Team.Team2) ? GameConfig.Instance.BotTeam2.name : GameConfig.Instance.BotTeam1.name;


            if (agent.AITeam == Team.None) { Debug.LogError($"bot {agent.AIName} has not team"); }

            //Check if the bot has been assigned to a team, or if not, check if there's a space for him
            if (VerifyTeamAffiliation(agent, agent.AITeam))
            {
                spawnPoint = SpawnPointManager.Instance.GetSpawnPointForTeam(agent.AITeam, SpawnPointManager.SpawnMode.Sequential);
            }
            else // there's not space in the team for this bot
            {
                //Check if the bot was registered in a team before
                int ind = BotsStatistics.FindIndex(x => x.Name == agent.AIName);
                if (ind != -1 && ind <= BotsStatistics.Count - 1)
                {
                    //delete the bot data since it won't play anymore.
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
            //AiName = (_team == Team.Team2) ? GameConfig.Instance.BotTeam2.name : GameConfig.Instance.BotTeam1.name;
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
        //use InstantiateSceneObject to make the bots by controlled by Master Client but not destroy them when MC leave the room.
        GameObject bot = PhotonNetwork.InstantiateRoomObject(AiName, spawnPosition, spawnRot, 0, botEssentialData);

        AIShooter newAgent = bot.GetComponent<AIShooter>();
        //if this bot was already in the game
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
            //insert bot stats
            var bs = new GameBotProperties();
            bs.Name = newAgent.AIName;
            bs.Team = _team;
            bs.ViewID = bot.GetComponent<PhotonView>().ViewID;
            BotsStatistics.Add(bs);
            //reserve a space in the team for this bot
            VerifyTeamAffiliation(newAgent, _team);
        }
        newAgent.Init();

        //Build Player Data
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

    // Check if the bot is already assigned in a Team slot
    // <returns></returns>
    private bool VerifyTeamAffiliation(AIShooter agent, Team team)
    {
        var playerSlots = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        //check if the bot is assigned in the team
        if (playerSlots.Exists(x => x.Bot == agent.AIName)) return true;
        else
        {
            //if it's not assigned, check if we can add him
            if (hasSpaceInTeamForBot(team))
            {
                //assign the bot to the team
                int index = playerSlots.FindIndex(x => x.Player == string.Empty && x.Bot == string.Empty);
                playerSlots[index].Bot = agent.AIName;
                return true;
            }
            else { return false; }//bot can't be assigned in team
        }
    }

    // Fetch all the available players (alive) in the map.
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

        // Update the targets for each bot
        for (int i = 0; i < AllBots.Count; i++)
        {
            if (AllBots[i] == null) continue;

            AllBots[i].UpdateTargetList();
        }
    }

    // <param name="bot"></param>
    // <returns></returns>
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

    // Remove the death bot from the available bots and put it on the wait list to respawn
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
                // if the bot is still alive
                if (AllBots[i].gameObject != null)
                {
                    AllBots[i].Respawn();
                }
            }

            // if the bot is death
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

    // Put a bot to the pending list to respawn after the min respawn time.
    public void AddBotToRespawn(AIShooter bot)
    {
        //Debug.Log($"ADD BOT TO RESPAWN: " + bot.AIName);
        SpawningBots.Add(bot);
        //automatically spawn the bot after the re-spawn time
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
            //This fix the issue with the duplicate pv id when a master client re-enter in a room.
            PhotonNetwork.Destroy(SpawningBots[0].gameObject);
            SpawningBots.RemoveAt(0);
        }
    }

    // Update the killer bot kills count and sync with everyone
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

    // Called in all clients when a bot die Update the killed bot death count and sync with everyone.
    // bot that die
    public void SetBotDeath(string killed)
    {
        //if this bots was already replaced by a real player
        if (lastLifeBots.Contains(killed))
        {
            //this is his last life, so since he die, remove his data
            //last life due he got replace by a player so this bot wont respawn again.
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

        // Shouldn't this be called by Master client only?
        if (PhotonNetworkWrapper.IsMasterClient) photonView.RPC(nameof(SyncBotStat), RpcTarget.All, BotsStatistics[index].Name, 0, (byte)1);
    }

    public static void UpdateBotView(AIShooter bot, int viewID)
    {
        var stat = Instance.GetBotStatistics(bot.AIName);
        if (stat == null) return;

        stat.ViewID = viewID;
    }

    // <param name="bot"></param>
    // <param name="gameState"></param>
    public static void SetBotGameState(AIShooter bot, BotGameState gameState)
    {
        var stat = Instance.GetBotStatistics(bot.AIName);
        if (stat == null) return;

        stat.GameState = gameState;
    }

    // Event called when a new player enter in the match
    void OnPlayerEnter(Player player)
    {
        if (player.ActorNumber == PhotonNetworkWrapper.LocalPlayer.ActorNumber) return;

        //cause bots statistics are not sync by Hashtables as player data do we need sync it by RPC
        //so for sync it just one time (after will be update by the local client) we send it when a new player enter (only to the new player)
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            //so first we recollect all the stats from the master client and join it in a string line
            string line = GetCompiledBotsData();
            //and send to the new player so him can have the data and update locally.
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 0);

            //also send the slots data so all player have the same list in case the Master Client leave the game
            line = GetCompiledSlotsData();
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 1);
        }
    }

    // Event called when a player change some property This used to listen when a player change of Team
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

    // Replace one of the bots with the given player in the given team The player will not be immediate replaced but after he dies and won't respawn again.
    // <param name="newPlayer"></param>
    // <param name="playerTeam"></param>
    public void ReplaceBotWithPlayer(Player newPlayer, Team playerTeam)
    {
        if (!BotsActive) return;

        string remplaceBot = string.Empty;
        var slotList = playerTeam == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;

        //check if this player was already assigned (maybe just change of team)
        if (slotList.Exists(x => x.Player == newPlayer.NickName)) return;
        //find a empty slot in the team
        int index = slotList.FindIndex(x => x.Player == string.Empty);
        if (index != -1)
        {
            //replace the bot slot with the new player
            remplaceBot = slotList[index].Bot;
            DeleteBot(remplaceBot);
            slotList[index].Player = newPlayer.NickName;
            slotList[index].Bot = string.Empty;

            //sync the slot change with other players
            if (PhotonNetworkWrapper.IsMasterClient)
            {
                int teamCmdID = playerTeam == Team.Team2 ? 2 : 1;
                photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{teamCmdID}|{newPlayer.NickName}", index, (byte)4);
            }
            Debug.Log($"<color=yellow>Bot {remplaceBot} was replaced by {newPlayer.NickName}</color>");
        }

        //remove the bot that the master client replace
        if (newPlayer.IsMasterClient && PhotonNetworkWrapper.IsMasterClient && !isMasterAlredyInTeam && !string.IsNullOrEmpty(remplaceBot))
        {
            AIShooter bot = AllBots.Find(x => x.AIName == remplaceBot);
            if (bot != null)
            {
                //Debug.Log($"<color=blue>Bot {bot.AIName} was replaced by master {player.NickName}</color>");
                PhotonView bv = bot.GetComponent<PhotonView>();
                bot.References.shooterHealth.DestroyEntity();//destroy on remote clients
                AllBots.Remove(bot);
                AllBotsTransforms.Remove(bot.AimTarget);
                PhotonNetwork.Destroy(bv.gameObject);
            }
            isMasterAlredyInTeam = true;
        }
    }

    // <param name="newMasterClient"></param>
    public void OnMasterClientSwitched(Player newMasterClient)
    {
        //if the new master client is the local client
        if (newMasterClient.ActorNumber == PhotonNetworkWrapper.LocalPlayer.ActorNumber)
        {
            if (Team1PlayersSlots == null || Team1PlayersSlots.Count <= 0)
                SetUpSlots(false);

            //since bots where not collected on the new master client, lets take them manually
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
            // Debug.Log("Bots data has been build in new Master Client");
        }
    }

    public void OnPlayerLeft(Player player)
    {
        if (!BotsActive || GameController.Instance.GameFinish) return;

        //Check if the player was occupying a slot
        Team team = player.GetPlayerTeam();
        var slotList = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        int index = slotList.FindIndex(x => x.Player == player.NickName);
        //empty the occupied slot
        if (index != -1) { slotList[index].Player = ""; }

        //make the master client instance a new bot to replace the player that just left
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            //try to instance the new bot
            var newAgent = SpawnBot(null, team);
            if (newAgent == null) return;

            //find the slot id where the bot was assigned
            int botIndex = slotList.FindIndex(x => x.Bot == newAgent.AIName);
            //sync the new slot with all other players
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{(int)team}|{newAgent.AIName}|{newAgent.photonView.ViewID}", botIndex, (byte)5);
            //show a notification in all players with the new bot name
            KillFeedBase.Instance.SendMessageEvent($"{newAgent.AIName} has joined to {team.GetTeamName()}");
            Debug.Log($"<color=blue>Bot {newAgent.AIName} has replace the player {player.NickName}.</color>");
        }
    }

    // <param name="data"></param>
    // <param name="value"></param>
    // <param name="cmd"></param>
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

            //add the bot statistic
            bs = new GameBotProperties();
            bs.Name = dataSplit[1];
            bs.Team = team;
            bs.ViewID = int.Parse(dataSplit[2]);
            BotsStatistics.Add(bs);
        }
    }

    // <param name="data"></param>
    // <param name="cmd"></param>
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

    // <param name="Name"></param>
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

    // <param name="team"></param>
    // <returns></returns>
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

    // <returns></returns>
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

    // <param name="botName"></param>
    // <returns></returns>
    public GameBotProperties GetBotStatistics(string botName)
    {
        var id = BotsStatistics.FindIndex(x => x.Name == botName);
        if (id < 0) return null;
        return BotsStatistics[id];
    }

    // Count the empty slots (not occupied by a real player)
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

    // <returns></returns>
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