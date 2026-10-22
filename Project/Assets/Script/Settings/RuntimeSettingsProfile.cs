using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif
   [CreateAssetMenu(fileName = "Settings Profile", menuName = "项目/Settings/Profile")]
   public class RuntimeSettingsProfile : ScriptableObject
   {
       public enum ResolutionApplication
       {
           NoApply,
           ApplyCurrent,
           ApplyUserSelected
       }

       [Reorderable]
       public List<SettingValue> settingValues = new List<SettingValue>();
       public int[] RefreshRates = new int[] { 30, 60, 120, 144, 200, 260, 0 };
       /// 获取某项设置的已保存值
       /// 不存在时返回默认值
       public object GetSettingOf(string settingName)
       {
           int index = settingValues.FindIndex(x => x.Name.ToLower() == settingName.ToLower());
           if (index == -1) { Debug.LogWarning($"当前配置档案中没有定义设置项 {settingName}。"); return null; }

           return settingValues[index].GetValue();
       }
       /// 保存某项设置的值
       public void SetSettingOf(string settingName, object value, bool autoSave = false)
       {
           int index = settingValues.FindIndex(x => x.Name.ToLower() == settingName.ToLower());
           if (index == -1) { Debug.LogWarning($"当前配置档案中没有定义设置项 {settingName}。"); return; }

           settingValues[index].SetSettingValue(value, autoSave);
       }
       /// <see cref="settingValues"/> 列表里是否定义了该设置？
       /// 不检查该设置是否已保存过值。
       public bool HasSettingDefinedFor(string settingName)
       {
           return settingValues.Exists(x => x.Name.ToLower() == settingName.ToLower());
       }
       /// 保存当前设置
       public void SaveSettings()
       {
           foreach (var item in settingValues)
           {
               item.SaveValue();
           }
       }
       /// 应用该配置档的设置
       public void ApplySettings(ResolutionApplication resolutionApplication, bool fireCallback = true)
       {
           QualitySettings.SetQualityLevel((int)GetSettingOf("质量"), true);
           QualitySettings.anisotropicFiltering = (AnisotropicFiltering)(int)GetSettingOf("各向异性");
           int antiA = (int)GetSettingOf("抗锯齿");
           QualitySettings.antiAliasing = antiA;
           AudioListener.pause = !(bool)GetSettingOf("音频");
           AudioListener.volume = (float)GetSettingOf("音量");

           bool antialising = antiA == 0 ? false : true;
           GameEvents.SetEffectChange((bool)GetSettingOf("CrAberration"), antialising, (bool)GetSettingOf("Bloom"),
               (bool)GetSettingOf("SSAO"), (bool)GetSettingOf("动态模糊"));
           // webgl 下分辨率必须改网页 canvas，需要默认不支持的外部调用集成。
#if !UNITY_WEBGL
           if (!CommonUtil.isMobile)
           {
               ApplyResolutionSettings(resolutionApplication);
           }
#endif

           if (fireCallback)
               GameEvents.DispatchGameSettingsChange();
       }

       public void ApplyResolutionSettings(ResolutionApplication resolutionApplication)
       {
           if (resolutionApplication == ResolutionApplication.NoApply) return;

           if (resolutionApplication == ResolutionApplication.ApplyCurrent) ResolutionSettings.FetchResolutions();

           int displayModeID = (int)GetSettingOf("Display Mode");
           if (displayModeID != -1)
           {
               var currentResolution = Screen.currentResolution;
               if (resolutionApplication == ResolutionApplication.ApplyUserSelected)
               {
                   currentResolution = ResolutionSettings.ResolutionHandler.GetRelativeResolution();
               }
               else if (resolutionApplication == ResolutionApplication.ApplyCurrent)
               {
                   if (ResolutionSettings.ResolutionHandler != null)
                   {
                       currentResolution = ResolutionSettings.ResolutionHandler.GetResolution();
                   }
               }

               Screen.SetResolution(currentResolution.width, currentResolution.height, (FullScreenMode)displayModeID);
           }
       }

       [ContextMenu("Delete Saved")]
       public void DeleteSavedSettings()
       {
           foreach (var item in settingValues)
           {
               item.DeleteSavedValue();
           }
       }

       [System.Serializable]
       public class SettingValue
       {
           public string Name;
           public ValueType valueType = ValueType.Integer;
           public string Value;

           private bool hasBeenModified = false;

           public object GetValue()
           {
               string v = PlayerPrefs.GetString(SettingKey, Value);
               switch (valueType)
               {
                   case ValueType.Bool:
                       return v == "1" ? true : false;
                   case ValueType.Float:
                       float f = 0f;
                       v = v.Replace(",", ".");
                       if (float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                       {
                           return f;
                       }
                       else
                       {
                           Debug.LogWarning($"无法解析设置项“{Name}”的值：{v}");
                           return f;
                       }
                   case ValueType.Integer:
                       int iv = 0;
                       if (int.TryParse(v, out iv))
                       {
                           return iv;
                       }
                       else
                       {
                           Debug.LogWarning($"无法解析设置项“{Name}”的值：{v}");
                           return iv;
                       }
                   case ValueType.String:
                   default:
                       return v;
               }
           }

           public void SetSettingValue(object value, bool autoSave = false)
           {
               string v = "";
               if (value is bool) { v = (bool)value == true ? "1" : "0"; }
               else { v = value.ToString(); }
               Value = v;

               hasBeenModified = true;

               if (autoSave)
               {
                   SaveValue();
               }
           }

           public void SaveValue()
           {
               if (!hasBeenModified) return;

               PlayerPrefs.SetString(SettingKey, Value);
               hasBeenModified = false;
           }

           public void DeleteSavedValue() => PlayerPrefs.DeleteKey(SettingKey);

           public string SettingKey => $"{Application.productName}.msetting.{Name}";

           [System.Serializable]
           public enum ValueType
           {
               Float = 0,
               String = 1,
               Bool = 2,
               Integer = 3,
           }
       }

#if UNITY_EDITOR
       [CustomPropertyDrawer(typeof(SettingValue))]
       public class SettingValueDrawer : PropertyDrawer
       {
           // 在给定矩形内绘制属性
           public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
           {
               // 对父属性使用 BeginProperty / EndProperty 意味着
               // 预制体覆盖逻辑对整个属性生效。
               EditorGUI.BeginProperty(position, label, property);
               Rect r = position;
               r.height = EditorGUIUtility.singleLineHeight;
               // 绘制标签
               property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label);
               r.y += EditorGUIUtility.singleLineHeight + 2;

               if (property.isExpanded)
               {
                   // 绘制字段 - 每个都传 GUIContent.none，这样绘制时不带标签
                   EditorGUI.PropertyField(r, property.FindPropertyRelative("Name"), new GUIContent("Name"));
                   r.y += EditorGUIUtility.singleLineHeight + 2;
                   EditorGUI.PropertyField(r, property.FindPropertyRelative("valueType"), new GUIContent("Value Type"));
                   r.y += EditorGUIUtility.singleLineHeight + 2;
                   var value = property.FindPropertyRelative("Value");
                   if (property.FindPropertyRelative("valueType").intValue == 2)
                   {
                       bool bv = value.stringValue == "1" ? true : false;
                       bv = EditorGUI.ToggleLeft(r, "Value", bv, EditorStyles.toolbar);
                       value.stringValue = bv == true ? "1" : "0";
                   }
                   else
                       EditorGUI.PropertyField(r, value, new GUIContent("Value"));
               }

               EditorGUI.EndProperty();
           }

           public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
           {
               if (property.isExpanded)
               {
                   return EditorGUI.GetPropertyHeight(property);
               }
               else return EditorGUIUtility.singleLineHeight;
           }
       }
#endif
   }
