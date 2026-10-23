using UnityEngine;
using System.Collections.Generic;
using HashData = ExitGames.Client.Photon.Hashtable;
/// 此脚本负责实例化并同步玩家请求的空投/包裹。
/// 此类没有直接引用，可直接替换为自定义脚本
public class LootDispatcher : MonoBehaviour
{

    [Header("Delivery")]
    public GameObject DropDeliveryPrefab;
    public float DeliveryTime = 10;

    [Header("Available Kits")]
    public List<LootInfo> AvailablePacks = new List<LootInfo>();
    /// 激活时将该实例登记到事件中
    void OnEnable()
    {
        GameEvents.onAirSupply += SendDelivery;
        NetworkWrapper.AddNetworkCallback(NetworkKeys.DropManager, OnNetworkMessage);
    }
    /// 禁用时从事件中移除
    void OnDisable()
    {
        GameEvents.onAirSupply -= SendDelivery;
        NetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
    }

    void OnNetworkMessage(HashData data)
    {
        GameObject newInstance = Instantiate(DropDeliveryPrefab, transform.position, Quaternion.identity) as GameObject;

        newInstance.GetComponent<LootBase>().Dispatch(new LootBase.LootData()
        {
            DropPrefab = AvailablePacks[(int)data["pack"]].Prefab,
            DropPosition = (Vector3)data["pos"],
            DeliveryDuration = DeliveryTime,
        });
    }
    /// 由内部事件调用
    public void SendDelivery(Vector3 position, int packID)
    {
        var data = CommonUtil.CreatePhotonHashTable();
        data.Add("pos", position);
        data.Add("pack", packID);

        NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.DropManager, data);
    }

    [System.Serializable]
    public class LootInfo
    {
        public string Name;
        public GameObject Prefab;
    }
}
