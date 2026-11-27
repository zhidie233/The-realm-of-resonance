using System;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.UI;
namespace Resonance.Core
{

    [CreateAssetMenu(fileName = "MatchRules", menuName = "项目/Config/Match Rules")]
    public class MatchRulesConfig : ScriptableObject
    {
        [Serializable]
        public class CombatRules
        {
            [GameToggle] public bool tracerRoundsVisual = false;
            [GameToggle] public bool DiscardWeaponOnDeath = true;
            [GameToggle] public bool selfExplosionDamage = true;
            [GameToggle] public bool fireWhileSprinting = true;
            [GameToggle] public bool autoHealthRecovery = true;
            [GameToggle] public bool showGore = true;
            [GameToggle] public bool AirSupplyCausesDamage = true;
            [GameToggle] public bool fallDamageEnabled = true;
            [GameToggle] public bool simulatedBallistics = true;
            [GameToggle] public bool impactDecals = true;
            [GameToggle] public bool projectileTrailsEnabled = true;
            public AmmunitionType AmmoType = AmmunitionType.Bullets;
            public FirearmInventory.SwapOnPickup swapOnPickupMode = FirearmInventory.SwapOnPickup.OnlyOnEmptySlots;
            [Range(1, 10)] public int hitTestFrameStep = 2;
        }

        [Serializable]
        public class MatchFlowRules
        {
            [GameToggle] public bool directConnectMode = false;
            [GameToggle] public bool guardDuplicateSessions = true;
            [GameToggle] public bool lobbyChatEnabled = true;
            [GameToggle] public bool allowTeamSwitch = false;
            [GameToggle] public bool watchIdlePlayers = false;
            [GameToggle] public bool prematchCountdownEnabled = true;
            [GameToggle] public bool persistNickname = true;
            public LobbyAccess lobbyAccess = LobbyAccess.ReadyRoom;
            public DeathCam.DeathCamType deathReplayMode = DeathCam.DeathCamType.WatchBody;
            public ObserverMode.LeaveSpectatorModeAction onLeaveSpectatorMode = ObserverMode.LeaveSpectatorModeAction.AllowSelectTeam;
            public BotKillPolicy botKillCredit = BotKillPolicy.SameAsRealPlayers;
            public string buildVersion = "1.0.0";
            public string visitorNameFormat = "Guest {0}";
            public string botTagFormat = "BOT {0}";
            [Range(0, 10)] public int spawnShieldTime = 5;
            [Range(1, 60)] public int prematchTime = 7;
            [Range(1, 10)] public float respawnDelay = 5.0f;
            public int endMatchDelay = 10;
            public float idleKickTime = 60;
            public int teamSwitchLimit = 3;
            public int suicideLimit = 4;
        }

        [Serializable]
        public class FeedbackRules
        {
            [GameToggle] public bool allyHealthBars = true;
            [GameToggle] public bool syncRemoteFootsteps = false;
            [GameToggle] public bool showNetGraph = false;
            [GameToggle] public bool ShowWeaponEquipment = true;
            [GameToggle] public bool reticleEnabled = true;
            [GameToggle] public bool hitDirectionIndicator = true;
            [GameToggle] public bool spawnHandsVisual = true;
            [GameToggle] public bool cameraSwayEnabled = true;
            [GameToggle] public bool deathIconsEnabled = true;
            public KillReportWeaponShowMode deathFeedWeaponDisplay = KillReportWeaponShowMode.WeaponIcon;
            public KillNotifier.KillNotice personalKillFeedDisplay = KillNotifier.KillNotice.Queue;
        }

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

        public CombatRules combat = new CombatRules();
        public MatchFlowRules flow = new MatchFlowRules();
        public FeedbackRules feedback = new FeedbackRules();
        public ScoreScale matchScore = new ScoreScale();
    }
}
