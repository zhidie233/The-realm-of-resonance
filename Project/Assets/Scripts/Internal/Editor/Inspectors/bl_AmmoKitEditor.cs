using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GFWKEditor;

namespace GFWK.Runtime.Level
{
    [CustomEditor(typeof(bl_Ammo))]
    public class bl_AmmoKitEditor : Editor
    {
        bl_Ammo script;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            script = (bl_Ammo)target;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(4);
                script.itemAuthority = (bl_NetworkItem.ItemAuthority)EditorGUILayout.EnumPopup("物品权限", script.itemAuthority, EditorStyles.toolbarPopup);
                GUILayout.Space(4);
                Rect r = GUILayoutUtility.GetRect(Screen.width - 100, EditorGUIUtility.singleLineHeight);
                script.isSceneItem = GFWKEditorStyles.FeatureToogle(r, script.isSceneItem, "是否为场景物品");
                GUILayout.Space(10);
                r = GUILayoutUtility.GetRect(Screen.width - 100, EditorGUIUtility.singleLineHeight);
                script.isGlobal = GFWKEditorStyles.FeatureToogle(r, script.isGlobal, "是否为全局弹药");
                GUILayout.Space(2);
                r = GUILayoutUtility.GetRect(Screen.width - 100, EditorGUIUtility.singleLineHeight);
                script.autoRespawn = GFWKEditorStyles.FeatureToogle(r, script.autoRespawn, "自动重生");
                GUILayout.Space(2);
                if (!script.isGlobal)
                {
                    script.ForGun = EditorGUILayout.Popup("适用枪械", script.ForGun, bl_GameData.Instance.AllWeaponStringList(), EditorStyles.toolbarPopup);
                    GUILayout.Space(2);
                }
                script.Bullets = EditorGUILayout.IntField("子弹数", script.Bullets);
                script.Projectiles = EditorGUILayout.IntField("投射物数", script.Projectiles);

                script.PickSound = EditorGUILayout.ObjectField("拾取音效", script.PickSound, typeof(AudioClip), false) as AudioClip;
            }
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                script.EditorValidateName();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
}