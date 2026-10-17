using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using NetHashTable = ExitGames.Client.Photon.Hashtable;
using Game.Runtime.AI;
using GameEditor;

[RequireComponent(typeof(NavMeshAgent))]
public class AIShooterAgent : AIShooter
{
    #region Public Members
    [Space(5)]
    [ScriptableDrawer] public AIBehaviorSettings behaviorSettings;
    [ScriptableDrawer] public AISoldierSettings soldierSettings;

    [Header("Others")]
    public LayerMask ObstaclesLayer;
    public bool DebugStates = false;

    [Header("References")]
    public Transform aimTarget;
    public Footstep footstep;
    #endregion

    #region Public Properties
    public override Transform AimTarget
    {
        get => aimTarget;
    }
    public bool playerInFront { get; set; }
    public bool ObstacleBetweenTarget { get; set; }
    public AIShooterAttackBase AIWeapon { get; set; }
    public PlayerHealthManagerBase AIHealth { get; set; }
    public override float CachedTargetDistance { get; set; } = 0;
    public string DebugLine { get; set; }//last ID 38 
    public bool IsCrouch { get; set; } = false;
    #endregion

    #region Private members
    private Animator Anim;
    private Vector3 finalPosition;
    private float lastPathTime = 0;
    private AICoverPoint CoverPoint = null;
    private bool ForceCoverFire = false;
    private float CoverTime = 0;
    private Vector3 LastHitDirection;
    private int SwitchCoverTimes = 0;
    private float lookTime, lastHoldPositionTime = 0;
    private bool randomOnStartTake = false;
    private bool AllOrNothing = false;
    private GameMode m_GameMode;
    private float time, delta = 0;
    private Transform m_Transform;
    private float nextEnemysCheck = 0;
    private bool isGameStarted = false;
    private Vector3 targetDirection = Vector3.zero;
    private RaycastHit obsRay;
    private float lastDestinationTime = 0;
    private float velocityMagnitud = 0;
    private bool forceUpdateRotation = false;
    private Vector3 localRotation = Vector3.zero;
    private int[] animationHash = new int[] { 0 };
    private bool wasTargetInSight = false;
    public const string RPC_NAME = "RPCShooterBot";
    private int precisionRate = 4;
    private List<Transform> availableTargets = new List<Transform>();
    private Transform headBoneReference;
    public bool isMaster = false;
    private float lastCrouchTime = 0;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        Agent = References.Agent;
        // 添加高度感知设置
        if (Agent != null)
        {
            Agent.autoTraverseOffMeshLink = true; // 允许跨越OffMeshLink
            Agent.autoRepath = true; // 自动重新计算路径
            Agent.radius = 0.5f;
            Agent.height = 2.0f; // 适当的高度
        }
        AIHealth = References.shooterHealth;
        AIWeapon = References.shooterWeapon;
        Anim = References.PlayerAnimator;
        ObstacleBetweenTarget = false;
        m_GameMode = GetGameMode;
        Agent.updateRotation = false;
        animationHash[0] = Animator.StringToHash("Crouch");
        AIMananger.SetBotGameState(this, BotGameState.Playing);
        headBoneReference = References.aiAnimation.GetHumanBone(HumanBodyBones.Head);
    }

    public override void Init()
    {
        isMaster = PhotonNetworkWrapper.IsMasterClient;
        isGameStarted = MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;
        References.shooterNetwork.CheckNamePlate();
        References.Agent.enabled = isMaster;
    }
    
    public override void OnUpdate()
    {  
        time = Time.time;
        delta = Time.deltaTime;
        if (isDeath) return;
        isNewDebug = true;

        if (!isGameStarted) return;

        if (Target != null && isMaster)
        {
            if (AgentState != AIAgentState.Covering)
            {
                if (AgentState != AIAgentState.HoldingPosition)
                {
                    TargetControll();
                }
                else
                {
                    OnHoldingPosition();
                }
            }
            else
            {
                OnCovering();
            }
        }
        LookAtControl();
    }

    /// <summary>
    /// 每秒调用一次，而不是每帧
    /// </summary>
    public override void OnSlowUpdate()
    {
        if (isDeath) return;
        if (MatchTimeManagerBase.Instance.IsTimeUp() || !isGameStarted)
        {
            return;
        }

        velocityMagnitud = Agent.velocity.magnitude;

        if (isMaster)
        {
            if (!HasATarget)
            {
                SetDebugState(-1, true);
                //获取最近的玩家
                SearchPlayers();
                //目标仍为空就先巡逻
                RandomPatrol(!isOneTeamMode);
            }
            else
            {
                CheckEnemysDistances();
                CheckVision();
            }
        }

        FootStep();
    }

    private void OnAgentStateChanged(AIAgentState from, AIAgentState to)
    {
        
    }

    private void OnTargetChanged(Transform from, Transform to)
    {

    }

    /// <summary>
    /// 机器人在对目标失去直视线与重新获得直视线时调用
    /// </summary>
    /// <param name="from">是否看得见？</param>
    /// <param name="to">是否看得见？</param>
    private void OnTargetLineOfSightChanged(bool from, bool to)
    {
        if (from == true)//the player lost the line of vision with the target
        {
            if (HasATarget && TargetDistance > 5)//he lost the target but not because it is death.
                Invoke(nameof(CorrectLookAt), 3);//if after 3 second of loss the target, still now found it -> don't look at it (trough walls for example)
        }
        else//player now has direct vision to the target
        {
            if (AgentState == AIAgentState.Following)
                CancelInvoke(nameof(CorrectLookAt));
        }
    }

    void CorrectLookAt()
    {
        SetLookAtState(AILookAt.PathToTarget);
    }

    void TargetControll()
    {
        if (Time.frameCount % precisionRate != 0) return;

        CachedTargetDistance = UtilityHelper.Distance(Target.position, m_Transform.localPosition);
        if (CachedTargetDistance >= soldierSettings.limitRange)
        {
            WhenTargetOutOfRange();
        }
        else if (CachedTargetDistance > soldierSettings.closeRange && CachedTargetDistance < soldierSettings.mediumRange)
        {
            WhenTargetOnMediumRange();
        }
        else if (CachedTargetDistance <= soldierSettings.closeRange)
        {
            WhenTargetOnCloseRange();
        }
        else if (CachedTargetDistance < soldierSettings.limitRange)
        {
            WhenTargetOnLimitRange();
        }
        else
        {
            Debug.Log("Unknown state: " + CachedTargetDistance);
            SetDebugState(101, true);
        }
    }

    void WhenTargetOutOfRange()
    {
        if (behaviorSettings.targetOutRangeBehave == AITargetOutRangeBehave.SearchNewNearestTarget)
        {
            var newTarget = GetNearestPlayer();
            if (newTarget != Target)
            {
                SetTarget(newTarget);
                return;
            }
        }
        precisionRate = 8;
        if (AgentState == AIAgentState.Following || FocusOnSingleTarget)
        {
            if (!isOneTeamMode)
            {
                //每 300 帧更新一次目标位置
                if (!Agent.hasPath || (Time.frameCount % 300) == 0)
                {
                    if (UtilityHelper.Distance(TargetPosition, Agent.destination) > 1)//update the path only if the target has moved substantially
                    {
                        SetDestination(TargetPosition, 3);
                    }
                }
            }
            else
            {
                //单人队伍模式下，目标进入限定范围时
                //机器人会改为随机巡逻，不再跟随玩家。
                RandomPatrol(true);
            }
            SetDebugState(0, true);
        }
        else if (AgentState == AIAgentState.Searching)
        {
            SetDebugState(1, true);
            RandomPatrol(true);
        }
        else
        {
            SetDebugState(2, true);
            SetTarget(null);
            RandomPatrol(false);
            SetState(AIAgentState.Patroling);
        }
        Speed = behaviorSettings.agentBehave == AIAgentBehave.Agressive ? soldierSettings.runSpeed : soldierSettings.walkSpeed;
        if (!AIWeapon.IsFiring)
        {
            Anim.SetInteger("UpperState", 4);
        }
    }

    void WhenTargetOnMediumRange()
    {
        SetDebugState(3, true);
        precisionRate = 4;
        OnTargeContest(false);
    }

    void WhenTargetOnCloseRange()
    {
        SetDebugState(4, true);
        precisionRate = 2;
        Follow();
    }

    void WhenTargetOnLimitRange()
    {
        SetDebugState(5, true);
        OnTargeContest(true);
        precisionRate = 1;
    }

    void OnCovering()
    {
        if (Time.frameCount % precisionRate != 0) return;

        if (Target != null)
        {
            CachedTargetDistance = TargetDistance;
            if (CachedTargetDistance <= soldierSettings.mediumRange && playerInFront)//if in look range and in front, start follow him and shot
            {
                if (behaviorSettings.agentBehave == AIAgentBehave.Agressive)
                {
                    if (!Agent.hasPath)
                    {
                        SetDebugState(38, true);
                        SetState(AIAgentState.Following);
                        SetDestination(TargetPosition, 3);
                    }
                    else
                    {
                        if (AgentState == AIAgentState.Covering)
                        {
                            SetDebugState(6, true);

                            if (!ObstacleBetweenTarget)
                            {
                                TriggerFire(playerInFront ? AIShooterAttackBase.FireReason.Forced : AIShooterAttackBase.FireReason.OnMove);
                            }

                            if(CachedTargetDistance < soldierSettings.mediumRange)
                            {
                                SetCrouch(false);
                            }

                            if (AIHealth.GetHealth() <= 30 && CachedTargetDistance < soldierSettings.closeRange)
                            {
                                SetState(AIAgentState.Following);
                                SetDestination(GetPositionAround(TargetPosition, 35), 5);
                            }
                        }
                        else
                        {
                            SetDebugState(39, true);
                        }
                    }
                }
                else //to covert point and looking to it
                {
                    SetDebugState(7, true);
                    SetState(AIAgentState.Covering);
                    if (!Agent.hasPath)
                    {
                        Cover(false);
                    }
                }
                TriggerFire(AIShooterAttackBase.FireReason.OnMove);
            }
            else if (CachedTargetDistance > soldierSettings.limitRange && CanCover(7))// if out of line of sight, start searching him
            {
                SetDebugState(8, true);
                SetState(AIAgentState.Searching);
                SetCrouch(false);
                TriggerFire(AIShooterAttackBase.FireReason.OnMove);
            }
            else if (ForceCoverFire && !ObstacleBetweenTarget)//if bot is cover and still get damage, start shoot at the target (panic)
            {
                SetDebugState(9, true);
                SetLookAtState(AILookAt.Target);
                TriggerFire(AIShooterAttackBase.FireReason.Forced);

                if (behaviorSettings.agentBehave == AIAgentBehave.Protective || CachedTargetDistance > soldierSettings.mediumRange)
                {
                    if (CanCover(behaviorSettings.maxCoverTime)) { SwitchCover(); }
                }
            }
            else if (CanCover(behaviorSettings.maxCoverTime) && CachedTargetDistance >= 7)//if has been a time since cover and nothing happen, try a new spot.
            {
                SetDebugState(10, true);
                SwitchCover();
                TriggerFire(AIShooterAttackBase.FireReason.OnMove);
            }
            else
            {
                if (playerInFront)
                {
                    Speed = soldierSettings.walkSpeed;
                    TriggerFire(AIShooterAttackBase.FireReason.Forced);
                    SetDebugState(11, true);
                }
                else
                {
                    SetDebugState(12, true);
                    Speed = soldierSettings.runSpeed;
                    CheckConfrontation();
                    SetLookAtState(AILookAt.Target);
                    TriggerFire(AIShooterAttackBase.FireReason.OnMove);
                    SetCrouch(false);
                }
            }
        }
        if (Agent.pathStatus == NavMeshPathStatus.PathComplete)//once the bot reach the target cover point
        {
            if (CoverPoint != null && CoverPoint.Crouch) { SetCrouch(true); }//and the point required crouch -> do crouch
        }
    }

    private bool Cover(bool overridePoint, AIAgentCoverArea coverArea = AIAgentCoverArea.ToPoint)
    {
        // 添加空检查
        if (AICovertPointManager.Instance == null)
        {
            Debug.LogError("AICoverPointManager实例为空！");
            return false;
        }
        //目标太远就没必要躲掩体
        if (behaviorSettings.agentBehave == AIAgentBehave.Agressive && CachedTargetDistance > 20)
        {
            SetState(AIAgentState.Following);
            return false;
        }
        Transform t = transform;
        switch (coverArea)
        {
            case AIAgentCoverArea.ToTarget:
                t = Target;//find a point near the target
                break;
        }
        if (overridePoint)//override the current cover point
        {
            //如果机器人已到达当前目的地
            if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                //在 30 米内取一个随机点
                if (coverArea == AIAgentCoverArea.ToRandomPoint)
                {
                    //找另一个随机掩体点
                    CoverPoint = AICovertPointManager.Instance.GetCoverOnRadius(t, 30);
                }
                else
                {
                    //获取最近的掩体点
                    CoverPoint = AICovertPointManager.Instance.GetCloseCover(t, CoverPoint);
                }
            }
            SetDebugState(13);
        }
        else
        {
            SetDebugState(14);
            //找一个附近的掩体点
            CoverPoint = AICovertPointManager.Instance.GetCloseCover(t);
        }

        if (CoverPoint != null && (time - CoverTime) > behaviorSettings.coverColdDown)//if a point was found
        {
            SetDebugState(15);
            Speed = playerInFront ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
            SetDestination(CoverPoint.transform.position, 0.1f);
            SetState(AIAgentState.Covering);
            CoverTime = time;
            TriggerFire(behaviorSettings.agentBehave == AIAgentBehave.Agressive ? AIShooterAttackBase.FireReason.Normal : AIShooterAttackBase.FireReason.OnMove);
            return true;
        }
        else
        {
            //附近没有掩体点
            if (Target != null)//and have a target
            {
                SetDebugState(16);
                //跟随目标
                SetDestination(TargetPosition, 3);
                DetermineSpeedBaseOnRange();
                FocusOnSingleTarget = true;//and follow not matter the distance
                SetState(AIAgentState.Searching);
            }
            else//if don't have a target
            {
                SetDebugState(17);
                //强制获取一个掩体点
                CoverPoint = AICovertPointManager.Instance.GetCloseCoverForced(m_Transform);
                if (CoverPoint != null)
                {
                    SetDestination(CoverPoint.transform.position, 0.1f);
                    Speed = Probability(0.5f) ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
                }
            }
            return false;
        }
    }

    public override void OnGetHit(Vector3 pos)
    {
        LastHitDirection = pos;
        // 添加前置检查
        if (AICovertPointManager.Instance == null)
        {
            Debug.LogError("AICoverPointManager不可用，无法寻找掩护");
            SetLookAtState(AILookAt.Target);
            return;
        }
        //机器人没有躲掩体，就去找一个掩体点
        if (AgentState != AIAgentState.Covering)
        {
            //机器人正在跟随并攻击目标，就不去找掩体点
            if (AgentState == AIAgentState.Following && TargetDistance <= soldierSettings.mediumRange && !ObstacleBetweenTarget)
            {
                SetLookAtState(AILookAt.Target);
                return;
            }
            Cover(false);
        }
        else
        {
            //已在掩体中却仍被打到，就强制机器人开火。
            if (!playerInFront)
            {
                SetLookAtState(AILookAt.Target);
                Cover(true);
            }
            else
            {
                ForceCoverFire = true;
                SetLookAtState(AILookAt.HitDirection);
            }
            //机器人躲着掩体仍被击中，就另找掩体点
            if (AIHealth.GetHealth() <= 50 && Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Cover(true);
            }
        }
    }

    void OnHoldingPosition()
    {

        SetDebugState(37, true);
        if (time > lastHoldPositionTime)
        {
            SetState(Target != null ? AIAgentState.Following : AIAgentState.Searching);
            return;
        }
    }

    void SwitchCover()
    {
        if (Agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        if (SwitchCoverTimes <= 3)
        {
            Cover(true, AIAgentCoverArea.ToTarget);
            SwitchCoverTimes++;
        }
        else
        {
            SetState(AIAgentState.Following);
            SetDestination(TargetPosition, 3);
            SwitchCoverTimes = 0;
            AllOrNothing = true;//go straight to the target to confront him
        }
    }

    /// <summary>
    /// 目标进入视距时
    /// </summary>
    void OnTargeContest(bool overrideCover)
    {
        if (AgentState == AIAgentState.Following || ForceFollowAtHalfHealth)
        {
            if (!Cover(overrideCover) || CanCover(behaviorSettings.maxCoverTime) || AllOrNothing)
            {
                if (CachedTargetDistance <= 3)
                {
                    SetDebugState(35);
                    Cover(true, AIAgentCoverArea.ToRandomPoint);
                }
                else
                {
                    SetDebugState(18);
                    Follow();
                }
            }
            else
            {
                if (Target != null)
                {
                    var fr = TargetDistance < 12 ? AIShooterAttackBase.FireReason.Forced : AIShooterAttackBase.FireReason.OnMove;
                    TriggerFire(fr);
                }
                SetDebugState(19);
                SetCrouch(true);
            }
        }
        else if (AgentState == AIAgentState.Covering)
        {
            if (CanCover(5) && TargetDistance >= 7)
            {
                SetDebugState(21);
                Cover(true);
            }
            else
            {
                SetDebugState(22);
            }
        }
        else
        {

            OnDirectConfrontation();
        }
    }

    /// <summary>
    //目标靠近时
    //判断能否攻击它
    /// </summary>
    void OnDirectConfrontation()
    {
        precisionRate = 1;
        if (CachedTargetDistance <= 15)
        {
            SetCrouch(false);
            if (AIHealth.GetHealth() <= 25)
            {
                SetDebugState(36);
                SetState(AIAgentState.Covering);
                Cover(false, AIAgentCoverArea.ToRandomPoint);
                Speed = soldierSettings.runSpeed;
            }
            else
            {
                SetDebugState(23);
                if (time > lastHoldPositionTime)
                {
                    HoldPosition();
                }
                DetermineSpeedBaseOnRange();
            }
        }
        else
        {
            SetDebugState(35);
            DetermineSpeedBaseOnRange();
        }

        CheckConfrontation();
    }

    void SearchPlayers()
    {
        SetDebugState(-2);

        Transform target = behaviorSettings.agentBehave == AIAgentBehave.Agressive ? GetNearestPlayer() : GetTargetOnRange();
        if (target != null) SetTarget(target);

        //开局随机选一个目标
        if (PhotonNetworkWrapper.IsMasterClient && !randomOnStartTake && availableTargets.Count > 0)
        {
            if (behaviorSettings.GetRandomTargetOnStart)
            {
                SetTarget(availableTargets[Random.Range(0, availableTargets.Count)]);
                randomOnStartTake = true;
            }
        }

        if (!HasATarget)
        {
            if (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking) { SetState(AIAgentState.Searching); }
        }
    }

    void CheckVision()
    {
        if (!HasATarget || !PhotonNetworkWrapper.IsMasterClient)
        {
            ObstacleBetweenTarget = false;
            return;
        }

        Vector3 relative = m_Transform.InverseTransformPoint(TargetPosition);
        playerInFront = (relative.x < 2f && relative.x > -2f) || (relative.x > -2f && relative.x < 2f);

        if (Physics.Linecast(AIWeapon.GetFirePosition(), TargetPosition, out obsRay, ObstaclesLayer, QueryTriggerInteraction.Ignore))
        {
            ObstacleBetweenTarget = obsRay.transform.root.CompareTag(GameCore.LOCAL_PLAYER_TAG) == false;
        }
        else { ObstacleBetweenTarget = false; }

        if (wasTargetInSight != ObstacleBetweenTarget)
        {
            OnTargetLineOfSightChanged(wasTargetInSight, ObstacleBetweenTarget);
            wasTargetInSight = ObstacleBetweenTarget;
        }
    }

    /// <summary>
    /// 让机器人在原地停留指定时间
    /// </summary>
    /// <param name="forTime"></param>
    public void HoldPosition(float forTime = 10)
    {
        SetState(AIAgentState.HoldingPosition);
        lastDestinationTime = Time.time + forTime;
    }

    /// <summary>
    /// 敌人在范围外时，让机器人在地图上随机巡逻
    /// </summary>
    /// <param name="precision">巡逻时靠近敌人区域的程度</param>
    void RandomPatrol(bool precision)
    {
        if (isDeath || !PhotonNetworkWrapper.IsMasterClient)
            return;

        float precisionArea = soldierSettings.farRange;
        if (precision)
        {
            if (TargetDistance < soldierSettings.mediumRange)
            {
                SetDebugState(24);
                if (!HasATarget)
                {
                    SetTarget(GetNearestPlayer());
                }
                SetState(behaviorSettings.agentBehave == AIAgentBehave.Protective ? AIAgentState.Covering : AIAgentState.Looking);
                precisionArea = 5;
            }
            else
            {
                SetDebugState(25);
                SetState(behaviorSettings.agentBehave == AIAgentBehave.Agressive ? AIAgentState.Following : AIAgentState.Searching);
                precisionArea = 8;
            }
        }
        else
        {
            SetDebugState(26);
            SetState(AIAgentState.Patroling);
            if (behaviorSettings.agentBehave == AIAgentBehave.Agressive && !HasATarget)
            {
                SetTarget(GetNearestPlayer());
                SetCrouch(false);
            }
            ForceCoverFire = false;
        }

        SetLookAtState(AILookAt.Path);
        AIWeapon.IsFiring = false;

        if (!Agent.hasPath || (time - lastPathTime) > 5)
        {
            SetDebugState(27);
            bool toAnCover = (Random.value <= behaviorSettings.randomCoverProbability);//probability of get a cover point as random destination
            Vector3 randomDirection = TargetPosition + (Random.insideUnitSphere * precisionArea);
            if (toAnCover) { randomDirection = AICovertPointManager.Instance.GetCoverOnRadius(m_Transform, 20).Position; }
            if (!HasATarget && isOneTeamMode)
            {
                randomDirection += m_Transform.localPosition;
            }
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, precisionArea, 1);
            finalPosition = hit.position;
            lastPathTime = time + Random.Range(1, 5);
            DetermineSpeedBaseOnRange();
            SetCrouch(false);
        }
        else
        {
            if (Agent.hasPath)
            {
                SetDebugState(28);
            }
            else
            {
                SetDebugState(32);
            }
        }
        SetDestination(finalPosition, 1, true);
    }

    public void KillTheTarget()
    {
        if (!HasATarget) return;

        SetTarget(null);
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.SyncTarget);
        data.Add("viewID", -1);

        photonView.RPC(RPC_NAME, RpcTarget.Others, data);
    }

    void CheckConfrontation()
    {
        if (AgentState != AIAgentState.Covering)
        {
            if (lookTime >= 5)
            {
                SetState(AIAgentState.Following);
                lookTime = 0;
                return;
            }
            lookTime += delta;
            SetState(AIAgentState.Looking);
        }

        TriggerFire();
        SetCrouch(playerInFront && !ObstacleBetweenTarget);
    }

    private void LookAtControl()
    {
        if (!isMaster) return;

        if ((Time.frameCount % AIMananger.Instance.updateBotsLookAtEach) == 0 || forceUpdateRotation)
        {
            // ... 现有代码 ...
        
            // 改进高度处理
            if (UtilityHelper.Distance(m_Transform.localPosition, LookAtPosition) <= 3)
            {
                Vector3 lap = LookAtPosition;
                // 保持适当的高度差感知
                float heightDiff = Mathf.Abs(lap.y - headBoneReference.position.y);
                if (heightDiff > 2.0f) // 如果高度差过大，调整注视点
                {
                    lap.y = Mathf.Lerp(headBoneReference.position.y, lap.y, 0.5f);
                }
                LookAtPosition = lap;
                LookAtDirection = LookAtPosition - m_Transform.localPosition;
            }

            localRotation = m_Transform.localEulerAngles;
            localRotation.y = Quaternion.LookRotation(LookAtDirection).eulerAngles.y;
            forceUpdateRotation = false;
        }

        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(localRotation), delta * soldierSettings.rotationSmoothing);
    }

    void Follow()
    {
        if (AgentState == AIAgentState.Covering && Random.value > 0.5f) return;

        SetLookAtState(AILookAt.Target);

        SetCrouch(false);
        SetDestination(TargetPosition, 3);
        if (CachedTargetDistance <= 3)
        {
            CoverTime = 0;
            Speed = soldierSettings.walkSpeed;
            if (Cover(true, AIAgentCoverArea.ToTarget) && Random.value >= 0.5f)
            {
                SetDebugState(29);
            }
            else if (Cover(true, AIAgentCoverArea.ToRandomPoint))
            {
                SetDebugState(30);
            }
            else
            {
                SetDebugState(34);
                SetDestination(m_Transform.localPosition - (m_Transform.forward * 3), 0.1f);
            }
            SetState(AIAgentState.Covering);
            CheckConfrontation();
            SetCrouch(false);
            TriggerFire(AIShooterAttackBase.FireReason.Forced);
        }
        else
        {
            DetermineSpeedBaseOnRange();
            SetDebugState(33);
            TriggerFire();
        }
    }

    public void TriggerFire(AIShooterAttackBase.FireReason reason = AIShooterAttackBase.FireReason.Normal)
    {
        if (LookingAt == AILookAt.Path) SetLookAtState(AILookAt.Target);

        AIWeapon.Fire(reason);
    }

    private void DetermineSpeedBaseOnRange()
    {
        Speed = (Target != null && CachedTargetDistance > soldierSettings.mediumRange) ? soldierSettings.runSpeed : soldierSettings.walkSpeed;
    }

    public void SetState(AIAgentState newState)
    {
        if (newState == AgentState) return;

        OnAgentStateChanged(AgentState, newState);
        AgentState = newState;
    }

    public void SetTarget(Transform newTarget)
    {
        if (Target == newTarget) return;

        OnTargetChanged(Target, newTarget);
        Target = newTarget;

        if (!PhotonNetworkWrapper.IsMasterClient) return;

        //同步机器人目标
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.SyncTarget);

        if (Target == null)
        {
            data.Add("viewID", -1);
            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
        }
        else
        {
            PhotonView view = GetPhotonView(Target.root.gameObject);
            if (view != null)
            {                            
                data.Add("viewID", view.ViewID);
                photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            }
            else
            {
                Debug.Log("This Target " + Target.name + "no have photonview");
            }
        }
    }

    public void SetLookAtState(AILookAt newLookAt)
    {
        if (LookingAt == newLookAt) return;
        if (LookingAt == AILookAt.PathToTarget && newLookAt == AILookAt.Target)
        {
            if (ObstacleBetweenTarget) return;
        }

        LookingAt = newLookAt;
        forceUpdateRotation = true;
    }

    public void SetDestination(Vector3 position, float stopedDistance, bool checkRate = false)
    {
        if (checkRate && (time - lastDestinationTime) < 2) return;

        // 添加高度检查
        if (IsPositionReachable(position))
        {
            Agent.stoppingDistance = stopedDistance;
            Agent.SetDestination(position);
            lastDestinationTime = time;
        }
        else
        {
            // 如果目标位置不可达，寻找附近可达点
            Vector3 fallbackPosition = FindNearestReachablePosition(position);
            if (fallbackPosition != Vector3.zero)
            {
                Agent.stoppingDistance = stopedDistance;
                Agent.SetDestination(fallbackPosition);
                lastDestinationTime = time;
            }
        }
    }
    
    /// <summary>
    /// 检查位置是否可达（考虑高度差）
    /// </summary>
    public bool IsPositionReachable(Vector3 targetPosition)
    {
        // 计算高度差
        float heightDifference = Mathf.Abs(targetPosition.y - transform.position.y);
    
        // 如果高度差在合理范围内，认为可达
        if (heightDifference <= 500f) // 使用您的最大高度差值
        {
            // 使用 NavMesh 采样检查实际可达性
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 5.0f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (Agent.CalculatePath(hit.position, path))
                {
                    return path.status == NavMeshPathStatus.PathComplete;
                }
            }
        }
    
        return false;
    }
    
    /// <summary>
    /// 寻找最近的可达位置
    /// </summary>
    private Vector3 FindNearestReachablePosition(Vector3 targetPosition, float searchRadius = 10f)
    {
        for (int i = 0; i < 10; i++) // 尝试10次
        {
            Vector3 randomPoint = targetPosition + Random.insideUnitSphere * searchRadius;
            randomPoint.y = targetPosition.y; // 保持相同高度
        
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                if (IsPositionReachable(hit.position))
                {
                    return hit.position;
                }
            }
        }
        return Vector3.zero;
    }

    void SetCrouch(bool crouch)
    {
        if (IsCrouch == crouch || time - lastCrouchTime < 0.5f) return;
        lastCrouchTime = time;
        if (crouch && (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking))
        {
            crouch = false;
        }

        Anim.SetBool(animationHash[0], crouch);
        Speed = crouch ? soldierSettings.crounchSpeed : soldierSettings.walkSpeed;
        if (IsCrouch != crouch)
        {
            var data = UtilityHelper.CreatePhotonHashTable();
            data.Add("type", AIRemoteCallType.CrouchState);
            data.Add("state", crouch);

            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            IsCrouch = crouch;
        }
    }

    /// <summary>
    /// 机器人有目标时调用
    /// 检查是否有更近的敌人，必要时切换目标
    /// </summary>
    void CheckEnemysDistances()
    {
        if (!behaviorSettings.checkEnemysWhenHaveATarget || availableTargets.Count <= 0) return;
        if (time < nextEnemysCheck) return;

        CachedTargetDistance = UtilityHelper.Distance(m_Transform.localPosition, TargetPosition);
        for (int i = 0; i < availableTargets.Count; i++)
        {
            //敌人的 transform 不为空、不是当前目标、且未死亡时
            if (availableTargets[i] == null || availableTargets[i] == Target || availableTargets[i].name.Contains("(die)")) continue;
            //计算与这个敌人的距离
            float otherDistance = UtilityHelper.Distance(m_Transform.localPosition, availableTargets[i].position);
            if (otherDistance > soldierSettings.limitRange) continue;//if this enemy is too far away...
            //判断是否比当前目标更近（至少近 5 米）
            if (otherDistance < CachedTargetDistance && (CachedTargetDistance - otherDistance) > 5)
            {
                //计算机器人与该敌人之间的夹角，判断是否在“视野角”内
                Vector3 targetDir = availableTargets[i].position - m_Transform.localPosition;
                float Angle = Vector3.Angle(targetDir, m_Transform.forward);
                if (Angle > -55 && Angle < 55)
                {
                    //满足条件就把它当作新的危险目标
                    SetTarget(availableTargets[i]);
                    //至少接下来 3 秒内不再切换目标
                    nextEnemysCheck = time + 3;
                }
            }
        }
    }

    public override void UpdateTargetList()
    {
        if (Target != null) return;

        AIMananger.Instance.GetTargetsFor(this, ref availableTargets);
        AimTarget.name = AIName;
    }

    public override void CheckTargets()
    {
        if (Target != null && Target.name.Contains("(die)"))
        {
            SetTarget(null);
        }
    }

    /// <summary>
    /// 强制重生被调用、而该机器人仍存活时调用
    /// </summary>
    public override void Respawn()
    {
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.Respawn);
        photonView.RPC(RPC_NAME, RpcTarget.All, data);
    }

    public void FootStep()
    {
        if (velocityMagnitud > 0.2f)
            footstep?.UpdateStep(Agent.speed);
    }

    [System.Serializable]
    public class NetworkMessagesCount
    {
        public AIRemoteCallType CallType;
        public int Count;
    }
    public List<NetworkMessagesCount> networkMessagesCounts = new List<NetworkMessagesCount>();

    [PunRPC]
    public void RPCShooterBot(NetHashTable data, PhotonMessageInfo info)
    {
        var callType = (AIRemoteCallType)data["type"];

        var cid = networkMessagesCounts.FindIndex(x => x.CallType == callType);
        if(cid == -1)
        {
            networkMessagesCounts.Add(new NetworkMessagesCount()
            {
                CallType = callType,
                Count = 1
            });
        }
        else
        {
            networkMessagesCounts[cid].Count++;
        }

        switch (callType)
        {
            case AIRemoteCallType.DestroyBot:
                DestroyBot(data, info);
                break;
            case AIRemoteCallType.SyncTarget:
                SyncTargetAI(data);
                break;
            case AIRemoteCallType.CrouchState:
                Anim.SetBool(animationHash[0], (bool)data["state"]);
                break;
            case AIRemoteCallType.Respawn:
                DoAliveRespawn();
                break;
        }
    }

    /// <summary>
    /// 不销毁机器人实例直接重生
    /// </summary>
    private void DoAliveRespawn()
    {
        Agent.isStopped = false;
        AIHealth.SetHealth(100, true);
        isGameStarted = false;
        Target = null;
        this.InvokeAfter(2, () => { isGameStarted = MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started; });
        if (PhotonNetworkWrapper.IsMasterClient)
        {
            var spawn = SpawnPointManager.Instance.GetSequentialSpawnPoint(AITeam).transform;
            Agent.Warp(spawn.position);
            m_Transform.position = spawn.position;
            m_Transform.rotation = spawn.rotation;
        }      
    }

    /// <summary>
    /// 机器人死亡时，由主客户端在所有客户端上调用
    /// </summary>
    public void DestroyBot(NetHashTable data, PhotonMessageInfo info)
    {
        if (data.ContainsKey("instant"))
        {
            if (PhotonNetworkWrapper.IsMasterClient) PhotonNetworkWrapper.Destroy(gameObject);
            return;
        }

        Vector3 position = (Vector3)data["direction"];
        References.aiAnimation.Ragdolled(position, data.ContainsKey("explosion"));

        if ((PhotonNetworkWrapper.Time - info.SentServerTime) > GameConfig.Instance.PlayerRespawnTime + 1)
        {
            Debug.LogWarning("The death call get with too much delay.");
        }
        this.InvokeAfter(GameConfig.Instance.PlayerRespawnTime + 2, () =>
         {
             if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn != GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish)
                 Debug.LogWarning("For some reason this bot has not been destroyed yet.");
         });
    }

    void SyncTargetAI(NetHashTable data)
    {
        var view = (int)data["viewID"];
        if (view == -1)
        {
            SetTarget(null);
            return;
        }

        GameObject pr = FindPlayerRoot(view);
        if (pr == null)
        {
            Debug.Log($"Couldn't find the network view: {view}");
            return;
        }

        Transform t = pr.transform;
        if (t != null)
        {
            SetTarget(t);
        }
    }

    void OnLocalSpawn()
    {
        if (!isOneTeamMode && GameCore.LocalPlayer.Team == AITeam)
        {
            References.namePlateDrawer.enabled = true;
        }
    }

    /// <summary>
    /// 新玩家加入房间时
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (PhotonNetworkWrapper.IsMasterClient && newPlayer.ActorNumber != PhotonNetworkWrapper.LocalPlayer.ActorNumber)
        {
            //在 AIShooterHealth 中接收
            photonView.RPC("RpcSyncHealth", newPlayer, AIHealth.GetHealth());
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.onLocalPlayerSpawn += OnLocalSpawn;
        GameEvents.onMatchStart += OnMatchStart;
        GameEvents.onMatchStateChanged += OnMatchStateChange;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onLocalPlayerSpawn -= OnLocalSpawn;
        PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        GameEvents.onMatchStart -= OnMatchStart;
        GameEvents.onMatchStateChanged -= OnMatchStateChange;
    }

    void OnMatchStateChange(MatchState state)
    {
        if (state == MatchState.Waiting || state == MatchState.Starting) isGameStarted = false;
        else isGameStarted = MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;      
    }

    void OnMatchStart()
    {
        isGameStarted = GameController.Instance.GameMatchState != MatchState.Waiting && MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;
        UpdateTargetList();
    }

    public override void OnDeath() { CancelInvoke(); }

    private float Speed
    {
        get => Agent.speed;
        set
        {
            bool cr = Anim.GetBool(animationHash[0]);
            if (cr)
            {
                Agent.speed = 2;
            }
            else
            {
                Agent.speed = value;
            }
        }
    }

    public override Vector3 TargetPosition
    {
        get
        {
            if (Target != null) { return Target.position; }
            if (!isOneTeamMode && availableTargets.Count > 0)
            {
                Transform t = GetNearestPlayer();
                if (t != null)
                {
                    return t.position;
                }
                else { return m_Transform.position + (m_Transform.forward * 3); }
            }
            return Vector3.zero;
        }
    }

    private Vector3 GetPositionAround(Vector3 positionReference, float radius)
    {
        var v = Random.insideUnitSphere * radius;
        v.y = 0;
        v += positionReference;
        return v;
    }

    public Transform GetNearestPlayer()
    {
        if (availableTargets.Count > 0)
        {
            Transform t = null;
            float d = 1000;
            for (int i = 0; i < availableTargets.Count; i++)
            {
                if (availableTargets[i] == null || availableTargets[i].name.Contains("(die)")) continue;
                float dis = UtilityHelper.Distance(m_Transform.localPosition, availableTargets[i].position);
                if (dis < d)
                {
                    d = dis;
                    t = availableTargets[i];
                }
            }
            return t;
        }
        else { return null; }
    }

    public Transform GetTargetOnRange()
    {
        Transform t = null;
        if (availableTargets.Count <= 0) return t;

        List<Transform> inRange = new List<Transform>();
        float distance;
        for (int i = 0; i < availableTargets.Count; i++)
        {
            t = availableTargets[i];
            if (t == null || t.name.Contains("(die)")) continue;

            distance = UtilityHelper.Distance(m_Transform.localPosition, t.position);
            if (distance < soldierSettings.mediumRange)
            {
                inRange.Add(t);
            }
        }

        if (inRange.Count > 0) t = inRange[Random.Range(0, inRange.Count)];
        else t = null;

        return t;
    }

    private GamePlayer m_GameActor;
    public GamePlayer BotGameActor
    {
        get
        {
            if (m_GameActor == null) { m_GameActor = GameController.Instance.GetGamePlayer(AIName); }
            return m_GameActor;
        }
    }

    bool isNewDebug = false;
    public void SetDebugState(int stateID, bool initial = false)
    {
        if (!DebugStates) return;
        if (initial && isNewDebug)
        {
            DebugLine = $"{stateID}"; isNewDebug = true; return;
        }
        DebugLine += $"&{stateID}";
    }
    public bool Probability(float probability) { return Random.value <= probability; }
    public bool ForceFollowAtHalfHealth => AIHealth.GetHealth() < 50 && behaviorSettings.forceFollowAtHalfHealth;

    public float TargetDistance { get { return UtilityHelper.Distance(m_Transform.position, TargetPosition); } }
    public bool HasATarget { get => Target != null; }
    private bool CanCover(float inTimePassed) { return ((time - CoverTime) >= inTimePassed); }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (isDeath) return;
        if (Agent != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Agent.destination, 0.3f);
            Gizmos.DrawLine(m_Transform.localPosition, Agent.destination);

            if (Agent.path != null && Agent.path.corners != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < Agent.path.corners.Length; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    else if (i == 1)
                    {
                        Gizmos.DrawSphere(Agent.path.corners[i], 0.2f);
                    }

                    Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
                }
            }
        }
        if (Target != null)
        {
            Gizmos.color = playerInFront && !ObstacleBetweenTarget ? Color.green : Color.cyan;
            Gizmos.DrawLine(m_Transform.localPosition, TargetPosition);
        }
        if (m_Transform == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(m_Transform.localPosition, LookAtPosition);
        Gizmos.DrawWireCube(LookAtPosition, Vector3.one * 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!DebugStates || soldierSettings == null) return;
        if (m_Transform == null) { m_Transform = transform; }
        Gizmos.color = Color.yellow;
        UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.limitRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.farRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.yellow;
        UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.mediumRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.closeRange, 360, 12, Quaternion.identity);
    }
#endif
}