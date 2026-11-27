using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Resonance.Combat;
using Resonance.Core;

namespace Resonance.Editor
{
    /// <summary>
    /// Editor audit utilities for Missing Scripts, FirearmController serialization, and Resonance config integrity.
    /// </summary>
    public static class ArchitectureAuditTools
    {
        [MenuItem("工具/架构校验/检查全部 Prefab 与场景 Missing Script")]
        public static void CheckAllMissingScripts()
        {
            int missingCount = 0;

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                foreach (Transform node in prefab.GetComponentsInChildren<Transform>(true))
                {
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject);
                    if (count > 0)
                    {
                        missingCount += count;
                        Debug.LogError($"[MissingScript] Prefab: {path} -> {node.name} ({count})", prefab);
                    }
                }
            }

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                var opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
                foreach (GameObject root in opened.GetRootGameObjects())
                {
                    foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                    {
                        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject);
                        if (count > 0)
                        {
                            missingCount += count;
                            Debug.LogError($"[MissingScript] Scene: {scene.path} -> {node.name} ({count})");
                        }
                    }
                }
                EditorSceneManager.CloseScene(opened, true);
            }

            Debug.Log($"[ArchitectureAudit] Missing Script 扫描完成，共发现 {missingCount} 处问题。");
        }

        [MenuItem("工具/架构校验/校验 FirearmController 预制体序列化完整性")]
        public static void ValidateFirearmPrefabSerialization()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources", "Assets/Art/Player" });
            int checkedWeapons = 0;
            int warnings = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                FirearmController[] weapons = prefab.GetComponentsInChildren<FirearmController>(true);
                foreach (var weapon in weapons)
                {
                    checkedWeapons++;
                    if (weapon.WeaponID < 0)
                    {
                        warnings++;
                        Debug.LogWarning($"[FirearmAudit] {path} -> {weapon.name} WeaponID 无效 ({weapon.WeaponID})", prefab);
                    }
                    if (weapon.bulletsPerClip <= 0 && weapon.Info != null && weapon.Info.Type != Foundation.FirearmType.Knife)
                    {
                        warnings++;
                        Debug.LogWarning($"[FirearmAudit] {path} -> {weapon.name} bulletsPerClip={weapon.bulletsPerClip}", prefab);
                    }
                }
            }

            Debug.Log($"[ArchitectureAudit] FirearmController 校验完成：检查 {checkedWeapons} 个武器组件，告警 {warnings} 项。");
        }

        [MenuItem("工具/架构校验/校验 ProjectConfigRoot 与共鸣配置完整性")]
        public static void ValidateProjectConfigAndResonance()
        {
            var root = ProjectConfigRoot.Instance;
            if (root == null)
            {
                Debug.LogError("[ArchitectureAudit] 未找到 ProjectConfigRoot.asset");
                return;
            }

            var missing = new List<string>();
            if (root.rules == null) missing.Add(nameof(root.rules));
            if (root.weapons == null) missing.Add(nameof(root.weapons));
            if (root.arenas == null) missing.Add(nameof(root.arenas));
            if (root.factions == null) missing.Add(nameof(root.factions));
            if (root.resonance == null) missing.Add(nameof(root.resonance));
            if (root.resonanceConfig == null) missing.Add(nameof(root.resonanceConfig));
            if (root.gravityConfig == null) missing.Add(nameof(root.gravityConfig));
            if (root.groundFireConfig == null) missing.Add(nameof(root.groundFireConfig));
            if (root.meteoriteConfig == null) missing.Add(nameof(root.meteoriteConfig));
            if (root.robotConfig == null) missing.Add(nameof(root.robotConfig));
            if (root.supplyConfig == null) missing.Add(nameof(root.supplyConfig));

            if (missing.Count > 0)
            {
                Debug.LogError($"[ArchitectureAudit] ProjectConfigRoot 存在空配置引用: {string.Join(", ", missing)}", root);
                return;
            }

            Debug.Log("[ArchitectureAudit] ProjectConfigRoot 与全部共鸣子配置校验通过。");
        }
    }
}
