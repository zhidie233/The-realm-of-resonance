using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class GameConfig : ScriptableObject
{
    #region Public members
    [Header("Game Settings")]
    [GameToggle] public bool directConnectMode = false;
    [GameToggle] public bool guardDuplicateSessions = true;
    [GameToggle] public bool lobbyChatEnabled = true;
    [GameToggle] public bool tracerRoundsVisual = false;
    [GameToggle] public bool DiscardWeaponOnDeath = true;
    [GameToggle] public bool selfExplosionDamage = true;
    [GameToggle] public bool fireWhileSprinting = true;
    [GameToggle] public bool autoHealthRecovery = true;
    [GameToggle] public bool allyHealthBars = true;
    [GameToggle] public bool allowTeamSwitch = false;
    [GameToggle] public bool showGore = true;
    [GameToggle] public bool watchIdlePlayers = false;
    [GameToggle] public bool hostCanRemovePlayers = true;
    [GameToggle] public bool AirSupplyCausesDamage = true;
    [GameToggle] public bool syncRemoteFootsteps = false;
    [GameToggle] public bool showNetGraph = false;
    [GameToggle] public bool persistNickname = true;
    [GameToggle] public bool ShowWeaponEquipment = true;
    [GameToggle] public bool prematchCountdownEnabled = true;
    [GameToggle] public bool reticleEnabled = true;
    [GameToggle] public bool hitDirectionIndicator = true;
    [GameToggle] public bool spawnHandsVisual = true;
    [GameToggle] public bool cameraSwayEnabled = true;
    [GameToggle] public bool deathIconsEnabled = true;
    [GameToggle] public bool fallDamageEnabled = true;
    [GameToggle] public bool simulatedBallistics = true;
    [GameToggle] public bool impactDecals = true;
    [GameToggle] public bool projectileTrailsEnabled = true;
    public AmmunitionType AmmoType = AmmunitionType.Bullets;
    public KillReportWeaponShowMode deathFeedWeaponDisplay = KillReportWeaponShowMode.WeaponIcon;
    public LobbyAccess lobbyAccess = LobbyAccess.ReadyRoom;
    public DeathCam.DeathCamType deathReplayMode = DeathCam.DeathCamType.WatchBody;
    public KillNotifier.KillNotice personalKillFeedDisplay = KillNotifier.KillNotice.Queqe;
    public FirearmInventory.SwapOnPickup swapOnPickupMode = FirearmInventory.SwapOnPickup.OnlyOnEmptySlots;
    public ObserverModeBase.LeaveSpectatorModeAction onLeaveSpectatorMode = ObserverModeBase.LeaveSpectatorModeAction.AllowSelectTeam;
    /// <summary>
    /// 决定机器人击杀如何计入真人玩家战绩
    /// 用于避免玩家靠刷机器人击杀来提升个人战绩。
    /// </summary>
    public BotKillPolicy botKillCredit = BotKillPolicy.SameAsRealPlayers;

    [Header("Rewards")]
    public ScoreScale matchScore;
    public WalletEntry wallet;
#if UNITY_2020_2_OR_NEWER
    [NonReorderable]
#endif
    [ScriptableDrawer] public List<GameCoin> gameCoins;

    [Header("Settings")]
    public string buildVersion = "1.0.0";
    public string visitorNameFormat = "Guest {0}";
    public string botTagFormat = "BOT {0}";
    [Range(0, 10)] public int spawnShieldTime = 5; // Time were the player can't receive damage after spawn.
    [Range(1, 60)] public int prematchTime = 7; // Initial countdown time (when countdown is enabled)
    [Range(1, 10)] public float respawnDelay = 5.0f; // Time that takes to respawn after the player die
    [Range(1, 100)] public int contactLimit = 25; // Maximum number of friends that players can add (and save)
    [Range(1, 10)] public int hitTestFrameStep = 2; // Each how many frames detect bullets hits? 1 = each frame
    public int endMatchDelay = 10; // The time that the game resume shown after the match finish.
    public float idleKickTime = 60; // Time limit that a player can be AFK in a match.
    public int teamSwitchLimit = 3; // Max number of times that a player can change of team per match.
    public int selfKillLimit = 3; // Max number of times that a player can suicide per match.
    public string MainMenuScene = "MainMenu";
    public string OnDisconnectScene = "MainMenu";
    public Color focusColor = Color.green; // Color to highlight the local player in different UI elements.

    [Header("Levels Manager")]
    [Reorderable]
    public List<MapInfo> SceneTable = new List<MapInfo>();

    [Header("Weapons")]
#if UNITY_2020_2_OR_NEWER
    [NonReorderable] // DO NOT reorder the weapons manually, it will mess up your whole setup!
#endif
    public List<FirearmData> WeaponTable = new List<FirearmData>();

    [Header("Default Settings")]
    [ScriptableDrawer, SerializeField] private RuntimeSettingsProfile defaultSettings;

    [Header("Game Modes Available"), Reorderable, FormerlySerializedAs("AllGameModes")]
    public List<GameModeSettings> gameModeTable = new List<GameModeSettings>();

    [Header("Teams")]
    public string Team1Name = "Team1";
    public Color Team1Color = Color.blue;
    [Space(5)]
    public string Team2Name = "Team2";
    public Color Team2Color = Color.green;

    [Header("Players")]
    public PlayerNetworkHandler Player1;
    public PlayerNetworkHandler Player2;

    [Header("Bots")]
    public AIBotCore BotTeam1;
    public AIBotCore BotTeam2;
    public AIBotCore BotTeam3;

    [ScriptableDrawer] public MouseLookSettings mouseLookSettings;

    [ScriptableDrawer] public WeaponSlotGuide weaponSlotGuide;

    [ScriptableDrawer, SerializeField] private TagsAndLayersSettings tagsAndLayersSettings;

    public SceneLoaderBase sceneLoader;

    [Header("Game Team")]
    public List<GameTeamInfo> GameTeam = new List<GameTeamInfo>();
    #endregion

    #region Private Members
    public GameTeamInfo CurrentTeamUser { get; set; } = null;
    [HideInInspector] public bool isChatOpen = false;
    [HideInInspector] public string appKey = string.Empty;
    [HideInInspector] public int channelId = 2;
    [HideInInspector] public string sessionTicket = "";
    private static NetworkWrapper PhotonGameInstance = null;
    public static bool isDataCached = false;
    private static bool isCaching = false;
    private static GameConfig m_instance;
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInit()
    {
        //检查场景里存在 Photon 对象实例
        if (PhotonGameInstance == null && Application.isPlaying)
        {
            PhotonGameInstance = NetworkWrapper.Instance;
            if (PhotonGameInstance == null)
            {
                try
                {
                    var pgo = new GameObject("PhotonGame");
                    PhotonGameInstance = pgo.AddComponent<NetworkWrapper>();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    private static void OnDataCached()
    {
        if (!CommonUtil.isMobile && Instance.mouseLookSettings.customCursor != null)
        {
            Cursor.SetCursor(Instance.mouseLookSettings.customCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    void OnDisable()
    {
        isDataCached = false;
        RolePrefix = "";
    }

    public void ResetInstance()
    {
        m_settingProfile = null;
        isDataCached = false;
    }

    #region Getters
    /// <summary>
    /// 按 ID 获取武器信息
    /// </summary>
    /// <param name="ID">武器 ID，即 GameData 中该武器信息的 indexOf</param>
    /// <returns></returns>
    public FirearmData GetWeapon(int ID)
    {
        if (ID < 0 || ID > WeaponTable.Count - 1)
            return WeaponTable[0];

        return WeaponTable[ID];
    }

    /// <summary>
    /// 按名称获取武器信息
    /// </summary>
    /// <param name="gunName"></param>
    /// <returns></returns>
    public int GetWeaponID(string gunName)
    {
        int id = -1;
        if (WeaponTable.Exists(x => x.Name == gunName))
        {
            id = WeaponTable.FindIndex(x => x.Name == gunName);
        }
        return id;
    }

    public string[] WeaponStringList()
    {
        return WeaponTable.Select(x => x.Name).ToList().ToArray();
    }

    public int CheckPlayerName(string pName)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (pName == GameTeam[i].UserName)
            {
                return 1;
            }
        }
        if (pName.Contains('[') || pName.Contains('{'))
        {
            return 2;
        }
        CurrentTeamUser = null;
        return 0;
    }

    public bool CheckPasswordUse(string PName, string Pass)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (PName == GameTeam[i].UserName)
            {
                if (Pass == GameTeam[i].Password)
                {
                    CurrentTeamUser = GameTeam[i];
                    return true;
                }
            }
        }
        return false;
    }

    private string _role = string.Empty;
    public string RolePrefix
    {
        get
        {
            if (CurrentTeamUser != null && !string.IsNullOrEmpty(CurrentTeamUser.UserName))
            {
                return string.Format("<color=#{1}>[{0}]</color>", CurrentTeamUser.m_Role.ToString(), ColorUtility.ToHtmlStringRGBA(CurrentTeamUser.m_Color));
            }
            else
            {
                return string.Empty;
            }
        }
        private set => _role = value;
    }

    private RuntimeSettingsProfile m_settingProfile;
    public RuntimeSettingsProfile RuntimeSettings
    {
        get
        {
            if (m_settingProfile == null) m_settingProfile = Instantiate(defaultSettings);
            return m_settingProfile;
        }
    }

    private static TagsAndLayersSettings m_tagsAndLayerSettings;
    public static TagsAndLayersSettings TagsAndLayerSettings
    {
        get
        {
            if (m_tagsAndLayerSettings == null) m_tagsAndLayerSettings = Instance.tagsAndLayersSettings;
            return m_tagsAndLayerSettings;
        }
    }

    public static GameConfig Instance
    {
        get
        {
            if (m_instance == null && !isCaching)
            {
                if (!isDataCached && Application.isPlaying)
                {
                    Debug.Log("GameData was cached synchronous, that could cause bottleneck on load, try caching it asynchronous with AsyncLoadData()");
                    isDataCached = true;
                    OnDataCached();
                }
                m_instance = Resources.Load("GameData", typeof(GameConfig)) as GameConfig;
            }
            return m_instance;
        }
    }
    #endregion

    /// <summary>
    /// 异步从 Resources 缓存 GameData，避免首次访问实例时卡顿和阻塞主线程
    /// </summary>
    /// <returns></returns>
    public static IEnumerator AsyncLoadData()
    {
        if (m_instance == null)
        {
            isCaching = true;
            ResourceRequest rr = Resources.LoadAsync("GameData", typeof(GameConfig));
            while (!rr.isDone) { yield return null; }
            m_instance = rr.asset as GameConfig;
            isCaching = false;
            OnDataCached();
        }
        isDataCached = true;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        for (int i = 0; i < SceneTable.Count; i++)
        {
            if (SceneTable[i].m_Scene == null) continue;
            SceneTable[i].RealSceneName = SceneTable[i].m_Scene.name;
        }
    }
#endif

    #region Local Classes
    [Serializable]
    public class ScoreScale
    {
        public int KillReward = 50;
        public int HeadshotExtraReward = 25;
        public int WinMatchReward = 100;
        [Tooltip("Per minute played")]
        public int PerMinuteScore = 3;

        public int GetPerMinuteScore(int time)
        {
            if (PerMinuteScore <= 0) return 0;
            return time * PerMinuteScore;
        }
    }

    [Serializable]
    public class WalletEntry
    {
        [ItemCoinID] public int XPCoin;
        [Tooltip("how much score/xp worth one coin")]
        public int CoinScoreValue = 1000;//how much score/xp worth one coin
        public int InitialCoins
        {
            get
            {
                var coin = GameCore.Coins.GetCoinData(XPCoin);
                return coin == null ? 0 : coin.InitialCoins;
            }
        }

        public void AddCoins(int newCoins, string endPoint = "") => GameCore.Coins.GetCoinData(XPCoin)?.Add(newCoins, endPoint);

        public int GetCoinsPerScore(int score)
        {
            if (score <= 0 || score < CoinScoreValue || CoinScoreValue <= 0) return 0;

            return score / CoinScoreValue;
        }
    }

    [Serializable]
    public class GameTeamInfo
    {
        public string UserName;
        public Role m_Role = Role.Moderator;
        public string Password;
        public Color m_Color;

        public enum Role
        {
            Admin = 0,
            Moderator = 1,
        }
    }
    #endregion
}