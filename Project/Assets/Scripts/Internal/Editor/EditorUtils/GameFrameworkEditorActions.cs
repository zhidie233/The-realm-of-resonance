using UnityEngine;
using UnityEditor;

public static class MFPSEditorActions
{
 
    [MenuItem("游戏框架/操作/重置默认服务器")]
    static void ResetDefaultServer()
    {
        PlayerPrefs.DeleteKey(PropertiesKeys.GetUniqueKey("preferredregion"));
    }

    [MenuItem("游戏框架/操作/清除 PlayerPrefs")]
    static void DeleteAllPlayerPrefs()
    {
        if(EditorUtility.DisplayDialog("删除配置", "确定要清除全部 PlayerPrefs 吗？", "确定", "取消"))
        PlayerPrefs.DeleteAll();
    }
}