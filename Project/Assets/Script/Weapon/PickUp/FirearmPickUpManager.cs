using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class FirearmPickUpManager : FirearmPickUpManagerBase
{

    [Range(100,500)] public float ForceImpulse = 350;

    private void OnEnable()
    {
        NetworkWrapper.Instance.AddCallback(NetworkKeys.WeaponPickUpEvent, OnNetworkCall);
    }

    private void OnDisable()
    {
        if (NetworkWrapper.Instance != null)
        NetworkWrapper.Instance.RemoveCallback(OnNetworkCall);
    }

    void OnNetworkCall(Hashtable data)
    {
        int code = (int)data["type"];
        if (code == 0)
        {
            NetworkPickUp(data);
        }
        else if (code == 1)
        {
            NetworkInstanceWeapon(data);
        }
    }
    /// 调用后同步拾取武器的生成
    public override void ThrowGun(ThrowData throwData)
    {
        int[] i = new int[3];
        i[0] = throwData.Data[0];
        i[1] = throwData.Data[1];
        i[2] = GameController.LocalPlayerViewID;
        //防止物体重名
        int rand = Random.Range(0, 9999);
        //唯一的物体名
        string prefix = NetworkWrapper.NickName + rand;

        var data = CommonUtil.CreatePhotonHashTable();
        data.Add("type", 1);
        data.Add("name", prefix);
        data.Add("weaponId", throwData.WeaponID);
        data.Add("info", i);
        data.Add("destroy", throwData.AutoDestroy);
        data.Add("origin", throwData.Origin);
        data.Add("dir", throwData.Direction);

        NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.WeaponPickUpEvent, data);
    }
    /// 玩家生成武器（拾取武器）时在所有客户端调用
    void NetworkInstanceWeapon(Hashtable data)
    {
        FirearmData ginfo = GameConfig.Instance.GetWeapon((int)data["weaponId"]);
        if (ginfo.WorldPrefab == null)
        {
            Debug.LogError(string.Format("武器“{0}”的配置中没有拾取预制体。", ginfo.Name));
            return;
        }

        GameObject trow;
        trow = ginfo.WorldPrefab.gameObject;

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
        var gp = gun.GetComponent<FirearmPickUpBase>();
        gp.Ammunition.Clips = clips;
        gp.Ammunition.Bullets = info[1];
        gp.AutoDestroy = (bool)data["destroy"];
        gun.name = gun.name.Replace("(Clone)", string.Empty);
        gun.name += (string)data["name"];
        gun.transform.parent = transform;
    }
    /// 通知房间内所有客户端该玩家将要拾取武器
    public override void SendPickUp(PickUpData pickUpData)
    {
        var data = CommonUtil.CreatePhotonHashTable();
        data.Add("type", 0);
        data.Add("name", pickUpData.Identifier);
        data.Add("weaponId", pickUpData.WeaponID);
        data.Add("bullets", pickUpData.Ammunition.Bullets);
        data.Add("clips", pickUpData.Ammunition.Clips);
        data.Add("actorID", NetworkWrapper.LocalPlayer.ActorNumber);

        NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.WeaponPickUpEvent, data);
    }
    /// 玩家从地图上拾取武器时在所有客户端调用
    void NetworkPickUp(Hashtable data)
    {
        // 其中一条消息可能就是我们自己的
        // 注意：若不关心自己失败的拾取尝试，可以先判断 "active"。
        GameObject g = GameObject.Find((string)data["name"]);
        if (g == null)
        {
            Debug.LogWarning("该武器不存在于当前场景。");
            return;
        }
        //是我的
        if ((int)data["actorID"] == NetworkWrapper.LocalPlayer.ActorNumber)
        {
            FirearmPickUpData pi = new FirearmPickUpData();
            pi.ID = (int)data["weaponId"];
            pi.ItemObject = g;
            pi.Clips = (int)data["clips"];
            pi.Bullets = (int)data["bullets"];
            GameEvents.onPickUpGun(pi);
        }
        Destroy(g);
    }
}
