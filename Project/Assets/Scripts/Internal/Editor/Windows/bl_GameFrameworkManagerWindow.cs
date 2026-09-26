//#define AUTO_REFRESH
#define ASCOMPLIANCE
using GFWKEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class bl_GFWKManagerWindow : EditorWindow
{
    #region Parameters
    public static Color primaryColor = new Color(0.098f, 0.098f, 0.098f, 1.00f);
    public static Color altColor = new Color(0.07f, 0.07f, 0.07f, 1.00f);
    public static Color altColorLight = new Color(0.5411765f, 0.5411765f, 0.5411765f, 1.00f);
    public static Color hightlightColor = new Color(1f, 0.9882353f, 0.003921569f, 1.00f);
    public static Color whiteColor = new Color(0.754717f, 0.754717f, 0.754717f, 1.00f);

    private List<ManagerPanel> managerPanels = new List<ManagerPanel>()
    {
    new ManagerPanel(){Name = "游戏数据", BodyFuncName = nameof(DrawGameDataSettings)},
    new ManagerPanel(){Name = "玩家选择器", BodyFuncName = nameof(DrawPlayerSelector)},
    new ManagerPanel(){Name = "兵种自定义", BodyFuncName = nameof(DrawClassCustomizer)},
    new ManagerPanel(){Name = "Customizer", BodyFuncName = nameof(DrawCustomizer)},
    new ManagerPanel(){Name = "关卡管理器", BodyFuncName = nameof(DrawlevelManager)},
    new ManagerPanel(){Name = "输入管理器", BodyFuncName = nameof(DrawInputManager)},
    };
    private WindowType currentWindow = WindowType.Home;
    private string currentPanelType = "";
    public ManagerPanel currentPanel = null;
    public Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle>() { { "panelButton", null }, { "titleH2", null }, { "borders", null }, { "textC", null }, { "miniText", null }, { "text", null } };
    public bool m_initGUI = false;
    private bl_GameData gameData;
    private Vector2 bodyScroll = Vector2.zero;
    private Texture2D gfwkLogo, soldierIcon;
    SearchField weaponSearchfield, managerSearchField;
    private string weaponSearchKey, managerSearchKey = "";
    readonly string[] slotsNames = new string[] { "Assault", "Recon", "Support", "Engineer" };
    public int currentSlot, currentSlot2 = 0;
    private bl_PlayerClassLoadout selectedLoadout = null;
    private int selectedSlot = -1;
    private Vector2 weaponScroll, managersScroll = Vector2.zero;
    private PlayerClass currentPlayerClass = PlayerClass.Assault;
    private Dictionary<ScriptableObject, Editor> cachedEditors = new Dictionary<ScriptableObject, Editor>();
    public int editWeaponId = -1;
    public bl_GunInfo editGunInfo;
    public GameModeSettings selectedGameMode = null;
    public SerializedObject serializedObject;
    private Rect windowRect;
    private Type cachedType;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        titleContent = new GUIContent(" GFWK", GetUnityIcon("音频混音器"));
        gfwkLogo = Resources.Load("content/Images/gfwk-name", typeof(Texture2D)) as Texture2D;
        soldierIcon = AssetDatabase.LoadAssetAtPath("Assets/Art/UI/Icons/framework-soldier.png", typeof(Texture2D)) as Texture2D;
        minSize = new Vector2(720, 570);
        weaponSearchfield = new SearchField();
        managerSearchField = new SearchField();
        GFWorksStats.SetStat("gfwk-manager-window", 1);
        currentPlayerClass = currentPlayerClass.GetSavePlayerClass();
        cachedType = this.GetType();
        FetchCustomTabs();
        selectedGameMode = null;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        var cColor = GUI.contentColor;
        GUI.contentColor = whiteColor;

        windowRect = position;
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), EditorGUIUtility.isProSkin ? altColor : altColorLight);
        if (!m_initGUI || styles["panelButton"] == null) InitGUI();

        GUILayout.BeginHorizontal();
        LeftPanel();
        EditorGUILayout.BeginVertical();
        Header();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical();
        DrawBody();
        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUI.contentColor = cColor;

#if AUTO_REFRESH
        if (!Application.isPlaying) Repaint();
#endif

    }

    /// <summary>
    /// 
    /// </summary>
    void Header()
    {
        Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Height(60));
        EditorGUI.DrawRect(r, primaryColor);
        GUILayout.FlexibleSpace();
        DrawWindowButtons();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWindowButtons()
    {
        WindowButton("管理器", WindowType.Managers);
        WindowButton("武器", WindowType.Weapons);
        WindowButton("装备配置", WindowType.Loadouts);
        WindowButton("游戏模式", WindowType.GameModes);
#if LM
        WindowButton("关卡", WindowType.Levels);
#endif

        GUILayout.Space(25);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="windowType"></param>
    void WindowButton(string title, WindowType windowType)
    {
        Rect r = GUILayoutUtility.GetRect(new GUIContent(title), styles["textC"], GUILayout.Height(30), GUILayout.Width(100));
        r.y += 30;
        if (currentWindow == windowType)
        {
            EditorGUI.DrawRect(r, altColor);
            title = $"<b>{title}</b>";
        }
        else
        {
            EditorGUI.DrawRect(r, GetHexColor("#12121255"));
        }
        if (GUI.Button(r, title, styles["textC"]))
        {
            currentWindow = windowType;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void LeftPanel()
    {
        Rect r = EditorGUILayout.BeginVertical(GUILayout.Width(150));
        EditorGUI.DrawRect(r, primaryColor);
        GUILayout.Label(gfwkLogo, GUILayout.Height(58), GUILayout.Width(150));
        DrawBottomLine(0.2f);
        if (currentWindow == WindowType.Managers || currentWindow == WindowType.Home)
            DrawManagerButtons();
        if (currentWindow == WindowType.Weapons) DrawWeaponPanel();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawBody()
    {
        GUILayout.Space(10);
        bodyScroll = GUILayout.BeginScrollView(bodyScroll, false, false);
        if (currentWindow == WindowType.Managers) DrawManagersBody();
        else if (currentWindow == WindowType.Weapons) DrawWeaponsBody();
        else if (currentWindow == WindowType.Loadouts) DrawLoadoutsBody();
        else if (currentWindow == WindowType.Home) DrawHomeBody();
        else if (currentWindow == WindowType.GameModes) DrawGameModes();
#if LM
        else if (currentWindow == WindowType.Levels) LevelManagerWindowEditor.DrawLevels();
#endif
        GUILayout.FlexibleSpace();
        GUILayout.Space(100);
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawHomeBody()
    {
        GUILayout.Label("请选择游戏设置窗口", styles["textC"]);
        GUILayout.Space(20);
        Rect r;
        for (int i = 0; i < managerPanels.Count; i++)
        {
            r = GUILayoutUtility.GetRect(new GUIContent(managerPanels[i].Name), styles["panelButton"], GUILayout.Height(30));
            if (currentPanelType == managerPanels[i].Name)
            {
                // EditorGUI.DrawRect(r, altColor);
                float w = r.width;
                r.width = 1;
                EditorGUI.DrawRect(r, hightlightColor);
                r.width = w;
            }
            else
            {
                EditorGUI.DrawRect(r, primaryColor);
            }
            if (GUI.Button(r, managerPanels[i].Name, styles["panelButton"]))
            {
                bodyScroll = Vector2.zero;
                currentPanelType = managerPanels[i].Name;
                currentPanel = managerPanels[i];
                currentWindow = WindowType.Managers;
            }
            GUILayout.Space(4);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawManagersBody()
    {
        if (currentPanel == null) return;

        currentPanel.DrawBody(cachedType, this);
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWeaponsBody()
    {
        GUILayout.Space(10);
        if (editWeaponId != -1)
        {
            DrawEditWeapon();
            return;
        }

        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 75;
        var sls = GUI.skin.horizontalSlider;
        GUI.skin.horizontalSlider = GFWKEditorStyles.EditorSkin.customStyles[9];
        var slt = GUI.skin.horizontalSliderThumb;
        GUI.skin.horizontalSliderThumb = GFWKEditorStyles.EditorSkin.customStyles[10];

        var all = bl_GameData.Instance.AllWeapons;
        int rowID = 0;
        int SpaceBetweenRow = 20;
        int rowCapacity = Mathf.FloorToInt((position.width - 150) / (370 + SpaceBetweenRow));
        for (int i = 0; i < all.Count; i++)
        {
            if (!string.IsNullOrEmpty(weaponSearchKey))
            {
                if (all[i].Name.ToLower().Contains(weaponSearchKey))
                {
                    WeaponCard(all[i], i);
                    GUILayout.Space(SpaceBetweenRow);
                    continue;
                }
                else continue;
            }

            if (rowID == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }
            WeaponCard(all[i], i);
            GUILayout.Space(10);
            if (rowID == (rowCapacity - 1) || i == all.Count - 1)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(SpaceBetweenRow);
            }
            rowID = (rowID + 1) % rowCapacity;
        }

        EditorGUIUtility.labelWidth = lw;
        GUI.skin.horizontalSlider = sls;
        GUI.skin.horizontalSliderThumb = slt;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawEditWeapon()
    {
        serializedObject.UpdateIfRequiredOrScript();

        GUILayout.Space(18);
        var w = editGunInfo;

        DrawTitleText("General");
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(20);
            Rect iconR = GUILayoutUtility.GetRect(120, 70);
            GUI.DrawTexture(iconR, w.GunIcon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(20);
            w.GunIcon = EditorGUILayout.ObjectField(w.GunIcon, typeof(Sprite), false, GUILayout.Width(120), GUILayout.Height(70)) as Sprite;
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(18);

        w.Name = EditorGUILayout.TextField("武器名称", w.Name);
        w.Type = (GunType)EditorGUILayout.EnumPopup("武器类型", w.Type);

        GUILayout.Space(18);
        DrawTitleText("Specs");

        w.Damage = EditorGUILayout.IntField("伤害", w.Damage);
        w.FireRate = EditorGUILayout.FloatField("射速", w.FireRate);
        w.ReloadTime = EditorGUILayout.FloatField("换弹时间", w.ReloadTime);
        w.Range = EditorGUILayout.IntField("射程", w.Range);
        w.Accuracy = EditorGUILayout.IntField("精准度", w.Accuracy);
        w.Weight = EditorGUILayout.FloatField("重量", w.Weight);

        GUILayout.Space(18);
        DrawTitleText("Unlockability");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("editGunInfo").FindPropertyRelative("Unlockability"), true);

        GUILayout.Space(18);
        DrawTitleText("Others");

        w.PickUpPrefab = EditorGUILayout.ObjectField("枪械拾取预制体", w.PickUpPrefab, typeof(bl_GunPickUpBase), false) as bl_GunPickUpBase;

        GUILayout.Space(18);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("取消", GFWKEditorStyles.EditorSkin.customStyles[11], GUILayout.Width(100)))
            {
                editWeaponId = -1;
                editGunInfo = null;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("保存", GFWKEditorStyles.EditorSkin.customStyles[11], GUILayout.Width(110)))
            {
                EditorUtility.SetDirty(bl_GameData.Instance);
                editWeaponId = -1;
                editGunInfo = null;
            }
            GUILayout.Space(18);
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawLoadoutsBody()
    {
        GUILayout.Space(10);
#if !CLASS_CUSTOMIZER
        var player = bl_GameData.Instance.Player1.PlayerReferences;
        DrawPlayerLoadout(player, ref currentSlot);
        GUILayout.Space(15);
        player = bl_GameData.Instance.Player2.PlayerReferences;
        DrawPlayerLoadout(player, ref currentSlot2);
#else
        GUILayout.Label("<b>已启用兵种自定义</b>\n<b><size=8><i>玩家武器配置由全局配置统一管理，可在游戏内修改，不再像 GFWK 默认那样按玩家预制体固定配置</i></size></b>\n", styles["textC"]);
        GUILayout.Space(10);
        Rect r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        {
            EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
            //Slot buttons
            using (new GUILayout.HorizontalScope(GUILayout.Height(30)))
            {
                GUILayout.Space(5);
                for (int i = 0; i < slotsNames.Length; i++)
                {
                    Rect br = GUILayoutUtility.GetRect(new GUIContent(slotsNames[i]), styles["textC"], GUILayout.Height(30));
                    if (i != currentSlot)
                        EditorGUI.DrawRect(br, primaryColor);
                    else
                    {
                        EditorGUI.DrawRect(br, altColor);
                        GUI.color = new Color(1, 1, 1, 0.2f);
                        GUI.Box(br, GUIContent.none, GFWKEditorStyles.OutlineButtonStyle);
                        GUI.color = Color.white;
                    }
                    if (GUI.Button(br, slotsNames[i], styles["textC"]))
                    {
                        if (currentSlot != i)
                        {
                            selectedLoadout = null;
                            selectedSlot = -1;
                        }
                        currentSlot = i;
                    }
                }
                GUILayout.Space(5);
            }
            GUILayout.Space(20);

            var loadout = bl_ClassManager.Instance.DefaultAssaultClass;
            if (currentSlot == 1) loadout = bl_ClassManager.Instance.DefaultReconClass;
            if (currentSlot == 2) loadout = bl_ClassManager.Instance.DefaultSupportClass;
            if (currentSlot == 3) loadout = bl_ClassManager.Instance.DefaultEngineerClass;
            //loadout
            DrawSlots(loadout);

        }
        EditorGUILayout.EndVertical();
#endif
        GUILayout.Space(10);
        var latpc = currentPlayerClass;
        currentPlayerClass = (PlayerClass)EditorGUILayout.EnumPopup("当前玩家兵种", currentPlayerClass);
        if (currentPlayerClass != latpc)
        {
            currentPlayerClass.SavePlayerClass();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGameModes()
    {
        GUILayout.Space(10);

        if (selectedGameMode != null)
        {
            DrawGameModeEdit();
            return;
        }

        float areaWidth = position.width - 160;
        float rowWidth = 160;
        int horizontalRows = Mathf.FloorToInt(areaWidth / rowWidth);

        var gameModes = bl_GameData.Instance.gameModes;
        int hRow = 0;
        for (int i = 0; i < gameModes.Count; i++)
        {
            if (hRow == 0) EditorGUILayout.BeginHorizontal();

            DrawGameModeBox(gameModes[i]);
            GUILayout.Space(10);
            hRow++;
            if (hRow >= horizontalRows || i == gameModes.Count - 1)
            {
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
                hRow = 0;
            }

        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGameModeBox(GameModeSettings gameMode)
    {
        var r = EditorGUILayout.BeginVertical(GUILayout.Width(150), GUILayout.Height(200));
        EditorGUI.DrawRect(r, primaryColor);

        var sr = r;
        GUILayout.Space(5);
        GUILayout.Label($"<size=12>{gameMode.ModeName.ToUpper()}</size>\n<color=#606060FF><size=9>({gameMode.gameMode.ToString()})</size></color>", styles["titleH2"]);

        GUILayout.FlexibleSpace();
        string teamMode = isOneTeamMode(gameMode.gameMode) ? "单人对抗全体" : "阵营对抗";
        DrawText($"<b><size=10>阵营模式</size></b>\n<color=#A4A4A4FF><size=9>{teamMode}</size></color>");

        int maxPlayers = 1;
        foreach (var item in gameMode.maxPlayers)
        {
            maxPlayers = Mathf.Max(maxPlayers, item);
        }
        DrawText($"<b><size=10>所需玩家数</size></b>\n<color=#A4A4A4FF><size=9>{gameMode.RequiredPlayersToStart}，最多 {maxPlayers}</size></color>");
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();

        sr.width -= 20;
        if (GUI.Button(sr, GUIContent.none, GUIStyle.none))
        {
            selectedGameMode = gameMode;
        }

        // active icon
        sr = r;
        sr.x = sr.x + sr.width - 16;
        sr.y += 4;
        sr.width = sr.height = 12;

        GUI.color = gameMode.isEnabled ? Color.green : Color.grey;
        GUI.DrawTexture(sr, GetUnityIcon("ColorPicker-HueRing-Thumb-Fill"), ScaleMode.ScaleToFit);
        GUI.color = Color.white;

        if (GUI.Button(sr, GUIContent.none, GUIStyle.none))
        {
            gameMode.isEnabled = !gameMode.isEnabled;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGameModeEdit()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUI.BeginChangeCheck();

        var m = selectedGameMode;
        GUILayout.Space(10);
        DrawTitleText("游戏模式信息");
        m.ModeName = EditorGUILayout.TextField("游戏模式名称", m.ModeName);
        m.gameMode = (GameMode)EditorGUILayout.EnumPopup("模式标识", m.gameMode);
        var r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.isEnabled = GFWKEditorStyles.FeatureToogle(r, m.isEnabled, "是否启用");

        GUILayout.Space(10);
        DrawTitleText("SETTINGS");
        m.GoalName = EditorGUILayout.TextField("模式目标名称", m.GoalName);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.supportBots = GFWKEditorStyles.FeatureToogle(r, m.supportBots, "支持机器人？");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.AutoTeamSelection = GFWKEditorStyles.FeatureToogle(r, m.AutoTeamSelection, "强制自动分配阵营？");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.allowedPickupWeapons = GFWKEditorStyles.FeatureToogle(r, m.allowedPickupWeapons, "允许拾取武器？");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.UnlistGameAfterStarted = GFWKEditorStyles.FeatureToogle(r, m.UnlistGameAfterStarted, "开始后隐藏或关闭房间？");

        m.RequiredPlayersToStart = EditorGUILayout.IntSlider("开始所需玩家数", m.RequiredPlayersToStart, 1, 64);
        m.onRoundStartedSpawn = (GameModeSettings.OnRoundStartedSpawn)EditorGUILayout.EnumPopup("回合开始时", m.onRoundStartedSpawn);
        m.onPlayerDie = (GameModeSettings.OnPlayerDie)EditorGUILayout.EnumPopup("本地玩家死亡时", m.onPlayerDie);
        m.RoundModeAllowed = (GameModeSettings.RoundModeAllowedOptions)EditorGUILayout.EnumPopup("允许的回合模式", m.RoundModeAllowed);

        GUILayout.Space(10);
        DrawTitleText("模式选项");

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedGameMode").FindPropertyRelative("maxPlayers"), true);
            r = GUILayoutUtility.GetLastRect();
            r.x += 150;
            r.y += 27;
            r.width -= 150;
            r.height = EditorGUIUtility.singleLineHeight;
            string options = "";
            foreach (var item in m.maxPlayers)
            {
                if (isOneTeamMode(m.gameMode)) options += $"({item}) ";
                else options += $"({item / 2} 对 {item / 2}) ";
            }
            EditorGUI.HelpBox(r, $"当前选项：{options}", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedGameMode").FindPropertyRelative("GameGoalsOptions"), true);
            r = GUILayoutUtility.GetLastRect();
            r.x += 150;
            r.width -= 150;
            r.height = EditorGUIUtility.singleLineHeight;
            string options = "";
            foreach (var item in m.GameGoalsOptions)
            {
                options += $"({item} {m.GoalName}) ";
            }
            EditorGUI.HelpBox(r, $"当前选项：{options}", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedGameMode").FindPropertyRelative("timeLimits"), true);
            r = GUILayoutUtility.GetLastRect();
            r.x += 150;
            r.width -= 150;
            r.height = EditorGUIUtility.singleLineHeight;
            string options = "";
            foreach (var item in m.timeLimits)
            {
                if (item < 60) options += $"({item} 秒) ";
                else options += $"({Mathf.FloorToInt(item / 60)} 分钟) ";
            }
            EditorGUI.HelpBox(r, $"当前选项：{options}", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(18);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("返回", GFWKEditorStyles.EditorSkin.customStyles[11], GUILayout.Width(100)))
            {
                selectedGameMode = null;
            }
            GUILayout.Space(18);
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(50);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(bl_GameData.Instance);
        }
    }

    public bool isOneTeamMode(GameMode mode)
    {
        if (mode == GameMode.BR || mode == GameMode.GR || mode == GameMode.FFA)
            return true;

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="slotID"></param>
    void DrawPlayerLoadout(bl_PlayerReferences player, ref int slotID)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(256));
        {
            GUILayout.Space(10);
            //soldier icon
            Rect r = EditorGUILayout.BeginVertical(styles["borders"], GUILayout.Width(200));
            {
                EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(soldierIcon, GUILayout.Height(200), GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Label(player.name, styles["textC"]);
            }
            EditorGUILayout.EndVertical();

            r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
                //Slot buttons
                using (new GUILayout.HorizontalScope(GUILayout.Height(30)))
                {
                    GUILayout.Space(5);
                    for (int i = 0; i < slotsNames.Length; i++)
                    {
                        Rect br = GUILayoutUtility.GetRect(new GUIContent(slotsNames[i]), styles["textC"], GUILayout.Height(30));
                        if (i != slotID)
                            EditorGUI.DrawRect(br, primaryColor);
                        else
                        {
                            EditorGUI.DrawRect(br, altColor);
                            GUI.color = new Color(1, 1, 1, 0.2f);
                            GUI.Box(br, GUIContent.none, GFWKEditorStyles.OutlineButtonStyle);
                            GUI.color = Color.white;
                        }
                        if (GUI.Button(br, slotsNames[i], styles["textC"]))
                        {
                            if (slotID != i)
                            {
                                selectedLoadout = null;
                                selectedSlot = -1;
                            }
                            slotID = i;
                        }
                    }
                    GUILayout.Space(5);
                }
                GUILayout.Space(20);

                var loadout = player.gunManager.m_AssaultClass;
                if (slotID == 1) loadout = player.gunManager.m_ReconClass;
                if (slotID == 2) loadout = player.gunManager.m_SupportClass;
                if (slotID == 3) loadout = player.gunManager.m_EngineerClass;
                //loadout
                DrawSlots(loadout);

            }
            EditorGUILayout.EndVertical();

        }
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
    }

    void DrawSlots(bl_PlayerClassLoadout loadout)
    {
        if (loadout == null) return;

        if (selectedLoadout != null && selectedLoadout == loadout)
        {
            DrawWeaponSelectionList();
            return;
        }

        var gun = loadout.GetPrimaryGunInfo();
        DrawSlot(gun, "PRIMARY", loadout, 0);
        GUILayout.Space(10);
        gun = loadout.GetSecondaryGunInfo();
        DrawSlot(gun, "SECONDARY", loadout, 1);
        GUILayout.Space(10);
        gun = loadout.GetPerksGunInfo();
        DrawSlot(gun, "PERK", loadout, 2);
        GUILayout.Space(10);
        gun = loadout.GetLetalGunInfo();
        DrawSlot(gun, "LETAL", loadout, 3);
        GUILayout.Space(10);
    }

    void DrawSlot(bl_GunInfo gun, string Title, bl_PlayerClassLoadout loadout, int slotID)
    {
        if (gun == null) return;

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(25);
            GUILayout.Label($"<b>{Title}:</b>", styles["textC"], GUILayout.Width(100));
            GUILayout.Label(gun.Name, styles["textC"], GUILayout.Width(150));
            if (gun.GunIcon != null)
                GUILayout.Label(gun.GunIcon.texture, GUILayout.Height(22), GUILayout.Width(100));

            Rect br = GUILayoutUtility.GetRect(new GUIContent("更改"), styles["textC"], GUILayout.Width(100));
            EditorGUI.DrawRect(br, altColor);
            if (GUI.Button(br, "更改", styles["textC"]))
            {
                selectedSlot = slotID;
                selectedLoadout = loadout;
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawWeaponSelectionList()
    {
        var all = bl_GFWK.AllWeapons;
        weaponScroll = GUILayout.BeginScrollView(weaponScroll);
        GUILayout.Space(10);
        Color c = new Color(0, 0, 0, 0.1f);
        for (int i = 0; i < all.Count; i++)
        {
            Rect r = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(r, c);
            GUILayout.Space(20);
            GUILayout.Label(all[i].Name, styles["textC"], GUILayout.Width(120), GUILayout.Height(20));
            GUILayout.Space(25);
            if (all[i].GunIcon != null)
                GUILayout.Label(all[i].GunIcon.texture, GUILayout.Height(22), GUILayout.Width(100));

            GUILayout.Space(25);
            Rect br = GUILayoutUtility.GetRect(new GUIContent("选择"), styles["textC"], GUILayout.Width(100));
            EditorGUI.DrawRect(br, altColor);
            if (GUI.Button(br, "选择", styles["textC"]))
            {
                if (selectedSlot == 0) selectedLoadout.Primary = i;
                else if (selectedSlot == 1) selectedLoadout.Secondary = i;
                else if (selectedSlot == 2) selectedLoadout.Perks = i;
                else if (selectedSlot == 3) selectedLoadout.Letal = i;

                EditorUtility.SetDirty(selectedLoadout);
                AssetDatabase.SaveAssets();

                selectedSlot = -1;
                selectedLoadout = null;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        GUILayout.EndScrollView();
    }

    void WeaponCard(bl_GunInfo gun, int gunId)
    {
        Rect rect = EditorGUILayout.BeginVertical(styles["borders"], GUILayout.Width(370), GUILayout.Height(90));
        {
            EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.02f));

            EditorGUILayout.BeginHorizontal();
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(5);
                    GUILayout.Label($"<size=14>{gun.Name}</size>", styles["titleH2"], GUILayout.Height(18));
                    GUILayout.Label($"<size=9><color=#727272>{gun.Type.ToString()}</color></size>", styles["titleH2"], GUILayout.Height(18));
                    GUILayout.FlexibleSpace();
                    Rect iconR = GUILayoutUtility.GetRect(120, 70);
                    GUI.DrawTexture(iconR, gun.GunIcon.texture, ScaleMode.ScaleToFit);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            {
                gun.Damage = EditorGUILayout.IntSlider("DAMAGE", gun.Damage, 1, 100);
                gun.FireRate = EditorGUILayout.Slider("射速", gun.FireRate, 0.01f, 2f);
                gun.Accuracy = EditorGUILayout.IntSlider("ACCURACY", gun.Accuracy, 1, 5);
                gun.ReloadTime = EditorGUILayout.Slider("RELOAD", gun.ReloadTime, 0.5f, 10);
                gun.Range = EditorGUILayout.IntSlider("RANGE", gun.Range, 1, 700);
                gun.Weight = EditorGUILayout.Slider("WEIGHT", gun.Weight, 0, 4);
                gun.Unlockability.Price = EditorGUILayout.IntField("价格", gun.Unlockability.Price);
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        var editR = rect;
        editR.width = 40;
        editR.x += 4;
        editR.y += editR.height - 18;
        editR.height = 14;
        if (GUI.Button(editR, "编辑"))
        {
            editWeaponId = gunId;
            editGunInfo = gun;
        }
    }

    #region Manager Windows
    void DrawGameDataSettings()
    {
        if (gameData == null) gameData = bl_GameData.Instance;

        DrawTitleText("游戏数据");
        if (gameData == null) return;
        DrawEditorOf(gameData);
    }

    void DrawPlayerSelector()
    {
        DrawTitleText("玩家选择器");
#if PSELECTOR
        DrawEditorOf(bl_PlayerSelector.Data);
#endif
    }

    void DrawCustomizer()
    {
        DrawTitleText("Customizer");
#if CUSTOMIZER
        DrawEditorOf(bl_CustomizerData.Instance);
#endif
    }

    void DrawClassCustomizer()
    {
        DrawTitleText("兵种自定义");
#if CLASS_CUSTOMIZER
        DrawEditorOf(bl_ClassManager.Instance);
#endif
    }

    void DrawlevelManager()
    {
        DrawTitleText("关卡管理器");
#if LM
        DrawEditorOf(bl_LevelManager.Instance);
#endif
    }

    void DrawInputManager()
    {
        DrawTitleText("输入管理器");

        DrawEditorOf(bl_Input.InputData);
        GUILayout.Space(10);
        if (bl_Input.InputData.DefaultMapped != null)
        {
            DrawTitleText($"映射 {bl_Input.InputData.DefaultMapped.name}");
            DrawEditorOf(bl_Input.InputData.DefaultMapped);
        }
    }
    #endregion

    private bool lpAlt = false;
    void DrawManagerButtons()
    {
        GUILayout.Space(2);
        managerSearchKey = managerSearchField.OnToolbarGUI(managerSearchKey);
        Rect r;
        managersScroll = GUILayout.BeginScrollView(managersScroll);
        for (int i = 0; i < managerPanels.Count; i++)
        {
            if (!string.IsNullOrEmpty(managerSearchKey))
            {
                if (!managerPanels[i].Name.ToLower().Contains(managerSearchKey.ToLower())) continue;
            }

            lpAlt = !lpAlt;
            r = GUILayoutUtility.GetRect(new GUIContent(managerPanels[i].Name), styles["panelButton"], GUILayout.Height(26));
            EditorGUI.DrawRect(r, lpAlt ? GetHexColor("#1212122F") : GetHexColor("#1212120F"));
            if (currentPanelType == managerPanels[i].Name)
            {
                EditorGUI.DrawRect(r, altColor);
                float w = r.width;
                r.width = 1;
                EditorGUI.DrawRect(r, hightlightColor);
                r.width = w;
            }
            if (GUI.Button(r, managerPanels[i].Name, styles["panelButton"]))
            {
                bodyScroll = Vector2.zero;
                currentPanelType = managerPanels[i].Name;
                currentPanel = managerPanels[i];
                currentWindow = WindowType.Managers;
            }
        }
        //  DrawBottomLine(0.2f);
        GUILayout.Space(30);
        EditorGUILayout.EndScrollView();
    }

    void DrawWeaponPanel()
    {
        GUILayout.Space(20);
        weaponSearchKey = weaponSearchfield.OnToolbarGUI(weaponSearchKey);
    }

    /// <summary>
    /// 
    /// </summary>
    void InitGUI()
    {
        m_initGUI = true;

        styles["panelButton"] = GetStyle("label", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.fontStyle = FontStyle.Bold;
        });
        styles["textC"] = GetStyle("textmwc", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.richText = true;
            style.fontSize = 10;
        }, "label");
        styles["titleH2"] = GetStyle("titleH2", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 18;
        }, "label");
        styles["borders"] = GetStyle("grey_border", (ref GUIStyle style) =>
        {
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.overflow.left = style.overflow.right = style.overflow.top = style.overflow.bottom = 5;
            style.padding.left = -5; style.padding.right = 5;
            style.margin.left = style.margin.right = 10;
        });
        styles["miniText"] = GetStyle(EditorStyles.miniLabel.name, (ref GUIStyle style) =>
        {
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.richText = true;
        });
        styles["text"] = GetStyle("label", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.richText = true;
        });
    }

    #region Drawers
    public void DrawTitleText(string title)
    {
        GUILayout.Label(title.ToUpper(), styles["titleH2"]);
        DrawBottomLine();
        GUILayout.Space(20);
    }

    public void DrawText(string text, params GUILayoutOption[] options)
    {
        GUILayout.Label(text, styles["text"], options);
    }

    void DrawBottomLine(float alpha = 0.7f) => DrawBottomLine(GUILayoutUtility.GetLastRect(), alpha);

    void DrawBottomLine(Rect fullRect, float alpha = 0.7f)
    {
        var color = whiteColor;
        color.a = alpha;
        fullRect.y += fullRect.height;
        fullRect.height = 1;
        EditorGUI.DrawRect(fullRect, color);
    }

    public void DrawEditorOf(ScriptableObject scriptable)
    {
        if (scriptable == null) return;

        Editor editor;
        if (cachedEditors.ContainsKey(scriptable))
        {
            editor = cachedEditors[scriptable];
        }
        else
        {
            editor = Editor.CreateEditor(scriptable);
            cachedEditors.Add(scriptable, editor);
        }

        if (editor == null) return;

        Rect r = EditorGUILayout.BeginVertical(styles["borders"]);
        EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
        editor.OnInspectorGUI();
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region Utils
    private GUIStyle GetStyle(string name, TutorialWizard.Style.OnCreateStyleOp onCreate, string overlap = "") => TutorialWizard.Style.GetUnityStyle(name, onCreate, overlap);

    private static Dictionary<string, Texture2D> cachedUnityIcons;
    public static Texture2D GetUnityIcon(string iconName)
    {
        if (cachedUnityIcons == null) cachedUnityIcons = new Dictionary<string, Texture2D>();
        if (!cachedUnityIcons.ContainsKey(iconName))
        {
            var icon = (Texture2D)EditorGUIUtility.IconContent(iconName).image;
            cachedUnityIcons.Add(iconName, icon);
        }
        return cachedUnityIcons[iconName];
    }

    public static Color GetHexColor(string hex) => GFWKEditorStyles.GetColorFromHex(hex);

    private static Dictionary<string, Color> cachedColors;
    public static Color GetCachedColor(string key, Color color)
    {
        if (cachedColors == null) cachedColors = new Dictionary<string, Color>();
        if (!cachedColors.ContainsKey(key))
        {
            cachedColors.Add(key, color);
        }
        return cachedColors[key];
    }

    [MenuItem("游戏框架/管理器 %m")]
    static void Open()
    {
        GetWindow<bl_GFWKManagerWindow>("GFWK 管理器");
    }

    void FetchCustomTabs()
    {
        var classes = TypeCache.GetTypesWithAttribute<GFWKManagerTab>();
        foreach (var classInfo in classes)
        {
            // get the attribute information
            var attr = classInfo.GetCustomAttribute<GFWKManagerTab>();

            var panel = managerPanels.Find(x => x.Name == attr.name);
            if (panel != null)
            {
                if (panel.bodyFunc == null)
                {

                }
                else continue;
            }

            // Cast the GFWKManagerTabData type from the classInfo
            var tab = (GFWKManagerTabData)Activator.CreateInstance(classInfo);
            var data = tab.GetData();

            if (panel == null)
            {
                managerPanels.Add(new ManagerPanel()
                {
                    Name = attr.name,
                    bodyFunc = data.BodyDrawFunc,
                    isExternalDrawer = true,
                });
            }
            else
            {
                panel.bodyFunc = data.BodyDrawFunc;
            }
        }

    }

    [Serializable]
    public class ManagerPanel
    {
        public string Name;
        public string BodyFuncName;
        public Type CustomType;

        internal MethodInfo bodyFunc;
        internal bool isExternalDrawer = false;

        public void DrawBody(Type type, bl_GFWKManagerWindow target)
        {
            if (string.IsNullOrEmpty(BodyFuncName) && bodyFunc == null) { return; }

            if (bodyFunc == null)
            {
                bodyFunc = type.GetMethod(BodyFuncName, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (isExternalDrawer)
            {
                bodyFunc?.Invoke(null, new object[] { target });
            }
            else
            {
                bodyFunc?.Invoke(target, null);
            }
        }
    }

    [Serializable]
    public enum WindowType
    {
        Home,
        Managers,
        Weapons,
        Players,
        Loadouts,
        Levels,
        GameModes,
    }
    #endregion
}

/// <summary>
/// 
/// </summary>
public abstract class GFWKManagerTabData
{
    public struct Data
    {
        public string Title;
        public string BodyFuncName;
        public MethodInfo BodyDrawFunc;
    }

    public abstract Data GetData();

    public virtual MethodInfo GetMethodInfoOf(string staticMethodName)
    {
        return this.GetType().GetMethod(staticMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }
}

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class GFWKManagerTab : Attribute
{
    public string name;

    public GFWKManagerTab(string name)
    {
        this.name = name;
    }
}