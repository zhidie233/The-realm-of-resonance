using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public static class InputHandler
{
    public static InputBindings InputData => InputBindings.Instance;

    public static InputType inputType
    {
        get { return InputBindings.Instance.inputType; }
    }

    public static void Initialize()
    {
        InputBindings.Instance.Initialize();
    }

    public static bool isButton(string keyName)
    {
        return InputBindings.Instance.GetButton(keyName);
    }

    public static bool isButtonDown(string keyName)
    {
        return InputBindings.Instance.GetButtonDown(keyName);
    }

    public static bool isButtonUp(string keyName)
    {
        return InputBindings.Instance.GetButtonUp(keyName);
    }

    public static string GetButtonName(string keyName)
    {
        return InputBindings.Instance.GetButtonName(keyName);
    }

    public static InputType GetInputType()
    {
        string[] names = Input.GetJoystickNames();
        InputType t = InputType.Keyboard;
        for(int  i = 0; i < names.Length; i++)
        {
        }
        return t;
    }
    /// 用它代替 Input.GetAxis("Vertical");
    public static float VerticalAxis
    {
        get
        {
            if (!isGamePad)
            {
                if (isButton("Forward") && !isButton("Backward"))
                {
                    return 1;
                }
                else if (!isButton("Forward") && isButton("Backward"))
                {
                    return -1;
                }
                else if (isButton("Forward") && isButton("Backward"))
                {
                    return 0.5f;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return Input.GetAxis("Vertical");
            }
        }
    }
    /// 手柄上的开始按钮
    public static bool isStartPad
    {
        get
        {
            return Input.GetKeyDown("joystick button 7");
        }
    }
    /// 用它代替 Input.GetAxis("Horizontal");
    public static float HorizontalAxis
    {
        get
        {
            if (!isGamePad)
            {
                if (isButton("Right") && !isButton("Left"))
                {
                    return 1;
                }
                else if (!isButton("Right") && isButton("Left"))
                {
                    return -1;
                }
                else if (isButton("Right") && isButton("Left"))
                {
                    return 0.5f;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return Input.GetAxis("Horizontal");
            }
        }
    }

    public static void CheckGamePadRequired()
    {
        if (InputBindings.Instance.mappedInstance == null || !InputBindings.Instance.useGamePadNavigation) return;
        if (InputBindings.Instance.mappedInstance.inputType != InputType.Xbox && InputBindings.Instance.mappedInstance.inputType != InputType.Playstation)
            return;

        GamePadPointerModuleHandler dpm = Object.FindObjectOfType<GamePadPointerModuleHandler>();
        if(dpm == null)
        {
            GameObject go = GameObject.Instantiate(InputBindings.Instance.GamePadInputModule) as GameObject;
            dpm = go.GetComponent<GamePadPointerModuleHandler>();
            dpm.CheckCanvas();
        }

        GamePadPointer gmp = GamePadPointer.Instance; 
        if(gmp == null)
        {
            UIReferences uir = UIReferences.Instance;
            GameObject go = GameObject.Instantiate(InputBindings.Instance.GamePadPointerPrefab) as GameObject;
            gmp = go.GetComponent<GamePadPointer>();
            if (uir != null)
            {
                Transform parent = uir.transform.GetChild(1);
                go.transform.SetParent(parent, false);
                go.transform.SetAsLastSibling();
            }
            else
            {
                GraphicRaycaster c = GameObject.FindObjectOfType<GraphicRaycaster>();
                Transform parent = c.transform;
                go.transform.SetParent(parent, false);
                go.transform.SetAsLastSibling();
            }
        }
    }

    public static bool isGamePad { get { return InputBindings.Instance.inputType == InputType.Playstation || InputBindings.Instance.inputType == InputType.Xbox; } }

}
