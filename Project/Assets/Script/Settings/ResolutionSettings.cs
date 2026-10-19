using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Runtime.Settings
{
    public class ResolutionSettings : MonoBehaviour
    {
        public enum SettingType
        {
            DisplayMode,
            Resolution,
        }

        public class ResolutionData
        {
            public int Index;
            public string Name;
            public Resolution resolution;
        }

        public class ResolutionHandlerData
        {
            public ResolutionData[] resolutions;
            public int CurrentResolutionAbsoluteID = 0;
            public int CurrentResolutionRelativeID = 0;

            public Resolution GetResolution(int resolutionID)
            {
                int index = resolutions.ToList().FindIndex(x => x.Index == resolutionID);
                if (index < 0) return resolutions[0].resolution;

                return resolutions[index].resolution;
            }

            public Resolution GetRelativeResolution()
            {
                return resolutions[CurrentResolutionRelativeID].resolution;
            }

            public Resolution GetRelativeResolution(int resolutionID)
            {
                return resolutions[resolutionID].resolution;
            }

            public Resolution GetResolution()
            {
                return Screen.resolutions[CurrentResolutionAbsoluteID];
            }

            public ResolutionData GetCurrentResolutionData()
            {
                var currentRest = Screen.currentResolution;
                for (int i = 0; i < resolutions.Length; i++)
                {
                    if (resolutions[i].resolution.width == currentRest.width && resolutions[i].resolution.height == currentRest.height)
                    {
                        return resolutions[i];
                    }
                }
                return null;
            }

            public string[] GetResolutionNames()
            {
                return resolutions.Select(x => x.Name).ToArray();
            }
        }

        public SettingType settingType = SettingType.DisplayMode;
        public SingleSettingsBinding settingsBinding;

        public static ResolutionHandlerData ResolutionHandler;

        private void Start()
        {
            SetupResolutions();
            SetupDisplayMode();
        }

        private void SetupDisplayMode()
        {
            if (settingType != SettingType.DisplayMode) return;

            var modes = Enum.GetNames(typeof(FullScreenMode));
            var names = new string[modes.Length];

            for (int i = 0; i < modes.Length; i++)
            {
                names[i] = StringHelpers.NicifyVariableName(modes[i]);
                if (settingsBinding.autoUpperCase)
                {
                    names[i] = names[i].ToUpper();
                }
            }

            settingsBinding.SetOptions(names, false);

            int displayModeID = (int)GameCore.Settings.GetSettingOf("Display Mode");
            if (displayModeID == -1)
            {
                settingsBinding.currentOption = (int)Screen.fullScreenMode;
                settingsBinding.ApplyCurrentValue(false);
            }
            else
            {
                settingsBinding.currentOption = displayModeID;
                settingsBinding.ApplyCurrentValue(false);
            }
        }

        private void SetupResolutions()
        {
            if (settingType != SettingType.Resolution) return;

            FetchResolutions();

            var nameList = ResolutionHandler.GetResolutionNames();

            settingsBinding.SetOptions(nameList.ToArray(), false);
            settingsBinding.currentOption = ResolutionHandler.CurrentResolutionRelativeID;
        }

        public void OnChangeDisplayMode(int id)
        {
            GameCore.Settings.SetSettingOf("Display Mode", id, SettingsManager.Instance.autoSaveSettings);
        }

        public void OnChangeResolution(int id)
        {
            if (ResolutionHandler == null) return;

            ResolutionHandler.CurrentResolutionRelativeID = id;
            ResolutionHandler.CurrentResolutionAbsoluteID = ResolutionHandler.resolutions[id].Index;
            GameCore.Settings.SetSettingOf("Resolution", ResolutionHandler.CurrentResolutionAbsoluteID, SettingsManager.Instance.autoSaveSettings);
        }

        public static void FetchResolutions()
        {
            var resolutions = Screen.resolutions;
            var list = new List<ResolutionData>();
            var currentRes = Screen.currentResolution;

            if (GameCore.Settings.HasSettingDefinedFor("Resolution"))
            {
                int crid = (int)GameCore.Settings.GetSettingOf("Resolution");
                if (crid != -1 && crid <= resolutions.Length - 1)
                {
                    currentRes = resolutions[crid];
                }
            }

            ResolutionHandler = new ResolutionHandlerData();

            for (int i = resolutions.Length-1; i >0; i--)
            {
                var cr = resolutions[i];
                if (!list.Exists(x => x.resolution.width == cr.width && x.resolution.height == cr.height))
                {
                    list.Add(new ResolutionData()
                    {
                        Index = i,
                        resolution = cr,
                        Name = $"{cr.width} X {cr.height}"
                    });
                    if (currentRes.width == cr.width && currentRes.height == cr.height)
                    {
                        ResolutionHandler.CurrentResolutionAbsoluteID = i;
                        ResolutionHandler.CurrentResolutionRelativeID = list.Count - 1;
                    }
                }
            }
            ResolutionHandler.resolutions = list.ToArray();
        }
    }
}