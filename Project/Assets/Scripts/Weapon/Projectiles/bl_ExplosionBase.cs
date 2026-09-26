/// <summary>
/// GFWK Explosion base class
/// Inherited from this your custom explosion script.
/// </summary>
public abstract class bl_ExplosionBase : bl_PhotonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public abstract void InitExplosion(BulletData bulletData, GFWKPlayer fromPlayer);

    /// <summary>
    ///
    /// </summary>
    /// <param name="radius"></param>
    public abstract void SetRadius(float radius);
}