using System.Collections;
using UnityEngine;
using System;
/// 默认武器拾取脚本
public class FirearmPickUp : MonoBehaviourBase, IRayDetectable
{
    /// 弹药信息
    [Serializable]
    public class AmmoInformation
    {
        public int Bullets = 0;
        public int Clips = 0;

        public int GetBullets
        {
            get
            {
                int b = Bullets;
                if (GameConfig.Instance.AmmoType == AmmunitionType.Bullets)
                {
                    b = Bullets * Clips;
                }
                return b;
            }
        }
    }
    /// 武器标识
    [FirearmID] public int WeaponID = 0;
    /// 这把掉落武器的弹药数据
    public AmmoInformation Ammunition;
    /// 这把武器将被自动销毁
    /// 是否在特定时间后？
    public bool AutoDestroy = true;

    public FirearmData GunInfo
    {
        get
        {
            if (_gunInfo == null)
            {
                _gunInfo = GameConfig.Instance.GetWeapon(WeaponID);
            }
            return _gunInfo;
        }
        private set => _gunInfo = value;
    }
    private FirearmData _gunInfo;

    public DetectMode m_DetectMode = DetectMode.Raycast;
    [HideInInspector]
    public bool PickupOnCollide = true;
    public float DestroyIn = 15f;
    public GameEvents.UEvent onPickUp;

    private bool isFocus = false;
    private PlayerRig localPlayerIn = null;
    private bool localInsideTrigger = false;
    private byte uniqueLocal = 0;

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

        PickUpUI.Instance?.Hide();
        if (localPlayerIn != null)
        {
            if (m_DetectMode == DetectMode.Raycast)
            {
                if (localPlayerIn.cameraRay != null)
                {
                    localPlayerIn.cameraRay.SetActivator(false, uniqueLocal);
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
            FirearmPickUpManager.Instance.LastTrigger = this;
            localPlayerIn = playerReferences;

            if (m_DetectMode == DetectMode.Raycast)
            {
                if (playerReferences.cameraRay != null)
                {
                    playerReferences.cameraRay.SetActivator(true, uniqueLocal);
                }
            }
            else if (m_DetectMode == DetectMode.Trigger)
            {
                PickUpUI.Instance?.OnOverWeapon(this);
            }
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.isLocalPlayerCollider() && localInsideTrigger)
        {
            localInsideTrigger = false;
            PickUpUI.Instance?.Hide();
            if (localPlayerIn != null)
            {
                if (m_DetectMode == DetectMode.Raycast)
                {
                    if (localPlayerIn.cameraRay != null)
                    {
                        localPlayerIn.cameraRay.SetActivator(false, uniqueLocal);
                    }
                }
                localPlayerIn = null;
            }
        }
    } 

    public override void OnUpdate()
    {
        if (!localInsideTrigger) return;

        if (m_DetectMode == DetectMode.Trigger)
        {
            if (GameInput.Interact() && FirearmPickUpManager.Instance.LastTrigger == this)
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
    public void PickUp()
    {
        if (!GetGameMode.GetGameModeInfo().allowWeaponPickup) return;

        FirearmPickUpManager.Instance?.SendPickUp(new FirearmPickUpManager.PickUpData()
        {
            Identifier = gameObject.name,
            WeaponID = WeaponID,
            Ammunition = Ammunition
        });

        PickUpUI.Instance?.Hide();
        onPickUp?.Invoke();
    }
    /// 玩家看向武器时调用
    public void OnRayDetectedByPlayer()
    {
        isFocus = true;
        PickUpUI.Instance?.OnOverWeapon(this);
    }
    /// 玩家不再看向武器时调用
    public void OnUnDetectedByPlayer()
    {
        isFocus = false;
        PickUpUI.Instance?.Hide();
    }

    /// 延迟后销毁该武器实例
    public void DestroyAfter(float delay)
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
