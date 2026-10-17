#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

/// <summary>
/// 本作默认武器的基类。
/// GunController.cs 是写死的类，不便扩展，模块化武器系统还在做。
/// 目前要扩展、编写自己的武器脚本，走 CustomGunBase.cs。
/// </summary>
public abstract class GunBase : MonoBehaviourBase
{
    public enum FireType
    {
        Undefined,
        Auto,
        Single,
        Semi,
    }

    private GunData m_gunInfo = null;
    /// <summary>
    /// 武器信息
    /// </summary>
    public GunData Info
    {
        get
        {
            if (m_gunInfo == null)
            {
                m_gunInfo = GameConfig.Instance.GetWeapon(GunID);
            }
            return m_gunInfo;
        }
        set => m_gunInfo = value;
    }

    /// <summary>
    /// 武器索引
    /// </summary>
    public int GunID;

#if !ACTK_IS_HERE
    protected int _bulletLeft;
#else
    protected ObscuredInt _bulletLeft;
#endif
    /// <summary>
    /// 当前武器弹匣内剩余弹药
    /// </summary>
    public int bulletsLeft
    {
        get => _bulletLeft;
        set
        {
            _bulletLeft = value;
            GameEvents.DispatchLocalPlayerAmmoUpdate(_bulletLeft);
        }
    }

    /// <summary>
    /// 玩家当前是否正在瞄准？
    /// </summary>
    public bool isAiming
    {
        get;
        set;
    }

    /// <summary>
    /// 玩家当前是否正在用这把武器开火
    /// </summary>
    public bool isFiring
    {
        get;
        set;
    }

    public abstract FireType GetFireType();
}