/// 玩家武器后坐力运动基类
/// 自定义后坐力脚本继承此类。
public abstract class KickbackBase : MonoBehaviourBase
{
    public struct KickbackData
    {
        public float Amount;
        public float Speed;

        public KickbackData(float amount)
        {
            Amount = amount;
            Speed = 2;
        }
    };

    public abstract void SetRecoil(KickbackData data);

}
