using UnityEngine;
using System;
using UnityEngine.Events;
using Photon.Pun;

public static class GameEvents
{
    [Serializable]public class UEvent : UnityEvent { }
    /// 本地玩家在游戏中拾取生命值时触发
    public delegate void ItemsPickUpEvent(int Amount);
    public static ItemsPickUpEvent onPickUpHealth;
    /// 本地玩家呼叫空投时触发
    public delegate void EventAirSupply(Vector3 m_position, int type);
    public static EventAirSupply onAirSupply;
    /// 本地玩家在游戏中拾取弹药时触发
    public static Func<int, int, int, bool> onAmmoPickUp;
    /// 本地玩家在游戏中击杀或被击杀时触发。
    public delegate void LocalKillEvent(KillRecord killInfo);
    public static LocalKillEvent onLocalKill;
    /// 一局回合结束时触发
    public static Action onRoundEnd;
    /// 本地玩家下落着地时触发
    public static Action<float> onPlayerLand;
    /// 本地玩家在游戏中死亡时触发
    public static Action onLocalPlayerDeath;
    /// 远程玩家在游戏中死亡时触发
    public static Action<GamePlayer> onRemotePlayerDeath;
    public static void DispatchRemotePlayerDeath(GamePlayer player) => onRemotePlayerDeath?.Invoke(player);
    /// 本地玩家出生时触发
    public static Action onLocalPlayerSpawn;
    /// 共振值发生变化时触发
    public static Action<float> onResonanceChanged;

    public delegate void LocalPlayerShakeEvent(ShakePreset present, string key, float influence = 1);
    public static LocalPlayerShakeEvent onLocalPlayerShake;
    /// 本地玩家在游戏中更改一个或多个后处理效果选项时触发。
    public delegate void EffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb);
    public static EffectChange onEffectChange;
    /// 本地玩家拾取武器时触发
    public delegate void PickUpWeapon(FirearmPickUpData e);
    public static PickUpWeapon onPickUpGun;
    /// 本地玩家切换武器时触发
    public delegate void EventChangeWeapon(int WeaponID);
    public static EventChangeWeapon onChangeWeapon;

    public struct PlayerChangeData
    {
        public string PlayerName;
        public GamePlayer GameActor;
        public bool IsAlive;
        public PhotonView NetworkView;
    }
    /// 非本地玩家出生或死亡时触发
    public static Action<PlayerChangeData> onRemoteActorChange;
    public static void DispatchRemoteActorChange(PlayerChangeData changeData) => onRemoteActorChange?.Invoke(changeData);
    /// 房间对局开始时触发
    public static Action onMatchStart;
    public static void CallOnMatchStart() { if (onMatchStart != null) { onMatchStart.Invoke(); } }
    /// 本地玩家更改瞄准状态时触发
    public static Action<bool> onLocalAimChanged;
    public static void DispatchLocalAimEvent(bool isAiming) { onLocalAimChanged?.Invoke(isAiming); }
    /// 本地玩家更改游戏内设置/选项时触发
    public static Action onGameSettingsChange;
    public static void DispatchGameSettingsChange() { onGameSettingsChange?.Invoke(); }
    /// 暂停菜单打开或隐藏时触发
    public static Action<bool> onGamePause;
    public static void DispatchGamePauseEvent(bool paused) { onGamePause?.Invoke(paused); }
    /// 机器人死亡时在所有客户端调用
    public static Action<string> onBotDeath;
    public static void EventBotDeath(string botName) => onBotDeath?.Invoke(botName);
    /// 所有机器人信息获取完成后调用一次
    public static Action onBotsInitializated;
    /// 本地玩家更改职业/配装时调用
    public static Action onPlayerClassChanged;
    public static void DispatchPlayerClassChange(PlayerClass newClass) => onPlayerClassChanged?.Invoke();
    /// 本地玩家使用武器开火时调用
    public static Action<int> onLocalPlayerFire;
    public static void DispatchLocalPlayerFire(int weaponID) => onLocalPlayerFire?.Invoke(weaponID);
    /// 本地玩家命中敌人时调用。
    public static Action<GameHitData> onLocalPlayerHitEnemy;
    public static void DispatchLocalPlayerHitEnemy(GameHitData hitData) => onLocalPlayerHitEnemy?.Invoke(hitData);
    /// 对局状态变化时调用（Waiting、Player、Countdown 等），会在所有客户端调用
    public static Action<MatchState> onMatchStateChanged;
    public static void DispatchMatchStateChange(MatchState state) => onMatchStateChanged?.Invoke(state);
    /// UIMask 变化时调用
    public static Action<RoomUILayers> onUIMaskChanged;
    public static void DispatchUIMaskChange(RoomUILayers layers) => onUIMaskChanged?.Invoke(layers);
    /// 发送本地通知时调用
    public static Action<GameLocalNotification> onLocalNotification;
    /// 玩家控制器状态变化时调用（从某状态 -> 另一状态）
    public static Action<PlayerState, PlayerState> onLocalPlayerStateChanged;
    public static void DispatchLocalPlayerStateChange(PlayerState from, PlayerState to) => onLocalPlayerStateChanged?.Invoke(from, to);
    /// 本地玩家所持武器的弹药数每次变化时调用
    public static Action<int> onLocalPlayerAmmoUpdate;
    public static void DispatchLocalPlayerAmmoUpdate(int newAmmocount) => onLocalPlayerAmmoUpdate?.Invoke(newAmmocount);
    /// 本地玩家受到敌人或其他来源伤害时调用。
    public static Action<int> onlocalPlayerReceiveDamage;
    public static void DispatchLocalPlayerReceiveDamage(int damage) => onlocalPlayerReceiveDamage?.Invoke(damage);
    /// 房间属性重置时调用
    /// 在游戏或新回合开始时。
    /// 可用它重置任意自定义属性。
    public static Action onRoomPropertiesReset;
    public static void DispatchRoomPropertiesReset() => onRoomPropertiesReset?.Invoke();
    /// 本地玩家切换武器时调用
    public static void ChangeWeaponEvent(int WeaponID) => onChangeWeapon?.Invoke(WeaponID);
    /// 拾取医疗包时触发
    public static void DispatchPickUpHealth(int health) => onPickUpHealth?.Invoke(health);
    /// 呼叫新补给时触发
    public static void DispatchDropEvent(Vector3 t_position, int type) => onAirSupply?.Invoke(t_position, type);
    /// 通知所有监听者共振值已更新
    public static void DispatchResonanceChanged(float value) => onResonanceChanged?.Invoke(value);
    /// 拾取弹药时触发
    public static bool OnAmmo(int bullets, int projectiles, int weaponID) => onAmmoPickUp(bullets, projectiles, weaponID);
    /// 击杀新玩家时调用
    public static void DispatchLocalKillEvent(KillRecord killInfo) => onLocalKill?.Invoke(killInfo);
    /// 房间完成一回合时调用
    public static void DispatchRoundEndEvent() => onRoundEnd?.Invoke();

    public static void DispatchPlayerLandEvent(float impactAmount) => onPlayerLand?.Invoke(impactAmount);

    public static void DispatchPlayerLocalDeathEvent() => onLocalPlayerDeath?.Invoke();

    public static void DispatchPlayerLocalSpawnEvent() => onLocalPlayerSpawn?.Invoke();

    public static void DoPlayerCameraShake(ShakePreset present, string key, float influence = 1) => onLocalPlayerShake?.Invoke(present, key, influence);

    public static void PlayerLocalSpawnEvent(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
    {
        if (onEffectChange != null)
        {
            onEffectChange(chrab, anti, bloom, ssao, motionBlur);
        }
    }

    public static void PickUpGunEvent(FirearmPickUpData e)
    {
        if (onPickUpGun != null)
            onPickUpGun(e);
    }

    public static void SetEffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb)
    {
        if (onEffectChange != null)
            onEffectChange(chrab, anti, bloom, ssao, motionb);
    }
}
