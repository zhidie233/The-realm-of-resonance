using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 此特性只能应用于字段，因为其关联的
/// PropertyDrawer 只处理字段（public 或
/// 带 [SerializeField] 特性的字段），
/// 这些都位于目标 MonoBehaviour 上。
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Field)]
public class FirearmIDAttribute : PropertyAttribute
{
    public FirearmIDAttribute()
    {

    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FirearmIDAttribute))]
public class InspectorButtonPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        prop.intValue = EditorGUI.Popup(position, prop.displayName, prop.intValue, GameConfig.Instance.AllWeaponStringList());
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 2;
    }
}
#endif