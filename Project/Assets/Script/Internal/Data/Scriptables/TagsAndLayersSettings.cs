using UnityEngine;

namespace Game.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "TagsAndLayerSettings", menuName = "Game/Settings/TagsAndLayers")]
    public class TagsAndLayersSettings : ScriptableObject
    {
        public LayerMask LocalPlayerHitableLayers;
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