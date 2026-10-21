using UnityEngine;
using System.Collections;

public class DeathCam : DeathCamBase
{

    #region Public members
    public Transform target = null;
    public float distance = 10.0f;
    public float distanceMax = 15f;
    public float distanceMin = 0.5f;
    public float xSpeed = 120f;
    public float yMaxLimit = 80f;
    public float yMinLimit = -20f;
    public float ySpeed = 120f;
    public LayerMask layers; 
    #endregion

    #region Private members
    float x = 0;
    float y = 0;
    private int CurrentTarget = 0;
    private GameController Manager;
    private bool canManipulate = false;
    private DeathCamType cameraType = DeathCamType.WatchBody;
    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.onLocalPlayerSpawn += OnLocalSpawn;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onLocalPlayerSpawn -= OnLocalSpawn;
    }

    void Start()
    {
        transform.parent = null;
        Manager = GameController.Instance;
        cameraType = GameConfig.Instance.deathReplayMode;
        if (target != null)
        {
            transform.LookAt(target);
            StartCoroutine(ZoomOut());
        }
    }

    public override void OnUpdate()
    {
        if (target != null)
        {
            Orbit();
        }
    }

    void OnLocalSpawn()
    {
        target = null;
        canManipulate = false;
        DeathCamUIBase.Instance?.Hide();
        SetActive(false);
    }

    public void ChangeTarget(bool next)
    {
        if (Manager.OthersActorsInScene.Count <= 0)
            return;

        if (next) { CurrentTarget = (CurrentTarget + 1) % Manager.OthersActorsInScene.Count; }
        else
        {
            if (CurrentTarget > 0) { CurrentTarget--; } else { CurrentTarget = Manager.OthersActorsInScene.Count - 1; }
        }
        target = Manager.OthersActorsInScene[CurrentTarget].Actor;
    }

    /// <summary>
    /// 更新相机运动
    /// </summary>
    void Orbit()
    {
        if (!canManipulate || cameraType != DeathCamType.OrbitTarget)
            return;

        float targetDistance = distance;
        var ray = new Ray(target.position, CachedTransform.position - target.position);
        if (Physics.SphereCast(ray, 0.2f, out var hit, distance, layers))
        {
            targetDistance = MathHelpers.Distance(target.position, hit.point) - 0.21f;
        }

        x += ((GameInput.MouseX * this.xSpeed) * targetDistance) * 0.02f;
        y -= (GameInput.MouseY * this.ySpeed) * 0.02f;
        y = MathHelpers.ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
        Quaternion quaternion = Quaternion.Euler(this.y, this.x, 0f);

        Vector3 vector = new Vector3(0f, 0f, -targetDistance);
        Vector3 vector2 = target.position;
        vector2.y = target.position.y + 1f;
        Vector3 vector3 = (quaternion * vector) + vector2;
        transform.rotation = quaternion;
        transform.position = vector3;
    }

    /// <summary>
    /// 设置击杀回放聚焦/观察的目标
    /// </summary>
    /// <param name="targetName"></param>
    public override DeathCamBase SetTarget(DeathCamInfo info)
    {
        //如果玩家传入了目标
        if(info.Target != null && (GameConfig.Instance.deathReplayMode == DeathCamType.WatchBody || string.IsNullOrEmpty(info.TargetName)))
        {
            target = info.Target;
            ReadyToShow(info);
            return this;
        }

        //如果玩家只传了目标的名字
        var targetName = info.TargetName;
        if (string.IsNullOrEmpty(targetName)) return this;

        //尝试按名字查找
        var targetInstance = GameObject.Find(targetName);

        if (targetName == LocalName)
        {
            if (info.FallbackTarget == null) return this;

            targetInstance = info.FallbackTarget.gameObject;
        }

        if (targetInstance == null)
        {
            var playerActor = GameController.Instance.FindActor(targetName);
            if (playerActor != null && playerActor.Actor != null)
            {
                targetInstance = playerActor.Actor.gameObject;
            }
        }

        if(targetInstance == null && GameConfig.Instance.deathReplayMode == DeathCamType.OrbitTarget && info.FallbackTarget)
        {
            targetInstance = info.FallbackTarget.gameObject;
        }

        if (targetInstance == null)
        {
            Debug.LogWarning($"Couldn't found the kill cam target '{targetName}'.");
        }
        else
        {
            target = targetInstance.transform;
            ReadyToShow(info);
        }
        return this;
    }

    private void ReadyToShow(DeathCamInfo info)
    {
        canManipulate = true;
        DeathCamUIBase.Instance?.Show(info);
        if(GameConfig.Instance.deathReplayMode == DeathCamType.WatchBody && target != null)
        {
            PositionedAndLookAt(target);
        }
    }

    public override void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (!active) DeathCamUIBase.Instance?.Hide();
    }

    public void SpectPlayer(Transform player)
    {
        target = player;
        canManipulate = true;
    }

    public void PositionedAndLookAt(Transform reference)
    {
        float distanceFromLocal = 2.5f;
        Vector3 position = reference.position + (Vector3.up * 1.5f);
        CachedTransform.position = position - (reference.forward * distanceFromLocal);
        RaycastHit rayHit;
        if (Physics.Raycast(reference.position, -reference.forward, out rayHit, distanceFromLocal, layers, QueryTriggerInteraction.Ignore))
        {
            CachedTransform.position = Vector3.Lerp(rayHit.point, reference.position, 0.05f);
        }

        var up = CachedTransform.position + (Vector3.up * distanceFromLocal);
        if (Physics.Raycast(up, Vector3.down, out rayHit, distanceFromLocal, layers, QueryTriggerInteraction.Ignore))
        {
            CachedTransform.position = Vector3.Lerp(up, rayHit.point, 0.4f);
        }

        CachedTransform.LookAt(position + Vector3.up * 0.25f);
    }

    IEnumerator ZoomOut()
    {
        float d = 0;
        Vector3 next = target.position + transform.TransformDirection(new Vector3(0, 0, -3));
        Vector3 origin = target.position;
        transform.position = target.position;
        while (d < 1)
        {
            d += Time.deltaTime * 1.25f;
            transform.position = Vector3.Lerp(origin, next, d);
            transform.LookAt(target);
            yield return null;
        }
        x = CachedTransform.eulerAngles.y;
        y = CachedTransform.eulerAngles.x;
    }

    public enum DeathCamType
    {
        OrbitTarget,
        WatchBody,
    }
}