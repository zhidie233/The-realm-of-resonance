using System.Collections;
using UnityEngine;

   /// <summary>
   /// 这是游戏默认的基础门
   /// 若要自定义门，新建脚本并从 <see cref="DoorCore"/> 继承
   /// 此脚本仅作参考。
   /// </summary>
   public class SimpleDoor : DoorCore
   {
       [SerializeField] private Transform doorPivot = null;
       [ScriptableDrawer] public DoorStateConfig doorSettings;

       /// <summary>
       /// 此方法不会自动调用
       /// 需要手动调用
       /// 可以使用 ViewRayDetectableEvent.cs
       /// </summary>
       public void OnRayDetectedByPlayer()
       {
           DoorCoordinator.AddUpdateDoor(this);

           string doorAction = DoorState == State.Close ? GameTexts.OpenDoor : GameTexts.CloseDoor;
           string inputName = CommonUtil.isMobile ? GameTexts.Touch : InputHandler.GetButtonName("Interact");
           InputInteractionIndicator.ShowIndication(inputName, doorAction, () =>
           {
               SwithState();
           });
       }

       /// <summary>
       /// 此方法不会自动调用
       /// 需要手动调用
       /// 可以使用 ViewRayDetectableEvent.cs
       /// </summary>
       public void OnUnDetectedByPlayer()
       {
           DoorCoordinator.RemoveUpdateDoor(this);
           InputInteractionIndicator.SetActive(false);
       }

       /// <summary>
       /// 仅在本地玩家注视此门时每帧调用。
       /// </summary>
       public override void OnUpdateDoor()
       {
           if (GameInput.Interact())
           {
               SwithState();
           }
       }

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

           DoorCoordinator.SentDoorStateToAll(this, state);
       }

       /// <summary>
       /// 玩家更改门状态时由服务器调用
       /// </summary>
       public override void SetDoorState(State newState)
       {
           DoorState = newState;

           PlayAudioBasedOnState();
           DoTransitionToState();
       }

       /// <summary>
       /// 根据本地玩家视角方向获取开门方向
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

       public override void SetDoorStateInstantly(State newState)
       {
           DoorState = newState;

           if (doorPivot == null || doorSettings == null) return;

           if (newState == State.Close) doorPivot.localEulerAngles = doorSettings.CloseRotation;
           else if (newState == State.OpenToInside) doorPivot.localEulerAngles = doorSettings.OpenInsideRotation;
           else if (newState == State.OpenToOutside) doorPivot.localEulerAngles = doorSettings.OpenOutsideRotation;
       }

       public void PlayAudioBasedOnState()
       {
           if (doorSettings == null || AudioSource == null) return;

           var clip = doorSettings.CloseSound;
           if (DoorState != State.Close) clip = doorSettings.OpenSound;

           AudioSource.clip = clip;
           AudioSource.Play();
       }

       public void DoTransitionToState()
       {
           StopAllCoroutines();
           StartCoroutine(DoMotionToState());
       }

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