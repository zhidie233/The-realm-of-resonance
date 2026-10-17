﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Runtime.UI
{
    public class LoadoutDropdown : MonoBehaviour
    {
        private TMP_Dropdown dropdown;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
                int lid = (int)PlayerClass.Assault.GetSavePlayerClass();
                dropdown.value = lid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void OnChanged(int value)
        {
            var loadout = (PlayerClass)value;
            loadout.SavePlayerClass();
            GameEvents.DispatchPlayerClassChange(loadout);
        }
    }
}