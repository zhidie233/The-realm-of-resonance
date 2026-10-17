using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using GameEditor;
#endif

public class GameToggleAttribute : PropertyAttribute
{
    public readonly string title;
    public readonly float ExtraWidth = 0;

    public GameToggleAttribute(float extraWidth = 0)
    {
        title = string.Empty;
        ExtraWidth = extraWidth;
    }

    public GameToggleAttribute(string toggleTitle)
    {
        title = toggleTitle;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GameToggleAttribute))]
public class GameToggleAttributeDrawer : PropertyDrawer
{
    GameToggleAttribute script { get { return ((GameToggleAttribute)attribute); } }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string t = script.title;
        if (string.IsNullOrEmpty(script.title)) { t = property.displayName; }
        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth += script.ExtraWidth;
        position.x += 15 * EditorGUI.indentLevel;
        property.boolValue = GameEditorStyles.FeatureToogle(position, property.boolValue, t);
        EditorGUIUtility.labelWidth = lw;
    }
}
#endif