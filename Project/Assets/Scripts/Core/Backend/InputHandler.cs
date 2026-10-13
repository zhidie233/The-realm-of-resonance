using UnityEngine;
using System.Collections.Generic;
using GFWK.InputManager;
using UnityEngine.UI;

public static class InputHandler
{
    public static InputBindings InputData => InputBindings.Instance;

    public static InputType inputType
    {
        get { return InputBindings.Instance.inputType; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Initialize()
    {
        InputBindings.Instance.Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButton(string keyName)
    {
        return InputBindings.Instance.GetButton(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButtonDown(string keyName)
    {
        return InputBindings.Instance.GetButtonDown(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButtonUp(string keyName)
    {
        return InputBindings.Instance.GetButtonUp(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public static string GetButtonName(string keyName)
    {
        return InputBindings.Instance.GetButtonName(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static InputType GetInputType()
    {
        string[] names = Input.GetJoystickNames();
        InputType t = InputType.Keyboard;
        for(int  i = 0; i < names.Length; i++)
        {
            Debug.Log("Joystick: " + names[i]);
        }
        return t;
    }

    /// <summary>
    /// Use this instead of Input.GetAxis("Vertical");
    /// </summary>
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

    /// <summary>
    /// start button on game pad controllers
    /// </summary>
    public static bool isStartPad
    {
        get
        {
            return Input.GetKeyDown("joystick button 7");
        }
    }

    /// <summary>
    /// Use this instead of Input.GetAxis("Horizontal");
    /// </summary>
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