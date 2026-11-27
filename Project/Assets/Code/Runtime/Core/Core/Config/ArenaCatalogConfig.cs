using System.Collections.Generic;
using UnityEngine;
using Resonance.Foundation;
namespace Resonance.Core
{

    [CreateAssetMenu(fileName = "ArenaCatalog", menuName = "项目/Config/Arena Catalog")]
    public class ArenaCatalogConfig : ScriptableObject
    {
        public string MainMenuScene = "MainMenu";
        public string OnDisconnectScene = "MainMenu";
        public Color focusColor = new Color(0.45f, 0f, 1f, 1f);
        public SimpleSceneLoader sceneLoader;
        [Reorderable] public List<MapInfo> SceneTable = new List<MapInfo>();
        [Reorderable] public List<GameModeSettings> gameModeTable = new List<GameModeSettings>();

    #if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < SceneTable.Count; i++)
            {
                if (SceneTable[i].m_Scene == null) continue;
                SceneTable[i].RealSceneName = SceneTable[i].m_Scene.name;
            }
        }
    #endif
    }
}
