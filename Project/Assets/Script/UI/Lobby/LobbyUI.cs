using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public string MainWindowName = "room list";
    public List<WindowUI> windows = new List<WindowUI>();
    public List<PopUpWindows> popUpWindows = new List<PopUpWindows>();

    [Header("References")]
    public CanvasGroup FadeAlpha;
    public GameObject PhotonStaticticsUI;
    public GameObject PhotonGamePrefab;
    public GameObject EnterPasswordUI;
    public GameObject SeekingMatchUI;
    public TextMeshProUGUI PlayerNameText = null;
    [SerializeField] private TextMeshProUGUI PasswordLogText = null;

    public Image LevelIcon;
    public TMP_InputField PlayerNameField = null;
    [SerializeField] private TMP_Dropdown ServersDropdown = null;
    public ContactListUI FriendUI;
    public LevelUI m_LevelUI;
    public GameObject overAllEmptyLoading;
    public ConfirmationWindow confirmationWindow;
    public ConfirmationWindow messageWindow;
    public ConfirmationWindow connectionPopupMessage;
    public CanvasGroupFader blackScreenFader;

    #region Private members
    private string currentWindow = "";
    private int requestedRegionID = -1;
    #endregion

    public void InitialSetup()
    {
        GameEvents.onGameSettingsChange += ApplyRuntimeSettings;
        windows.ForEach(x => { if (x.UIRoot != null) { x.UIRoot.SetActive(false); } });
        if (PhotonStaticticsUI != null) { PhotonStaticticsUI.SetActive(MatchLobby.Instance.ShowPhotonStatistics); }
        blackScreenFader.SetAlpha(1);
    }

    private void OnEnable()
    {
    }

    void OnDisable()
    {
        GameEvents.onGameSettingsChange -= ApplyRuntimeSettings;
    }

    public void ChangeWindow(string window)
    {
        if (window == currentWindow) return;
        WindowUI w = windows.Find(x => x.Name == window);
        if (w == null) return;

        StopCoroutine("DoChangeWindow");
        StartCoroutine("DoChangeWindow", w);
        currentWindow = window;
    }

    public void SetEnableWindow(string windowName, bool active)
    {
        WindowUI w = windows.Find(x => x.Name == windowName);
        if (w == null || w.UIRoot == null) return;

        w.UIRoot.SetActive(active);
        if (w.MenuButton != null) w.MenuButton.interactable = !active;
    }

    public void Home() { ChangeWindow(MainWindowName); MatchLobby.Instance.onShowMenu?.Invoke(); }
    public void HideAll() { currentWindow = ""; windows.ForEach(x => { if (x.UIRoot != null) { x.UIRoot.SetActive(false); } }); }

    IEnumerator DoChangeWindow(WindowUI window)
    {
        //现在切换窗口
        for (int i = 0; i < windows.Count; i++)
        {
            WindowUI w = windows[i];
            if (w.UIRoot == null) continue;

            if (w.isPersistent)
            {
                w.UIRoot.SetActive(!window.hidePersistents);
            }
            else
            {
                if (w.MenuButton != null) w.MenuButton.interactable = true;
                w.UIRoot.SetActive(false);
            }
            window.UIRoot.SetActive(true);
            if (window.MenuButton != null) window.MenuButton.interactable = false;
            if (window.showTopMenu)
            {
                SetEnableWindow("top menu", true);
            }
        }

        if (window.playFadeInOut)
        {
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime * 2;
                FadeAlpha.alpha = d;
                yield return null;
            }
        }
    }

    public void ShowPopUpWindow(string popUpName)
    {
        PopUpWindows w = popUpWindows.Find(x => x.Name == popUpName);
        if (w == null || w.Window == null) return;

        w.Window.SetActive(true);
    }

    private void Update()
    {
        if (NetworkWrapper.IsConnected && GameConfig.isDataCached)
        {
            PlayerNameText.text = GameCore.LocalPlayer.FullNickName();
        }
    }

    public void SignOut()
    {
        MatchLobby.Instance.SignOut();
    }

    #region Settings

    public void Setup()
    {
        LoadSettings();
        FullSetUp();
    }

    public void LoadSettings()
    {
        ApplyRuntimeSettings();

        bool km = PlayerPrefs.GetInt(NetworkKeys.KickKey, 0) == 1;
        if (km) { ShowPopUpWindow("kicked"); }
        if (NetworkWrapper.Instance.hasPingKick) { ShowPopUpWindow("ping kick"); NetworkWrapper.Instance.hasPingKick = false; }
        PlayerPrefs.SetInt(NetworkKeys.KickKey, 0);
        MatchLobby.Instance.rememberMe = !string.IsNullOrEmpty(PlayerPrefs.GetString(NetworkKeys.GetUniqueKey("remembernick"), string.Empty));
        if (NetworkWrapper.Instance.hasAFKKick) { ShowPopUpWindow("afk kick"); NetworkWrapper.Instance.hasAFKKick = false; }

        InputHandler.Initialize();
        InputHandler.CheckGamePadRequired();

        if (GameConfig.Instance.hostCanRemovePlayers)
        {
            PhotonNetwork.EnableCloseConnection = true;
        }
    }

    void ApplyRuntimeSettings()
    {
        if (GameCore.Settings != null)
        {
            var apm = ResolutionSettings.ResolutionHandler == null ? RuntimeSettingsProfile.ResolutionApplication.ApplyCurrent : RuntimeSettingsProfile.ResolutionApplication.NoApply;
            GameCore.Settings.ApplySettings(apm, false);
            Application.targetFrameRate = GameCore.Settings.RefreshRates[(int)GameCore.Settings.GetSettingOf("帧率上限")];
            GameCore.MusicVolume = (float)GameCore.Settings.GetSettingOf("音乐音量");
            AudioManager.Instance.ForceStopAllFades();
            AudioManager.Instance.BackgroundVolume = GameCore.MusicVolume;
            AudioManager.Instance.MaxBackgroundVolume = GameCore.MusicVolume;
        }
    }

    public void FullSetUp()
    {
        if (LevelIcon != null) LevelIcon.gameObject.SetActive(false);

        SetRegionDropdown();
    }

    public void ResetSettings()
    {
        LoadSettings();
        FullSetUp();
    }

    void SetRegionDropdown()
    {
        //当使用 Photon Server 时
        if (!PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            //禁用下拉框的服务器选择，因为它与 Photon Server 不兼容。
            ServersDropdown.gameObject.SetActive(false);
            return;
        }
        string key = PlayerPrefs.GetString(NetworkKeys.GetUniqueKey("preferredregion"), MatchLobby.Instance.DefaultServer.ToString());
        string[] Regions = Enum.GetNames(typeof(ServerRegionCode));
        for (int i = 0; i < Regions.Length; i++)
        {
            if (key == Regions[i])
            {
                int id = i;
                if (id > 4) { id--; }
                ServersDropdown.value = id;
                MatchLobby.Instance.ServerRegionID = id;
                break;
            }
        }
        ServersDropdown.RefreshShownValue();
        // 下拉框设置完成后再绑定回调，避免意外触发监听。
        ServersDropdown.onValueChanged.AddListener(ChangeServerCloud);
    }
    #endregion

    #region UI Callbacks
    public void EnterName(TMP_InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        int check = GameConfig.Instance.CheckPlayerName(field.text);
        if (check == 1)
        {
            MatchLobby.Instance.CachePlayerName = field.text;
            SetEnableWindow("user password", true);
            return;
        }
        else if (check == 2)
        {
            field.text = string.Empty;
            return;
        }
        MatchLobby.Instance.SetPlayerName(field.text);
    }

    public void EnterPassword(TMP_InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        string pass = field.text;
        if (!MatchLobby.Instance.EnterPassword(pass))
        {
            field.text = string.Empty;
        }
    }

    public void CheckRoomPassword(RoomInfo room)
    {
        EnterPasswordUI.SetActive(true);
        MatchLobby.Instance.CheckRoomPassword(room);
    }

    public void OnEnterPassworld(TMP_InputField pass)
    {
        if (!MatchLobby.Instance.SetRoomPassworld(pass.text))
        {
            PasswordLogText.text = "Wrong room password";
        }
    }

    public void ChangeServerCloud(int id)
    {
        requestedRegionID = id;
        confirmationWindow.AskConfirmation(GameTexts.ChangeRegionAsk, () =>
         {
             MatchLobby.Instance.ConnectToServerRegion(id);
         });
    }

    public void LoadLocalLevel(string level)
    {
        MatchLobby.Instance.LoadLocalLevel(level);
    }

    public void ShowBuyCoins()
    {
    }

    public void Quit()
    {
        confirmationWindow.AskConfirmation(GameTexts.QuitGameConfirmation, () =>
         {
#if UNITY_EDITOR
             EditorApplication.ExecuteMenuItem("Edit/Play");
#else
             Application.Quit();
#endif
         });
    }

    public WindowUI AddMainMenuOption(string name, GameObject window, bool showTopMenu = true)
    {
        var refWindow = windows.Find(x => x.Name == "home");

        if (window == null)
        {
            window = new GameObject(name);
            RectTransform rectTransform = window.AddComponent<RectTransform>();
            rectTransform.SetParent(refWindow.UIRoot.transform.parent, false);

            var refRect = refWindow.UIRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = refRect.anchorMin;
            rectTransform.anchorMax = refRect.anchorMax;
            rectTransform.pivot = refRect.pivot;
            rectTransform.sizeDelta = refRect.sizeDelta;
            rectTransform.anchoredPosition = refRect.anchoredPosition;
        }

        var templateButton = refWindow.MenuButton;
        var btnNew = Instantiate(templateButton.gameObject, templateButton.transform.parent, false);
        btnNew.name = name;
        Button btn = btnNew.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.UnregisterPersistentListener(btn.onClick, 0);
            var targetInfo = UnityEngine.Events.UnityEvent.GetValidMethodInfo(this, nameof(ChangeWindow), new Type[] { typeof(string) });
            UnityEngine.Events.UnityAction<string> methodDelegate = Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<string>), this, targetInfo) as UnityEngine.Events.UnityAction<string>;
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btn.onClick, methodDelegate, name);
#endif
        }
        else
        {
            btn.onClick.AddListener(() => ChangeWindow(name));
        }

        var btnText = btn.GetComponentInChildren<TextMeshProUGUI>(true);
        btnText.text = name.ToUpper();

        var windowUI = new WindowUI()
        {
            Name = name,
            UIRoot = window,
            MenuButton = btn,
            showTopMenu = showTopMenu,
            playFadeInOut = true
        };
        windows.Add(windowUI);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(btn);
            UnityEditor.EditorUtility.SetDirty(btnText);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        return windows[windows.Count - 1];
    }

    public static void ShowEmptyLoading(bool active) => Instance.overAllEmptyLoading.SetActive(active);
    public static void ShowOverAllMessage(string text) => Instance.messageWindow?.ShowMessage(text);
    public static void ShowConfirmationWindow(string text, Action onAccept, Action onCancel = null) => Instance.confirmationWindow?.AskConfirmation(text, onAccept, onCancel);

    public void AutoMatch() { MatchLobby.Instance.AutoMatch(); }
    public void CreateRoom() { MatchLobby.Instance.CreateRoom(); }
    public void SetRememberMe(bool value) { MatchLobby.Instance.SetRememberMe(value); }
    #endregion

    #region Classes
    [System.Serializable]
    public class WindowUI
    {
        public string Name;
        public GameObject UIRoot;
        public Button MenuButton;
        public bool isPersistent = false;
        public bool hidePersistents = false;
        public bool playFadeInOut = false;
        public bool showTopMenu = true;
    }

    [System.Serializable]
    public class PopUpWindows
    {
        public string Name;
        public GameObject Window;
    }

    [System.Serializable]
    public class LevelUI
    {
        public GameObject Root;
        public Image Icon;
        public TextMeshProUGUI LevelNameText;
    }

    private static LobbyUI _instance;
    public static LobbyUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LobbyUI>();
            }
            return _instance;
        }
    }
    #endregion
}
