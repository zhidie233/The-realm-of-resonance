using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 移除 GFWK 顶层菜单及其全部子项，并关闭由插件自动弹出的
/// GFWK News / Updates 窗口。菜单项来自编译好的
/// GameFrameworkEditor.dll 与 GameFrameworkUpdater.dll，无法改源码，
/// 因此在编辑器加载完成后通过反射调用内部接口注销。
/// </summary>
[InitializeOnLoad]
public static class GFWKMenuRemover
{
    // 需要移除的菜单项路径，子项在前，顶层菜单在最后
    private static readonly string[] MenuItems =
    {
        "GFWK/GFWK",
        "GFWK/GFWK News",
        "GFWK/Tools/SetUp Tags and Layers",
        "GFWK/Tools/Verification",
        "GFWK/Tools",
        "GFWK/Updates",
        "GFWK"
    };

    // 需要强制关闭的插件窗口类型名片段
    private static readonly string[] WindowTypeNames =
    {
        "MFPSNews",
        "MFPSUpdaterWindow"
    };

    static GFWKMenuRemover()
    {
        // 等所有插件的 InitializeOnLoad 执行完再移除，避免被后注册的菜单复活
        EditorApplication.delayCall += RemoveAll;
        // 兜底再执行一次，覆盖更晚注册的情形
        EditorApplication.delayCall += () => EditorApplication.delayCall += RemoveAll;
    }

    private static void RemoveAll()
    {
        foreach (var item in MenuItems)
        {
            RemoveMenuItem(item);
        }
        ClosePluginWindows();
    }

    private static void RemoveMenuItem(string path)
    {
        try
        {
            var menuType = typeof(Editor).Assembly.GetType("UnityEditor.Menu");
            var removeMethod = menuType?.GetMethod("RemoveMenuItem",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            removeMethod?.Invoke(null, new object[] { path });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GFWKMenuRemover] 移除菜单 {path} 失败: {e.Message}");
        }
    }

    private static void ClosePluginWindows()
    {
        try
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                var name = window.GetType().Name;
                foreach (var target in WindowTypeNames)
                {
                    if (name.Contains(target))
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GFWKMenuRemover] 关闭插件窗口失败: {e.Message}");
        }
    }
}
