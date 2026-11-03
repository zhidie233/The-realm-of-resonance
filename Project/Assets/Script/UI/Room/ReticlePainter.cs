using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
/// 默认准星实现
public class ReticlePainter : MonoBehaviourBase
{
    /// 为 true 时冻结并屏蔽对准星的所有操作/调用
    public bool Block
    {
        get;
        set;
    }

    private static ReticlePainter m_instance;
    public static ReticlePainter Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<ReticlePainter>();
            }
            return m_instance;
        }
    }

    [Header("Settings")]
    [GameToggle] public bool fadeOnAim = true;
    [Range(1, 10)] public float ScaleLerp = 5;
    [Range(0.1f, 5)] public float RotationSpeed = 2;
    [Range(0.01f, 1)] public float OnFireScaleRate = 0.1f;
    [Range(0.01f, 1)] public float OnCrouchReduce = 8;

    [Header("Hit Marker")]
    [Range(0.1f, 3)] public float Duration = 1;
    [Range(5, 50)] public float IncreaseAmount = 25;
    public Color HitMarkerColor = Color.white;
    public AnimationCurve hitScaleCurve;
    public AnimationCurve hitAlphaCurve;

    [Header("References")]
    [SerializeField] private ReticleInfo genericCrosshair = null;
    [SerializeField] private WeaponReticle[] weaponCrosshairs = null;

    [SerializeField] private RectTransform RootContent = null;
    [SerializeField] private RectTransform HitMarkerRoot = null;

    private Vector2 InitSizeDelta;
    private bool isAim = false;
    private Canvas m_Canvas;
    private Vector3 InitialRotation;
    private float lastTimeFire;
    private CanvasGroup m_HitAlpha;
    private Vector2 defaultHitSize;
    private float hitDuration;
    private CanvasGroup CrossAlpha;
    private bool m_allowFade = true;
    private PlayerState playerState = PlayerState.Idle;
    private Vector2 extraScale = Vector2.zero;

    protected override void Awake()
    {
        base.Awake();
        if (RootContent != null)
        {
            InitSizeDelta = RootContent.sizeDelta;
            InitialRotation = RootContent.eulerAngles;
            CrossAlpha = RootContent.GetComponent<CanvasGroup>();
        }
        if (HitMarkerRoot != null)
        {
            m_HitAlpha = HitMarkerRoot.GetComponent<CanvasGroup>();
            defaultHitSize = HitMarkerRoot.sizeDelta;
            if (m_HitAlpha != null) { m_HitAlpha.alpha = 0; }
            Graphic[] hmg = HitMarkerRoot.GetComponentsInChildren<Graphic>();
            foreach (Graphic g in hmg) { g.color = HitMarkerColor; }
        }
        m_Canvas = transform.root.GetComponent<Canvas>();
        if (m_Canvas == null) { m_Canvas = transform.root.GetComponentInChildren<Canvas>(); }
    }

    public override void OnUpdate()
    {
        if (GetCrosshair.isStatic)
            return;

        ScaleContent();
        FadeControl();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (m_HitAlpha != null)
        {
            m_HitAlpha.alpha = 0;
        }
        GameEvents.onLocalPlayerDeath += OnLocalDeath;
        GameEvents.onLocalPlayerStateChanged += OnLocalPlayerStateChange;
        RootContent.gameObject.SetActive(GameConfig.Instance.reticleEnabled);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onLocalPlayerDeath -= OnLocalDeath;
        GameEvents.onLocalPlayerStateChanged -= OnLocalPlayerStateChange;
    }

    void OnLocalDeath()
    {
        playerState = PlayerState.Idle;
        isAim = false;
    }

    void OnLocalPlayerStateChange(PlayerState from, PlayerState to)
    {
        playerState = to;
    }

    void ScaleContent()
    {
        if (RootContent == null)
            return;

        Vector2 target = (isAim) ? new Vector2(GetCrosshair.OnAimScaleAmount, GetCrosshair.OnAimScaleAmount) : (InitSizeDelta + extraScale);
        Vector2 size = (isCrouch) ? target - CrouchVector : target;
        RootContent.sizeDelta = Vector2.Lerp(RootContent.sizeDelta, size, Time.unscaledDeltaTime * ScaleLerp);
    }

    void FadeControl()
    {
        if (CrossAlpha == null)
            return;

        float at = (isAim && CanFade) ? 0 : 1;
        CrossAlpha.alpha = Mathf.Lerp(CrossAlpha.alpha, at, Time.deltaTime * 10);
    }

    public void SetupCrosshairForWeapon(FirearmController gun)
    {
        if (gun == null) return;

        extraScale = Vector2.one * gun.CrossHairScale;
        SetupCrosshairForWeapon(gun.WeaponType);
    }

    public void SetupCrosshairForWeapon(FirearmType gun)
    {
        for (int i = 0; i < weaponCrosshairs.Length; i++)
        {
            if (weaponCrosshairs[i] == null) continue;
            weaponCrosshairs[i].crosshair?.SetActive(false);
        }

        int crossId = weaponCrosshairs.ToList().FindIndex(x => x.gunType == gun);
        if (crossId == -1)
        {
            ActiveGenericCrosshair();
        }
        else
        {
            weaponCrosshairs[crossId].crosshair?.SetActive(true);
            m_currentCrosshair = weaponCrosshairs[crossId].crosshair;
        }
    }

    public void ActiveGenericCrosshair()
    {
        genericCrosshair.SetActive(true);
    }

    /// 显示/隐藏准星
    public void Show(bool show)
    {
        if (Block || !GameConfig.Instance.reticleEnabled)
            return;

        RootContent.gameObject.SetActive(show);
    }

    public void OnFire()
    {
        if (!Interactable)
            return;
        if (Time.time < lastTimeFire)
            return;

        Vector2 size = (isCrouch) ? new Vector2(GetCrosshair.OnFireScaleAmount, GetCrosshair.OnFireScaleAmount) - CrouchVector : new Vector2(GetCrosshair.OnFireScaleAmount, GetCrosshair.OnFireScaleAmount);
        RootContent.sizeDelta = size;
        lastTimeFire = Time.time + OnFireScaleRate;
    }

    public void OnAim(bool aim)
    {
        if (!Interactable)
            return;

        isAim = aim;
    }

    /// 本地玩家命中可伤害对象时调用
    public void OnHit()
    {
        if (HitMarkerRoot == null)
            return;

        StopCoroutine(nameof(OnHitMarker));
        StartCoroutine(nameof(OnHitMarker));
    }

    /// 允许准星淡入淡出
    public void AllowFade(bool allow)
    {
       m_allowFade = allow;
    }

    public void Reset()
    {
        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
        RootContent.eulerAngles = InitialRotation;
    }

    private bool CanFade
    {
        get => m_allowFade && fadeOnAim;
    }

    private bool Interactable => RootContent != null && !GetCrosshair.isStatic;

    private ReticleInfo m_currentCrosshair;
    public ReticleInfo GetCrosshair
    {
        get
        {
            if (m_currentCrosshair == null) m_currentCrosshair = genericCrosshair;
            return m_currentCrosshair;
        }
    }

    IEnumerator OnHitMarker()
    {
        hitDuration = 0;
        HitMarkerRoot.sizeDelta = defaultHitSize;
        Vector2 sizeTarget = new Vector2(IncreaseAmount, IncreaseAmount);

        while (hitDuration < 1)
        {
            hitDuration += Time.unscaledDeltaTime / Duration;
            HitMarkerRoot.sizeDelta = Vector2.Lerp(defaultHitSize, sizeTarget, hitScaleCurve.Evaluate(hitDuration));
            if (m_HitAlpha != null) { m_HitAlpha.alpha = hitAlphaCurve.Evaluate(hitDuration); }
            yield return null;
        }
    }

    private Vector2 CrouchVector
    {
        get
        {
            return isCrouch ? Vector2.one * OnCrouchReduce : Vector2.zero;
        }
    }

    private bool isCrouch => playerState == PlayerState.Crouching;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (RootContent == null)
            return;

        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
    }
#endif

    [System.Serializable]
    public class WeaponReticle
    {
        public FirearmType gunType;
        public ReticleInfo crosshair;
    }
}
