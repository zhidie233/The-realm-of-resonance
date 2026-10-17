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
    /// Is this player touching the ground?
    /// This value should be provided by PhotonNetworkWrapper.cs
    /// </summary>
    public bool IsGrounded
    {
        get;
        set;
    }

    /// <summary>
    /// The velocity of this player
    /// </summary>
    public Vector3 Velocity
    {
        get;
        set;
    } = Vector3.zero;

    /// <summary>
    /// The local velocity of this player
    /// </summary>
    public Vector3 LocalVelocity
    {
        get;
        set;
    } = Vector3.zero;

    /// <summary>
    /// Called when the player has changed of weapon
    /// </summary>
    public abstract void SetNetworkGun(GunType weaponType, RemoteGun networkGun);

    /// <summary>
    /// Play the fire animation for a specific gun type.
    /// </summary>
    /// <param name="gunType"></param>
    public abstract void PlayFireAnimation(GunType gunType);

    /// <summary>
    /// Update the player animator parameters.
    /// </summary>
    public abstract void UpdateAnimatorParameters();

    /// <summary>
    /// Called when the player get hit by an enemy.
    /// </summary>
    public abstract void OnGetHit();

    /// <summary>
    /// Block / Unequipped the weapons
    /// </summary>
    public abstract void BlockWeapons(int blockType);
}