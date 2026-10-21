using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Slot Guide", menuName = "Game/Weapon/SlotGuide")]
public class WeaponSlotGuide : ScriptableObject
{
    [Reorderable] public FirearmType[] primarySlots;
    [Reorderable] public FirearmType[] secondarySlots;
    [Reorderable] public FirearmType[] perksSlots;
    [Reorderable] public FirearmType[] letalSlots;
    public bool CanBeOnSlot(FirearmType gunType, int slotID)
    {
        if (slotID == 0) return TypeInList(gunType, primarySlots);
        else if(slotID == 1) return TypeInList(gunType, secondarySlots);
        else if (slotID == 2) return TypeInList(gunType, letalSlots);
        else return TypeInList(gunType, perksSlots);
    }

    private bool TypeInList(FirearmType gunType, FirearmType[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == gunType) return true;
        }
        return false;
    }
}