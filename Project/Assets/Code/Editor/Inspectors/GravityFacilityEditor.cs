using UnityEditor;
using UnityEngine;
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

    [CustomEditor(typeof(GravityFacility))]
    public class GravityFacilityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GravityFacility script = (GravityFacility)target;
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("将当前设为初始位置"))
            {
                script.StartLimitPosition = script.transform.position;
                script.StartLimitRotation = script.transform.rotation;
                EditorUtility.SetDirty(script);
            }
            if (GUILayout.Button("将当前设为最终位置"))
            {
                script.FinalLimitPosition = script.transform.position;
                script.FinalLimitRotation = script.transform.rotation;
                EditorUtility.SetDirty(script);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("移动到初始位置"))
            {
                script.MoveToStartTransform();
                EditorUtility.SetDirty(script);
            }
            if (GUILayout.Button("移动到最终位置"))
            {
                script.MoveToFinalTransform();
                EditorUtility.SetDirty(script);
            }
            GUILayout.EndHorizontal();
        }
    }
}
