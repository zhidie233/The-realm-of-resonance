using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class WeaponLoadoutUIBase : MonoBehaviour
{

    [Serializable]
    public enum VisibilityMode
    {
        AutoHide,
        AlwaysShow,
    }

    [Flags]
    public enum LoadoutFlags : byte
    {
        None = 0,
        Animated = 1
    }

    /// <summary>
    /// Show the given weapons in the slots UI
    /// </summary>
    /// <param name="weapons"></param>
    public abstract void SetLoadout(List<GunController> weapons);

    /// <summary>
    /// Mark the given slot as the current equipped weapon
    /// </summary>
    /// <param name="slotID">Slot to be marked as equipped.</param>
    public abstract void SetEquippedSlot(int slotID);

    /// <summary>
    /// Replace the given slot UI with the given weapon info.
    /// </summary>
    /// <param name="slot">Slot to be replace.</param>
    /// <param name="newGun">Weapon to fetch the information from.</param>
    public abstract void ReplaceSlot(int slotID, GunController newWeapon);

    public abstract void SetVisibility(VisibilityMode visibility);

    /// <summary>
    /// Empty the weapon slots UI
    /// Since the slots are not auto updated if a weapon is unequipped, this have to be called manually.
    /// </summary>
    public abstract void ClearSlots(LoadoutFlags flags = LoadoutFlags.None);

    public abstract VisibilityMode GetVisibilityMode();

    public abstract void SetActive(bool active);

    private static WeaponLoadoutUIBase _instance;
    public static WeaponLoadoutUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<WeaponLoadoutUIBase>(); }
            if(_instance == null) { _instance = UIReferences.Instance.GetComponentInChildren<WeaponLoadoutUIBase>(true); }
            return _instance;
        }
    }
}
