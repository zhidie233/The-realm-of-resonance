using UnityEditor;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FindMissingFontsTool : EditorWindow
{
    [MenuItem("Tools/TextMeshPro/查找缺失字体的TMP组件")]
    public static void ShowWindow()
    {
        GetWindow<FindMissingFontsTool>("缺失字体查找器");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("开始在场景和预制体中查找"))
        {
            FindAndReportMissingFonts();
        }
    }

    private void FindAndReportMissingFonts()
    {
        List<GameObject> objectsWithMissingFonts = new List<GameObject>();
        // 查找所有TMP文本组件（包括禁用和预制体中的）
        TMP_Text[] allTextComponents = Resources.FindObjectsOfTypeAll<TMP_Text>();

        foreach (TMP_Text textComp in allTextComponents)
        {
            // 检查字体资源是否缺失
            if (textComp.font == null)
            {
                objectsWithMissingFonts.Add(textComp.gameObject);
                // 在控制台打印警告，并可点击定位到对象
                Debug.LogWarning($"发现缺失字体的TMP组件: {textComp.name}", textComp.gameObject);
            }
        }
        Debug.Log($"检查完成。共发现 {objectsWithMissingFonts.Count} 个缺失字体的TMP组件。");
    }
}