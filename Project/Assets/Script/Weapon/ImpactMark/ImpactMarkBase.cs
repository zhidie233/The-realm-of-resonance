using UnityEngine;

namespace Game.Runtime.Level
{
    public abstract class ImpactMarkBase : MonoBehaviour
    {
        public abstract ImpactMarkBase Init(Transform parent);

        public abstract ImpactMarkBase SetDecalMaterial(Material mat);

        public abstract ImpactMarkBase SetToHit(RaycastHit hit, bool asPendingParent = false);

        public abstract ImpactMarkBase SetScaleVariation(Vector3 scaleBase, Vector2 Range);

        public abstract void BackToOrigin();
    }
}