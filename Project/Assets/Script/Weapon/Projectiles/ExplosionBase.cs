/// <summary>
/// Game 爆炸基类
/// 自定义爆炸脚本继承此类。
/// </summary>
public abstract class ExplosionBase : PhotonHelper
{
    public abstract void InitExplosion(BulletData bulletData, GamePlayer fromPlayer);

    public abstract void SetRadius(float radius);
}