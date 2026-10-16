#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    /// <summary>
    /// Textures and drawing helpers shared by the project editor tools.
    /// </summary>
    public static class GameEditorUtils
    {
        static Texture2D m_RemoveButton;
        static Texture2D m_GrabHandle;

        /// <summary>
        /// Icon of the remove button used by the list inspectors.
        /// </summary>
        public static Texture2D texRemoveButton
        {
            get
            {
                if (m_RemoveButton == null) { m_RemoveButton = EditorGUIUtility.FindTexture("Toolbar Minus"); }
                return m_RemoveButton;
            }
        }

        /// <summary>
        /// Icon of the drag handle used by the list inspectors.
        /// </summary>
        public static Texture2D texGrabHandle
        {
            get
            {
                if (m_GrabHandle == null) { m_GrabHandle = EditorGUIUtility.FindTexture("Toolbar Plus"); }
                return m_GrabHandle;
            }
        }

        public static void DrawTexture(Rect rect, Texture texture)
        {
            if (texture != null) { GUI.DrawTexture(rect, texture); }
        }
    }
}
#endif
