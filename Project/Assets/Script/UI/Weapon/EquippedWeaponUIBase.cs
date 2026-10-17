using UnityEngine;

public abstract class EquippedWeaponUIBase : MonoBehaviour
{

    /// <summary>
    /// Update the ammo information in the local player interface
    /// </summary>
    /// <param name="gun"></param>
    public abstract void SetAmmoOf(GunController gun);

    /// <summary>
    /// Update the fire type text for the local player interface.
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
