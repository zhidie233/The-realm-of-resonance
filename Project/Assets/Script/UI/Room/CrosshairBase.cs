/// <summary>
/// Base class for the Crosshair
/// Inherited from this your custom crosshair script.
/// </summary>
public abstract class CrosshairBase : MonoBehaviourBase
{
    /// <summary>
    /// If true = Freeze and block any action/call to the crosshair
    /// </summary>
    public bool Block
    {
        get;
        set;
    }

    /// <summary>
    /// Show/Hide the crosshair
    /// </summary>
    /// <param name="active"></param>
    public abstract void Show(bool active);

    public abstract void OnAim(bool aiming);

    /// <summary>
    /// Call when the local player hit a damageable object
    /// </summary>
    public abstract void OnHit();

    public abstract void OnFire();

    public abstract void SetupCrosshairForWeapon(GunType gunType);

    public abstract void SetupCrosshairForWeapon(GunController weapon);

    /// <summary>
    /// Allow the crosshair to fade in/out
    /// </summary>
    /// <param name="allow"></param>
    public abstract void AllowFade(bool allow);

    private static CrosshairBase m_instance;
    public static CrosshairBase Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<CrosshairBase>();
            }
            return m_instance;
        }
    }
}