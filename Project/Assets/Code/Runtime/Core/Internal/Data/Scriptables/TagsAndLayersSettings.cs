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
    [CreateAssetMenu(fileName = "TagsAndLayerSettings", menuName = "项目/Settings/TagsAndLayers")]
    public class TagsAndLayersSettings : ScriptableObject
    {
        public LayerMask LocalPlayerHittableLayers;
        public LayerMask RemotePlayerRootLayer;
        public LayerMask LocalPlayerRootLayer;
        public LayerMask EnvironmentOnly;

        public int GetLocalPlayerLayerIndex()
        {
            return (int)Mathf.Log(LocalPlayerRootLayer, 2);
        }

        public static int GetLayerMaskIndex(LayerMask mask)
        {
            return (int)Mathf.Log(mask.value, 2);
        }
    }
}
