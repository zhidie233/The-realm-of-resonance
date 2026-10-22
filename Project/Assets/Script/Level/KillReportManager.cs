using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class KillReportManager : KillReportEntry
{    
    public Color SelfColor = Color.green;

    void Awake()
    {
        if (NetworkWrapper.InRoom)
        {
            OnJoined();
        }
    }

    void OnEnable()
    {
        NetworkCallbacks.PlayerLeftRoom += OnPhotonPlayerDisconnected;
    }

    void OnDisable()
    {
        NetworkCallbacks.PlayerLeftRoom -= OnPhotonPlayerDisconnected;
    }
    /// 玩家加入了？同步
    void OnJoined()
    {
        SendMessageEvent(string.Format("{0} {1}", NetworkWrapper.LocalPlayer.NickName, GameTexts.JoinedInMatch));
    }

    public override void SendKillMessageEvent(FeedData feedData)
    {
        HashTable data = new HashTable();
        data.Add("killer", feedData.LeftText);
        data.Add("killed", feedData.RightText);
        data.Add("gunid", (int)feedData.Data["gunid"]);
        data.Add("team", feedData.Team);
        data.Add("headshot", (bool)feedData.Data["headshot"]);
        data.Add("mt", KillReportMessageType.WeaponKillEvent);
        SendMessageOverNetwork(data);
    }

    public override void SendMessageEvent(string message, bool localOnly = false)
    {
        HashTable data = new HashTable();
        data.Add("message", message);
        data.Add("mt", KillReportMessageType.Message);
        if (localOnly) { ReceiveMessage(data); return; }

        SendMessageOverNetwork(data);
    }

    public override void SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam)
    {
        HashTable data = new HashTable();
        data.Add("killer", teamHighlightMessage);
        data.Add("message", normalMessage);
        data.Add("team", playerTeam);
        data.Add("mt", KillReportMessageType.TeamHighlightMessage);
        SendMessageOverNetwork(data);
    }

    void SendMessageOverNetwork(HashTable data)
    {
        NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.KillFeedEvent, data);
    }

    public override void OnMessageReceive(HashTable data)
    {
        KillReportMessageType mtype = (KillReportMessageType)data["mt"];
        switch (mtype)
        {
            case KillReportMessageType.WeaponKillEvent:
                ReceiveWeaponKillEvent(data);
                break;
            case KillReportMessageType.Message:
                ReceiveMessage(data);
                break;
            case KillReportMessageType.TeamHighlightMessage:
                ReceiveOnePlayerMessage(data);
                break;
        }
    }

    void ReceiveWeaponKillEvent(HashTable data)
    {
        KillReport kf = new KillReport();
        kf.Killer = (string)data["killer"];
        kf.Killed = (string)data["killed"];
        kf.GunID = (int)data["gunid"];
        kf.HeadShot = (bool)data["headshot"];
        kf.KillerTeam = (Team)data["team"];
        kf.messageType = KillReportMessageType.WeaponKillEvent;

        KillReportUIBase.Instance.SetKillFeed(kf);
    }

    void ReceiveMessage(HashTable data)
    {
        KillReport kf = new KillReport();
        kf.Message = (string)data["message"];
        kf.messageType = KillReportMessageType.Message;

        KillReportUIBase.Instance.SetKillFeed(kf);
    }

    void ReceiveOnePlayerMessage(HashTable data)
    {
        KillReport kf = new KillReport();
        kf.Killer = (string)data["killer"];
        kf.Message = (string)data["message"];
        kf.KillerTeam = (Team)data["team"];
        kf.messageType = KillReportMessageType.TeamHighlightMessage;

        KillReportUIBase.Instance.SetKillFeed(kf);
    }

    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
        SendMessageEvent(string.Format("{0} {1}", otherPlayer.NickName, GameTexts.LeftOfMatch), true);
    }

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

    public override Sprite GetCustomIcon(string keyName)
    {
        Sprite spr = null;
        if (customIcons.Exists(x => x.Name == keyName))
        {
            spr = customIcons.Find(x => x.Name == keyName).Icon;
        }
        else { Debug.LogWarning($"自定义图标 {keyName} 未登记在图标列表中。"); }

        return spr;
    }
}
