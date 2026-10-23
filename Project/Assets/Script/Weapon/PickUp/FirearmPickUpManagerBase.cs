using UnityEngine;

public abstract class FirearmPickUpManagerBase : NetworkHelper
{
    public struct ThrowData
    {
        public int WeaponID;
        public Vector3 Origin;
        public Vector3 Direction;
        public int[] Data;
        public bool AutoDestroy;
    }

    public struct PickUpData
    {
        public string Identifier;
        public int WeaponID;
        public FirearmPickUpBase.AmmoInformation Ammunition;
    }

    public FirearmPickUpBase LastTrigger { get; set; }
    /// 在所有客户端上投掷武器
    public abstract void ThrowGun(ThrowData throwData);
    /// 通知房间内所有客户端该玩家将要拾取武器
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
