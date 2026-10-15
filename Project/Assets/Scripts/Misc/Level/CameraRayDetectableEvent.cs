﻿using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// This script invoke OnDetected or UnDetected events when the local player is looking at the object
    /// A collider is required and the local player CameraRay.Checking have to be true.
    /// Check CameraRayActiveTrigger.cs
    /// 
    /// How it works?
    /// 
    /// For optimization reasons, the player camera is not detecting (RayCast) every frame.
    /// First you have to let it know when start checking (Firing raycasts)
    /// In order to do that, you have to set the 'Checking' bool of the CameraRay of the local player to = true;
    /// You can do this with a trigger collider that represent the area where the player should start detecting when the player enter in the trigger and 
    /// stop when exit it, check CameraRayActiveTrigger.cs for this implementation,
    /// </summary>
    public class CameraRayDetectableEvent : MonoBehaviour, IRayDetectable
    {
        public GameEvents.UEvent onDetected;
        public GameEvents.UEvent onUndetected;

        /// <summary>
        /// 
        /// </summary>
        public void OnRayDetectedByPlayer()
        {
            onDetected?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnUnDetectedByPlayer()
        {
            onUndetected?.Invoke();
        }
    }
}