using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(bl_FirstPersonController))]
public class bl_FirstPersonControllerEditor : Editor
{
    bl_FirstPersonController script;
    public Dictionary<string, AnimBool> animatedBools = new Dictionary<string, AnimBool>()
    {
        {"move", null },  {"jump", null }, {"fall", null }, {"mouse", null }, {"bob", null }, {"sound", null }, {"misc", null }, {"slide", null }
    };

    SerializedProperty moveProp;
    SerializedProperty jumpProp;
    SerializedProperty fallProp;
    SerializedProperty mouseProp;
    SerializedProperty bobProp;
    SerializedProperty soundProp;
    SerializedProperty miscProp;
    SerializedProperty slideProp;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_FirstPersonController)target;

        moveProp = serializedObject.FindProperty("WalkSpeed");
        jumpProp = serializedObject.FindProperty("jumpSpeed");
        fallProp = serializedObject.FindProperty("FallDamage");
        mouseProp = serializedObject.FindProperty("mouseLook");
        bobProp = serializedObject.FindProperty("headBobMagnitude");
        soundProp = serializedObject.FindProperty("footstep");
        miscProp = serializedObject.FindProperty("KeepToCrouch");
        slideProp = serializedObject.FindProperty("canSlide");

        animatedBools["move"] = new AnimBool(moveProp.isExpanded, Repaint);
        animatedBools["jump"] = new AnimBool(jumpProp.isExpanded, Repaint);
        animatedBools["fall"] = new AnimBool(fallProp.isExpanded, Repaint);
        animatedBools["mouse"] = new AnimBool(mouseProp.isExpanded, Repaint);
        animatedBools["bob"] = new AnimBool(bobProp.isExpanded, Repaint);
        animatedBools["sound"] = new AnimBool(soundProp.isExpanded, Repaint);
        animatedBools["misc"] = new AnimBool(miscProp.isExpanded, Repaint);
        animatedBools["slide"] = new AnimBool(slideProp.isExpanded, Repaint);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        MovementSpeeds();
        MouseLookBox();
        JumpBox();
        SlideBox();
        FallBox();
        HeadBobBox();
        MiscBox();
        SoundBox();
    }

    /// <summary>
    /// 
    /// </summary>
    void MovementSpeeds()
    {
        moveProp.isExpanded = animatedBools["move"].target = MFPSEditorStyles.ContainerHeaderFoldout("Speed", moveProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["move"].faded))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("box");
            script.WalkSpeed = EditorGUILayout.Slider("行走速度", script.WalkSpeed, 2, 12);
            script.runSpeed = EditorGUILayout.Slider("奔跑速度", script.runSpeed, script.WalkSpeed, 16);
            script.stealthSpeed = EditorGUILayout.Slider("潜行速度", script.stealthSpeed, 1, 3);
            script.acceleration = EditorGUILayout.Slider("Acceleration", script.acceleration, 1, 30);
            script.crouchSpeed = EditorGUILayout.Slider("蹲行速度", script.crouchSpeed, 1, 8);
            script.crouchTransitionSpeed = EditorGUILayout.Slider("蹲下过渡速度", script.crouchTransitionSpeed, 0.01f, 0.5f);
            script.slideSpeed = EditorGUILayout.Slider("滑铲速度", script.slideSpeed, 10, 20);
            EditorGUILayout.EndVertical();
            EndChangeCheck();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void JumpBox()
    {
        jumpProp.isExpanded = animatedBools["jump"].target = MFPSEditorStyles.ContainerHeaderFoldout("Jump", jumpProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["jump"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.jumpSpeed = EditorGUILayout.Slider("跳跃力度", script.jumpSpeed, -30, 30);
            script.JumpMinRate = EditorGUILayout.Slider("跳跃间隔", script.JumpMinRate, 0.2f, 1.5f);
            script.jumpMomentumBooster = EditorGUILayout.Slider("跳跃动量增幅", script.jumpMomentumBooster, 0.2f, 4.5f);
            script.momentunDecaySpeed = EditorGUILayout.Slider("动量衰减速度", script.momentunDecaySpeed, 0.2f, 12f);
            script.m_GravityMultiplier = EditorGUILayout.Slider("重力倍率", script.m_GravityMultiplier, 0.1f, 5);
            script.m_StickToGroundForce = EditorGUILayout.Slider("贴地力", script.m_StickToGroundForce, 4, 12);
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void SlideBox()
    {
        slideProp.isExpanded = animatedBools["slide"].target = MFPSEditorStyles.ContainerHeaderFoldout("Slide", slideProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["slide"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.canSlide = MFPSEditorStyles.FeatureToogle(r, script.canSlide, "玩家可滑铲");
            script.slideTime = EditorGUILayout.Slider("滑铲时长", script.slideTime, 0.2f, 1.5f);
            script.slideCoolDown = EditorGUILayout.Slider("滑铲冷却", script.slideCoolDown, 0.1f, 2.5f);
            script.slideFriction = EditorGUILayout.Slider("滑铲摩擦", script.slideFriction, 1, 12);
            script.slideCameraTiltAngle = EditorGUILayout.Slider("相机倾斜角度", script.slideCameraTiltAngle, -35, 35);
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void FallBox()
    {
        fallProp.isExpanded = animatedBools["fall"].target = MFPSEditorStyles.ContainerHeaderFoldout("Fall", fallProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["fall"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.FallDamage = MFPSEditorStyles.FeatureToogle(r, script.FallDamage, "坠落伤害");
            script.SafeFallDistance = EditorGUILayout.Slider("安全高度", script.SafeFallDistance, 0.1f, 7f);
            script.DeathFallDistance = EditorGUILayout.Slider("致死高度", script.DeathFallDistance, script.SafeFallDistance, 25);
            script.AirControlMultiplier = EditorGUILayout.Slider("空中控制倍率", script.AirControlMultiplier, 0, 2);
            GUILayout.Space(10);
            GUILayout.Label("掉落", EditorStyles.boldLabel);
            script.dropControlSpeed = EditorGUILayout.Slider("下坠控制速度", script.dropControlSpeed, 15, 40);
            EditorGUILayout.MinMaxSlider("下坠角度速度范围", ref script.dropTiltSpeedRange.x, ref script.dropTiltSpeedRange.y, 10, 75);
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void MouseLookBox()
    {
        mouseProp.isExpanded = animatedBools["mouse"].target = MFPSEditorStyles.ContainerHeaderFoldout("视角控制", mouseProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["mouse"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            if (script.mouseLook == null) script.mouseLook = new MFPS.PlayerController.MouseLook();
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.mouseLook.clampVerticalRotation = MFPSEditorStyles.FeatureToogle(r, script.mouseLook.clampVerticalRotation, "限制垂直旋转");
            if (script.mouseLook.clampVerticalRotation)
            {
                EditorGUILayout.LabelField($"垂直旋转限制 ({script.mouseLook.MinimumX.ToString("0.0")},{script.mouseLook.MaximumX.ToString("0.0")})");
                EditorGUILayout.MinMaxSlider(ref script.mouseLook.MinimumX, ref script.mouseLook.MaximumX, -180, 180);
            }
            r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUILayout.Label("默认灵敏度可在 游戏数据 -> 默认设置 中修改。", EditorStyles.helpBox);
            var prop = serializedObject.FindProperty("headRoot");
            EditorGUI.indentLevel++;
            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, "References");
            if (prop.isExpanded)
            {
                EditorGUILayout.PropertyField(prop);
                script.CameraRoot = EditorGUILayout.ObjectField("相机根节点", script.CameraRoot, typeof(Transform), true) as Transform;
            }
            EditorGUI.indentLevel--;
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void HeadBobBox()
    {
        bobProp.isExpanded = animatedBools["bob"].target = MFPSEditorStyles.ContainerHeaderFoldout("头部晃动", bobProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["bob"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.headBobMagnitude = EditorGUILayout.Slider("头部晃动幅度", script.headBobMagnitude, 0, 1.2f);
            script.headVerticalBobMagnitude = EditorGUILayout.Slider("垂直晃动幅度", script.headVerticalBobMagnitude, 0, 1f);
            if (script.m_JumpBob == null) script.m_JumpBob = new bl_FirstPersonController.LerpControlledBob();
            script.m_JumpBob.BobAmount = EditorGUILayout.Slider("跳跃晃动幅度", script.m_JumpBob.BobAmount, 0.1f, 1);
            script.m_JumpBob.BobDuration = EditorGUILayout.Slider("跳跃晃动时长", script.m_JumpBob.BobDuration, 0.1f, 1);

            GUILayout.Label("头部晃动参数可在 bl_WeaponBob 中修改。", EditorStyles.helpBox);
            if (GUILayout.Button("定位 bl_WeaponBob.cs", EditorStyles.toolbarButton))
            {
                var wb = script.transform.GetComponentInChildren<bl_WeaponBobBase>(true);
                if (wb != null)
                {
                    Selection.activeObject = wb.gameObject;
                    EditorGUIUtility.PingObject(wb.gameObject);
                }
            }
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void SoundBox()
    {
        soundProp.isExpanded = animatedBools["sound"].target = MFPSEditorStyles.ContainerHeaderFoldout("Sounds", soundProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["sound"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.footstep = EditorGUILayout.ObjectField("脚步声控制器", script.footstep, typeof(bl_Footstep), true) as bl_Footstep;
            script.jumpSound = EditorGUILayout.ObjectField("跳跃音效", script.jumpSound, typeof(AudioClip), true) as AudioClip;
            script.landSound = EditorGUILayout.ObjectField("落地音效", script.landSound, typeof(AudioClip), true) as AudioClip;
            script.slideSound = EditorGUILayout.ObjectField("滑铲音效", script.slideSound, typeof(AudioClip), true) as AudioClip;
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void MiscBox()
    {
        miscProp.isExpanded = animatedBools["misc"].target = MFPSEditorStyles.ContainerHeaderFoldout("Misc", miscProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["misc"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.runToAimBehave = (PlayerRunToAimBehave)EditorGUILayout.EnumPopup("奔跑瞄准行为", script.runToAimBehave);

            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.KeepToCrouch = MFPSEditorStyles.FeatureToogle(r, script.KeepToCrouch, "切换蹲下");

            r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.canStealthMode = MFPSEditorStyles.FeatureToogle(r, script.canStealthMode, "可使用潜行模式");

            r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.RunFovEffect = MFPSEditorStyles.FeatureToogle(r, script.RunFovEffect, "冲刺视野效果");

            script.crouchHeight = EditorGUILayout.Slider("蹲下高度", script.crouchHeight, 0.2f, 3);
            if (script.RunFovEffect)
            {
                script.runFOVAmount = EditorGUILayout.Slider("奔跑视野变化量", script.runFOVAmount, 0, 12);
            }
            script.StandIcon = EditorGUILayout.ObjectField("站立图标", script.StandIcon, typeof(Sprite), false) as Sprite;
            script.CrouchIcon = EditorGUILayout.ObjectField("蹲下图标", script.CrouchIcon, typeof(Sprite), false) as Sprite;
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    private void EndChangeCheck()
    {
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}