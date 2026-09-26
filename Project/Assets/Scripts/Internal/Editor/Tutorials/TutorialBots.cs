using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GFWKEditor;

public class TutorialBots : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "gfwk2/editor/bots/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "替换机器人模型", StepsLenght = 3 },
     new Steps { Name = "掩体点", StepsLenght = 0 },
     new Steps { Name = "bl_AICoverPointManager", StepsLenght = 0 },
     new Steps { Name = "机器人名称", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        allowTextSuggestions = true;
        FetchWebTutorials("gfwk2/tutorials/");
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawModel();
        }
        else if (window == 1) CoverPointDoc();
        else if (window == 2) AICoverPointManagerDoc();
        else if (window == 3) BotsNameDoc();
    }

    void CoverPointDoc()
    {
        DrawSuperText("GFWK 的 AI 系统支持掩体点。<b>掩体点是按策略分布在地图中的点位，可优化 AI 的导航寻路</b>。机器人会根据战场情况利用这些点，为自身行为增加随机性，用于躲避敌人或作为随机移动目标。\n \n这些点位推荐使用但并非必需。地图中掩体点越多，机器人的行为就越随机、导航也越自然。\n \n<?title=18>添加新的掩体点</title>\n \n添加掩体点只需复制一个现有点位，再手动摆放到地图中。\n \n为便于预览所有掩体点，可在 <i><b>地图场景层级 ➔ AIManager ➔ bl_AICoverPointManager ➔ Show Gizmos</b></i> 中开启辅助显示。");
        DrawServerImage("img-5.png");
        DownArrow();
        DrawText("每个掩体点都必须挂载 <b>bl_AICoverPoint</b> 脚本，否则不会作为掩体点生效，该脚本在检视面板中有几个公开属性：");
        DrawServerImage("img-6.png");
        DrawPropertieInfo("下蹲", "bool", "机器人使用该掩体点时是否下蹲或起身");
        DrawPropertieInfo("邻近掩体点", "List", "邻近的掩体点列表，当该掩体点被占用时作为备用。");
    }

    void AICoverPointManagerDoc()
    {
        DrawText("脚本 <b>bl_AICoverPointManager.cs</b> 挂载于每个 <i><b>地图场景 ➔ AIManager ➔ bl_AICoverPointManager</b></i> 下，负责掩体点的选择逻辑，当机器人请求掩体点时，该脚本会根据请求机器人的状态决定场景中使用哪个掩体点。\n\n该脚本在检视面板中有若干可调公开属性：");
        DrawServerImage("img-7.png");
        DrawPropertieInfo("最大距离", "float", "达成邻近关系的掩体点之间的最大距离。");
        DrawPropertieInfo("占用时间", "float", "掩体点被使用后再次可用的冷却时间。");
        DrawPropertieInfo("显示辅助线", "bool", "在地图中显示每个掩体点的辅助线框。");
        DrawPropertieInfo("烘焙邻近点", "Button", "自动计算场景中每个掩体点的邻近掩体点，每次编辑场景掩体点后都应执行一次。");
        DrawPropertieInfo("对齐到地面", "Button", "自动调整掩体点的垂直位置，使其紧贴正下方地面而不悬空。");
    }

    GameObject ModelPrefab = null;
    void DrawModel()
    {
        if (subStep == 0)
        {
            DrawText("要替换某个机器人预制体中的人形模型，需要一个人形骨骼模型。\n \n" +
                "模型需在导入设置中配置为 <b>Humanoid</b> 骨骼类型：");
            Space(2);
            DrawImage(GetServerImage(0));
            DownArrow();
            DrawText("然后把玩家模型拖入下方空槽，点击 <b>创建</b> 按钮");
            Space(2);
            GUILayout.BeginVertical("box");
            ModelPrefab = EditorGUILayout.ObjectField("人形模型", ModelPrefab, typeof(GameObject), true) as GameObject;
            if (ModelPrefab != null)
            {
                Space(4);
                if (DrawButton("创建"))
                {
                    ReplaceBotModel();
                    NextStep();
                }
            }
            GUILayout.EndVertical();
        }else if(subStep == 1)
        {
            DrawText("如果一切正常，场景层级中会出现名为 <b>AISoldier [NEW]</b> 的预制体，其中已集成你的人形模型，这就是机器人预制体。" +
                "模型已自动集成并配置完成，但仍有一处需要手动修正，武器模型已移动到新模型的右手骨骼下，" +
                "但其位置可能有偏差，需要自行调整到位。");
            DrawImage(GetServerImage(1));
            DrawText("点击下方按钮可自动选中武器父节点。");
            Space(2);
            if(DrawButton("选择机器人武器父节点"))
            {
                var asw = FindObjectOfType<bl_AIShooterAttack>();
                Transform wr = asw.aiWeapons[0].transform.parent;
                Selection.activeTransform = wr;
                EditorGUIUtility.PingObject(wr);
            }
            DownArrow();
            DrawText("现在调整武器位置，即移动已选中的节点，使其看起来像被模型握持：");
            DrawImage(GetServerImage(2));
        }else if(subStep == 2)
        {
            DrawText("一切就绪，现在需要为其创建预制体，或替换现有的机器人预制体。" +
                "将层级中的 <b>AISoldier [NEW]</b> 拖入 <b>Resources</b> 文件夹，默认可拖到 <i>GFWK -> Resources</i>，在该文件夹中可创建预制体" +
                "或替换默认的机器人预制体（AISoldier 或 AISoldier2）。若创建了新预制体，还需在 游戏数据 -> BotTeam1 或 BotTeam2 中指定该预制体。");
            DrawImage(GetServerImage(3));
            DrawText("就这些 :)");
        }
    }

    void BotsNameDoc()
    {
        DrawHyperlinkText("机器人名称从一份预设名称列表中随机选取，作为开发者你可以方便地修改。\n \n首先可以定义随机名称的前缀，默认为 <b>BOT</b>，可在 <link=asset:Assets/Resources/GameData.asset>GameData</link> ➔ <b>Bots Name Prefix</b> 中改成任意内容。\n \n要修改随机名称列表，打开脚本 <b>bl_GameTexts.cs</b> ➔ <b>RandomNames</b>，在该列表中增删或编辑任意一项即可。");
        DrawServerImage("img-8.png");
    }

    void ReplaceBotModel()
    {
        if (ModelPrefab == null) return;
        GameObject model = ModelPrefab;
        if(PrefabUtility.IsPartOfAnyPrefab(ModelPrefab))
        {
            model = PrefabUtility.InstantiatePrefab(ModelPrefab) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
        }
        model.name += " [NEW]";
        GameObject botPrefab = PrefabUtility.InstantiatePrefab(bl_GameData.Instance.BotTeam1.gameObject) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(botPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
        botPrefab.name = "AISoldier 新建";
        var oldModel = botPrefab.GetComponentInChildren<bl_AIAnimationBase>();
        oldModel.name += " [OLD]";
        Animator modelAnimator = model.GetComponent<Animator>();
        modelAnimator.applyRootMotion = false;
        modelAnimator.runtimeAnimatorController = oldModel.GetComponent<Animator>().runtimeAnimatorController;
        if (!AutoRagdoller.Build(modelAnimator))
        {
            Debug.LogError("无法为该模型创建布娃娃");
            return;
        }

        bl_AIShooterAgent aisa = botPrefab.GetComponent<bl_AIShooterAgent>();
        if(aisa != null)
        botPrefab.GetComponent<bl_AIShooterAgent>().aimTarget = modelAnimator.GetBoneTransform(HumanBodyBones.Spine);
        var botReferences = botPrefab.GetComponent<bl_AIShooterReferences>();

        model.transform.parent = oldModel.transform.parent;
        model.transform.localPosition = oldModel.transform.localPosition;
        model.transform.localRotation = oldModel.transform.localRotation;
        var aia = model.AddComponent<bl_AIAnimation>();
        botReferences.aiAnimation = aia;
        aia.mRigidBody.Clear();
        aia.mRigidBody.AddRange(model.transform.GetComponentsInChildren<Rigidbody>());
        Collider[] allColliders = model.transform.GetComponentsInChildren<Collider>();
        for (int i = 0; i < allColliders.Length; i++)
        {
            allColliders[i].gameObject.layer = LayerMask.NameToLayer("Player");
        }
        botReferences.hitBoxManager.SetupHitboxes(modelAnimator);
        botReferences.PlayerAnimator = modelAnimator;
        EditorUtility.SetDirty(botReferences.hitBoxManager);
        Transform weaponRoot = botPrefab.GetComponent<bl_AIShooterAttack>().aiWeapons[0].transform.parent;
        Vector3 wrp = weaponRoot.localPosition;
        Quaternion wrr = weaponRoot.localRotation;
        weaponRoot.parent = modelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        weaponRoot.localRotation = wrr;
        weaponRoot.localPosition = wrp;
        DestroyImmediate(oldModel.gameObject);

        var view = (SceneView)SceneView.sceneViews[0];
        view.LookAt(botPrefab.transform.position);
        EditorGUIUtility.PingObject(botPrefab);
        Selection.activeTransform = botPrefab.transform;
    }

    [MenuItem("游戏框架/教程/更换机器人", false, 501)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TutorialBots));
    }
}