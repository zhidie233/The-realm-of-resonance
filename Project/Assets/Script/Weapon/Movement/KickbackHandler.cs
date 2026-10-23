using UnityEngine;
/// Game 默认后坐力运动
/// 想用自己的后坐力脚本，就让脚本继承 KickbackBase.cs
/// 此脚本仅供参考。
public class KickbackHandler : KickbackBase
{
    [Range(1, 25)] public float MaxRecoil = 5;
    public bool AutomaticallyComeBack = true;

    #region Private members
    private Transform m_Transform;
    private Vector3 RecoilRot;
    private float Recoil = 0;
    private float RecoilSpeed = 2;
    private bool wasFiring = false;
    private float lerpRecoil = 0;  
    #endregion

    private void Start()
    {
        GameObject g = new GameObject("Recoil");
        m_Transform = g.transform;
        m_Transform.parent = transform.parent;
        m_Transform.localPosition = Vector3.zero;
        m_Transform.localEulerAngles = Vector3.zero;
        transform.parent = m_Transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        RecoilRot = m_Transform.localEulerAngles;
    }

    public override void OnUpdate()
    {
        RecoilControl();
    }

    void RecoilControl()
    {
        if (GunManager == null)
            return;

        if (GunManager.CurrentGun != null)
        {
            if (GunManager.CurrentGun.isFiring)
            {
                if (AutomaticallyComeBack)
                {
                    Quaternion q = Quaternion.Euler(new Vector3(-Recoil, 0, 0));
                    m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, q, Time.deltaTime * RecoilSpeed);
                }
                else
                {
                    lerpRecoil = Mathf.Lerp(lerpRecoil, Recoil, Time.deltaTime * RecoilSpeed);
                    fpController.GetMouseLook().SetVerticalOffset(-lerpRecoil);
                }
                wasFiring = true;
            }
            else
            {
                if (AutomaticallyComeBack)
                {
                    BackToOrigin();
                }
                else
                {
                    if (wasFiring)
                    {
                        Recoil = 0;
                        lerpRecoil = 0;
                        fpController.GetMouseLook().CombineVerticalOffset();
                        wasFiring = false;
                    }
                }
            }
        }
        else
        {
            BackToOrigin();
        }
    }

    void BackToOrigin()
    {
        if (m_Transform == null) return;

        Quaternion q = Quaternion.Euler(RecoilRot);
        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, q, Time.deltaTime * RecoilSpeed);
        Recoil = m_Transform.localEulerAngles.x;
    }
    /// 添加后坐力
    public override void SetRecoil(KickbackData data)
    {

        Recoil += data.Amount;
        Recoil = Mathf.Clamp(Recoil, 0, MaxRecoil);
        RecoilSpeed = data.Speed;
    }

    private FirearmInventory GunManager => PlayerReferences.gunManager;
    private FirstPersonControllerBase fpController => PlayerReferences.firstPersonController;

    private PlayerRig playerReferences;
    public PlayerRig PlayerReferences
    {
        get
        {
            if (playerReferences == null) playerReferences = transform.root.GetComponent<PlayerRig>();
            return playerReferences;
        }
    }
}
