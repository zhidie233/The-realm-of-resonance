/// <summary>
/// 玩家和机器人的生命值管理器基类脚本
/// </summary>
public abstract class PlayerHealthManagerBase : MonoBehaviourBase
{

    private bool damageEnable = true;
    public bool DamageEnabled
    {
        get => damageEnable;
        set => damageEnable = value;
    }

    /// <summary>
    /// 负责处理本地玩家造成的伤害
    /// 它应把伤害同步给其他所有玩家。
    /// </summary>
    /// <param name="damageData"></param>
    public abstract void DoDamage(DamageData damageData);

    public abstract void DoFallDamage(int damage);

    /// <summary>
    /// 让玩家立即死亡
    /// </summary>
    public abstract bool Suicide();

    /// <summary>
    /// 返回玩家当前生命值
    /// </summary>
    /// <returns></returns>
    public abstract int GetHealth();

    public abstract int GetMaxHealth();

    /// <summary>
    /// 增加或替换当前生命值
    /// 只应由网络实体的拥有者或主客户端调用
    /// </summary>
    /// <param name="replace">为 true 时用该数量替换当前生命值，否则在其上增加</param>
    public abstract void SetHealth(int amount, bool replace = false);

    public abstract void DoRepetingDamage(RepetingDamageInfo info);

    public abstract void CancelRepetingDamage();

    public abstract void DestroyEntity();

    public abstract bool IsDeath();

    public class RepetingDamageInfo
    {
        public int Damage;
        public float Rate;
        public DamageData DamageData;
    }
}