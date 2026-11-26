using System;
using System.Collections.Generic;
using UnityEngine;
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

    [CreateAssetMenu(fileName = "FactionRoster", menuName = "项目/Config/Faction Roster")]
    public class FactionRosterConfig : ScriptableObject
    {
        [Serializable]
        public class GameTeamInfo
        {
            public string UserName;
            public Role m_Role = Role.Moderator;
            public string Password;
            public Color m_Color;
            public enum Role
            {
                Admin = 0,
                Moderator = 1,
            }
        }

        [Header("阵营")]
        public string Team1Name = "Team1";
        public Color Team1Color = Color.blue;
        public string Team2Name = "Team2";
        public Color Team2Color = Color.green;

        [Header("玩家预制体")]
        public PlayerNetworkHandler Team1PlayerPrefab;
        public PlayerNetworkHandler Team2PlayerPrefab;

        [Header("机器人预制体")]
        public AIBotCore BotTeam1;
        public AIBotCore BotTeam2;
        public AIBotCore BotTeam3;

        [Header("后台账号")]
        public List<GameTeamInfo> GameTeam = new List<GameTeamInfo>();
    }
}
