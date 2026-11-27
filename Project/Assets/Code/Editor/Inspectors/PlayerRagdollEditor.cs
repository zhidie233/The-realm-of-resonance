using UnityEngine;
using UnityEditor;
using Resonance.Actors;
namespace Resonance.Editor
{

    [CustomEditor(typeof(PlayerRagdoll))]
    public class PlayerRagdollEditor : UnityEditor.Editor
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
}
