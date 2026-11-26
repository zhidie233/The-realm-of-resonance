using UnityEngine;
using UnityEditor;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Editor
{

    public static class ResonanceRealmEditorActions
    {
        [MenuItem("项目工具/操作/重置默认服务器")]
        static void ResetDefaultServer()
        {
            PlayerPrefs.DeleteKey(NetProtocol.GetUniqueKey("preferredregion"));
        }
        [MenuItem("项目工具/操作/清除 PlayerPrefs")]
        static void DeleteAllPlayerPrefs()
        {
            if(EditorUtility.DisplayDialog("删除配置", "确定要清除全部 PlayerPrefs 吗？", "确定", "取消"))
            PlayerPrefs.DeleteAll();
        }
    }
}
