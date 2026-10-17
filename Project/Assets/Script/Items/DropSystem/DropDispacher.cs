using UnityEngine;
using System.Collections.Generic;
using HashData = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// 此脚本负责实例化并同步玩家请求的空投/包裹。
/// 此类没有直接引用，可直接替换为自定义脚本
/// </summary>
public class DropDispacher : MonoBehaviour
{

    [Header("Delivery")]
    public GameObject DropDeliveryPrefab;
    public float DeliveryTime = 10;

    [Header("Available Kits")]
    public List<DropInfo> AvailableDrops = new List<DropInfo>();

    /// <summary>
    /// 激活时将该实例登记到事件中
    /// </summary>
    void OnEnable()
    {
        GameEvents.onAirKit += SendDevilery;
        PhotonNetworkWrapper.AddNetworkCallback(PropertiesKeys.DropManager, OnNetworkMessage);
    }

    /// <summary>
    /// 禁用时从事件中移除
    /// </summary>
    void OnDisable()
    {
        GameEvents.onAirKit -= SendDevilery;
        PhotonNetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
    }

    void OnNetworkMessage(HashData data)
    {
        GameObject newInstance = Instantiate(DropDeliveryPrefab, transform.position, Quaternion.identity) as GameObject;

        newInstance.GetComponent<DropBase>().Dispatch(new DropBase.DropData()
        {
            DropPrefab = AvailableDrops[(int)data["kit"]].Prefab,
            DropPosition = (Vector3)data["pos"],
            DeliveryDuration = DeliveryTime,
        });
    }

    /// <summary>
    /// 由内部事件调用
    /// </summary>
    public void SendDevilery(Vector3 position, int kitID)
    {
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("pos", position);
        data.Add("kit", kitID);

        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.DropManager, data);
    }

    [System.Serializable]
    public class DropInfo
    {
        public string Name;
        public GameObject Prefab;
    }
}