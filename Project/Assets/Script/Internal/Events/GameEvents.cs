using UnityEngine;
using System;
using Game.Core.Motion;
using Game.Internal.Structures;
using Game.Internal.Scriptables;
using UnityEngine.Events;
using Photon.Pun;

public static class GameEvents
{
    [Serializable]public class UEvent : UnityEvent { }

    /// <summary>
    /// 本地玩家在游戏中拾取生命值时触发
    /// </summary>
    public delegate void ItemsPickUpEvent(int Amount);
    public static ItemsPickUpEvent onPickUpHealth;

    /// <summary>
    /// 本地玩家呼叫空投时触发
    /// </summary>
    public delegate void EventAirDrop(Vector3 m_position, int type);
    public static EventAirDrop onAirKit;

    /// <summary>
    /// 本地玩家在游戏中拾取弹药时触发
    /// </summary>
    /// <param name="bullets"></param>
    /// <param name="projectiles"></param>
    /// <param name="gunID"></param>
    public static Func<int, int, int, bool> onAmmoPickUp;

    /// <summary>
    /// 本地玩家在游戏中击杀或被击杀时触发。
    /// </summary>
    public delegate void LocalKillEvent(KillInfo killInfo);
    public static LocalKillEvent onLocalKill;

    /// <summary>
    /// 一局回合结束时触发
    /// </summary>
    public static Action onRoundEnd;

    /// <summary>
    /// 本地玩家下落着地时触发
    /// </summary>
    public static Action<float> onPlayerLand;

    /// <summary>
    /// 本地玩家在游戏中死亡时触发
    /// </summary>
    public static Action onLocalPlayerDeath;

    /// <summary>
    /// 远程玩家在游戏中死亡时触发
    /// </summary>
    public static Action<GamePlayer> onRemotePlayerDeath;
    public static void DispatchRemotePlayerDeath(GamePlayer player) => onRemotePlayerDeath?.Invoke(player);

    /// <summary>
    /// 本地玩家出生时触发
    /// </summary>
    public static Action onLocalPlayerSpawn;

    public delegate void LocalPlayerShakeEvent(ShakerPresent present, string key, float influence = 1);
    public static LocalPlayerShakeEvent onLocalPlayerShake;

    /// <summary>
    /// 本地玩家在游戏中更改一个或多个后处理效果选项时触发。
    /// </summary>
    public delegate void EffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb);
    public static EffectChange onEffectChange;

    /// <summary>
    /// 本地玩家拾取武器时触发
    /// </summary>
    public delegate void PickUpWeapon(GunPickUpData e);
    public static PickUpWeapon onPickUpGun;

    /// <summary>
    /// 本地玩家切换武器时触发
    /// </summary>
    /// <param name="GunID"></param>
    public delegate void EventChangeWeapon(int GunID);
    public static EventChangeWeapon onChangeWeapon;

    public struct PlayerChangeData
    {
        public string PlayerName;
        public GamePlayer GameActor;
        public bool IsAlive;
        public PhotonView NetworkView;
    }
    /// <summary>
    /// 非本地玩家出生或死亡时触发
    /// </summary>
    public static Action<PlayerChangeData> onRemoteActorChange;
    public static void DispatchRemoteActorChange(PlayerChangeData changeData) => onRemoteActorChange?.Invoke(changeData);

    /// <summary>
    /// 房间对局开始时触发
    /// </summary>
    public static Action onMatchStart;
    public static void CallOnMatchStart() { if (onMatchStart != null) { onMatchStart.Invoke(); } }

    /// <summary>
    /// 本地玩家更改瞄准状态时触发
    /// </summary>
    public static Action<bool> onLocalAimChanged;
    public static void DispatchLocalAimEvent(bool isAiming) { onLocalAimChanged?.Invoke(isAiming); }

    /// <summary>
    /// 本地玩家更改游戏内设置/选项时触发
    /// </summary>
    public static Action onGameSettingsChange;
    public static void DispatchGameSettingsChange() { onGameSettingsChange?.Invoke(); }

    /// <summary>
    /// 暂停菜单打开或隐藏时触发
    /// </summary>
    public static Action<bool> onGamePause;
    public static void DispatchGamePauseEvent(bool paused) { onGamePause?.Invoke(paused); }

    /// <summary>
    /// 机器人死亡时在所有客户端调用
    /// </summary>
    public static Action<string> onBotDeath;
    public static void EventBotDeath(string botName) => onBotDeath?.Invoke(botName);

    /// <summary>
    /// 所有机器人信息获取完成后调用一次
    /// </summary>
    public static Action onBotsInitializated;

    /// <summary>
    /// 本地玩家更改职业/配装时调用
    /// </summary>
    public static Action onPlayerClassChanged;
    public static void DispatchPlayerClassChange(PlayerClass newClass) => onPlayerClassChanged?.Invoke();

    /// <summary>
    /// 本地玩家使用武器开火时调用
    /// </summary>
    public static Action<int> onLocalPlayerFire;
    public static void DispatchLocalPlayerFire(int gunID) => onLocalPlayerFire?.Invoke(gunID);

    /// <summary>
    /// 本地玩家命中敌人时调用。
    /// </summary>
    public static Action<GameHitData> onLocalPlayerHitEnemy;
    public static void DispatchLocalPlayerHitEnemy(GameHitData hitData) => onLocalPlayerHitEnemy?.Invoke(hitData);

    /// <summary>
    /// 对局状态变化时调用（Waiting、Player、Countdown 等），会在所有客户端调用
    /// </summary>
    public static Action<MatchState> onMatchStateChanged;
    public static void DispatchMatchStateChange(MatchState state) => onMatchStateChanged?.Invoke(state);

    /// <summary>
    /// UIMask 变化时调用
    /// </summary>
    public static Action<RoomUILayers> onUIMaskChanged;
    public static void DispatchUIMaskChange(RoomUILayers layers) => onUIMaskChanged?.Invoke(layers);

    /// <summary>
    /// 发送本地通知时调用
    /// </summary>
    public static Action<GameLocalNotification> onLocalNotification;

    /// <summary>
    /// 玩家控制器状态变化时调用（从某状态 -> 另一状态）
    /// </summary>
    public static Action<PlayerState, PlayerState> onLocalPlayerStateChanged;
    public static void DispatchLocalPlayerStateChange(PlayerState from, PlayerState to) => onLocalPlayerStateChanged?.Invoke(from, to);

    /// <summary>
    /// 本地玩家所持武器的弹药数每次变化时调用
    /// </summary>
    public static Action<int> onLocalPlayerAmmoUpdate;
    public static void DispatchLocalPlayerAmmoUpdate(int newAmmocount) => onLocalPlayerAmmoUpdate?.Invoke(newAmmocount);

    /// <summary>
    /// 本地玩家受到敌人或其他来源伤害时调用。
    /// </summary>
    public static Action<int> onlocalPlayerReceiveDamage;
    public static void DispatchLocalPlayerReceiveDamage(int damage) => onlocalPlayerReceiveDamage?.Invoke(damage);

    /// <summary>
    /// 某个硬币的数值每次变化时调用。
    /// </summary>
    public static Action<GameCoin> onCoinUpdate;
    public static void DispatchCoinUpdate(GameCoin updatedCoin) => onCoinUpdate?.Invoke(updatedCoin);

    /// <summary>
    /// 房间属性重置时调用
    /// 在游戏或新回合开始时。
    /// 可用它重置任意自定义属性。
    /// </summary>
    public static Action onRoomPropertiesReset;
    public static void DispatchRoomPropertiesReset() => onRoomPropertiesReset?.Invoke();

    /// <summary>
    /// 本地玩家切换武器时调用
    /// </summary>
    public static void ChangeWeaponEvent(int GunID) => onChangeWeapon?.Invoke(GunID);

    /// <summary>
    /// 拾取医疗包时触发
    /// </summary>
    public static void DispatchPickUpHealth(int health) => onPickUpHealth?.Invoke(health);

    /// <summary>
    /// 呼叫新补给时触发
    /// </summary>
    public static void DispatchDropEvent(Vector3 t_position, int type) => onAirKit?.Invoke(t_position, type);

    /// <summary>
    /// 拾取弹药时触发
    /// </summary>
    /// <param name="bullets"></param>
    /// <param name="projectiles"></param>
    /// <param name="gunID"></param>
    public static bool OnAmmo(int bullets, int projectiles, int gunID) => onAmmoPickUp(bullets, projectiles, gunID);

    /// <summary>
    /// 击杀新玩家时调用
    /// </summary>
    public static void DispatchLocalKillEvent(KillInfo killInfo) => onLocalKill?.Invoke(killInfo);

    /// <summary>
    /// 房间完成一回合时调用
    /// </summary>
    public static void DispatchRoundEndEvent() => onRoundEnd?.Invoke();

    public static void DispatchPlayerLandEvent(float impactAmount) => onPlayerLand?.Invoke(impactAmount);

    public static void DispatchPlayerLocalDeathEvent() => onLocalPlayerDeath?.Invoke();

    public static void DispatchPlayerLocalSpawnEvent() => onLocalPlayerSpawn?.Invoke();

    public static void DoPlayerCameraShake(ShakerPresent present, string key, float influence = 1) => onLocalPlayerShake?.Invoke(present, key, influence);

    public static void PlayerLocalSpawnEvent(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
    {
        if (onEffectChange != null)
        {
            onEffectChange(chrab, anti, bloom, ssao, motionBlur);
        }
    }

    public static void PickUpGunEvent(GunPickUpData e)
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