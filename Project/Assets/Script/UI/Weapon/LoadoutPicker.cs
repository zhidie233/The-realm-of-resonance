using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Runtime.UI
{
    public class LoadoutPicker : MonoBehaviour
    {
        private TMP_Dropdown dropdown;

        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
                int lid = (int)PlayerClass.Assault.GetSavePlayerClass();
                dropdown.value = lid;
            }
        }

        public void OnChanged(int value)
        {
            var loadout = (PlayerClass)value;
            loadout.SavePlayerClass();
            GameEvents.DispatchPlayerClassChange(loadout);
        }
    }
}