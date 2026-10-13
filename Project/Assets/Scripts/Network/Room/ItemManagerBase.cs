using UnityEngine;

public abstract class ItemManagerBase : MonoBehaviourBase
{

    /// <summary>
    /// Queue the given item and respawn after the defined time
    /// </summary>
    /// <param name="item"></param>
    /// <param name="delay">if 0 the time defined in the PickupItemManager inspector will be used</param>
    public abstract void RespawnAfter(NetworkItem item, float delay = 0);

    /// <summary>
    /// Add the given item to the pool network items
    /// </summary>
    /// <param name="itemName">pool list name</param>
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