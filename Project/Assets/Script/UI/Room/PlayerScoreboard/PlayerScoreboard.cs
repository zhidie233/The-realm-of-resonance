using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using System.Linq;
using TMPro;

public class PlayerScoreboard : PlayerScoreboardBase
{
    #region Public members
    [Header("Settings")]
    [GameToggle] public bool updateOnEnable = true;
    [GameToggle] public bool autoUpdate = true;

    [Header("References")]
    public PlayerScoreboardTableBase[] TwoTeamScoreboards;
    public PlayerScoreboardTableBase OneTeamScoreboard;
    public ScoreboardPopUpMenuBase popUpMenu;
    public GameObject playerscoreboardUIBinding;
    public TextMeshProUGUI SpectatorsCountText;
    #endregion

    #region Private members
    Dictionary<int, PlayerScoreboardUIBase> cachedUIBindings = new Dictionary<int, PlayerScoreboardUIBase>();
    Dictionary<GameBotProperties, PlayerScoreboardUIBase> cachedBotsUIBindings = new Dictionary<GameBotProperties, PlayerScoreboardUIBase>();
    bool botsScoreInstance = false;
    private List<PlayerScoreboardUIBase> cachePlayerScoreboardSorted = new List<PlayerScoreboardUIBase>();
    private List<PlayerScoreboardUIBase> cachePlayerScoreboardSorted2 = new List<PlayerScoreboardUIBase>();
    #endregion

    public bool isShowingTables { get; set; } = false;

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        NetworkCallbacks.PlayerLeftRoom += OnPlayerLeftRoom;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        NetworkCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
        NetworkCallbacks.RoomPropertiesUpdate += OnRoomPropertiesUpdate;
        if (updateOnEnable) ForceUpdateAll();
        if (popUpMenu != null) popUpMenu.SetActive(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        NetworkCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        NetworkCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesUpdate;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkCallbacks.PlayerLeftRoom -= OnPlayerLeftRoom;
    }
    #endregion

    public void UpdateTables()
    {
        int spectators = 0;
        var players = NetworkWrapper.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            //检查玩家是否已选择队伍
            if (p.GetPlayerTeam() != Team.None)
            {
                //该玩家的 UI 绑定是否已创建？
                if (cachedUIBindings.ContainsKey(p.ActorNumber))
                {
                    //若已创建，只需刷新其信息
                    cachedUIBindings[p.ActorNumber]?.Refresh();
                }
                else
                {
                    PlayerScoreboardUIBase script = null;
                    if (!isOneTeamMode)
                    {
                        if (p.GetPlayerTeam() == Team.Team1)
                            script = TwoTeamScoreboards[0].Instance(p, playerscoreboardUIBinding);
                        else
                            script = TwoTeamScoreboards[1].Instance(p, playerscoreboardUIBinding);
                    }
                    else
                    {
                        script = OneTeamScoreboard.Instance(p, playerscoreboardUIBinding);
                    }
                    cachedUIBindings.Add(p.ActorNumber, script);
                }
            }
            else { spectators++; }//if has not team
        }
        UpdateBotScoreboard();
        if (SpectatorsCountText != null)
        {
            SpectatorsCountText.text = string.Format(GameTexts.Spectators, spectators);
        }
        SortScoreboard();
    }

    /// <summary>
    /// 更新记分板中的玩家和机器人字段
    /// </summary>
    void UpdateBotScoreboard()
    {
        if (AIManager.Instance == null || !AIManager.Instance.BotsActive || AIManager.Instance.BotsStatistics.Count <= 0) return;

        int c = AIManager.Instance.BotsStatistics.Count;
        for (int i = 0; i < c; i++)
        {
            GameBotProperties stat = AIManager.Instance.BotsStatistics[i];
            if (botsScoreInstance)
            {
                if (cachedBotsUIBindings.ContainsKey(stat))
                {
                    cachedBotsUIBindings[stat]?.UpdateBot();
                }
                else
                {
                    InstanceBotUIBinding(stat);
                }
            }
            else
            {
                InstanceBotUIBinding(stat);
            }
        }
        botsScoreInstance = true;
    }

    void InstanceBotUIBinding(GameBotProperties info)
    {
        PlayerScoreboardUIBase script = null;
        if (!isOneTeamMode)
        {
            if (info.Team == Team.Team1)
                script = TwoTeamScoreboards[0].InstanceBot(info, playerscoreboardUIBinding);
            else
                script = TwoTeamScoreboards[1].InstanceBot(info, playerscoreboardUIBinding);
        }
        else
        {
            script = OneTeamScoreboard.InstanceBot(info, playerscoreboardUIBinding);
        }
        cachedBotsUIBindings.Add(info, script);
    }

    /// <summary>
    /// 强制刷新记分板信息
    /// </summary>
    public override void ForceUpdateAll()
    {
        List<PlayerScoreboardUIBase> ailist = cachedBotsUIBindings.Values.ToList();
        foreach (var item in ailist)
        {
            item?.Refresh();
        }
        UpdateTables();
    }

    /// <summary>
    /// 按得分对记分板玩家排序
    /// </summary>
    void SortScoreboard()
    {
        if (isOneTeamMode)
        {
            cachePlayerScoreboardSorted.Clear();
            cachePlayerScoreboardSorted.AddRange(cachedUIBindings.Values.ToArray());
            cachePlayerScoreboardSorted.AddRange(cachedBotsUIBindings.Values.ToArray());
            cachePlayerScoreboardSorted = cachePlayerScoreboardSorted.OrderBy(x => x.GetScore()).ToList();

            for (int i = 0; i < cachePlayerScoreboardSorted.Count; i++)
            {
                if (cachePlayerScoreboardSorted[i] == null) return;
                cachePlayerScoreboardSorted[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted.Count - 1) - i);
            }
        }
        else
        {
            cachePlayerScoreboardSorted.Clear();
            cachePlayerScoreboardSorted2.Clear();
            List<PlayerScoreboardUIBase> all = new List<PlayerScoreboardUIBase>();
            all.AddRange(cachedUIBindings.Values.ToArray());
            all.AddRange(cachedBotsUIBindings.Values.ToArray());

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].GetTeam() == Team.Team1)
                {
                    cachePlayerScoreboardSorted.Add(all[i]);
                }
                else if (all[i].GetTeam() == Team.Team2)
                {
                    cachePlayerScoreboardSorted2.Add(all[i]);
                }
            }
            cachePlayerScoreboardSorted = cachePlayerScoreboardSorted.OrderBy(x => x.GetScore()).ToList();
            cachePlayerScoreboardSorted2 = cachePlayerScoreboardSorted2.OrderBy(x => x.GetScore()).ToList();
            for (int i = 0; i < cachePlayerScoreboardSorted.Count; i++)
            {
                if (cachePlayerScoreboardSorted[i] == null) return;
                cachePlayerScoreboardSorted[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted.Count - 1) - i);
            }
            for (int i = 0; i < cachePlayerScoreboardSorted2.Count; i++)
            {
                if (cachePlayerScoreboardSorted2[i] == null) return;
                cachePlayerScoreboardSorted2[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted2.Count - 1) - i);
            }
        }
    }

    public override void SetActiveByTeamMode(bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm);
        foreach (var table in TwoTeamScoreboards) { table?.SetActive(!tm); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = true;
        ForceUpdateAll();
    }

    public override void SetActive(bool active, bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm && active);
        foreach (var table in TwoTeamScoreboards) { table?.SetActive(!tm && active); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = active;
        if (active) { ForceUpdateAll(); }
    }

    public override void SetActiveByGameState(bool active)
    {
        SetActive(active, (!GameController.Instance.FirstSpawnDone && GameController.Joined));
    }

    public override void ResetJoinButtons()
    {
        OneTeamScoreboard?.ResetJoinButton();
        foreach (var table in TwoTeamScoreboards) { table?.ResetJoinButton(); }
    }

    public override void SetActiveJoinButtons(bool active)
    {
        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActiveJoinButton(active && tm);
        foreach (var table in TwoTeamScoreboards) { table?.SetActiveJoinButton(active && !tm); }
    }

    public override void OnPlayerClicked(Player player)
    {
        if (popUpMenu != null)
        {
            bool localPlayer = player != null && player.IsLocal;
            popUpMenu
                .SetTargetPlayer(player)
                .SetActive(true)
               .FilterMenuOptions(new ScoreboardPopUpMenuBase.MenuFilter()
                {
                    IsLocalPlayer = localPlayer,
                    IsBot = player == null,
                });
        }
    }

    public override bool RemoveUIBinding(PlayerScoreboardUIBase uiBinding)
    {
        if (uiBinding.isBotBinding)
        {
            if (cachedBotsUIBindings.ContainsKey(uiBinding.Bot))
            {
                Destroy(cachedBotsUIBindings[uiBinding.Bot].gameObject);
                cachedBotsUIBindings.Remove(uiBinding.Bot);
                return true;
            }
        }
        else
        {
            if (cachedUIBindings.ContainsKey(uiBinding.RealPlayer.ActorNumber))
            {
                Destroy(cachedUIBindings[uiBinding.RealPlayer.ActorNumber].gameObject);
                cachedUIBindings.Remove(uiBinding.RealPlayer.ActorNumber);
                return true;
            }
        }
        return false;
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!autoUpdate) return;

        if (cachedUIBindings.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(cachedUIBindings[otherPlayer.ActorNumber].gameObject);
            cachedUIBindings.Remove(otherPlayer.ActorNumber);
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!isShowingTables || !autoUpdate) return;
        UpdateTables();
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (!isShowingTables || !autoUpdate) return;
        UpdateTables();
    }
}