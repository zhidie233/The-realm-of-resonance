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
    /// 该玩家是否接触地面？
    /// 该值应由 NetworkWrapper.cs 提供
    public bool IsGrounded
    {
        get;
        set;
    }
    /// 该玩家的速度
    public Vector3 Velocity
    {
        get;
        set;
    } = Vector3.zero;
    /// 该玩家的本地速度
    public Vector3 LocalVelocity
    {
        get;
        set;
    } = Vector3.zero;
    /// 玩家切换武器时调用
    public abstract void SetNetworkGun(FirearmType weaponType, NetworkFirearm networkGun);
    /// 播放指定枪械类型的开火动画。
    public abstract void PlayFireAnimation(FirearmType gunType);
    /// 更新玩家动画器参数。
    public abstract void UpdateAnimatorParameters();
    /// 玩家被敌人击中时调用。
    public abstract void OnGetHit();
    /// 收起 / 卸下武器
    public abstract void BlockWeapons(int blockType);
}
