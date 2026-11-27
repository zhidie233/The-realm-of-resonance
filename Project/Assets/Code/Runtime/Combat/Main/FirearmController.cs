using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Domain.Combat;
using Resonance.Core;
using Resonance.Actors;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.UI;

namespace Resonance.Combat
{
    public class FirearmController : NetworkActorBehaviour
    {
        public enum FireType { Undefined, Auto, Single, Semi }

        private FirearmData m_gunInfo = null;
        /// 武器信息
        public FirearmData Info
        {
            get
            {
                if (m_gunInfo == null) m_gunInfo = ProjectConfigRoot.Instance.GetWeapon(WeaponID);
                return m_gunInfo;
            }
            set => m_gunInfo = value;
        }
        /// 武器索引
        public int WeaponID;

        /// 当前武器弹匣内剩余弹药
        public int bulletsLeft
        {
            get => _ammoModel.CurrentMagazine;
            set
            {
                _ammoModel.CurrentMagazine = value;
                EventBus.Publish(new LocalAmmoUpdatedEvent(_ammoModel.CurrentMagazine));
            }
        }
        public bool isAiming { get; set; }
        public bool isFiring { get; set; }

        public LauncherFirearm customWeapon;
        public float CrossHairScale = 8;
        public ShotInstanceMethod bulletInstanceMethod = ShotInstanceMethod.Pooled;
        public string BulletName = "bullet";
        public GameObject bulletPrefab;
        public GameObject grenade = null;
        public ParticleSystem muzzleFlash = null;
        public Transform muzzlePoint = null;
        public WeaponEffects weaponFX = null;
        public ParticleSystem shell = null;
        public GameObject impactEffect = null;
        public Vector3 AimPosition;
        public Vector3 aimRotation = Vector3.zero;
        public bool useSmooth = true;
        public float AimSmooth;
        public ShakePreset shakerPreset;
        [Range(0, 179)] public float aimZoom = 50;
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
        public Renderer[] weaponRenderers = null;
        public PlayerConfig playerSettings;

        public int Damage { get => Info.Damage + extraDamage; set => extraDamage = value; }
        public int RemainingClips { get => _ammoModel.ReserveCount; set => _ammoModel.ReserveCount = value; }
        public float Zoom { get => aimZoom + extraZoom; set => extraZoom = value; }
        public int BulletsPerMagazine { get => bulletsPerClip + extraBullets; set => extraBullets = value; }
        public float MaxSpread { get => extraSpread; set => extraSpread = value; }
        public float WeaponWeight { get => Info.Weight + extraWeight; set => extraWeight = value; }
        public float WeaponRecoil { get => RecoilAmount + extraRecoil; set => extraRecoil = value; }
        public float AimSpeed { get => AimSmooth + extraAimSpeed; set => extraAimSpeed = value; }
        public float ReloadTime { get => Info.ReloadTime + extraReloadTime; set => extraReloadTime = value; }
        public float BulletSpeed { get => bulletSpeed + extraBulletSpeed; set => extraBulletSpeed = value; }
        public int Range { get => Info.Range + extraRange; set => extraRange = value; }
        public AudioClip FireAudioClip
        {
            get => FireSound;
            set => FireSound = _audioFx.ResolveFireClip(FireSound, value);
        }

        public float nextFireTime { get; set; }
        public Camera PlayerCamera { get; private set; }
        public bool BlockAimFoV { get; set; }
        public bool HasInfiniteAmmo => _ammoModel.HasInfiniteAmmo;
        public Action<bool> onWeaponRenderersActive;

        private bool _enable = true;
        public bool canBeTakenWhenIsEmpty = true;
        private bool alreadyKnife = false;
        private Camera WeaponCamera;
        private bool inReloadMode = false;
        private AmmunitionType AmmoType = AmmunitionType.Bullets;
        private bool isInitialized = false;
        private Transform _transform;
        public ShotData BulletSettings = new ShotData();
        public PlayerFPState FPState = PlayerFPState.Idle;
        public int extraDamage, extraBullets = 0;
        private Transform defaultMuzzlePoint = null;
        private float extraWeight, extraRecoil, extraAimSpeed, extraReloadTime, extraBulletSpeed, extraZoom, extraSpread = 0;
        private int extraRange = 0;
        private bool grenadeFired = false;
        private bool isBursting = false;
        private float lastShotTime = 0;
        private bool pendingFirstShot = false;
        private bool _canFire = false;
        private bool _canAim;
        private FirearmType currentGunType = FirearmType.None;

        private readonly AmmoInventoryModel _ammoModel = new AmmoInventoryModel();
        private readonly SpreadRecoilModel _spreadModel = new SpreadRecoilModel();
        private readonly WeaponAimPresenter _aimPresenter = new WeaponAimPresenter();
        private readonly WeaponAudioFxPresenter _audioFx = new WeaponAudioFxPresenter();

        protected virtual void Awake() => Initialized();

        public void Initialized()
        {
            if (isInitialized)
            {
                _audioFx.SyncFireVolume();
                return;
            }

            _transform = transform;
            playerSettings = PlayerReferences.playerSettings;
            _audioFx.Initialize(GetComponent<AudioSource>(), PlayerReferences.gunManager.GetFireAudioSource());
            PlayerCamera = PlayerReferences.playerCamera;

            _spreadModel.SetBaseRange(spreadMinMax.x, spreadMinMax.y);
            AmmoType = ProjectConfigRoot.Instance.AmmoType;
            ReticlePainter.Instance.Block = false;
            Setup();
            customWeapon?.Initialize(this);
            isInitialized = true;
        }

        protected virtual void OnEnable()
        {
            if (!isInitialized) return;

            _audioFx.PlayClip(TakeSound);
            CanFire = false;
            pendingFirstShot = false;
            UpdateUI();
            if (WeaponAnimation) Invoke(nameof(DrawComplete), WeaponAnimation.PlayTakeIn());
            else DrawComplete();

            EventBus.Subscribe<AmmoPickupQuery>(HandleAmmoPickupQuery);
            EventBus.Subscribe<RoundEndedEvent>(HandleRoundEnded);
            ReticlePainter.Instance.SetupCrosshairForWeapon(this);
            HeldWeaponUI.Instance.SetFireType(GetFireType());
            playerSettings?.DoSpawnWeaponRenderEffect(weaponRenderers);
            OnAmmoLauncher.ForEach(x => { x?.SetActive(true); });
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe<AmmoPickupQuery>(HandleAmmoPickupQuery);
            EventBus.Unsubscribe<RoundEndedEvent>(HandleRoundEnded);
            alreadyKnife = false;
            if (PlayerCamera == null) PlayerCamera = PlayerReferences.playerCamera;
            StopAllCoroutines();
            if (isReloading) { inReloadMode = true; isReloading = false; }
            isAiming = false;
            isFiring = false;
            _aimPresenter.ResetAimState();
            ResetDefaultMuzzlePoint();
        }

        public void Setup(bool initial = false)
        {
            var mode = AmmoType == AmmunitionType.Clips ? AmmoStorageMode.Clips : AmmoStorageMode.Bullets;
            _ammoModel.Configure(mode, BulletsPerMagazine, numberOfClips, initial);
            EventBus.Publish(new LocalAmmoUpdatedEvent(_ammoModel.CurrentMagazine));

            if (WeaponType != FirearmType.Shotgun && WeaponType != FirearmType.Sniper)
                reloadPer = ReloadPer.Magazine;

            _aimPresenter.CaptureDefaults(transform);
            if (PlayerCamera == null) PlayerCamera = PlayerReferences.playerCamera;
            WeaponCamera = PlayerReferences.weaponCamera;
            WeaponCamera.fieldOfView = RuntimeServices.Settings != null ? (float)RuntimeServices.Settings.GetSettingOf("武器FOV") : 55;
            CanAim = true;
            ResolveInitialFireMode();
        }

        private void ResolveInitialFireMode()
        {
            if (WeaponType != FirearmType.Machinegun && WeaponType != FirearmType.Pistol && WeaponType != FirearmType.Burst) return;

            FireModeOption preferred = WeaponType == FirearmType.Machinegun
                ? FireModeOption.Auto
                : (WeaponType == FirearmType.Burst ? FireModeOption.Burst : FireModeOption.Single);
            FireModeOption resolved = FireModeSelector.ResolveInitialMode(preferred, CanAuto, CanSemi, CanSingle);
            if (resolved != preferred)
            {
                currentGunType = resolved == FireModeOption.Auto
                    ? FirearmType.Machinegun
                    : (resolved == FireModeOption.Burst ? FirearmType.Burst : FirearmType.Pistol);
            }
        }

        protected virtual void Update()
        {
            if (!_enable) return;

            InputUpdate();
            Aim();
            DetermineUpperState();

            _spreadModel.CurrentSpread = spread;
            bool isCrouching = PlayerReferences.PlayerLocomotionController.State == PlayerState.Crouching;
            spread = _spreadModel.Step(isFiring, isCrouching, spreadPerSecond, decreaseSpreadPerSec, extraSpread);
        }

        void InputUpdate()
        {
            if (ProjectConfigRoot.Instance.isChatOpen || !gunManager.isGameStarted || !LobbyMenu.Instance.isCursorLocked) return;

            bool fireDown = GameInput.Fire(GameInputType.Down);
            if (!CommonUtil.isMobile)
            {
                if (CanFire)
                {
                    if (fireDown) SingleFire();
                    if (GameInput.Fire()) LoopFire();
                }
                else if (fireDown && bulletsLeft <= 0 && !isReloading)
                {
                    PlayEmptyFireSound();
                }

                isAiming = GameInput.Aim() && CanAim;
            }

            if (fireDown && isReloading && (WeaponType == FirearmType.Sniper || WeaponType == FirearmType.Shotgun) && bulletsLeft > 0)
            {
                CancelReloading();
            }

            if (LobbyMenu.Instance.isCursorLocked) ReticlePainter.Instance.OnAim(isAiming);
            if (GameInput.Reload() && CanReload) Reload();
            if (WeaponType == FirearmType.Machinegun || WeaponType == FirearmType.Burst || WeaponType == FirearmType.Pistol) ChangeTypeFire();

            if (!CommonUtil.isMobile && WeaponType != FirearmType.Grenade && WeaponType != FirearmType.Knife)
            {
                if (WeaponType == FirearmType.Machinegun)
                {
                    isFiring = GameInput.Fire() && CanFire;
                }
                else if (fireDown && CanFire)
                {
                    isFiring = true;
                    CancelInvoke(nameof(CancelFiring));
                    Invoke(nameof(CancelFiring), 0.12f);
                }
            }
        }

        void ChangeTypeFire()
        {
            if (!GameInput.SwitchFireMode()) return;

            FireModeOption current = WeaponType == FirearmType.Machinegun
                ? FireModeOption.Auto
                : (WeaponType == FirearmType.Burst ? FireModeOption.Burst : FireModeOption.Single);
            FireModeOption next = FireModeSelector.CycleNextMode(current, CanAuto, CanSemi, CanSingle);
            WeaponType = next == FireModeOption.Auto
                ? FirearmType.Machinegun
                : (next == FireModeOption.Burst ? FirearmType.Burst : FirearmType.Pistol);
            HeldWeaponUI.Instance.SetFireType(GetFireType());
            gunManager.PlaySound(0);
        }

        void SingleFire()
        {
            if ((PlayerReferences.PlayerLocomotionController.State == PlayerState.Running && Time.time - lastShotTime > Info.FireRate * 2) || pendingFirstShot)
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

        void LoopFire()
        {
            if (WeaponType != FirearmType.Machinegun) return;
            if ((PlayerReferences.PlayerLocomotionController.State == PlayerState.Running && Time.time - lastShotTime > Info.FireRate * 5) || pendingFirstShot)
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

        void GunTypeFire(bool auto)
        {
            if (customWeapon != null) { customWeapon.OnFireDown(); return; }

            switch (WeaponType)
            {
                case FirearmType.Machinegun: if (auto) MachineGunFire(); break;
                case FirearmType.Shotgun: ShotgunFire(); break;
                case FirearmType.Burst: if (!isBursting) StartCoroutine(BurstFire()); break;
                case FirearmType.Grenade: if (!grenadeFired) GrenadeFire(); break;
                case FirearmType.Pistol: MachineGunFire(); break;
                case FirearmType.Sniper: SniperFire(); break;
                case FirearmType.Knife: if (!alreadyKnife) KnifeFire(); break;
            }
        }

        void MachineGunFire()
        {
            float time = Time.time;
            if (nextFireTime > time) return;
            nextFireTime = time + (Info.FireRate * 0.1f);
            FireOneShot();
            if (isAiming) WeaponAnimation?.PlayFire(WeaponRigHandler.AnimationFlags.Aiming);
            else WeaponAnimation?.PlayFire();
            OnFireCommons();
            PlayFX();
            CheckBullets();
        }

        void SniperFire()
        {
            float time = Time.time;
            if (time - Info.FireRate > nextFireTime) nextFireTime = time - Time.deltaTime;
            while (nextFireTime < time)
            {
                FireOneShot();
                WeaponAnimation?.PlayFire();
                StartCoroutine(_audioFx.DelayFireSoundRoutine(FireSound, delayForSecondFireSound, DelaySource, ReloadSound3));
                OnFireCommons();
                gunManager.HeadAnimator.Play("Sniper", 0, 0);
                PlayFX();
                CheckBullets(delayForSecondFireSound + 0.2f);
            }
        }

        private float KnifeFire(bool quickFire = false)
        {
            if (Time.time - Info.FireRate > nextFireTime) nextFireTime = Time.time - Time.deltaTime;
            float time = 0;
            while (nextFireTime < Time.time)
            {
                isFiring = true;
                alreadyKnife = true;
                StartCoroutine(KnifeSendFire());
                CastMeleeHit();
                if (WeaponAnimation != null)
                {
                    time = quickFire ? WeaponAnimation.PlayFire(WeaponRigHandler.AnimationFlags.QuickFire) : WeaponAnimation.PlayFire();
                }
                PlayerNetwork.ReplicateFire(FirearmType.Knife, Vector3.zero, Vector3.zero);
                OnFireCommons();
                ReticlePainter.Instance.OnFire();
                isFiring = false;
                EventBus.Publish(new LocalWeaponFiredEvent(WeaponID));
            }
            return time;
        }

        private void CastMeleeHit()
        {
            Vector3 position = PlayerCamera.transform.position;
            Vector3 direction = PlayerCamera.transform.TransformDirection(Vector3.forward);
            float range = PlayerReferences.cameraRay == null ? Info.Range : Info.Range + PlayerReferences.cameraRay.ExtraRayDistance;
            if (!Physics.SphereCast(position, 0.2f, direction, out RaycastHit hit, range)) return;

            var damageable = hit.transform.GetComponent<IGameDamageable>();
            if (damageable != null)
            {
                var damageData = new DamageInfo
                {
                    Damage = Info.Damage,
                    Actor = LocalPlayer,
                    GameActor = GameController.Instance.LocalActor,
                    Cause = DamageCause.Player,
                    Direction = _transform.position,
                    WeaponID = WeaponID,
                    ActorViewID = GameController.LocalPlayerViewID,
                    From = LocalPlayer.NickName
                };
                damageable.ReceiveDamage(damageData);
            }
            if (hit.transform.CompareTag(RuntimeServices.HITBOX_TAG))
            {
                ObjectRecycler.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            }
        }

        IEnumerator BurstFire()
        {
            int shotCounter = 0;
            if (Time.time - lagBetweenBurst > nextFireTime) nextFireTime = Time.time - Time.deltaTime;
            int shots = Mathf.Min(roundsPerBurst, bulletsLeft);
            while (nextFireTime < Time.time)
            {
                while (shotCounter < shots)
                {
                    isBursting = true;
                    FireOneShot();
                    shotCounter++;
                    OnFireCommons();
                    PlayFX();
                    if (isAiming) WeaponAnimation?.PlayFire(WeaponRigHandler.AnimationFlags.Aiming);
                    else WeaponAnimation?.PlayFire();
                    yield return new WaitForSeconds(Info.FireRate);
                    if (bulletsLeft <= 0) break;
                }
                nextFireTime += lagBetweenBurst;
                CheckBullets();
            }
            isBursting = false;
        }

        void ShotgunFire()
        {
            if (Time.time - Info.FireRate > nextFireTime) nextFireTime = Time.time - Time.deltaTime;
            int pelletCounter = 0;
            while (nextFireTime < Time.time)
            {
                do { FireOneShot(); pelletCounter++; } while (pelletCounter < pelletsPerShot);
                if (!SoundReloadByAnim) StartCoroutine(_audioFx.DelayFireSoundRoutine(FireSound, delayForSecondFireSound, DelaySource, ReloadSound3));
                else PlayFireAudio();
                WeaponAnimation?.PlayFire();
                OnFireCommons();
                PlayFX();
                CheckBullets(delayForSecondFireSound + 0.3f);
            }
        }

        void GrenadeFire(bool fastFire = false)
        {
            if (grenadeFired || (Time.time - nextFireTime) <= Info.FireRate) return;
            if (!fastFire && bulletsLeft == 0 && RemainingClips > 0) { Reload(1); return; }
            isFiring = true;
            grenadeFired = true;
            if (ThrowByAnimation)
            {
                nextFireTime = Time.time + Info.FireRate;
                if (fastFire) WeaponAnimation?.PlayFire(WeaponRigHandler.AnimationFlags.QuickFire);
                else WeaponAnimation?.PlayFire();
            }
            else StartCoroutine(ThrowGrenade(fastFire));
        }

        public IEnumerator ThrowGrenade(bool fastFire = false, bool useDelay = true)
        {
            float t = 0;
            float throwForce = BulletSpeed * 22;
            float upwardForce = bulletDropFactor * 33;
            if (useDelay)
            {
                nextFireTime = Time.time + Info.FireRate;
                t = fastFire ? WeaponAnimation.PlayFire(WeaponRigHandler.AnimationFlags.QuickFire) : WeaponAnimation.PlayFire();
                float d = fastFire ? DelayFire + WeaponAnimation.GetAnimationDuration(WeaponRigHandler.WeaponRigType.TakeIn) : DelayFire;
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
            if (bulletsLeft <= 0) OnAmmoLauncher.ForEach(x => { if (x != null) x.SetActive(false); });
            if (!fastFire && AutoReload)
            {
                if (useDelay) yield return new WaitForSeconds(t - DelayFire);
                if (bulletsLeft <= 0 && RemainingClips > 0) Reload(0);
                else if (bulletsLeft <= 0) gunManager?.OnReturnWeapon();
            }
        }

        public IEnumerator FastGrenadeFire(Action callBack)
        {
            float tt = WeaponAnimation.GetAnimationDuration(WeaponRigHandler.WeaponRigType.AimFire)
                + WeaponAnimation.GetAnimationDuration(WeaponRigHandler.WeaponRigType.TakeIn);
            GrenadeFire(true);
            yield return new WaitForSeconds(tt);
            if (RemainingClips > 0)
            {
                bulletsLeft++;
                if (!HasInfiniteAmmo) RemainingClips--;
                UpdateUI();
            }
            callBack();
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        public void QuickMelee(Action callBack) => StartCoroutine(QuickMeleeSequence(callBack));

        IEnumerator QuickMeleeSequence(Action callBack)
        {
            float tt = KnifeFire(true);
            yield return new WaitForSeconds(tt);
            callBack();
            gameObject.SetActive(false);
        }

        void FireOneShot()
        {
            BuildBulletData();
            var instanceData = GetBulletPosition();
            GameObject instancedBullet = ObjectRecycler.Instance.Instantiate(BulletName, instanceData.Position, instanceData.Rotation);
            instancedBullet.GetComponent<ProjectileBase>().InitProjectile(BulletSettings);
            ReticlePainter.Instance.OnFire();
            PlayerNetwork.ReplicateFire(OriginalWeaponType, instanceData.ProjectedHitPoint, BulletSettings.Inaccuracy);
            EventBus.Publish(new LocalWeaponFiredEvent(WeaponID));
            if (bulletsLeft == 0) Reload();
        }

        void FireOneProjectile(Vector3 direction, Vector3 origin)
        {
            BuildBulletData();
            GameObject newNoobTube = Instantiate(grenade, origin, transform.parent.rotation);
            var proRigid = newNoobTube.GetComponent<Rigidbody>();
            if (proRigid != null) proRigid.AddForce(direction, ForceMode.Impulse);
            newNoobTube.GetComponent<ProjectileBase>().InitProjectile(BulletSettings);
            grenadeFired = false;
            EventBus.Publish(new LocalWeaponFiredEvent(WeaponID));
        }

        public void OnFireCommons()
        {
            PlayFireAudio();
            if (WeaponType != FirearmType.Knife) bulletsLeft--;
            UpdateUI();
            nextFireTime += Info.FireRate;
            _audioFx.EjectShell(shell);
            Kick();
            _audioFx.PublishShake(shakerPreset, isAiming);
        }

        public void PlayFX() => _audioFx.PlayMuzzleFx(isAiming, weaponFX, muzzleFlash);

        public ShotInstanceData GetBulletPosition() => BallisticsSolver.ComputeShotInstanceData(
            _transform.parent, PlayerCamera, muzzlePoint, ProjectConfigRoot.TagsAndLayerSettings.LocalPlayerHittableLayers, isAiming, WeaponType);

        public void BuildBulletData() => BallisticsSolver.PopulateBulletData(BulletSettings, this, _spreadModel);

        void Aim()
        {
            bool activeAim = _aimPresenter.UpdateAim(
                _transform, PlayerCamera, PlayerReferences, controller,
                isAiming, isReloading, BlockAimFoV, useSmooth, AimSpeed,
                AimPosition, aimRotation, Zoom, Time.deltaTime);
            _spreadModel.ApplyAimState(activeAim, spreadAimMultiplier);
            spreadMinMax = new Vector2(_spreadModel.CurrentMin, _spreadModel.CurrentMax);
        }

        public void Kick() => _audioFx.ApplyKick(PlayerReferences, WeaponRecoil, RecoilSpeed);

        public void CheckBullets(float delay = 0)
        {
            if (bulletsLeft <= 0 && RemainingClips > 0 && AutoReload) Reload(delay);
        }

        public void Reload(float delay = 0.2f)
        {
            if (isReloading || !gameObject.activeInHierarchy) return;
            StopCoroutine(nameof(DoReload));
            StartCoroutine(nameof(DoReload), delay);
        }

        IEnumerator DoReload(float waitTime = 0.2f)
        {
            isAiming = CanFire = false;
            if (isReloading) yield break;
            yield return new WaitForSeconds(waitTime);
            if (RemainingClips > 0 || inReloadMode)
            {
                isReloading = true;
                if (WeaponAnimation != null)
                {
                    if (reloadPer == ReloadPer.Bullet)
                    {
                        int add = _ammoModel.CalculateSplitReloadRounds(BulletsPerMagazine);
                        WeaponAnimation.PlayReload(ReloadTime, new int[] { add }, WeaponRigHandler.AnimationFlags.SplitReload);
                        yield break;
                    }
                    WeaponAnimation.PlayReload(ReloadTime, null);
                }
                if (!SoundReloadByAnim)
                {
                    int missing = BulletsPerMagazine - bulletsLeft;
                    StartCoroutine(_audioFx.ReloadSoundRoutine(ReloadTime, WeaponType, missing, ReloadSound, ReloadSound2, ReloadSound3, gunManager));
                }
                if (!inReloadMode) _ammoModel.ConsumeClipOnReloadStart();
                if (WeaponType == FirearmType.Grenade) OnAmmoLauncher.ForEach(x => { x?.SetActive(true); });
                yield return new WaitForSeconds(ReloadTime);
                FillMagazine();
            }
            UpdateUI();
            isReloading = inReloadMode = false;
            CanAim = CanFire = true;
        }

        public void FillMagazine()
        {
            _ammoModel.FillMagazine(BulletsPerMagazine);
            EventBus.Publish(new LocalAmmoUpdatedEvent(_ammoModel.CurrentMagazine));
        }

        public void AddBullet(int bullet)
        {
            _ammoModel.AddSingleRound(bullet);
            EventBus.Publish(new LocalAmmoUpdatedEvent(_ammoModel.CurrentMagazine));
            UpdateUI();
        }

        public void SetInfiniteAmmo(bool infinite)
        {
            _ammoModel.SetInfiniteAmmo(infinite, BulletsPerMagazine);
            EventBus.Publish(new LocalAmmoUpdatedEvent(_ammoModel.CurrentMagazine));
            if (gameObject.activeInHierarchy) UpdateUI();
        }

        void DetermineUpperState()
        {
            if (PlayerNetwork == null) return;
            FPState = (PlayerFPState)(byte)UpperBodyStateResolver.Resolve(
                isFiring, isAiming, isReloading, controller.State == PlayerState.Running);
            PlayerNetwork.FPState = FPState;
        }

        public void SetToAim() => _aimPresenter.SnapToAim(_transform, AimPosition, aimRotation);
        public void PlayFireAudio() => _audioFx.PlayFireAudio(FireSound);

        public void FinishReload()
        {
            _ammoModel.ConsumeClipOnReloadStart();
            isReloading = false;
            CanAim = true;
            CanFire = true;
            inReloadMode = false;
        }

        public void PlayReloadAudio(int part) => _audioFx.PlayReloadAudio(part, SoundReloadByAnim, ReloadSound, ReloadSound2, ReloadSound3);
        public void PlayEmptyFireSound() => _audioFx.PlayEmptyFireSound(WeaponType, DryFireSound);

        IEnumerator KnifeSendFire()
        {
            yield return new WaitForSeconds(0.5f);
            isFiring = false;
            alreadyKnife = false;
        }

        public float DisableWeapon(bool isFastKill = false)
        {
            CanAim = false;
            if (isReloading) { inReloadMode = true; isReloading = false; }
            CanFire = false;
            gunManager?.HeadAnimation(0, 1);
            if (PlayerCamera == null) PlayerCamera = PlayerReferences.playerCamera;
            if (!isFastKill) StopAllCoroutines();
            return WeaponAnimation != null ? WeaponAnimation.PlayTakeOut() : 0;
        }

        void DrawComplete()
        {
            CanFire = !GameController.Instance.GameFinish;
            CanAim = true;
            if (!inReloadMode) return;
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

        public bool OnPickUpAmmo(int bullets, int projectiles, int weaponID)
        {
            var result = _ammoModel.TryPickUpAmmo(
                bullets, projectiles, weaponID, WeaponID,
                WeaponType == FirearmType.Knife, WeaponType == FirearmType.Grenade,
                BulletsPerMagazine, maxNumberOfClips);
            if (!result.Consumed) return false;

            if (result.ShouldNotify)
            {
                string msg = result.IsGrenade
                    ? string.Format("+{0} {1}", result.AddedAmount.ToString(), Info.Name)
                    : string.Format("+{0} {1} Bullets", result.AddedAmount, Info.Name);
                new GameLocalNotification(msg);
            }
            UpdateUI();
            return true;
        }

        public void OverrideMuzzlePoint(Transform newPoint) { defaultMuzzlePoint = muzzlePoint; muzzlePoint = newPoint; }

        public void SetWeaponRenderersActive(bool active) =>
            _audioFx.SetWeaponRenderersActive(_transform, ref weaponRenderers, active, onWeaponRenderersActive);

        public float GetReloadTime()
        {
            if (reloadPer == ReloadPer.Bullet && WeaponAnimation != null)
            {
                int add = _ammoModel.CalculateSplitReloadRounds(BulletsPerMagazine);
                return WeaponAnimation.GetAnimationDuration(WeaponRigHandler.WeaponRigType.Reload, new float[] { add });
            }
            return ReloadTime;
        }

        void CancelFiring() => isFiring = false;
        void CancelReloading() => WeaponAnimation?.CancelAnimation(WeaponRigHandler.WeaponRigType.Reload);
        public void ResetDefaultMuzzlePoint() { if (defaultMuzzlePoint != null) muzzlePoint = defaultMuzzlePoint; }
        public void UpdateUI() => HeldWeaponUI.Instance?.SetAmmoOf(this);
        public void SetDefaultWeaponCameraFOV(float fov) => WeaponCamera.fieldOfView = fov;
        public Vector3 GetDefaultPosition() => _aimPresenter.DefaultPosition;

        public FireType GetFireType() => WeaponType switch
        {
            FirearmType.Machinegun => FireType.Auto,
            FirearmType.Burst => FireType.Semi,
            FirearmType.Knife => FireType.Undefined,
            _ => FireType.Single
        };

        public FirearmType WeaponType
        {
            get
            {
                if (currentGunType == FirearmType.None) currentGunType = Info.Type;
                return currentGunType;
            }
            set => currentGunType = value;
        }

        public FirearmType OriginalWeaponType => Info.Type;
        public int GetCompactClips => _ammoModel.GetCompactClips(BulletsPerMagazine);
        public bool CanFire { get => bulletsLeft > 0 && _canFire && !isReloading && FireWhileRun; set => _canFire = value; }
        public bool FireRatePassed => (Time.time - nextFireTime) > Info.FireRate;
        public bool AllowQuickFire() => m_AllowQuickFire && bulletsLeft > 0 && FireRatePassed;

        public bool CanAim
        {
            get
            {
                if (WeaponType == FirearmType.Grenade || WeaponType == FirearmType.Knife) return false;
                if (controller.GetRunToAimBehavior() == PlayerRunToAimBehavior.BlockAim && controller.State == PlayerState.Running) return false;
                return _canAim;
            }
            set => _canAim = value;
        }

        bool CanReload => _ammoModel.CanReload(
            BulletsPerMagazine,
            controller.State == PlayerState.Running,
            isReloading,
            WeaponType == FirearmType.Knife,
            nextFireTime < Time.time);

        bool FireWhileRun => ProjectConfigRoot.Instance.fireWhileSprinting || controller.State != PlayerState.Running;
        public bool CanBeTaken() => _ammoModel.CanBeTaken(canBeTakenWhenIsEmpty);
        public bool IsOutOfAmmo() => _ammoModel.IsOutOfAmmo();

        public PlayerLocomotionController controller => PlayerReferences.PlayerLocomotionController;
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
        private WeaponRigHandler _anim;
        public WeaponRigHandler WeaponAnimation
        {
            get
            {
                if (_anim == null) _anim = GetComponentInChildren<WeaponRigHandler>();
                return _anim;
            }
        }

        [Serializable] public enum ShotInstanceMethod { Pooled, Instanced }
        [Serializable] public enum ReloadPer { Bullet, Magazine }
        public struct ShotInstanceData { public Vector3 Position; public Quaternion Rotation; public Vector3 ProjectedHitPoint; }

    #if UNITY_EDITOR
        public bool _aimRecord = false;
        public Vector3 _defaultPosition = new Vector3(-100, 0, 0);
        public Vector3 _defaultRotation = new Vector3(-100, 0, 0);
        private void OnDrawGizmos()
        {
            if (muzzlePoint == null) return;
            Gizmos.color = new Color(0, 1, 0, 0.4f);
            Gizmos.DrawSphere(muzzlePoint.position, 0.022f);
            Gizmos.color = Color.white;
        }
    #endif

        private void HandleAmmoPickupQuery(AmmoPickupQuery q) { if (OnPickUpAmmo(q.Bullets, q.Projectiles, q.WeaponId)) q.Consumed = true; }
        private void HandleRoundEnded(RoundEndedEvent e) => _enable = false;
    }
}
