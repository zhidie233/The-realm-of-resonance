using UnityEngine;

public abstract class PauseMenuBase : MonoBehaviour
{
    public int CurrentWindow 
    { 
        get; 
        set; 
    } = -1;

    public bool isMenuOpen 
    { 
        get; 
        set; 
    } = false;

    public enum LayoutPart
    {
        Header = 1,
        Body = 2,
        Footer = 4
    }

    public static bool IsMenuOpen => Instance.isMenuOpen;

    public abstract void OpenMenu(string windowOpen = "");

    public abstract void OpenWindow(string windowName);

    public abstract void OpenWindow(int windowIndex);

    public abstract void CloseMenu();

    public abstract void CloseWindow(string windowName);

    public abstract void SetActiveLayouts(LayoutPart layouts);

    private static PauseMenuBase _instance;
    public static PauseMenuBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<PauseMenuBase>(); }
            return _instance;
        }
    }
}