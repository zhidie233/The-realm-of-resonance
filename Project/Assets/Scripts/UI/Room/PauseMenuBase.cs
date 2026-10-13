using UnityEngine;

public abstract class PauseMenuBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public int CurrentWindow 
    { 
        get; 
        set; 
    } = -1;

    /// <summary>
    /// 
    /// </summary>
    public bool isMenuOpen 
    { 
        get; 
        set; 
    } = false;

    /// <summary>
    /// 
    /// </summary>
    public enum LayoutPart
    {
        Header = 1,
        Body = 2,
        Footer = 4
    }

    public static bool IsMenuOpen => Instance.isMenuOpen;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowOpen"></param>
    public abstract void OpenMenu(string windowOpen = "");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowOpen"></param>
    public abstract void OpenWindow(string windowName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowIndex"></param>
    public abstract void OpenWindow(int windowIndex);

    /// <summary>
    /// 
    /// </summary>
    public abstract void CloseMenu();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowName"></param>
    public abstract void CloseWindow(string windowName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layouts"></param>
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