using UnityEngine;

public abstract class PlayerAnimationsBase : MonoBehaviourBase
{
    [SerializeField] private Animator m_animator = null;
    public Animator Animator
    {
        get => m_animator;
        set => m_animator = value;
    }

    public PlayerState BodyState
    {
        get;
        set;
    } = PlayerState.Idle;

    public PlayerFPState FPState
    {
        get;
        set;
    } = PlayerFPState.Idle;

    /// <summary>
    /// 该玩家是否接触地面？
    /// 该值应由 PhotonNetworkWrapper.cs 提供
    /// </summary>
    public bool IsGrounded
    {
        get;
        set;
    }

    /// <summary>
    /// 该玩家的速度
    /// </summary>
    public Vector3 Velocity
    {
        get;
        set;
    } = Vector3.zero;

    /// <summary>
    /// 该玩家的本地速度
    /// </summary>
    public Vector3 LocalVelocity
    {
        get;
        set;
    } = Vector3.zero;

    /// <summary>
    /// 玩家切换武器时调用
    /// </summary>
    public abstract void SetNetworkGun(GunType weaponType, RemoteGun networkGun);

    /// <summary>
    /// 播放指定枪械类型的开火动画。
    /// </summary>
    /// <param name="gunType"></param>
    public abstract void PlayFireAnimation(GunType gunType);

    /// <summary>
    /// 更新玩家动画器参数。
    /// </summary>
    public abstract void UpdateAnimatorParameters();

    /// <summary>
    /// 玩家被敌人击中时调用。
    /// </summary>
    public abstract void OnGetHit();

    /// <summary>
    /// 收起 / 卸下武器
    /// </summary>
    public abstract void BlockWeapons(int blockType);
}