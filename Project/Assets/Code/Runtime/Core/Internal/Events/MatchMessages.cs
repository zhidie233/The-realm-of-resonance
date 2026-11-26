using Photon.Pun;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{

    public readonly struct HealthPickedUpEvent
    {
        public readonly int Amount;
        public HealthPickedUpEvent(int amount) { Amount = amount; }
    }

    public readonly struct AirSupplyRequestedEvent
    {
        public readonly Vector3 Position;
        public readonly int SupplyType;
        public AirSupplyRequestedEvent(Vector3 position, int supplyType) { Position = position; SupplyType = supplyType; }
    }

    public sealed class AmmoPickupQuery
    {
        public readonly int Bullets;
        public readonly int Projectiles;
        public readonly int WeaponId;
        public bool Consumed;
        public AmmoPickupQuery(int bullets, int projectiles, int weaponId)
        {
            Bullets = bullets;
            Projectiles = projectiles;
            WeaponId = weaponId;
        }
    }

    public readonly struct LocalKillEvent
    {
        public readonly KillRecord Record;
        public LocalKillEvent(KillRecord record) { Record = record; }
    }

    public readonly struct RoundEndedEvent { }
    public readonly struct PlayerLandedEvent { public readonly float ImpactAmount; public PlayerLandedEvent(float impactAmount) { ImpactAmount = impactAmount; } }
    public readonly struct LocalPlayerDiedEvent { }
    public readonly struct RemotePlayerDiedEvent { public readonly GamePlayer Actor; public RemotePlayerDiedEvent(GamePlayer actor) { Actor = actor; } }
    public readonly struct LocalPlayerSpawnedEvent { }
    public readonly struct ResonanceValueChangedEvent { public readonly float Value; public ResonanceValueChangedEvent(float value) { Value = value; } }

    public readonly struct CameraShakeEvent
    {
        public readonly ShakePreset Preset;
        public readonly string Key;
        public readonly float Influence;
        public CameraShakeEvent(ShakePreset preset, string key, float influence = 1f) { Preset = preset; Key = key; Influence = influence; }
    }

    public readonly struct PostEffectChangedEvent
    {
        public readonly bool ChromaticAberration;
        public readonly bool AntiAliasing;
        public readonly bool Bloom;
        public readonly bool Ssao;
        public readonly bool MotionBlur;
        public PostEffectChangedEvent(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
        {
            ChromaticAberration = chrab;
            AntiAliasing = anti;
            Bloom = bloom;
            Ssao = ssao;
            MotionBlur = motionBlur;
        }
    }

    public readonly struct WeaponPickedUpEvent { public readonly FirearmPickUpData Data; public WeaponPickedUpEvent(FirearmPickUpData data) { Data = data; } }
    public readonly struct WeaponSwitchedEvent { public readonly int WeaponId; public WeaponSwitchedEvent(int weaponId) { WeaponId = weaponId; } }

    public struct RemoteActorStateData
    {
        public string PlayerName;
        public GamePlayer GameActor;
        public bool IsAlive;
        public PhotonView NetworkView;
    }

    public readonly struct RemoteActorChangedEvent { public readonly RemoteActorStateData Data; public RemoteActorChangedEvent(RemoteActorStateData data) { Data = data; } }
    public readonly struct MatchStartedEvent { }
    public readonly struct LocalAimChangedEvent { public readonly bool IsAiming; public LocalAimChangedEvent(bool isAiming) { IsAiming = isAiming; } }
    public readonly struct GameSettingsChangedEvent { }
    public readonly struct GamePausedEvent { public readonly bool IsPaused; public GamePausedEvent(bool isPaused) { IsPaused = isPaused; } }
    public readonly struct BotDiedEvent { public readonly string BotName; public BotDiedEvent(string botName) { BotName = botName; } }
    public readonly struct BotsInitializedEvent { }
    public readonly struct PlayerClassChangedEvent { public readonly PlayerClass NewClass; public PlayerClassChangedEvent(PlayerClass newClass) { NewClass = newClass; } }
    public readonly struct LocalWeaponFiredEvent { public readonly int WeaponId; public LocalWeaponFiredEvent(int weaponId) { WeaponId = weaponId; } }
    public readonly struct LocalHitEnemyEvent { public readonly GameHitData HitData; public LocalHitEnemyEvent(GameHitData hitData) { HitData = hitData; } }
    public readonly struct MatchStateChangedEvent { public readonly MatchState State; public MatchStateChangedEvent(MatchState state) { State = state; } }
    public readonly struct UIMaskChangedEvent { public readonly RoomUILayers Layers; public UIMaskChangedEvent(RoomUILayers layers) { Layers = layers; } }
    public readonly struct LocalNotificationEvent { public readonly GameLocalNotification Notification; public LocalNotificationEvent(GameLocalNotification notification) { Notification = notification; } }
    public readonly struct LocalPlayerStateChangedEvent { public readonly PlayerState From; public readonly PlayerState To; public LocalPlayerStateChangedEvent(PlayerState from, PlayerState to) { From = from; To = to; } }
    public readonly struct LocalAmmoUpdatedEvent { public readonly int AmmoCount; public LocalAmmoUpdatedEvent(int ammoCount) { AmmoCount = ammoCount; } }
    public readonly struct LocalDamageReceivedEvent { public readonly int Damage; public LocalDamageReceivedEvent(int damage) { Damage = damage; } }
    public readonly struct RoomPropertiesResetEvent { }
}
