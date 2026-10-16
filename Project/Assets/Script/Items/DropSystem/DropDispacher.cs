using UnityEngine;
using System.Collections.Generic;
using HashData = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// This script take care of instantiate and synchronize drop/packages requested by players.
/// this class doesn't have direct references hence can be just replaced with a custom script
/// </summary>
public class DropDispacher : MonoBehaviour
{

    [Header("Delivery")]
    public GameObject DropDeliveryPrefab;
    public float DeliveryTime = 10;

    [Header("Available Kits")]
    public List<DropInfo> AvailableDrops = new List<DropInfo>();

    /// <summary>
    /// when activated, record this in the event
    /// </summary>
    void OnEnable()
    {
        GameEvents.onAirKit += SendDevilery;
        PhotonNetworkWrapper.AddNetworkCallback(PropertiesKeys.DropManager, OnNetworkMessage);
    }

    /// <summary>
    /// when disabled, quit this in the event
    /// </summary>
    void OnDisable()
    {
        GameEvents.onAirKit -= SendDevilery;
        PhotonNetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
    }

    /// <summary>
    /// 
    /// </summary>
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
    /// This is called by an internal event
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