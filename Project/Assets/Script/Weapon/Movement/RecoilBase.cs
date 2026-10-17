/// <summary>
/// 玩家武器后坐力运动基类
/// 自定义后坐力脚本继承此类。
/// </summary>
public abstract class RecoilBase : MonoBehaviourBase
{
    public struct RecoilData
    {
        public float Amount;
        public float Speed;

        public RecoilData(float amount)
        {
            Amount = amount;
            Speed = 2;
        }
    };

    public abstract void SetRecoil(RecoilData data);

}