/// 本作默认武器的基类。
/// FirearmController.cs 是写死的类，不便扩展，模块化武器系统还在做。
/// 目前要扩展、编写自己的武器脚本，走 CustomFirearmBase.cs。
public abstract class FirearmBase : MonoBehaviourBase
{
    public enum FireType
    {
        Undefined,
        Auto,
        Single,
        Semi,
    }

    private FirearmData m_gunInfo = null;
    /// 武器信息
    public FirearmData Info
    {
        get
        {
            if (m_gunInfo == null)
            {
                m_gunInfo = GameConfig.Instance.GetWeapon(WeaponID);
            }
            return m_gunInfo;
        }
        set => m_gunInfo = value;
    }
    /// 武器索引
    public int WeaponID;

    protected int _bulletLeft;
    /// 当前武器弹匣内剩余弹药
    public int bulletsLeft
    {
        get => _bulletLeft;
        set
        {
            _bulletLeft = value;
            GameEvents.DispatchLocalPlayerAmmoUpdate(_bulletLeft);
        }
    }
    /// 玩家当前是否正在瞄准？
    public bool isAiming
    {
        get;
        set;
    }
    /// 玩家当前是否正在用这把武器开火
    public bool isFiring
    {
        get;
        set;
    }

    public abstract FireType GetFireType();
}
