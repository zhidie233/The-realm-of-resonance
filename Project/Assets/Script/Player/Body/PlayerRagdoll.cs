using UnityEngine;
using System.Collections.Generic;

public class PlayerRagdoll : PlayerRagdollBase
{
    #region Public members
    [GameToggle] public bool ApplyVelocityToRagdoll = true;
    public PlayerReferencesHandler playerReferences;
    public Transform RightHand;
    public Transform PelvisBone;
    public Collider[] playerColliders;
    public List<Rigidbody> rigidBodys = new List<Rigidbody>();
    #endregion

    private Collider[] allPlayerCollider;

    void Awake()
    {
        if (rigidBodys.Count > 0)
        {
            SetKinematic();
        }
    }

    /// <summary>
    /// 设置本地玩家的布娃娃状态
    /// </summary>
    public override void SetLocalRagdoll(RagdollInfo info)
    {
        if (RightHand != null && info.RightHandChild != null)
        {
            Vector3 RootPos = info.RightHandChild.localPosition;
            info.RightHandChild.parent = RightHand;
            info.RightHandChild.localPosition = RootPos;
        }
        Ragdolled(info);
    }

    /// <summary>
    /// 修改布娃娃的运动学状态
    /// </summary>
    /// <param name="kinematic">是否为运动学刚体？</param>
    public void SetKinematic(bool kinematic = true)
    {
        if (rigidBodys == null || rigidBodys.Count <= 0)
            return;

        foreach (Rigidbody r in rigidBodys)
        {
            if (r == null) continue;
            r.isKinematic = kinematic;
        }
    }

    /// <summary>
    /// 把当前玩家变成布娃娃
    /// 玩家不再由 Character Controller 控制
    /// 并会从玩家根节点上分离
    /// </summary>
    public override void Ragdolled(RagdollInfo info)
    {
        gameObject.SetActive(true);
        if (!info.AutoDestroy)
        {
            //用当前玩家状态给动画器推进一帧
            playerReferences.PlayerAnimator.speed = 5;
            playerReferences.playerAnimations.UpdateAnimatorParameters();
            //一次性更新多个参数，才能播放正确的姿势
            for (int i = 0; i < 4; i++)
                playerReferences.PlayerAnimator.Update(1);
        }
        Destroy(playerReferences.playerAnimations);
        this.transform.parent = null;

        playerReferences.PlayerAnimator.enabled = false;
        SetActiveRagdollPhysics(true);
        foreach (Rigidbody r in rigidBodys)
        {
            if (r == null) continue;

            r.isKinematic = false;
            r.useGravity = true;
            if (ApplyVelocityToRagdoll)
            {
                r.velocity = info.AutoDestroy ? playerReferences.PlayerAnimator.velocity : info.Velocity;
            }
            if (info.IsFromExplosion)
            {
                r.AddExplosionForce(875, info.ForcePosition, 7);
            }
        }

        if (info.AutoDestroy) Destroy(gameObject, GameConfig.Instance.PlayerRespawnTime);
    }

    /// <summary>
    /// 让本地玩家忽略自身的 Character Controller 碰撞体。
    /// </summary>
    public override void IgnorePlayerCollider()
    {
        GameObject p = GameController.Instance.LocalPlayer;
        if (p == null) return;

        Collider Player = p.GetComponent<Collider>();
        if (Player != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                var col = playerColliders[i];
                if (col == null) continue;

                Physics.IgnoreCollision(col, Player);
            }
        }
    }

    /// <summary>
    /// 启用/停用玩家的刚体和碰撞体
    /// </summary>
    public override void SetActiveRagdollPhysics(bool active)
    {
        for (int i = 0; i < playerColliders.Length; i++)
        {
            if (playerColliders[i] == null) continue;

            playerColliders[i].enabled = active;
        }
        foreach (var item in rigidBodys)
        {
            if (item == null) continue;

            item.isKinematic = !active;
        }
    }

    /// <summary>
    /// 让该玩家的碰撞体忽略指定的碰撞体
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ignore"></param>
    public override void IgnoreColliders(Collider[] list, bool ignore)
    {
        if (allPlayerCollider == null || allPlayerCollider.Length <= 0)
        {
            allPlayerCollider = transform.GetComponentsInChildren<Collider>();
        }
        for (int e = 0; e < list.Length; e++)
        {
            if (list[e] == null) continue;

            for (int i = 0; i < allPlayerCollider.Length; i++)
            {
                if (allPlayerCollider[i] != null)
                {
                    Physics.IgnoreCollision(allPlayerCollider[i], list[e], ignore);
                }
            }
        }
    }

    [ContextMenu("Setup")]
    public void SetUpHitBoxes()
    {   
        GetRigidBodys();
        GetRequireBones();
    }

    void GetRigidBodys()
    {
        rigidBodys.Clear();
        rigidBodys.AddRange(transform.GetComponentsInChildren<Rigidbody>());
        playerColliders = transform.GetComponentsInChildren<Collider>();
    }

    public void GetRequireBones()
    {
        RightHand = playerReferences.PlayerAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        PelvisBone = playerReferences.PlayerAnimator.GetBoneTransform(HumanBodyBones.Hips);
    }
}