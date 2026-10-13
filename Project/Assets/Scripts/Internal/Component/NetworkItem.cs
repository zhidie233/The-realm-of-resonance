using UnityEditor;
using UnityEngine;

public class NetworkItem : MonoBehaviour
{
    public ItemAuthority itemAuthority = ItemAuthority.All;
    [GFWorksToogle] public bool isSceneItem = false;

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

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        Init();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLocalInstanced()
    {
        // ensure to reset the name of the item since the prefab could be already have an custom name.
        m_itemName = string.Empty;
        isInitializated = false;
    }

    /// <summary>
    /// If the local player create this item, this will be executed
    /// </summary>
    void Init()
    {
        if (isInitializated)
        {
            //in case this is a scene item
            ItemManagerBase.Instance.PoolItem(m_itemName, this);
            return;
        }

        string prefabName = gameObject.name.Replace(" (Clone)", "");
        gameObject.name = ItemName;
        OwnerActorID = PhotonNetworkWrapper.LocalPlayer.ActorNumber;
        isInitializated = true;

        //add the item in the pool here since it won't be added for the local player
        ItemManagerBase.Instance.PoolItem(ItemName, this);

        //instead of use a PhotonView for each item, we simple sync the information
        //and identify the item by an unique name
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("prefab", prefabName);
        data.Add("name", ItemName);
        data.Add("actorID", OwnerActorID);
        data.Add("position", transform.position);
        data.Add("rotation", transform.rotation);

        //this call is received and handled in PickupItemManager.cs -> OnNetworkItemInstance()
        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemInstance, data);
    }

    /// <summary>
    /// If this item was created for other player, this code will be executed
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
    /// Destroy this network item for all clients
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
    /// Active or disable this network item for all clients
    /// </summary>
    public void SetActiveSync(bool active)
    {
        if (!isAuthorized()) return;
        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("name", ItemName);
        data.Add("active", active ? 1 : 0);

        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemChange, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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