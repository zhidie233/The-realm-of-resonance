using System;
using System.Collections;
using UnityEngine;
/// 处理 FPWeapon 动画的 Game 默认脚本。
/// 脚本继承 WeaponRigBase.cs 后可以用自己的脚本处理动画
public class WeaponRigHandler : WeaponRigBase
{
    #region Public members
    public AnimationType m_AnimationType = AnimationType.Animation;
    public FireBlendMethod fireBlendMethod = FireBlendMethod.FireSpeed;
    public AnimationClip DrawName;
    public AnimationClip TakeOut;
    public AnimationClip SoloFireClip;
    public AnimationClip[] FireAnimations;
    public AnimationClip FireAimAnimation;
    public AnimationClip ReloadName;
    public AnimationClip IdleClip;
    [Range(0.1f, 5)] public float FireSpeed = 1.0f;
    [Range(0.1f, 5)] public float DrawSpeed = 1.0f;
    [Range(0.1f, 5)] public float HideSpeed = 1.0f;
    public AnimationClip StartReloadAnim;
    public AnimationClip InsertAnim;
    public AnimationClip AfterReloadAnim;
    [Range(0.1f, 5)] public float InsertSpeed = 1.0f;
    public AnimationClip QuickFireAnim;
    public ParticleSystem[] Particles;
    [Range(1, 10)] public float ParticleRate = 5;
    public bool HasParticles = false;
    public bool AnimatedMovements = false;
    public bool DrawAfterFire = false;

    public AudioClip Reload_1;
    public AudioClip Reload_2;
    public AudioClip Reload_3;
    public AudioClip m_Fire;
    #endregion

    #region Private members
    private FirearmInventory GunManager;
    private bool cancelReload = false;
    private PlayerRig playerReferences;
    private AudioSource m_source;
    private FirearmType gunType = FirearmType.Machinegun;
    [NonSerialized] private FirearmController m_linkWeapon = null;
    public FirearmController ParentGun
    {
        get
        {
            if (m_linkWeapon == null)
            {
                m_linkWeapon = transform.GetComponentInParent<FirearmController>(true);
            }
            return m_linkWeapon;
        }
    }
    #endregion

    void Awake()
    {
        gunType = ParentGun.Info.Type;
        GunManager = transform.GetComponentInParent<FirearmInventory>();
        playerReferences = GunManager.playerReferences;

        m_source = GetComponent<AudioSource>();
        if (m_source == null)
        {
            m_source = gameObject.AddComponent<AudioSource>();
            m_source.playOnAwake = false;
        }
    }

    void OnEnable()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.wrapMode = WrapMode.Once;
        }
        else
        {
            if (AnimatedMovements) { playerReferences.weaponBob.AnimatedThis(UpdateAnimated, true); } else { playerReferences.weaponBob.AnimatedThis(null, false); }
            GameEvents.onLocalAimChanged += OnAim;
        }
    }

    private void OnDisable()
    {
        if (m_AnimationType == AnimationType.Animator)
        {
            GameEvents.onLocalAimChanged -= OnAim;
        }
    }

    void OnAim(bool isAiming)
    {
        if (m_AnimationType == AnimationType.Animator)
        {
            if (!isAiming || ParentGun.FPState == PlayerFPState.Reloading) return;

            animator.Play("Idle", 0, 0);
        }
    }

    #region Base interface
    public override float PlayFire(AnimationFlags flags)
    {
        switch (gunType)
        {
            case FirearmType.Knife:
                return KnifeFire(flags.IsEnumFlagPresent(AnimationFlags.QuickFire));
            case FirearmType.Grenade:
                return FireGrenade(flags.IsEnumFlagPresent(AnimationFlags.QuickFire));
            default:
                if (!flags.IsEnumFlagPresent(AnimationFlags.Aiming))
                    return Fire();
                else return AimFire();
        }
    }

    public override void PlayReload(float reloadTime, int[] data, AnimationFlags flags, Action onFinish = null)
    {
        if (flags.IsEnumFlagPresent(AnimationFlags.SplitReload))
            SplitReload(reloadTime, data[0]);
        else
            Reload(reloadTime);
    }

    public override float PlayTakeIn()
    {
        // 这里不直接处理，因为 DrawWeapon() 函数从 1 版本就存在。
        return DrawWeapon();
    }

    public override float PlayTakeOut()
    {
        // 这里不直接处理，因为 HideWeapon() 函数从 1 版本就存在。
        return HideWeapon();
    }

    public override void CancelAnimation(WeaponRigType animationType)
    {
        if (animationType == WeaponRigType.Reload) cancelReload = true;
    }

    public override float GetAnimationDuration(WeaponRigType animationType, float[] data = null)
    {
        switch (animationType)
        {
            case WeaponRigType.Fire:
                return GetFireLenght;
            case WeaponRigType.AimFire:
                return FireAimAnimation.length / FireSpeed;
            case WeaponRigType.TakeIn:
                return DrawName.length / DrawSpeed;
            case WeaponRigType.TakeOut:
                return TakeOut.length / HideSpeed;
            case WeaponRigType.Reload:
                if (ParentGun.reloadPer == FirearmController.ReloadPer.Magazine)
                    return ReloadName.length / ParentGun.Info.ReloadTime;
                else
                {
                    float insertD = InsertAnim.length / InsertSpeed;
                    if (data != null && data.Length > 0)
                    {
                        insertD = insertD * data[0];
                    }
                    return StartReloadAnim.length + AfterReloadAnim.length + insertD;
                }
            default:
                return 0;
        }
    }

    #endregion

    void UpdateAnimated(PlayerState playerState)
    {
        if (AnimatedMovements)
        {
            float speedPercentage = playerReferences.firstPersonController.VelocityMagnitude / playerReferences.firstPersonController.GetSpeedOnState(PlayerState.Running, true);
            animator.SetFloat("Speed", speedPercentage);
            animator.SetInteger("PlayerState", (int)playerState);

            if (speedPercentage > 0.55f && ParentGun.FPState != PlayerFPState.Reloading && ParentGun.WeaponType != FirearmType.Knife && ParentGun.WeaponType != FirearmType.Grenade)
            {
                animator.CrossFade("Run", 0.2f);
            }
        }
    }
    /// 播放一次开火动画
    public float Fire()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (FireAnimations.Length <= 0)
                return 0;

            int id = UnityEngine.Random.Range(0, FireAnimations.Length);
            if (FireAnimations[id] == null) { id = 0; }

            string n = FireAnimations[id].name;
            Anim.Rewind(n);
            Anim[n].speed = FireSpeed;
            Anim.Play(n);

            return FireAnimations[id].length / FireSpeed;
        }
        else
        {
            float fireSpeed = FireSpeed;
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = ((SoloFireClip.length * 0.5f) / ParentGun.Info.FireRate);
            }

            animator.SetFloat("FireSpeed", fireSpeed);
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                animator.Play("Fire", 0, 0.03f);
            }
            else
            {
                animator.CrossFade("Fire", 0.025f, 0, 0.03f);
            }

            return SoloFireClip.length / FireSpeed;
        }
    }
    /// 播放刀的开火动画
    public float KnifeFire(bool Quickfire)
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (FireAimAnimation == null)
                return 0;

            string an = Quickfire ? QuickFireAnim.name : FireAimAnimation.name;
            Anim[an].speed = FireSpeed;
            Anim.Play(an);

            return Anim[an].length / FireSpeed;
        }
        else
        {
            animator.SetFloat("FireSpeed", FireSpeed);
            string an = Quickfire ? "QuickFire" : "Fire";
            animator.Play(an, 0, 0);
            if (SoloFireClip == null) return FireSpeed;
            return SoloFireClip.length / FireSpeed;
        }
    }
    /// 播放手雷的开火动画
    public float FireGrenade(bool fastFire)
    {
        if (fastFire)
        {
            StartCoroutine(FastGrenade());
            return GetFireLenght + (DrawName.length / DrawSpeed);
        }
        else
        {
            if (m_AnimationType == AnimationType.Animation)
            {
                return AimFire();
            }
            else
            {
                animator.SetFloat("FireSpeed", FireSpeed);
                animator.Play("Fire", 0, 0);
                float t = SoloFireClip.length / FireSpeed;
                animator.SetLayerWeight(1, 0);
                if (DrawAfterFire) { Invoke(nameof(DrawWeapon), ((t + ParentGun.Info.ReloadTime)) - (GetAnimationDuration(WeaponRigType.TakeIn) * 0.5f)); }
                return t;
            }
        }
    }

    IEnumerator FastGrenade()
    {
        yield return new WaitForSeconds(DrawWeapon());
        AimFire();
    }
    /// 抛出手雷预制体（来自 FirearmController.cs）
    /// 应从动画事件中调用
    public void ThrowProjectile()
    {
        StartCoroutine(ParentGun.ThrowGrenade(false, false));
    }

    public float AimFire()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (FireAimAnimation == null)
                return 0;

            Anim.Rewind(FireAimAnimation.name);
            Anim[FireAimAnimation.name].speed = FireSpeed;
            Anim.Play(FireAimAnimation.name);

            return FireAimAnimation.length / FireSpeed;
        }
        else
        {
            float fireSpeed = FireSpeed;
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = ((SoloFireClip.length * 0.5f) / ParentGun.Info.FireRate);
            }

            animator.SetFloat("FireSpeed", fireSpeed);
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                animator.Play("AimFire", 0, 0.02f);
            }
            else
            {
                animator.CrossFade("AimFire", 0.025f, 0, 0.02f);
            }
            return FireAimAnimation.length / FireSpeed;
        }
    }
    /// 播放掏枪动画
    public float DrawWeapon()
    {
        if (DrawName == null || !gameObject.activeInHierarchy)
            return 0;

        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.Rewind(DrawName.name);
            Anim[DrawName.name].speed = DrawSpeed;
            Anim[DrawName.name].time = 0;
            Anim.Play(DrawName.name);

            return Anim[DrawName.name].length / DrawSpeed;
        }
        else
        {
            animator.SetFloat("DrawSpeed", DrawSpeed);
            animator.Play("Draw", 0, 0);
            animator.SetLayerWeight(1, 1);
            return DrawName.length / DrawSpeed;
        }
    }
    ///  播放收枪动画
    public float HideWeapon()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (TakeOut == null)
                return 0;

            Anim[TakeOut.name].speed = HideSpeed;
            Anim[TakeOut.name].time = 0;
            Anim[TakeOut.name].wrapMode = WrapMode.Once;
            Anim.Play(TakeOut.name);
        }
        else
        {
            animator.SetFloat("HideSpeed", HideSpeed);
            animator.CrossFade("Hide", 0.25f);
        }
        return TakeOut == null ? 0 : TakeOut.length / HideSpeed;
    }
    /// 处于换弹状态时由动画调用的事件
    public void Reload(float ReloadTime)
    {
        if (ReloadName == null)
            return;

        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.Stop(ReloadName.name);
            Anim[ReloadName.name].wrapMode = WrapMode.Once;
            Anim[ReloadName.name].speed = (ReloadName.length / ReloadTime);
            Anim.Play(ReloadName.name);
        }
        else
        {
            animator.SetFloat("ReloadSpeed", ReloadName.length / ReloadTime);
            animator.Play("Reload", 0, 0);
        }
    }
    /// 开火时由动画调用的事件
    public void FireAudio()
    {
        if (m_source != null && m_Fire != null)
        {
            m_source.clip = m_Fire;
            m_source.pitch = UnityEngine.Random.Range(1, 1.5f);
            m_source.Play();
        }
    }

    public void SplitReload(float ReloadTime, int bulletsToAdd)
    {
        StartCoroutine(StartShotgunReload());
        IEnumerator StartShotgunReload()
        {
            if (m_AnimationType == AnimationType.Animation)
            {
                Anim.CrossFade(StartReloadAnim.name, 0.2f);
                ParentGun.PlayReloadAudio(0);
                yield return new WaitForSeconds(StartReloadAnim.length);
                for (int i = 0; i < bulletsToAdd; i++)
                {
                    Anim[InsertAnim.name].wrapMode = WrapMode.Loop;
                    float speed = Anim[InsertAnim.name].length / InsertSpeed;
                    Anim[InsertAnim.name].speed = speed;
                    Anim.CrossFade(InsertAnim.name);
                    GunManager.HeadAnimation(3, speed);
                    ParentGun.PlayReloadAudio(1);
                    yield return new WaitForSeconds(InsertAnim.length / speed);
                    ParentGun.AddBullet(1);
                    if (cancelReload)
                    {
                        Anim.CrossFade(AfterReloadAnim.name, 0.2f);
                        GunManager.HeadAnimation(0, AfterReloadAnim.length);
                        yield return new WaitForSeconds(AfterReloadAnim.length);
                        ParentGun.FinishReload();
                        cancelReload = false;
                        yield break;
                    }
                }
                Anim.CrossFade(AfterReloadAnim.name, 0.2f);
                GunManager.HeadAnimation(0, AfterReloadAnim.length);
                ParentGun.PlayReloadAudio(2);
                yield return new WaitForSeconds(AfterReloadAnim.length);
                ParentGun.FinishReload();
            }
            else
            {
                animator.speed = 1;
                animator.Play("StartReload", 0, 0);
                ParentGun.PlayReloadAudio(0);
                yield return new WaitForSeconds(StartReloadAnim.length);
                for (int i = 0; i < bulletsToAdd; i++)
                {
                    float speed = InsertAnim.length / InsertSpeed;
                    animator.SetFloat("ReloadSpeed", InsertSpeed);
                    animator.Play("Insert", 0, 0);
                    GunManager.HeadAnimation(3, speed);
                    ParentGun.PlayReloadAudio(1);
                    yield return new WaitForSeconds(speed);
                    ParentGun.AddBullet(1);
                    if (cancelReload)
                    {
                        animator.CrossFade("EndReload", 0.32f, 0);
                        GunManager.HeadAnimation(0, AfterReloadAnim.length);
                        yield return new WaitForSeconds(AfterReloadAnim.length);
                        ParentGun.FinishReload();
                        cancelReload = false;
                        yield break;
                    }
                }
                animator.Play("EndReload", 0, 0);
                GunManager.HeadAnimation(0, AfterReloadAnim.length);
                ParentGun.PlayReloadAudio(2);
                yield return new WaitForSeconds(AfterReloadAnim.length);
                ParentGun.FinishReload();
            }
        }
    }
    /// 用来更好地配合
    /// 换弹音效与动画配合
    public void ReloadSound(int index)
    {
        if (m_source == null)
            return;

        switch (index)
        {
            case 0:
                m_source.clip = Reload_1;
                m_source.Play();
                break;
            case 1:
                m_source.clip = Reload_2;
                m_source.Play();
                GunManager.HeadAnimation(3, 1);
                break;
            case 2:
                if (Reload_3 != null)
                {
                    m_source.clip = Reload_3;
                    m_source.Play();
                }
                break;
        }
    }

    public void PlayHeadAnimation(string motionName)
    {
        var animator = GunManager.HeadAnimator;

        if (animator == null) return;

        if (!animator.IsPlaying())
        {
            animator.Play(motionName, 0, 0);
        }
        else
        {
            animator.CrossFade(motionName, 0.2f, 0);
        }
    }

    public void PlayThrow()
    {
        // 自 1.9.3 起注释掉，用于修复手雷投掷方向的问题
        // 该问题由动画位移引起。

    }

    public void PlayParticle(int id)
    {
        ParticleSystem.EmissionModule m = Particles[id].emission;
        m.rateOverTime = ParticleRate;
    }

    public void StopParticle(int id)
    {
        ParticleSystem.EmissionModule m = Particles[id].emission;
        m.rateOverTime = 0;
    }

    public void PlayHeadShake() => GunManager.HeadAnimator.Play("shake-small", 0, 0);

    public float GetFireLenght { get { return FireAimAnimation.length / FireSpeed; } }

    [Serializable]
    public enum AnimationType
    {
        Animation,
        Animator,
    }

    public enum FireBlendMethod
    {
        FireRate,
        FireSpeed,
        FireRateCrossFade,
        FireSpeedCrossFade
    }

    private Animator _Animator;
    private Animator animator
    {
        get
        {
            if (_Animator == null)
            {
                _Animator = this.GetComponent<Animator>();
            }
            return _Animator;
        }
    }

    private Animation _Anim;
    private Animation Anim
    {
        get
        {
            if (_Anim == null)
            {
                _Anim = this.GetComponent<Animation>();
            }
            return _Anim;
        }
    }
}
