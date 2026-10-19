using UnityEngine;

// 包含一颗已实例化子弹的全部信息
public class ShotData 
{

    // 发射这颗子弹的武器名称。
    public string WeaponName;

    // 这颗子弹造成的基础伤害
    public float Damage;

    // 这颗子弹的发射位置。
    public Vector3 Position;

    // 命中物体带 RigidBody 时施加的力
    public float ImpactForce;

    // 作用于投射方向的子弹散布
    public Vector3 Inaccuracity = Vector3.zero;

    // 子弹速度
    public float Speed;

    // 这颗子弹未命中任何物体时的最大飞行距离。
    public float Range;

    // 子弹下坠量
    public float DropFactor;

    // 发射这颗子弹的武器 GunID
    public int WeaponID;

    // 这颗子弹是否由远程玩家创建？
    public bool isNetwork;

    // 这颗子弹是否由真正的本地玩家创建。与 <see cref="isNetwork"/> 的区别：机器人创建子弹时 IsNetwork 可能为 false（即不是远程子弹），而此值确保子弹由真正的本地玩家创建。
    public bool IsLocalPlayer;

    // 缓存的 PhotonView
    public int ActorViewID { get; set; }

    // 创建这颗子弹的 GameActor
    public GamePlayer GameActor { get; set; }

    // 创建子弹数据并从 <see cref="DamageInfo"/> 取信息
    // <param name="data"></param>
    public ShotData(DamageInfo data)
    {
        GameActor = data.GameActor;
        Damage = data.Damage;
        ActorViewID = data.ActorViewID;
        WeaponID = data.GunID;
        Position = data.Direction;
    }

    // 计算并设置弹道散布向量
    // <param name="spreadBase"></param>
    // <param name="maxSpread"></param>
    // <returns></returns>
    public ShotData SetInaccuracity(float spreadBase, float maxSpread)
    {
        Inaccuracity = new Vector3(Random.Range(-maxSpread, maxSpread) * spreadBase, Random.Range(-maxSpread, maxSpread) * spreadBase, 1);
        return this;
    }

    public ShotData()
    {
    }
}