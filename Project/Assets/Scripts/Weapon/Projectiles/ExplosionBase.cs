﻿/// <summary>
/// Game Explosion base class
/// Inherited from this your custom explosion script.
/// </summary>
public abstract class ExplosionBase : PhotonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public abstract void InitExplosion(BulletData bulletData, GamePlayer fromPlayer);

    /// <summary>
    ///
    /// </summary>
    /// <param name="radius"></param>
    public abstract void SetRadius(float radius);
}