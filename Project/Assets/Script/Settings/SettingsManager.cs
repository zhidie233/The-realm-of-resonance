using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Runtime.Settings
{
    public class SettingsManager : MonoBehaviour
    {
        public int defaultWindow = 0;
        public WindowUI[] windows;

        [Header("Settings")]
        public bool autoSaveSettings = false;
        public bool applySettingsOnChange = false;

        [Header("References")]
        public TextMeshProUGUI titleText;

        private int currentWindow = -1;

        private void Awake()
        {
            ChangeWindow(defaultWindow);
        }

        /// <summary>
        /// 切换设置 UI 窗口
        /// </summary>
        public void ChangeWindow(int id)
        {
            if (currentWindow == id) return;

            foreach (var item in windows)
            {
                item?.SetActive(false);
                item.button.interactable = true;
            }
            windows[id].SetActive(true);

            if(titleText != null)
            {
                titleText.text = windows[id].Window.name.Localized(windows[id].Window.name.ToLower()).ToUpper();
            }
        }

        public void RevertAll()
        {
            var all = transform.GetComponentsInChildren<SingleSettingsBinding>(true);
            foreach (var item in all)
            {
                item.Revert();
            }
            var alls = transform.GetComponentsInChildren<SingleSettingsSlider>(true);
            foreach (var item in alls)
            {
                item.Revert();
            }

            var im = transform.GetComponentInChildren<InputManager.InputUI>();
            if(im != null)
            {
                im.RevertChanges();
            }
        }

        public void ApplySettings()
        {
            GameCore.Settings.SaveSettings();
            GameCore.Settings.ApplySettings(RuntimeSettingsProfile.ResolutionApplication.ApplyUserSelected);

            var im = transform.GetComponentInChildren<InputManager.InputUI>();
            if (im != null)
            {
                im.ApplyChanges();
            }
        }

        [System.Serializable]
        public class WindowUI
        {
            public GameObject Window;
            public Button button;

            public void SetActive(bool active)
            {
                Window.SetActive(active);
                button.interactable = !active;
            }
        }

        private static SettingsManager _instance = null;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<SettingsManager>(); }
                return _instance;
            }
        }
    }
}