﻿using System.Collections;
using UnityEngine;
using GameEditor;
using Game.Internal.Scriptables;

namespace Game.Runtime.Level
{
    /// <summary>
    /// This is the default Basic Door of Game
    /// If you want to create a custom door, create a new script and inherited from <see cref="DoorBase"/>
    /// Use this only as reference.
    /// </summary>
    public class BasicDoor : DoorBase
    {
        [SerializeField] private Transform doorPivot = null;
        [ScriptableDrawer] public DoorStateSettings doorSettings;

        /// <summary>
        /// This is not called automatically
        /// it should be invoked manually
        /// You can use CameraRayDetectableEvent.cs
        /// </summary>
        public void OnRayDetectedByPlayer()
        {
            DoorManager.AddUpdateDoor(this);

            string doorAction = DoorState == State.Close ? GameTexts.OpenDoor.Localized(207) : GameTexts.CloseDoor.Localized(208);
            string inputName = UtilityHelper.isMobile ? GameTexts.Touch : InputHandler.GetButtonName("Interact");
            InputInteractionIndicator.ShowIndication(inputName, doorAction, () =>
            {
                SwithState();
            });
        }

        /// <summary>
        /// This is not called automatically
        /// it should be invoked manually
        /// You can use CameraRayDetectableEvent.cs
        /// </summary>
        public void OnUnDetectedByPlayer()
        {
            DoorManager.RemoveUpdateDoor(this);
            InputInteractionIndicator.SetActive(false);
        }

        /// <summary>
        /// Call each frame only when the local player is looking at this door.
        /// </summary>
        public override void OnUpdateDoor()
        {
            if (GameInput.Interact())
            {
                SwithState();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void SwithState()
        {
            var state = DoorState;
            if(state != State.Close)
            {
                state = State.Close;
            }
            else
            {
                state = GetOpenDirection();
            }

            DoorManager.SentDoorStateToAll(this, state);
        }

        /// <summary>
        /// Called from server when a player change the door state
        /// </summary>
        public override void SetDoorState(State newState)
        {
            DoorState = newState;

            PlayAudioBasedOnState();
            DoTransitionToState();
        }

        /// <summary>
        /// Get open direction based on the local player view direction
        /// </summary>
        /// <returns></returns>
        private State GetOpenDirection()
        {
            var player = GameCore.LocalPlayerReferences;
            if (player == null) return State.Close;

            Vector3 dir = transform.position + player.Transform.forward;
            Vector3 lhs = dir - transform.position;
            float dot = Vector3.Dot(lhs, transform.forward);

            if (dot > 0.1f) return State.OpenToOutside;
            return State.OpenToInside;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newState"></param>
        public override void SetDoorStateInstantly(State newState)
        {
            DoorState = newState;

            if (doorPivot == null || doorSettings == null) return;

            if (newState == State.Close) doorPivot.localEulerAngles = doorSettings.CloseRotation;
            else if (newState == State.OpenToInside) doorPivot.localEulerAngles = doorSettings.OpenInsideRotation;
            else if (newState == State.OpenToOutside) doorPivot.localEulerAngles = doorSettings.OpenOutsideRotation;
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlayAudioBasedOnState()
        {
            if (doorSettings == null || AudioSource == null) return;

            var clip = doorSettings.CloseSound;
            if (DoorState != State.Close) clip = doorSettings.OpenSound;

            AudioSource.clip = clip;
            AudioSource.Play();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DoTransitionToState()
        {
            StopAllCoroutines();
            StartCoroutine(DoMotionToState());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoMotionToState()
        {
            if (doorSettings == null) yield break;

            Vector3 targetDir = doorSettings.CloseRotation;
            if (DoorState == State.OpenToInside) targetDir = doorSettings.OpenInsideRotation;
            else if (DoorState == State.OpenToOutside) targetDir = doorSettings.OpenOutsideRotation;

            Quaternion origin = doorPivot.localRotation;
            Quaternion target = Quaternion.Euler(targetDir);
            float d = 0;
            float t;
            while(d <= 1)
            {
                d += Time.deltaTime / doorSettings.TransitionDuration;
                t = doorSettings.TransitionEasing.Evaluate(d);
                doorPivot.localRotation = Quaternion.Slerp(origin, target, t);
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (doorPivot == null) return;

            Gizmos.DrawWireSphere(doorPivot.position, 0.1f);
            Gizmos.DrawLine(doorPivot.position - doorPivot.up, doorPivot.position + doorPivot.up);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        }

        private AudioSource _audioSource = null;
        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }
    }
}