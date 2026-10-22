using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class GameConfig : ScriptableObject
{
    #region Public members
    [Header("游戏规则")]
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
    /// 决定机器人击杀如何计入真人玩家战绩
    /// 用于避免玩家靠刷机器人击杀来提升个人战绩。
    public BotKillPolicy botKillCredit = BotKillPolicy.SameAsRealPlayers;
    [Header("奖励")]
    public ScoreScale matchScore;
    public WalletEntry wallet;
#if UNITY_2020_2_OR_NEWER
    [NonReorderable]
#endif
    [ScriptableDrawer] public List<GameCoin> gameCoins;
    [Header("参数")]
    public string buildVersion = "1.0.0";
    public string visitorNameFormat = "Guest {0}";
    public string botTagFormat = "BOT {0}";
    [Range(0, 10)] public int spawnShieldTime = 5;
    [Range(1, 60)] public int prematchTime = 7;
    [Range(1, 10)] public float respawnDelay = 5.0f;
    [Range(1, 100)] public int contactLimit = 25;
    [Range(1, 10)] public int hitTestFrameStep = 2;
    public int endMatchDelay = 10;
    public float idleKickTime = 60;
    public int teamSwitchLimit = 3;
    public int selfKillLimit = 3;
    public string MainMenuScene = "MainMenu";
    public string OnDisconnectScene = "MainMenu";
    public Color focusColor = Color.green;
    [Header("地图列表")]
    [Reorderable]
    public List<MapInfo> SceneTable = new List<MapInfo>();
    [Header("武器列表")]
#if UNITY_2020_2_OR_NEWER
    [NonReorderable]
#endif
    public List<FirearmData> WeaponTable = new List<FirearmData>();
    [Header("默认画质")]
    [ScriptableDrawer, SerializeField] private RuntimeSettingsProfile defaultSettings;
    [Header("可用模式"), Reorderable, FormerlySerializedAs("AllGameModes")]
    public List<GameModeSettings> gameModeTable = new List<GameModeSettings>();
    [Header("阵营")]
    public string Team1Name = "Team1";
    public Color Team1Color = Color.blue;
    [Space(5)]
    public string Team2Name = "Team2";
    public Color Team2Color = Color.green;
    [Header("玩家预制体")]
    public PlayerNetworkHandler Player1;
    public PlayerNetworkHandler Player2;
    [Header("机器人预制体")]
    public AIBotCore BotTeam1;
    public AIBotCore BotTeam2;
    public AIBotCore BotTeam3;

    [ScriptableDrawer] public MouseLookSettings mouseLookSettings;

    [ScriptableDrawer] public WeaponSlotGuide weaponSlotGuide;

    [ScriptableDrawer, SerializeField] private TagsAndLayersSettings tagsAndLayersSettings;

    public SceneLoaderBase sceneLoader;
    [Header("后台账号")]
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
    private static AsyncOperationHandle<GameConfig> m_handle;
    public const string DataAddress = "GameConfigData";

#if UNITY_EDITOR
    private const string EditorAssetPath = "Assets/Config/GameConfigData.asset";
#endif
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
        if (Instance == null || Instance.mouseLookSettings == null) return;
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
        m_tagsAndLayerSettings = null;
        isDataCached = false;
        if (m_handle.IsValid())
        {
            Addressables.Release(m_handle);
            m_handle = default;
        }
        m_instance = null;
    }
    #region Getters
    /// 按 ID 获取武器信息
    /// <param name="ID">武器 ID，即 GameData 中该武器信息的 indexOf</param>
    public FirearmData GetWeapon(int ID)
    {
        if (ID < 0 || ID > WeaponTable.Count - 1)
            return WeaponTable[0];
        return WeaponTable[ID];
    }
    /// 按名称获取武器信息
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
            if (m_tagsAndLayerSettings == null && Instance != null)
            {
                m_tagsAndLayerSettings = Instance.tagsAndLayersSettings;
            }
            return m_tagsAndLayerSettings;
        }
    }
    public static GameConfig Instance
    {
        get
        {
            if (m_instance == null && !isCaching)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    m_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(EditorAssetPath);
                    return m_instance;
                }
#endif
                m_handle = Addressables.LoadAssetAsync<GameConfig>(DataAddress);
                m_instance = m_handle.WaitForCompletion();
                if (m_instance != null)
                {
                    isDataCached = true;
                    OnDataCached();
                }
            }
            return m_instance;
        }
    }
    #endregion
    /// 异步从 Addressables 加载全局配置资产
    public static IEnumerator AsyncLoadData()
    {
        if (m_instance == null)
        {
            isCaching = true;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                m_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(EditorAssetPath);
                isCaching = false;
                isDataCached = true;
                yield break;
            }
#endif
            m_handle = Addressables.LoadAssetAsync<GameConfig>(DataAddress);
            yield return m_handle;
            if (m_handle.Status == AsyncOperationStatus.Succeeded)
            {
                m_instance = m_handle.Result;
            }
            else
            {
                Debug.LogError($"配置资产加载失败：{DataAddress}");
                m_instance = null;
                Addressables.Release(m_handle);
                m_handle = default;
            }
            isCaching = false;
            if (m_instance != null) OnDataCached();
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
        [Tooltip("每游玩一分钟的得分")]
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
        [Tooltip("多少分数或经验折算一枚金币")]
        public int CoinScoreValue = 1000;
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
