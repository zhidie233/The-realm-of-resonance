using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class EquipmentScreenBase : MonoBehaviour
{

    [Serializable]
    public enum VisibilityMode
    {
        AutoHide,
        AlwaysShow,
    }

    [Flags]
    public enum EquipmentFlags : byte
    {
        None = 0,
        Animated = 1
    }

    /// <summary>
    /// 在槽位 UI 中显示给定的武器
    /// </summary>
    /// <param name="weapons"></param>
    public abstract void SetEquipment(List<FirearmController> weapons);

    /// <summary>
    /// 把给定槽位标记为当前装备武器
    /// </summary>
    /// <param name="slotID">要标记为已装备的槽位。</param>
    public abstract void SetEquippedSlot(int slotID);

    /// <summary>
    /// 用给定武器信息替换指定槽位的 UI。
    /// </summary>
    /// <param name="slot">要替换的槽位。</param>
    /// <param name="newGun">读取信息的武器。</param>
    public abstract void ReplaceSlot(int slotID, FirearmController newWeapon);

    public abstract void SetVisibility(VisibilityMode visibility);

    /// <summary>
    /// 清空武器槽位 UI
    /// 卸下武器时槽位不会自动刷新，需手动调用。
    /// </summary>
    public abstract void ClearSlots(EquipmentFlags flags = EquipmentFlags.None);

    public abstract VisibilityMode GetVisibilityMode();

    public abstract void SetActive(bool active);

    private static EquipmentScreenBase _instance;
    public static EquipmentScreenBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<EquipmentScreenBase>(); }
            if(_instance == null) { _instance = UIReferences.Instance.GetComponentInChildren<EquipmentScreenBase>(true); }
            return _instance;
        }
    }
}
