using UnityEngine;

public abstract class FirearmPickUpManagerBase : NetworkHelper
{
    public struct ThrowData
    {
        public int GunID;
        public Vector3 Origin;
        public Vector3 Direction;
        public int[] Data;
        public bool AutoDestroy;
    }

    public struct PickUpData
    {
        public string Identifier;
        public int GunID;
        public FirearmPickUpBase.AmmoInformation Ammunition;
    }

    public FirearmPickUpBase LastTrigger { get; set; }

    /// <summary>
    /// 在所有客户端上投掷武器
    /// </summary>
    public abstract void ThrowGun(ThrowData throwData);

    /// <summary>
    /// 通知房间内所有客户端该玩家将要拾取武器
    /// </summary>
    public abstract void SendPickUp(PickUpData pickUpData);

    private static FirearmPickUpManagerBase _instance;
    public static FirearmPickUpManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<FirearmPickUpManagerBase>(); }
            return _instance;
        }
    }
}