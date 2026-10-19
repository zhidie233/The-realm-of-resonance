using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameEditor;

[CustomEditor(typeof(AISoldierAnimation))]
public class AISoldierAnimationEditor : Editor
{
    AISoldierAnimation script;

    private void OnEnable()
    {
        script = (AISoldierAnimation)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        GUILayout.Space(10);
        if(GUILayout.Button("刷新刚体列表"))
        {
            script.GetRigidBodys();
        }
        if (GUILayout.Button("运行动画器"))
        {
            var window = (AnimationRunner)EditorWindow.GetWindow(typeof(AnimationRunner));
            window.Show();
            Animator anim = script.gameObject.GetComponent<Animator>();
            window.SetAnim(anim, null, true);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}