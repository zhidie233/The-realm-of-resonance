using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirearmController : FirearmBase
{
    public CustomFirearmBase customWeapon;
    public float CrossHairScale = 8;
    public ShotInstanceMethod bulletInstanceMethod = ShotInstanceMethod.Pooled;
    public string BulletName = "bullet";
    public GameObject bulletPrefab;
    public GameObject grenade = null;
    public ParticleSystem muzzleFlash = null;
    public Transform muzzlePoint = null;
    public WeaponEffectsBase weaponFX = null;
    public ParticleSystem shell = null;
    public GameObject impactEffect = null;
    public Vector3 AimPosition;
    public Vector3 aimRotation = Vector3.zero;
    public bool useSmooth = true;
    public float AimSmooth;
    public ShakePreset shakerPresent;
    [Range(0, 179)]
    public float aimZoom = 50;
    public bool CanAuto = true;
    public bool CanSemi = true;
    public bool CanSingle = true;
    //霰弹枪专用变量
    public int pelletsPerShot = 10;
    public float delayForSecondFireSound = 0.45f;
    //点射专用变量
    public int roundsPerBurst = 3;
    public float lagBetweenBurst = 0.5f;
    //发射器专用变量
    public List<GameObject> OnAmmoLauncher = new List<GameObject>();
    public bool ThrowByAnimation = false;
    public int impactForce = 50;
    public float bulletSpeed = 200.0f;
    public float bulletDropFactor = 1;
    public bool AutoReload = true;
    public bool m_AllowQuickFire = true;
    public ReloadPer reloadPer = ReloadPer.Bullet;
    public int bulletsPerClip = 50;
    public int numberOfClips = 5;
    public int maxNumberOfClips = 10;
    public float DelayFire = 0.85f;
    public float delayFireOnSprinting = 0.12f;
    public Vector2 spreadMinMax = new Vector2(1, 3);
    public float spreadAimMultiplier = 0.5f;
    public float spreadPerSecond = 0.2f;
    public float spread = 0.0f;
    public float decreaseSpreadPerSec = 0.5f;
    [HideInInspector] public bool isReloading = false;
    // 后坐力
    public float RecoilAmount = 5.0f;
    public float RecoilSpeed = 2;
    public bool SoundReloadByAnim = false;
    public AudioClip TakeSound;
    public AudioClip FireSound;
    public AudioClip DryFireSound;
    public AudioClip ReloadSound;
    public AudioClip ReloadSound2 = null;
    public AudioClip ReloadSound3 = null;
    public AudioSource DelaySource = null;
    //缓存的玩家组件
    public Renderer[] weaponRenders = null;
    public PlayerConfig playerSettings;

    public int Damage
    {
        get { return Info.Damage + extraDamage; }
        set { extraDamage = value; }
    }

    private int _remainingClips = 5;
    public int RemainingClips
    {
        get => _remainingClips;
        set => _remainingClips = value;
    }

    public float Zoom
    {
        get { return aimZoom + extraZoom; }
        set { extraZoom = value; }
    }

    public int BulletsPerMagazine
    {
        get { return bulletsPerClip + extraBullets; }
        set { extraBullets = value; }
    }

    public float MaxSpread
    {
        get { return extraSpread; }
        set { extraSpread = value; }
    }

    private float extraWeight = 0;
    public float WeaponWeight
    {
        get => Info.Weight + extraWeight;
        set => extraWeight = value;
    }

    private float extraRecoil = 0;
    public float WeaponRecoil
    {
        get => RecoilAmount + extraRecoil;
        set => extraRecoil = value;
    }

    private float extraAimSpeed = 0;
    public float AimSpeed
    {
        get => AimSmooth + extraAimSpeed;
        set => extraAimSpeed = value;
    }

    private float extraReloadTime = 0;
    public float ReloadTime
    {
        get => Info.ReloadTime + extraReloadTime;
        set => extraReloadTime = value;
    }

    private float extraBulletSpeed = 0;
    public float BulletSpeed
    {
        get => bulletSpeed + extraBulletSpeed;
        set => extraBulletSpeed = value;
    }

    private int extraRange = 0;
    public int Range
    {
        get => Info.Range + extraRange;
        set => extraRange = value;
    }

    public AudioClip FireAudioClip
    {
        get => FireSound;
        set
        {
            if (defaultFireSound == null) defaultFireSound = FireSound;
            FireSound = value;
            if (FireSound == null) FireSound = defaultFireSound;
        }
    }

    public float nextFireTime { get; set; }
    public Camera PlayerCamera { get; private set; }
    public bool BlockAimFoV { get; set; }
    public bool HaveInfinityAmmo { get; private set; }
    public System.Action<bool> onWeaponRendersActive;

    private bool _enable = true;
    public bool canBeTakenWhenIsEmpty = true;
    private bool alreadyKnife = false;
    private AudioSource Source;
    private Camera WeaponCamera;
    private bool inReloadMode = false;
    private AmmunitionType AmmoType = AmmunitionType.Bullets;
    private AudioSource FireSource = null;
    private bool isInitialized = false;
    private Transform _transform;
    public ShotData BulletSettings = new ShotData();
    public PlayerFPState FPState = PlayerFPState.Idle;
    private Vector2 defaultSpreadRange;
    public int extraDamage, extraBullets = 0;
    private Transform defaultMuzzlePoint = null;
    private bool lastAimState = false;
    private float currentZoom, extraZoom, extraSpread = 0;
    private bool grenadeFired = false;
    GameObject instancedBullet = null;
    RaycastHit hit;
    private Vector3 DefaultPos;
    private Vector3 CurrentPos;
    private Quaternion currentRotation, defaultRotation;
    private bool isBursting = false;
    private static ShotInstanceData bulletInstanceData;
    private AudioClip defaultFireSound;
    private float lastShotTime = 0;
    private bool pendingFirstShot = false;
    private bool _canFire = false;
    private bool _canAim;
    private FirearmType currentGunType = FirearmType.None;

    protected override void Awake()
    {
        base.Awake();
        Initialized();
    }
    // 启动时武器管理器会对所有已装备武器调用。
    public void Initialized()
    {
        if (isInitialized)
        {
            FireSource.volume = Source.volume;
            return;
        }

        _transform = transform;
        playerSettings = PlayerReferences.playerSettings;

        if (FireSource == null) FireSource = PlayerReferences.gunManager.GetFireAudioSource();
        Source = GetComponent<AudioSource>();
        FireSource.volume = Source.volume;
        PlayerCamera = PlayerReferences.playerCamera;

        defaultSpreadRange = spreadMinMax;
        AmmoType = GameConfig.Instance.AmmoType;
        ReticleBase.Instance.Block = false;
        Setup();
        customWeapon?.Initialitate(this);
        isInitialized = true;
    }

    protected override void OnEnable()
    {

        if (!isInitialized) return;

        base.OnEnable();
        PlayClip(TakeSound);
        CanFire = false;
        pendingFirstShot = false;
        UpdateUI();
        if (WeaponAnimation)
        {
            float t = WeaponAnimation.PlayTakeIn();
            Invoke(nameof(DrawComplete), t);
        }
        else
        {
            DrawComplete();
        }
        GameEvents.onAmmoPickUp += this.OnPickUpAmmo;
        GameEvents.onRoundEnd += this.OnRoundEnd;
        ReticleBase.Instance.SetupCrosshairForWeapon(this);
        HeldWeaponUIBase.Instance.SetFireType(GetFireType());
        playerSettings?.DoSpawnWeaponRenderEffect(weaponRenders);
        OnAmmoLauncher.ForEach(x => { x?.SetActive(true); });
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onAmmoPickUp -= this.OnPickUpAmmo;
        GameEvents.onRoundEnd -= this.OnRoundEnd;
        alreadyKnife = false;
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        StopAllCoroutines();
        if (isReloading) { inReloadMode = true; isReloading = false; }
        isAiming = false;
        isFiring = false;
        lastAimState = false;
        ResetDefaultMuzzlePoint();
    }
    // 武器管理器对所有已装备武器调用，即使它们尚未启用
    public void Setup(bool initial = false)
    {
        // 初始弹药设置
        bulletsLeft = BulletsPerMagazine;
        if (AmmoType == AmmunitionType.Clips) _remainingClips = numberOfClips;
        if (WeaponType != FirearmType.Shotgun && WeaponType != FirearmType.Sniper) { reloadPer = ReloadPer.Magazine; }
        if (!initial)
        {
            if (AmmoType == AmmunitionType.Bullets)
            {
                _remainingClips = BulletsPerMagazine * numberOfClips;
            }
        }

        DefaultPos = transform.localPosition;
        defaultRotation = transform.localRotation;
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        WeaponCamera = PlayerReferences.weaponCamera;
        WeaponCamera.fieldOfView = GameCore.Settings != null ? (float)GameCore.Settings.GetSettingOf("武器FOV") : 55;
        CanAiming = true;
        // @TODO: 换成更完善的射击类型系统
        // 初始射击类型设置的临时修正
        if (WeaponType == FirearmType.Machinegun && !CanAuto)
        {
            if (CanSemi) currentGunType = FirearmType.Burst;
            else if (CanSingle) currentGunType = FirearmType.Pistol;
        }
        else if (WeaponType == FirearmType.Pistol && !CanSingle)
        {
            if (CanSemi) currentGunType = FirearmType.Burst;
            else if (CanAuto) currentGunType = FirearmType.Machinegun;
        }
        else if (WeaponType == FirearmType.Burst && !CanSemi)
        {
            if (CanAuto) currentGunType = FirearmType.Machinegun;
            else if (CanSingle) currentGunType = FirearmType.Pistol;
        }
    }
    // 每帧检查玩家的状态
    public override void OnUpdate()
    {
        if (!_enable)
            return;

        InputUpdate();
        Aim();
        DetermineUpperState();
        if (isFiring)
        {
            spread += (PlayerReferences.firstPersonController.State == PlayerState.Crouching) ? spreadPerSecond * 0.5f : spreadPerSecond;
        }
        else
        {
            spread -= decreaseSpreadPerSec;
        }
        spread = Mathf.Clamp(spread, BaseSpread + extraSpread, spreadMinMax.y + extraSpread);
    }
    // 全部输入事件
    void InputUpdate()
    {
        if (GameConfig.Instance.isChatOpen || !gunManager.isGameStarted || !LobbyMenu.Instance.isCursorLocked) return;

        bool fireDown = GameInput.Fire(GameInputType.Down);
        if (CommonUtil.isMobile)
        {
        }
        else
        {

            if (CanFire)
            {
                // 单击
                if (fireDown) SingleFire();
                // 长按
                if (GameInput.Fire()) LoopFire();
            }
            else
            {
                if (fireDown && bulletsLeft <= 0 && !isReloading)
                {
                    PlayEmptyFireSound();
                }
            }
        }
        if (fireDown && isReloading)
        {
            if (WeaponType == FirearmType.Sniper || WeaponType == FirearmType.Shotgun)
            {
                if (bulletsLeft > 0)
                {
                    CancelReloading();
                }
            }
        }
        if (CommonUtil.isMobile)
        {

        }
        else
        {
            isAiming = GameInput.Aim() && CanAiming;
        }
        if (LobbyMenu.Instance.isCursorLocked)
        {
            ReticleBase.Instance.OnAim(isAiming);
        }
        if (GameInput.Reload() && CanReload)
        {
            Reload();
        }
        if (WeaponType == FirearmType.Machinegun || WeaponType == FirearmType.Burst || WeaponType == FirearmType.Pistol)
        {
            ChangeTypeFire();
        }
        //扳机一直按下时用来降低武器精度
        if (WeaponType != FirearmType.Grenade && WeaponType != FirearmType.Knife)
        {
            if (CommonUtil.isMobile)
            {
            }
            else
            {
                if (WeaponType == FirearmType.Machinegun)
                {
                    isFiring = (GameInput.Fire() && CanFire);
                }
                else
                {
                    if (fireDown && CanFire)
                    {
                        isFiring = true;
                        CancelInvoke(nameof(CancelFiring));
                        Invoke(nameof(CancelFiring), 0.12f);
                    }
                }
            }
        }
    }
    // 切换枪械射击类型
    void ChangeTypeFire()
    {
        bool inp = GameInput.SwitchFireMode();
        if (inp)
        {
            switch (WeaponType)
            {
                case FirearmType.Machinegun:
                    if (CanSemi) WeaponType = FirearmType.Burst;
                    else if (CanSingle) WeaponType = FirearmType.Pistol;
                    break;
                case FirearmType.Burst:
                    if (CanSingle) WeaponType = FirearmType.Pistol;
                    else if (CanAuto) WeaponType = FirearmType.Machinegun;
                    break;
                case FirearmType.Pistol:
                    if (CanAuto) WeaponType = FirearmType.Machinegun;
                    else if (CanSemi) WeaponType = FirearmType.Burst;
                    break;
            }
            HeldWeaponUIBase.Instance.SetFireType(GetFireType());
            gunManager.PlaySound(0);
        }
    }
    // 由移动端按钮事件调用
    void OnFire()
    {
        if (!gunManager.isGameStarted) return;
        if (bulletsLeft <= 0 && !isReloading)
        {
            PlayEmptyFireSound();
        }
        if (isReloading)
        {
            if (WeaponType == FirearmType.Sniper || WeaponType == FirearmType.Shotgun)
            {
                if (bulletsLeft > 0)
                {
                    CancelReloading();
                }
            }
        }
        if (!CanFire)
            return;
        SingleFire();
    }
    void HandleAutoFire()
    {
        if (!CanFire) return;
    }
    // 单次开火
    void SingleFire()
    {
        if (PlayerReferences.firstPersonController.State == PlayerState.Running && Time.time - lastShotTime > (Info.FireRate * 2) || pendingFirstShot)
        {
            if (pendingFirstShot) return;
            isFiring = true;
            pendingFirstShot = true;
            this.InvokeAfter(delayFireOnSprinting, () =>
            {
                pendingFirstShot = false;
                GunTypeFire(false);
            });
        }
        else
        {
            GunTypeFire(false);
        }
        lastShotTime = Time.time;
    }
    // 持续开火
    void LoopFire()
    {
        if (WeaponType != FirearmType.Machinegun) return;
        if (PlayerReferences.firstPersonController.State == PlayerState.Running && Time.time - lastShotTime > (Info.FireRate * 5) || pendingFirstShot)
        {
            if (pendingFirstShot) return;
            isFiring = true;
            pendingFirstShot = true;
            this.InvokeAfter(delayFireOnSprinting, () =>
            {
                pendingFirstShot = false;
                if (!GameInput.Fire(GameInputType.Down)) return;
                GunTypeFire(true);
            });
        }
        else
        {
            GunTypeFire(true);
        }
        lastShotTime = Time.time;
    }
    // 按武器类型调用对应的开火函数
    // 是否自动开火？
    void GunTypeFire(bool auto)
    {
        if (customWeapon != null) { customWeapon.OnFireDown(); }
        else
        {
            switch (WeaponType)
            {
                case FirearmType.Machinegun:
                    if (auto) MachineGunFire();
                    break;
                case FirearmType.Shotgun:
                    ShotgunFire();
                    break;
                case FirearmType.Burst:
                    if (!isBursting) { StartCoroutine(BurstFire()); }
                    break;
                case FirearmType.Grenade:
                    if (!grenadeFired) { GrenadeFire(); }
                    break;
                case FirearmType.Pistol:
                    MachineGunFire();
                    break;
                case FirearmType.Sniper:
                    SniperFire();
                    break;
                case FirearmType.Knife:
                    if (!alreadyKnife) { KnifeFire(); }
                    break;
            }
        }
    }
    // 机枪开火
    void MachineGunFire()
    {
        // 若上一帧到这一帧之间有多于一颗子弹
        float time = Time.time;
        if (nextFireTime > time) return;
        nextFireTime = time + (Info.FireRate * 0.1f);
        FireOneShot();
        //播放开火动画
        if (isAiming) WeaponAnimation?.PlayFire(WeaponRigBase.AnimationFlags.Aiming);
        else WeaponAnimation?.PlayFire();
        OnFireCommons();
        PlayFX();
        //是否自动换弹
        if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
        {
            Reload();
        }
    }
    // 狙击枪开火
    void SniperFire()
    {
        float time = Time.time;
        if (time - Info.FireRate > nextFireTime)
            nextFireTime = time - Time.deltaTime;
        // 持续开火直到用完开火时长
        while (nextFireTime < time)
        {
            FireOneShot();
            WeaponAnimation?.PlayFire();
            StartCoroutine(DelayFireSound());
            OnFireCommons();
            gunManager.HeadAnimator.Play("Sniper", 0, 0);
            PlayFX();
            //是否自动换弹
            if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
            {
                Reload(delayForSecondFireSound + 0.2f);
            }
        }
    }
    private float KnifeFire(bool quickFire = false)
    {
        // 若上一帧到这一帧之间有多于一次射击
        // 重置 nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;
        float time = 0;
        // 持续开火直到用完开火时长
        while (nextFireTime < Time.time)
        {
            isFiring = true;
            alreadyKnife = true;
            StartCoroutine(KnifeSendFire());
            Vector3 position = PlayerCamera.transform.position;
            Vector3 direction = PlayerCamera.transform.TransformDirection(Vector3.forward);
            RaycastHit hit;
            float range = PlayerReferences.cameraRay == null ? Info.Range : Info.Range + PlayerReferences.cameraRay.ExtraRayDistance;
            if (Physics.SphereCast(position, 0.2f, direction, out hit, range))
            {
                if (hit.transform.CompareTag(GameCore.HITBOX_TAG))
                {
                    var bp = hit.transform.GetComponent<IGameDamageable>();
                    var damageData = new DamageInfo();
                    damageData.Damage = Info.Damage;
                    damageData.Actor = LocalPlayer;
                    damageData.GameActor = GameController.Instance.LocalActor;
                    damageData.Cause = DamageCause.Player;
                    damageData.Direction = transform.position;
                    damageData.WeaponID = WeaponID;
                    damageData.ActorViewID = GameController.LocalPlayerViewID;
                    damageData.From = LocalPlayer.NickName;
                    bp?.ReceiveDamage(damageData);
                    ObjectRecyclerBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                }
                else
                {
                    var damageable = hit.transform.GetComponent<IGameDamageable>();
                    if (damageable != null)
                    {
                        DamageInfo damageData = new DamageInfo()
                        {
                            Damage = (int)Info.Damage,
                            Direction = _transform.position,
                            GameActor = GameController.Instance.LocalActor,
                            ActorViewID = GameController.LocalPlayerViewID,
                            WeaponID = WeaponID,
                            From = LocalPlayer.NickName,
                        };
                        damageData.Cause = DamageCause.Player;
                        damageable.ReceiveDamage(damageData);
                    }
                }
            }
            if (WeaponAnimation != null)
            {
                if (quickFire) time = WeaponAnimation.PlayFire(WeaponRigBase.AnimationFlags.QuickFire);
                else time = WeaponAnimation.PlayFire();
            }
            PlayerNetwork.ReplicateFire(FirearmType.Knife, Vector3.zero, Vector3.zero);
            OnFireCommons();
            ReticleBase.Instance.OnFire();
            isFiring = false;
            GameEvents.DispatchLocalPlayerFire(WeaponID);
        }
        return time;
    }
    // 点射
    IEnumerator BurstFire()
    {
        int shotCounter = 0;
        // 若上一帧到这一帧之间有多于一颗子弹
        // 重置 nextFireTime
        if (Time.time - lagBetweenBurst > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;
        int shots = Mathf.Min(roundsPerBurst, bulletsLeft);
        // 持续开火直到用完开火时长
        while (nextFireTime < Time.time)
        {
            while (shotCounter < shots)
            {
                isBursting = true;
                FireOneShot();
                shotCounter++;
                OnFireCommons();
                PlayFX();
                //播放开火动画
                if (isAiming) WeaponAnimation?.PlayFire(WeaponRigBase.AnimationFlags.Aiming);
                else WeaponAnimation?.PlayFire();
                yield return new WaitForSeconds(Info.FireRate);
                if (bulletsLeft <= 0) { break; }
            }
            nextFireTime += lagBetweenBurst;
            //是否自动换弹
            if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
            {
                Reload();
            }
        }
        isBursting = false;
    }
    // 霰弹枪开火
    void ShotgunFire()
    {
        // 若上一帧到这一帧之间有多于一颗子弹
        // 重置 nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;
        int pelletCounter = 0;
        // 持续开火直到用完开火时长
        while (nextFireTime < Time.time)
        {
            do
            {
                FireOneShot();
                pelletCounter++;
            } while (pelletCounter < pelletsPerShot);
            if (!SoundReloadByAnim)
                StartCoroutine(DelayFireSound());
            else
                PlayFireAudio();
            WeaponAnimation?.PlayFire();
            OnFireCommons();
            PlayFX();
            //是否自动换弹
            if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
            {
                Reload(delayForSecondFireSound + 0.3f);
            }
        }
    }
    void GrenadeFire(bool fastFire = false)
    {
        if (grenadeFired || (Time.time - nextFireTime) <= Info.FireRate)
            return;
        if (!fastFire && bulletsLeft == 0 && _remainingClips > 0)
        {
            Reload(1);
            return;
        }
        isFiring = true;
        grenadeFired = true;
        if (ThrowByAnimation)
        {
            nextFireTime = Time.time + Info.FireRate;
            if (fastFire) WeaponAnimation?.PlayFire(WeaponRigBase.AnimationFlags.QuickFire);
            else WeaponAnimation?.PlayFire();
        }
        else { StartCoroutine(ThrowGrenade(fastFire)); }
    }
    // 发射器开火
    public IEnumerator ThrowGrenade(bool fastFire = false, bool useDelay = true)
    {
        float t = 0;
        // 与原版 Game 旧版本的默认 Inspector 数值相乘，保持兼容
        float throwForce = BulletSpeed * 22;
        float upwardForce = bulletDropFactor * 33;
        if (useDelay)
        {
            nextFireTime = Time.time + Info.FireRate;
            if (fastFire) t = WeaponAnimation.PlayFire(WeaponRigBase.AnimationFlags.QuickFire);
            else t = WeaponAnimation.PlayFire();
            float d = (fastFire) ? DelayFire + WeaponAnimation.GetAnimationDuration(WeaponRigBase.WeaponRigType.TakeIn) : DelayFire;
            yield return new WaitForSeconds(d);
        }
        Vector3 direction = PlayerCamera.transform.forward * throwForce + _transform.parent.up * upwardForce;
        Vector3 origin = muzzlePoint.position;
        FireOneProjectile(direction, origin);
        bulletsLeft--;
        UpdateUI();
        Kick();
        PlayerNetwork.IsFireGrenade(spread, origin, transform.parent.rotation, direction);
        PlayFireAudio();
        isFiring = false;
        //没有弹药时隐藏手雷模型
        if (bulletsLeft <= 0)
        {
            OnAmmoLauncher.ForEach(x =>
           {
               if (x != null)
                   x?.SetActive(false);
           });
        }
        //是否自动换弹
        if (!fastFire && AutoReload)
        {
            if (useDelay)
            {
                t = t - DelayFire;
                yield return new WaitForSeconds(t);
            }
            if (bulletsLeft <= 0 && _remainingClips > 0)
            {
                Reload(0);
            }
            else if (bulletsLeft <= 0)
            {
                gunManager?.OnReturnWeapon();
            }
        }
    }
    public IEnumerator FastGrenadeFire(System.Action callBack)
    {
        float tt = WeaponAnimation.GetAnimationDuration(WeaponRigBase.WeaponRigType.AimFire)
            + WeaponAnimation.GetAnimationDuration(WeaponRigBase.WeaponRigType.TakeIn);
        GrenadeFire(true);
        yield return new WaitForSeconds(tt);
        if (_remainingClips > 0)
        {
            bulletsLeft++;
            if (!HaveInfinityAmmo) _remainingClips--;
            UpdateUI();
        }
        callBack();
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    public void QuickMelee(System.Action callBack)
    {
        StartCoroutine(QuickMeleeSequence(callBack));
    }
    IEnumerator QuickMeleeSequence(System.Action callBack)
    {
        float tt = KnifeFire(true);
        yield return new WaitForSeconds(tt);
        callBack();
        gameObject.SetActive(false);
    }
    // 创建并发射子弹
    void FireOneShot()
    {
        // 把枪械信息写入数组传给子弹
        BuildBulletData();
        var instanceData = GetBulletPosition();
        //子弹信息在 start 函数中设置
        instancedBullet = ObjectRecyclerBase.Instance.Instantiate(BulletName, instanceData.Position, instanceData.Rotation);
        instancedBullet.GetComponent<ProjectileBase>().InitProjectile(BulletSettings);
        ReticleBase.Instance.OnFire();
        PlayerNetwork.ReplicateFire(OriginalWeaponType, instanceData.ProjectedHitPoint, BulletSettings.Inaccuracity);
        GameEvents.DispatchLocalPlayerFire(WeaponID);
        if ((bulletsLeft == 0))
        {
            Reload();
        }
    }
    // 创建并发射 1 个发射器投掷物
    void FireOneProjectile(Vector3 direction, Vector3 origin)
    {
        BuildBulletData();
        //生成手雷
        GameObject newNoobTube = Instantiate(grenade, origin, transform.parent.rotation) as GameObject;
        var proRigid = newNoobTube.GetComponent<Rigidbody>();
        if (proRigid != null)
        {
            proRigid.AddForce(direction, ForceMode.Impulse);
        }
        newNoobTube.GetComponent<ProjectileBase>().InitProjectile(BulletSettings);
        grenadeFired = false;
        GameEvents.DispatchLocalPlayerFire(WeaponID);
    }
    public void OnFireCommons()
    {
        PlayFireAudio();
        if (WeaponType != FirearmType.Knife) bulletsLeft--;
        UpdateUI();
        nextFireTime += Info.FireRate;
        EjectShell();
        Kick();
        Shake();
    }
    // 播放枪口火光特效。
    public void PlayFX()
    {
        if (!isAiming)
        {
            if (weaponFX == null)
            {
                if (muzzleFlash) { muzzleFlash.Play(); }
            }
            else weaponFX.PlayFireFX();
        }
    }
    // 获取子弹的出生位置和旋转
    public ShotInstanceData GetBulletPosition()
    {
        bulletInstanceData.Rotation = _transform.parent.rotation;
        Vector3 cp = PlayerCamera.transform.position;
        Vector3 mp = muzzlePoint.position;
        bulletInstanceData.ProjectedHitPoint = cp + (PlayerCamera.transform.forward * 100);
        // 若玩家相机与武器开火点之间存在碰撞体
        // 说明开火点穿进了碰撞体
        if (Physics.Linecast(cp, mp, GameConfig.TagsAndLayerSettings.LocalPlayerHitableLayers))
        {
            // 因为不能从开火点射击（子弹会穿过碰撞体）
            // 子弹改从玩家相机中心生成
            bulletInstanceData.Position = cp;
        }
        else
        {
            // 可以从开火点射击
            bulletInstanceData.Position = mp;
        }
        // 还有一种情况是子弹打不到目标位置
        // 发生在子弹轨迹被靠近生成点的碰撞体挡住时。
        // 此时玩家可能并没有瞄准该碰撞体，但子弹会打到它而不是玩家瞄准的位置
        // 为此做如下处理：
        // 检测开火点与玩家瞄准方向（长 5 米）之间的碰撞体
        if (Physics.Raycast(bulletInstanceData.Position, PlayerCamera.transform.forward, out hit, 5, GameConfig.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
        {
            // 若确实有碰撞体挡住轨迹
            // 就从相机生成子弹，让它绕过障碍并打中目标位置。
            bulletInstanceData.ProjectedHitPoint = hit.point;
            bulletInstanceData.Position = cp;
        }
        else
        {
            if (isAiming)
            {
                if (WeaponType != FirearmType.Sniper)
                {
                    // 这样瞄准时弹道更真实，但近处目标会有轻微偏差
                    bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                    if (Physics.Raycast(cp, PlayerCamera.transform.forward, out hit, 500, GameConfig.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
                    {
                        bulletInstanceData.ProjectedHitPoint = hit.point;
                        bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                    }
                    // 另一种做法是子弹精确沿玩家瞄准方向飞行，但子弹从屏幕中心生成
                    // 若子弹用了自定义特效就会看出来（比如从眼睛射出子弹）
                    // 若这正是你需要的且没有用自定义弹道/特效，就注释上面的行，取消注释下面这行：
                }
                else
                {
                    bulletInstanceData.Position = cp;
                }
            }
            else
            {
            }
        }
        return bulletInstanceData;
    }
    // 收集下一颗子弹要附带的信息，包含来源武器和射击玩家的全部所需数据
    public void BuildBulletData()
    {
        BulletSettings.Damage = Damage;
        BulletSettings.ImpactForce = impactForce;
        if (WeaponType == FirearmType.Sniper && !isAiming)
        {
            BulletSettings.SetInaccuracity(spread * 3, spreadMinMax.y * 3);
        }
        else
        {
            BulletSettings.SetInaccuracity(spread, spreadMinMax.y);
        }
        BulletSettings.Speed = BulletSpeed;
        BulletSettings.WeaponName = Info.Name;
        BulletSettings.Position = PlayerReferences.transform.localPosition;
        BulletSettings.WeaponID = WeaponID;
        BulletSettings.isNetwork = false;
        BulletSettings.DropFactor = bulletDropFactor;
        BulletSettings.Range = Range;
        BulletSettings.ActorViewID = GameController.LocalPlayerViewID;
        BulletSettings.GameActor = GameController.Instance.LocalActor;
        BulletSettings.IsLocalPlayer = true;
    }
    // 瞄准控制
    void Aim()
    {
        if (isAiming && !isReloading)
        {
            CurrentPos = AimPosition;
            currentRotation = Quaternion.Euler(aimRotation);
            currentZoom = Zoom;
            PlayerReferences.weaponSway.UseAimSettings();
            spreadMinMax = defaultSpreadRange * spreadAimMultiplier;
        }
        else
        {
            CurrentPos = DefaultPos;
            currentRotation = defaultRotation;
            currentZoom = PlayerReferences.DefaultCameraFOV;
            PlayerReferences.weaponSway.ResetSettings();
            spreadMinMax = defaultSpreadRange;
        }
        float delta = Time.deltaTime;
        //应用位置
        _transform.localPosition = useSmooth ? Vector3.Lerp(_transform.localPosition, CurrentPos, delta * AimSpeed) :
        Vector3.MoveTowards(_transform.localPosition, CurrentPos, delta * AimSpeed);
        _transform.localRotation = Quaternion.Slerp(_transform.localRotation, currentRotation, delta * AimSpeed);
        if (PlayerCamera != null && !BlockAimFoV)
        {
            PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, currentZoom + controller.GetSprintFov(), delta * AimSpeed);
        }
        if (lastAimState != isAiming)
        {
            GameEvents.DispatchLocalAimEvent(isAiming);
            lastAimState = isAiming;
        }
    }
    // 开火时把后坐力反馈给鼠标视角
    public void Kick() => PlayerReferences.recoil.SetRecoil(new KickbackBase.KickbackData() { Amount = WeaponRecoil, Speed = RecoilSpeed });
    void Shake()
    {
        float influence = isAiming ? 0.5f : 1;
        GameEvents.DoPlayerCameraShake(shakerPresent, "fpweapon", influence);
    }
    void EjectShell()
    {
        if (shell != null)
            shell?.Play();
    }
    public void CheckBullets(float delay = 0)
    {
        if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
        {
            Reload(delay);
        }
    }
    void OnReload()
    {
        if (!CanReload) return;
        Reload();
    }
    public void Reload(float delay = 0.2f)
    {
        if (isReloading || !gameObject.activeInHierarchy)
            return;
        StopCoroutine(nameof(DoReload));
        StartCoroutine(nameof(DoReload), delay);
    }
    // 开始换弹，扣掉一个新弹匣的弹药；同时禁用开火、瞄准和奔跑
    IEnumerator DoReload(float waitTime = 0.2f)
    {
        isAiming = CanFire = false;
        if (isReloading)
            yield break;
        // 开始换弹前的延迟
        yield return new WaitForSeconds(waitTime);
        if (_remainingClips > 0 || inReloadMode)
        {
            isReloading = true;
            if (WeaponAnimation != null)
            {
                if (reloadPer == ReloadPer.Bullet)
                {
                    int t_repeat = BulletsPerMagazine - bulletsLeft;
                    int add = (_remainingClips >= t_repeat) ? t_repeat : (int)_remainingClips;
                    WeaponAnimation?.PlayReload(ReloadTime, new int[1] { add }, WeaponRigBase.AnimationFlags.SplitReload);
                    // 这种换弹方式由多段动画组成，换弹时间
                    // 由动画决定，最后一段动画播完后换弹才结束。
                    yield break;
                }
                else
                {
                    WeaponAnimation?.PlayReload(ReloadTime, null);
                }
            }
            if (!SoundReloadByAnim)
            {
                StartCoroutine(ReloadSoundIE());
            }
            if (!inReloadMode)
            {
                if (AmmoType == AmmunitionType.Clips)
                {
                    if (!HaveInfinityAmmo) _remainingClips--;
                }
            }
            if (WeaponType == FirearmType.Grenade) { OnAmmoLauncher.ForEach(x => { x?.SetActive(true); }); }
            yield return new WaitForSeconds(ReloadTime);
            FillMagazine();
        }
        UpdateUI();
        isReloading = inReloadMode = false;
        CanAiming = CanFire = true;
    }
    public void FillMagazine()
    {
        if (AmmoType == AmmunitionType.Clips)
        {
            bulletsLeft = BulletsPerMagazine;
        }
        else
        {
            int need = BulletsPerMagazine - bulletsLeft;
            int add = (_remainingClips >= need) ? need : (int)_remainingClips;
            bulletsLeft += add;
            if (!HaveInfinityAmmo) _remainingClips -= add;
        }
    }
    public void AddBullet(int bullet)
    {
        if (AmmoType == AmmunitionType.Bullets)
        {
            _remainingClips -= bullet;
        }
        bulletsLeft += bullet;
        UpdateUI();
    }
    // 把该武器设为无限弹药
    public void SetInifinityAmmo(bool infinity)
    {
        HaveInfinityAmmo = infinity;
        bulletsLeft = BulletsPerMagazine;
        _remainingClips = AmmoType == AmmunitionType.Bullets ? BulletsPerMagazine * _remainingClips : (int)_remainingClips;
        if (gameObject.activeInHierarchy)
            UpdateUI();
    }
    // 同步上半身动画的武器状态
    void DetermineUpperState()
    {
        if (PlayerNetwork == null)
            return;
        if (isFiring && !isReloading)
        {
            FPState = (isAiming) ? PlayerFPState.FireAiming : PlayerFPState.Firing;
        }
        else if (isAiming && !isFiring && !isReloading)
        {
            FPState = PlayerFPState.Aiming;
        }
        else if (isReloading)
        {
            FPState = PlayerFPState.Reloading;
        }
        else if (controller.State == PlayerState.Running && !isReloading && !isFiring && !isAiming)
        {
            FPState = PlayerFPState.Running;
        }
        else
        {
            FPState = PlayerFPState.Idle;
        }
        PlayerNetwork.FPState = FPState;
    }
    // 把武器吸附到瞄准位置。
    public void SetToAim()
    {
        _transform.localPosition = AimPosition;
        _transform.localRotation = Quaternion.Euler(aimRotation);
    }
    public void PlayFireAudio()
    {
        FireSource.clip = FireSound;
        FireSource.spread = Random.Range(1.0f, 1.5f);
        FireSource.pitch = Random.Range(1.0f, 1.075f);
        FireSource.Play();
    }
    // 多数霰弹枪是先开枪声后换弹声
    IEnumerator DelayFireSound()
    {
        PlayFireAudio();
        yield return new WaitForSeconds(delayForSecondFireSound);
        if (DelaySource != null)
        {
            DelaySource.clip = ReloadSound3;
            DelaySource.Play();
        }
        else
        {
            PlayClip(ReloadSound3);
        }
    }
    public void FinishReload()
    {
        if (AmmoType == AmmunitionType.Clips)
        {
            if (!HaveInfinityAmmo) _remainingClips--;
        }
        isReloading = false;
        CanAiming = true;
        CanFire = true;
        inReloadMode = false;
    }
    private void PlayClip(AudioClip clip, float pitch = 1)
    {
        Source.clip = clip;
        Source.pitch = pitch;
        Source.Play();
    }
    public void PlayReloadAudio(int part)
    {
        if (SoundReloadByAnim) return;
        if (part == 0) PlayClip(ReloadSound);
        else if (part == 1) PlayClip(ReloadSound2);
        else if (part == 2) PlayClip(ReloadSound3);
    }
    public void PlayEmptyFireSound()
    {
        if (WeaponType != FirearmType.Knife && DryFireSound != null)
        {
            PlayClip(DryFireSound);
        }
    }
    // 用这个方法配置多段换弹音效。只有 1 段音效时，只在 Inspector 里填一个，其他留空
    IEnumerator ReloadSoundIE()
    {
        float t_time = ReloadTime / 3;
        if (ReloadSound != null)
        {
            PlayClip(ReloadSound);
            gunManager?.HeadAnimation(1, t_time);
        }
        if (ReloadSound2 != null)
        {
            if (WeaponType == FirearmType.Shotgun)
            {
                int t_repeat = BulletsPerMagazine - bulletsLeft;
                for (int i = 0; i < t_repeat; i++)
                {
                    yield return new WaitForSeconds(t_time / t_repeat + 0.025f);
                    PlayClip(ReloadSound2);
                }
            }
            else
            {
                yield return new WaitForSeconds(t_time);
                PlayClip(ReloadSound2);
            }
        }
        if (ReloadSound3 != null)
        {
            yield return new WaitForSeconds(t_time);
            PlayClip(ReloadSound3);
            gunManager?.HeadAnimation(2, t_time);
        }
        yield return new WaitForSeconds(0.65f);
        gunManager?.HeadAnimation(0, t_time);
    }
    IEnumerator KnifeSendFire()
    {
        yield return new WaitForSeconds(0.5f);
        isFiring = false;
        alreadyKnife = false;
    }
    // 禁用枪械时调用动画并关闭基础功能
    public float DisableWeapon(bool isFastKill = false)
    {
        CanAiming = false;
        if (isReloading) { inReloadMode = true; isReloading = false; }
        CanFire = false;
        gunManager?.HeadAnimation(0, 1);
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        if (!isFastKill) { StopAllCoroutines(); }
        if (WeaponAnimation != null)
            return WeaponAnimation.PlayTakeOut();
        else return 0;
    }
    void DrawComplete()
    {
        CanFire = !GameController.Instance.GameFinish;
        CanAiming = true;
        if (inReloadMode)
        {
            if (WeaponType == FirearmType.Grenade)
            {
                FillMagazine();
                inReloadMode = false;
                UpdateUI();
            }
            else
            {
                Reload(0.2f);
            }
        }
    }
    // 回合结束时不能开火
    void OnRoundEnd() => _enable = false;
    public bool OnPickUpAmmo(int bullets, int projectiles, int weaponID)
    {
        //若这不是全局弹药，而是某把武器专用
        if (weaponID != -1)
        {
            //而且这又不是对应的那把武器
            if (weaponID != WeaponID) return false;
        }
        if (WeaponType == FirearmType.Knife) return false;
        if (AmmoType == AmmunitionType.Clips)
        {
            //弹匣数量上限
            if (_remainingClips >= maxNumberOfClips) return false;
            _remainingClips += Mathf.FloorToInt(bullets / BulletsPerMagazine);
            if (_remainingClips > maxNumberOfClips)
            {
                _remainingClips = maxNumberOfClips;
            }
        }
        else
        {
            if (WeaponType == FirearmType.Grenade)
            {
                _remainingClips += projectiles;
                new GameLocalNotification(string.Format("+{0} {1}", projectiles.ToString(), Info.Name));
            }
            else
            {
                if (_remainingClips >= BulletsPerMagazine * maxNumberOfClips) return false;
                int oldCount = _remainingClips;
                _remainingClips += bullets;
                _remainingClips = Mathf.Clamp(_remainingClips, 0, BulletsPerMagazine * maxNumberOfClips);
                new GameLocalNotification(string.Format("+{0} {1} Bullets", _remainingClips - oldCount, Info.Name));
            }
        }
        UpdateUI();
        return true;
    }
    public void OverrideMuzzlePoint(Transform newPoint)
    {
        defaultMuzzlePoint = muzzlePoint;
        muzzlePoint = newPoint;
    }
    // 启用/禁用武器渲染器和模型
    public void SetWeaponRendersActive(bool active)
    {
        if (weaponRenders == null)
        {
            weaponRenders = _transform.GetComponentsInChildren<Renderer>();
        }
        foreach (var item in weaponRenders)
        {
            if (item == null) continue;
            item.gameObject.SetActive(active);
        }
        onWeaponRendersActive?.Invoke(active);
    }
    public float GetReloadTime()
    {
        if (reloadPer == ReloadPer.Bullet && WeaponAnimation != null)
        {
            int missingBullets = BulletsPerMagazine - bulletsLeft;
            int add = (_remainingClips >= missingBullets) ? missingBullets : (int)_remainingClips;
            float[] bullets = { add };
            return WeaponAnimation.GetAnimationDuration(WeaponRigBase.WeaponRigType.Reload, bullets);
        }
        return ReloadTime;
    }
    void CancelFiring() { isFiring = false; }
    void CancelReloading() { WeaponAnimation?.CancelAnimation(WeaponRigBase.WeaponRigType.Reload); }
    public void ResetDefaultMuzzlePoint() { if (defaultMuzzlePoint != null) muzzlePoint = defaultMuzzlePoint; }
    public void UpdateUI() => HeldWeaponUIBase.Instance?.SetAmmoOf(this);
    public void SetDefaultWeaponCameraFOV(float fov) => WeaponCamera.fieldOfView = fov;
    public Vector3 GetDefaultPosition() => DefaultPos;
    public override FireType GetFireType()
    {
        switch (WeaponType)
        {
            case FirearmType.Machinegun: return FireType.Auto;
            case FirearmType.Burst: return FireType.Semi;
            case FirearmType.Knife: return FireType.Undefined;
            default: return FireType.Single;
        }
    }
    // 当前的 <see cref="FirearmType"/>，武器射击类型在运行时变化时会跟着变。
    public FirearmType WeaponType
    {
        get
        {
            if (currentGunType == FirearmType.None) currentGunType = Info.Type;
            return currentGunType;
        }
        set
        {
            currentGunType = value;
        }
    }
    // GameData 中定义的默认 <see cref="FirearmType"/>
    public FirearmType OriginalWeaponType
    {
        get => Info.Type;
    }
    private float BaseSpread
    {
        get { return PlayerReferences.firstPersonController.State == PlayerState.Crouching ? spreadMinMax.x * 0.5f : spreadMinMax.x; }
    }
    public int GetCompactClips { get { return (_remainingClips / BulletsPerMagazine); } }
    // 判断玩家能否射击这把武器。
    public bool CanFire
    {
        get
        {
            return (bulletsLeft > 0 && _canFire && !isReloading && FireWhileRun);
        }
        set => _canFire = value;
    }
    public bool FireRatePassed { get { return (Time.time - nextFireTime) > Info.FireRate; } }
    public bool AllowQuickFire() => m_AllowQuickFire && bulletsLeft > 0 && FireRatePassed;
    // 判断玩家在当前状态下能否用这把武器瞄准
    public bool CanAiming
    {
        get
        {
            if (WeaponType == FirearmType.Grenade || WeaponType == FirearmType.Knife) return false;
            if (controller.GetRunToAimBehave() == PlayerRunToAimBehave.BlockAim && controller.State == PlayerState.Running) return false;
            return (_canAim);
        }
        set => _canAim = value;
    }
    // 判断玩家能否给这把武器换弹
    bool CanReload
    {
        get
        {
            bool can = false;
            if (bulletsLeft < BulletsPerMagazine && _remainingClips > 0 && controller.State != PlayerState.Running && !isReloading)
            {
                can = true;
            }
            if (WeaponType == FirearmType.Knife && nextFireTime < Time.time)
            {
                can = false;
            }
            return can;
        }
    }
    bool FireWhileRun
    {
        get
        {
            if (GameConfig.Instance.fireWhileSprinting)
            {
                return true;
            }
            if (controller.State != PlayerState.Running)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public bool CanBeTaken() { if (canBeTakenWhenIsEmpty) return true; else return (bulletsLeft > 0 || _remainingClips > 0); }
    public bool IsOutOfAmmo() => bulletsLeft <= 0 && _remainingClips <= 0;
    public FirstPersonControllerBase controller => PlayerReferences.firstPersonController;
    public PlayerNetworkHandler PlayerNetwork => PlayerReferences.playerNetwork;
    private FirearmInventory gunManager => PlayerReferences.gunManager;
    private PlayerRig _playerReferences;
    public PlayerRig PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.root.GetComponent<PlayerRig>();
            return _playerReferences;
        }
    }
    private WeaponRigBase _anim;
    public WeaponRigBase WeaponAnimation
    {
        get
        {
            if (_anim == null) { _anim = GetComponentInChildren<WeaponRigBase>(); }
            return _anim;
        }
    }
    [System.Serializable]
    public enum ShotInstanceMethod
    {
        Pooled,
        Instanced,
    }
    [System.Serializable]
    public enum ReloadPer
    {
        Bullet,
        Magazine,
    }
    public struct ShotInstanceData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 ProjectedHitPoint;
    }
#if UNITY_EDITOR
    public bool _aimRecord = false;
    public Vector3 _defaultPosition = new Vector3(-100, 0, 0);
    public Vector3 _defaultRotation = new Vector3(-100, 0, 0);
    private void OnDrawGizmos()
    {
        if (muzzlePoint != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.4f);
            Gizmos.DrawSphere(muzzlePoint.position, 0.022f);
            Gizmos.color = Color.white;
        }
    }
#endif
}
