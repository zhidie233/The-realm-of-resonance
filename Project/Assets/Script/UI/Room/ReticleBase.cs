/// <summary>
/// 准星基类
/// 自定义准星脚本继承此类。
/// </summary>
public abstract class ReticleBase : MonoBehaviourBase
{
    /// <summary>
    /// 为 true 时冻结并屏蔽对准星的所有操作/调用
    /// </summary>
    public bool Block
    {
        get;
        set;
    }

    /// <summary>
    /// 显示/隐藏准星
    /// </summary>
    /// <param name="active"></param>
    public abstract void Show(bool active);

    public abstract void OnAim(bool aiming);

    /// <summary>
    /// 本地玩家命中可伤害对象时调用
    /// </summary>
    public abstract void OnHit();

    public abstract void OnFire();

    public abstract void SetupCrosshairForWeapon(FirearmType gunType);

    public abstract void SetupCrosshairForWeapon(FirearmController weapon);

    /// <summary>
    /// 允许准星淡入淡出
    /// </summary>
    /// <param name="allow"></param>
    public abstract void AllowFade(bool allow);

    private static ReticleBase m_instance;
    public static ReticleBase Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<ReticleBase>();
            }
            return m_instance;
        }
    }
}