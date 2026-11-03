using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using System.Linq;
using TMPro;

public class PlayerScoreboard : MonoBehaviourBase
{
    /// 不想显示记分板时，把此属性设为 false 即可
    private bool blockScoreboards = false;
    public bool BlockScoreboards
    {
        get { return blockScoreboards; }
        set
        {
            if (value == true)
            {
                SetActive(false);
            }
            blockScoreboards = value;
        }
    }

    private static PlayerScoreboard _instance;
    public static PlayerScoreboard Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<PlayerScoreboard>(); }
            if (_instance == null) { _instance = UIReferences.Instance.playerScoreboards; }
            return _instance;
        }
    }

    [Header("Settings")]
    [GameToggle] public bool updateOnEnable = true;
    [GameToggle] public bool autoUpdate = true;

    [Header("References")]
    public PlayerScoreboardTable[] TwoTeamScoreboards;
    public PlayerScoreboardTable OneTeamScoreboard;
    public ScoreboardPopUpMenu popUpMenu;
    public GameObject playerscoreboardUIBinding;
    public TextMeshProUGUI SpectatorsCountText;

    Dictionary<int, PlayerScoreboardUI> cachedUIBindings = new Dictionary<int, PlayerScoreboardUI>();
    Dictionary<GameBotProperties, PlayerScoreboardUI> cachedBotsUIBindings = new Dictionary<GameBotProperties, PlayerScoreboardUI>();
    bool botsScoreInstance = false;
    private List<PlayerScoreboardUI> cachePlayerScoreboardSorted = new List<PlayerScoreboardUI>();
    private List<PlayerScoreboardUI> cachePlayerScoreboardSorted2 = new List<PlayerScoreboardUI>();

    public bool isShowingTables { get; set; } = false;

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
                    PlayerScoreboardUI script = null;
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
            else { spectators++; }
        }
        UpdateBotScoreboard();
        if (SpectatorsCountText != null)
        {
            SpectatorsCountText.text = string.Format(GameTexts.Spectators, spectators);
        }
        SortScoreboard();
    }
    /// 更新记分板中的玩家和机器人字段
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
        PlayerScoreboardUI script = null;
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
    /// 强制刷新记分板信息
    public void ForceUpdateAll()
    {
        List<PlayerScoreboardUI> ailist = cachedBotsUIBindings.Values.ToList();
        foreach (var item in ailist)
        {
            item?.Refresh();
        }
        UpdateTables();
    }
    /// 按得分对记分板玩家排序
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
            List<PlayerScoreboardUI> all = new List<PlayerScoreboardUI>();
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

    public void SetActiveByTeamMode(bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm);
        foreach (var table in TwoTeamScoreboards) { table?.SetActive(!tm); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = true;
        ForceUpdateAll();
    }

    public void SetActive(bool active, bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm && active);
        foreach (var table in TwoTeamScoreboards) { table?.SetActive(!tm && active); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = active;
        if (active) { ForceUpdateAll(); }
    }

    public void SetActiveByGameState(bool active)
    {
        SetActive(active, (!GameController.Instance.FirstSpawnDone && GameController.Joined));
    }

    public void ResetJoinButtons()
    {
        OneTeamScoreboard?.ResetJoinButton();
        foreach (var table in TwoTeamScoreboards) { table?.ResetJoinButton(); }
    }

    public void SetActiveJoinButtons(bool active)
    {
        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActiveJoinButton(active && tm);
        foreach (var table in TwoTeamScoreboards) { table?.SetActiveJoinButton(active && !tm); }
    }

    public void OnPlayerClicked(Player player)
    {
        if (popUpMenu != null)
        {
            bool localPlayer = player != null && player.IsLocal;
            popUpMenu
                .SetTargetPlayer(player)
                .SetActive(true)
               .FilterMenuOptions(new ScoreboardPopUpMenu.MenuFilter()
                {
                    IsLocalPlayer = localPlayer,
                    IsBot = player == null,
                });
        }
    }

    public bool RemoveUIBinding(PlayerScoreboardUI uiBinding)
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
