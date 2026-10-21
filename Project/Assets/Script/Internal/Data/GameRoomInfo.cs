using System;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// 用于便捷获取所有房间属性
/// 用法：RoomProperties props = PhotonNetwork.CurrentRoom.GetRoomInfo();
/// </summary>
public struct GameRoomInfo 
{
    public string roomName { get; set; }
    public string mapName { get; set; }
    public string sceneName { get; set; }
    public string password { get; set; }
    public GameMode gameMode { get; set; }
    public int goal { get; set; }
    public int time { get; set; }
    public int maxPing { get; set; }
    public Room room { get; set; }
    public int maxPlayers { get; set; }
    public bool friendlyFire { get; set; }
    public bool withBots { get; set; }
    public bool autoAssignTeams { get; set; }
    public RoundStyle roundStyle { get; set; }

    public bool isPrivate { get { return !string.IsNullOrEmpty(password); } }

    public GameRoomInfo(Room roomTarget)
    {
        room = roomTarget;
        roomName = room.Name;
        mapName = (string)roomTarget.CustomProperties[NetworkKeys.CustomSceneName];
        sceneName = (string)roomTarget.CustomProperties[NetworkKeys.SceneNameKey];
        password = (string)roomTarget.CustomProperties[NetworkKeys.RoomPassword];
        string gm = (string)room.CustomProperties[NetworkKeys.GameModeKey];
        gameMode = (GameMode)Enum.Parse(typeof(GameMode), gm);
        time = (int)room.CustomProperties[NetworkKeys.TimeRoomKey];
        goal = (int)room.CustomProperties[NetworkKeys.RoomGoal];
        maxPing = (int)room.CustomProperties[NetworkKeys.MaxPing];
        maxPlayers = room.MaxPlayers;
        friendlyFire = (bool)room.CustomProperties[NetworkKeys.RoomFriendlyFire];
        withBots = (bool)room.CustomProperties[NetworkKeys.WithBotsKey];
        autoAssignTeams = (bool)room.CustomProperties[NetworkKeys.TeamSelectionKey];
        roundStyle = (RoundStyle)room.CustomProperties[NetworkKeys.RoomRoundKey];
    }

    public MapInfo GetMapInfo()
    {
        var mapName = sceneName;
        var map = GameConfig.Instance.SceneTable.Find(x => x.RealSceneName == mapName);
        return map;
    }
}