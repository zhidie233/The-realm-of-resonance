using GameEditor;
using System.Collections;
using UnityEngine;

public class CameraMotionRig : CameraMotionRigBase
{
    [ScriptableDrawer] public CameraMotionRigConfig motionSettings;

    #region Private members
    private bool wiggleMotion = true;
    private bool breathingMotion = false;
    private float m_breatingAmplitude;
    private float m_startBreathingTime;
    private Vector3 breathRotation = Vector3.zero;
    private float sideTilt = 0;
    private Quaternion rotationMotion, extraRotation;
    private bool isSliding = false;
    private PlayerReferencesHandler playerRefs;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        playerRefs = GetComponentInParent<PlayerReferencesHandler>();
        m_breatingAmplitude = motionSettings.breatheAmplitude;
        rotationMotion = CachedTransform.localRotation;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.onPlayerLand += OnSmallImpact;
        wiggleMotion = GameConfig.Instance.playerCameraWiggle;
        GameEvents.onLocalPlayerStateChanged += OnPlayerStateChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onPlayerLand -= OnSmallImpact;
        GameEvents.onLocalPlayerStateChanged -= OnPlayerStateChanged;
    }

    public override void OnUpdate()
    {
        if (!LobbyMenu.Instance.isCursorLocked) return;

        Breathe();

        Wiggle();

        CachedTransform.localRotation = rotationMotion * extraRotation;
    }

    void Wiggle()
    {
        if (!wiggleMotion) return;

        float rollAngle = -GameInput.MouseX * motionSettings.tiltAngle;
        if (isSliding) rollAngle = 0;
        rollAngle = Mathf.Clamp(rollAngle, -motionSettings.tiltAngle, motionSettings.tiltAngle);

        if (motionSettings.sideMoveEffector > 0)
        {
            sideTilt = Mathf.Lerp(sideTilt, -GameInput.Horizontal * motionSettings.sideMoveEffector, Time.deltaTime * (motionSettings.smooth * 2));
            rollAngle += sideTilt;
        }

        breathRotation.z = GameInput.Aim() ? rollAngle * motionSettings.aimMultiplier : rollAngle;
        rotationMotion = Quaternion.Slerp(rotationMotion, Quaternion.Euler(breathRotation), Time.smoothDeltaTime * motionSettings.smooth);
    }

    void Breathe()
    {
        if (!breathingMotion) return;

        float theta = (Time.time - m_startBreathingTime) / motionSettings.breathePeriod;
        float vertical = m_breatingAmplitude * Mathf.Sin(theta);
        float horizontal = m_breatingAmplitude * Mathf.Cos(theta * 0.5f);
        breathRotation = (Vector3.up * vertical) + (Vector3.right * horizontal);
    }

    public override void AddCameraRotation(Quaternion rotation)
    {
        extraRotation = rotation;
    }

    public override void SetActiveBreathing(bool active, float amplitude = 0)
    {
        breathingMotion = active;

        if (!breathingMotion)
        {
            m_breatingAmplitude = motionSettings.breatheAmplitude;
            breathRotation = Vector3.zero;
        }
        else
        {
            m_breatingAmplitude = amplitude;
            m_startBreathingTime = Time.time;
        }
    }

    void OnSmallImpact(float impactAmount)
    {
        StartCoroutine(FallEffect());
    }

    IEnumerator FallEffect()
    {
        Vector3 target = Vector3.zero;
        Quaternion rot;
        float d = 0;

        while (d <= 1)
        {
            d += Time.deltaTime / motionSettings.downMotionDuration;

            target.x = (motionSettings.downPitchMovement.Evaluate(d) * motionSettings.DownAmount);
            rot = Quaternion.Euler(target);
            CachedTransform.localRotation = rotationMotion * extraRotation * rot;
            yield return null;
        }
    }

    void OnPlayerStateChanged(PlayerState from, PlayerState to)
    {
        isSliding = to == PlayerState.Sliding;
    }
}