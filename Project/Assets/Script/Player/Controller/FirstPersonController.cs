using Game.PlayerController;
using Game.Runtime.Level;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : FirstPersonControllerBase
{
    [Header("设置")]
    public float WalkSpeed = 4.5f;
    [FormerlySerializedAs("m_CrouchSpeed")]
    public float runSpeed = 8;
    public float stealthSpeed = 1;
    [FormerlySerializedAs("m_CrouchSpeed")]
    public float crouchSpeed = 2;
    public float slideSpeed = 10;
    [FormerlySerializedAs("m_ClimbSpeed")]
    public float climbSpeed = 1f;
    [FormerlySerializedAs("m_JumpSpeed")]
    public float jumpSpeed;
    public float acceleration = 9;
    public float slopeFriction = 3f;

    public float crouchTransitionSpeed = 0.25f;
    public float crouchHeight = 1.4f;

    public bool canSlide = true;
    [Range(0.2f, 1.5f)] public float slideTime = 0.75f;
    [Range(1, 12)] public float slideFriction = 10;
    [Range(0.1f, 2.5f)] public float slideCoolDown = 1.2f;
    public float slideCameraTiltAngle = -22;

    [Range(0, 2)] public float JumpMinRate = 0.82f;
    public float jumpMomentumBooster = 2;
    public float momentunDecaySpeed = 5;
    [Range(0, 2)] public float AirControlMultiplier = 0.8f;
    public float m_StickToGroundForce;
    public float m_GravityMultiplier;

    [GameToggle] public bool RunFovEffect = true;
    public float runFOVAmount = 8;
    [GameToggle] public bool KeepToCrouch = true;
    public bool canStealthMode = true;
    [Header("坠落伤害")]
    [GameToggle] public bool FallDamage = true;
    [Range(0.1f, 5f)]
    public float SafeFallDistance = 3;
    [Range(3, 25)]
    public float DeathFallDistance = 15;
    public PlayerRunToAimBehave runToAimBehave = PlayerRunToAimBehave.StopRunning;

    [Header("空中控制")]
    public float dropControlSpeed = 25;
    public Vector2 dropTiltSpeedRange = new Vector2(20, 60);
    [Header("鼠标视角")] 
    [FormerlySerializedAs("m_MouseLook")]
    public MouseLook mouseLook;
    [FormerlySerializedAs("HeatRoot")]
    public Transform headRoot;
    public Transform CameraRoot;
    [Header("头部摇晃")]
    [Range(0, 1.2f)] public float headBobMagnitude = 0.9f;
    public float headVerticalBobMagnitude = 0.4f;
    public LerpControlledBob m_JumpBob = new LerpControlledBob();
    [Header("脚步声")]
    public Footstep footstep;
    public AudioClip jumpSound;           // the sound played when character leaves the ground.
    public AudioClip landSound;           // the sound played when character touches back on ground.
    public AudioClip slideSound;

    [Header("界面")]
    public Sprite StandIcon;
    public Sprite CrouchIcon;

    public float RunFov { get; set; } = 0;
    public CollisionFlags m_CollisionFlags { get; set; }
    public override Vector3 Velocity { get; set; }
    public override float VelocityMagnitude { get; set; }
    public override bool isControlable { get; set; } = true;

    private bool hasPlatformJump = false;
    private float PlatformJumpForce = 0;
    private bool _jump, jumpPressed;
    private Vector2 _input = new Vector2();
    private Vector3 targetDirection, moveDirection = Vector3.zero;
    private bool _previouslyGrounded = true;
    private bool _jumping = false;
    private bool Crounching = false;
    private AudioSource _audioSource;
    private bool Finish = false;
    private Vector3 defaultCameraRPosition;
    private bool isClimbing, isAiming = false;
    private ClimbableLadder _ladder;
    private bool MoveToStarted = false;
    private float PostGroundVerticalPos = 0;
    private bool isFalling = false;
    private int JumpDirection = 0;
    private float HigherPointOnJump;
    private CharacterController _characterController;
    private float lastJumpTime = 0;
    private float WeaponWeight = 1;
    private bool hasTouchGround = false;
    private bool JumpInmune = false;
    private Transform _transform;
    private RaycastHit[] SurfaceRay = new RaycastHit[1];
    private Vector3 desiredMove, momentum = Vector3.zero;
    private float VerticalInput, HorizontalInput;
    private bool lastCrouchState = false;
    private float fallingTime = 0;
    private bool haslanding = false;
    private float capsuleRadious;
    private readonly Vector3 feetPositionOffset = new Vector3(0, 0.8f, 0);
    private float slideForce = 0;
    private float lastSlideTime = 0;
    private PlayerReferencesHandler playerReferences;
    private Vector3 forwardVector = Vector3.forward;
    private PlayerState lastState = PlayerState.Idle;
    private bool forcedCrouch = false;
    private Vector3 surfaceNormal, surfacePoint = Vector3.zero;
    private float defaultStepOffset = 0.4f;
    private float desiredSpeed = 4;
    private float defaultHeight = 2;
    private bool overrideNextLandEvent = false;

    protected override void Awake()
    {
        if (!photonView.IsMine)
            return;

        base.Awake();
        _transform = transform;
        playerReferences = GetComponent<PlayerReferencesHandler>();
        _characterController = playerReferences.characterController;
        defaultCameraRPosition = CameraRoot.localPosition;
        _audioSource = gameObject.AddComponent<AudioSource>();
        mouseLook.Init(_transform, headRoot);
        lastJumpTime = Time.time;
        defaultStepOffset = _characterController.stepOffset;
        capsuleRadious = _characterController.radius * 0.5f;
        defaultHeight = _characterController.height;
        isControlable = MatchTimeManagerBase.HaveTimeStarted();
    }

    protected override void OnEnable()
    {
        GameEvents.onRoundEnd += OnRoundEnd;
        GameEvents.onChangeWeapon += OnChangeWeapon;
        GameEvents.onMatchStart += OnMatchStart;
        GameEvents.onGameSettingsChange += OnGameSettingsChanged;
        GameEvents.onLocalAimChanged += OnAimChange;
    }

    protected override void OnDisable()
    {
        GameEvents.onRoundEnd -= OnRoundEnd;
        GameEvents.onChangeWeapon -= OnChangeWeapon;
        GameEvents.onMatchStart -= OnMatchStart;
        GameEvents.onGameSettingsChange -= OnGameSettingsChanged;
        GameEvents.onLocalAimChanged -= OnAimChange;
    }

    public override void OnUpdate()
    {
        Velocity = _characterController.velocity;
        VelocityMagnitude = Velocity.magnitude;

        if (Finish) return;

        MovementInput();
        CheckStates();
    }

    public override void OnLateUpdate()
    {
        UpdateMouseLook();

        if (Finish) return;
        if (_characterController == null || !_characterController.enabled) return;

        moveDirection = Vector3.Lerp(moveDirection, targetDirection, acceleration * Time.deltaTime);

        // 把移动方向应用到 Character Controller
        // 在 LateUpdate 里应用移动，避免移动并旋转玩家摄像机时画面抖动。
        m_CollisionFlags = _characterController.Move(moveDirection * Time.deltaTime);
    }

    public void FixedUpdate()
    {
        if (Finish || _characterController == null || !_characterController.enabled || MoveToStarted)
            return;

        GroundDetection();
        //如果玩家焦点在游戏内
        if (RoomMenu.Instance.isCursorLocked && !GameConfig.Instance.isChating)
        {
            //确定玩家速度
            GetInput(out float s);
            desiredSpeed = Mathf.Lerp(desiredSpeed, s, Time.fixedDeltaTime * 8);
            speed = desiredSpeed;
        }
        else if (State != PlayerState.Sliding)//if player is not focus in game
        {
            _input = Vector2.zero;
        }

        if (isClimbing && _ladder != null)
        {
            //攀爬控制
            OnClimbing();
        }
        else
        {
            //玩家移动
            Move();
        }
    }

    // 该玩家控制器的状态变化时触发。
    private void OnStateChanged(PlayerState from, PlayerState to)
    {
        if (from == PlayerState.Crouching || to == PlayerState.Crouching)
        {
            DoCrouchTransition();
        }
        else if (from == PlayerState.Sliding || to == PlayerState.Sliding)
        {
            DoCrouchTransition();
        }
        GameEvents.DispatchLocalPlayerStateChange(from, to);
    }

    // 处理玩家控制器的按键输入
    void MovementInput()
    {
        jumpPressed |= GameInput.Jump();
        if (State == PlayerState.Sliding)
        {
            slideForce -= Time.deltaTime * slideFriction;
            speed = slideForce;
            if (GameInput.Jump())
            {
                State = PlayerState.Jumping;
                _jump = true;
            }
            else return;
        }

        if (UtilityHelper.isMobile) return;

        if (!_jump && State != PlayerState.Crouching && (Time.time - lastJumpTime) > JumpMinRate)
        {
            _jump = GameInput.Jump();
        }

        if (State != PlayerState.Jumping && State != PlayerState.Climbing)
        {
            if (forcedCrouch) return;
            if (KeepToCrouch)
            {
                Crounching = GameInput.Crouch();
                if (Crounching != lastCrouchState)
                {
                    OnCrouchChanged();
                    lastCrouchState = Crounching;
                }
            }
            else
            {
                if (GameInput.Crouch(GameInputType.Down))
                {
                    Crounching = !Crounching;
                    OnCrouchChanged();
                }
            }
        }
    }

    // 检查玩家是否站在地面上（不在跳跃或下落）
    void GroundDetection()
    {
        //如果玩家下落之后接触了地面
        if (!_previouslyGrounded && _characterController.isGrounded)
        {
            OnLand();
        }
        else if (_previouslyGrounded && !_characterController.isGrounded)//when the player start jumping
        {
            PostGroundVerticalPos = _transform.position.y;
            if (!isFalling)
            {
                isFalling = true;
                fallingTime = Time.time;
            }
        }

        if (isFalling)
        {
            VerticalDirectionCheck();
        }

        if (!_characterController.isGrounded && !_jumping && _previouslyGrounded)
        {
            targetDirection.y = 0f;
        }

        if (forcedCrouch)
        {
            if ((Time.frameCount % 10) == 0)
            {
                if (!IsHeadHampered())
                {
                    forcedCrouch = false;
                    State = PlayerState.Idle;
                }
            }
        }

        _previouslyGrounded = _characterController.isGrounded;
    }

    // 把移动方向应用到 CharacterController
    void Move()
    {
        //如果玩家接触地面
        if (_characterController.isGrounded)
        {
            OnSurface();
            //垂直阻力
            moveDirection.y = -m_StickToGroundForce;
            hasTouchGround = true;
            //有待执行的跳跃
            if (_jump || hasPlatformJump)
            {
                DoJump();
            }
        }
        else//if the player is not touching the ground
        {
            //如果玩家正在下坠
            if (State == PlayerState.Dropping)
            {
                //空中移动由单独流程处理
                OnDropping();
                return;
            }
            else if (State == PlayerState.Gliding)
            {
                //滑翔移动由单独函数处理
                OnGliding();
                return;
            }

            OnAir();
        }
    }

    // 玩家在地面上时的控制
    void OnSurface()
    {
        // 始终沿摄像机前方移动，因为那是瞄准方向
        // desiredMove = (_transform.forward * _input.y) + (_transform.right * _input.x);

        desiredMove.Set(_input.x, 0.0f, _input.y);
        desiredMove = _transform.TransformDirection(desiredMove);

        // 获取所接触地面的法线，以便沿地面移动
        Physics.SphereCastNonAlloc(_transform.localPosition, capsuleRadious, Vector3.down, SurfaceRay, _characterController.height * 0.5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        //根据当前玩家所踩地面的法线确定移动角度
        desiredMove = Vector3.ProjectOnPlane(desiredMove, SurfaceRay[0].normal);
        targetDirection.x = desiredMove.x * speed;
        targetDirection.z = desiredMove.z * speed;

        SlopeControl();
    }

    // 玩家在空中时的控制（既不在下坠也不在滑翔）
    void OnAir()
    {
        desiredMove = (_transform.forward * Mathf.Clamp01(_input.y)) + (_transform.right * _input.x);
        desiredMove += momentum;

        targetDirection += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        targetDirection.x = (desiredMove.x * speed) * AirControlMultiplier;
        targetDirection.z = (desiredMove.z * speed) * AirControlMultiplier;

        momentum = Vector3.Lerp(momentum, Vector3.zero, Time.fixedDeltaTime * momentunDecaySpeed);
    }

    // 玩家下落/跳跃后撞到地面时调用
    void OnLand()
    {
        if (isClimbing) return;

        if (!overrideNextLandEvent)
        {
            //落地摄像机效果
            StartCoroutine(m_JumpBob.DoBobCycle());

            isFalling = false;
            float fallDistance = CalculateFall();
            if (fallDistance > 0.05f) GameEvents.DispatchPlayerLandEvent(fallDistance);
        }
        else overrideNextLandEvent = false;

        haslanding = true;
        JumpDirection = 0;
        targetDirection.y = 0f;
        _jumping = false;
        fallingTime = 0;
        HigherPointOnJump = _transform.position.y;
        _characterController.stepOffset = defaultStepOffset;
        if (State != PlayerState.Crouching)
            State = PlayerState.Idle;
    }

    void OnCrouchChanged()
    {
        if (Crounching)
        {
            State = PlayerState.Crouching;
            UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = CrouchIcon;

            //滑铲实现
            if (canSlide && VelocityMagnitude > WalkSpeed && GetLocalVelocity().z > 0.1f)
            {
                DoSlide();
            }
        }
        else
        {
            if (!IsHeadHampered())
            {
                State = PlayerState.Idle;
                UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = StandIcon;
            }
            else forcedCrouch = true;
        }
    }

    public void DoCrouchTransition()
    {
        StopCoroutine(nameof(CrouchTransition));
        StartCoroutine(nameof(CrouchTransition));
    }

    IEnumerator CrouchTransition()
    {
        bool isCrouch = Crounching || State == PlayerState.Sliding;
        float height = isCrouch ? crouchHeight : defaultHeight;
        Vector3 center = new Vector3(0, height * 0.5f, 0);
        Vector3 cameraPosition = CameraRoot.localPosition;
        Vector3 verticalCameraPos = isCrouch ? new Vector3(cameraPosition.x, defaultCameraRPosition.y - (defaultHeight - height), cameraPosition.z) : defaultCameraRPosition;

        float originHeight = _characterController.height;
        Vector3 originCenter = _characterController.center;
        Vector3 originCameraPosition = cameraPosition;

        float d = 0;
        while (d < 1)
        {
            d += Time.deltaTime / crouchTransitionSpeed;
            _characterController.height = Mathf.Lerp(originHeight, height, d);
            _characterController.center = Vector3.Lerp(originCenter, center, d);
            CameraRoot.localPosition = Vector3.Lerp(originCameraPosition, verticalCameraPos, d);
            yield return null;
        }
    }

    // 让玩家跳跃
    public override void DoJump()
    {
        momentum = desiredMove * jumpMomentumBooster;
        moveDirection.y = (hasPlatformJump) ? PlatformJumpForce : jumpSpeed;
        targetDirection.y = moveDirection.y;
        PlayJumpSound();
        _jump = false;
        _jumping = true;
        hasPlatformJump = false;
        if (State == PlayerState.Sliding) mouseLook.SetTiltAngle(0);
        Crounching = false;
        State = PlayerState.Jumping;
        lastJumpTime = Time.time;
        _characterController.stepOffset = 0;
        m_CollisionFlags = _characterController.Move(moveDirection * Time.deltaTime);
    }

    // 让玩家滑铲
    public override void DoSlide()
    {
        if ((Time.time - lastSlideTime) < slideTime * slideCoolDown) return;//wait the equivalent of one extra slide before be able to slide again
        if (_jumping) return;

        Vector3 startPosition = (_transform.position - feetPositionOffset) + (_transform.forward * _characterController.radius);
        if (Physics.Linecast(startPosition, startPosition + _transform.forward)) return;//there is something in front of the feet's

        State = PlayerState.Sliding;
        slideForce = slideSpeed;//slide force will be continually decreasing
        speed = slideSpeed;
        //playerReferences.gunManager.HeadAnimator.Play("slide-start", 0, 0); // 想改用动画播放就解开这行
        mouseLook.SetTiltAngle(slideCameraTiltAngle);
        if (slideSound != null)
        {
            _audioSource.clip = slideSound;
            _audioSource.volume = 0.7f;
            _audioSource.Play();
        }
        mouseLook.UseOnlyCameraRotation();
        this.InvokeAfter(slideTime, () =>
        {
            if (Crounching && !UtilityHelper.isMobile)
                State = PlayerState.Crouching;
            else if (State != PlayerState.Jumping)
                State = PlayerState.Idle;

            Crounching = false;
            DoCrouchTransition();
            lastSlideTime = Time.time;
            mouseLook.SetTiltAngle(0);
            mouseLook.PortBodyOrientationToCamera();
        });
    }

    // 检测坡度上限并应用滑铲物理。
    void SlopeControl()
    {
        float angle = Vector3.Angle(Vector3.up, surfaceNormal);

        if (angle <= _characterController.slopeLimit || angle >= 75) return;

        Vector3 direction = Vector3.up - surfaceNormal * Vector3.Dot(Vector3.up, surfaceNormal);
        float speed = slideSpeed + 1 + Time.deltaTime;

        targetDirection += direction * -speed;
        targetDirection.y = targetDirection.y - surfacePoint.y;
    }

    // 让玩家进入下坠
    public override void DoDrop()
    {
        if (isGrounded)
        {
            Debug.Log("Can't drop when player is in a surface");
            return;
        }
        State = PlayerState.Dropping;
    }

    // 玩家下坠时每帧调用（下落控制开启）
    void OnDropping()
    {
        //获取摄像机倒置角度
        float tilt = Mathf.InverseLerp(0, 90, mouseLook.VerticalAngle);
        //归一化
        tilt = Mathf.Clamp01(tilt);
        if (mouseLook.VerticalAngle <= 0 || mouseLook.VerticalAngle >= 180) tilt = 0;
        //获取玩家摄像机的前方向
        desiredMove = headRoot.forward * Mathf.Clamp01(_input.y);
        if (desiredMove.y > 0) desiredMove.y = 0;

        //根据摄像机倒置角度计算下坠速度
        float dropSpeed = Mathf.Lerp(m_GravityMultiplier * dropTiltSpeedRange.x, m_GravityMultiplier * dropTiltSpeedRange.y, tilt);
        targetDirection = Physics.gravity * dropSpeed * Time.fixedDeltaTime;
        //如果玩家按下纵向输入 -> 沿摄像机注视方向加速
        targetDirection += desiredMove * dropControlSpeed;

        //把移动方向应用到 Character Controller
        m_CollisionFlags = _characterController.Move(targetDirection * Time.fixedDeltaTime);
    }

    // 让玩家滑翔
    public override void DoGliding()
    {
        if (isGrounded)
        {
            Debug.Log("Can't gliding when player is in a surface");
            return;
        }
        State = PlayerState.Gliding;
    }

    // 玩家滑翔时每帧调用
    void OnGliding()
    {
        desiredMove = (_transform.forward * _input.y) + (_transform.right * _input.x);
        //玩家在空中时能施加多少控制。
        float airControlMult = AirControlMultiplier * 5;
        //下落重力大小
        float gravity = m_GravityMultiplier * 15;

        targetDirection = Physics.gravity * gravity * Time.fixedDeltaTime;
        targetDirection.x = (desiredMove.x * speed) * airControlMult;
        targetDirection.z = (desiredMove.z * speed) * airControlMult;

        m_CollisionFlags = _characterController.Move(targetDirection * Time.fixedDeltaTime);
    }

    void OnClimbing()
    {
        if (_ladder.Status == ClimbableLadder.LadderStatus.Detaching)
        {
            if (!_ladder.Exiting)
            {
                _ladder.Exiting = true;
                bool wasControllable = isControlable;
                isControlable = false;

                StartCoroutine(MoveTo(_ladder.GetNearestExitPosition(_transform), () =>
                {
                    SetActiveClimbing(false);
                    _ladder.JumpOut();
                    _ladder = null;
                    isControlable = true;
                    GameEvents.onPlayerLand(0.5f);
                }));
            }
        }
        else
        {
            desiredMove = _ladder.transform.rotation * forwardVector * _input.y;
            targetDirection.y = desiredMove.y * climbSpeed;
            targetDirection.x = desiredMove.x * climbSpeed;
            targetDirection.z = desiredMove.z * climbSpeed;

            if (jumpPressed)
            {
                SetActiveClimbing(false);
                _ladder.JumpOut();
                _ladder = null;

                targetDirection = -_transform.forward * 20;
                targetDirection.y = jumpSpeed;
                lastJumpTime = Time.time;
                jumpPressed = false;
                _jump = false;
            }
            m_CollisionFlags = _characterController.Move(targetDirection * Time.smoothDeltaTime);

            if (_ladder != null) _ladder.WatchLimits();
        }
    }

    // 坠落伤害计算原理
    private float CalculateFall()
    {
        float fallDistance = HigherPointOnJump - _transform.position.y;
        if (JumpDirection == -1)
        {
            float normalized = PostGroundVerticalPos - _transform.position.y;
            fallDistance = Mathf.Abs(normalized);
        }

        if (FallDamage && hasTouchGround && haslanding)
        {
            if (JumpInmune)
            {
                JumpInmune = false;
                return fallDistance;
            }
            if ((Time.time - fallingTime) <= 0.4f)
            {
                if (fallDistance > 0.05f) GameEvents.DispatchPlayerLandEvent(0.2f);
                return fallDistance;
            }

            float ave = fallDistance / DeathFallDistance;
            if (fallDistance > SafeFallDistance && State != PlayerState.InVehicle)
            {
                int damage = Mathf.FloorToInt(ave * 100);
                playerReferences.playerHealthManager.DoFallDamage(damage);
            }

            PlayLandingSound(ave);
            fallingTime = Time.time;
        }
        else PlayLandingSound(1);
        return fallDistance;
    }

    // 检查玩家正朝哪个纵向方向移动
    void VerticalDirectionCheck()
    {
        if (_transform.position.y == PostGroundVerticalPos) return;

        //如果方向还没确定
        if (JumpDirection == 0)
        {
            //玩家在起跳面之上还是之下？
            // 1 = 上方（跳跃），-1 = 下方（下落）
            JumpDirection = (_transform.position.y > PostGroundVerticalPos) ? 1 : -1;
        }
        else if (JumpDirection == 1)//if the player jump
        {
            //但还没开始下落
            if (_transform.position.y < PostGroundVerticalPos)
            {
                //获取跳跃到达的最高点
                HigherPointOnJump = PostGroundVerticalPos;
            }
            else//if still going up
            {
                PostGroundVerticalPos = _transform.position.y;
            }
        }
        else//if the player was falling without jumping
        {

        }
    }

    private void GetInput(out float outputSpeed)
    {
        if (!isControlable)
        {
            _input = Vector2.zero;
            outputSpeed = 0;
            return;
        }

        // 读取输入
        HorizontalInput = GameInput.Horizontal;
        VerticalInput = GameInput.Vertical;

        if (State == PlayerState.Sliding)
        {
            VerticalInput = 1;
            HorizontalInput = 0;
        }

        // _input = Vector2.Lerp(_input, new Vector2(HorizontalInput, VerticalInput), Time.deltaTime * 25);
        _input.Set(HorizontalInput, VerticalInput);

        float inputMagnitude = _input.magnitude;
        //如果玩家正在下坠，速度在下坠函数里计算
        if (State == PlayerState.Dropping || State == PlayerState.Gliding) { outputSpeed = 0; return; }

        if (State != PlayerState.Climbing && State != PlayerState.Sliding)
        {
            if (inputMagnitude > 0 && State != PlayerState.Crouching)
            {
                if (VelocityMagnitude > 0)
                {
                    float forwardVelocity = GetLocalVelocity().z;
                    if (!UtilityHelper.isMobile)
                    {
                        // 独立构建版本中，按键可切换行走/奔跑速度。
                        // 记录角色当前是行走还是奔跑
                        if (GameInput.Run() && forwardVelocity > 0.1f)
                        {
                            if (runToAimBehave == PlayerRunToAimBehave.AimWhileRunning)
                                State = PlayerState.Running;
                            else if (runToAimBehave == PlayerRunToAimBehave.StopRunning)
                                State = isAiming ? PlayerState.Walking : PlayerState.Running;
                        }
                        else if (canStealthMode && GameInput.Stealth() && VelocityMagnitude > 0.1f)
                        {
                            State = PlayerState.Stealth;
                        }
                        else
                        {
                            State = PlayerState.Walking;
                        }
                    }
                    else
                    {
                        if (VerticalInput > 1 && forwardVelocity > 0.1f)
                        {
                            State = PlayerState.Running;
                        }
                        else if (canStealthMode && VerticalInput > 0.05f && VerticalInput <= 0.15f)
                        {
                            State = PlayerState.Stealth;
                        }
                        else
                        {
                            State = PlayerState.Walking;
                        }
                    }
                }
                else
                {
                    if (State != PlayerState.Jumping)
                    {
                        State = PlayerState.Idle;
                    }
                }

            }
            else if (_characterController.isGrounded)
            {
                if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }

        if (Crounching)
        {
            outputSpeed = (State == PlayerState.Crouching) ? crouchSpeed : runSpeed;
            if (State == PlayerState.Sliding)
            {
                outputSpeed += 1;
            }
        }
        else
        {
            // 把期望速度设为行走或奔跑速度
            outputSpeed = (State == PlayerState.Running && _characterController.isGrounded) ? runSpeed : WalkSpeed;
            if (State == PlayerState.Stealth)
            {
                outputSpeed = stealthSpeed;
            }
        }
        // 输入合成长度超过 1 时归一化：
        if (inputMagnitude > 1)
        {
            _input.Normalize();
        }

        if (RunFovEffect)
        {
            float rf = (State == PlayerState.Running && _characterController.isGrounded) ? runFOVAmount : 0;
            RunFov = Mathf.Lerp(RunFov, rf, Time.deltaTime * 6);
        }
    }

    // 本帧强制玩家停下
    public override void Stop()
    {
        _input = Vector2.zero;
        moveDirection = Vector3.zero;
        desiredMove = Vector3.zero;
        targetDirection = Vector3.zero;
        State = PlayerState.Idle;
    }

    public override void PlayFootStepSound()
    {
        if (State == PlayerState.Sliding || footstep == null) return;
        if (!_characterController.isGrounded && !isClimbing)
            return;

        if (!isClimbing)
        {
            if (State == PlayerState.Stealth || State == PlayerState.Crouching)
            {
                footstep?.SetVolumeMuliplier(footstep.stealthModeVolumeMultiplier);
            }
            else footstep?.SetVolumeMuliplier(1f);

            footstep?.DetectAndPlaySurface();
        }
        else
        {
            footstep?.PlayStepForTag("Generic");
        }
    }

    public override void PlatformJump(float force)
    {
        hasPlatformJump = true;
        PlatformJumpForce = force;
        JumpInmune = true;
    }


    public override void UpdateMouseLook()
    {
        mouseLook.Update();

        if (GameInput.InputFocus != GameInputFocus.Player) return;

        if (!isClimbing)
        {
            mouseLook.UpdateLook(_transform, headRoot);
        }
        else
        {
            mouseLook.UpdateLook(_transform, headRoot, _ladder);
        }
    }

    private void CheckStates()
    {
        if (lastState == State) return;
        OnStateChanged(lastState, State);
        lastState = State;
    }

    private void PlayLandingSound(float vol = 1)
    {
        vol = Mathf.Clamp(vol, 0.05f, 1);
        _audioSource.clip = landSound;
        _audioSource.volume = vol;
        _audioSource.Play();
    }

    private void PlayJumpSound()
    {
        _audioSource.volume = 1;
        _audioSource.clip = jumpSound;
        _audioSource.Play();
    }

    // 检查玩家头顶是否有障碍物
    // <returns></returns>
    public bool IsHeadHampered()
    {
        Vector3 origin = _transform.localPosition + _characterController.center + Vector3.up * _characterController.height * 0.5F;
        float dist = 2.05f - _characterController.height;
        return Physics.Raycast(origin, Vector3.up, dist);
    }

    void OnRoundEnd()
    {
        Finish = true;
        isControlable = false;
        Stop();
    }

    void OnChangeWeapon(int id)
    {
        isAiming = false;
        var currentWeapon = playerReferences.gunManager.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            WeaponWeight = currentWeapon.WeaponWeight;
        }
        else
        {
            WeaponWeight = 0;
        }
    }
    void OnMatchStart()
    {
        isControlable = true;
    }
    void OnGameSettingsChanged() => mouseLook.FetchSettings();
    void OnAimChange(bool aim)
    {
        isAiming = aim;
        mouseLook.OnAimChange(aim);
    }

    private void SetActiveClimbing(bool active)
    {
        isClimbing = active;
        State = (isClimbing) ? PlayerState.Climbing : PlayerState.Idle;
        if (isClimbing) InputInteractionIndicator.ShowIndication(InputHandler.GetButtonName("Jump"), GameTexts.JumpOffLadder);
        else InputInteractionIndicator.SetActiveIfSame(false, GameTexts.JumpOffLadder);
    }

    IEnumerator MoveTo(Vector3 position, Action onFinish)
    {
        if (_transform == null) _transform = transform;

        float t = 0;
        Vector3 from = _transform.localPosition;
        _characterController.enabled = false;
        while (t < 1)
        {
            t += Time.deltaTime / 0.5f;
            _transform.localPosition = Vector3.Lerp(from, position, t);
            yield return null;
        }
        _characterController.enabled = true;
        onFinish?.Invoke();
    }

    private void CheckLadderTrigger(Collider other)
    {
        // 碰撞体没有父节点就不是有效梯子
        if (isClimbing || other.transform.parent == null)
            return;

        var ladder = other.GetComponentInParent<ClimbableLadder>();
        if (ladder == null || !ladder.CanUse)
            return;

        // 进入梯子触发器

        Stop();
        _ladder = ladder;
        // 根据玩家身高设置上梯和下梯位置
        ladder.SetUpBounds(playerReferences);

        bool wasControllable = isControlable;
        isControlable = false;
        JumpInmune = true;
        jumpPressed = false;

        SetActiveClimbing(true);

        // 获取让玩家自动移动到开始爬梯/下梯的位置
        Vector3 startPos = ladder.GetAttachPosition(other, _characterController.height);
        overrideNextLandEvent = true;
        // 把玩家移动到起始位置
        StartCoroutine(MoveTo(startPos, () =>
        {
            // 位置调整完成之后
            isControlable = wasControllable;
            ladder.Status = ClimbableLadder.LadderStatus.Climbing;

            // 之后的移动由 OnClimbing() 函数处理
        }));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        CheckLadderTrigger(other);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        surfaceNormal = hit.normal;
        surfacePoint = hit.point;
        // 想让玩家控制器在碰撞时对刚体施力就启用这段
        // 出于性能考虑默认注释掉。
        /* Rigidbody body = hit.collider.attachedRigidbody;
         // 角色踩在刚体上时不要推动它
         if (m_CollisionFlags == CollisionFlags.Below)
         {
             return;
         }

         if (body == null || body.isKinematic)
         {
             return;
         }
         body.AddForceAtPosition(_characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);*/
    }

    internal float _speed = 0;
    public float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value - WeaponWeight;
            float min = 1.75f;
            if (State == PlayerState.Stealth)
            {
                min = 1;
            }
            _speed = Mathf.Max(_speed, min);
        }
    }

    public override float GetCurrentSpeed()
    {
        return speed;
    }

    public override float GetSpeedOnState(PlayerState playerState, bool includeModifiers)
    {
        switch (playerState)
        {
            case PlayerState.Walking:
                return includeModifiers ? WalkSpeed - WeaponWeight : WalkSpeed;
            case PlayerState.Running:
                return includeModifiers ? runSpeed - WeaponWeight : runSpeed;
            case PlayerState.Crouching:
                return includeModifiers ? crouchSpeed - WeaponWeight : crouchSpeed;
            case PlayerState.Dropping:
                return dropTiltSpeedRange.y;
        }
        return includeModifiers ? WalkSpeed - WeaponWeight : WalkSpeed;
    }

    public override PlayerRunToAimBehave GetRunToAimBehave()
    {
        return runToAimBehave;
    }

    public override MouseLookBase GetMouseLook()
    {
        return mouseLook;
    }

    public override Footstep GetFootStep()
    {
        return footstep;
    }

    public override float GetSprintFov()
    {
        return RunFov;
    }

    public override float GetHeadBobMagnitudes(bool vertical)
    {
        if (vertical) return headVerticalBobMagnitude;
        return headBobMagnitude;
    }

    public Vector3 GetLocalVelocity() => _transform.InverseTransformDirection(Velocity);
    public Vector3 MovementDirection => targetDirection;
    public override bool isGrounded { get { return _characterController.isGrounded; } }

    [SerializeField]
    public class LerpControlledBob
    {
        public float BobDuration;
        public float BobAmount;

        private float _offset = 0f;

        // 提供可用的偏移量
        public float Offset()
        {
            return _offset;
        }

        public IEnumerator DoBobCycle()
        {
            // 让摄像机略微下移
            float t = 0f;
            while (t < BobDuration)
            {
                _offset = Mathf.Lerp(0f, BobAmount, t / BobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // 让它回到中位
            t = 0f;
            while (t < BobDuration)
            {
                _offset = Mathf.Lerp(BobAmount, 0f, t / BobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            _offset = 0f;
        }
    }
}