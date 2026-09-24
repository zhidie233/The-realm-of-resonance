using System;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

[DefaultExecutionOrder(-1002)]
public class bl_PhotonNetwork : bl_PhotonHelper
{

    [HideInInspector] public bool hasPingKick = false;
    public bool hasAFKKick { get; set; }
    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();
    private List<PhotonEventsCallbacks> callbackList = new List<PhotonEventsCallbacks>();

    private void Awake()
    {
        gameObject.name = "MFPS Network";
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
        callbackList.Add(new PhotonEventsCallbacks() { Code = code, Callback = callback });
    }

    public static void AddNetworkCallback(byte code, Action<Hashtable> callback)
    {
        if (Instance == null) return;
        Instance.AddCallback(code, callback);
    }

    public void RemoveCallback(Action<Hashtable> callback)
    {
        PhotonEventsCallbacks e = callbackList.Find(x => x.Callback == callback);
        if (e != null) { callbackList.Remove(e); }
    }

    // <param name="callback"></param>
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
            case PropertiesKeys.KickPlayerEvent:
                OnKick();
                break;
            case PropertiesKeys.KillFeedEvent:
                bl_KillFeedBase.Instance?.OnMessageReceive((Hashtable)data.CustomData);
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

    // Send an event to be invoke in all clients subscribed to the callback Similar to PhotonView.RPC but without needed of a PhotonView
    public void SendDataOverNetwork(byte code, Hashtable data)
    {
        SendOptions so = new SendOptions();
        PhotonNetwork.RaiseEvent(code, data, EventsAll, so);
    }

    // Send an event to be invoke in a specific client subscribed to the callback Similar to PhotonView.RPC but without needed of a PhotonView
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
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        PhotonNetwork.RaiseEvent(PropertiesKeys.KickPlayerEvent, data, new RaiseEventOptions() { TargetActors = new int[] { p.ActorNumber } }, so);
    }

    void OnKick()
    {
        if (PhotonNetwork.InRoom)
        {
            PlayerPrefs.SetInt(PropertiesKeys.KickKey, 1);
            PhotonNetwork.LeaveRoom(false);
        }
    }

    private void OnApplicationQuit()
    {
        bl_GameData.Instance.ResetInstance();
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
    // Are we the master client?
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

    public class PhotonEventsCallbacks
    {
        public byte Code;
        public Action<Hashtable> Callback;
    }

    private static bl_PhotonNetwork _instance;
    public static bl_PhotonNetwork Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<bl_PhotonNetwork>();
            return _instance;
        }
    }
}