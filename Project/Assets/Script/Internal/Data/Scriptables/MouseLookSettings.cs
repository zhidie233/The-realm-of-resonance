using System.Collections.Generic;
using UnityEngine;
   [CreateAssetMenu(fileName = "Mouse Settings", menuName = "项目/Camera/Mouse Settings")]
   public class MouseLookSettings : ScriptableObject
   {
       [GameToggle] public bool useSmoothing = true;
       [Range(2, 12)] public float framesOfSmoothing = 5f;
       [GameToggle] public bool lerpMovement = false;
       [Range(2,12)] public float smoothTime = 5f;
       [Tooltip("Relative：随玩家相机视野变化\nFixed：使用固定值")]
       public AimSensitivityAdjust aimSensitivityAdjust = AimSensitivityAdjust.Relative;
       public Texture2D customCursor;

       [System.Serializable]
       public enum AimSensitivityAdjust
       {
           Relative,
           Fixed
       }
   }
