using MFPS.Internal.Structures;
using MFPSEditor;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class AddMapTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/map/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "开始使用", StepsLenght = 0 },
    new Steps { Name = "设置场景", StepsLenght = 6 },
    new Steps { Name = "提示", StepsLenght = 0 },
    new Steps { Name = "地图资源", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////
    private Object m_SceneReference;
    string[] RequiredsPaths = new string[]
    {
        "Assets/Prefabs/Network/Managers/GameManager.prefab",
        "Assets/Prefabs/Network/Managers/AIManager.prefab",
        "Assets/Prefabs/Network/Managers/ItemManager.prefab",
        "Assets/Prefabs/GamePlay/GameModes/GameModes.prefab",
        "Assets/Prefabs/UI/UI.prefab",
    };
    bool[] RequiredInstanced = new bool[] { false, false, false, false, false, };
    string sceneName = "";
    Sprite scenePreview = null;
    AssetStoreAffiliate pcMapsAssets;
    AssetStoreAffiliate mobileMapsAssets;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/GameFrameworkEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        allowTextSuggestions = true;
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawStarted();
        }
        else if (window == 1) { DrawSetup(); }
        else if (window == 2) { DrawTips(); }
        else if (window == 3) { MapAssetsDoc(); }
    }

    void DrawStarted()
    {
        DrawNote("本教程将逐步讲解如何为 MFPS 游戏添加新地图。");
        DownArrow();
        DrawText("首先，你当然需要一张地图。这里的地图指的是关卡环境设计，包含所有美术内容，模型、预制体、灯光、天空等，摆放成战场的样子。");
        DrawSuperText("地图有一些基本要求，是所有 Unity 用户都应了解的，适用于所有游戏而不仅是 MFPS：\n\n-<b>地图模型网格必须带碰撞体</b>，仅作装饰的模型除外。场景中玩家不该穿过的所有模型网格都必须有碰撞体。\n\n-<b>灯光</b>，灯光是地图的重要组成部分，但对游戏性能影响很大。网上有大量关于搭建场景灯光和烘焙光照贴图的教程，例如：\n<?link=https://learn.unity.com/tutorial/introduction-to-lighting-and-rendering#>https://learn.unity.com/tutorial/introduction-to-lighting-and-rendering#</link>\n\n-<b>性能优化优先于画面效果</b>。人人都喜欢游戏有好的画质，但优化不佳的关卡会毁掉你的游戏。MFPS 在代码层面已做了不错的优化，但比代码更重要的是图形优化。Unity 官方有一篇不错的图形优化文章，值得一看：\n<?link=https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html>https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html</link>");
        DownArrow();
        DrawText("好，如果你的地图关卡设计已就绪，我们继续。");
    }

    void DrawSetup()
    {
        if (subStep == 0)
        {
            DrawText("如前所述，你需要准备好新地图设计，但不能只做成预制体，而要<b>放置在一个只包含地图环境的 Unity 场景中</b>。如果没有，请在编辑器顶部菜单 ➔ <b>File ➔ New Scene</b> 新建场景，然后在打开的空场景中放入地图环境或直接在其中设计。\n                 \n准备好后，在 Unity 工程中保存该场景<b>（File ➔ Save）</b>，然后继续下一步。");
            DrawNote("<b>在接入 MFPS 之前，请务必删除地图场景中的所有相机</b>。这里不需要它们，MFPS 会创建所需的相机。");
            DrawImage(GetServerImage(0));
            DownArrow();
            DrawText("在下方字段中指定你的 Unity 地图场景<i>（.scene）</i>，<b>然后点击继续</b>按钮进行场景校验。");
            Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("地图场景： ", GUILayout.Width(100));
            m_SceneReference = EditorGUILayout.ObjectField(m_SceneReference, typeof(SceneAsset), false) as SceneAsset;
            GUILayout.EndHorizontal();
            GUI.enabled = m_SceneReference != null;
            Space(5);
            if (GUILayout.Button("CONTINUE", MFPSEditorStyles.EditorSkin.customStyles[11]))
            {
                if (EditorSceneManager.GetActiveScene().name == m_SceneReference.name)
                {
                    NextStep();
                }
                else
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    string path = AssetDatabase.GetAssetPath(m_SceneReference);
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    NextStep();
                }
            }
            GUI.enabled = true;
        }
        else if (subStep == 1)
        {
            HideNextButton = isMissing();
            DrawText("地图场景已打开，现在拖入 MFPS 所需的对象。先检查场景中已有的资源，点击下方按钮自动检测。");
            Space();
            if (DrawButton("检查场景"))
            {
                CheckScene();
            }
            if (sceneChecked)
            {
                EditorStyles.helpBox.richText = true;
                GUILayout.BeginVertical("box");
                GUILayout.Label(string.Format("游戏管理器： {0}", RequiredInstanced[0] ? "<color=green>是</color>" : "<color=red>否</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("AI 管理器： {0}", RequiredInstanced[1] ? "<color=green>是</color>" : "<color=red>否</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("物品管理器： {0}", RequiredInstanced[2] ? "<color=green>是</color>" : "<color=red>否</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("游戏模式对象： {0}", RequiredInstanced[3] ? "<color=green>是</color>" : "<color=red>否</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("UI: {0}", RequiredInstanced[4] ? "<color=green>是</color>" : "<color=red>否</color>"), EditorStyles.helpBox);
                GUILayout.EndVertical();

                if (isMissing())
                {
                    DrawText("场景尚未正确配置，点击下方按钮自动添加所需组件。");
                    Space();
                    if (DrawButton("设置场景"))
                    {
                        for (int i = 0; i < RequiredsPaths.Length; i++)
                        {
                            Debug.Log(RequiredsPaths[i]);
                            if (RequiredInstanced[i] || RequiredsPaths[i] == string.Empty) continue;
                            GameObject prefab = AssetDatabase.LoadAssetAtPath(RequiredsPaths[i], typeof(GameObject)) as GameObject;
                            if (prefab != null)
                            {
                                PrefabUtility.InstantiatePrefab(prefab, EditorSceneManager.GetActiveScene());
                            }
                            else
                            {
                                Debug.LogWarning("未找到预制体，路径： " + RequiredsPaths[i]);
                            }
                        }
                        CheckScene();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Repaint();
                    }
                }
                else
                {
                    DrawText("一切正常，场景已包含全部所需对象，继续下一步。");
                }
            }
        }
        else if (subStep == 2)
        {
            DrawText("实例化 MFPS 对象后，你会在 Game 视图中看到一个从顶部渲染的相机，这是玩家进入房间选择阵营时显示俯视角的相机。请将它摆放到能俯瞰地图全貌的位置。\n \n该相机位于<i>（层级面板）<b>GameManager ➔ Room Camera</b></i>。");
            DrawImage(GetServerImage(1));
            DownArrow();
            DrawText("地图中有一些对象需要重新摆位，其中两个是 CTF 模式的两面旗帜，位于层级面板中的 <b>GameModes -> CaptureOfFlag</b> 对象下，" +
                " 选中它们并摆放到你想要的位置。");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("对 <b>ItemManager</b> 下的对象做同样处理，将它们分布到地图各处。这一步可选，这些物品（医疗包和弹药）可作为地图中的常驻补给品。" +
                "这样玩家就能在游戏中使用它们。若不需要，直接从场景中删除即可。");
            DrawImage(GetServerImage(3));
        }
        else if (subStep == 3)
        {
            DrawText("现在需要为每个阵营创建<b>出生点</b><i>（阵营 1、阵营 2 以及混战模式）</i>。出生点并不是固定位置，而是一个<b>球形区域</b>，玩家会在该区域半径内的随机位置出生。\n \n<b><size=16>如何创建出生点：</size></b>\n \n在场景中新建一个空物体，挂载 <b>bl_SpawnPoint.cs</b> 脚本，并指定区域和阵营。\n \n为方便操作，下方提供了一个创建出生点的按钮，选择阵营后点击 <b>创建出生点</b> 按钮 ➔ 即可创建出生点，然后在场景视图中选中它并摆放位置。");
            Space();
            GUILayout.BeginVertical();
            DrawText("<color=yellow>创建出生点</color>");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("出生点所属： ");
            SpawnTeam = (Team)EditorGUILayout.EnumPopup(SpawnTeam);
            if (GUILayout.Button("创建出生点", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                CreateSpawnPoint();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            DownArrow();
            DrawText("创建并摆放好出生点后，效果大致如下：");
            DrawImage(GetServerImage(4));
            DrawText("这就是出生点区域。半球体与居中辅助线框表示的玩家实际尺寸可用来预览玩家的可出生范围。\n \n 可在 bl_Spawnpoint 中增大或减小该区域。" +
                "脚本挂到该对象上 -> 在 <b>Spawn Space</b> 中设置。你还可以旋转它来设定默认朝向，即出生玩家面朝的方向。\n \n注意让玩家辅助图标的脚部与球形区域位于地面之上，" +
                " 否则玩家出生后会掉落。");
            DrawText("\n用上方按钮可创建任意数量的出生点。确保每个阵营至少有一个出生点，完成后继续下一步。");
        }
        else if (subStep == 4)
        {
            DrawText("要让 AI 机器人在这张地图上工作，需要设置并烘焙 <b>导航网格表面</b>。简单来说，导航网格表面就是允许 AI 智能体移动的区域，Unity 烘焙时会根据地图几何结构自动计算该区域，但你需要设置参与烘焙的网格。关于 Unity 导航网格的深入说明和手动设置方法，请查阅官方文档：");
            if (DrawLinkText("创建 Navmesh 文档"))
            {
                Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1/manual/CreateNavMesh.html");
            }
            DownArrow();
            DrawText("若要自动设置导航网格，点击下方按钮即可。注意该操作会基于地图中所有碰撞体生成导航网格，之后可在 <b>AI Navmesh</b> 对象中修改。");
            Space(5);
            if (DrawButton("自动设置导航网格"))
            {
                SetupNavmesh();
            }

            DrawText("烘焙导航网格后，效果大致如下：");
            DrawImage(GetServerImage(5));
            DrawText("机器人可自由移动的区域由蓝色叠加网格标识。要让机器人发挥最佳效果，必须策略性地摆放 <b>AI 掩体点</b>。掩体点是挂载 <b>bl_AICoverPoint</b> 脚本的空物体，作为机器人战斗时寻找掩体的参考位置。\n \n为简化流程，AIManager 对象已内置一组默认 AI 掩体点。在 Unity 编辑器的层级面板中展开 <b>AIManager ➔ *</b> 即可找到它们，可随意微调位置或增加掩体点，以提升机器人寻找掩体时的战术判断能力。");
            DrawImage(GetServerImage(6), TextAlignment.Center);
            DrawText("数量可自行决定，不需要那么多就删掉一些，需要更多就复制。\n \n逐个选中并摆放到地图上的战术位置；在 <i>AIManager ➔ bl_AIManager ➔ <b>显示辅助线</b></i> 中开启后可预览区域范围。");
            DrawImage(GetServerImage(7));
            DrawText("<i>关于 AI 掩体点的更多信息请查看：</i>");
            if (Buttons.OutlineButton("AI 掩体点"))
            {
                var tut = GetWindow<TutorialBots>();
                tut.windowID = 1;
            }
        }
        else if (subStep == 5)
        {
            DrawText("场景已设置完成！\n \n接下来只需将其登记到可用场景列表中，玩家创建房间时就能选择该场景。可以手动在 GameData 的 <b>AllScenes</b> 列表中新增一项：<b><i>（MFPS 的 Resources 文件夹）GameData ➔ AllScenes ➔ 新增一项</i></b>，并填写所需信息\n\n或者<b>在此处自动完成</b>，只需在下方为地图设置名称和预览图：");
            DrawImage(GetServerImage(8));
            DownArrow();
            GUILayout.BeginVertical("box");
            sceneName = EditorGUILayout.TextField("地图自定义名称", sceneName);
            scenePreview = EditorGUILayout.ObjectField("地图预览", scenePreview, typeof(Sprite), false) as Sprite;
            GUI.enabled = m_SceneReference == null;
            m_SceneReference = EditorGUILayout.ObjectField("Scene", m_SceneReference, typeof(SceneAsset), false) as SceneAsset;
            GUI.enabled = !string.IsNullOrEmpty(sceneName) && m_SceneReference != null;
            if (DrawButton("列出地图"))
            {
                if (!bl_GameData.Instance.AllScenes.Exists(x => x.ShowName == sceneName))
                {
                    var si = new MapInfo();
                    si.ShowName = sceneName;
                    si.m_Scene = m_SceneReference;
                    si.Preview = scenePreview;
                    bl_GameData.Instance.AllScenes.Add(si);
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    var original = EditorBuildSettings.scenes;
                    var newSettings = new EditorBuildSettingsScene[original.Length + 1];
                    System.Array.Copy(original, newSettings, original.Length);
                    string path = AssetDatabase.GetAssetPath(m_SceneReference);
                    var sceneToAdd = new EditorBuildSettingsScene(path, true);
                    newSettings[newSettings.Length - 1] = sceneToAdd;
                    EditorBuildSettings.scenes = newSettings;
                    sceneListed = true;
                }
                else
                {
                    Debug.LogWarning("已存在同名地图。");
                }
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            if (sceneListed)
            {
                DownArrow();
                DrawText("很好，你已成功为游戏添加了一张新地图，现在可在主菜单或大厅的创建房间中选择该地图。\n \n请阅读下一节获取一些提示。");
                DrawImage(GetServerImage(9));
            }
        }
    }

    void DrawTips()
    {
        DrawText("<b><size=16>优化</size></b>");
        DrawText("正如开头所说，若希望游戏在同一地图中超过 8 名玩家时仍保持良好帧率，" +
                 "你需要优化地图的图形内容，包括模型、贴图、着色器等。这在单机游戏中不那么关键，但在多人游戏中，本地客户端既要处理本地信息，" +
            "也要处理从远程玩家收到的信息。因此优化极其重要。\n以下是一些可能有用的 Unity 图形优化参考链接");
        if (DrawLinkText("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html"))
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html");
        }

        if (DrawLinkText("https://unity3d.com/es/learn/tutorials/temas/performance-optimization/optimizing-graphics-rendering-unity-games"))
        {
            Application.OpenURL("https://unity3d.com/es/learn/tutorials/temas/performance-optimization/optimizing-graphics-rendering-unity-games");
        }

        if (DrawLinkText("https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html"))
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html");
        }

        if (DrawLinkText("https://cgcookie.com/articles/maximizing-your-unity-games-performance"))
        {
            Application.OpenURL("https://cgcookie.com/articles/maximizing-your-unity-games-performance");
        }
        Space(20);
        DrawText("<b><size=16>关卡设计</size></b>");
        DrawText("以下资源可用于学习或提升关卡设计能力，来自 3A 项目或经验丰富的设计师：");
        if (DrawLinkText("第一人称关卡设计实用指南"))
        {
            Application.OpenURL("https://medium.com/ironequal/practical-guide-on-first-person-level-design-e187e45c744c");
        }
        if (DrawLinkText("多人地图设计理论 战争机器"))
        {
            Application.OpenURL("https://docs.unrealengine.com/udk/Three/GearsMultiplayerMapTheory.html");
        }
        if (DrawLinkText("关卡设计准则"))
        {
            Application.OpenURL("http://www.mikebarclay.co.uk/my-level-design-guidelines/");
        }
    }

    void MapAssetsDoc()
    {
        DrawText("这里有两份资源商店素材清单，由我挑选，适用于动作射击游戏。若你想为 MFPS 添加更多地图，可作参考。\n \n两份清单分别面向高端平台如 <b>PC 与主机</b> 的最佳素材，以及适合 <b>移动端</b> 的素材，后者可用于移动端游戏的地图。");

        using (new GUILayout.HorizontalScope())
        {
            EditorGUILayout.BeginVertical();
            DrawTitleText("高质量地图");
            if (pcMapsAssets == null)
            {
                pcMapsAssets = new AssetStoreAffiliate();
                pcMapsAssets.randomize = true;
                pcMapsAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673399302530/widget-medium");
                pcMapsAssets.FixedHeight = 360;
            }
            else
                pcMapsAssets.OnGUI();
            GUILayout.Space(20);
            DrawTitleText("移动端友好地图");
            if (mobileMapsAssets == null)
            {
                mobileMapsAssets = new AssetStoreAffiliate();
                mobileMapsAssets.randomize = true;
                mobileMapsAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673399298719/widget-medium");
                mobileMapsAssets.FixedHeight = 360;
            }
            else
                mobileMapsAssets.OnGUI();
            EditorGUILayout.EndVertical();
            GUILayout.Space(60);
        }
    }

    Team SpawnTeam = Team.All;
    bool sceneChecked = false;
    bool sceneListed = false;
    void CheckScene()
    {
        RequiredInstanced[0] = FindObjectOfType<bl_GameManager>() != null;
        RequiredInstanced[1] = FindObjectOfType<bl_AIMananger>() != null;
        RequiredInstanced[2] = FindObjectOfType<bl_ItemManagerBase>() != null;
        RequiredInstanced[3] = GameObject.Find("GameModes") != null;
        RequiredInstanced[4] = FindObjectOfType<bl_UIReferences>() != null;
        sceneChecked = true;
    }

    void CreateSpawnPoint()
    {
        GameObject parent = GameObject.Find("SpawnPoints");
        if (parent == null)
        {
            parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;
        }
        if (SpawnTeam == Team.Team1)
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team1Name));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team1Name));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", bl_GameData.Instance.Team1Name));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.team = SpawnTeam;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
        else if (SpawnTeam == Team.Team2)
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team2Name));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team2Name));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", bl_GameData.Instance.Team2Name));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.team = SpawnTeam;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
        else
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", "ALL"));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", "ALL"));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", "ALL"));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.team = Team.All;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
    }

    void SetupNavmesh()
    {
        GameObject navmeshObject = GameObject.Find("AI Navmesh");
        if (navmeshObject != null)
        {
            Debug.LogWarning("该地图中已存在 Navmesh 对象。若要配置新的，请先禁用或移除现有的。");
            return;
        }

        navmeshObject = new GameObject("AI Navmesh");
        navmeshObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        navmeshObject.transform.localScale = Vector3.one;

        var navSurface = navmeshObject.AddComponent<NavMeshSurface>();
        navSurface.collectObjects = CollectObjects.All;
        navSurface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;

        navSurface.BuildNavMesh();
        EditorUtility.SetDirty(navSurface);
    }

    private bool isMissing()
    {
        for (int i = 0; i < RequiredInstanced.Length; i++)
        {
            if (RequiredInstanced[i] == false) return true;
        }
        return false;
    }

    [MenuItem("游戏框架/教程/添加地图", false, 500)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddMapTutorial));
    }
}