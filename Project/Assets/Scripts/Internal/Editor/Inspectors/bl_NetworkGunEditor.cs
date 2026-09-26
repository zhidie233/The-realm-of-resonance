using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using GFWKEditor;

[CustomEditor(typeof(bl_NetworkGun))]
public class bl_NetworkGunEditor : Editor
{
    private bl_NetworkGun script;
    private bl_PlayerReferences playerReferences;
    private List<string> FPWeaponsAvailable = new List<string>();
    private bl_Gun[] LocalGuns;
    private int selectLG = 0;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_NetworkGun)target;
        playerReferences = script.transform.root.GetComponent<bl_PlayerReferences>();
        if (playerReferences != null && playerReferences.gunManager != null)
        {
            LocalGuns = playerReferences.gunManager.transform.GetComponentsInChildren<bl_Gun>(true);
            for (int i = 0; i < LocalGuns.Length; i++)
            {
                FPWeaponsAvailable.Add(LocalGuns[i].name);
            }
        }

        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        bool allowSceneObjects = !EditorUtility.IsPersistent(script);

        EditorGUI.BeginChangeCheck();
        if (script.LocalGun != null)
        {
            script.gameObject.name = bl_GameData.Instance.GetWeapon(script.LocalGun.GunID).Name;
        }

        EditorGUILayout.BeginVertical("box");
        script.LocalGun = EditorGUILayout.ObjectField("本地武器", script.LocalGun, typeof(bl_Gun), allowSceneObjects) as bl_Gun;
        EditorGUILayout.EndVertical();

        if (script.LocalGun != null)
        {
            if (script.LocalGun.Info.Type != GunType.Knife)
            {
                EditorGUILayout.BeginVertical("box");

                if (script.LocalGun.Info.Type == GunType.Grenade)
                {
                    script.Bullet = EditorGUILayout.ObjectField("Bullet", script.Bullet, typeof(GameObject), allowSceneObjects) as GameObject;
                }
                if (script.LocalGun.Info.Type != GunType.Grenade)
                {
                    script.MuzzleFlash = EditorGUILayout.ObjectField("枪口火焰", script.MuzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                }
                if (script.LocalGun.Info.Type == GunType.Grenade)
                {
                    script.DesactiveOnOffAmmo = EditorGUILayout.ObjectField("无弹药时禁用", script.DesactiveOnOffAmmo, typeof(GameObject), allowSceneObjects) as GameObject;
                }
                EditorGUILayout.EndVertical();
            }

            GUILayout.BeginVertical("box");
            script.useCustomPlayerAnimations = EditorGUILayout.ToggleLeft("使用自定义玩家动画", script.useCustomPlayerAnimations, EditorStyles.toolbarButton);
            if (script.useCustomPlayerAnimations)
            {
                GUILayout.Space(2);
                EditorGUILayout.HelpBox("请查阅 玩家动画 > 武器动画 文档进行设置。", MessageType.Info);
                script.customUpperAnimationID = EditorGUILayout.IntField("自定义动画状态 ID", script.customUpperAnimationID);
                script.customFireAnimationName = EditorGUILayout.TextField("自定义开火动画名", script.customFireAnimationName);
            }
            GUILayout.EndVertical();

            if (script.LocalGun.Info.Type != GunType.Grenade && script.LocalGun.Info.Type != GunType.Knife)
            {
                GUILayout.BeginVertical("box");
                if (script.LeftHandPosition != null)
                {
                    if (GUILayout.Button("编辑手部位置", EditorStyles.toolbarButton))
                    {
                        OpenIKWindow(script);
                    }
                }
                else
                {
                    if (GUILayout.Button("设置手部 IK", EditorStyles.toolbarButton))
                    {

                        GameObject gobject = new GameObject("LeftHandPoint");
                        gobject.transform.parent = script.transform;
                        gobject.transform.position = bl_UtilityHelper.CalculateCenter(script.transform);
                        gobject.transform.localEulerAngles = Vector3.zero;
                        script.LeftHandPosition = gobject.transform;
                        OpenIKWindow(script);
                    }
                }
                if (playerReferences != null && playerReferences.playerNetwork != null && !playerReferences.playerNetwork.NetworkGuns.Contains(script))
                {
                    if (GUILayout.Button("登记第三人称武器", EditorStyles.toolbarButton))
                    {
                        playerReferences.playerNetwork.NetworkGuns.Add(script);
                        EditorUtility.SetDirty(playerReferences.playerNetwork);
                    }
                }
                GUILayout.EndVertical();
            }
            else
            {
                if (playerReferences != null && playerReferences.playerNetwork != null && !playerReferences.playerNetwork.NetworkGuns.Contains(script))
                {
                    if (GUILayout.Button("登记第三人称武器", EditorStyles.toolbarButton))
                    {
                        playerReferences.playerNetwork.NetworkGuns.Add(script);
                        EditorUtility.SetDirty(playerReferences.playerNetwork);
                    }
                }
            }
        }
        else
        {
            if (playerReferences != null && playerReferences.gunManager != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("选择该第三人称武器对应的本地武器");
                GUILayout.BeginHorizontal();
                GUILayout.Label("第一人称武器：", GUILayout.Width(100));
                selectLG = EditorGUILayout.Popup(selectLG, FPWeaponsAvailable.ToArray());
                if (GUILayout.Button("选择", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    script.LocalGun = LocalGuns[selectLG];
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            else
            {
                if (GUILayout.Button("打开第一人称武器", EditorStyles.toolbarButton))
                {
                    bl_GunManager gm = script.transform.root.GetComponentInChildren<bl_GunManager>();
                    Selection.activeObject = gm.transform.GetChild(0).gameObject;
                    EditorGUIUtility.PingObject(gm.transform.GetChild(0).gameObject);
                }
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (playerReferences == null || playerReferences.playerCamera == null) return;

        Vector3 origin = script.transform.position;
        Vector3 target = playerReferences.playerCamera.transform.position + (playerReferences.playerCamera.transform.forward * 25);

        Handles.color = Color.yellow;
        Handles.DrawDottedLine(origin, target, 3f);
        Handles.color = Color.white;
    }

    void OpenIKWindow(bl_NetworkGun script)
    {
        AnimatorRunner window = (AnimatorRunner)EditorWindow.GetWindow(typeof(AnimatorRunner));
        window.Show();
        bl_PlayerReferences pa = script.transform.root.GetComponent<bl_PlayerReferences>();
        Animator anim = pa.playerAnimations.Animator;
        pa.EditorSelectedGun = script;

        var pis = pa.playerAnimations.GetComponentsInChildren<bl_PlayerIKBase>(true);
        if(pis == null || pis.Length == 0)
        {
            Debug.LogWarning("在玩家预制体中未找到玩家 IK 脚本！");
            return;
        }

        bl_PlayerIKBase hm = pis[0];
        if(pis.Length > 1)
        {
            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i] == null || !pis[i].gameObject.activeSelf) continue;

                hm = pis[i];
                break;
            }
        }

        if (hm != null)
        {
            hm.enabled = true;
            hm.Init();
            hm.CustomArmsIKHandler = null;
        }

        window.SetAnim(anim, () =>
        {

        });
        Selection.activeObject = script.LeftHandPosition.gameObject;
    }
}