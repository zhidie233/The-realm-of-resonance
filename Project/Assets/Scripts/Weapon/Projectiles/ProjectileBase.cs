/// <summary>
/// Base class for all the weapon projectiles (bullets, grenades, etc...)
/// </summary>
public abstract class ProjectileBase : MonoBehaviourBase
{
    /// <summary>
    /// Initialize the projectile
    /// </summary>
    public abstract void InitProjectile(BulletData data);
}