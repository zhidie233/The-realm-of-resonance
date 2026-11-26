using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
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

    [Serializable]
    public class MapInfo
    {
        public string ShowName;
        [SerializeField]
        public Object m_Scene;
        [HideInInspector] public string RealSceneName;
        [SpritePreview] public Sprite Preview;
        public List<GameMode> BlockedModes = new List<GameMode>();
        /// 从给定模式列表中返回此地图允许的游戏模式。
        public GameModeSettings[] GetAllowedGameModes(GameModeSettings[] allAvailables)
        {
            var list = new List<GameModeSettings>();
            for (int i = 0; i < allAvailables.Length; i++)
            {
                if (!BlockedModes.Contains(allAvailables[i].gameMode))
                {
                    list.Add(allAvailables[i]);
                }
            }
            return list.ToArray();
        }
    }
}
