using UnityEngine;
using Photon.Realtime;

// 包含全部伤害信息的游戏类
public class DamageData
{
    // 对 IDamageable 对象施加的伤害量
    public int Damage = 10;

    // 缓存的伤害来源名称。伤害并非总来自 <see cref="GameActor"/>（玩家或机器人），这是获取来源名称的替代方式。
    public string From;

    // 伤害来源
    public DamageCause Cause = DamageCause.Player;

    // 伤害的产生位置（子弹起点、爆炸点等）
    public Vector3 Direction { get; set; } = Vector3.zero;

    // 该伤害是否来自爆头？
    public bool isHeadShot { get; set; } = false;

    // 造成此伤害的武器 GunID；若伤害并非来自武器可忽略。
    public int GunID { get; set; } = 0;

    // 此伤害所属的网络玩家
    public Player Actor { get; set; }

    // 此伤害的 GameActor，可能是真实玩家或机器人
    public GamePlayer GameActor { get; set; }

    // 缓存的伤害来源网络 ViewID
    public int ActorViewID { get; set; }

    // 用该伤害数据创建哈希表
    // <returns></returns>
    public ExitGames.Client.Photon.Hashtable GetAsHashtable()
    {
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("d", Damage);
        data.Add("gi", GunID);
        data.Add("vi", ActorViewID);
        data.Add("c", Cause);
        data.Add("f", From);
        if (Direction != Vector3.zero)
            data.Add("dr", Direction);

        return data;
    }

    public DamageData(ExitGames.Client.Photon.Hashtable data)
    {
        Damage = (int)data["d"];
        GunID = (int)data["gi"];
        ActorViewID = (int)data["vi"];
        Cause = (DamageCause)data["c"];
        From = (string)data["f"];
        if (data.ContainsKey("dr")) Direction = (Vector3)data["dr"];

        GameActor = GameController.Instance.GetGameActor(ActorViewID);
    }

    public DamageData() { }
}

public struct GameHitData
{
    // 被命中物体的名称
    public string HitName;

    // 对命中物体施加的伤害量
    public int Damage;

    // 命中碰撞点
    public Vector3 HitPosition;

    // 命中物体的 Transform 引用。它可能被销毁，访问前需判空
    public Transform HitTransform;
}