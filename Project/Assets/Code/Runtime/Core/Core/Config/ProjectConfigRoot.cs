using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Multiplayer;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{

    [CreateAssetMenu(fileName = "ProjectConfigRoot", menuName = "项目/Config/Project Config Root")]
    public class ProjectConfigRoot : ScriptableObject
    {
        [Header("模块化配置")]
        public MatchRulesConfig rules;
        public WeaponCatalogConfig weapons;
        public ArenaCatalogConfig arenas;
        public FactionRosterConfig factions;
        public ResonanceSuiteConfig resonance;

        [Header("基础设置")]
        [ScriptableDrawer, SerializeField] private RuntimeSettingsProfile defaultSettings;
        [ScriptableDrawer] public MouseLookSettings mouseLookSettings;
        [ScriptableDrawer, SerializeField] private TagsAndLayersSettings tagsAndLayersSettings;

        public FactionRosterConfig.GameTeamInfo CurrentTeamUser { get; set; } = null;
        [HideInInspector] public bool isChatOpen = false;
        [HideInInspector] public string appKey = string.Empty;
        [HideInInspector] public int channelId = 2;
        [HideInInspector] public string sessionTicket = "";

        private static NetworkWrapper PhotonGameInstance = null;
        public static bool isDataCached = false;
        private static bool isCaching = false;
        private static ProjectConfigRoot m_instance;
        private static AsyncOperationHandle<ProjectConfigRoot> m_handle;
        public const string DataAddress = "ProjectConfigRoot";

    #if UNITY_EDITOR
        private const string EditorAssetPath = "Assets/Config/ProjectConfigRoot.asset";
    #endif

        // Domain forwarding properties
        public bool directConnectMode => rules.flow.directConnectMode;
        public bool guardDuplicateSessions => rules.flow.guardDuplicateSessions;
        public bool lobbyChatEnabled => rules.flow.lobbyChatEnabled;
        public bool tracerRoundsVisual => rules.combat.tracerRoundsVisual;
        public bool DiscardWeaponOnDeath => rules.combat.DiscardWeaponOnDeath;
        public bool selfExplosionDamage => rules.combat.selfExplosionDamage;
        public bool fireWhileSprinting => rules.combat.fireWhileSprinting;
        public bool autoHealthRecovery => rules.combat.autoHealthRecovery;
        public bool allyHealthBars => rules.feedback.allyHealthBars;
        public bool allowTeamSwitch => rules.flow.allowTeamSwitch;
        public bool showGore => rules.combat.showGore;
        public bool watchIdlePlayers => rules.flow.watchIdlePlayers;
        public bool AirSupplyCausesDamage => rules.combat.AirSupplyCausesDamage;
        public bool syncRemoteFootsteps => rules.feedback.syncRemoteFootsteps;
        public bool showNetGraph => rules.feedback.showNetGraph;
        public bool persistNickname => rules.flow.persistNickname;
        public bool ShowWeaponEquipment => rules.feedback.ShowWeaponEquipment;
        public bool prematchCountdownEnabled => rules.flow.prematchCountdownEnabled;
        public bool reticleEnabled => rules.feedback.reticleEnabled;
        public bool hitDirectionIndicator => rules.feedback.hitDirectionIndicator;
        public bool spawnHandsVisual => rules.feedback.spawnHandsVisual;
        public bool cameraSwayEnabled => rules.feedback.cameraSwayEnabled;
        public bool deathIconsEnabled => rules.feedback.deathIconsEnabled;
        public bool fallDamageEnabled => rules.combat.fallDamageEnabled;
        public bool simulatedBallistics => rules.combat.simulatedBallistics;
        public bool impactDecals => rules.combat.impactDecals;
        public bool projectileTrailsEnabled => rules.combat.projectileTrailsEnabled;
        public AmmunitionType AmmoType => rules.combat.AmmoType;
        public KillReportWeaponShowMode deathFeedWeaponDisplay => rules.feedback.deathFeedWeaponDisplay;
        public LobbyAccess lobbyAccess => rules.flow.lobbyAccess;
        public DeathCam.DeathCamType deathReplayMode => rules.flow.deathReplayMode;
        public KillNotifier.KillNotice personalKillFeedDisplay => rules.feedback.personalKillFeedDisplay;
        public FirearmInventory.SwapOnPickup swapOnPickupMode => rules.combat.swapOnPickupMode;
        public ObserverMode.LeaveSpectatorModeAction onLeaveSpectatorMode => rules.flow.onLeaveSpectatorMode;
        public BotKillPolicy botKillCredit => rules.flow.botKillCredit;
        public MatchRulesConfig.ScoreScale matchScore => rules.matchScore;
        public string buildVersion => rules.flow.buildVersion;
        public string visitorNameFormat => rules.flow.visitorNameFormat;
        public string botTagFormat => rules.flow.botTagFormat;
        public int spawnShieldTime => rules.flow.spawnShieldTime;
        public int prematchTime => rules.flow.prematchTime;
        public float respawnDelay => rules.flow.respawnDelay;
        public int hitTestFrameStep => rules.combat.hitTestFrameStep;
        public int endMatchDelay => rules.flow.endMatchDelay;
        public float idleKickTime => rules.flow.idleKickTime;
        public int teamSwitchLimit => rules.flow.teamSwitchLimit;
        public int suicideLimit => rules.flow.suicideLimit;

        public string MainMenuScene => arenas.MainMenuScene;
        public string OnDisconnectScene => arenas.OnDisconnectScene;
        public Color focusColor => arenas.focusColor;
        public SimpleSceneLoader sceneLoader => arenas.sceneLoader;
        public List<MapInfo> SceneTable => arenas.SceneTable;
        public List<GameModeSettings> gameModeTable => arenas.gameModeTable;

        public List<FirearmData> WeaponTable => weapons.WeaponTable;
        public WeaponSlotGuide weaponSlotGuide => weapons.weaponSlotGuide;

        public string Team1Name => factions.Team1Name;
        public Color Team1Color => factions.Team1Color;
        public string Team2Name => factions.Team2Name;
        public Color Team2Color => factions.Team2Color;
        public PlayerNetworkHandler Team1PlayerPrefab => factions.Team1PlayerPrefab;
        public PlayerNetworkHandler Team2PlayerPrefab => factions.Team2PlayerPrefab;
        public AIBotCore BotTeam1 => factions.BotTeam1;
        public AIBotCore BotTeam2 => factions.BotTeam2;
        public AIBotCore BotTeam3 => factions.BotTeam3;
        public List<FactionRosterConfig.GameTeamInfo> GameTeam => factions.GameTeam;

        public bool resonanceEnabled => resonance.resonanceEnabled;
        public ResonanceConfig resonanceConfig => resonance.resonanceConfig;
        public GravityConfig gravityConfig => resonance.gravityConfig;
        public GroundFireConfig groundFireConfig => resonance.groundFireConfig;
        public MeteoriteConfig meteoriteConfig => resonance.meteoriteConfig;
        public RobotConfig robotConfig => resonance.robotConfig;
        public SupplyConfig supplyConfig => resonance.supplyConfig;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeInit()
        {
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
                    catch (Exception) { }
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

        public FirearmData GetWeapon(int id) => weapons.GetWeapon(id);
        public int GetWeaponID(string gunName) => weapons.GetWeaponID(gunName);
        public string[] WeaponStringList() => weapons.WeaponStringList();

        public int CheckPlayerName(string pName)
        {
            for (int i = 0; i < GameTeam.Count; i++)
            {
                if (pName == GameTeam[i].UserName) return 1;
            }
            if (pName.Contains("[") || pName.Contains("{")) return 2;
            CurrentTeamUser = null;
            return 0;
        }

        public bool CheckPasswordUse(string PName, string Pass)
        {
            for (int i = 0; i < GameTeam.Count; i++)
            {
                if (PName == GameTeam[i].UserName && Pass == GameTeam[i].Password)
                {
                    CurrentTeamUser = GameTeam[i];
                    return true;
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
                return string.Empty;
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

        public static ProjectConfigRoot Instance
        {
            get
            {
                if (m_instance == null && !isCaching)
                {
    #if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        m_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectConfigRoot>(EditorAssetPath);
                        return m_instance;
                    }
    #endif
                    m_handle = Addressables.LoadAssetAsync<ProjectConfigRoot>(DataAddress);
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

        public static IEnumerator AsyncLoadData()
        {
            if (m_instance == null)
            {
                isCaching = true;
    #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    m_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectConfigRoot>(EditorAssetPath);
                    isCaching = false;
                    isDataCached = true;
                    yield break;
                }
    #endif
                m_handle = Addressables.LoadAssetAsync<ProjectConfigRoot>(DataAddress);
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
    }
}
