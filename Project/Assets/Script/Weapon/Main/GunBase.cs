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
    /// Weapon information
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
    /// Weapon Index
    /// </summary>
    public int GunID;

#if !ACTK_IS_HERE
    protected int _bulletLeft;
#else
    protected ObscuredInt _bulletLeft;
#endif
    /// <summary>
    /// Current weapon bullet left in the clip
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
    /// Is the player currently aiming?
    /// </summary>
    public bool isAiming
    {
        get;
        set;
    }

    /// <summary>
    /// Is the player currently firing with this weapon
    /// </summary>
    public bool isFiring
    {
        get;
        set;
    }

    public abstract FireType GetFireType();
}