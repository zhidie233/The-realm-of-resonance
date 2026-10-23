using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

   public class SingleSettingsBinding : MonoBehaviour
   {
       [Header("Settings")]
       public string SettingKeyName = "";
       public string[] optionsNames = new string[] { "禁用", "启用" };
       [GameToggle] public bool autoUpperCase = false;

       [Header("Listener")]
       public OnChange onChange;

       public int currentOption { get; set; } = 0;
       public bool CurrentSelectionAsBool() => currentOption == 1 ? true : false;

       [System.Serializable] public class OnChange : UnityEvent<int> { }
       [Header("References")]
       public Text optionText;
       public TextMeshProUGUI optionTextTMP;

       private void Awake()
       {
       }

       IEnumerator Start()
       {
           if (string.IsNullOrEmpty(SettingKeyName))
           {
               ApplyCurrentValue();
               yield break;
           }

           while (!GameConfig.isDataCached) yield return null;
           Load();
       }

       private void OnDestroy()
       {
       }

       public void Load()
       {
           var val = GameCore.Settings.GetSettingOf(SettingKeyName);
           int v = 0;
           if (val is bool) { v = (bool)val == true ? 1 : 0; }
           else if (val is int)
           {
               v = (int)val;
           }
           else
           {
               Debug.LogWarning($"设置项 {SettingKeyName} 的类型无效。"); return;
           }
           currentOption = v;
           ApplyCurrentValue();
       }

       public void SetOptions(string[] newOptions, bool applySetting = true)
       {
           optionsNames = newOptions;
           currentOption = 0;
           ApplyCurrentValue(applySetting);

       }

       public void ApplyCurrentValue(bool applySetting = true)
       {
           if (currentOption == -1) return;
           if (currentOption >= optionsNames.Length) return;

           string value = autoUpperCase ? optionsNames[currentOption].ToUpper() : optionsNames[currentOption];
           if(optionText != null) optionText.text = value;
           if (optionTextTMP != null) optionTextTMP.text = value;

           if(applySetting) ApplySetting();
       }

       public void ApplySetting()
       {
           onChange?.Invoke(currentOption);
           if (string.IsNullOrEmpty(SettingKeyName)) return;

           //把修改后的设置值写入对应的设置组实例。
           GameCore.Settings.SetSettingOf(SettingKeyName, currentOption, SettingsManager.Instance.autoSaveSettings);
           if (SettingsManager.Instance.applySettingsOnChange)
           {
               //调用监听器在游戏中应用设置
               GameCore.Settings.ApplySettings(RuntimeSettingsProfile.ResolutionApplication.NoApply);
           }
       }

       public void ChangeOption(bool forward)
       {
           if (forward)
           {
               currentOption = (currentOption + 1) % optionsNames.Length;
           }
           else
           {
               if (currentOption <= 0) { currentOption = optionsNames.Length - 1; }
               else { currentOption--; }
           }
           ApplyCurrentValue();
       }

       public void SetActive(bool active) => gameObject.SetActive(active);

       public void SaveValue()
       {
           GameCore.Settings.SaveSettings();
       }

       public void Revert()
       {
           Load();
       }

       public void ResetSelection()
       {
           currentOption = 0;
           ApplyCurrentValue();
       }

       public void SetCurrentFromOptionName(string optionName)
       {
           for (int i = 0; i < optionsNames.Length; i++)
           {
               if(optionsNames[i] == optionName)
               {
                   currentOption = i;
                   break;
               }
           }
       }

#if UNITY_EDITOR
       private void OnValidate()
       {
           if (optionText == null || optionsNames == null || optionsNames.Length <= 0) return;
           if (currentOption >= optionsNames.Length) { currentOption = 0; return; }

           string value = autoUpperCase ? optionsNames[currentOption].ToUpper() : optionsNames[currentOption];
           optionText.text = value;
       }
#endif
   }
