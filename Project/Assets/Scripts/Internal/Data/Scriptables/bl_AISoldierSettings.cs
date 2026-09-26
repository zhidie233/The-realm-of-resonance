using System.Collections.Generic;
using UnityEngine;

namespace GFWK.Runtime.AI
{
    [CreateAssetMenu(fileName = "AI Soldier Settings", menuName = "Game Framework/AI/Soldier Settings")]
    public class bl_AISoldierSettings : ScriptableObject
    {
        [Header("Speeds")]
        public float walkSpeed = 4;
        public float runSpeed = 8;
        public float crounchSpeed = 2;
        public float rotationSmoothing = 6.0f;

        [Header("Ranges")]
        public float closeRange = 10.0f;
        public float mediumRange = 25.0f;
        public float farRange = 20f;
        public float limitRange = 50f;
        
        [Header("Height Perception")]
        public float maxHeightDifference = 500.0f; // 最大可跨越高度差
        public float stepHeight = 0.5f; // 台阶高度
        public float slopeLimit = 45f; // 最大坡度
    }
}