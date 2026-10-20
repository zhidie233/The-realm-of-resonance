using System;
using System.Collections.Generic;
using UnityEngine;

   [Serializable]
   public class MouseLook : MouseLookBase
   {
       #region Public members
       public bool clampVerticalRotation = true;
       public float MinimumX = -90F;
       public float MaximumX = 90F;
       #endregion

       #region Private members
       private Quaternion m_CharacterTargetRot, m_CameraTargetRot;
       private bool InvertVertical, InvertHorizontal;
       private float verticalRotation, horizontalRotation;
       private float sensitivity, aimSensitivity = 3f;
       private Quaternion extraRotation = Quaternion.identity;
       private Transform m_CameraTransform, m_CharacterBody;
       private List<float> rotArrayX = new List<float>();
       private float rotAverageX;
       private List<float> rotArrayY = new List<float>();
       private float rotAverageY;
       private bool ClampHorizontal = false;
       private Vector2 horizontalClamp = new Vector2(-360, 360);
       private PlayerReferencesHandler playerReferences;
       private MouseLookSettings lookSettings;
       private float tiltValue, tiltTarget = 0;
       private float verticalOffset = 0;
       #endregion

       public float CurrentSensitivity { get; set; } = 3;
       public bool onlyCameraTransform { get; set; } = false;

       /// <summary>
       ///  用角色初始旋转初始化摄像机控制器。
       /// </summary>
       public override void Init(Transform character, Transform camera)
       {
           m_CameraTransform = camera;
           m_CharacterBody = character;
           m_CharacterTargetRot = character.localRotation;
           m_CameraTargetRot = camera.localRotation;
           FetchSettings();
           CurrentSensitivity = sensitivity;
           playerReferences = character.GetComponent<PlayerReferencesHandler>();
           lookSettings = GameConfig.Instance.mouseLookSettings;
       }

       /// <summary>
       /// 根据玩家输入更新角色和摄像机旋转。
       /// </summary>
       public override void UpdateLook(Transform character, Transform camera, ClimbableLadder ladder = null)
       {
           // 使用移动设备或 Unity Remote 时
           if (CommonUtil.isMobile)
           {
           }
           else
           {
               if (!LobbyMenu.Instance.isCursorLocked)
                   return;

               Move(GameInput.MouseX, GameInput.MouseY, character, camera, ladder);
           }
       }

       public void Move(float inputX, float inputY, Transform character, Transform camera, ClimbableLadder ladder)
       {
           if (ladder == null)
           {
               horizontalRotation = inputX * CurrentSensitivity;
               horizontalRotation = (InvertHorizontal) ? (horizontalRotation * -1f) : horizontalRotation;

               if (lookSettings.useSmoothing)
               {
                   rotArrayX.Add(horizontalRotation);
                   if ((float)rotArrayX.Count >= lookSettings.framesOfSmoothing)
                   {
                       rotArrayX.RemoveAt(0);
                   }
                   for (int i = 0; i < rotArrayX.Count; i++)
                   {
                       rotAverageX += rotArrayX[i];
                   }
                   rotAverageX /= rotArrayX.Count;
               }
               else rotAverageX = horizontalRotation;

               m_CharacterTargetRot *= Quaternion.Euler(0f, rotAverageX, 0f);
           }
           else
           {
               Vector3 direction = ladder.GetLookDirection();
               direction.y = 0;
               Quaternion lookRotation = Quaternion.LookRotation(direction);
               m_CharacterTargetRot = Quaternion.Slerp(m_CharacterTargetRot, lookRotation, Time.deltaTime * 5);
           }

           verticalRotation = inputY * CurrentSensitivity;
           verticalRotation = (InvertVertical) ? (verticalRotation * -1f) : verticalRotation;
           if (lookSettings.useSmoothing)
           {
               rotArrayY.Add(verticalRotation);
               if ((float)rotArrayY.Count >= lookSettings.framesOfSmoothing)
               {
                   rotArrayY.RemoveAt(0);
               }
               for (int j = 0; j < rotArrayY.Count; j++)
               {
                   rotAverageY += rotArrayY[j];
               }
               rotAverageY /= rotArrayY.Count;
           }
           else rotAverageY = verticalRotation;

           if (!onlyCameraTransform)
               m_CameraTargetRot *= Quaternion.Euler(-rotAverageY, 0f, 0);
           else
               m_CameraTargetRot *= Quaternion.Euler(-rotAverageY, horizontalRotation, 0);

           if (clampVerticalRotation)
               m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

           if (ClampHorizontal)
           {
               var re = m_CharacterTargetRot.eulerAngles;
               re.y = MathHelpers.Clamp360Angle(re.y, horizontalClamp.x, horizontalClamp.y);
               m_CharacterTargetRot = Quaternion.Euler(re);
           }

           if (lookSettings.lerpMovement)
           {
               if (character != null && !onlyCameraTransform) { character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, lookSettings.smoothTime * Time.deltaTime); }
               if (camera != null) { camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot * extraRotation, lookSettings.smoothTime * Time.deltaTime); }
           }
           else
           {
               if (!onlyCameraTransform)
               {
                   character.localRotation = m_CharacterTargetRot;
               }
               else
               {
                   var fixEuler = m_CameraTargetRot.eulerAngles;
                   fixEuler.z = 0;
                   m_CameraTargetRot = Quaternion.Euler(fixEuler);
               }
               camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot * extraRotation, 1);
           }
       }

       public void Update()
       {
           tiltValue = Mathf.Lerp(tiltValue, tiltTarget, Time.deltaTime * 4);
           extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);
       }

       public override void SetVerticalOffset(float amount)
       {
           verticalOffset = amount;
           extraRotation = Quaternion.Euler(amount, 0, tiltValue);
       }

       public override void CombineVerticalOffset()
       {
           m_CameraTargetRot *= extraRotation;
           verticalOffset = 0;
           extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);

           if (clampVerticalRotation)
           {
               m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
           }
       }

       /// <summary>
       /// 只旋转摄像机/头部，不旋转角色身体
       /// </summary>
       public void UseOnlyCameraRotation()
       {
           onlyCameraTransform = true;
       }

       /// <summary>
       /// 把当前摄像机旋转拆分为身体的水平旋转和头部的垂直旋转
       /// 身体负责水平旋转，摄像机/头部负责垂直旋转
       /// 只应在之前用过 OnlyCameraRotation 时调用。
       /// </summary>
       public void PortBodyOrientationToCamera()
       {
           onlyCameraTransform = false;
           Vector3 direction = Vector3.zero;
           direction.y = m_CameraTransform.eulerAngles.y;
           m_CharacterBody.eulerAngles = direction;

           direction = Vector3.zero;
           direction.x = m_CameraTransform.localEulerAngles.x;
           m_CameraTransform.localEulerAngles = direction;

           m_CharacterTargetRot = m_CharacterBody.localRotation;
           m_CameraTargetRot = m_CameraTransform.localRotation;

           m_CameraTransform.localRotation *= extraRotation;
       }

       /// <summary>
       /// 强制角色注视世界中的某个位置。
       /// </summary>
       public override void LookAt(Transform reference, bool extrapolate = true)
       {
           m_CharacterTargetRot = Quaternion.Euler(0f, reference.eulerAngles.y, 0f);
           Quaternion relative = Quaternion.Inverse(Quaternion.identity) * reference.rotation;
           m_CameraTargetRot = Quaternion.Euler(relative.eulerAngles.x, 0, 0);

           if (extrapolate)
           {
               m_CharacterBody.localRotation = m_CharacterTargetRot;
               m_CameraTransform.localRotation = m_CameraTargetRot;
           }
       }

       public void LookAt(Vector3 direction, bool extrapolate = true, float influence = 1)
       {
           Quaternion rotate = Quaternion.LookRotation(direction);
           rotate = Quaternion.Slerp(m_CameraTransform.rotation, rotate, influence);

           m_CharacterTargetRot = Quaternion.Euler(0f, rotate.eulerAngles.y, 0f);
           m_CameraTargetRot = Quaternion.Euler(rotate.eulerAngles.x, 0, 0);

           if (extrapolate)
           {
               m_CharacterBody.localRotation = m_CharacterTargetRot;
               m_CameraTransform.localRotation = m_CameraTargetRot;
           }
       }

       public override void SetTiltAngle(float angle)
       {
           tiltTarget = angle;
       }

       public void FetchSettings()
       {
           sensitivity = (float)GameCore.Settings.GetSettingOf("灵敏度");
           aimSensitivity = (float)GameCore.Settings.GetSettingOf("瞄准灵敏度");
           InvertHorizontal = (bool)GameCore.Settings.GetSettingOf("倒置水平视角");
           InvertVertical = (bool)GameCore.Settings.GetSettingOf("倒置垂直视角");
           CurrentSensitivity = sensitivity;
       }

       public void OnAimChange(bool isAiming)
       {
           CurrentSensitivity = isAiming ? sensitivity * aimSensitivity : sensitivity;

           if (isAiming && lookSettings.aimSensitivityAdjust == MouseLookSettings.AimSensitivityAdjust.Relative)
               AdjustSensitivityBasedOnFOV();
       }

       /// <summary>
       /// 根据摄像机视场角调整鼠标灵敏度。
       /// </summary>
       public void AdjustSensitivityBasedOnFOV()
       {
           if (playerReferences == null) return;

           float fovPercentage = playerReferences.playerCamera.fieldOfView / playerReferences.DefaultCameraFOV;
           CurrentSensitivity = CurrentSensitivity * fovPercentage;
       }

       public override void ClampHorizontalRotation(float min, float max)
       {
           horizontalClamp = new Vector2(min, max);
           ClampHorizontal = true;
       }

       public override void UnClampHorizontal() => ClampHorizontal = false;

       public float VerticalAngle => m_CameraTransform.localEulerAngles.x;

       Quaternion ClampRotationAroundXAxis(Quaternion q) => MathHelpers.ClampRotationAroundAxis(q, MinimumX, MaximumX, UnityEngine.Animations.Axis.X);
       Quaternion ClampRotationAroundYAxis(Quaternion q) => MathHelpers.ClampRotationAroundAxis(q, horizontalClamp.x, horizontalClamp.y, UnityEngine.Animations.Axis.Y);

       public override Vector2 HorizontalLimits { get => horizontalClamp; set => horizontalClamp = value; }
       public override Vector2 VerticalLimits
       {
           get => new Vector2(MinimumX, MaximumX);
           set
           {
               MinimumX = value.x;
               MaximumX = value.y;
           }
       }
   }