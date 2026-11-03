using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// 默认武器行走摇晃运动
/// 同时控制头部摇晃。
public class SwayStride : MonoBehaviourBase
{
    public float Intensity { get; set; } = 1;

    public SwaySettings settings;

    Vector3 midpoint;
    Vector3 localRotation;
    float timer = 0.0f;
    float lerp = 2;
    float bobbingSpeed;
    FirstPersonController motor;
    float BobbingAmount;
    float tempWalkSpeed = 0;
    float tempRunSpeed = 0;
    float tempIdleSpeed = 0;
    float waveslice = 0.0f;
    float waveslice2 = 0.0f;
    float eulerZ = 0;
    float eulerX = 0;
    private bool rightFoot = false;
    Vector3 currentPosition, currentRotation = Vector3.zero;
    Vector3 bobEuler = Vector3.zero;
    private Action<PlayerState> AnimateCallback = null;
    private PlayerRig playerReferences;
    private bool useAnimation = false;
    private Quaternion bobRotation = Quaternion.identity;
    private float rotationIntensity = 1;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    public void Init()
    {
        playerReferences = transform.GetComponentInParent<PlayerRig>();
        motor = playerReferences.firstPersonController;
        midpoint = transform.localPosition;
        localRotation = transform.localEulerAngles;
        Intensity = 1;
        rotationIntensity = 1;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.onLocalAimChanged += OnLocalAimChange;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onLocalAimChanged -= OnLocalAimChange;
    }

    public override void OnUpdate()
    {
        if (motor == null) return;

        if (useAnimation)
        {
            if (AnimateCallback == null) return;

            AnimateCallback.Invoke(motor.State);
            MoveToDefault();
            CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, midpoint, Time.deltaTime * lerp);
        }
        StateControl();
    }

    public override void OnFixedUpdate()
    {
        Movement();
    }

    /// 停止行走摇晃运动
    public void Stop()
    {
        bobbingSpeed = tempIdleSpeed;
        BobbingAmount = settings.WalkOscillationAmount * 0.1f;
        lerp = settings.WalkLerpSpeed;
        eulerZ = settings.EulerZAmount;
        eulerX = settings.EulerXAmount;
        MoveToDefault();
    }

    void StateControl()
    {
        if (motor.State == PlayerState.Jumping) return;
        if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Running)
        {
            bobbingSpeed = tempWalkSpeed;
            BobbingAmount = settings.WalkOscillationAmount;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }
        else if (motor.State == PlayerState.Running)
        {
            bobbingSpeed = tempRunSpeed;
            BobbingAmount = settings.RunOscillationAmount;
            lerp = settings.RunLerpSpeed;
            eulerZ = settings.RunEulerZAmount;
            eulerX = settings.RunEulerXAmount;
        }

        if (motor.State != PlayerState.Running && motor.VelocityMagnitude < 0.1f || !LobbyMenu.Instance.isCursorLocked)
        {
            bobbingSpeed = tempIdleSpeed;
            BobbingAmount = settings.WalkOscillationAmount * 0.1f;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }
    }

    void Movement()
    {
        tempWalkSpeed = 0;
        tempRunSpeed = 0;
        tempIdleSpeed = 0;

        if (tempIdleSpeed != settings.idleBobbingSpeed)
        {
            tempWalkSpeed = motor.GetCurrentSpeed() * 0.06f * settings.WalkSpeedMultiplier;
            tempRunSpeed = motor.GetCurrentSpeed() * 0.03f * settings.RunSpeedMultiplier;
            tempIdleSpeed = settings.idleBobbingSpeed;
        }

        waveslice = Mathf.Sin(timer * 2);
        waveslice2 = Mathf.Sin(timer);
        timer = timer + bobbingSpeed;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }

        float w1v = (waveslice + 1) / 2;
        w1v = settings.pitchCurve.Evaluate(w1v);
        waveslice = Mathf.Lerp(-1, 1, w1v);

        float w2v = (waveslice2 + 1) / 2;
        w2v = settings.rollCurve.Evaluate(w2v);
        waveslice2 = Mathf.Lerp(-1, 1, w2v);

        ApplyMovement();
        UpdateFootStep();
    }

    void ApplyMovement()
    {
        if (useAnimation) return;
        float time = Time.smoothDeltaTime;
        if (waveslice != 0)
        {
            float verticalMovement = waveslice * BobbingAmount * Intensity;
            if (!settings.pitchTowardUp) verticalMovement = -verticalMovement;
            float horizontalMovement = waveslice2 * BobbingAmount * Intensity;
            float zFlow = waveslice2 * eulerZ * rotationIntensity;
            float xFlow = waveslice * eulerX * rotationIntensity;

            if (motor.isGrounded)
            {
                //若玩家正在移动
                if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Idle)
                {
                    currentPosition.Set(midpoint.x + horizontalMovement, midpoint.y + verticalMovement, midpoint.z);
                    currentRotation.Set(localRotation.x + xFlow, localRotation.y, localRotation.z + zFlow);
                    bobEuler.Set((xFlow * 0.33f) * motor.GetHeadBobMagnitudes(true), 0, zFlow * motor.GetHeadBobMagnitudes(false));
                    CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(currentRotation), time * lerp);
                    bobRotation = Quaternion.Slerp(bobRotation, Quaternion.Euler(bobEuler), time * lerp);
                }
                else
                {
                    currentPosition.Set(midpoint.x, midpoint.y + verticalMovement, midpoint.z);
                    CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(Vector3.zero), time * 10);
                    bobRotation = Quaternion.Slerp(bobRotation, Quaternion.Euler(Vector3.zero), time * lerp);
                }
            }
            else
            {
                //玩家未移动
                MoveToDefault();
            }
        }
        else
        {
            //玩家未移动
            MoveToDefault();
        }
        CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, currentPosition, time * lerp);
        playerReferences.cameraMotion.AddCameraRotation(bobRotation);
    }
    void MoveToDefault()
    {
        currentPosition = midpoint;
        CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(Vector3.zero), Time.smoothDeltaTime * 12);
        bobRotation = Quaternion.Slerp(bobRotation, Quaternion.Euler(Vector3.zero), Time.smoothDeltaTime * lerp);
    }
    void UpdateFootStep()
    {
        if (!motor.isGrounded) return;
        if (motor.VelocityMagnitude > 0.1f)
        {
            if (waveslice2 >= 0.97f && !rightFoot)
            {
                motor.PlayFootStepSound();
                rightFoot = true;
            }
            else if (waveslice2 <= (-0.97f) && rightFoot)
            {
                motor.PlayFootStepSound();
                rightFoot = false;
            }
        }
    }
    void OnLocalAimChange(bool aiming)
    {
        Intensity = aiming ? settings.AimIntensity : 1;
        rotationIntensity = aiming ? settings.aimRotationIntensity : 1;
    }
    /// 在别处更新行走动画
    /// useAnimation == false -> 由武器摇晃脚本控制运动
    /// else -> 由传入的 'callback' 委托控制，不更新武器摇晃脚本。
    public void AnimatedThis(Action<PlayerState> callback, bool useAnim)
    {
        AnimateCallback = callback;
        useAnimation = useAnim;

    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SwayStride))]
public class SwayStridePreview : Editor
{
    SwayStride script;
    private void OnEnable()
    {
        script = (SwayStride)target;
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.Space(5);
        SerializedProperty so = serializedObject.FindProperty("settings");
        EditorGUILayout.PropertyField(so);
        if (so != null && so.objectReferenceValue != null)
        {
            var editor = Editor.CreateEditor(so.objectReferenceValue);
            if (editor != null)
            {
                EditorGUILayout.BeginVertical("box");
                editor.DrawDefaultInspector();
                EditorGUILayout.EndVertical();
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
