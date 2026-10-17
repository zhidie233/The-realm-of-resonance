using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;
using Game.Internal.Structures;
using Photon.Realtime;

public class KillFeedManager : KillFeedBase
{    
    public Color SelfColor = Color.green;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (PhotonNetworkWrapper.InRoom)
        {
            OnJoined();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        PhotonCallbacks.PlayerLeftRoom += OnPhotonPlayerDisconnected;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonCallbacks.PlayerLeftRoom -= OnPhotonPlayerDisconnected;
    }

    /// <summary>
    /// Player Joined? sync
    /// </summary>
    void OnJoined()
    {
        SendMessageEvent(string.Format("{0} {1}", PhotonNetworkWrapper.LocalPlayer.NickName, GameTexts.JoinedInMatch));
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendKillMessageEvent(FeedData feedData)
    {
        HashTable data = new HashTable();
        data.Add("killer", feedData.LeftText);
        data.Add("killed", feedData.RightText);
        data.Add("gunid", (int)feedData.Data["gunid"]);
        data.Add("team", feedData.Team);
        data.Add("headshot", (bool)feedData.Data["headshot"]);
        data.Add("mt", KillFeedMessageType.WeaponKillEvent);
        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendMessageEvent(string message, bool localOnly = false)
    {
        HashTable data = new HashTable();
        data.Add("message", message);
        data.Add("mt", KillFeedMessageType.Message);
        if (localOnly) { ReceiveMessage(data); return; }

        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam)
    {
        HashTable data = new HashTable();
        data.Add("killer", teamHighlightMessage);
        data.Add("message", normalMessage);
        data.Add("team", playerTeam);
        data.Add("mt", KillFeedMessageType.TeamHighlightMessage);
        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    void SendMessageOverNetwork(HashTable data)
    {
        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.KillFeedEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnMessageReceive(HashTable data)
    {
        KillFeedMessageType mtype = (KillFeedMessageType)data["mt"];
        switch (mtype)
        {
            case KillFeedMessageType.WeaponKillEvent:
                ReceiveWeaponKillEvent(data);
                break;
            case KillFeedMessageType.Message:
                ReceiveMessage(data);
                break;
            case KillFeedMessageType.TeamHighlightMessage:
                ReceiveOnePlayerMessage(data);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveWeaponKillEvent(HashTable data)
    {
        KillFeed kf = new KillFeed();
        kf.Killer = (string)data["killer"];
        kf.Killed = (string)data["killed"];
        kf.GunID = (int)data["gunid"];
        kf.HeadShot = (bool)data["headshot"];
        kf.KillerTeam = (Team)data["team"];
        kf.messageType = KillFeedMessageType.WeaponKillEvent;

        KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveMessage(HashTable data)
    {
        KillFeed kf = new KillFeed();
        kf.Message = (string)data["message"];
        kf.messageType = KillFeedMessageType.Message;


        KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveOnePlayerMessage(HashTable data)
    {
        KillFeed kf = new KillFeed();
        kf.Killer = (string)data["killer"];
        kf.Message = (string)data["message"];
        kf.KillerTeam = (Team)data["team"];
        kf.messageType = KillFeedMessageType.TeamHighlightMessage;

        KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
        SendMessageEvent(string.Format("{0} {1}", otherPlayer.NickName, GameTexts.LeftOfMatch), true);
    }
  
    /// <summary>
    /// 
    /// </summary>
    public override void AddCustomIcon(string keyName, Sprite icon)
    {
        if (customIcons.Exists(x => x.Name.Equals(keyName))) return;

        customIcons.Add(new CustomIcons()
        {
            Name = keyName,
            Icon = icon
        });
    }

    public override int GetCustomIconIndex(string keyName) => customIcons.FindIndex(x => x.Name == keyName);
    public override CustomIcons GetCustomIconByIndex(int index)
    {
        if (index > customIcons.Count - 1) return null;
        return customIcons[index];
    }

    /// <summary>
    /// 
    /// </summary>
    public override Sprite GetCustomIcon(string keyName)
    {
        Sprite spr = null;
        if (customIcons.Exists(x => x.Name == keyName))
        {
            spr = customIcons.Find(x => x.Name == keyName).Icon;
        }
        else { Debug.LogWarning($"Custom icon {keyName} has not been register in the custom icons list"); }

        return spr;
    }
}