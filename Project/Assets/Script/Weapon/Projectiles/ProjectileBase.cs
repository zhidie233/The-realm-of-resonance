/// <summary>
/// 所有武器投掷物（子弹、手雷等）的基类
/// </summary>
public abstract class ProjectileBase : MonoBehaviourBase
{
    /// <summary>
    /// 初始化投掷物
    /// </summary>
    public abstract void InitProjectile(BulletData data);
}