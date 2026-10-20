using UnityEngine;

   public abstract class BulletMarkBase : MonoBehaviour
   {
       public abstract BulletMarkBase Init(Transform parent);

       public abstract BulletMarkBase SetDecalMaterial(Material mat);

       public abstract BulletMarkBase SetToHit(RaycastHit hit, bool asPendingParent = false);

       public abstract BulletMarkBase SetScaleVariation(Vector3 scaleBase, Vector2 Range);

       public abstract void BackToOrigin();
   }