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

    [CustomEditor(typeof(ButtonMapped))]
    public class ButtonMappedEditor : UnityEditor.Editor
    {
        ButtonMapped script;

        private void OnEnable()
        {
            script = (ButtonMapped)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(10);
            base.OnInspectorGUI();
            GUILayout.Space(10);
            string key = $"{InputBindings.KEYS}.{(short)script.inputType}";
            if (PlayerPrefs.HasKey(key))
            {
                if(GUILayout.Button("删除已保存的输入绑定"))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);

                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }
    }
}
