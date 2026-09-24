using System;
using System.Collections.Generic;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public abstract class bl_KillFeedBase : bl_PhotonHelper
{

    public List<CustomIcons> customIcons = new List<CustomIcons>();

    public struct FeedData
    {
        public string LeftText;
        public string CenterText;
        public string RightText;
        public Team Team;
        public Dictionary<string, object> Data;

        public void AddData(string key, object value)
        {
            if (Data == null) Data = new Dictionary<string, object>();
            Data.Add(key, value);
        }
    }

    [Serializable]
    public class CustomIcons
    {
        public string Name;
        public Sprite Icon;
    }

    public abstract void SendKillMessageEvent(FeedData feedData);

    // <param name="message"></param>
    // <param name="localOnly"></param>
    public abstract void SendMessageEvent(string message, bool localOnly = false);

    // <param name="teamHighlightMessage"></param>
    // <param name="normalMessage"></param>
    // <param name="playerTeam"></param>
    public abstract void SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam);

    // <param name="data"></param>
    public abstract void OnMessageReceive(HashTable data);

    // <param name="keyName"></param>
    // <param name="icon"></param>
    public abstract void AddCustomIcon(string keyName, Sprite icon);

    // <param name="keyName"></param>
    // <returns></returns>
    public abstract int GetCustomIconIndex(string keyName);

    // <param name="index"></param>
    // <returns></returns>
    public abstract CustomIcons GetCustomIconByIndex(int index);

    // <param name="keyName"></param>
    // <returns></returns>
    public abstract Sprite GetCustomIcon(string keyName);

    private static bl_KillFeedBase _instance;
    public static bl_KillFeedBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_KillFeedBase>(); }
            return _instance;
        }
    }
}