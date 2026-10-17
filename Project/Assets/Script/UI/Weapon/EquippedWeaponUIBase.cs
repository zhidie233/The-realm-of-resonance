using UnityEngine;

public abstract class EquippedWeaponUIBase : MonoBehaviour
{

    /// <summary>
    /// 更新本地玩家界面中的弹药信息
    /// </summary>
    /// <param name="gun"></param>
    public abstract void SetAmmoOf(GunController gun);

    /// <summary>
    /// 更新本地玩家界面中的开火模式文本。
    /// </summary>
    /// <param name="fireType"></param>
    public abstract void SetFireType(GunBase.FireType fireType);

    private static EquippedWeaponUIBase _instance;
    public static EquippedWeaponUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<EquippedWeaponUIBase>(); }
            if (_instance == null) { _instance = UIReferences.Instance.GetComponentInChildren<EquippedWeaponUIBase>(true); }
            return _instance;
        }
    }
}
