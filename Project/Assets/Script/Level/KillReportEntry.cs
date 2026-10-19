using System;
using System.Collections.Generic;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public abstract class KillReportEntry : NetworkHelper
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

    public abstract void SendMessageEvent(string message, bool localOnly = false);

    public abstract void SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam);

    public abstract void OnMessageReceive(HashTable data);

    public abstract void AddCustomIcon(string keyName, Sprite icon);

    public abstract int GetCustomIconIndex(string keyName);

    public abstract CustomIcons GetCustomIconByIndex(int index);

    public abstract Sprite GetCustomIcon(string keyName);

    private static KillReportEntry _instance;
    public static KillReportEntry Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<KillReportEntry>(); }
            return _instance;
        }
    }
}