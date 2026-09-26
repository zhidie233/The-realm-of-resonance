using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using GFWKEditor;
#endif

namespace GFWKEditor.Addons
{
    [CreateAssetMenu(fileName = "Game Framework 插件", menuName = "游戏框架/扩展信息", order = 300)]
    public class GFWKAddon : ScriptableObject
    {
        public string Name;
        public string Version;
        public string MinGFWKVersion = "1.6";

        [TextArea(4, 10)]
        public string Instructions;
        public string TutorialScript = "";
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(GFWKAddon))]
    public class GFWKAddonsEditor : Editor
    {
        GFWKAddon script;
        private GUIStyle TextStyle = null;
        private GUIStyle TextStyleFlat = null;
        private bool editMode = false;
        public TutorialWizardText contentText;

        private void OnEnable()
        {
            script = (GFWKAddon)target;
            TextStyle = Resources.Load<GUISkin>("content/GameFrameworkEditorSkin").customStyles[3];
            TextStyleFlat = Resources.Load<GUISkin>("content/GameFrameworkEditorSkin").customStyles[1];
            contentText = new TutorialWizardText();

            if (GFWKAddonsData.Instance != null)
            {
                int i = GFWKAddonsData.Instance.Addons.FindIndex(x => x.NiceName == script.Name);
                if (i >= 0 && GFWKAddonsData.Instance.Addons[i].Info == null)
                {
                    GFWKAddonsData.Instance.Addons[i].Info = script;
                    EditorUtility.SetDirty(GFWKAddonsData.Instance);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    GFWorksStats.SetStat($"aa-{script.Name}", 1);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            Rect rect;
            Rect r = EditorGUILayout.BeginVertical();
            {
                rect = r;
                TutorialWizard.Style.DrawGlowRect(r, GFWKEditorStyles.GFWorksEditorPalette.GetMainColor(true), Color.white);
                if (!editMode && !string.IsNullOrEmpty(script.Name))
                {
                    r = EditorGUILayout.BeginVertical();
                    {
                        TutorialWizard.Style.DrawGlowRect(r, GFWKEditorStyles.GFWorksEditorPalette.GetBackgroundColor(true), Color.white);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"<size=30>{script.Name.ToUpper()}</size>", TextStyle);
                        GUILayout.FlexibleSpace();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(string.Format("<size=14>版本： <b>{0}</b></size>", script.Version), TextStyleFlat);
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField(string.Format("<size=14>最低 GFWK 版本： <b>{0}</b></size>", script.MinGFWKVersion), TextStyleFlat);
                        EditorGUILayout.EndHorizontal();
                        if (!string.IsNullOrEmpty(script.TutorialScript))
                        {
                            GUILayout.Space(5);
                            if (GFWKEditorStyles.ButtonOutline("文档", GFWKEditorStyles.GFWorksEditorPalette.GetHighlightColor(true)))
                            {
                                EditorWindow.GetWindow(System.Type.GetType(string.Format("{0}, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", script.TutorialScript)));
                            }
                        }
                    }
                    GUILayout.Space(10);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                    if(!string.IsNullOrEmpty(script.Instructions))
                    contentText.DrawText(script.Instructions, TextStyleFlat);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.EndHorizontal();
                    DrawDefaultInspector();
                }
                GUILayout.Space(25);
                if (TutorialWizard.Buttons.GlowButton("扩展管理器", GFWKEditorStyles.GFWorksEditorPalette.GetBackgroundColor(true), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    EditorWindow.GetWindow<GFWKAddonsWindow>().OpenAddonPage(script.Name);
                }
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();

            rect.x += rect.width - 15;
            rect.y += 5;
            rect.width = 10; rect.height = 25;

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) editMode = !editMode;
            rect.width = 1;
            EditorGUI.DrawRect(rect, Color.gray);
        }
    }
#endif
}