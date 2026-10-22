using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(PlayerTagDrawer))]
public class PlayerTagDrawerEditor : Editor
{
    PlayerTagDrawer script;
    public bool isSimulating = false;
    public int simulatedHealth = 100;
    public string simulatedName = "Player";
    public bool editPresent = false;

    private void OnEnable()
    {
        script = (PlayerTagDrawer)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        using (new EditorGUILayout.VerticalScope("box"))
        {
            DrawDefaultInspector();
        }

        EditorGUILayout.BeginVertical("box");
        GUI.enabled = script.StylePreset != null;
        if (GUILayout.Button("编辑外观"))
        {
            editPresent = !editPresent;
        }
        if (editPresent)
        {
            var e = Editor.CreateEditor(script.StylePreset);
            if (e != null)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    e.DrawDefaultInspector();
                }
            }
        }
        string isOn = isSimulating ? "ON" : "OFF";
        GUI.enabled = !Application.isPlaying && script.StylePreset != null;
        if (GUILayout.Button($"Simulate [{isOn}]"))
        {
            isSimulating = !isSimulating;
            ActiveEditorTracker.sharedTracker.isLocked = isSimulating;
        }
        if (isSimulating)
        {
            simulatedHealth = EditorGUILayout.IntSlider("Simulated Health", simulatedHealth, 0, 100);
            simulatedName = EditorGUILayout.TextField("模拟名称", simulatedName);
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {
        if (!isSimulating) return;
        if (script.StylePreset == null) return;
        if (script == null) { script = (PlayerTagDrawer)target; }
        if (script.positionReference == null || SceneView.lastActiveSceneView == null) return;

        Camera sc = SceneView.lastActiveSceneView.camera;
        if (sc == null) return;

        float distance = CommonUtil.Distance(sc.transform.position, script.positionReference.position);
        Handles.BeginGUI();
        Vector3 vector = sc.WorldToScreenPoint(script.positionReference.position);
        if (vector.z > 0)
        {
            float distanceDifference = Mathf.Clamp(distance - 0.1f, 1, 12);
            vector.y += distanceDifference * (script.distanceModifier + 3.3f);

            int vertical = GameConfig.Instance.allyHealthBars ? 15 : 10;
            if (distance < script.hideDistance)
            {
                GUI.Label(new Rect(vector.x - 5, (Screen.height - vector.y) - vertical, 10, 11), simulatedName, script.StylePreset.style);
                if (GameConfig.Instance.allyHealthBars)
                {
                    float mh = 100;
                    float h = simulatedHealth;
                    GUI.color = script.StylePreset.HealthBackColor;
                    GUI.DrawTexture(new Rect(vector.x - (mh / 2), (Screen.height - vector.y), mh, script.StylePreset.HealthBarThickness), script.StylePreset.HealthBarTexture);
                    GUI.color = script.StylePreset.HealthBarColor;
                    GUI.DrawTexture(new Rect(vector.x - (mh / 2), (Screen.height - vector.y), h, script.StylePreset.HealthBarThickness), script.StylePreset.HealthBarTexture);
                    GUI.color = Color.white;
                }
            }
            else
            {
                float iconsSize = script.StylePreset.IndicatorIconSize;
                GUI.DrawTexture(new Rect(vector.x - (iconsSize / 2), (Screen.height - vector.y) - (iconsSize / 2), iconsSize, iconsSize), script.StylePreset.IndicatorIcon);
            }
        }
        Handles.EndGUI();
    }
}