using GFWK.Core.Motion;
using GFWK.Internal.Structures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_Gun : bl_GunBase
{
    public bl_CustomGunBase customWeapon;
    public float CrossHairScale = 8;
    public BulletInstanceMethod bulletInstanceMethod = BulletInstanceMethod.Pooled;
    public string BulletName = "bullet";
    public GameObject bulletPrefab;
    public GameObject grenade = null;       // the grenade style round... this can also be used for arrows or similar rounds
    public ParticleSystem muzzleFlash = null;     // the muzzle flash for this weapon
    public Transform muzzlePoint = null;    // the muzzle point of this weapon
    public bl_WeaponFXBase weaponFX = null;
    public ParticleSystem shell = null;          // the weapons empty shell particle
    public GameObject impactEffect = null;  // impact effect, used for raycast bullet types 
    public Vector3 AimPosition; //position of gun when is aimed
    public Vector3 aimRotation = Vector3.zero;
    public bool useSmooth = true;
    public float AimSmooth;
    public ShakerPresent shakerPresent;
    [Range(0, 179)]
    public float aimZoom = 50;
    public bool CanAuto = true;
    public bool CanSemi = true;
    public bool CanSingle = true;
    //Shotgun Specific Vars
    public int pelletsPerShot = 10;         // number of pellets per round fired for the shotgun
    public float delayForSecondFireSound = 0.45f;
    //Burst Specific Vars
    public int roundsPerBurst = 3;          // number of rounds per burst fire
    public float lagBetweenBurst = 0.5f;    // time between each shot in a burst
    //Launcher Specific Vars
    public List<GameObject> OnAmmoLauncher = new List<GameObject>();
    public bool ThrowByAnimation = false;
    public int impactForce = 50;            // how much force applied to a rigid body
    public float bulletSpeed = 200.0f;      // how fast are your bullets
    public float bulletDropFactor = 1;
    public bool AutoReload = true;
    public bool m_AllowQuickFire = true;
    public ReloadPer reloadPer = ReloadPer.Bullet;
    public int bulletsPerClip = 50;         // number of bullets in each clip
    public int numberOfClips = 5;
    public int maxNumberOfClips = 10;       // maximum number of clips you can hold
    public float DelayFire = 0.85f;
    public float delayFireOnSprinting = 0.12f; // fire delay in the first shot when the player is sprinting.
    public Vector2 spreadMinMax = new Vector2(1, 3);
    public float spreadAimMultiplier = 0.5f;
    public float spreadPerSecond = 0.2f;    // if trigger held down, increase the spread of bullets
    public float spread = 0.0f;             // current spread of the gun
    public float decreaseSpreadPerSec = 0.5f;// amount of accuracy regained per frame when the gun isn't being fired 
    [HideInInspector] public bool isReloading = false;       // am I in the process of reloading
    // Recoil
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
    //cached player components
    public Renderer[] weaponRenders = null;
    public bl_PlayerSettings playerSettings;

    public int Damage
    {
        get { return Info.Damage + extraDamage; }
        set { extraDamage = value; }//don't modify the base damage, only the extra value
    }

#if !ACTK_IS_HERE
    private int _remainingClips = 5;
    public int RemainingClips
    {
        get => _remainingClips;
        set => _remainingClips = value;
    }
#else
    private ObscuredInt _remainingClips = 5;
    public ObscuredInt RemainingClips
    {
        get => _remainingClips;
        set => _remainingClips = value;
    }
#endif

    public float Zoom
    {
        get { return aimZoom + extraZoom; }
        set { extraZoom = value; }//don't modify the base zoom, only the extra value
    }

    public int BulletsPerMagazine
    {
        get { return bulletsPerClip + extraBullets; }
        set { extraBullets = value; }//don't modify the base bullets, only the extra value
    }

    public float MaxSpread
    {
        get { return extraSpread; }
        set { extraSpread = value; }//don't modify the base bullets, only the extra value
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
    public BulletData BulletSettings = new BulletData();
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
    private static BulletInstanceData bulletInstanceData;
    private AudioClip defaultFireSound;
    private float lastShotTime = 0;
    private bool pendingFirstShot = false;
    private bool _canFire = false;
    private bool _canAim;
    private GunType currentGunType = GunType.None;

    protected override void Awake()
    {
        base.Awake();
        Initialized();
    }

    // This is called from the Weapon Manager at start for all the equipped weapons.
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
        AmmoType = bl_GameData.Instance.AmmoType;
        bl_CrosshairBase.Instance.Block = false;
        Setup();
        customWeapon?.Initialitate(this);
        isInitialized = true;
    }

    protected override void OnEnable()
    {
#if GFWKM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.OnFireClick += OnFire;
            bl_TouchHelper.OnReload += OnReload;
        }
#endif

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
        bl_EventHandler.onAmmoPickUp += this.OnPickUpAmmo;
        bl_EventHandler.onRoundEnd += this.OnRoundEnd;
        bl_CrosshairBase.Instance.SetupCrosshairForWeapon(this);
        bl_EquippedWeaponUIBase.Instance.SetFireType(GetFireType());
        playerSettings?.DoSpawnWeaponRenderEffect(weaponRenders);
        OnAmmoLauncher.ForEach(x => { x?.SetActive(true); });
#if GFWKTPV
        var tpScript = PlayerReferences.GetComponent<bl_PlayerCameraSwitcher>();
        if (tpScript != null)
        {
            SetWeaponRendersActive(!bl_CameraViewSettings.IsThirdPerson());
        }
#endif
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onAmmoPickUp -= this.OnPickUpAmmo;
        bl_EventHandler.onRoundEnd -= this.OnRoundEnd;
#if GFWKM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.OnFireClick -= OnFire;
            bl_TouchHelper.OnReload -= OnReload;
        }
#endif
        alreadyKnife = false;
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        StopAllCoroutines();
        if (isReloading) { inReloadMode = true; isReloading = false; }
        isAiming = false;
        isFiring = false;
        lastAimState = false;
        ResetDefaultMuzzlePoint();
    }

    // Called by the weapon manager on all the equipped weapons even when they have not been enabled
    public void Setup(bool initial = false)
    {
        // Initial ammo setup
        bulletsLeft = BulletsPerMagazine;
        if (AmmoType == AmmunitionType.Clips) _remainingClips = numberOfClips;
        if (WeaponType != GunType.Shotgun && WeaponType != GunType.Sniper) { reloadPer = ReloadPer.Magazine; }
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
        WeaponCamera.fieldOfView = bl_GFWK.Settings != null ? (float)bl_GFWK.Settings.GetSettingOf("武器FOV") : 55;
        CanAiming = true;

        // @TODO: Change this with more elaborated fire type system
        // Temporal fix to the initial fire type setup
        if (WeaponType == GunType.Machinegun && !CanAuto)
        {
            if (CanSemi) currentGunType = GunType.Burst;
            else if (CanSingle) currentGunType = GunType.Pistol;
        }
        else if (WeaponType == GunType.Pistol && !CanSingle)
        {
            if (CanSemi) currentGunType = GunType.Burst;
            else if (CanAuto) currentGunType = GunType.Machinegun;
        }
        else if (WeaponType == GunType.Burst && !CanSemi)
        {
            if (CanAuto) currentGunType = GunType.Machinegun;
            else if (CanSingle) currentGunType = GunType.Pistol;
        }
    }

    // check what the player is doing every frame
    // <returns></returns>
    public override void OnUpdate()
    {
        if (!_enable)
            return;

        InputUpdate();
        Aim();
        DetermineUpperState();

        if (isFiring) // if the gun is firing
        {
            spread += (PlayerReferences.firstPersonController.State == PlayerState.Crouching) ? spreadPerSecond * 0.5f : spreadPerSecond; // gun is less accurate with the trigger held down
        }
        else
        {
            spread -= decreaseSpreadPerSec; // gun regains accuracy when trigger is released
        }
        spread = Mathf.Clamp(spread, BaseSpread + extraSpread, spreadMinMax.y + extraSpread);
    }

    // All Input events
    void InputUpdate()
    {
        if (bl_GameData.Instance.isChating || !gunManager.isGameStarted || !bl_RoomMenu.Instance.isCursorLocked) return;

        bool fireDown = bl_GameInput.Fire(GameInputType.Down);
        if (bl_UtilityHelper.isMobile)
        {
#if GFWKM
            if (bl_GameData.Instance.AutoWeaponFire && bl_AutoWeaponFire.Instance != null)
                HandleAutoFire();
            else
            {
                if (WeaponType == GunType.Machinegun && bl_TouchHelper.Instance != null)
                {
                    if (bl_TouchHelper.Instance.FireDown && CanFire)
                        LoopFire();
                }
            }
#endif
        }
        else
        {
#if GFWKM
            if (bl_GameData.Instance.AutoWeaponFire && bl_AutoWeaponFire.Instance != null)
            {
                HandleAutoFire();
            }
#endif

            if (CanFire)
            {
                // if press once
                if (fireDown) SingleFire();

                // if keep pressing
                if (bl_GameInput.Fire()) LoopFire();
            }
            else
            {
                if (fireDown && bulletsLeft <= 0 && !isReloading)//if try fire and don't have more bullets
                {
                    PlayEmptyFireSound();
                }
            }
        }

        if (fireDown && isReloading)//if try fire while reloading 
        {
            if (WeaponType == GunType.Sniper || WeaponType == GunType.Shotgun)
            {
                if (bulletsLeft > 0)//and has at least one bullet
                {
                    CancelReloading();
                }
            }
        }

        if (bl_UtilityHelper.isMobile)
        {
#if GFWKM
            isAiming = bl_TouchHelper.Instance.isAim && CanAiming;
#endif
        }
        else
        {
            isAiming = bl_GameInput.Aim() && CanAiming;
        }

        if (bl_RoomMenu.Instance.isCursorLocked)
        {
            bl_CrosshairBase.Instance.OnAim(isAiming);
        }

        if (bl_GameInput.Reload() && CanReload)
        {
            Reload();
        }

        if (WeaponType == GunType.Machinegun || WeaponType == GunType.Burst || WeaponType == GunType.Pistol)
        {
            ChangeTypeFire();
        }

        //used to decrease weapon accuracy as long as the trigger remains down
        if (WeaponType != GunType.Grenade && WeaponType != GunType.Knife)
        {
            if (bl_UtilityHelper.isMobile)
            {
#if GFWKM
                if (!bl_GameData.Instance.AutoWeaponFire && bl_TouchHelper.Instance != null)
                {
                    isFiring = (bl_TouchHelper.Instance.FireDown && CanFire);
                }
#endif
            }
            else
            {
                if (WeaponType == GunType.Machinegun)
                {
                    isFiring = (bl_GameInput.Fire() && CanFire); // fire is down, gun is firing
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

    // change the type of gun gust
    void ChangeTypeFire()
    {
        bool inp = bl_GameInput.SwitchFireMode();
        if (inp)
        {
            switch (WeaponType)
            {
                case GunType.Machinegun:
                    if (CanSemi) WeaponType = GunType.Burst;
                    else if (CanSingle) WeaponType = GunType.Pistol;
                    break;
                case GunType.Burst:
                    if (CanSingle) WeaponType = GunType.Pistol;
                    else if (CanAuto) WeaponType = GunType.Machinegun;
                    break;
                case GunType.Pistol:
                    if (CanAuto) WeaponType = GunType.Machinegun;
                    else if (CanSemi) WeaponType = GunType.Burst;
                    break;
            }
            bl_EquippedWeaponUIBase.Instance.SetFireType(GetFireType());
            gunManager.PlaySound(0);
        }
    }

    // Called by mobile button event
    void OnFire()
    {
        if (!gunManager.isGameStarted) return;

        if (bulletsLeft <= 0 && !isReloading)
        {
            PlayEmptyFireSound();
        }

        if (isReloading)
        {
            if (WeaponType == GunType.Sniper || WeaponType == GunType.Shotgun)
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

#if GFWKM
        bool fireDown = bl_AutoWeaponFire.Instance.Fire();
        isFiring = fireDown;
        if (fireDown)
        {
            if (WeaponType == GunType.Machinegun)
                LoopFire();
            else
                SingleFire();
        }
#endif
    }

    // Fire one time
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

    // Fire continuously
    void LoopFire()
    {
        if (WeaponType != GunType.Machinegun) return;

        if (PlayerReferences.firstPersonController.State == PlayerState.Running && Time.time - lastShotTime > (Info.FireRate * 5) || pendingFirstShot)
        {
            if (pendingFirstShot) return;

            isFiring = true;
            pendingFirstShot = true;

            this.InvokeAfter(delayFireOnSprinting, () =>
            {
                pendingFirstShot = false;
                if (!bl_GameInput.Fire(GameInputType.Down)) return;

                GunTypeFire(true);
            });
        }
        else
        {
            GunTypeFire(true);
        }
        lastShotTime = Time.time;
    }

    // Call the fire function depending on the weapon type
    // automatic fire?
    void GunTypeFire(bool auto)
    {
        if (customWeapon != null) { customWeapon.OnFireDown(); }
        else
        {
            switch (WeaponType)
            {
                case GunType.Machinegun:
                    if (auto) MachineGunFire();
                    break;
                case GunType.Shotgun:
                    ShotgunFire();
                    break;
                case GunType.Burst:
                    if (!isBursting) { StartCoroutine(BurstFire()); }
                    break;
                case GunType.Grenade:
                    if (!grenadeFired) { GrenadeFire(); }
                    break;
                case GunType.Pistol:
                    MachineGunFire();
                    break;
                case GunType.Sniper:
                    SniperFire();
                    break;
                case GunType.Knife:
                    if (!alreadyKnife) { KnifeFire(); }
                    break;
            }
        }
    }

    // fire the machine gun
    void MachineGunFire()
    {
        // If there is more than one bullet between the last and this frame
        float time = Time.time;

        if (nextFireTime > time) return;
        nextFireTime = time + (Info.FireRate * 0.1f); // multiply by 0.1 just to add compatibility (keep the same fire rate) with older versions of GFWK.

        FireOneShot();

        //Play fire animation
        if (isAiming) WeaponAnimation?.PlayFire(bl_WeaponAnimationBase.AnimationFlags.Aiming);
        else WeaponAnimation?.PlayFire();

        OnFireCommons();
        PlayFX();

        //is Auto reload
        if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
        {
            Reload();
        }
    }

    // fire the sniper gun
    void SniperFire()
    {
        float time = Time.time;
        if (time - Info.FireRate > nextFireTime)
            nextFireTime = time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < time)
        {
            FireOneShot();
            WeaponAnimation?.PlayFire();
            StartCoroutine(DelayFireSound());
            OnFireCommons();
            gunManager.HeadAnimator.Play("Sniper", 0, 0);
            PlayFX();
            //is Auto reload
            if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
            {
                Reload(delayForSecondFireSound + 0.2f);
            }
        }
    }

    private float KnifeFire(bool quickFire = false)
    {
        // If there is more than one shot  between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        float time = 0;
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            isFiring = true; // fire is down, gun is firing
            alreadyKnife = true;
            StartCoroutine(KnifeSendFire());

            Vector3 position = PlayerCamera.transform.position;
            Vector3 direction = PlayerCamera.transform.TransformDirection(Vector3.forward);

            RaycastHit hit;
            float range = PlayerReferences.cameraRay == null ? Info.Range : Info.Range + PlayerReferences.cameraRay.ExtraRayDistance;
            if (Physics.SphereCast(position, 0.2f, direction, out hit, range))
            {
                if (hit.transform.CompareTag(bl_GFWK.HITBOX_TAG))
                {
                    var bp = hit.transform.GetComponent<IGFWKDamageable>();
                    var damageData = new DamageData();
                    damageData.Damage = Info.Damage;
                    damageData.Actor = LocalPlayer;
                    damageData.GFWKActor = bl_GameManager.Instance.LocalActor;
                    damageData.Cause = DamageCause.Player;
                    damageData.Direction = transform.position;
                    damageData.GunID = GunID;
                    damageData.ActorViewID = bl_GameManager.LocalPlayerViewID;
                    damageData.From = LocalPlayer.NickName;
                    bp?.ReceiveDamage(damageData);

                    bl_ObjectPoolingBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                }
                else
                {
                    var damageable = hit.transform.GetComponent<IGFWKDamageable>();
                    if (damageable != null)
                    {
                        DamageData damageData = new DamageData()
                        {
                            Damage = (int)Info.Damage,
                            Direction = _transform.position,
                            GFWKActor = bl_GameManager.Instance.LocalActor,
                            ActorViewID = bl_GameManager.LocalPlayerViewID,
                            GunID = GunID,
                            From = LocalPlayer.NickName,
                        };
                        damageData.Cause = DamageCause.Player;
                        damageable.ReceiveDamage(damageData);
                    }
                }
            }

            if (WeaponAnimation != null)
            {
                if (quickFire) time = WeaponAnimation.PlayFire(bl_WeaponAnimationBase.AnimationFlags.QuickFire);
                else time = WeaponAnimation.PlayFire();
            }

            PlayerNetwork.ReplicateFire(GunType.Knife, Vector3.zero, Vector3.zero);
            OnFireCommons();
            bl_CrosshairBase.Instance.OnFire();
            isFiring = false;
            bl_EventHandler.DispatchLocalPlayerFire(GunID);
        }
        return time;
    }

    // burst shooting
    // <returns></returns>
    IEnumerator BurstFire()
    {
        int shotCounter = 0;
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - lagBetweenBurst > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        int shots = Mathf.Min(roundsPerBurst, bulletsLeft);
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            while (shotCounter < shots)
            {
                isBursting = true;
                FireOneShot();
                shotCounter++;
                OnFireCommons();
                PlayFX();

                //Play fire animation
                if (isAiming) WeaponAnimation?.PlayFire(bl_WeaponAnimationBase.AnimationFlags.Aiming);
                else WeaponAnimation?.PlayFire();

                yield return new WaitForSeconds(Info.FireRate);
                if (bulletsLeft <= 0) { break; }
            }

            nextFireTime += lagBetweenBurst;
            //is Auto reload
            if (bulletsLeft <= 0 && _remainingClips > 0 && AutoReload)
            {
                Reload();
            }
        }
        isBursting = false;
    }

    // fire the shotgun
    void ShotgunFire()
    {
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        int pelletCounter = 0;  // counter used for pellets per round
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            do
            {
                FireOneShot();
                pelletCounter++; // add another pellet         
            } while (pelletCounter < pelletsPerShot); // if number of pellets fired is less then pellets per round... fire more pellets

            if (!SoundReloadByAnim)
                StartCoroutine(DelayFireSound());
            else
                PlayFireAudio();

            WeaponAnimation?.PlayFire();
            OnFireCommons();
            PlayFX();
            //is Auto reload
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
            Reload(1); // if out of ammo, reload
            return;
        }
        isFiring = true;
        grenadeFired = true;
        if (ThrowByAnimation)
        {
            nextFireTime = Time.time + Info.FireRate;
            if (fastFire) WeaponAnimation?.PlayFire(bl_WeaponAnimationBase.AnimationFlags.QuickFire);
            else WeaponAnimation?.PlayFire();
        }
        else { StartCoroutine(ThrowGrenade(fastFire)); }
    }

    // fire your launcher
    public IEnumerator ThrowGrenade(bool fastFire = false, bool useDelay = true)
    {
        float t = 0;
        // multiple these values to add compatibility with the default inspector values in older GFWK versions
        float throwForce = BulletSpeed * 22;
        float upwardForce = bulletDropFactor * 33;

        if (useDelay)
        {
            nextFireTime = Time.time + Info.FireRate;

            if (fastFire) t = WeaponAnimation.PlayFire(bl_WeaponAnimationBase.AnimationFlags.QuickFire);
            else t = WeaponAnimation.PlayFire();

            float d = (fastFire) ? DelayFire + WeaponAnimation.GetAnimationDuration(bl_WeaponAnimationBase.WeaponAnimationType.TakeIn) : DelayFire;
            yield return new WaitForSeconds(d);
        }

        Vector3 direction = PlayerCamera.transform.forward * throwForce + _transform.parent.up * upwardForce;
        Vector3 origin = muzzlePoint.position;

        FireOneProjectile(direction, origin);

        bulletsLeft--; // subtract a bullet
        UpdateUI();
        Kick();

        PlayerNetwork.IsFireGrenade(spread, origin, transform.parent.rotation, direction);
        PlayFireAudio();
        isFiring = false;

        //hide the grenade mesh when doesn't have ammo
        if (bulletsLeft <= 0)
        {
            OnAmmoLauncher.ForEach(x =>
           {
               if (x != null)
                   x?.SetActive(false);
           });
        }

        //is Auto reload
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
        float tt = WeaponAnimation.GetAnimationDuration(bl_WeaponAnimationBase.WeaponAnimationType.AimFire)
            + WeaponAnimation.GetAnimationDuration(bl_WeaponAnimationBase.WeaponAnimationType.TakeIn);

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

    // Create and fire a bullet
    // <returns></returns>
    void FireOneShot()
    {
        // set the gun's info into an array to send to the bullet
        BuildBulletData();
        var instanceData = GetBulletPosition();
        //bullet info is set up in start function
        instancedBullet = bl_ObjectPoolingBase.Instance.Instantiate(BulletName, instanceData.Position, instanceData.Rotation); // create a bullet
        instancedBullet.GetComponent<bl_ProjectileBase>().InitProjectile(BulletSettings);// send the gun's info to the bullet
        bl_CrosshairBase.Instance.OnFire();
        PlayerNetwork.ReplicateFire(OriginalWeaponType, instanceData.ProjectedHitPoint, BulletSettings.Inaccuracity);
        bl_EventHandler.DispatchLocalPlayerFire(GunID);
        if ((bulletsLeft == 0))
        {
            Reload();
        }
    }

    // Create and Fire 1 launcher projectile
    // <returns></returns>
    void FireOneProjectile(Vector3 direction, Vector3 origin)
    {
        BuildBulletData();

        //Instantiate grenade
        GameObject newNoobTube = Instantiate(grenade, origin, transform.parent.rotation) as GameObject;

        var proRigid = newNoobTube.GetComponent<Rigidbody>();
        if (proRigid != null)//if grenade have a rigidbody,then apply velocity
        {
            proRigid.AddForce(direction, ForceMode.Impulse);
        }

        newNoobTube.GetComponent<bl_ProjectileBase>().InitProjectile(BulletSettings);// send the gun's info to the grenade, bl_Projectile.cs  
        grenadeFired = false;
        bl_EventHandler.DispatchLocalPlayerFire(GunID);
    }

    public void OnFireCommons()
    {
        PlayFireAudio();
        if (WeaponType != GunType.Knife) bulletsLeft--;
        UpdateUI();
        nextFireTime += Info.FireRate;
        EjectShell();
        Kick();
        Shake();
    }

    // Play muzzleflash effect.
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

    // Get the bullet spawn position and rotation
    public BulletInstanceData GetBulletPosition()
    {
        bulletInstanceData.Rotation = _transform.parent.rotation;
        Vector3 cp = PlayerCamera.transform.position;
        Vector3 mp = muzzlePoint.position;
        bulletInstanceData.ProjectedHitPoint = cp + (PlayerCamera.transform.forward * 100);

        // if there's a collider between the player camera and the weapon fire point
        // that means the fire point is going through the collider
        if (Physics.Linecast(cp, mp, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers))
        {
            // since we can't shoot from the fire point (since the bullets will go through the collider)
            // the bullet will be instanced from the center of the player camera
            bulletInstanceData.Position = cp;
        }
        else
        {
            // we can shoot from the fire point
            bulletInstanceData.Position = mp;
        }

        // Now there is a situation where the bullet wont get to hit the desired destination 
        // this happens when the bullet trajectory is hampered by a collider that is near to the bullet instantiation point.
        // in these case the player may not be aiming to that collider but the bullet will hit that collider instead of where the player is aiming to
        // so to fix this we do the following:

        // detect colliders between the fire point and the direction (5 meters long) where the player is aiming to
        if (Physics.Raycast(bulletInstanceData.Position, PlayerCamera.transform.forward, out hit, 5, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
        {
            // if indeed there is a collider hampering the trajectory
            // instance the bullet from the camera to make the bullet avoid the obstacle and hit the desired destination.

            bulletInstanceData.ProjectedHitPoint = hit.point;
            bulletInstanceData.Position = cp;
        }
        else // All clear, not obstacles detected
        {
            if (isAiming)
            {
                if (WeaponType != GunType.Sniper)
                {
                    // this result in a more realist bullet shoot effect when aiming but add a sightly inaccuracy on close targets
                    bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                    if (Physics.Raycast(cp, PlayerCamera.transform.forward, out hit, 500, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
                    {
                        bulletInstanceData.ProjectedHitPoint = hit.point;
                        bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);

#if GFWKTPV
                        if (bl_CameraViewSettings.IsThirdPerson())
                        {
                            // In third person view, we have to make sure the projected hit point is not behind the weapon fire point
                            if (bl_MathUtility.Distance(hit.point, cp) <= bl_MathUtility.Distance(mp, cp))
                            {
                                // if the projected hit point is behind the fire point, then the bullet raycast has to be fired from the 
                                // actual fire point
                                bulletInstanceData.ProjectedHitPoint = cp + (PlayerCamera.transform.forward * 100);
                                bulletInstanceData.Rotation = _transform.parent.rotation;
                            }
                        }
#endif
                    }

                    // this in the other hand make the bullet travel exactly where the player is aiming but the bullets are instanced from the center of the screen
                    // which if you are using some sort of custom effects in the bullets will be noticed (like shooting bullets from the eyes)
                    // but if this is what you need and you are not using custom bullet trails/effects, comment the above lines and uncomment the following one:

                    // bulletInstanceData.Position = cp;
                }
                else
                {
                    bulletInstanceData.Position = cp;
                }
            }
            else
            {
#if GFWKTPV
                // in third person we have to always get the direction from the camera
                if (bl_CameraViewSettings.IsThirdPerson())
                {
                    if (Physics.Raycast(cp, PlayerCamera.transform.forward, out hit, 500, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
                    {

                        // In third person view, we have to make sure the projected hit point is not behind the weapon fire point
                        if (bl_MathUtility.Distance(hit.point, cp) > bl_MathUtility.Distance(mp, cp))
                        {
                            bulletInstanceData.ProjectedHitPoint = hit.point;
                            bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                        }
                        else
                        {
                            // if the projected hit point is behind the fire point, then the bullet raycast has to be fired from the 
                            // actual fire point
                        }
                    }
                }
#endif
            }
        }

        return bulletInstanceData;
    }

    // Fetch the information that will be attached to the next fired bullet containing all required information from the weapon from which the bullet was shoot and the information about the player who shoot it
    public void BuildBulletData()
    {
        BulletSettings.Damage = Damage;
        BulletSettings.ImpactForce = impactForce;
        if (WeaponType == GunType.Sniper && !isAiming)
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
        BulletSettings.WeaponID = GunID;
        BulletSettings.isNetwork = false;
        BulletSettings.DropFactor = bulletDropFactor;
        BulletSettings.Range = Range;
        BulletSettings.ActorViewID = bl_GameManager.LocalPlayerViewID;
        BulletSettings.GFWKActor = bl_GameManager.Instance.LocalActor;
        BulletSettings.IsLocalPlayer = true;
    }

    // Aiming control
    void Aim()
    {
        if (isAiming && !isReloading)
        {
            CurrentPos = AimPosition; //Place in the center ADS
            currentRotation = Quaternion.Euler(aimRotation);
            currentZoom = Zoom; //create a zoom camera
            PlayerReferences.weaponSway.UseAimSettings();
            spreadMinMax = defaultSpreadRange * spreadAimMultiplier;
        }
        else // if not aiming
        {
            CurrentPos = DefaultPos; //return to default gun position
            currentRotation = defaultRotation;
            currentZoom = PlayerReferences.DefaultCameraFOV; //return to default fog
            PlayerReferences.weaponSway.ResetSettings();
            spreadMinMax = defaultSpreadRange;
        }
        float delta = Time.deltaTime;
        //apply position
        _transform.localPosition = useSmooth ? Vector3.Lerp(_transform.localPosition, CurrentPos, delta * AimSpeed) : //with smooth effect
        Vector3.MoveTowards(_transform.localPosition, CurrentPos, delta * AimSpeed); // with snap effect
        _transform.localRotation = Quaternion.Slerp(_transform.localRotation, currentRotation, delta * AimSpeed);

        if (PlayerCamera != null && !BlockAimFoV)
        {
            PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, currentZoom + controller.GetSprintFov(), delta * AimSpeed);
        }
        if (lastAimState != isAiming)
        {
            bl_EventHandler.DispatchLocalAimEvent(isAiming);
            lastAimState = isAiming;
        }
    }

    // send kick back to mouse look when is fire
    public void Kick() => PlayerReferences.recoil.SetRecoil(new bl_RecoilBase.RecoilData() { Amount = WeaponRecoil, Speed = RecoilSpeed });

    void Shake()
    {
        float influence = isAiming ? 0.5f : 1;
        bl_EventHandler.DoPlayerCameraShake(shakerPresent, "fpweapon", influence);
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

    // start reload weapon deduct the remaining bullets in the cartridge of a new clip as this happens, we disable the options: fire, aim and run
    // <returns></returns>
    IEnumerator DoReload(float waitTime = 0.2f)
    {
        isAiming = CanFire = false;

        if (isReloading)
            yield break; // if already reloading... exit and wait till reload is finished

        // Delay before start reloading
        yield return new WaitForSeconds(waitTime);

        if (_remainingClips > 0 || inReloadMode)//if have at least one cartridge
        {
            isReloading = true; // we are now reloading

            if (WeaponAnimation != null)
            {
                if (reloadPer == ReloadPer.Bullet)//insert one bullet at a time
                {
                    int t_repeat = BulletsPerMagazine - bulletsLeft; //get the number of spent bullets
                    int add = (_remainingClips >= t_repeat) ? t_repeat : (int)_remainingClips;
                    WeaponAnimation?.PlayReload(ReloadTime, new int[1] { add }, bl_WeaponAnimationBase.AnimationFlags.SplitReload);

                    // Since this reload method const of multiple animation parts, the reload time
                    // will be determined by the animations, the reload will end once the last animation has been played.
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

            if (WeaponType == GunType.Grenade) { OnAmmoLauncher.ForEach(x => { x?.SetActive(true); }); }
            yield return new WaitForSeconds(ReloadTime); // wait for reload time
            FillMagazine();
        }
        UpdateUI();
        isReloading = inReloadMode = false; // done reloading
        CanAiming = CanFire = true;
    }

    public void FillMagazine()
    {
        if (AmmoType == AmmunitionType.Clips)
        {
            bulletsLeft = BulletsPerMagazine; // fill up the gun
        }
        else
        {
            int need = BulletsPerMagazine - bulletsLeft;
            int add = (_remainingClips >= need) ? need : (int)_remainingClips;
            bulletsLeft += add;
            if (!HaveInfinityAmmo) _remainingClips -= add;
        }
    }

    // <param name="bullet"></param>
    public void AddBullet(int bullet)
    {
        if (AmmoType == AmmunitionType.Bullets)
        {
            _remainingClips -= bullet;
        }
        bulletsLeft += bullet;
        UpdateUI();
    }

    // Set unlimited ammo to this weapon
    // <param name="infinity"></param>
    public void SetInifinityAmmo(bool infinity)
    {
        HaveInfinityAmmo = infinity;
        bulletsLeft = BulletsPerMagazine;
        _remainingClips = AmmoType == AmmunitionType.Bullets ? BulletsPerMagazine * _remainingClips : (int)_remainingClips;
        if (gameObject.activeInHierarchy)
            UpdateUI();
    }

    // Sync Weapon state for Upper animations
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

    // Snap the weapon to the aim position.
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

    // most shotguns have the sound of shooting and then reloading
    // <returns></returns>
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
        isReloading = false; // done reloading
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

    // <param name="part"></param>
    public void PlayReloadAudio(int part)
    {
        if (SoundReloadByAnim) return;

        if (part == 0) PlayClip(ReloadSound);
        else if (part == 1) PlayClip(ReloadSound2);
        else if (part == 2) PlayClip(ReloadSound3);
    }

    public void PlayEmptyFireSound()
    {
        if (WeaponType != GunType.Knife && DryFireSound != null)
        {
            PlayClip(DryFireSound);
        }
    }

    // use this method to various sounds reload. if you have only 1 sound, them put only one in inspector and leave empty other box
    // <returns></returns>
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
            if (WeaponType == GunType.Shotgun)
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

    // <returns></returns>
    IEnumerator KnifeSendFire()
    {
        yield return new WaitForSeconds(0.5f);
        isFiring = false;
        alreadyKnife = false;
    }

    // When we disable the gun ship called the animation and disable the basic functions
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
        CanFire = !bl_GameManager.Instance.GameFinish;
        CanAiming = true;
        if (inReloadMode)
        {
            if (WeaponType == GunType.Grenade)
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

    // When round is end we can't fire
    void OnRoundEnd() => _enable = false;

    public bool OnPickUpAmmo(int bullets, int projectiles, int gunID)
    {
        //if this is not a global ammo but for a specific weapon
        if (gunID != -1)
        {
            //and this is not the weapon that is for
            if (gunID != GunID) return false;
        }

        if (WeaponType == GunType.Knife) return false;
        if (AmmoType == AmmunitionType.Clips)
        {
            //max number of clips
            if (_remainingClips >= maxNumberOfClips) return false;

            _remainingClips += Mathf.FloorToInt(bullets / BulletsPerMagazine);
            if (_remainingClips > maxNumberOfClips)
            {
                _remainingClips = maxNumberOfClips;
            }
        }
        else
        {
            if (WeaponType == GunType.Grenade)
            {
                _remainingClips += projectiles;
                new GFWKLocalNotification(string.Format("+{0} {1}", projectiles.ToString(), Info.Name));
            }
            else
            {
                if (_remainingClips >= BulletsPerMagazine * maxNumberOfClips) return false;

                int oldCount = _remainingClips;
                _remainingClips += bullets;
                _remainingClips = Mathf.Clamp(_remainingClips, 0, BulletsPerMagazine * maxNumberOfClips);
                new GFWKLocalNotification(string.Format("+{0} {1} Bullets", _remainingClips - oldCount, Info.Name));
            }
        }
        UpdateUI();
        return true;
    }

    // <param name="newPoint"></param>
    public void OverrideMuzzlePoint(Transform newPoint)
    {
        defaultMuzzlePoint = muzzlePoint;
        muzzlePoint = newPoint;
    }

    // Enable/Disable the weapon renders/meshes
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
#if CUSTOMIZER
        var customizerWeapon = GetComponent<bl_CustomizerWeapon>();
        if (active && customizerWeapon != null && customizerWeapon.ApplyOnStart)
        {
            customizerWeapon.LoadAttachments();
            customizerWeapon.ApplyAttachments();
        }
#endif
        onWeaponRendersActive?.Invoke(active);
    }

    // <returns></returns>
    public float GetReloadTime()
    {
        if (reloadPer == ReloadPer.Bullet && WeaponAnimation != null)
        {
            int missingBullets = BulletsPerMagazine - bulletsLeft; //get the number of spent bullets
            int add = (_remainingClips >= missingBullets) ? missingBullets : (int)_remainingClips;
            float[] bullets = { add };
            return WeaponAnimation.GetAnimationDuration(bl_WeaponAnimationBase.WeaponAnimationType.Reload, bullets);
        }
        return ReloadTime;
    }

    void CancelFiring() { isFiring = false; }
    void CancelReloading() { WeaponAnimation?.CancelAnimation(bl_WeaponAnimationBase.WeaponAnimationType.Reload); }
    public void ResetDefaultMuzzlePoint() { if (defaultMuzzlePoint != null) muzzlePoint = defaultMuzzlePoint; }
    public void UpdateUI() => bl_EquippedWeaponUIBase.Instance?.SetAmmoOf(this);
    public void SetDefaultWeaponCameraFOV(float fov) => WeaponCamera.fieldOfView = fov;
    public Vector3 GetDefaultPosition() => DefaultPos;

    // <returns></returns>
    public override FireType GetFireType()
    {
        switch (WeaponType)
        {
            case GunType.Machinegun: return FireType.Auto;
            case GunType.Burst: return FireType.Semi;
            case GunType.Knife: return FireType.Undefined;
            default: return FireType.Single;
        }
    }

    // The current <see cref="GunType"/> This can change if the weapon fire type changes in runtime.
    public GunType WeaponType
    {
        get
        {
            if (currentGunType == GunType.None) currentGunType = Info.Type;
            return currentGunType;
        }
        set
        {
            currentGunType = value;
        }
    }

    // The default <see cref="GunType"/> defined in GameData
    public GunType OriginalWeaponType
    {
        get => Info.Type;
    }

    private float BaseSpread
    {
        get { return PlayerReferences.firstPersonController.State == PlayerState.Crouching ? spreadMinMax.x * 0.5f : spreadMinMax.x; }
    }

    public int GetCompactClips { get { return (_remainingClips / BulletsPerMagazine); } }

    // Determine if the player can shoot this weapon.
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

    // Determine if the player can aiming with this weapon in the current state
    public bool CanAiming
    {
        get
        {
            if (WeaponType == GunType.Grenade || WeaponType == GunType.Knife) return false;
            if (controller.GetRunToAimBehave() == PlayerRunToAimBehave.BlockAim && controller.State == PlayerState.Running) return false;

            return (_canAim);
        }
        set => _canAim = value;
    }

    // Determine if the player can reload this weapon
    bool CanReload
    {
        get
        {
            bool can = false;
            if (bulletsLeft < BulletsPerMagazine && _remainingClips > 0 && controller.State != PlayerState.Running && !isReloading)
            {
                can = true;
            }
            if (WeaponType == GunType.Knife && nextFireTime < Time.time)
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
            if (bl_GameData.Instance.CanFireWhileRunning)
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

    public bl_FirstPersonControllerBase controller => PlayerReferences.firstPersonController;
    public bl_PlayerNetwork PlayerNetwork => PlayerReferences.playerNetwork;
    private bl_GunManager gunManager => PlayerReferences.gunManager;

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.root.GetComponent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }

    private bl_WeaponAnimationBase _anim;
    public bl_WeaponAnimationBase WeaponAnimation
    {
        get
        {
            if (_anim == null) { _anim = GetComponentInChildren<bl_WeaponAnimationBase>(); }
            return _anim;
        }
    }

    [System.Serializable]
    public enum BulletInstanceMethod
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

    public struct BulletInstanceData
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