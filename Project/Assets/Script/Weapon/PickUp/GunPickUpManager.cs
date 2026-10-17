using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GunPickUpManager : GunPickUpManagerBase
{

    [Range(100,500)] public float ForceImpulse = 350;

    private void OnEnable()
    {
        PhotonNetworkWrapper.Instance.AddCallback(PropertiesKeys.WeaponPickUpEvent, OnNetworkCall);
    }

    private void OnDisable()
    {
        if (PhotonNetworkWrapper.Instance != null)
        PhotonNetworkWrapper.Instance.RemoveCallback(OnNetworkCall);
    }

    void OnNetworkCall(Hashtable data)
    {
        int code = (int)data["type"];
        if (code == 0)//Pick up a weapon from the map
        {
            NetworkPickUp(data);
        }
        else if (code == 1)//Instance a weapon in all clients
        {
            NetworkInstanceWeapon(data);
        }
    }

    /// <summary>
    /// 调用后同步拾取武器的生成
    /// </summary>
    public override void ThrowGun(ThrowData throwData)
    {
        int[] i = new int[3];
        i[0] = throwData.Data[0];
        i[1] = throwData.Data[1];
        i[2] = GameController.LocalPlayerViewID;
        //防止物体重名
        int rand = Random.Range(0, 9999);
        //唯一的物体名
        string prefix = PhotonNetworkWrapper.NickName + rand;

        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("type", 1);
        data.Add("name", prefix);
        data.Add("gunID", throwData.GunID);
        data.Add("info", i);
        data.Add("destroy", throwData.AutoDestroy);
        data.Add("origin", throwData.Origin);
        data.Add("dir", throwData.Direction);

        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.WeaponPickUpEvent, data);
    }

    /// <summary>
    /// 玩家生成武器（拾取武器）时在所有客户端调用
    /// </summary>
    void NetworkInstanceWeapon(Hashtable data)
    {
        GunData ginfo = GameConfig.Instance.GetWeapon((int)data["gunID"]);
        if (ginfo.PickUpPrefab == null)
        {
            Debug.LogError(string.Format("The weapon: '{0}' not have a pick up prefab in Gun info", ginfo.Name));
            return;
        }

        GameObject trow;
        trow = ginfo.PickUpPrefab.gameObject;

        int[] info = (int[])data["info"];
        GameObject p = FindPlayerRoot(info[2]);
        if (p == null) return;

        var direction = (Vector3)data["dir"];
        GameObject gun = Instantiate(trow, (Vector3)data["origin"], Quaternion.identity) as GameObject;
        Collider[] c = p.GetComponentsInChildren<Collider>();
        for (int i = 0; i < c.Length; i++)
        {
            Physics.IgnoreCollision(c[i], gun.GetComponent<Collider>());
        }
        gun.GetComponent<Rigidbody>().AddForce(direction * ForceImpulse);
        int clips = info[0];
        var gp = gun.GetComponent<GunPickUpBase>();
        gp.Ammunition.Clips = clips;
        gp.Ammunition.Bullets = info[1];
        gp.AutoDestroy = (bool)data["destroy"];
        gun.name = gun.name.Replace("(Clone)", string.Empty);
        gun.name += (string)data["name"];
        gun.transform.parent = transform;
    }

    /// <summary>
    /// 通知房间内所有客户端该玩家将要拾取武器
    /// </summary>
    public override void SendPickUp(PickUpData pickUpData)
    {
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("type", 0);
        data.Add("name", pickUpData.Identifier);
        data.Add("gunID", pickUpData.GunID);
        data.Add("bullets", pickUpData.Ammunition.Bullets);
        data.Add("clips", pickUpData.Ammunition.Clips);
        data.Add("actorID", PhotonNetworkWrapper.LocalPlayer.ActorNumber);

        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.WeaponPickUpEvent, data);
    }

    /// <summary>
    /// 玩家从地图上拾取武器时在所有客户端调用
    /// </summary>
    /// <param name="data"></param>
    void NetworkPickUp(Hashtable data)
    {
        // 其中一条消息可能就是我们自己的
        // 注意：若不关心自己失败的拾取尝试，可以先判断 "active"。
        GameObject g = GameObject.Find((string)data["name"]);
        if (g == null)
        {
            Debug.LogWarning("This Gun does not exist in this scene");
            return;
        }
        //是我的
        if ((int)data["actorID"] == PhotonNetworkWrapper.LocalPlayer.ActorNumber)
        {
            GunPickUpData pi = new GunPickUpData();
            pi.ID = (int)data["gunID"];
            pi.ItemObject = g;
            pi.Clips = (int)data["clips"];
            pi.Bullets = (int)data["bullets"];
            GameEvents.onPickUpGun(pi);//that call will be received in GunInventory.cs
        }
        Destroy(g);
    }
}