using UnityEngine;
using UnityEditor;

public static class ResonanceRealmEditorActions
{
 
    [MenuItem("游戏设置/操作/重置默认服务器")]
    static void ResetDefaultServer()
    {
        PlayerPrefs.DeleteKey(NetworkKeys.GetUniqueKey("preferredregion"));
    }

    [MenuItem("游戏设置/操作/清除 PlayerPrefs")]
    static void DeleteAllPlayerPrefs()
    {
        if(EditorUtility.DisplayDialog("删除配置", "确定要清除全部 PlayerPrefs 吗？", "确定", "取消"))
        PlayerPrefs.DeleteAll();
    }
}