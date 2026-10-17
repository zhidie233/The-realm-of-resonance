using UnityEditor;
using UnityEngine;

public class NetworkItem : MonoBehaviour
{
    public ItemAuthority itemAuthority = ItemAuthority.All;
    [GameToggle] public bool isSceneItem = false;

    [SerializeField, HideInInspector]
    private bool isInitializated = false;

    public int OwnerActorID { get; set; } = -1;

    [SerializeField, HideInInspector]
    private string m_itemName;
    public string ItemName
    {
        get
        {
            if (string.IsNullOrEmpty(m_itemName))
            {
                m_itemName = $"{gameObject.name.Replace(" (Clone)", "")} [{StringUtility.GenerateKey()}]";
            }
            return m_itemName;
        }
        set => m_itemName = value;
    }

    private void Start()
    {
        Init();
    }

    public void OnLocalInstanced()
    {
        // 重置物品名称，因为预制体可能已带有自定义名称。
        m_itemName = string.Empty;
        isInitializated = false;
    }

    /// <summary>
    /// 本地玩家创建此物品时执行
    /// </summary>
    void Init()
    {
        if (isInitializated)
        {
            //以防这是场景物品
            ItemManagerBase.Instance.PoolItem(m_itemName, this);
            return;
        }

        string prefabName = gameObject.name.Replace(" (Clone)", "");
        gameObject.name = ItemName;
        OwnerActorID = PhotonNetworkWrapper.LocalPlayer.ActorNumber;
        isInitializated = true;

        //在此把物品加入池中，因为本地玩家不会走添加流程
        ItemManagerBase.Instance.PoolItem(ItemName, this);

        //不为每个物品各用一个 PhotonView，而是直接同步信息
        //并通过唯一名称识别物品
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("prefab", prefabName);
        data.Add("name", ItemName);
        data.Add("actorID", OwnerActorID);
        data.Add("position", transform.position);
        data.Add("rotation", transform.rotation);

        //该调用由 PickupItemManager.cs 的 OnNetworkItemInstance() 接收并处理
        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemInstance, data);
    }

    /// <summary>
    /// 此物品是为其他玩家创建时执行
    /// </summary>
    public void OnNetworkInstance(ExitGames.Client.Photon.Hashtable data)
    {
        isSceneItem = false;
        ItemName = (string)data["name"];
        OwnerActorID = (int)data["actorID"];

        gameObject.name = ItemName;
        isInitializated = true;
    }

    /// <summary>
    /// 为所有客户端销毁此网络物品
    /// </summary>
    public void DestroySync()
    {
        if (!isAuthorized()) return;

        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("name", ItemName);
        data.Add("active", -1);

        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemChange, data);
    }

    /// <summary>
    /// 为所有客户端启用或禁用此网络物品
    /// </summary>
    public void SetActiveSync(bool active)
    {
        if (!isAuthorized()) return;
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("name", ItemName);
        data.Add("active", active ? 1 : 0);

        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemChange, data);
    }

    public bool isAuthorized()
    {
        if (itemAuthority == ItemAuthority.All) return true;
        if (itemAuthority == ItemAuthority.MasterClientOnly) return PhotonNetworkWrapper.IsMasterClient;
        return PhotonNetworkWrapper.LocalPlayer.ActorNumber == OwnerActorID;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!isSceneItem) { isInitializated = false; }
        if (Application.isPlaying || !isSceneItem || ItemManagerBase.Instance == null) return;

        EditorValidateName();
    }

    public void EditorValidateName()
    {
        if (string.IsNullOrEmpty(m_itemName))
        {
            m_itemName = ItemName;
            gameObject.name = m_itemName;
            isInitializated = true;
        }
        else if (gameObject.name.Contains("("))
        {
            int io = gameObject.name.LastIndexOf('[');
            m_itemName = "";
            if (io != -1)
            {
                string baseName = gameObject.name.Substring(0, io - 1);
                gameObject.name = baseName;
            }
            else
            {
                gameObject.name = "New Item";
            }
            m_itemName = ItemName;
            m_itemName = m_itemName.Replace("(", "");
            m_itemName = m_itemName.Replace(")", "");
            gameObject.name = m_itemName;
            isInitializated = true;
            EditorUtility.SetDirty(this);
        }
    }
#endif

    [System.Serializable]
    public enum ItemAuthority
    {
        All = 0,
        CreatorOnly,
        MasterClientOnly
    }
}