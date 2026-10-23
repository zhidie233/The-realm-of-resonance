#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
   /// 项目编辑器工具共用的样式。
   public static class ResonanceRealmEditorStyles
   {
       public delegate void OnCreateStyleOp(ref GUIStyle style);

       const string EditorSkinPath = "Assets/Script/Editor/Resources/content/ResonanceRealmEditorSkin.guiskin";
       const string EditorSkinResource = "content/ResonanceRealmEditorSkin";

       static GUISkin m_EditorSkin;
       /// 自定义编辑器窗口用的皮肤。
       public static GUISkin EditorSkin
       {
           get
           {
               if (m_EditorSkin == null)
               {
                   m_EditorSkin = AssetDatabase.LoadAssetAtPath(EditorSkinPath, typeof(GUISkin)) as GUISkin;
               }
               if (m_EditorSkin == null)
               {
                   m_EditorSkin = Resources.Load(EditorSkinResource, typeof(GUISkin)) as GUISkin;
               }
               if (m_EditorSkin == null)
               {
                   m_EditorSkin = CreateFallbackSkin();
               }
               return m_EditorSkin;
           }
       }

       static GUIStyle SkinStyle(int index, GUIStyle fallback)
       {
           var skin = EditorSkin;
           if (skin == null || skin.customStyles == null) { return fallback; }
           if (index < 0 || index >= skin.customStyles.Length) { return fallback; }
           return skin.customStyles[index] != null ? skin.customStyles[index] : fallback;
       }
       /// 给可折叠标题描边用的方框样式。
       public static GUIStyle OutlineButtonStyle { get { return SkinStyle(7, EditorStyles.helpBox); } }
       /// 折叠标题样式。
       public static GUIStyle HeaderFoldoutStyle { get { return SkinStyle(6, EditorStyles.foldout); } }
       /// 武器卡片用的滑杆样式。
       public static GUIStyle HorizontalSliderStyle { get { return SkinStyle(9, EditorStyles.label); } }
       /// 武器卡片用的滑杆滑块样式。
       public static GUIStyle HorizontalSliderThumbStyle { get { return SkinStyle(10, EditorStyles.label); } }
       /// 自定义编辑器窗口用的按钮样式。
       public static GUIStyle WindowButtonStyle { get { return SkinStyle(11, EditorStyles.toolbarButton); } }
       /// 在给定矩形里画一个带标签的开关。
       public static bool FeatureToogle(Rect rect, bool value, string label)
       {
           return string.IsNullOrEmpty(label) ? EditorGUI.Toggle(rect, value) : EditorGUI.Toggle(rect, label, value);
       }
       /// 用来展开和收起分节的折叠标题。
       public static bool ContainerHeaderFoldout(string title, bool expanded)
       {
           return EditorGUILayout.Foldout(expanded, title, true, HeaderFoldoutStyle);
       }
       /// 在当前布局里画一个描边按钮。
       public static bool ButtonOutline(string label)
       {
           return GUILayout.Button(label, OutlineButtonStyle);
       }
       /// 解析十六进制颜色，带不带前缀 # 都可以。
       public static Color GetColorFromHex(string hex)
       {
           if (string.IsNullOrEmpty(hex)) { return Color.white; }
           if (hex[0] != '#') { hex = "#" + hex; }
           Color color;
           return ColorUtility.TryParseHtmlString(hex, out color) ? color : Color.white;
       }
       /// 克隆最接近的内置样式，把副本交给调用方调整。
       public static GUIStyle GetUnityStyle(string name, OnCreateStyleOp onCreate, string overlap = "")
       {
           var style = new GUIStyle(BaseStyle(name, overlap));
           if (!string.IsNullOrEmpty(name)) { style.name = name; }
           if (onCreate != null) { onCreate(ref style); }
           return style;
       }

       static GUIStyle BaseStyle(string name, string overlap)
       {
           var style = FindBuiltInStyle(name);
           if (style == null) { style = FindBuiltInStyle(overlap); }
           return style != null ? style : EditorStyles.label;
       }

       static GUIStyle FindBuiltInStyle(string name)
       {
           if (string.IsNullOrEmpty(name)) { return null; }
           switch (name)
           {
               case "label": return EditorStyles.label;
               case "miniLabel": return EditorStyles.miniLabel;
               case "boldLabel": return EditorStyles.boldLabel;
               case "toolbarButton": return EditorStyles.toolbarButton;
               case "helpBox": return EditorStyles.helpBox;
           }
           if (GUI.skin != null)
           {
               var style = GUI.skin.FindStyle(name);
               if (style != null) { return style; }
           }
           return null;
       }

       static GUISkin CreateFallbackSkin()
       {
           var skin = ScriptableObject.CreateInstance<GUISkin>();
           var styles = new GUIStyle[13];
           for (int i = 0; i < styles.Length; i++) { styles[i] = new GUIStyle(EditorStyles.label); }
           styles[3] = new GUIStyle(EditorStyles.boldLabel);
           styles[6] = new GUIStyle(EditorStyles.foldout);
           styles[7] = new GUIStyle(EditorStyles.helpBox);
           styles[9] = new GUIStyle(EditorStyles.label);
           styles[10] = new GUIStyle(EditorStyles.label);
           styles[11] = new GUIStyle(EditorStyles.toolbarButton);
           skin.customStyles = styles;
           return skin;
       }
       /// 自定义编辑器窗口用的颜色。
       public static class EditorPalette
       {
           public static Color GetBackgroundColor()
           {
               return new Color(0.098f, 0.098f, 0.098f, 1f);
           }
       }
   }
#endif
