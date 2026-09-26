using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using GFWKEditor;
#endif

public class GFWorksToogleAttribute : PropertyAttribute
{
    public readonly string title;
    public readonly float ExtraWidth = 0;

    public GFWorksToogleAttribute(float extraWidth = 0)
    {
        title = string.Empty;
        ExtraWidth = extraWidth;
    }

    public GFWorksToogleAttribute(string toggleTitle)
    {
        title = toggleTitle;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GFWorksToogleAttribute))]
public class GFWorksToogleAttributteDrawer : PropertyDrawer
{
    GFWorksToogleAttribute script { get { return ((GFWorksToogleAttribute)attribute); } }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string t = script.title;
        if (string.IsNullOrEmpty(script.title)) { t = property.displayName; }
        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth += script.ExtraWidth;
        position.x += 15 * EditorGUI.indentLevel;
        property.boolValue = GFWKEditorStyles.FeatureToogle(position, property.boolValue, t);
        EditorGUIUtility.labelWidth = lw;
    }
}
#endif