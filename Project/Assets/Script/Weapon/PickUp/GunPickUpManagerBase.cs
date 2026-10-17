using UnityEngine;

public abstract class GunPickUpManagerBase : PhotonHelper
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
        public GunPickUpBase.AmmoInformation Ammunition;
    }

    public GunPickUpBase LastTrigger { get; set; }

    /// <summary>
    /// Throw a weapon on all clients
    /// </summary>
    public abstract void ThrowGun(ThrowData throwData);

    /// <summary>
    /// Send a call to all the room clients to let them know that player will pick up a weapon
    /// </summary>
    public abstract void SendPickUp(PickUpData pickUpData);

    private static GunPickUpManagerBase _instance;
    public static GunPickUpManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<GunPickUpManagerBase>(); }
            return _instance;
        }
    }
}