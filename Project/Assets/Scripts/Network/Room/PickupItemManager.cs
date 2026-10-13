using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handle the instantiation of network items
/// Network items are those game objects that instantiation and or destruction is replicated on all other clients in the room.
/// With this script, those network items doesn't have to have a PhotonView or be located in a Resources folder
/// Making way more network and memory efficient.
/// 
/// In order to instance a network item, there's only one requirement:
/// 1. the item prefab has to be listed in the 'Network Items Prefabs' list
/// </summary>
public class PickupItemManager : ItemManagerBase
{
    [Header("Network Prefabs")]
    public List<NetworkItem> networkItemsPrefabs = new List<NetworkItem>();

    [Header("Settings")]
    public float respawnItemsAfter = 12;

    //private
    private Dictionary<string, NetworkItem> networkItemsPool = new Dictionary<string, NetworkItem>();
    private List<RespawnItems> respawnItems = new List<RespawnItems>();


    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        if (!PhotonNetworkWrapper.IsConnected) return;

        base.OnEnable();
        PhotonNetworkWrapper.Instance.AddCallback(PropertiesKeys.NetworkItemInstance, OnNetworkItemInstance);
        PhotonNetworkWrapper.Instance.AddCallback(PropertiesKeys.NetworkItemChange, OnNetworkItemChange);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        PhotonNetworkWrapper.Instance?.RemoveCallback(OnNetworkItemInstance);
        PhotonNetworkWrapper.Instance?.RemoveCallback(OnNetworkItemChange);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkItemInstance(ExitGames.Client.Photon.Hashtable data)
    {
        //don't instance for the player that create the item since it's already instance for him
        int actorID = (int)data["actorID"];
        if (PhotonNetworkWrapper.LocalPlayer.ActorNumber == actorID) return;

        string prefabName = (string)data["prefab"];
        NetworkItem prefab = networkItemsPrefabs.Find(x =>
        {
            return (x != null && x.gameObject.name == prefabName);
        });

        if (prefab == null)
        {
            Debug.LogWarning($"The network prefab {prefabName} is not listed in the PickupItemManager of this scene.");
            return;
        }

        prefab = Instantiate(prefab.gameObject, (Vector3)data["position"], (Quaternion)data["rotation"]).GetComponent<NetworkItem>();
        prefab.OnNetworkInstance(data);
        //pool this network item
        PoolItem(prefabName, prefab);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void PoolItem(string itemName, NetworkItem item)
    {
        if (networkItemsPool.ContainsKey(itemName)) return;
        networkItemsPool.Add(itemName, item);
    }

    /// <summary>
    /// Called when the state of a network item change, eg: OnDestroy, OnEnable or OnDisable
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

    /// <summary>
    /// 
    /// </summary>
    public override void OnSlowUpdate()
    {
        CheckTimers();
    }

    /// <summary>
    /// 
    /// </summary>
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
    /// Add an item to the waiting list to respawn it after certain time
    /// </summary>
    public override void RespawnAfter(NetworkItem item, float respawnAfter = 0)
    {
        respawnItems.Add(new RespawnItems()
        {
            Item = item,
            AddedTime = Time.time,
            RespawnAfter = respawnAfter <= 0 ? respawnItemsAfter : respawnAfter
        });
        item.SetActiveSync(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public class RespawnItems
    {
        public float AddedTime;
        public float RespawnAfter;
        public NetworkItem Item;
    }
}