#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

   /// <summary>
   /// 项目编辑器工具共用的贴图与绘制辅助。
   /// </summary>
   public static class ResonanceRealmEditorUtils
   {
       static Texture2D m_RemoveButton;
       static Texture2D m_GrabHandle;

       /// <summary>
       /// 列表 Inspector 用的移除按钮图标。
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
       /// 列表 Inspector 用的拖拽手柄图标。
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
#endif
