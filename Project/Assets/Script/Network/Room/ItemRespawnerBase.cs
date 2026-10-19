using UnityEngine;

public abstract class ItemRespawnerBase : MonoBehaviourBase
{

    /// <summary>
    /// 将给定物品加入队列，并在指定时间后重生
    /// </summary>
    /// <param name="item"></param>
    /// <param name="delay">为 0 时使用 PickupSpawner inspector 中设定的时间</param>
    public abstract void RespawnAfter(NetworkedItem item, float delay = 0);

    /// <summary>
    /// 将给定物品加入网络物品池
    /// </summary>
    /// <param name="itemName">池列表名称</param>
    /// <param name="item"></param>
    public abstract void PoolItem(string itemName, NetworkedItem item);

    private static ItemRespawnerBase _instance;
    public static ItemRespawnerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<ItemRespawnerBase>(); }
            return _instance;
        }
    }
}