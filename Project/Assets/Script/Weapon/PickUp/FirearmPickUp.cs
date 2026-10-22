using System.Collections;
using UnityEngine;
/// 默认武器拾取脚本
/// 继承 FirearmPickUpBase 即可用自己的脚本
public class FirearmPickUp : FirearmPickUpBase, IRayDetectable
{
    #region Public members
    public DetectMode m_DetectMode = DetectMode.Raycast;
    [HideInInspector]
    public bool PickupOnCollide = true;
    public float DestroyIn = 15f;
    public GameEvents.UEvent onPickUp;
    #endregion

    #region Private members
    private bool isFocus = false;
    private PlayerRig localPlayerIn = null;
    private bool localInsideTrigger = false;
    private byte uniqueLocal = 0;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        if (!NetworkWrapper.IsConnected) return;

        base.Awake();
        uniqueLocal = (byte)UnityEngine.Random.Range(0, 9998);
    }

    IEnumerator Start()
    {
        if (AutoDestroy)
        {
            DestroyAfter(DestroyIn);
        }

        if (m_DetectMode == DetectMode.Trigger)
        {
            PickupOnCollide = false;
            // 再次检测投掷出去的武器前的延迟。
            yield return new WaitForSeconds(2f);
            PickupOnCollide = true;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!localInsideTrigger) return;

        PickUpUIBase.Instance?.Hide();
        if (localPlayerIn != null)
        {
            if (m_DetectMode == DetectMode.Raycast)
            {
                if (localPlayerIn.cameraRay != null)
                {
                    localPlayerIn.cameraRay.SetActiver(false, uniqueLocal);
                }
            }
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if (!PickupOnCollide || GameController.Instance.GameMatchState == MatchState.Waiting)
            return;
        if (!GetGameMode.GetGameModeInfo().allowWeaponPickup) return;
        // 若不是本地玩家，直接忽略。
        if (!c.isLocalPlayerCollider()) return;

        var playerReferences = c.GetComponent<PlayerRig>();
        if (playerReferences != null)
        {
            localInsideTrigger = true;
            FirearmPickUpManagerBase.Instance.LastTrigger = this;
            localPlayerIn = playerReferences;

            if (m_DetectMode == DetectMode.Raycast)
            {
                if (playerReferences.cameraRay != null)
                {
                    playerReferences.cameraRay.SetActiver(true, uniqueLocal);
                }
            }
            else if (m_DetectMode == DetectMode.Trigger)
            {
                PickUpUIBase.Instance?.OnOverWeapon(this);
            }
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.isLocalPlayerCollider() && localInsideTrigger)
        {
            localInsideTrigger = false;
            PickUpUIBase.Instance?.Hide();
            if (localPlayerIn != null)
            {
                if (m_DetectMode == DetectMode.Raycast)
                {
                    if (localPlayerIn.cameraRay != null)
                    {
                        localPlayerIn.cameraRay.SetActiver(false, uniqueLocal);
                    }
                }
                localPlayerIn = null;
            }
        }
    } 
    #endregion

    public override void OnUpdate()
    {
        if (!localInsideTrigger) return;

        if (m_DetectMode == DetectMode.Trigger)
        {
            if (GameInput.Interact() && FirearmPickUpManagerBase.Instance.LastTrigger == this)
            {
                PickUp();
            }
        }
        else if (m_DetectMode == DetectMode.Raycast)
        {
            if (!localInsideTrigger || !isFocus) return;

            if (GameInput.Interact())
            {
                PickUp();
            }
        }
    }
    /// 拾取武器
    public override void PickUp()
    {
        if (!GetGameMode.GetGameModeInfo().allowWeaponPickup) return;

        FirearmPickUpManagerBase.Instance?.SendPickUp(new FirearmPickUpManagerBase.PickUpData()
        {
            Identifier = gameObject.name,
            WeaponID = WeaponID,
            Ammunition = Ammunition
        });

        PickUpUIBase.Instance?.Hide();
        onPickUp?.Invoke();
    }
    /// 玩家看向武器时调用
    public void OnRayDetectedByPlayer()
    {
        isFocus = true;
        PickUpUIBase.Instance?.OnOverWeapon(this);
    }
    /// 玩家不再看向武器时调用
    public void OnUnDetectedByPlayer()
    {
        isFocus = false;
        PickUpUIBase.Instance?.Hide();
    }

    public override void DestroyAfter(float delay)
    {
        this.InvokeAfter(delay, () =>
        {
            Destroy(gameObject);
        });
    }

#if UNITY_EDITOR
    private SphereCollider SpheCollider;
    static Color _gizmoColor = new Color(0, 1, 0, 0.5f);
    private void OnDrawGizmos()
    {
        if (SpheCollider != null)
        {
            Gizmos.color = _gizmoColor;
            CommonUtil.DrawWireArc(SpheCollider.bounds.center, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
        }
        else
        {
            SpheCollider = GetComponent<SphereCollider>();
        }
    }
#endif

    [System.Serializable]
    public enum DetectMode
    {
        Raycast,
        Trigger,
    }
}
