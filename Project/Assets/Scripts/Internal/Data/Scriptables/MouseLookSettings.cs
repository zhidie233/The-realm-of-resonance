using System.Collections.Generic;
using UnityEngine;

namespace GFWK.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Mouse Settings", menuName = "Game Framework/Camera/Mouse Settings")]
    public class MouseLookSettings : ScriptableObject
    {
        [GFWorksToogle] public bool useSmoothing = true;
        [Range(2, 12)] public float framesOfSmoothing = 5f;
        [GFWorksToogle] public bool lerpMovement = false;
        [Range(2,12)] public float smoothTime = 5f;
        [Tooltip("Relative: To the player camera field of view\nFixed: to a fixed value")]
        public AimSensitivityAdjust aimSensitivityAdjust = AimSensitivityAdjust.Relative;
        public Texture2D customCursor;

        [System.Serializable]
        public enum AimSensitivityAdjust
        {
            Relative,
            Fixed
        }
    }
}