using UnityEngine;

/// <summary>
/// 游戏默认的 IK 实现
/// 想用自己的 IK 控制系统，只需新建一个脚本
/// 并继承 PlayerIKBase.cs
/// </summary>
[ExecuteInEditMode]
public class PlayerIK : PlayerIKBase
{
    #region Public members
    public Transform Target;
    [Header("UPPER BODY")]
    [Range(0, 1)] public float Weight;
    [Range(0, 1)] public float Body;
    [Range(0, 1)] public float Head;
    [Range(0, 1)] public float Eyes;
    [Range(0, 1)] public float Clamp;
    [Range(1, 20)] public float Lerp = 8;

    public Vector3 HandOffset;
    public Vector3 AimSightPosition = new Vector3(0.02f, 0.19f, 0.02f);

    [Header("FOOT IK")]
    [GameToggle] public bool useFootPlacement = true;
    public LayerMask FootLayers;
    [Range(0.1f, 1)] public float FootHeight = 0.43f;
    [Range(-0.5f, 0.5f)] public float TerrainOffset = 0.13f;
    public Vector3 leftKneeTarget, rightKneeTarget;
    public Vector3 leftFeetRotationOffset, rightFeetRotationOffset;
    [GameToggle] public bool debugFootGizmos = true;

    public bool IsCustomHeadTarget { get; set; } = false;
    #endregion

    #region Private members
    private Transform RightFeed;
    private Transform LeftFeed;
    private float lFootIKWeight, rFootIKWeight = 0;
    private Animator animator;
    private Vector3 targetPosition;
    private float rightArmIKRotationWeight = 1;
    private float leftArmIkWight = 1;
    private float rightArmIKPositionWeight = 0;
    private Transform m_headTransform, rightUpperArm;
    private PlayerAnimationsBase PlayerAnimation;
    private float deltaTime = 0;
    private Transform m_headTarget;
    private Transform m_leftArmRef;
    private Transform oldLHTarget;
    private RaycastHit footRaycast;
    private Quaternion footIKRotation = Quaternion.identity;
    //仅编辑器
    [HideInInspector] public int editor_previewMode = 0;
    [HideInInspector] public float editor_weight = 1;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    public override void Init()
    {
        if (playerReferences == null) return;

        PlayerAnimation = playerReferences.playerAnimations;
        animator = playerReferences.PlayerAnimator;
        m_leftArmRef = playerReferences.leftArmTarget.transform;
        if (HeadLookTarget == null) HeadLookTarget = Target;

        m_headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        if (useFootPlacement)
        {
            LeftFeed = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            RightFeed = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
        
        editor_previewMode = 0;
        editor_weight = 0;
    }

    /// <summary>
    /// 动画更新后由 Animator 调用
    /// </summary>
    void OnAnimatorIK(int layer)
    {
        if (HeadLookTarget == null || animator == null)
            return;

        deltaTime = Time.deltaTime;

        if (layer == 0) BottomBody();
        else if (layer == 1) UpperBody();
    }

    /// <summary>
    /// 控制腿部 IK
    /// </summary>
    void BottomBody()
    {
        animator.SetLookAtWeight(Weight, Body, Head, Eyes, Clamp);
        targetPosition = Vector3.Slerp(targetPosition, HeadLookTarget.position, deltaTime * 8);
        animator.SetLookAtPosition(targetPosition);

        if (useFootPlacement && (PlayerAnimation.IsGrounded && PlayerAnimation.LocalVelocity.magnitude <= 0.1f) || editor_previewMode == 1)
        {
            LegsIK();
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }
    }

    /// <summary>
    /// 控制手臂和头部 IK 骨骼
    /// </summary>
    void UpperBody()
    {
        //如果已有其他脚本处理手臂 IK
        if (CustomArmsIKHandler != null)
        {
            CustomArmsIKHandler.OnUpdate();
        }
        else if (LeftHandTarget != null && ControlArmsWithIK)
        {
            ArmsIK();
        }
        else
        {
            ResetWeightIK();
        }
    }

    /// <summary>
    /// 控制左右手臂
    /// </summary>
    void ArmsIK()
    {
        //仅在玩家瞄准或开火时控制手臂
        float weight = (inPointMode) ? 1 : 0;
        float lweight = (PlayerSync.FPState != PlayerFPState.Running && PlayerSync.FPState != PlayerFPState.Reloading) ? 1 : 0;
        rightArmIKRotationWeight = Mathf.Lerp(rightArmIKRotationWeight, lweight, deltaTime * 6);
        leftArmIkWight = Mathf.Lerp(leftArmIkWight, weight, deltaTime * 6);

        animator.SetIKRotation(AvatarIKGoal.LeftHand, m_leftArmRef.rotation);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, m_leftArmRef.position);

        if (rightArmIKRotationWeight > 0)
        {
            // 让右臂瞄准玩家注视的方向
            // 获取注视方向
            Quaternion lookAt = Quaternion.LookRotation(targetPosition - rightUpperArm.position);
            lookAt *= Quaternion.Euler(HandOffset);
            animator.SetIKRotation(AvatarIKGoal.RightHand, lookAt);
        }

        float rpw = (PlayerSync.FPState == PlayerFPState.Aiming || PlayerSync.FPState == PlayerFPState.FireAiming) ? 0.5f : 0;
        rightArmIKPositionWeight = Mathf.Lerp(rightArmIKPositionWeight, rpw, deltaTime * 7);
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (editor_previewMode == 1) rightArmIKPositionWeight = editor_weight;
        }
#endif
        Vector3 relativeAimPosition = m_headTransform.TransformPoint(AimSightPosition);
        animator.SetIKPosition(AvatarIKGoal.RightHand, relativeAimPosition);

        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightArmIKRotationWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightArmIKPositionWeight);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftArmIkWight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftArmIkWight);
    }

    /// <summary>
    /// 检测地面并用 IK 放置脚部
    /// </summary>
    void LegsIK()
    {
        Vector3 leftPosition = LeftFeed.position;
        Vector3 rightPosition = RightFeed.position;
        Vector3 upStart = Vector3.up * FootHeight;
        float detectionRange = FootHeight * 2;

        if (Physics.Raycast(leftPosition + upStart, Vector3.down, out footRaycast, detectionRange, FootLayers))
        {
            leftPosition.y = footRaycast.point.y + TerrainOffset;

            footIKRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(CachedTransform.forward, footRaycast.normal), footRaycast.normal);
            footIKRotation *= Quaternion.Euler(leftFeetRotationOffset);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftPosition);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, footIKRotation);

            animator.SetIKHintPosition(AvatarIKHint.LeftKnee, CachedTransform.TransformPoint(leftKneeTarget));

            lFootIKWeight = Mathf.Lerp(lFootIKWeight, 1, deltaTime * 10);
        }
        else
        {
            lFootIKWeight = Mathf.Lerp(lFootIKWeight, 0, deltaTime * 4);
        }

        if (Physics.Raycast(rightPosition + upStart, Vector3.down, out footRaycast, detectionRange, FootLayers))
        {
            rightPosition.y = footRaycast.point.y + TerrainOffset;

            footIKRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(CachedTransform.forward, footRaycast.normal), footRaycast.normal);
            footIKRotation *= Quaternion.Euler(rightFeetRotationOffset);

            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightPosition);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, footIKRotation);

            animator.SetIKHintPosition(AvatarIKHint.RightKnee, CachedTransform.TransformPoint(rightKneeTarget));

            rFootIKWeight = Mathf.Lerp(rFootIKWeight, 1, deltaTime * 10);
        }
        else
        {
            rFootIKWeight = Mathf.Lerp(rFootIKWeight, 0, deltaTime * 0);
        }

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, lFootIKWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, lFootIKWeight);
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, lFootIKWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rFootIKWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rFootIKWeight);
        animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, rFootIKWeight);
    }

    void ResetWeightIK()
    {
        leftArmIkWight = 0;
        rightArmIKRotationWeight = 0;
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
    }

    public override Transform HeadLookTarget
    {
        get
        {
            if (m_headTarget == null)
            {
                m_headTarget = Target;
                IsCustomHeadTarget = false;
            }
            return m_headTarget;
        }
        set
        {
            if (value == null)
            {
                m_headTarget = Target;
                IsCustomHeadTarget = false;
            }
            else
            {
                m_headTarget = value;
                IsCustomHeadTarget = true;
            }
        }
    }

    /// <summary>
    /// 玩家是否处于需要用 IK 控制手臂的状态
    /// </summary>
    private bool inPointMode
    {
        get
        {
            return (PlayerSync.FPState != PlayerFPState.Running && PlayerSync.FPState != PlayerFPState.Reloading);
        }
    }

    private Transform LeftHandTarget
    {
        get
        {
            if (Application.isPlaying)
            {
                if (PlayerSync != null && PlayerSync.CurrenGun != null)
                {
                    CompareLeftArmTarget(PlayerSync.CurrenGun.LeftHandPosition);
                    return PlayerSync.CurrenGun.LeftHandPosition;
                }
            }
            else//called from an editor script to simulate IK in editor
            {
                if (PlayerSync != null && playerReferences.playerAnimations != null && playerReferences.EditorSelectedGun)
                {
                    if (m_leftArmRef == null) m_leftArmRef = playerReferences.leftArmTarget.transform;

                    CompareLeftArmTarget(playerReferences.EditorSelectedGun.LeftHandPosition);
                    return playerReferences.EditorSelectedGun.LeftHandPosition;
                }
            }
            return null;
        }
    }

    private void CompareLeftArmTarget(Transform laTarget)
    {
        if (oldLHTarget != laTarget)
        {
            oldLHTarget = laTarget;
            while (playerReferences.leftArmTarget.sourceCount > 0)
                playerReferences.leftArmTarget.RemoveSource(0);

            if (playerReferences.leftArmTarget != null && oldLHTarget != null)
            {
                playerReferences.leftArmTarget.transform.position = oldLHTarget.position;
                playerReferences.leftArmTarget.transform.rotation = oldLHTarget.rotation;
                playerReferences.leftArmTarget.AddSource(new UnityEngine.Animations.ConstraintSource()
                {
                    sourceTransform = oldLHTarget,
                    weight = 1
                });
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (animator == null) { animator = GetComponent<Animator>(); }
        if (m_headTransform == null) { m_headTransform = animator.GetBoneTransform(HumanBodyBones.Head); }
        if (m_headTransform != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 hf = m_headTransform.TransformPoint(AimSightPosition);
            Gizmos.DrawLine(m_headTransform.position, hf);
            Gizmos.DrawSphere(hf, 0.015f);
        }
        FootGizmos();
    }

    void FootGizmos()
    {
        if (!debugFootGizmos) return;

        if (LeftFeed == null) LeftFeed = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        if (RightFeed == null) RightFeed = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        Vector3 leftPosition = LeftFeed.position;
        Vector3 rightPosition = RightFeed.position;
        Vector3 upStart = Vector3.up * FootHeight;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(leftPosition, 0.1f);
        Gizmos.DrawWireSphere(rightPosition, 0.1f);

        Vector3 lStartLine = leftPosition + upStart;
        Vector3 rStartLine = rightPosition + upStart;
        float distance = FootHeight * 2;
        Vector3 lineDirection = Vector3.down * distance;

        Gizmos.DrawRay(lStartLine, lineDirection);
        Gizmos.DrawRay(rStartLine, lineDirection);

        Transform leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        if (leftKnee != null)
        {
            Vector3 hintPos = transform.TransformPoint(leftKneeTarget);
            UnityEditor.Handles.DrawDottedLine(leftKnee.position, hintPos, 3f);
            Gizmos.DrawWireSphere(hintPos, 0.07f);
        }

        leftKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        if (leftKnee != null)
        {
            Vector3 hintPos = transform.TransformPoint(rightKneeTarget);
            UnityEditor.Handles.DrawDottedLine(leftKnee.position, hintPos, 3f);
            Gizmos.DrawWireSphere(hintPos, 0.07f);
        }
    }
#endif

    private PlayerReferencesHandler m_playerReferences;
    private PlayerReferencesHandler playerReferences
    {
        get
        {
            if (m_playerReferences == null) m_playerReferences = CachedTransform.GetComponentInParent<PlayerReferencesHandler>();
            return m_playerReferences;
        }
    }

    private PlayerNetworkHandler PlayerSync
    {
        get
        {
            if (playerReferences != null)
                return playerReferences.playerNetwork;

            return null;
        }
    }
}