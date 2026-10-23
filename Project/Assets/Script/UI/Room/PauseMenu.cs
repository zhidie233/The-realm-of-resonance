using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : PauseMenuBase
{
    #region Public members    
    public string firstWindow = "scoreboard";

    [Header("Windows")]
    public List<MenuWindow> windows;

    [Header("Events")]
    public GameEvents.UEvent onOpen;
    public GameEvents.UEvent onClose;

    [Header("References")]
    [SerializeField] GameObject content = null;
    [SerializeField] private GameObject[] layoutRoots = null;
    #endregion

    private void Start()
    {
        if (!IsMenuOpen) CloseMenu();
    }
    /// 打开暂停菜单
    public override void OpenMenu(string windowOpen = "")
    {
        var win = string.IsNullOrEmpty(windowOpen) ? firstWindow : windowOpen;
        SetActiveLayouts(LayoutPart.Header | LayoutPart.Body | LayoutPart.Footer);
        OpenWindow(win);
        isMenuOpen = true;
    }

    public override void OpenWindow(string windowName)
    {
        var id = windows.FindIndex(x => x.Name == windowName);
        if(id == -1)
        {
            Debug.LogWarning($"暂停窗口 {windowName} 不存在。");
            return;
        }
        OpenWindow(id);
    }

    public override void OpenWindow(int windowIndex)
    {
        if (windowIndex == CurrentWindow && isMenuOpen) return;

        content.SetActive(true);
        CurrentWindow = windowIndex;

        windows.ForEach(x => x.SetActive(false));

        var window = windows[CurrentWindow];
        window.SetActive(true);
        onOpen?.Invoke();
    }

    public override void CloseMenu()
    {
        isMenuOpen = false;
        onClose?.Invoke();
        content.SetActive(false);
    }

    public override void CloseWindow(string windowName)
    {
        var id = windows.FindIndex(x => x.Name == windowName);
        if (id != -1)
        {
            var win = windows[id];
            if (win.Window != null) win.SetActive(false);
        }
    }
    /// 仅启用给定的菜单布局部分（禁用其余部分）
    public override void SetActiveLayouts(LayoutPart layouts)
    {
        layoutRoots[0].SetActive(layouts.IsEnumFlagPresent(LayoutPart.Header));
        layoutRoots[1].SetActive(layouts.IsEnumFlagPresent(LayoutPart.Body));
        layoutRoots[2].SetActive(layouts.IsEnumFlagPresent(LayoutPart.Footer));
    }

    public bool IsWindowOpen(string windowName)
    {
        if (CurrentWindow < 0) return false;
        return windows[CurrentWindow].Name == windowName;
    }

    [Serializable]
    public class MenuWindow
    {
        public string Name;
        public GameObject Window;
        public Button OpenButton;
        public GameEvents.UEvent onOpen;

        public void SetActive(bool active)
        {
            if (Window != null) Window.SetActive(active);
            if (OpenButton != null) OpenButton.interactable = !active;
            onOpen?.Invoke();
        }
    }
}
