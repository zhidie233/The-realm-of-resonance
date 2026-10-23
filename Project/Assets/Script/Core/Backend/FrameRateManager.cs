using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameRateManager : MonoBehaviour
{
    public Dropdown optionDropdown;

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
            int df = PlayerPrefs.GetInt(NetworkKeys.GetUniqueKey("framerateid"), (int)GameCore.Settings.GetSettingOf("帧率上限"));
            optionDropdown.value = df;
            Application.targetFrameRate = GameCore.Settings.RefreshRates[df];
        }
    }

    public void OnChange(int option)
    {        
        Application.targetFrameRate = GameCore.Settings.RefreshRates[option];
        PlayerPrefs.SetInt(NetworkKeys.GetUniqueKey("framerateid"), option);
    }

    public void OnChangeCustom(int option)
    {
        Application.targetFrameRate = GameCore.Settings.RefreshRates[option];
    }
}