using System;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

[DefaultExecutionOrder(-1002)]
public class NetworkWrapper : NetworkHelper
{

    [HideInInspector] public bool hasPingKick = false;
    public bool hasAFKKick { get; set; }
    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();
    private List<NetworkEventCallbacks> callbackList = new List<NetworkEventCallbacks>();

    private void Awake()
    {
        gameObject.name = "Game Network";
        DontDestroyOnLoad(gameObject);
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventCustom;
    }

    public void AddCallback(byte code, Action<Hashtable> callback)
    {
        callbackList.Add(new NetworkEventCallbacks() { Code = code, Callback = callback });
    }

    public static void AddNetworkCallback(byte code, Action<Hashtable> callback)
    {
        if (Instance == null) return;
        Instance.AddCallback(code, callback);
    }

    public void RemoveCallback(Action<Hashtable> callback)
    {
        NetworkEventCallbacks e = callbackList.Find(x => x.Callback == callback);
        if (e != null) { callbackList.Remove(e); }
    }

    public static void RemoveNetworkCallback(Action<Hashtable> callback)
    {
        if (Instance == null) return;
        Instance.RemoveCallback(callback);
    }

    public void OnEventCustom(EventData data)
    {
        if (data.CustomData == null) return;
        
        switch (data.Code)
        {
            case NetworkKeys.KickPlayerEvent:
                OnKick();
                break;
            case NetworkKeys.KillFeedEvent:
                KillReportEntry.Instance?.OnMessageReceive((Hashtable)data.CustomData);
                break;
            default:
                if(callbackList.Count > 0)
                {
                    for (int i = 0; i < callbackList.Count; i++)
                    {
                        if(callbackList[i].Code == data.Code)
                        {
                            Hashtable hastTable = (Hashtable)data.CustomData;
                            callbackList[i].Callback.Invoke(hastTable);
                        }
                    }
                }
                break;
        }
    }

    // 向所有订阅了该回调的客户端发送事件，类似 PhotonView.RPC，但不需要 PhotonView
    public void SendDataOverNetwork(byte code, Hashtable data)
    {
        SendOptions so = new SendOptions();
        PhotonNetwork.RaiseEvent(code, data, EventsAll, so);
    }

    // 向订阅了该回调的指定客户端发送事件，类似 PhotonView.RPC，但不需要 PhotonView
    public void SendDataOverNetworkToPlayer(byte code, Hashtable data, Player targetPlayer)
    {
        SendOptions so = new SendOptions();
        RaiseEventOptions reo = new RaiseEventOptions();
        reo.TargetActors = new int[1] { targetPlayer.ActorNumber };
        reo.CachingOption = EventCaching.DoNotCache;
        PhotonNetwork.RaiseEvent(code, data, reo, so);
    }

    public void OnPingKick()
    {
        hasPingKick = true;
    }

    public void KickPlayer(Player p)
    {
        SendOptions so = new SendOptions();
        var data = CommonUtil.CreatePhotonHashTable();
        PhotonNetwork.RaiseEvent(NetworkKeys.KickPlayerEvent, data, new RaiseEventOptions() { TargetActors = new int[] { p.ActorNumber } }, so);
    }

    void OnKick()
    {
        if (PhotonNetwork.InRoom)
        {
            PlayerPrefs.SetInt(NetworkKeys.KickKey, 1);
            PhotonNetwork.LeaveRoom(false);
        }
    }

    private void OnApplicationQuit()
    {
        GameConfig.Instance.ResetInstance();
        Resources.UnloadUnusedAssets();
    }

    public static new Player LocalPlayer { get { return PhotonNetwork.LocalPlayer; } }
    public static string NickName
    {
        get => PhotonNetwork.NickName;
        set => PhotonNetwork.NickName = value;
    }
    public static bool IsConnected { get { return PhotonNetwork.IsConnected; } }
    public static bool IsConnectedInRoom { get { return PhotonNetwork.IsConnected && PhotonNetwork.InRoom; } }
    // 我们是主客户端吗？
    public static bool IsMasterClient => PhotonNetwork.IsMasterClient;
    public static bool InRoom => PhotonNetwork.InRoom;
    public static Room CurrentRoom => PhotonNetwork.CurrentRoom;
    public static Player[] PlayerList => PhotonNetwork.PlayerList;
    public static Player[] PlayerListOthers => PhotonNetwork.PlayerListOthers;
    public static double Time => PhotonNetwork.Time;
    public static bool OfflineMode
    {
        get => PhotonNetwork.OfflineMode;
        set => PhotonNetwork.OfflineMode = value;
    }
    public static bool IsMessageQueueRunning
    {
        get => PhotonNetwork.IsMessageQueueRunning;
        set => PhotonNetwork.IsMessageQueueRunning = value;
    }

    public static void LeaveRoom(bool becomeInactive = true) => PhotonNetwork.LeaveRoom(becomeInactive);

    public static Hashtable GetRoomProperties()
    {
        return CurrentRoom.CustomProperties;
    }

    public static void SetRoomProperties(Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
    {
        CurrentRoom.SetCustomProperties(propertiesToSet, expectedProperties, webFlags);
    }
    
    public static void CleanBuffer()
    {      
        PhotonNetwork.OpCleanActorRpcBuffer(LocalPlayer.ActorNumber);
        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(LocalPlayer.ActorNumber);
    }

    public static int GetPing() => PhotonNetwork.GetPing();

    public static void Destroy(GameObject targetGo) => PhotonNetwork.Destroy(targetGo);

    public class NetworkEventCallbacks
    {
        public byte Code;
        public Action<Hashtable> Callback;
    }

    private static NetworkWrapper _instance;
    public static NetworkWrapper Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<NetworkWrapper>();
            return _instance;
        }
    }
}