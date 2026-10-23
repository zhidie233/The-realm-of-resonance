using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class ResonanceManager : MonoBehaviourBase
{
    public static ResonanceManager Instance { get; private set; }

    [Header("当前共鸣值")]
    [SerializeField] private float currentResonance = 0f;

    private PhotonView _photonView;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _photonView = GetComponent<PhotonView>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        NetworkCallbacks.RoomPropertiesUpdate += OnRoomPropertiesUpdate;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        NetworkCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesUpdate;
    }

    private void Start()
    {
        if (GameConfig.Instance.resonanceConfig != null)
            GameConfig.Instance.resonanceConfig.Initialize();

        currentResonance = ReadRoomResonance();
        GameEvents.DispatchResonanceChanged(currentResonance);
    }

    /// 修改共鸣值，仅房主生效，结果写入房间属性
    public void ChangeResonance(string changeKey, float multiplier = 1f)
    {
        if (!NetworkWrapper.IsMasterClient) return;
        if (!GameConfig.Instance.resonanceEnabled) return;

        currentResonance += GameConfig.Instance.resonanceConfig.GetResonanceChangeValue(changeKey) * multiplier;

        Hashtable properties = new Hashtable();
        properties[NetworkKeys.ResonanceValueKey] = currentResonance;
        NetworkWrapper.CurrentRoom.SetCustomProperties(properties);

        GameEvents.DispatchResonanceChanged(currentResonance);
    }

    /// 任意端请求修改共鸣值：房主直接结算，其余端转交房主
    public static void RequestChange(string changeKey, float damage)
    {
        if (Instance == null) return;
        Instance.SendChangeRequest(changeKey, damage);
    }

    void SendChangeRequest(string changeKey, float damage)
    {
        if (NetworkWrapper.IsMasterClient)
        {
            ChangeResonance(changeKey, damage);
            return;
        }

        if (_photonView == null || !NetworkWrapper.InRoom) return;
        _photonView.RPC(nameof(RPC_RequestChange), RpcTarget.MasterClient, changeKey, damage);
    }

    [PunRPC]
    void RPC_RequestChange(string changeKey, float damage)
    {
        ChangeResonance(changeKey, damage);
    }

    void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!propertiesThatChanged.ContainsKey(NetworkKeys.ResonanceValueKey)) return;

        currentResonance = (float)propertiesThatChanged[NetworkKeys.ResonanceValueKey];
        GameEvents.DispatchResonanceChanged(currentResonance);
    }

    float ReadRoomResonance()
    {
        if (!NetworkWrapper.InRoom) return 0f;
        object value;
        if (NetworkWrapper.CurrentRoom.CustomProperties.TryGetValue(NetworkKeys.ResonanceValueKey, out value))
            return (float)value;
        return 0f;
    }

    public float GetCurrentResonance()
    {
        return currentResonance;
    }
}
