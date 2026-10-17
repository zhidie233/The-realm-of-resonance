/// <summary>
/// Game Explosion base class
/// Inherited from this your custom explosion script.
/// </summary>
public abstract class ExplosionBase : PhotonHelper
{
    public abstract void InitExplosion(BulletData bulletData, GamePlayer fromPlayer);

    public abstract void SetRadius(float radius);
}