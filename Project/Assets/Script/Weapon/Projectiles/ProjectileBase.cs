/// 所有武器投掷物（子弹、手雷等）的基类
public abstract class ProjectileBase : MonoBehaviourBase
{
    /// 初始化投掷物
    public abstract void InitProjectile(ShotData data);
}
