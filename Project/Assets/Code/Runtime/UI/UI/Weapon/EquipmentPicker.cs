using UnityEngine;
using TMPro;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
namespace Resonance.UI
{

    public class EquipmentPicker : MonoBehaviour
    {
        private TMP_Dropdown dropdown;

        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
                int lid = (int)PlayerClass.Vanguard.GetSavePlayerClass();
                dropdown.value = lid;
            }
        }

        public void OnChanged(int value)
        {
            var preset = (PlayerClass)value;
            preset.SavePlayerClass();
            EventBus.Publish(new PlayerClassChangedEvent(preset));
        }
    }
}
