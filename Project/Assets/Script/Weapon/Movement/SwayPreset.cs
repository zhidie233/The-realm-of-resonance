using UnityEngine;

/// <summary>
/// 玩家冲刺时控制第一人称武器的旋转
/// 此类没有直接引用，可以换成自己的脚本。
/// </summary>
public class SwayPreset : MonoBehaviourBase
{

    #region Public members
    [Header("Weapon On Run Position")]
    [Tooltip("Weapon Position and Position On Run")]
    public Vector3 moveTo;
    [Tooltip("Weapon Rotation and Position On Run")]
    public Vector3 rotateTo;
    [Space(5)]
    [Header("Weapon On Run and Reload Position")]
    [Tooltip("Weapon Position and Position On Run and Reload")]
    public Vector3 moveToReload;
    [Tooltip("Weapon Rotation and Position On Run and Reload")]
    public Vector3 rotateToReload;
    [Space(5)]
    public float accelerationMultiplier = 2.5f;
    public float InSpeed = 15;
    public float OutSpeed = 12;
    public float rotationSpeedMultiplier = 1;
    public AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [HideInInspector] public float _previewWeight = 0;
    #endregion

    #region Private members
    private float vel;
    private Quaternion DefaultRot;
    private Quaternion sprintRot, sprintReloadRot;
    private Vector3 DefaultPos;
    private FirearmController Gun;
    private FirstPersonControllerBase controller;
    private float acceleration = 1;
    private bool defaultState = true;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        DefaultRot = CachedTransform.localRotation;
        DefaultPos = CachedTransform.localPosition;
        Gun = transform.parent.GetComponent<FirearmController>();
        controller = Gun.PlayerReferences.firstPersonController;
        sprintRot = Quaternion.Euler(rotateTo);
        sprintReloadRot = Quaternion.Euler(rotateToReload);
    }

    public override void OnLateUpdate()
    {
        if (controller == null)
            return;

        vel = controller.VelocityMagnitude;
        RotateControl();
    }

    void RotateControl()
    {
        float delta = Time.smoothDeltaTime;
        acceleration = Mathf.Lerp(acceleration, 1, delta * accelerationMultiplier);
        float acc = accelerationCurve.Evaluate(acceleration);

        if ((vel > 1f && controller.isGrounded) && controller.State == PlayerState.Running && !Gun.isFiring && !Gun.isAiming)
        {
            if (defaultState)
            {
                acceleration = 0f;
                acc = 0;
                defaultState = false;
            }

            if (Gun.isReloading)
            {
                CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, sprintReloadRot, (delta * (InSpeed * rotationSpeedMultiplier)) * acc);
                CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, moveToReload, delta * InSpeed * acc);
            }
            else
            {
                CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, sprintRot, (delta * (InSpeed * rotationSpeedMultiplier)) * acc);
                CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, moveTo, (delta * InSpeed) * acc);
            }
        }
        else
        {
            defaultState = true;

            CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, DefaultRot, delta * (OutSpeed * rotationSpeedMultiplier));
            CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, DefaultPos, delta * OutSpeed);
        }
    }
}