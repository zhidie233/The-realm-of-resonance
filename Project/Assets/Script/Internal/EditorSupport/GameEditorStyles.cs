#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    /// <summary>
    /// Styles shared by the project editor tools.
    /// </summary>
    public static class GameEditorStyles
    {
        public delegate void OnCreateStyleOp(ref GUIStyle style);

        const string EditorSkinPath = "Assets/Script/Editor/Resources/content/GameEditorSkin.guiskin";
        const string EditorSkinResource = "content/GameEditorSkin";

        static GUISkin m_EditorSkin;

        /// <summary>
        /// Skin used by the custom editor windows.
        /// </summary>
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

        /// <summary>
        /// Box style used to outline a selectable header.
        /// </summary>
        public static GUIStyle OutlineButtonStyle { get { return SkinStyle(7, EditorStyles.helpBox); } }

        /// <summary>
        /// Foldout header style.
        /// </summary>
        public static GUIStyle HeaderFoldoutStyle { get { return SkinStyle(6, EditorStyles.foldout); } }

        /// <summary>
        /// Slider style used by the weapon cards.
        /// </summary>
        public static GUIStyle HorizontalSliderStyle { get { return SkinStyle(9, EditorStyles.label); } }

        /// <summary>
        /// Slider thumb style used by the weapon cards.
        /// </summary>
        public static GUIStyle HorizontalSliderThumbStyle { get { return SkinStyle(10, EditorStyles.label); } }

        /// <summary>
        /// Button style used by the custom editor windows.
        /// </summary>
        public static GUIStyle WindowButtonStyle { get { return SkinStyle(11, EditorStyles.toolbarButton); } }

        /// <summary>
        /// Draws a labeled toggle inside the given rect.
        /// </summary>
        public static bool FeatureToogle(Rect rect, bool value, string label)
        {
            return string.IsNullOrEmpty(label) ? EditorGUI.Toggle(rect, value) : EditorGUI.Toggle(rect, label, value);
        }

        /// <summary>
        /// Foldout header used to open and close a section.
        /// </summary>
        public static bool ContainerHeaderFoldout(string title, bool expanded)
        {
            return EditorGUILayout.Foldout(expanded, title, true, HeaderFoldoutStyle);
        }

        /// <summary>
        /// Outline button drawn in the current layout.
        /// </summary>
        public static bool ButtonOutline(string label)
        {
            return GUILayout.Button(label, OutlineButtonStyle);
        }

        /// <summary>
        /// Parses a hex color, with or without the leading '#'.
        /// </summary>
        public static Color GetColorFromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) { return Color.white; }
            if (hex[0] != '#') { hex = "#" + hex; }
            Color color;
            return ColorUtility.TryParseHtmlString(hex, out color) ? color : Color.white;
        }

        /// <summary>
        /// Clones the closest built in style and lets the caller customize the copy.
        /// </summary>
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

        /// <summary>
        /// Colors used by the custom editor windows.
        /// </summary>
        public static class EditorPalette
        {
            public static Color GetBackgroundColor()
            {
                return new Color(0.098f, 0.098f, 0.098f, 1f);
            }
        }
    }
}
#endif
