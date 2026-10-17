using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameEditor;
using UnityEngine.Serialization;
using Game.Runtime.Settings;
using Game.Internal.Structures;
using Game.Internal.Scriptables;
using Game.Internal.BaseClass;
using Game.Runtime.AI;

public class GameConfig : ScriptableObject
{
    #region Public members
    [Header("Game Settings")]
    [GameToggle] public bool offlineMode = false;
    [GameToggle] public bool verifySingleSession = true;
    [GameToggle] public bool UseLobbyChat = true;
    [GameToggle] public bool UseVoiceChat = false;
    [GameToggle] public bool BulletTracer = false;
    [GameToggle] public bool DropGunOnDeath = true;
    [GameToggle] public bool SelfGrenadeDamage = true;
    [GameToggle] public bool CanFireWhileRunning = true;
    [GameToggle] public bool HealthRegeneration = true;
    [GameToggle] public bool ShowTeamMateHealthBar = true;
    [GameToggle] public bool CanChangeTeam = false;
    [GameToggle] public bool ShowBlood = true;
    [GameToggle] public bool DetectAFK = false;
    [GameToggle] public bool MasterCanKickPlayers = true;
    [GameToggle] public bool ArriveKitsCauseDamage = true;
    [GameToggle] public bool CalculateNetworkFootSteps = false;
    [GameToggle] public bool ShowNetworkStats = false;
    [GameToggle] public bool RememberPlayerName = true;
    [GameToggle] public bool ShowWeaponLoadout = true;
    [GameToggle] public bool useCountDownOnStart = true;
    [GameToggle] public bool showCrosshair = true;
    [GameToggle] public bool showDamageIndicator = true;
    [GameToggle] public bool doSpawnHandMeshEffect = true;
    [GameToggle] public bool playerCameraWiggle = true;
    [GameToggle] public bool showDeathIcons = true;
    [GameToggle] public bool allowFallDamage = true;
    [GameToggle] public bool realisticBullets = true;
    [GameToggle] public bool bulletDecals = true;
    [GameToggle] public bool showProjectilesTrails = true;
    public AmmunitionType AmmoType = AmmunitionType.Bullets;
    public KillFeedWeaponShowMode killFeedWeaponShowMode = KillFeedWeaponShowMode.WeaponIcon;
    public LobbyJoinMethod lobbyJoinMethod = LobbyJoinMethod.WaitingRoom;
    public KillCam.KillCameraType killCameraType = KillCam.KillCameraType.ObserveDeath;
    public LocalKillNotifier.LocalKillDisplay localKillsShowMode = LocalKillNotifier.LocalKillDisplay.Queqe;
    public GunInventory.AutoChangeOnPickup switchToPickupWeapon = GunInventory.AutoChangeOnPickup.OnlyOnEmptySlots;
    public SpectatorModeBase.LeaveSpectatorModeAction onLeaveSpectatorMode = SpectatorModeBase.LeaveSpectatorModeAction.AllowSelectTeam;
    /// <summary>
    /// Determine how the bot eliminations are considered for real player stats
    /// If you want to avoid players being able to level up exploiting bots eliminations to increase their personal stats.
    /// </summary>
    public BotKillConsideration howConsiderBotsEliminations = BotKillConsideration.SameAsRealPlayers;

    [Header("Rewards")]
    public ScoreRewards ScoreReward;
    public VirtualCoin VirtualCoins;
#if UNITY_2020_2_OR_NEWER
    [NonReorderable]
#endif
    [ScriptableDrawer] public List<GameCoin> gameCoins;

    [Header("Settings")]
    public string GameVersion = "1.0.0";
    public string guestNameFormat = "Guest {0}";
    public string botsNameFormat = "BOT {0}";
    [Range(0, 10)] public int SpawnProtectedTime = 5; // Time were the player can't receive damage after spawn.
    [Range(1, 60)] public int CountDownTime = 7; // Initial countdown time (when countdown is enabled)
    [Range(1, 10)] public float PlayerRespawnTime = 5.0f; // Time that takes to respawn after the player die
    [Range(1, 100)] public int MaxFriendsAdded = 25; // Maximum number of friends that players can add (and save)
    [Range(1, 10)] public int BulletHitDetectionStep = 2; // Each how many frames detect bullets hits? 1 = each frame
    public int afterMatchCountdown = 10; // The time that the game resume shown after the match finish.
    public float AFKTimeLimit = 60; // Time limit that a player can be AFK in a match.
    public int MaxChangeTeamTimes = 3; // Max number of times that a player can change of team per match.
    public int maxSuicideAttempts = 3; // Max number of times that a player can suicide per match.
    public string MainMenuScene = "MainMenu";
    public string OnDisconnectScene = "MainMenu";
    public Color highLightColor = Color.green; // Color to highlight the local player in different UI elements.

    [Header("Levels Manager")]
    [Reorderable]
    public List<MapInfo> AllScenes = new List<MapInfo>();

    [Header("Weapons")]
#if UNITY_2020_2_OR_NEWER
    [NonReorderable] // DO NOT reorder the weapons manually, it will mess up your whole setup!
#endif
    public List<GunData> AllWeapons = new List<GunData>();

    [Header("Default Settings")]
    [ScriptableDrawer, SerializeField] private RuntimeSettingsProfile defaultSettings;

    [Header("Game Modes Available"), Reorderable, FormerlySerializedAs("AllGameModes")]
    public List<GameModeSettings> gameModes = new List<GameModeSettings>();

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
    public AIShooter BotTeam1;
    public AIShooter BotTeam2;
    public AIShooter BotTeam3;

    [ScriptableDrawer] public MouseLookSettings mouseLookSettings;

    [ScriptableDrawer] public WeaponSlotRuler weaponSlotRuler;

    [ScriptableDrawer, SerializeField] private TagsAndLayersSettings tagsAndLayersSettings;

    public SceneLoaderBase sceneLoader;

    [Header("Game Team")]
    public List<GameTeamInfo> GameTeam = new List<GameTeamInfo>();
    #endregion

    #region Private Members
    public GameTeamInfo CurrentTeamUser { get; set; } = null;
    [HideInInspector] public bool isChating = false;
    [HideInInspector] public string _ProductKey = string.Empty;
    [HideInInspector] public int _DistributionType = 2;
    [HideInInspector] public string _SessionToken = "";
    private static PhotonNetworkWrapper PhotonGameInstance = null;
    public static bool isDataCached = false;
    private static bool isCaching = false;
    private static GameConfig m_instance;
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInit()
    {
        //check that there's an instance of the Photon object in scene
        if (PhotonGameInstance == null && Application.isPlaying)
        {
            PhotonGameInstance = PhotonNetworkWrapper.Instance;
            if (PhotonGameInstance == null)
            {
                try
                {
                    var pgo = new GameObject("PhotonGame");
                    PhotonGameInstance = pgo.AddComponent<PhotonNetworkWrapper>();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    private static void OnDataCached()
    {
        if (!UtilityHelper.isMobile && Instance.mouseLookSettings.customCursor != null)
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
    /// Get a weapon info by they ID
    /// </summary>
    /// <param name="ID">the id of the weapon, this ID is the indexOf the weapon info in GameData</param>
    /// <returns></returns>
    public GunData GetWeapon(int ID)
    {
        if (ID < 0 || ID > AllWeapons.Count - 1)
            return AllWeapons[0];

        return AllWeapons[ID];
    }

    /// <summary>
    /// Get a weapon info by they Name
    /// </summary>
    /// <param name="gunName"></param>
    /// <returns></returns>
    public int GetWeaponID(string gunName)
    {
        int id = -1;
        if (AllWeapons.Exists(x => x.Name == gunName))
        {
            id = AllWeapons.FindIndex(x => x.Name == gunName);
        }
        return id;
    }

    public string[] AllWeaponStringList()
    {
        return AllWeapons.Select(x => x.Name).ToList().ToArray();
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
    /// cache the GameData from Resources asynchronous to avoid overhead and freeze the main thread the first time we access to the instance
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
        for (int i = 0; i < AllScenes.Count; i++)
        {
            if (AllScenes[i].m_Scene == null) continue;
            AllScenes[i].RealSceneName = AllScenes[i].m_Scene.name;
        }
    }
#endif

    #region Local Classes
    [Serializable]
    public class ScoreRewards
    {
        public int ScorePerKill = 50;
        public int ScorePerHeadShot = 25;
        public int ScoreForWinMatch = 100;
        [Tooltip("Per minute played")]
        public int ScorePerTimePlayed = 3;

        public int GetScorePerTimePlayed(int time)
        {
            if (ScorePerTimePlayed <= 0) return 0;
            return time * ScorePerTimePlayed;
        }
    }

    [Serializable]
    public class VirtualCoin
    {
        [GameCoinID] public int XPCoin;
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