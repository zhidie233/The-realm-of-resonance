using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class GravityFacilityTools
{
    [MenuItem("Tools/GravityFacility/Reset All to Start Position")]
    public static void ResetAllToStartPosition()
    {
        // 获取所有GravityFacility组件（包括子物体）
        GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
        
        if (facilities.Length == 0)
        {
            Debug.LogWarning("No GravityFacility components found in the scene.");
            return;
        }

        // 记录撤销操作
        Undo.RecordObjects(facilities, "Reset All GravityFacility to Start Position");

        foreach (GravityFacility facility in facilities)
        {
            facility.MoveToStartTransform();
            EditorUtility.SetDirty(facility);
        }

        Debug.Log($"Reset {facilities.Length} GravityFacility objects to start position.");
    }

    [MenuItem("Tools/GravityFacility/Reset All to Final Position")]
    public static void ResetAllToFinalPosition()
    {
        // 获取所有GravityFacility组件（包括子物体）
        GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
        
        if (facilities.Length == 0)
        {
            Debug.LogWarning("No GravityFacility components found in the scene.");
            return;
        }

        // 记录撤销操作
        Undo.RecordObjects(facilities, "Reset All GravityFacility to Final Position");

        foreach (GravityFacility facility in facilities)
        {
            facility.MoveToFinalTransform();
            EditorUtility.SetDirty(facility);
        }

        Debug.Log($"Reset {facilities.Length} GravityFacility objects to final position.");
    }

    [MenuItem("Tools/GravityFacility/Select All GravityFacility Objects")]
    public static void SelectAllGravityFacilityObjects()
    {
        List<GameObject> selectedObjects = new List<GameObject>();
        GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);

        foreach (GravityFacility facility in facilities)
        {
            selectedObjects.Add(facility.gameObject);
        }

        if (selectedObjects.Count > 0)
        {
            Selection.objects = selectedObjects.ToArray();
            Debug.Log($"Selected {selectedObjects.Count} GravityFacility objects.");
        }
        else
        {
            Debug.LogWarning("No GravityFacility objects found to select.");
        }
    }

    // 新增功能：将所有GravityFacility对象的当前位置设置为初始位置
    [MenuItem("Tools/GravityFacility/Set Current as Start Position for All")]
    public static void SetCurrentAsStartPositionForAll()
    {
        GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
        
        if (facilities.Length == 0)
        {
            Debug.LogWarning("No GravityFacility components found in the scene.");
            return;
        }

        // 记录撤销操作
        Undo.RecordObjects(facilities, "Set Current as Start Position for All GravityFacility");

        foreach (GravityFacility facility in facilities)
        {
            // 将当前位置和旋转设置为初始位置和旋转
            facility.StartLimitPosition = facility.transform.position;
            facility.StartLimitRotation = facility.transform.rotation;
            EditorUtility.SetDirty(facility);
        }

        Debug.Log($"Set current position as start position for {facilities.Length} GravityFacility objects.");
    }

    // 新增功能：将所有GravityFacility对象的当前位置设置为最终位置
    [MenuItem("Tools/GravityFacility/Set Current as Final Position for All")]
    public static void SetCurrentAsFinalPositionForAll()
    {
        GravityFacility[] facilities = Object.FindObjectsOfType<GravityFacility>(true);
        
        if (facilities.Length == 0)
        {
            Debug.LogWarning("No GravityFacility components found in the scene.");
            return;
        }

        // 记录撤销操作
        Undo.RecordObjects(facilities, "Set Current as Final Position for All GravityFacility");

        foreach (GravityFacility facility in facilities)
        {
            // 将当前位置和旋转设置为最终位置和旋转
            facility.FinalLimitPosition = facility.transform.position;
            facility.FinalLimitRotation = facility.transform.rotation;
            EditorUtility.SetDirty(facility);
        }

        Debug.Log($"Set current position as final position for {facilities.Length} GravityFacility objects.");
    }
}