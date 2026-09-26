using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class AddPlayerTutorial : TutorialWizard
{

    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/player/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "dDHltGGDrAA", Image = null, Type = NetworkImages.ImageType.Youtube},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "https://www.lovattostudio.com/en/wp-content/uploads/2017/03/player-selector-product-cover-925x484.png",Type = NetworkImages.ImageType.Custom},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "3D 模型", StepsLenght = 0, DrawFunctionName = nameof(DrawModelInfo) },
    new Steps { Name = "布娃娃系统", StepsLenght = 3, DrawFunctionName = nameof(DrawRagdolled) },
    new Steps { Name = "玩家预制体", StepsLenght = 6, DrawFunctionName = nameof(DrawPlayerPrefab) },
    new Steps { Name = "玩家模型资源", StepsLenght = 1, DrawFunctionName = nameof(PlayerModelAssetsDoc) },
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "addpt3.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
    };
    //final required////////////////////////////////////////////////

    private GameObject PlayerInstantiated;
    private GameObject PlayerModel;
    private Animator PlayerAnimator;
    private Avatar PlayerModelAvatar;
    private string LogLine = "";
    private ModelImporter ModelInfo;
    Editor p1editor;
    AssetStoreAffiliate playerAssets;
    public TPWeaponOrientationMode weaponOrientationMode = TPWeaponOrientationMode.KeepSameLocation;
    public bool autoPoseAiming = true;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/GameFrameworkEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        if (playerAssets == null)
        {
            playerAssets = new AssetStoreAffiliate();
            playerAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/157287/widget-medium");
            playerAssets.FixedHeight = 420;
            playerAssets.randomize = true;
        }
        allowTextSuggestions = true;
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void DrawModelInfo()
    {
        DrawText("本教程将一步步指导你替换玩家预制体中的玩家模型，你需要准备：");
        DrawHorizontalColumn("玩家模型", "使用标准骨骼的人形 <b>Rigged</b> 3D 模型，或任何兼容 Unity 动画重定向系统的骨骼模型。");
        DrawText("要让模型支持动画重定向，模型导入设置的 <b>Rig</b> 必须设为 <b>Humanoid</b>。选中玩家模型<i>（模型本体而非预制体）</i>，在检视面板顶部工具栏切换到 Rig 选项卡，将 <b>Animation Type</b> 设为 Humanoid，设置应如下图所示：");
        DrawServerImage("img-0.png");
        DownArrow();
        DrawNote("<b>重要：</b>模型需具备正确的 <b>T-Pose 骨骼</b> 才能与重定向动画正常配合。若角色模型的骨骼姿势有误，玩家模型上的动画会表现异常，可参考以下视频教程修正骨骼姿势：");
        DrawYoutubeCover("调整 Avatar 以实现正确的动画重定向", GetServerImage(1), "https://www.youtube.com/watch?v=dDHltGGDrAA");
    }

    void DrawRagdolled()
    {
        if (subStep == 0)
        {
            HideNextButton = true;
            DrawText("模型就绪后即可开始配置。\n \n首先要为新玩家模型制作布娃娃。在 Unity 中通常通过 GameObject ➔ 3D Object ➔ Ragdoll 手动创建，并在向导窗口中逐个指定骨骼，而本工具可自动完成，你只需把玩家模型拖到下方。");
            DownArrow();
            DrawText("从 <b>Project 视图</b> 中将玩家模型拖到此处");
            PlayerModel = EditorGUILayout.ObjectField("玩家模型", PlayerModel, typeof(GameObject), false) as GameObject;
            GUI.enabled = PlayerModel != null;
            if (DrawButton("继续"))
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(PlayerModel));
                if (importer != null)
                {
                    ModelInfo = importer as ModelImporter;
                    if (ModelInfo != null)
                    {
                        if (ModelInfo.animationType != ModelImporterAnimationType.Human)
                        {
                            ModelInfo.animationType = ModelImporterAnimationType.Human;
                            EditorUtility.SetDirty(ModelInfo);
                            ModelInfo.SaveAndReimport();
                        }
                        if (ModelInfo.animationType == ModelImporterAnimationType.Human)
                        {
                            PlayerInstantiated = PrefabUtility.InstantiatePrefab(PlayerModel) as GameObject;
                            UnPackPrefab(PlayerInstantiated);
                            PlayerInstantiated.transform.rotation = Quaternion.identity;
                            PlayerAnimator = PlayerInstantiated.GetComponent<Animator>();
                            PlayerModelAvatar = PlayerAnimator.avatar;
                            var view = (SceneView)SceneView.sceneViews[0];
                            view.camera.transform.position = PlayerInstantiated.transform.position + ((PlayerInstantiated.transform.forward * 10) + Vector3.up);
                            view.LookAt(PlayerInstantiated.transform.position);
                            EditorGUIUtility.PingObject(PlayerInstantiated);
                            Selection.activeTransform = PlayerInstantiated.transform;
                            subStep++;
                        }
                        else
                        {
                            LogLine = "你的模型未设置为 <b>Humanoid</b> 骨骼类型，请设置：";
                        }
                    }
                    else
                    {
                        LogLine = "请在 Project 视图中选择模型资源，而不是该模型的预制体。";
                    }
                }
                else { LogLine = "请在 Project 视图中选择模型资源，而不是该模型的预制体。"; }
            }
            GUI.enabled = true;
            if (!string.IsNullOrEmpty(LogLine))
            {
                GUILayout.Label(LogLine);
                if (LogLine.Contains("Humanoid"))
                {
                    DrawImage(GetServerImage(0));
                }
            }
        }
        else if (subStep == 1)
        {
            HideNextButton = false;
            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            PlayerInstantiated = EditorGUILayout.ObjectField("玩家预制体", PlayerInstantiated, typeof(GameObject), false) as GameObject;
            PlayerModelAvatar = EditorGUILayout.ObjectField("Avatar", PlayerModelAvatar, typeof(Avatar), true) as Avatar;
            MeshSizeChecker meshChecker = null;
            if (PlayerInstantiated != null)
            {
                meshChecker = PlayerInstantiated.GetComponent<MeshSizeChecker>();
                if (meshChecker == null) meshChecker = PlayerInstantiated.AddComponent<MeshSizeChecker>();
                meshChecker.Check();

                GUILayout.Label(string.Format("模型高度：<b>{0}</b> | 期望高度：<b>2</b>", meshChecker.Height));
                if (ModelInfo != null) GUILayout.Label(string.Format("模型骨骼类型：{0}", ModelInfo.animationType.ToString()));

                GUI.enabled = true;
                if (meshChecker.Height < 1.9f)
                {
                    GUILayout.Label("<color=yellow>模型尺寸过小</color>，是否尝试自动调整尺寸？", EditorStyles.label);
                    if (DrawButton("是，自动调整尺寸"))
                    {
                        Vector3 v = PlayerInstantiated.transform.localScale;
                        float dif = 2f / meshChecker.Height;
                        v = v * dif;
                        PlayerInstantiated.transform.localScale = v;
                    }
                }
                else if (meshChecker.Height > 2.25f)
                {
                    GUILayout.Label("<color=yellow>模型尺寸过大</color>，是否自动调整尺寸？", EditorStyles.label);
                    if (DrawButton("是，自动调整尺寸"))
                    {
                        Vector3 v = PlayerInstantiated.transform.localScale;
                        float dif = meshChecker.Height / 2;
                        v = v / dif;
                        PlayerInstantiated.transform.localScale = v;
                    }
                }
            }

            GUILayout.EndVertical();
            GUI.enabled = true;
            if (PlayerModelAvatar != null && PlayerAnimator != null)
            {
                DownArrow();
                DrawText("已具备创建布娃娃的条件，点击下方按钮进行构建。");
                if (DrawButton("构建布娃娃"))
                {
                    if (AutoRagdoller.Build(PlayerAnimator))
                    {
                        if (meshChecker != null) DestroyImmediate(meshChecker);
                        else
                        {
                            meshChecker = PlayerInstantiated.GetComponent<MeshSizeChecker>();
                            if (meshChecker != null) DestroyImmediate(meshChecker);
                        }

                        var view = (SceneView)SceneView.sceneViews[0];
                        view.ShowNotification(new GUIContent("布娃娃已创建！"));
                        NextStep();
                    }
                }
            }
            else
            {
                GUILayout.Label("<color=yellow>这里出了点问题，无法获取模型 Avatar。</color>", EditorStyles.label);
            }
        }
        else if (subStep == 2)
        {
            DrawText("此时你的玩家模型<i>（场景中）</i>应大致如下：");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("这些 <b>Box</b> 和 <b>Capsule</b> 碰撞体就是玩家碰撞盒<i>（检测子弹命中玩家的碰撞体）</i>。某些模型上这些碰撞体可能位置或朝向不正确，导致游戏中玩家的部分身体无法被命中。\n\n因此请确认所有碰撞体完整覆盖玩家模型，必要时调整碰撞体参数。\n\n确认无误后即可进入下一步。");

        }
    }

    void DrawPlayerInstanceButton(GameObject player)
    {
        if (player == null) return;

        if (GUILayout.Button(player.name, GUILayout.Width(150)))
        {
            PlayerModel = PlayerInstantiated;
            PlayerInstantiated = PrefabUtility.InstantiatePrefab(player) as GameObject;
            UnPackPrefab(PlayerInstantiated);
            Selection.activeObject = PlayerInstantiated;
            EditorGUIUtility.PingObject(PlayerInstantiated);
            NextStep();
        }
        GUILayout.Space(5);
    }

    void DrawPlayerPrefab()
    {
        if (subStep == 0)
        {
            DrawText("玩家模型已完成布娃娃配置，接下来将其加入玩家预制体，打开一个现有的玩家预制体。\n\n下方列出了所有可用的玩家预制体，点击你想作为参照的那个以替换其模型。");
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                {
                    DrawPlayerInstanceButton(bl_GameData.Instance.Player1.gameObject);
                    DrawPlayerInstanceButton(bl_GameData.Instance.Player2.gameObject);
#if PSELECTOR
                    foreach (var p in bl_PlayerSelector.Data.AllPlayers)
                    {
                        if (p == null || p.Prefab == null) continue;
                        DrawPlayerInstanceButton(p.Prefab);
                    }
#endif
                }
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        else if (subStep == 1)
        {
            GUI.enabled = (PlayerInstantiated == null || PlayerModel == null);
            PlayerInstantiated = EditorGUILayout.ObjectField("玩家预制体", PlayerInstantiated, typeof(GameObject), true) as GameObject;
            if (PlayerModel == null)
            {
                GUILayout.Label("<color=yellow>请在层级中选择已创建布娃娃的玩家模型</color>");
            }
            PlayerModel = EditorGUILayout.ObjectField("玩家模型", PlayerModel, typeof(GameObject), true) as GameObject;
            GUI.enabled = true;
            if (PlayerModel != null && PlayerInstantiated != null)
            {
                DownArrow();
                DrawText("没问题，点击下方按钮将模型设置到玩家预制体中。");
                GUILayout.Space(10);
                var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
                autoPoseAiming = MFPSEditorStyles.FeatureToogle(r, autoPoseAiming, "自动摆出瞄准姿态");
                GUILayout.Space(4);
                weaponOrientationMode = (TPWeaponOrientationMode)EditorGUILayout.EnumPopup("第三人称武器重定位方式", weaponOrientationMode);
                GUILayout.Space(20);

                using (new CenteredScope())
                {
                    if (Buttons.GlowButton("<color=#1e1e1e>设置模型</color>", Style.highlightColor, GUILayout.Height(30), GUILayout.Width(200)))
                    {
                        SetUpModelInPrefab();
                        NextStep();
                    }
                }
            }
        }
        else if (subStep == 2)
        {
            string pin = PlayerInstantiated == null ? "MPlayer" : PlayerInstantiated.name;
            DrawText($"如果一切正常，控制台中应只出现一条日志：<b><i>Player model integrated</i></b>。\n\n若确实如此，在场景层级中实例化出的玩家预制体里，<b>{pin} -> RemotePlayer -></b> 下应同时存在新旧两个模型，旧模型名称末尾带有 <b> DELETE THIS </b> 标记。");
            DrawServerImage("img-3.png");
            DrawNote("旧模型不会自动删除，以防两个模型在位置、缩放或旋转上存在明显差异。若有差异，可以旧模型为参照手动调整新模型的位置、旋转和缩放；若一切正常，直接删除旧模型即可。");
            DownArrow();
            DrawText("有一处需要手动处理。\n \n第三人称武器<i>（TPWeapons）</i>已从旧玩家模型迁移到新模型，但模型之间的局部坐标轴朝向往往不同，因此这些武器在新玩家手中的位置和朝向不会正确，需要手动调整。可以先调整 <b>第三人称武器根节点</b>，也就是所有第三人称武器的父节点，即名为 <b>RemoteWeapons</b> 的对象。\n \n以下是替换玩家模型后武器可能出现的样子<i>（大致如此）</i>：");
            DrawNote("自 1.8 版本起，玩家会自动摆出 <i><b>瞄准姿态</b></i>，便于调整武器位置。即便你的玩家外观与下图不同，原理一致：<b>像玩家双手握持那样摆放武器。</b>");
            DrawImage(GetServerImage(4));
            DownArrow();
            DrawText("要调整位置和朝向，请选中玩家预制体中的 <b>RemoteWeapons</b> 对象<i>（位于玩家模型的右手下）</i>，或点击下方按钮尝试在层级面板中自动定位它。\n");
            if (DrawButton("定位 RemoteWeapons"))
            {
                if (PlayerInstantiated != null)
                {
                    Transform t = PlayerInstantiated.GetComponent<bl_PlayerNetwork>().NetworkGuns[0].transform.parent;
                    Selection.activeTransform = t;
                    EditorGUIUtility.PingObject(t);
                    if (t != null)
                        NextStep();
                }
            }

        }
        else if (subStep == 3)
        {
            DrawText("此时 RemoteWeapons 对象应在层级面板中被选中并框显。为方便预览武器位置，若当前没有启用或显示任何武器，请从 <i>RemoteWeapons</i> 对象中选择一个并启用它以显示；若已启用多个，建议全部禁用只保留一个，便于观察。\n\n然后重新选中 <b>RemoteWeapons</b> 父节点<i>（不是某个武器子节点）</i>，旋转或移动它，使武器看起来像被玩家握在右手中，效果大致如下：");
            DrawNote("自 1.8 版本起，玩家会自动摆出 <i><b>瞄准姿态</b></i>，便于调整武器位置。即便你的玩家外观与下图不同，原理一致：<b>像玩家双手握持那样摆放武器。</b>");
            DrawImage(GetServerImage(5));
            DrawNote("你也可以在层级中启用某把第三人称武器 <i>位于 RemoteWeapons 的 Transform 下</i> 来查看其外观，并按新玩家逐把微调，使姿态更准确。");
            DownArrow();
            DrawSuperText("<?background=#CCCCCCFF>瞄准位置</background>\n\n武器位置调整完成后，同样只保留一个第三人称武器处于激活状态以便预览姿势。\n \n手臂瞄准位置由 IK 控制，可在检视面板中自定义。选中玩家预制体中带 <b>(NEW)</b> 标记的玩家模型，它位于 RemotePlayer 对象内 ➔ 打开检视面板 ➔ bl_PlayerIK ➔ 在脚本检视面板底部 ➔ 点击 <b>预览瞄准位置</b> 按钮 ➔ 移动自动选中的轴心点，可看到手臂随之移动 ➔ 将轴心点放到你想要的瞄准位置 ➔ 确定后点击黄色的 <b>完成</b> 按钮即可。");
            DrawNote("请确保编辑器中已启用 <b>Gizmos</b>，否则无法移动轴心。");
            DrawAnimatedImage(0);
            DownArrow();
            DrawText("完成后，如果还没有删除旧模型，现在请删除：");
            DrawImage(GetServerImage(6));
        }
        else if (subStep == 4)
        {
            DrawText("现在需要将该预制体复制到 <b>Resources</b> 文件夹，拖到 MFPS -> Resources 即可，可按需重命名。");
            DrawImage(GetServerImage(7));
            DownArrow();
            DrawText("接下来需要将该新玩家预制体分配给某个阵营使用（阵营 1 或阵营 2）。打开 GameData（同样位于 Resources 文件夹）-> Players 区段，在对应字段（Team1 或 Team2）中指定，" +
                "拖入新的玩家预制体。");
            DrawImage(GetServerImage(8));
        }
        else if (subStep == 5)
        {
            DrawText("完成，新玩家模型已集成！\n\n 请注意：部分模型与默认玩家动画的重定向并不完全兼容，会导致" +
                "会导致部分动画看起来不自然。遗憾的是无法自动修复。你有两个选择：编辑该动画，或替换成你确认" +
                " 可用的动画。关于如何替换动画的更多信息见文档。");
            GUILayout.Space(7);
            DrawText("希望提供多个玩家选项供玩家挑选？请查看 <b>玩家选择器</b> 扩展，可添加任意数量的玩家模型：");
            GUILayout.Space(5);
            if (DrawButton("玩家选择器"))
            {
                Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/player-selector/");
            }
            DrawImage(GetServerImage(9));
        }
    }

    void PlayerModelAssetsDoc()
    {
        DrawText("以下资源商店玩家模型素材可用于集成到 MFPS");
        Space(10);
        playerAssets.OnGUI();
    }

    void UnPackPrefab(GameObject prefab)
    {
#if UNITY_2018_3_OR_NEWER
        if (PrefabUtility.GetPrefabInstanceStatus(prefab) == PrefabInstanceStatus.Connected)
        {
            PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
#endif
    }

    void SetUpModelInPrefab()
    {
        UnPackPrefab(PlayerModel);
        UnPackPrefab(PlayerInstantiated);
        GameObject TempPlayerPrefab = PlayerInstantiated;
        GameObject TempPlayerModel = PlayerModel;

        //change name of prefabs to identify
        PlayerInstantiated.gameObject.name += " [NEW]";
        PlayerInstantiated.transform.SetAsLastSibling();
        PlayerModel.name += " [NEW]";

        // get the current player model
        GameObject RemoteChildPlayer = TempPlayerPrefab.GetComponentInChildren<bl_PlayerAnimationsBase>().gameObject;
        GameObject ActualModel = TempPlayerPrefab.GetComponentInChildren<bl_PlayerIKBase>().gameObject;
        Transform NetGunns = TempPlayerPrefab.GetComponent<bl_PlayerNetwork>().NetworkGuns[0].transform.parent;

        //set the new model to the same position as the current model
        TempPlayerModel.transform.parent = RemoteChildPlayer.transform;
        TempPlayerModel.transform.localPosition = ActualModel.transform.localPosition;
        TempPlayerModel.transform.localRotation = ActualModel.transform.localRotation;

        //add and copy components of actual player model
        bl_PlayerIK ahl = ActualModel.GetComponent<bl_PlayerIK>();
        if (TempPlayerModel.GetComponent<Animator>() == null) { TempPlayerModel.AddComponent<Animator>(); }
        Animator NewAnimator = TempPlayerModel.GetComponent<Animator>();

        if (ahl != null)
        {
            bl_PlayerIK newht = TempPlayerModel.AddComponent<bl_PlayerIK>();
            newht.Target = ahl.Target;
            newht.Body = ahl.Body;
            newht.Weight = ahl.Weight;
            newht.Head = ahl.Head;
            newht.Lerp = ahl.Lerp;
            newht.Eyes = ahl.Eyes;
            newht.Clamp = ahl.Clamp;
            newht.useFootPlacement = ahl.useFootPlacement;
            newht.FootHeight = ahl.FootHeight;
            newht.FootLayers = ahl.FootLayers;
            newht.AimSightPosition = ahl.AimSightPosition;
            newht.HandOffset = ahl.HandOffset;
            newht.TerrainOffset = ahl.TerrainOffset;
            newht.leftFeetRotationOffset = ahl.leftFeetRotationOffset;
            newht.rightFeetRotationOffset = ahl.rightFeetRotationOffset;
            newht.leftKneeTarget = ahl.leftKneeTarget;
            newht.rightKneeTarget = ahl.rightKneeTarget;

            Animator oldAnimator = ActualModel.GetComponent<Animator>();
            NewAnimator.runtimeAnimatorController = oldAnimator.runtimeAnimatorController;
            NewAnimator.applyRootMotion = oldAnimator.hasRootMotion;
            if (NewAnimator.avatar == null)
            {
                NewAnimator.avatar = oldAnimator.avatar;
                Debug.LogWarning("你的新模型没有 Avatar，这可能导致动画出现问题，请务必手动添加。");
            }
        }
        Transform RightHand = NewAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        if (RightHand == null)
        {
            Debug.Log("无法从新模型获取右手骨骼，确认这是 humanoid 骨骼类型吗？");
            return;
        }

        var tempPlayerReferences = TempPlayerPrefab.GetComponent<bl_PlayerReferences>();
        tempPlayerReferences.PlayerAnimator = NewAnimator;
        var pa = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerAnimationsBase>();
        var tempRagdoll = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerRagdoll>();
        pa.Animator = NewAnimator;
        ActualModel.SetActive(false);
        tempRagdoll.SetUpHitBoxes();
        tempPlayerReferences.hitBoxManager.SetupHitboxes(NewAnimator);
        tempPlayerReferences.playerSettings.carrierPoint = NewAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

        if (tempPlayerReferences.gunManager != null)
        {
            // hide the FPWeapons so the TPWeapons can be seen clearly.
            foreach (var weapon in tempPlayerReferences.gunManager.AllGuns)
            {
                if (weapon == null) continue;
                weapon.gameObject.SetActive(false);
            }
        }

        EditorUtility.SetDirty(tempPlayerReferences);

        if (autoPoseAiming)
        {
            if (pa.Animator != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    pa.Animator.Update(0);
                }
            }
        }

        if (RightHand != null)
        {
            var npos = NetGunns.localPosition;
            var nrot = NetGunns.localRotation;

            NetGunns.parent = RightHand;
            if (weaponOrientationMode == TPWeaponOrientationMode.SameLocalAsOldModel)
            {
                NetGunns.localPosition = npos;
                NetGunns.localRotation = nrot;
            }
            else if (weaponOrientationMode == TPWeaponOrientationMode.ResetInNewModel)
            {
                NetGunns.localPosition = Vector3.zero;
                NetGunns.rotation = RightHand.rotation;
            }
            else
            {

            }
        }
        else
        {
            Debug.Log("找不到右手骨骼");
        }

        ActualModel.name += " 请删除此项";
        ActualModel.SetActive(false);

        var view = (SceneView)SceneView.sceneViews[0];
        var pbounds = MFPSEditorUtils.GetTransformBounds(tempPlayerReferences.gameObject);
        pbounds.center += Vector3.up * 0.5f;
        view.LookAt(pbounds.center);
        //view.Frame(pbounds);

        view.ShowNotification(new GUIContent("玩家设置"));
        Debug.Log("玩家模型已集成。");
    }

    private Rigidbody[] GetRigidBodys(Transform t)
    {
        Rigidbody[] R = t.GetComponentsInChildren<Rigidbody>();
        return R;
    }

    private Collider[] GetCollider(Transform t)
    {
        Collider[] R = t.GetComponentsInChildren<Collider>();
        return R;
    }

    public enum TPWeaponOrientationMode
    {
        SameLocalAsOldModel,
        ResetInNewModel,
        KeepSameLocation
    }

    [MenuItem("游戏框架/教程/添加玩家", false, 500)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddPlayerTutorial));
    }
}