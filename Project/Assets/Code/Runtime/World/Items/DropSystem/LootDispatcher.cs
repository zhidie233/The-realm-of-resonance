using UnityEngine;
using System.Collections.Generic;
using HashData = ExitGames.Client.Photon.Hashtable;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Multiplayer;
namespace Resonance.World
{
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
            EventBus.Subscribe<AirSupplyRequestedEvent>(OnEvt_onAirSupply_SendDelivery);
            NetworkWrapper.AddNetworkCallback(NetProtocol.DropManager, OnNetworkMessage);
        }
        /// 禁用时从事件中移除
        void OnDisable()
        {
            EventBus.Unsubscribe<AirSupplyRequestedEvent>(OnEvt_onAirSupply_SendDelivery);
            NetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
        }

        void OnNetworkMessage(HashData data)
        {
            GameObject newInstance = Instantiate(DropDeliveryPrefab, transform.position, Quaternion.identity) as GameObject;

            newInstance.GetComponent<LootDelivery>().Dispatch(new LootDelivery.LootData()
            {
                DropPrefab = AvailablePacks[(int)data["pack"]].Prefab,
                DropPosition = (Vector3)data["pos"],
                DeliveryDuration = DeliveryTime,
            });
        }
        /// 由内部事件调用
        public void SendDelivery(Vector3 position, int packID)
        {
            var data = NetPropertyStore.CreateBag();
            data.Add("pos", position);
            data.Add("pack", packID);

            NetworkWrapper.Instance.SendDataOverNetwork(NetProtocol.DropManager, data);
        }

        [System.Serializable]
        public class LootInfo
        {
            public string Name;
            public GameObject Prefab;
        }

        private void OnEvt_onAirSupply_SendDelivery(AirSupplyRequestedEvent e) => SendDelivery(e.Position, e.SupplyType);
    }
}
