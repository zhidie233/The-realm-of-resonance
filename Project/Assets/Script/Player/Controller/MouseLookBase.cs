using UnityEngine;

   public abstract class MouseLookBase
   {

       public abstract Vector2 HorizontalLimits
       {
           get;
           set;
       }

       public abstract Vector2 VerticalLimits
       {
           get;
           set;
       }

       public abstract void Init(Transform character, Transform camera);

       public abstract void UpdateLook(Transform character, Transform camera, ClimbableLadder ladder = null);

       public abstract void LookAt(Transform reference, bool extrapolate = true);

       public abstract void SetTiltAngle(float angle);

       public abstract void ClampHorizontalRotation(float min, float max);

       public abstract void UnClampHorizontal();

       public abstract void SetVerticalOffset(float value);

       public abstract void CombineVerticalOffset();

       public void SetVerticalClamp(bool minimun, float value)
       {
           var v = VerticalLimits;
           if (minimun) v.x = value;
           else v.y = value;

           VerticalLimits = v;
       }
   }