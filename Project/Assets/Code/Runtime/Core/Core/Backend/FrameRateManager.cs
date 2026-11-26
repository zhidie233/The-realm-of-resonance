using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{

    public class FrameRateManager : MonoBehaviour
    {
        public Dropdown optionDropdown;

        void OnStart()
        {
            if(optionDropdown != null && RuntimeServices.Settings != null)
            {
                optionDropdown.ClearOptions();
                List<Dropdown.OptionData> ol = new List<Dropdown.OptionData>();
                int[] options = RuntimeServices.Settings.RefreshRates;
                for (int i = 0; i < options.Length; i++)
                {
                    if(options[i] == 0) { ol.Add(new Dropdown.OptionData() { text = "UNLIMITED" }); continue; }
                    ol.Add(new Dropdown.OptionData() { text = options[i].ToString() });
                }
                optionDropdown.AddOptions(ol);
                int df = PlayerPrefs.GetInt(NetProtocol.GetUniqueKey("framerateid"), (int)RuntimeServices.Settings.GetSettingOf("帧率上限"));
                optionDropdown.value = df;
                Application.targetFrameRate = RuntimeServices.Settings.RefreshRates[df];
            }
        }

        public void OnChange(int option)
        {        
            Application.targetFrameRate = RuntimeServices.Settings.RefreshRates[option];
            PlayerPrefs.SetInt(NetProtocol.GetUniqueKey("framerateid"), option);
        }

        public void OnChangeCustom(int option)
        {
            Application.targetFrameRate = RuntimeServices.Settings.RefreshRates[option];
        }
    }
}
