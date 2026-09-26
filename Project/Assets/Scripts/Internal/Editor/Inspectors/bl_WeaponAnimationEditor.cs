using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Animations;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using System.Linq;

[CustomEditor(typeof(bl_WeaponAnimation))]
public class bl_WeaponAnimationEditor : Editor
{
    private bl_Gun Gun;
    GunType gType;
    private ReorderableList list;
    private Animator _animator;
    bl_WeaponAnimation script;
    bool allowSceneObjects = false;
    AnimationClip WalkAnim;
    AnimationClip RunAnim;

    private void OnEnable()
    {
        script = (bl_WeaponAnimation)target;
        Gun = script.transform.parent.GetComponent<bl_Gun>();
        gType = bl_GameData.Instance.GetWeapon(Gun.GunID).Type;
        if (script.m_AnimationType == bl_WeaponAnimation.AnimationType.Animator)
        {
            _animator = script.GetComponent<Animator>();
        }
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("FireAnimations"), true, true, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "开火动画");  };
    }

    public override void OnInspectorGUI()
    {
        if(script == null) { script = (bl_WeaponAnimation)target; }
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        allowSceneObjects = !EditorUtility.IsPersistent(script);

        GUILayout.BeginVertical("box");

        GUILayout.BeginVertical("box");
        script.m_AnimationType = (bl_WeaponAnimation.AnimationType)EditorGUILayout.EnumPopup("动画类型", script.m_AnimationType);
        GUILayout.EndVertical();

        if (script.m_AnimationType == bl_WeaponAnimation.AnimationType.Animation)
        {
            AnimationGUI();
        }
        else
        {
            AnimatorGUI();
        }
        GUILayout.EndVertical();
        if (Gun.SoundReloadByAnim && gType != GunType.Knife)
        {
            GUILayout.BeginVertical("box");
            script.Reload_1 = EditorGUILayout.ObjectField("卸弹音效", script.Reload_1, typeof(AudioClip), allowSceneObjects) as AudioClip;
            script.Reload_2 = EditorGUILayout.ObjectField("装弹音效", script.Reload_2, typeof(AudioClip), allowSceneObjects) as AudioClip;
            script.Reload_3 = EditorGUILayout.ObjectField("滑套音效", script.Reload_3, typeof(AudioClip), allowSceneObjects) as AudioClip;
            GUILayout.EndVertical();
        }
        EditorGUI.EndChangeCheck();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
        serializedObject.ApplyModifiedProperties();
    }

    void AnimatorGUI()
    {
        if (_animator == null)
        {
            _animator = script.GetComponent<Animator>();
            if (_animator == null)
            {
                EditorGUILayout.HelpBox("该武器没有 Animator 组件！", MessageType.Warning);
                return;
            }
        }
        if (_animator.runtimeAnimatorController == null)
        {
            EditorGUILayout.HelpBox("该武器的动画器尚未指定。若你已有动画器，请在 Animator 组件中指定；否则可在此创建，" +
                "把动画片段拖入下方对应字段，然后点击 SetUp 按钮", MessageType.Info);

            GUILayout.BeginVertical("box");
            script.DrawName = EditorGUILayout.ObjectField("掏枪动画", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.TakeOut = EditorGUILayout.ObjectField("收枪动画", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.SoloFireClip = EditorGUILayout.ObjectField("开火动画", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireAimAnimation = EditorGUILayout.ObjectField("瞄准开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            if (gType != GunType.Knife)
            {
                if (Gun.reloadPer == bl_Gun.ReloadPer.Bullet)
                {
                    script.StartReloadAnim = EditorGUILayout.ObjectField("开始换弹", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.InsertAnim = EditorGUILayout.ObjectField("插入子弹", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.AfterReloadAnim = EditorGUILayout.ObjectField("换弹后", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                }
                else
                {
                    script.ReloadName = EditorGUILayout.ObjectField("换弹动画", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                }
            }
            else
            {
                script.QuickFireAnim = EditorGUILayout.ObjectField("快速开火动画", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            if (gType == GunType.Grenade || gType == GunType.Launcher)
            {
                script.QuickFireAnim = EditorGUILayout.ObjectField("快速开火动画", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            script.IdleClip = EditorGUILayout.ObjectField("待机动画", script.IdleClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.AnimatedMovements = EditorGUILayout.ToggleLeft("动作自定义动画", script.AnimatedMovements, EditorStyles.toolbarButton);
            GUILayout.Space(4);
            if (script.AnimatedMovements)
            {
                WalkAnim = EditorGUILayout.ObjectField("行走动画", WalkAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                RunAnim = EditorGUILayout.ObjectField("奔跑动画", RunAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            if (GUILayout.Button("设置", EditorStyles.toolbarButton))
            {
                CreateAnimator();
            }
            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginHorizontal("box");
            script.DrawName = EditorGUILayout.ObjectField("掏枪动画", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.DrawSpeed = EditorGUILayout.Slider(script.DrawSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.TakeOut = EditorGUILayout.ObjectField("收枪动画", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.HideSpeed = EditorGUILayout.Slider(script.HideSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            if (gType == GunType.Machinegun || gType == GunType.Pistol || gType == GunType.Burst)
            {
                script.fireBlendMethod = (bl_WeaponAnimation.FireBlendMethod)EditorGUILayout.EnumPopup("开火融合方式", script.fireBlendMethod);
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("开火动画", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                if(script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeed || script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeedCrossFade)
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("瞄准开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.ReloadName = EditorGUILayout.ObjectField("换弹动画", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            else if (gType == GunType.Shotgun || gType == GunType.Sniper)
            {
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("开火动画", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                script.FireAimAnimation = EditorGUILayout.ObjectField("瞄准开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                if (Gun.reloadPer == bl_Gun.ReloadPer.Bullet)
                {
                    GUILayout.BeginHorizontal("box");
                    script.StartReloadAnim = EditorGUILayout.ObjectField("开始换弹", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.InsertAnim = EditorGUILayout.ObjectField("插入子弹", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.InsertSpeed = EditorGUILayout.Slider(script.InsertSpeed, 0.1f, 3, GUILayout.Width(125));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.AfterReloadAnim = EditorGUILayout.ObjectField("结束换弹", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal("box");
                    script.ReloadName = EditorGUILayout.ObjectField("换弹动画", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                }
            }
            else if (gType == GunType.Grenade || gType == GunType.Launcher)
            {
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("开火动画", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.QuickFireAnim = EditorGUILayout.ObjectField("快速开火动画", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.ReloadName = EditorGUILayout.ObjectField("换弹动画", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                script.HasParticles = EditorGUILayout.ToggleLeft("使用粒子", script.HasParticles, EditorStyles.toolbarPopup);
                if (script.HasParticles)
                {
                    script.ParticleRate = EditorGUILayout.Slider("粒子生成速率", script.ParticleRate, 0.1f, 10);
                    var prop = serializedObject.FindProperty("Particles");
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(prop, true);
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.Space(2);
                script.DrawAfterFire = EditorGUILayout.ToggleLeft("开火后绘制", script.DrawAfterFire, EditorStyles.toolbarButton);
            }
            else if (gType == GunType.Knife)
            {
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.QuickFireAnim = EditorGUILayout.ObjectField("快速开火动画", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal("box");
            script.IdleClip = EditorGUILayout.ObjectField("待机动画", script.IdleClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            script.AnimatedMovements = EditorGUILayout.ToggleLeft("动作自定义动画", script.AnimatedMovements, EditorStyles.toolbarButton);
            GUILayout.EndHorizontal();
        }
    }

    void CreateAnimator()
    {
        string lastFolder = PlayerPrefs.GetString("mfpseditor.wanimator.save", "Assets/");
        string path = EditorUtility.SaveFolderPanel("动画器保存文件夹", lastFolder, script.gameObject.name);
        if (string.IsNullOrEmpty(path)) { Debug.Log("设置已取消"); return; }

        PlayerPrefs.SetString("mfpseditor.wanimator.save", path);

        path += string.Format("/{0}.controller", Gun.gameObject.name);
        string relativepath = "Assets" + path.Substring(Application.dataPath.Length);
        string copyName = string.Format("Assets/Prefabs/Weapons/Animators/FPWeapon [{0}].controller", gType.ToString());

        if (AssetDatabase.CopyAsset(copyName, relativepath))
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath(relativepath, typeof(AnimatorController)) as AnimatorController;
            // Add StateMachines
            var rootStateMachine = controller.layers[0].stateMachine;
            var movementStateMachine = controller.layers[1].stateMachine;

            ChildAnimatorState s = rootStateMachine.states.ToList().Find(x => x.state.name == "Draw");
            s.state.motion = script.DrawName;
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Hide");
            s.state.motion = script.TakeOut;
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Fire");
            s.state.motion = script.SoloFireClip;
            if (gType != GunType.Knife && gType != GunType.Grenade)
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "AimFire");
                s.state.motion = script.FireAimAnimation;
            }
            else
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "QuickFire");
                s.state.motion = script.QuickFireAnim;
            }
            if (gType == GunType.Machinegun || gType == GunType.Pistol || gType == GunType.Burst || gType == GunType.Grenade || gType == GunType.Launcher)
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload");
                s.state.motion = script.ReloadName;
            }
            else if (gType == GunType.Sniper || gType == GunType.Shotgun)
            {
                if (Gun.reloadPer != bl_Gun.ReloadPer.Bullet)
                {
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload");
                    s.state.motion = script.ReloadName;
                }
                else
                {
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "StartReload");
                    s.state.motion = script.StartReloadAnim;
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "Insert");
                    s.state.motion = script.InsertAnim;
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "EndReload");
                    s.state.motion = script.AfterReloadAnim;
                }
            }
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Idle");
            s.state.motion = script.IdleClip;

            if (script.AnimatedMovements)
            {
                 s = rootStateMachine.states.ToList().Find(x => x.state.name == "Run");
                 s.state.motion = RunAnim;

                s = movementStateMachine.states.ToList().Find(x => x.state.name == "Movement");
                var moveBlend = (UnityEditor.Animations.BlendTree)s.state.motion;

                var childs = moveBlend.children;
                childs[0].motion = script.IdleClip;
                childs[1].motion = script.IdleClip;
                childs[2].motion = WalkAnim;
                childs[3].motion = RunAnim;
                moveBlend.children = childs;
            }

            EditorUtility.SetDirty(controller);
            _animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(_animator);
        }
    }

    void AnimationGUI()
    {
        GUILayout.BeginHorizontal("box");
        script.DrawName = EditorGUILayout.ObjectField("掏枪动画", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
        script.DrawSpeed = EditorGUILayout.Slider(script.DrawSpeed, 0.1f, 3, GUILayout.Width(125));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        script.TakeOut = EditorGUILayout.ObjectField("收枪动画", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
        script.HideSpeed = EditorGUILayout.Slider(script.HideSpeed, 0.1f, 3, GUILayout.Width(125));
        GUILayout.EndHorizontal();
        if (gType == GunType.Machinegun || gType == GunType.Pistol || gType == GunType.Burst)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("瞄准开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.ReloadName = EditorGUILayout.ObjectField("换弹动画", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
            list.DoLayoutList();
        }
        else if (gType == GunType.Shotgun || gType == GunType.Sniper)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("瞄准开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            if (Gun.reloadPer == bl_Gun.ReloadPer.Bullet)
            {
                GUILayout.BeginHorizontal("box");
                script.StartReloadAnim = EditorGUILayout.ObjectField("开始换弹", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.InsertAnim = EditorGUILayout.ObjectField("插入子弹", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.InsertSpeed = EditorGUILayout.Slider(script.InsertSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.AfterReloadAnim = EditorGUILayout.ObjectField("结束换弹", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal("box");
                script.ReloadName = EditorGUILayout.ObjectField("换弹动画", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            list.DoLayoutList();
        }
        else if (gType == GunType.Grenade || gType == GunType.Launcher)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.QuickFireAnim = EditorGUILayout.ObjectField("快速开火动画", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.ReloadName = EditorGUILayout.ObjectField("换弹动画", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
            script.HasParticles = EditorGUILayout.ToggleLeft("使用粒子", script.HasParticles, EditorStyles.toolbarPopup);
            if (script.HasParticles)
            {
                script.ParticleRate = EditorGUILayout.Slider("粒子生成速率", script.ParticleRate, 0.1f, 10);
                var prop = serializedObject.FindProperty("Particles");
                serializedObject.Update();
                EditorGUILayout.PropertyField(prop, true);
                serializedObject.ApplyModifiedProperties();
            }
        }
        else if (gType == GunType.Knife)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("开火动画", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.QuickFireAnim = EditorGUILayout.ObjectField("快速开火动画", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
        }
    }
}