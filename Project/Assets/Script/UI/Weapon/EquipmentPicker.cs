using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
           GameEvents.DispatchPlayerClassChange(preset);
       }
   }