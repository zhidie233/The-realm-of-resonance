using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 该脚本负责网络物品的实例化
/// 网络物品指实例化和销毁会同步到房间内其他所有客户端的 game object。
/// 有了该脚本，这些网络物品不必挂 PhotonView，也不必放进 Resources 目录
/// 从而大幅提高网络和内存效率。
/// 
/// 实例化网络物品只有一个要求：
/// 1. 物品预制体必须列在 'Network Items Prefabs' 列表中
/// </summary>
public class PickupSpawner : ItemRespawnerBase
{
    [Header("Network Prefabs")]
    public List<NetworkedItem> networkItemsPrefabs = new List<NetworkedItem>();

    [Header("Settings")]
    public float respawnItemsAfter = 12;

    //私有
    private Dictionary<string, NetworkedItem> networkItemsPool = new Dictionary<string, NetworkedItem>();
    private List<RespawnItems> respawnItems = new List<RespawnItems>();


    protected override void OnEnable()
    {
        if (!NetworkWrapper.IsConnected) return;

        base.OnEnable();
        NetworkWrapper.Instance.AddCallback(NetworkKeys.NetworkItemInstance, OnNetworkItemInstance);
        NetworkWrapper.Instance.AddCallback(NetworkKeys.NetworkItemChange, OnNetworkItemChange);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        NetworkWrapper.Instance?.RemoveCallback(OnNetworkItemInstance);
        NetworkWrapper.Instance?.RemoveCallback(OnNetworkItemChange);
    }

    void OnNetworkItemInstance(ExitGames.Client.Photon.Hashtable data)
    {
        //创建该物品的玩家不再实例化，因为他本地已经有了
        int actorID = (int)data["actorID"];
        if (NetworkWrapper.LocalPlayer.ActorNumber == actorID) return;

        string prefabName = (string)data["prefab"];
        NetworkedItem prefab = networkItemsPrefabs.Find(x =>
        {
            return (x != null && x.gameObject.name == prefabName);
        });

        if (prefab == null)
        {
            Debug.LogWarning($"The network prefab {prefabName} is not listed in the PickupSpawner of this scene.");
            return;
        }

        prefab = Instantiate(prefab.gameObject, (Vector3)data["position"], (Quaternion)data["rotation"]).GetComponent<NetworkedItem>();
        prefab.OnNetworkInstance(data);
        //回收该网络物品到池中
        PoolItem(prefabName, prefab);
    }

    public override void PoolItem(string itemName, NetworkedItem item)
    {
        if (networkItemsPool.ContainsKey(itemName)) return;
        networkItemsPool.Add(itemName, item);
    }

    /// <summary>
    /// 网络物品状态变化时调用，如 OnDestroy、OnEnable 或 OnDisable
    /// </summary>
    void OnNetworkItemChange(ExitGames.Client.Photon.Hashtable data)
    {
        string itemName = (string)data["name"];

        if (!networkItemsPool.ContainsKey(itemName))
        {
            Debug.LogWarning($"The network item {itemName} couldn't be found, maybe was instanced before this player enter in the room.");
            return;
        }

        int state = (int)data["active"];
        if (state == -1)
        {
            if (networkItemsPool[itemName] != null) Destroy(networkItemsPool[itemName].gameObject);
            networkItemsPool.Remove(itemName);
        }
        else
        {
            networkItemsPool[itemName].gameObject.SetActive(state == 1 ? true : false);
        }
    }

    public override void OnSlowUpdate()
    {
        CheckTimers();
    }

    void CheckTimers()
    {
        if (respawnItems.Count <= 0) return;

        int c = respawnItems.Count;
        for (int i = c - 1; i >= 0; i--)
        {
            if (Time.time - respawnItems[i].AddedTime >= respawnItems[i].RespawnAfter)
            {
                respawnItems[i].Item.SetActiveSync(true);
                respawnItems.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 将物品加入等待列表，在指定时间后重生
    /// </summary>
    public override void RespawnAfter(NetworkedItem item, float respawnAfter = 0)
    {
        respawnItems.Add(new RespawnItems()
        {
            Item = item,
            AddedTime = Time.time,
            RespawnAfter = respawnAfter <= 0 ? respawnItemsAfter : respawnAfter
        });
        item.SetActiveSync(false);
    }

    public class RespawnItems
    {
        public float AddedTime;
        public float RespawnAfter;
        public NetworkedItem Item;
    }
}