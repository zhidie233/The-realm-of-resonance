﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameRateManager : MonoBehaviour
{
    public Dropdown optionDropdown;

    /// <summary>
    /// 
    /// </summary>
    void OnStart()
    {
        if(optionDropdown != null && GameCore.Settings != null)
        {
            optionDropdown.ClearOptions();
            List<Dropdown.OptionData> ol = new List<Dropdown.OptionData>();
            int[] options = GameCore.Settings.RefreshRates;
            for (int i = 0; i < options.Length; i++)
            {
                if(options[i] == 0) { ol.Add(new Dropdown.OptionData() { text = "UNLIMITED" }); continue; }
                ol.Add(new Dropdown.OptionData() { text = options[i].ToString() });
            }
            optionDropdown.AddOptions(ol);
            int df = PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKey("framerateid"), (int)GameCore.Settings.GetSettingOf("帧率上限"));
            optionDropdown.value = df;
            Application.targetFrameRate = GameCore.Settings.RefreshRates[df];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnChange(int option)
    {        
        Application.targetFrameRate = GameCore.Settings.RefreshRates[option];
        PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKey("framerateid"), option);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnChangeCustom(int option)
    {
        Application.targetFrameRate = GameCore.Settings.RefreshRates[option];
    }
}