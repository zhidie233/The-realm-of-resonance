using System.Collections.Generic;
using UnityEngine;
   [CreateAssetMenu(fileName = "InputManager", menuName = "项目/Input/Manager")]
   public class InputBindings : ScriptableObject
   {

       [SerializeField, ScriptableDrawer] private ButtonMapped Mapped = null;
       public string inputVersion = "1.0.0";
       [GameToggle] public bool useGamePadNavigation = false;
       [GameToggle] public bool runWithButton = false;

       [Header("References")]
       public GameObject GamePadInputModule;
       public GameObject GamePadPointerPrefab;

       [Header("Mapped Options")]
       public ButtonMapped[] mappedOptions;

       public ButtonMapped mappedInstance { get; set; }
       private Dictionary<string, ButtonData> cachedKeys = new Dictionary<string, ButtonData>();
       public const string KEYS = "resonancerealm.input.bindings";

       public InputType inputType
       {
           get
           {
               if (Mapped != null) return Mapped.inputType;
               return InputType.Keyboard;
           }
       }

       public void Initialize()
       {
           cachedKeys.Clear();
           mappedInstance = null;
           LoadMapped();
       }

       void LoadMapped()
       {
           if (PlayerPrefs.HasKey(MappedBindingKey))
           {
               string json = PlayerPrefs.GetString(MappedBindingKey);
               mappedInstance = Instantiate(Mapped);
               mappedInstance.mapped = JsonUtility.FromJson<Mapped>(json);
           }
           else
           {
               mappedInstance = Instantiate(Mapped);
           }
       }

       public void ChangeMapped(int mappedID)
       {
           Mapped = mappedOptions[mappedID];
           Initialize();
           if(Mapped.inputType != InputType.Keyboard)
           {
               Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
           }
       }

       public bool GetButton(string key)
       {
           if (!IsCached(key)) return false;
           return cachedKeys[key].isButton();
       }

       public bool GetButtonDown(string key)
       {
           if (!IsCached(key)) return false;
           return cachedKeys[key].isButtonDown();
       }

       public bool GetButtonUp(string key)
       {
           if (!IsCached(key)) return false;
           return cachedKeys[key].isButtonUp();
       }
       /// 获取主按键名称
       public string GetButtonName(string key)
       {
           if (!IsCached(key)) return "None";
           return cachedKeys[key].GetInputName();
       }

       private bool IsCached(string key)
       {
           if(mappedInstance == null) { Initialize(); }
           if (!cachedKeys.ContainsKey(key))
           {
               int index = mappedInstance.ButtonMap.FindIndex(x => x.KeyName.Equals(key));
               if (index >= 0) { cachedKeys.Add(key, mappedInstance.ButtonMap[index]); }
               else
               {
                   return false;
               }
           }
           return true;
       }

       public void SaveMappedInstance()
       {
           string json = JsonUtility.ToJson(mappedInstance.mapped);
           PlayerPrefs.SetString(MappedBindingKey, json);
       }

       public void RevertMappedInstance()
       {
           LoadMapped();
       }

       public string MappedBindingKey => Mapped == null ? KEYS : $"{KEYS}.{(short)Mapped.inputType}.{inputVersion}";
       public ButtonMapped DefaultMapped => Mapped;

       private static InputBindings m_Data;
       public static InputBindings Instance
       {
           get
           {
               if (m_Data == null)
               {
                   m_Data = Resources.Load("InputManager", typeof(InputBindings)) as InputBindings;
               }
               return m_Data;
           }
       }
   }
