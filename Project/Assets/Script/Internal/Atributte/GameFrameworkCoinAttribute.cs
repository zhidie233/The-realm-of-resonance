﻿using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameEditor
{
    public class GameCoinIDAttribute : PropertyAttribute
    {
        public GameCoinIDAttribute()
        {
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameCoinIDAttribute))]
    public class GameCoinIDAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property);
            }
            else
            {
                var names = GameConfig.Instance.gameCoins.Select(x =>
                {
                    if (x == null) return "Empty";

                    return x.CoinName;
                }).ToArray();
                property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, names);
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}