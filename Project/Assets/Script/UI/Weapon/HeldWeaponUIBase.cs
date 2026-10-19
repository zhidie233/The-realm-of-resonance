using UnityEngine;

public abstract class HeldWeaponUIBase : MonoBehaviour
{

    /// <summary>
    /// 更新本地玩家界面中的弹药信息
    /// </summary>
    /// <param name="gun"></param>
    public abstract void SetAmmoOf(FirearmController gun);

    /// <summary>
    /// 更新本地玩家界面中的开火模式文本。
    /// </summary>
    /// <param name="fireType"></param>
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
