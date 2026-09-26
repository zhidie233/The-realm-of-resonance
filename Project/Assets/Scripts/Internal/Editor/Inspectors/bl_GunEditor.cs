using MFPS.Core.Motion;
using MFPSEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(bl_Gun))]
public class bl_GunEditor : Editor
{

    private ReorderableList list;
    private bl_GameData GameData;
    private bl_Gun script;
    bool allowSceneObjects;
    private GameObject AimReference;
    bl_PlayerReferences playerReferences;
    bl_GunManager GunManager;
    Texture2D aimIcon;
    private int oldID = -1;

    private void OnEnable()
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(Dependency.GOListPropiertie), true, true, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "无弹药时禁用"); };
        GameData = bl_GameData.Instance;
        script = (bl_Gun)target;
        playerReferences = script.transform.GetComponentInParent<bl_PlayerReferences>();
        if (playerReferences != null) { AimReference = playerReferences.playerSettings.AimPositionReference; }
        aimIcon = Resources.Load("content/Images/editor-aim-icon", typeof(Texture2D)) as Texture2D;
        if (script != null)
        {
            GunManager = script.transform.parent.GetComponent<bl_GunManager>();
            script.weaponRenders = script.transform.GetComponentsInChildren<Renderer>();
            if (script.playerSettings == null) { script.playerSettings = script.transform.root.GetComponent<bl_PlayerSettings>(); }
        }

        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        allowSceneObjects = !EditorUtility.IsPersistent(script);
        EditorGUI.BeginChangeCheck();
        DrawGlobalSettings();
        EditorGUILayout.BeginVertical("box");
        if (script.Info.Type == GunType.Machinegun || script.Info.Type == GunType.Pistol || script.Info.Type == GunType.Sniper)
        {
            DrawSeparator("瞄准设置");
            DrawAimSettings();
            EditorGUILayout.Space();
            DrawSeparator("References");
            EditorGUILayout.BeginVertical("box");
            script.muzzlePoint = EditorGUILayout.ObjectField("开火点", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("武器特效", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("枪口火焰", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as UnityEngine.ParticleSystem;
                script.shell = EditorGUILayout.ObjectField("弹壳", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            }
            EditorGUILayout.EndVertical();

            DrawSeparator("Settings");
            EditorGUILayout.BeginVertical("box");
            script.BulletName = EditorGUILayout.TextField("子弹", script.BulletName, EditorStyles.helpBox);
            script.bulletSpeed = EditorGUILayout.FloatField("子弹速度", script.bulletSpeed);
            script.bulletDropFactor = EditorGUILayout.Slider("子弹下坠系数", script.bulletDropFactor, 0, 10);
            script.impactForce = EditorGUILayout.IntSlider("冲击力", script.impactForce, 0, 30);
            script.delayFireOnSprinting = EditorGUILayout.Slider("冲刺后首发延迟", script.delayFireOnSprinting, 0, 1);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawSpreadSettings();
            EditorGUILayout.EndVertical();

            DrawAudioSettings();

        }
        else
        if (script.Info.Type == GunType.Burst)
        {
            DrawSeparator("枪械设置");
            DrawAimSettings();
            DrawSeparator("References");
            script.muzzlePoint = EditorGUILayout.ObjectField("开火点", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("武器特效", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("枪口火焰", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                script.shell = EditorGUILayout.ObjectField("弹壳", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            }
            DrawSeparator("Settings");
            script.BulletName = EditorGUILayout.TextField("子弹", script.BulletName, EditorStyles.helpBox);
            script.roundsPerBurst = EditorGUILayout.IntSlider("每轮点射发数", script.roundsPerBurst, 1, 10);
            script.lagBetweenBurst = EditorGUILayout.Slider("点射间隔", script.lagBetweenBurst, 0.01f, 5.0f);
            script.bulletSpeed = EditorGUILayout.FloatField("子弹速度", script.bulletSpeed);
            script.bulletDropFactor = EditorGUILayout.Slider("子弹下坠系数", script.bulletDropFactor, 0, 10);
            script.impactForce = EditorGUILayout.IntField("冲击力", script.impactForce);
            script.delayFireOnSprinting = EditorGUILayout.Slider("冲刺后首发延迟", script.delayFireOnSprinting, 0, 1);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawSpreadSettings();
            DrawAudioSettings();
        }
        else
        if (script.Info.Type == GunType.Shotgun)
        {
            DrawSeparator("霰弹枪设置");
            DrawAimSettings();
            DrawSeparator("References");
            script.muzzlePoint = EditorGUILayout.ObjectField("开火点", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("武器特效", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("枪口火焰", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                script.shell = EditorGUILayout.ObjectField("弹壳", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            }
            DrawSeparator("Settings");
            script.BulletName = EditorGUILayout.TextField("子弹", script.BulletName, EditorStyles.helpBox);
            script.pelletsPerShot = EditorGUILayout.IntSlider("每次射击弹丸数", script.pelletsPerShot, 1, 10);
            script.bulletSpeed = EditorGUILayout.FloatField("子弹速度", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntField("冲击力", script.impactForce);
            script.delayFireOnSprinting = EditorGUILayout.Slider("冲刺后首发延迟", script.delayFireOnSprinting, 0, 1);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawSpreadSettings();
            DrawAudioSettings();
        }
        else
        if (script.Info.Type == GunType.Grenade)
        {
            DrawSeparator("手雷设置");
            EditorGUILayout.Space();
            script.grenade = EditorGUILayout.ObjectField("手雷", script.grenade, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            script.muzzlePoint = EditorGUILayout.ObjectField("开火点", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            DrawSeparator("Settings");
            script.ThrowByAnimation = EditorGUILayout.ToggleLeft("按动画事件投掷", script.ThrowByAnimation, EditorStyles.toolbarButton);
            script.canBeTakenWhenIsEmpty = EditorGUILayout.ToggleLeft("空仓时可被拾取", script.canBeTakenWhenIsEmpty, EditorStyles.toolbarButton);
            GUILayout.Space(2);
            if (!script.ThrowByAnimation)
            {
                script.DelayFire = EditorGUILayout.FloatField("开火延迟", script.DelayFire);
            }
            script.bulletSpeed = EditorGUILayout.FloatField("投射物速度", script.bulletSpeed);
            script.bulletDropFactor = EditorGUILayout.FloatField("向上力度", script.bulletDropFactor);
            script.impactForce = EditorGUILayout.IntField("冲击力", script.impactForce);
            script.m_AllowQuickFire = EditorGUILayout.ToggleLeft("允许快速开火", script.m_AllowQuickFire, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            list.DoLayoutList();
            DrawAudioSettings();

        }
        else if (script.Info.Type == GunType.Launcher)
        {
            DrawAimSettings();
            DrawSeparator("发射器设置");
            DrawWeaponBinding();
            EditorGUILayout.Space();
            DrawBullet();
            script.muzzlePoint = EditorGUILayout.ObjectField("开火点", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("武器特效", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("开火特效", script.muzzleFlash, typeof(ParticleSystem), true) as ParticleSystem;
            }
            script.bulletSpeed = EditorGUILayout.FloatField("投射物力度", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntField("冲击力", script.impactForce);
            script.delayFireOnSprinting = EditorGUILayout.Slider("冲刺后首发延迟", script.delayFireOnSprinting, 0, 1);
            script.m_AllowQuickFire = EditorGUILayout.ToggleLeft("允许快速开火", script.m_AllowQuickFire, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAudioSettings();
        }
        if (script.Info.Type == GunType.Knife)
        {
            DrawSeparator("匕首设置");
            EditorGUILayout.Space();
            script.BulletName = EditorGUILayout.TextField("子弹", script.BulletName, EditorStyles.helpBox);
            script.impactEffect = EditorGUILayout.ObjectField("命中特效", script.impactEffect, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            DrawSeparator("Settings");
            script.bulletSpeed = EditorGUILayout.FloatField("射线速度", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntSlider("冲击力", script.impactForce, 0, 30);
            script.m_AllowQuickFire = EditorGUILayout.ToggleLeft("允许快速开火", script.m_AllowQuickFire, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            DrawSeparator("音频设置");
            EditorGUILayout.BeginVertical("box");
            script.FireSound = EditorGUILayout.ObjectField("开火音效", script.FireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.TakeSound = EditorGUILayout.ObjectField("拾取音效", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        if (GunManager != null && !GunManager.AllGuns.Contains(script))
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("这把武器尚未加入 bl_GunManager 列表，现在添加？", MessageType.Info);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("添加到列表", EditorStyles.toolbarButton))
            {
                if (GunManager != null)
                {
                    GunManager.AllGuns.Add(script);
                    EditorUtility.SetDirty(GunManager);
                }
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void DrawGlobalSettings()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label("武器信息", EditorStyles.toolbarButton);
        if (playerReferences != null)
        {
            GUILayout.Space(2);
            if (GUILayout.Button("游戏数据", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                Selection.activeObject = bl_GameData.Instance;
                EditorGUIUtility.PingObject(bl_GameData.Instance);
            }
            GUILayout.Space(2);
            if (GUILayout.Button("导出", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                EditorWindow.GetWindow<bl_ImportExportWeapon>("导出", true).PrepareToExport(script, playerReferences.playerNetwork);
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        script.GunID = EditorGUILayout.Popup("枪械 ID ", script.GunID, GameData.AllWeaponStringList());
        if (oldID != script.GunID)
        {
            script.Info = null;
            oldID = script.GunID;
        }
        script.Info.Type = bl_GameData.Instance.GetWeapon(script.GunID).Type;
        GunType t = script.Info.Type;
        if (t == GunType.Machinegun || t == GunType.Pistol || t == GunType.Burst)
        {
            EditorGUILayout.BeginHorizontal("box");
            int w = ((int)EditorGUIUtility.currentViewWidth / 3) - 25;
            GUI.enabled = t != GunType.Machinegun;
            script.CanAuto = EditorGUILayout.ToggleLeft("Auto", script.CanAuto, GUILayout.Width(w));
            GUI.enabled = t != GunType.Burst;
            script.CanSemi = EditorGUILayout.ToggleLeft("Semi", script.CanSemi, GUILayout.Width(w));
            GUI.enabled = t != GunType.Pistol;
            script.CanSingle = EditorGUILayout.ToggleLeft("Single", script.CanSingle, GUILayout.Width(w));
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        script.CrossHairScale = EditorGUILayout.Slider("准星缩放： ", script.CrossHairScale, 1, 30);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawAimSettings()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        script.AimPosition = EditorGUILayout.Vector3Field("瞄准位置", script.AimPosition);
        if (AimReference != null && script.gameObject.activeSelf)
        {
            EditorStyles.toolbarButton.richText = true;
            Color ac = (AimReference.activeSelf) ? Color.red : Color.yellow;
            GUI.color = ac;
            if (GUILayout.Button(new GUIContent(aimIcon, "模拟瞄准"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                AimReference.SetActive(!AimReference.activeSelf);
                if (AimReference.activeSelf)
                {
                    script._defaultPosition = script.transform.localPosition;
                    script._defaultRotation = script.transform.localEulerAngles;
                    script.transform.localPosition = script.AimPosition;
                    script.transform.localEulerAngles = script.aimRotation;
                    script._aimRecord = true;
                    ActiveEditorTracker.sharedTracker.isLocked = true;
                }
                else if (script._aimRecord)
                {
                    script.AimPosition = script.transform.localPosition;
                    script.aimRotation = script.transform.localEulerAngles;
                    script.transform.localPosition = script._defaultPosition;
                    script.transform.localEulerAngles = script._defaultRotation;
                    script._aimRecord = false;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                    ActiveEditorTracker.sharedTracker.isLocked = false;
                }
            }
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        script.aimRotation = EditorGUILayout.Vector3Field("瞄准旋转", script.aimRotation);
        GUILayout.Space(2);
        script.useSmooth = EditorGUILayout.ToggleLeft("平滑瞄准", script.useSmooth, EditorStyles.toolbarButton);
        GUILayout.Space(2);
        script.aimZoom = EditorGUILayout.Slider("瞄准视野 变焦", script.aimZoom, 0.0f, 179);
        script.AimSmooth = EditorGUILayout.Slider("瞄准平滑", script.AimSmooth, 0.01f, 30f);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWeaponBinding()
    {
        GUILayout.BeginVertical("box");
        script.customWeapon = EditorGUILayout.ObjectField("自定义武器逻辑", script.customWeapon, typeof(bl_CustomGunBase), true) as bl_CustomGunBase;
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawBullet()
    {
        script.bulletInstanceMethod = (bl_Gun.BulletInstanceMethod)EditorGUILayout.EnumPopup("投射物实例化方式", script.bulletInstanceMethod, EditorStyles.toolbarDropDown);
        GUILayout.Space(2);
        if (script.bulletInstanceMethod == bl_Gun.BulletInstanceMethod.Pooled)
        {
            script.BulletName = EditorGUILayout.TextField("子弹", script.BulletName, EditorStyles.helpBox);
        }
        else
        {
            script.bulletPrefab = EditorGUILayout.ObjectField("子弹预制体", script.bulletPrefab, typeof(GameObject), false) as GameObject;
        }
    }

    void DrawAmmoSettings()
    {
        script.AutoReload = EditorGUILayout.ToggleLeft("自动装弹", script.AutoReload, EditorStyles.toolbarButton);
        GUILayout.Space(2);
        if (script.Info.Type == GunType.Sniper || script.Info.Type == GunType.Shotgun)
        {
            script.reloadPer = (bl_Gun.ReloadPer)EditorGUILayout.EnumPopup("换弹间隔", script.reloadPer, EditorStyles.toolbarPopup);
            GUILayout.Space(2);
        }
        else
        {
            script.reloadPer = bl_Gun.ReloadPer.Magazine;
        }
        script.bulletsPerClip = EditorGUILayout.IntField("每弹匣弹药", script.bulletsPerClip);
        script.numberOfClips = EditorGUILayout.IntSlider("弹匣数量", script.numberOfClips, 0, script.maxNumberOfClips);
        script.maxNumberOfClips = EditorGUILayout.IntField("最大弹匣数", script.maxNumberOfClips);
    }

    void DrawRecoil()
    {
        script.shakerPresent = EditorGUILayout.ObjectField("震动程度", script.shakerPresent, typeof(ShakerPresent), false) as ShakerPresent;
        script.RecoilAmount = EditorGUILayout.Slider("Recoil", script.RecoilAmount, 0.1f, 10);
        script.RecoilSpeed = EditorGUILayout.Slider("后坐力速度", script.RecoilSpeed, 1, 10);
    }

    void DrawSpreadSettings()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("散布范围", GUILayout.Width(EditorGUIUtility.labelWidth - 10));
            script.spreadMinMax.x = EditorGUILayout.FloatField(script.spreadMinMax.x, GUILayout.Width(30));
            EditorGUILayout.MinMaxSlider(ref script.spreadMinMax.x, ref script.spreadMinMax.y, 0, 7);
            script.spreadMinMax.y = EditorGUILayout.FloatField(script.spreadMinMax.y, GUILayout.Width(30));
        }
        script.spreadAimMultiplier = EditorGUILayout.Slider("瞄准散布倍率", script.spreadAimMultiplier, 0, 1);
        script.spreadPerSecond = EditorGUILayout.Slider("每秒散布增量", script.spreadPerSecond, 0, 1);
        script.decreaseSpreadPerSec = EditorGUILayout.Slider("每秒散布衰减", script.decreaseSpreadPerSec, 0, 1);
    }

    void DrawAudioSettings()
    {
        DrawSeparator("音频设置");
        EditorGUILayout.BeginVertical("box");
        script.FireSound = EditorGUILayout.ObjectField("开火音效", script.FireSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
        if (script.Info.Type != GunType.Grenade)
        {
            script.DryFireSound = EditorGUILayout.ObjectField("空仓开火音效", script.DryFireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
        }
        if (script.Info.Type == GunType.Sniper)
        {
            script.delayForSecondFireSound = EditorGUILayout.Slider("二次开火音效延迟", script.delayForSecondFireSound, 0.0f, 2.0f);
            script.DelaySource = EditorGUILayout.ObjectField("第二音源", script.DelaySource, typeof(UnityEngine.AudioSource), allowSceneObjects) as UnityEngine.AudioSource;
        }
        script.TakeSound = EditorGUILayout.ObjectField("拾取武器音效", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
        script.SoundReloadByAnim = EditorGUILayout.ToggleLeft("按动画播放装弹音效", script.SoundReloadByAnim, EditorStyles.toolbarButton);
        if (!script.SoundReloadByAnim)
        {
            script.ReloadSound = EditorGUILayout.ObjectField("换弹开始", script.ReloadSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.ReloadSound2 = EditorGUILayout.ObjectField("换弹中段", script.ReloadSound2, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.ReloadSound3 = EditorGUILayout.ObjectField("换弹结束", script.ReloadSound3, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
        }
        EditorGUILayout.EndVertical();
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!script._aimRecord || playerReferences == null || playerReferences.playerCamera == null) return;

        Vector3 origin = playerReferences.playerCamera.transform.position;
        Vector3 target = playerReferences.playerCamera.transform.position + (playerReferences.playerCamera.transform.forward * 25);

        Handles.color = Color.yellow;
        Handles.CircleHandleCap(0, origin, Quaternion.identity, 0.2f, EventType.Repaint);
        Handles.DrawDottedLine(origin, target, 3f);
        Handles.color = Color.white;
    }

    void DrawSeparator(string title)
    {
        GUILayout.Label(title, EditorStyles.toolbarButton);
    }
}