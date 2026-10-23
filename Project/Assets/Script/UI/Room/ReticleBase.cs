/// 准星基类
/// 自定义准星脚本继承此类。
public abstract class ReticleBase : MonoBehaviourBase
{
    /// 为 true 时冻结并屏蔽对准星的所有操作/调用
    public bool Block
    {
        get;
        set;
    }
    /// 显示/隐藏准星
    public abstract void Show(bool active);

    public abstract void OnAim(bool aiming);
    /// 本地玩家命中可伤害对象时调用
    public abstract void OnHit();

    public abstract void OnFire();

    public abstract void SetupCrosshairForWeapon(FirearmType gunType);

    public abstract void SetupCrosshairForWeapon(FirearmController weapon);
    /// 允许准星淡入淡出
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
