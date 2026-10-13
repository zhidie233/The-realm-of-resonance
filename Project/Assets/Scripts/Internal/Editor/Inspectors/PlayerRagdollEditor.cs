using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerRagdoll))]
public class PlayerRagdollEditor : Editor
{
    PlayerRagdoll script;

    public override void OnInspectorGUI()
    {
        script = (PlayerRagdoll)target;
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;
        base.OnInspectorGUI();
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        if (GUILayout.Button("刷新"))
        {
            script.SetUpHitBoxes();
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

}