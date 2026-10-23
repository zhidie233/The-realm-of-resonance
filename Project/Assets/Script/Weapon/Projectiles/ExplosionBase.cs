/// Game 爆炸基类
/// 自定义爆炸脚本继承此类。
public abstract class ExplosionBase : NetworkHelper
{
    public abstract void InitExplosion(ShotData bulletData, GamePlayer fromPlayer);

    public abstract void SetRadius(float radius);
}
