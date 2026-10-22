using UnityEngine;

   public class InputDeviceSwitcher : MonoBehaviour
   {
       private bool init = false;
       private SingleSettingsBinding settingsBinding;

       private void Awake()
       {
           settingsBinding = GetComponent<SingleSettingsBinding>();           
       }

       private void OnEnable()
       {
           InitialSetup();
       }

       void InitialSetup()
       {
           if (init) return;

           InputType currentType = InputBindings.Instance.mappedInstance.inputType;
           int id = 0;
           if(currentType == InputType.Xbox || currentType == InputType.Playstation)
           {
               id = 1;
           }

           settingsBinding.currentOption = id;
           settingsBinding.ApplyCurrentValue();
           init = true;
       }
   }
