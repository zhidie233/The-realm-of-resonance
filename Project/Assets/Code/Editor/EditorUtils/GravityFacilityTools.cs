using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Resonance.Anomaly;

namespace Resonance.Editor
{
    public static class GravityFacilityTools
    {
        [MenuItem("项目工具/共振/重置全部到初始位置")]
        public static void ResetAllToStartPosition()
        {
            GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
            if (facilities.Length == 0)
            {
                Debug.LogWarning("场景中没有 GravityFacility 组件。");
                return;
            }
            Undo.RecordObjects(facilities, "Reset All GravityFacility to Start Position");
            foreach (GravityFacility facility in facilities)
            {
                facility.MoveToStartTransform();
                EditorUtility.SetDirty(facility);
            }
        }

        [MenuItem("项目工具/共振/重置全部到最终位置")]
        public static void ResetAllToFinalPosition()
        {
            GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
            if (facilities.Length == 0)
            {
                Debug.LogWarning("场景中没有 GravityFacility 组件。");
                return;
            }
            Undo.RecordObjects(facilities, "Reset All GravityFacility to Final Position");
            foreach (GravityFacility facility in facilities)
            {
                facility.MoveToFinalTransform();
                EditorUtility.SetDirty(facility);
            }
        }

        [MenuItem("项目工具/共振/选中全部重力设施")]
        public static void SelectAllGravityFacilityObjects()
        {
            List<GameObject> selectedObjects = new List<GameObject>();
            GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
            foreach (GravityFacility facility in facilities)
            {
                selectedObjects.Add(facility.gameObject);
            }
            if (selectedObjects.Count > 0) Selection.objects = selectedObjects.ToArray();
            else Debug.LogWarning("没有可选择的 GravityFacility 对象。");
        }

        [MenuItem("项目工具/共振/将当前设为全部初始位置")]
        public static void SetCurrentAsStartPositionForAll()
        {
            GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
            if (facilities.Length == 0)
            {
                Debug.LogWarning("场景中没有 GravityFacility 组件。");
                return;
            }
            Undo.RecordObjects(facilities, "Set Current as Start Position for All GravityFacility");
            foreach (GravityFacility facility in facilities)
            {
                facility.StartLimitPosition = facility.transform.position;
                facility.StartLimitRotation = facility.transform.rotation;
                EditorUtility.SetDirty(facility);
            }
        }

        [MenuItem("项目工具/共振/将当前设为全部最终位置")]
        public static void SetCurrentAsFinalPositionForAll()
        {
            GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
            if (facilities.Length == 0)
            {
                Debug.LogWarning("场景中没有 GravityFacility 组件。");
                return;
            }
            Undo.RecordObjects(facilities, "Set Current as Final Position for All GravityFacility");
            foreach (GravityFacility facility in facilities)
            {
                facility.FinalLimitPosition = facility.transform.position;
                facility.FinalLimitRotation = facility.transform.rotation;
                EditorUtility.SetDirty(facility);
            }
        }
    }
}
