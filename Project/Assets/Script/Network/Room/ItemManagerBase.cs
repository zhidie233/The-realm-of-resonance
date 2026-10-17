using UnityEngine;

public abstract class ItemManagerBase : MonoBehaviourBase
{

    /// <summary>
    /// 将给定物品加入队列，并在指定时间后重生
    /// </summary>
    /// <param name="item"></param>
    /// <param name="delay">为 0 时使用 PickupItemManager inspector 中设定的时间</param>
    public abstract void RespawnAfter(NetworkItem item, float delay = 0);

    /// <summary>
    /// 将给定物品加入网络物品池
    /// </summary>
    /// <param name="itemName">池列表名称</param>
    /// <param name="item"></param>
    public abstract void PoolItem(string itemName, NetworkItem item);

    private static ItemManagerBase _instance;
    public static ItemManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<ItemManagerBase>(); }
            return _instance;
        }
    }
}