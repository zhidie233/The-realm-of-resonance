﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Game.Runtime.Settings
{
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


        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
        }

        /// <summary>
        /// 
        /// </summary>
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
                Debug.LogWarning($"Setting {SettingKeyName} is a invalid type"); return;
            }
            currentOption = v;
            ApplyCurrentValue();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetOptions(string[] newOptions, bool applySetting = true)
        {
            optionsNames = newOptions;
            currentOption = 0;
            ApplyCurrentValue(applySetting);

        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplyCurrentValue(bool applySetting = true)
        {
            if (currentOption == -1) return;
            if (currentOption >= optionsNames.Length) return;

            string value = autoUpperCase ? optionsNames[currentOption].ToUpper() : optionsNames[currentOption];
            if(optionText != null) optionText.text = value;
            if (optionTextTMP != null) optionTextTMP.text = value;

            if(applySetting) ApplySetting();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplySetting()
        {
            onChange?.Invoke(currentOption);
            if (string.IsNullOrEmpty(SettingKeyName)) return;

            //Set the changed setting value to the instance settings group.
            GameCore.Settings.SetSettingOf(SettingKeyName, currentOption, SettingsManager.Instance.autoSaveSettings);
            if (SettingsManager.Instance.applySettingsOnChange)
            {
                //call the listener that will apply the settings in game
                GameCore.Settings.ApplySettings(RuntimeSettingsProfile.ResolutionApplication.NoApply);
            }
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active) => gameObject.SetActive(active);

        /// <summary>
        /// 
        /// </summary>
        public void SaveValue()
        {
            GameCore.Settings.SaveSettings();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Revert()
        {
            Load();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetSelection()
        {
            currentOption = 0;
            ApplyCurrentValue();
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        void Localize(Dictionary<string,string> data)
        {
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
}