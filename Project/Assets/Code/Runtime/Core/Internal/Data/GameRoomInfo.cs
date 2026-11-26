using System;
using Photon.Realtime;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{
    /// 用于便捷获取所有房间属性
    /// 用法：RoomProperties props = PhotonNetwork.CurrentRoom.GetRoomInfo();
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
        public bool withBots { get; set; }
        public bool autoAssignTeams { get; set; }
        public RoundStyle roundStyle { get; set; }

        public bool isPrivate { get { return !string.IsNullOrEmpty(password); } }

        public GameRoomInfo(Room roomTarget)
        {
            room = roomTarget;
            roomName = room.Name;
            mapName = (string)roomTarget.CustomProperties[NetProtocol.CustomSceneName];
            sceneName = (string)roomTarget.CustomProperties[NetProtocol.SceneNameKey];
            password = (string)roomTarget.CustomProperties[NetProtocol.RoomPassword];
            string gm = (string)room.CustomProperties[NetProtocol.GameModeKey];
            gameMode = (GameMode)Enum.Parse(typeof(GameMode), gm);
            time = (int)room.CustomProperties[NetProtocol.TimeRoomKey];
            goal = (int)room.CustomProperties[NetProtocol.RoomGoal];
            maxPing = (int)room.CustomProperties[NetProtocol.MaxPing];
            maxPlayers = room.MaxPlayers;
            withBots = (bool)room.CustomProperties[NetProtocol.WithBotsKey];
            autoAssignTeams = (bool)room.CustomProperties[NetProtocol.TeamSelectionKey];
            roundStyle = (RoundStyle)room.CustomProperties[NetProtocol.RoomRoundKey];
        }

        public MapInfo GetMapInfo()
        {
            var mapName = sceneName;
            var map = ProjectConfigRoot.Instance.SceneTable.Find(x => x.RealSceneName == mapName);
            return map;
        }
    }
}
