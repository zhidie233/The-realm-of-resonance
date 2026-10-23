using UnityEngine;

public abstract class HeldWeaponUIBase : MonoBehaviour
{
    /// 更新本地玩家界面中的弹药信息
    public abstract void SetAmmoOf(FirearmController gun);
    /// 更新本地玩家界面中的开火模式文本。
    public abstract void SetFireType(FirearmBase.FireType fireType);

    private static HeldWeaponUIBase _instance;
    public static HeldWeaponUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<HeldWeaponUIBase>(); }
            if (_instance == null) { _instance = UIReferences.Instance.GetComponentInChildren<HeldWeaponUIBase>(true); }
            return _instance;
        }
    }
}
