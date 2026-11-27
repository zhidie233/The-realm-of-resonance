using UnityEngine;

namespace Resonance.World
{
    /// <summary>
    /// Shared helper for locating and sampling the scene's DropArea volume.
    /// </summary>
    public static class WorldDropZone
    {
        public const string DropAreaTag = "DropArea";

        public static BoxCollider FindDropArea()
        {
            GameObject dropAreaObject = GameObject.FindGameObjectWithTag(DropAreaTag);
            return dropAreaObject != null ? dropAreaObject.GetComponent<BoxCollider>() : null;
        }

        public static Vector3 SampleWorldPoint(BoxCollider area, float downwardOffset = 0f)
        {
            if (area == null) return Vector3.zero;

            Vector3 localRandomPos = new Vector3(
                Random.Range(-area.size.x * 0.5f, area.size.x * 0.5f),
                Random.Range(-area.size.y * 0.5f, area.size.y * 0.5f),
                Random.Range(-area.size.z * 0.5f, area.size.z * 0.5f));

            Vector3 worldPos = area.transform.TransformPoint(localRandomPos);
            worldPos.y -= downwardOffset;
            return worldPos;
        }
    }
}
