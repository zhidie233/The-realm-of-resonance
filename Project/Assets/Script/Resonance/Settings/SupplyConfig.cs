using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SupplyConfig", menuName = "项目/共振/SupplyConfig")]
public class SupplyConfig : ScriptableObject
{
    public int supplyInterval = 50;
    [Header("掉落包裹")]
    [Tooltip("对应 LootDispatcher 中 AvailablePacks 的索引")]
    public int[] supplyPackIDs = new int[] { 0, 1 };
}
