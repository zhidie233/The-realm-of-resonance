using MFPSEditor;
using System.IO;
using System.Linq;
using UnityEditor;
//using Lovatto.DevTools;
using UnityEditor.Animations;
using UnityEngine;

public class MFPSGeneralDoc : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/general/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.jpg", Image = null},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.png", Image = null},
        new NetworkImages{Name = "img-9.png", Image = null},
        new NetworkImages{Name = "img-10.png", Image = null},
        new NetworkImages{Name = "img-11.png", Image = null},
        new NetworkImages{Name = "img-12.png", Image = null},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_33.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_6.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_27.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_14.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_31.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_22.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_24.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_19.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_29.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://img.youtube.com/vi/ysYqI4w1vq4/0.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_23.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "img-13.jpg", Image = null},
        new NetworkImages{Name = "img-14.jpg", Image = null},
        new NetworkImages{Name = "img-15.png", Image = null},
        new NetworkImages{Name = "img-16.png", Image = null},
        new NetworkImages{Name = "img-17.png", Image = null},
        new NetworkImages{Name = "img-18.png", Image = null},
        new NetworkImages{Name = "img-19.png", Image = null},
        new NetworkImages{Name = "img-20.png", Image = null},//31
        new NetworkImages{Name = "img-21.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "createwindowobj.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
        new GifData{ Path = "createwindowbutton.gif" },
        new GifData{ Path = "addonintegrateprevw.gif"},
        new GifData{ Path = "mfps-urpcsps.gif"},
        new GifData{ Path = "playerai-1.gif"},
        new GifData{ Path = "cpaoc.gif"},
        new GifData{ Path = "adcwtp.gif"},
        new GifData{ Path = "adcwtp2.gif"},
    };
    private Steps[] AllSteps = new Steps[] {
    new Steps { Name = "首页", StepsLenght = 0, DrawFunctionName = nameof(Resume) },
    new Steps { Name = "术语", StepsLenght = 0, DrawFunctionName = nameof(TerminologyDoc) },
    new Steps { Name = "如何修改 MFPS", StepsLenght = 3, DrawFunctionName = nameof(HowToEditMFPSDoc),
     SubStepsNames = new string[]{ "如何修改", "代码修改", "其他修改" } },
    new Steps { Name = "游戏数据", StepsLenght = 0, DrawFunctionName = nameof(GameDataDoc) },
    new Steps { Name = "Photon PUN", StepsLenght = 3, DrawFunctionName = nameof(DrawPhotonPunDoc),
    SubStepsNames = new string[]{ "Photon PUN", "Photon 服务器", "什么是 CCU？"}},
    new Steps { Name = "离线模式", StepsLenght = 0, DrawFunctionName = nameof(OfflineDoc) },
    new Steps { Name = "URP 管线", StepsLenght = 4, DrawFunctionName = nameof(UniversalRPDoc) },
    new Steps { Name = "HDRP 管线", StepsLenght = 5, DrawFunctionName = nameof(HDRPDoc) },
    new Steps { Name = "击杀提示", StepsLenght = 2, DrawFunctionName = nameof(KillFeedDoc) },
    new Steps { Name = "玩家预制体", StepsLenght = 0, DrawFunctionName = nameof(PlayerPrefabsDoc) },
    new Steps { Name = "玩家兵种", StepsLenght = 0, DrawFunctionName = nameof(PlayerClassesDoc) },
    new Steps { Name = "头部晃动", StepsLenght = 0, DrawFunctionName = nameof(HeadBobDoc) },
    new Steps { Name = "游戏音频", StepsLenght = 3, DrawFunctionName = nameof(AudioDoc),
    SubStepsNames = new string[]{ "音频换皮", "音频范围", "音频资源"}},
    new Steps { Name = "游戏文本", StepsLenght = 0, DrawFunctionName = nameof(DrawGameTexts) },
    new Steps { Name = "游戏输入", StepsLenght = 5, DrawFunctionName = nameof(GameInputDoc),
    SubStepsNames = new string[]{ "游戏输入", "默认映射", "添加输入", "手柄", "添加映射" }},
    new Steps { Name = "游戏界面", StepsLenght = 2, DrawFunctionName = nameof(GameUIDoc),
    SubStepsNames = new string[]{ "界面换皮", "界面资源" }},
    new Steps { Name = "队伍", StepsLenght = 0, DrawFunctionName = nameof(DrawTeamsDoc) },
    new Steps { Name = "金币", StepsLenght = 2 , DrawFunctionName = nameof(DrawCoins),
    SubStepsNames = new string[]{ "属性", "操作" } },
    new Steps { Name = "游戏模式", StepsLenght = 3, DrawFunctionName = nameof(GameModesDoc),
    SubStepsNames = new string[]{ "游戏模式", "自定义模式", "按地图模式" } },
    new Steps { Name = "玩家动画", StepsLenght = 4, DrawFunctionName = nameof(DrawPlayerAnimationDoc),
    SubStepsNames = new string[]{ "基础", "进阶", "武器动画", "动画资源" } },
    new Steps { Name = "名牌", StepsLenght = 0, DrawFunctionName = nameof(NamePlatesDoc) },
    new Steps { Name = "子弹", StepsLenght = 3 , DrawFunctionName = nameof(DrawBullets),
    SubStepsNames = new string[]{ "子弹预制体", "子弹贴花", "自定义子弹" } },
    new Steps { Name = "装备套件", StepsLenght = 0, DrawFunctionName = nameof(DrawKitsSystem) },
    new Steps { Name = "击杀区域", StepsLenght = 0, DrawFunctionName = nameof(DrawKillZones) },
    new Steps { Name = "房间属性", StepsLenght = 0, DrawFunctionName = nameof(RoomPropertiesDoc) },
    new Steps { Name = "游戏设置", StepsLenght = 0, DrawFunctionName = nameof(DrawGameSettings) },
    new Steps { Name = "鼠标视角", StepsLenght = 0, DrawFunctionName = nameof(MouseLookDoc) },
    new Steps { Name = "对象池", StepsLenght = 0, DrawFunctionName = nameof(DrawObjectPooling) },
    new Steps { Name = "新增菜单", StepsLenght = 0, DrawFunctionName = nameof(AddNewMenu) },
    new Steps { Name = "玩家手臂 IK", StepsLenght = 0, DrawFunctionName = nameof(PlayerIKDoc) },
    new Steps { Name = "准星", StepsLenght = 0, DrawFunctionName = nameof(CrosshairDoc) },
    new Steps { Name = "移动端", StepsLenght = 0, DrawFunctionName = nameof(DrawMobileDoc) },
    new Steps { Name = "粒子与贴花", StepsLenght = 2, DrawFunctionName = nameof(ParticlesDecalsDoc) ,
    SubStepsNames = new string[]{ "粒子与贴花", "特效资源" }},
    new Steps { Name = "后处理", StepsLenght = 3, DrawFunctionName = nameof(PostProcessingDoc),
        SubStepsNames = new string[]{ "后处理", "自定义配置", "错误处理" } },
    new Steps { Name = "好友列表", StepsLenght = 0, DrawFunctionName = nameof(DrawFriendListDoc) },
    new Steps { Name = "游戏内聊天", StepsLenght = 0, DrawFunctionName = nameof(InGameChatDoc) },
    new Steps { Name = "脚步声", StepsLenght = 0, DrawFunctionName = nameof(FootStepsDoc) },
    new Steps { Name = "门", StepsLenght = 0, DrawFunctionName = nameof(DoorsDoc) },
    new Steps { Name = "服务器区域", StepsLenght = 0, DrawFunctionName = nameof(ServerRegionDoc) },
    new Steps { Name = "本地通知", StepsLenght = 0, DrawFunctionName = nameof(LocalNotificationsDoc) },
    new Steps { Name = "踢人投票", StepsLenght = 0, DrawFunctionName = nameof(KickVotationDoc) },
    new Steps { Name = "游戏管理员", StepsLenght = 0, DrawFunctionName = nameof(GameStaffDoc) },
    new Steps { Name = "网络状态", StepsLenght = 0, DrawFunctionName = nameof(NetworkStats) },
    new Steps { Name = "玩家碰撞盒", StepsLenght = 2, DrawFunctionName = nameof(PlayerHitboxDoc),
    SubStepsNames = new string[]{ "碰撞体", "伤害"}},
    new Steps { Name = "伤害结算", StepsLenght = 2, DrawFunctionName = nameof(PlayerDamageDoc),
     SubStepsNames = new string[]{ "玩家伤害", "物体伤害"}},
    new Steps { Name = "梯子", StepsLenght = 0, DrawFunctionName = nameof(LadderDoc) },
    new Steps { Name = "事件", StepsLenght = 0, DrawFunctionName = nameof(MFPSEventsDoc) },
    new Steps { Name = "扩展", StepsLenght = 0, DrawFunctionName = nameof(DrawAddonsDoc) },
    new Steps { Name = "编辑器菜单", StepsLenght = 0, DrawFunctionName = nameof(EditorMenusDoc) },
    new Steps { Name = "更新 MFPS", StepsLenght = 0, DrawFunctionName = nameof(UpdateMFPSDoc) },
    new Steps { Name = "反作弊", StepsLenght = 0, DrawFunctionName = nameof(AntiCheatDoc) },
    new Steps { Name = "第一人称手臂材质", StepsLenght = 0, DrawFunctionName = nameof(FPArmsMaterial) },
    new Steps { Name = "挂机检测", StepsLenght = 0, DrawFunctionName = nameof(AfkDoc) },
    new Steps { Name = "大厅聊天", StepsLenght = 0, DrawFunctionName = nameof(DrawLobbyChat) },
    new Steps { Name = "常见问题", StepsLenght = 0, DrawFunctionName = nameof(CommonQADoc) },
    new Steps { Name = "已知问题", StepsLenght = 0, DrawFunctionName = nameof(KnownIssuesDoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        FetchWebTutorials("mfps2/tutorials/");
        allowTextSuggestions = true;
    }

    void Resume()
    {
        DrawTitleText("游戏框架");
        DrawText("版本： " + MFPSEditor.AssetData.Version);
        DrawYoutubeCover("MFPS 入门视频", GetServerImage(22), "https://www.youtube.com/watch?v=ysYqI4w1vq4");
        DrawTitleText("热门教程");

        if (DrawLinkText("添加地图"))
        {
            EditorApplication.ExecuteMenuItem("游戏框架/教程/添加地图");
        }
        if (DrawLinkText("添加武器"))
        {
            EditorApplication.ExecuteMenuItem("游戏框架/教程/添加武器");
        }
        if (DrawLinkText("添加玩家"))
        {
            EditorApplication.ExecuteMenuItem("游戏框架/教程/添加玩家");
        }
        if (DrawLinkText("更换机器人"))
        {
            EditorApplication.ExecuteMenuItem("游戏框架/教程/更换机器人");
        }

        DrawTitleText("自定义集成教程");
        if (DrawLinkText("将加载界面集成到 MFPS"))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/integrate-loading-screen-to-mfps-2-0/");
        }
        if (DrawLinkText("将 DestroyIt 集成到 MFPS"))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/integrate-destroyit-to-mfps-2-0/");
        }
    }

    void TerminologyDoc()
    {
        DrawText("本文档中会频繁出现一些你可能不熟悉的词汇和说法，这里给出简要解释。");
        DrawHorizontalSeparator();
        Space(10);
        DrawHorizontalColumn("GameData", "一个 <i>ScriptableObject</i>，包含大量 MFPS 前端设置，可轻松调整以满足需求并为游戏换皮。选项从是否显示血液的简单开关，到武器和游戏模式信息一应俱全，位于 MFPS 的 <b>Resources</b> 文件夹中。\n\n更多信息请查看 <b>GameData</b> 章节。");

        DrawHorizontalColumn("Player Prefab", "构成 MFPS 玩家控制器的预制体，包含所需的全部脚本、对象和层级结构，\n默认位于 MFPS 的 <b>Resources</b> 文件夹中。\n\n更多信息请查看 <b>玩家预制体</b> 章节。");

        DrawHorizontalColumn("FPWeapon", "即<b>第一人称武器</b>，指本机玩家相机中看到的武器，也称<b>视图模型</b>。它与<i>第三人称武器</i>的区别在于包含武器本体和手臂或手部模型，且仅本机玩家相机可见。\n \n第一人称武器位于每个玩家预制体的 <b>Local</b> 子节点内。");

        DrawHorizontalColumn("TPWeapon", "即<b>第三人称武器</b>，指其他玩家的士兵模型上看到的武器，也称<b>世界模型</b>。它与<i>第一人称武器</i>的区别在于只是单个武器模型，放置在玩家或士兵模型的双手之间。\n \n第三人称武器位于每个玩家预制体的 <b>Remote</b> 子节点内，具体在玩家模型的右手骨骼上。");

        DrawHorizontalColumn("Unity 顶部菜单", "指 Unity 编辑器顶部的菜单项，<i><b>File、Edit、Assets、GameObject、Components、MFPS 等…</b></i>");
    }

    void HowToEditMFPSDoc()
    {
        if (subStep == 0)
        {
            DrawText("这个问题听起来像是修改 MFPS 会受到某种限制，但事实并非如此。只要清楚要做什么、知道怎么做，理论上没有任何限制，MFPS 包含游戏的完整源代码和资源，一切都可以按开发者意愿修改。\n \n本指南旨在给出修改 MFPS 的理想方式建议，<b>以便将来合并 MFPS 更新时轻松许多。</b>\n \n如果使用 MFPS 已有一段时间，你会知道它最大的问题之一就是更新合并。每个大版本更新都需导入到新的 Unity 工程，因为把新版本导入到已有旧版本 MFPS 的工程中，会丢失此前对游戏所做的全部修改，而且极可能直接搞坏游戏。因此在旧版本工程中应用新版本的改进和修复，唯一可行的办法就是手动合并，逐个脚本检查、逐个预制体比对，堪称噩梦。\n \n这在一定程度上源于 MFPS 的创建方式。由于代码硬编码和引用关系盘根错节，修改代码的唯一途径就是直接改游戏的默认脚本；\n另外与工具类或扩展类素材不同，游戏模板本就预期用户或开发者去修改核心内容，这也加剧了升级到新版本的难度。\n从 1.9 版起，游戏后端设计开始重做，大部分代码支持继承，开发者无需修改默认脚本即可改代码，只需新建一个脚本 ➔ 继承自基类 ➔ 实现自己的改动。\n \n下一页会详细介绍这一点以及如何使用这类代码设计。");
        }
        else if (subStep == 1)
        {
            DrawSuperText("<?background=#FFF>代码修改</background>\n\n从 MFPS 1.9 起，大部分默认脚本采用代码继承设计，<b>继承允许你或你的程序员基于游戏已有的类创建新类，在保持行为一致的前提下指定新的实现，实现代码复用，并独立扩展原有代码。</b>\n \n<b>以下举例说明这种设计如何工作、有何帮助：</b>\n \n在 MFPS 1.9 之前，若想修改武器拾取系统，必须直接改 <b>bl_GunPickUp.cs</b> <i>（默认脚本）</i>，这会导致该脚本无法再自动更新，否则你的改动就会丢失。\n而在 MFPS 1.9 中做同样的修改，无需直接改默认脚本，而是新建一个脚本并继承其基类（本例为 <b>bl_GunPickUpBase</b>）➔ 把默认脚本的代码复制到新脚本中，然后做想要的修改。由于所有引用都指向基类，无需担心引用问题，只需在原本挂载默认脚本的物体上换挂你的新脚本，并移除或卸载默认脚本即可。\n \n<?background=#FFF>如何继承脚本？</background>\n \n若你是有中高级经验的程序员，可能已熟悉这种编程设计，它不仅是优秀代码的应有之义，也让代码更易维护和扩展，并带来多态结构，从而以更少代价实现新功能或变体。\n \n若你不是程序员或不熟悉代码继承，做法很简单：以武器拾取为例新建一个脚本，假设该脚本用于修改武器拾取逻辑，创建后其结构大致如下：");
            DrawCodeText("using UnityEngine;\n \npublic class bl_GunPickUp2 : MonoBehaviour\n{\n    ...\n}");
            DrawText("接下来要修改基类<i>（紧随你的类名之后那个类名）</i>，此处原为 <color=#0E6148FF>MonoBeheaviour</color>，需改为 <i>bl_GunPickUp</i> 的基类 <color=#0E6148FF>bl_GunPickUpBase</color>，结果如下：");
            DrawCodeText("using UnityEngine;\n \npublic class bl_GunPickUp2 : bl_GunPickUpBase\n{\n    ...\n}");
            DrawNote("几乎所有默认脚本的基类都遵循同样的命名规则，只是在名称末尾加上 \"Base\" 一词。不过要确认某个脚本的基类，最可靠的方法还是打开该默认脚本查看。");
            DrawText("改好基类后，需要重写基类的函数或方法。若不知如何操作，直接把原类<i>（本例为 <b>bl_GunPickUp.cs</b>）</i>的代码复制粘贴到你的脚本中即可，\n这样就够了，之后便可在新脚本中做任意修改。");
            DrawHorizontalSeparator();
            DrawText("如前所述，这并非修改代码的强制要求，而是推荐做法。MFPS 并非所有代码都支持继承，截至 1.9 版已有一半以上代码按此方式重做，但工作仍在进行，目标是让全部或至少大部分代码支持继承并在未来更新中保持模块化。");
        }
        else if (subStep == 2)
        {
            DrawText("<i>代码修改</i>的同一思路也适用于预制体、菜单、场景等其他游戏内容的修改。\n<b>理想做法是不修改默认内容，而是创建副本</b>并使用副本，例如：\n对于 MainMenu 场景，应创建该场景的副本，保留默认场景原样以备将来参考。\n这样将来合并 MFPS 更新时，导入后不会丢失你在该场景上的修改，且保留原场景可作为参照，对比新版本在该场景中做了哪些改动。\n \n玩家预制体同理，不要使用默认玩家预制体<i>（MPlayer 和 MPlayer2）</i>，而应为每个创建副本并使用这些副本<i>（在 GameData 中指定）</i>。\n \n道理就是这样，不要使用默认内容 ➔ 创建并使用副本。");
            DrawNote("在编辑器中，几乎任何内容都可以通过以下方式复制：在 Project 视图中选中它，Windows 上按 <b>Ctrl + D</b>，Mac 上按 <b>Command + D</b>。");
        }
    }

    void GameDataDoc()
    {
        DrawTitleText("GameData");
        DrawText("使用 MFPS 时你会注意到，文档、readMe.txt 以及许多注释中都频繁提到 <b>GameData</b>。\n若不清楚 <b>GameData</b> 是什么、如何工作或位于何处，这里简要说明：\n\n<b>GameData</b> 是一个 <i>ScriptableObject</i>，包含大量前端设置，可轻松调整以满足需求并为游戏换皮。选项从是否显示血液的简单开关，到武器和游戏模式信息一应俱全。");
        DrawHyperlinkText("<link=asset:Assets/Resources/GameData.asset>GameData</link> 位于 MFPS 的 <b>Resources</b> 文件夹中：");
        DrawServerImage(3);
        DrawNote("也可通过 <b>MFPS 管理器</b> 窗口快速打开，在 Windows 编辑器中按 <b>Ctrl + M</b>，Mac 上按 <b>Command + M</b>。");
    }

    void DrawPhotonPunDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b>Photon Unity Networking</b> 简称 <b>Photon PUN</b> 是 MFPS 用来处理所有网络与服务端事务的网络方案，它是 Unity 领域最扎实的解决方案之一，速度快、稳定、可扩展，符合你对通用网络方案的预期。PUN 在全球提供多个服务器节点，你可以连接到延迟最低的那个。\n\n在 Unity 中 PUN 以第三方插件形式提供，可在 Unity 资源商店免费下载 <i><b><size=8>按照「快速开始」教程操作你多半已经下载过了</size></b></i>。\n关于这套网络方案有一些常见疑问。首先，服务端事务必然产生成本，PUN 承担了全部服务端工作，包括代码、托管、运维、扩容、服务器维护等，这些都不用你操心，Photon 团队会处理。但正因为存在服务成本，<b>PUN 是付费服务</b>，同时提供免费套餐供开发阶段使用，正式发布前再升级即可。\n");

            DrawHyperlinkText("你可以在其官网查看所有可用套餐：\n<link=https://www.photonengine.com/en-US/PUN/pricing>Photon PUN 套餐</link>\n");
            DrawHorizontalSeparator();
            DrawText("我经常收到的一个问题是：\n\n<i><b><size=16>那权威服务器呢？</size></b></i>\n\n如果你此前接触过网络系统，可能已经注意到 Photon 使用一个客户端 <i>称为 Master Client</i> 而非服务器 <i>Master Server</i> 来裁定游戏逻辑。这给作弊者留下了空间，他们可以在自己的客户端上修改玩法并同步给其他客户端，因为没有独立的 Master Server 来比对和校验逻辑。\n");
            DrawHyperlinkText("开箱即用的 Photon PUN 并没有针对这个问题的完备方案，官方给出的是 <link=https://www.photonengine.com/en-us/Server>Photon OnPremise</link> 即 Photon Server。用它你可以自行托管服务端代码与 SDK，通过修改服务端代码打造自己的权威服务器，但这需要相应的技术积累。如果你对 Photon Server 感兴趣，下一节有更多说明。");
        }
        else if (subStep == 1)
        {
            DrawText("如上一节所述，<b>Photon Server 是 Photon PUN 的替代方案</b>，它有一些优势，也有需要权衡的地方。相比 Photon PUN，使用 Photon Server 的好处包括：\n \n- 自托管服务器，对服务端代码有更强控制力。\n- CCU 套餐价格更实惠，并提供不限量套餐。\n- 可以在 PUN 不覆盖的特定区域部署服务器。\n- 整体成本低于 Photon PUN 套餐。");
            DrawText("但使用 Photon Server 时必须考虑以下问题：\n \n- 服务器的持续运行由你负责，崩溃、停机、清理等都要自行处理。\n\n- 可用区域取决于你把服务器托管在哪里。Photon PUN 可以轻松切换全球 13 个以上的区域，而 Photon Server 只有你托管的那一个区域，除非你在游戏里另外做一套服务器选择系统。\n\n- 扩展性取决于你的托管类型和套餐。Photon PUN 需要更多 CCU 时直接升级套餐即可，Photon Server 即便使用不限量 CCU 套餐，如果托管服务器本身不支持自动扩容，游戏规模增长就会遇到瓶颈。因此推荐使用 AWS EC2 这类可弹性伸缩的服务器方案。");
            DrawSuperText("Photon Server 的详细资料见官方页面：\n<?link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-intro>https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-intro</link>");
            Space(18);
            DrawSuperText("<b><size=16>在 MFPS 中使用 Photon Server</size></b>\n \n在 MFPS 中使用 Photon Server 替代 Photon PUN 不需要改动 MFPS 代码，但需要手动配置服务端 SDK，这需要一定的经验。好在官方文档对搭建和部署自有服务器的流程讲解得清晰且步骤完整，指南见此处：\n<?link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min>https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min</link>");
            DrawSuperText("Photon Server SDK 搭建并运行起来之后，项目里只需做几件事即可开始使用：\n \n- 在 MFPS 的 Unity 项目中打开 <?link=asset:Assets/Required/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset>PhotonServerSettings 点击此处</link>，其默认位置在 <i>Assets ➔ Required ➔ Photon ➔ PhotonUnityNetworking ➔ Resources ➔ PhotonServerSettings</i>。\n\n- 展开 <b>Server/Cloud Settings</b>，取消勾选 <b>Use Name Server</b>。\n\n- 在 <b>Server</b> 文本框中填入服务器 <i>即你部署 Photon Server SDK 的位置</i> 的公网 IP 或域名。\n \n- 在 <b>Port</b> 输入框中填写你为服务器开放的端口号，若未改动则使用默认值：<b>UDP 为 5055</b>，<b>TCP 为 4530</b>。\n \n完成以上设置后，若配置正确，游戏应当能正常运行并连接到你的服务器。");
            DrawServerImage("img-49.png", TextAlignment.Center);
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=16>什么是 CCU？</size></b>\n \n<b>CCU</b> 是 <b>Concurrently Connected Users</b> 的缩写，即同时在线用户数。在 Photon 及其他网络插件中，它指服务器或套餐允许的同时在线玩家上限。\n \nPhoton PUN 的每个套餐都有 CCU 上限，达到上限后会拒绝新连接，新玩家必须等到有位置空出才能进入。用于开发调试的默认套餐上限为 20 CCU，即最多 20 名玩家同时在线。");
            DrawNote("不要把 <b>CCU</b> 与 <b>DAU</b> <i>日活跃用户</i> 或 <b>MAU</b> <i>月活跃用户</i> 混淆，CCU 只统计同一时刻在线的玩家。");
            DrawText("<b><size=16>如何判断自己需要多大的 CCU 上限？</size></b>\n \nPhoton 套餐的 CCU 上限越高，月费越贵，因此选对套餐很重要。可以依据 CCU 与 DAU、MAU 之间的平均比例关系推算：根据你的游戏日活或月活玩家数量，就能大致判断需要多大的 CCU 套餐。");
            DrawNote("这些数据取自 Photon 官方统计，覆盖了大量使用 Photon 的游戏作品。");
            DrawText("CCU 到 DAU 的系数为 10 到 100 之间，视游戏而定。\nDAU 到 MAU 的系数为 10。\n \n所以 <b>100 CCU 可能足以支撑多达 10 万月活用户</b>。\n刚上线、初期关注度和在线时长都很高的新游戏会明显偏离这些数值，因此这些系数并不固定。");
        }
    }

    void OfflineDoc()
    {
        DrawText("MFPS 支持 Photon <b>离线模式</b>，可以直接运行地图场景进行测试，无需经过进入大厅、创建房间、加载地图场景的流程。\n\n当你修改了玩家预制体或武器并想立刻在运行时验证时，这个功能尤其省事，能节省大量时间、提升开发效率。\n\n开关位置在 <b>GameData</b> -> Offline Mode。\n启用后直接打开地图场景点运行即可。");
        DrawNote("离线模式<b>并非</b>用来借助 MFPS 开发单机游戏，它的目的是方便多人在线玩法的开发调试。");
    }

    void UniversalRPDoc()
    {
        if (subStep == 0)
        {
            DrawText("MFPS 默认使用旧版内置渲染管线。等 URP <i>通用渲染管线</i> 更加标准化之后，MFPS 会将其作为默认渲染管线。目前若要在 MFPS 中使用 URP 或 HDRP，必须手动转换项目，本文档说明具体做法：\n\n<b><size=20>将 MFPS 项目转换为 URP：</size></b>\n\n*<i>本教程假定你的 MFPS 项目基于内置渲染管线，Unity 版本为 2018.4 或更高</i>*\n\n首先需要移除 Post-Processing 包，操作为 Unity 顶部菜单 <b>MFPS -> Tools -> Delete Post-Processing</b>，等待脚本编译完成。\n\n<i>继续下一步。</i>");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=22>安装 URP</size></b>\n\n1. 在 Unity 中打开你的项目。\n2. 顶部导航栏选择 Window > Package Manager 打开包管理器窗口。\n3. 选择 All 选项卡，这里列出了当前 Unity 版本可用的全部包。\n4. 在包列表中选择 Universal RP。\n5. 点击包管理器窗口右下角的 Install。Unity 会把 URP 直接安装到你的项目中。\n\n<b><size=22>配置 URP</size></b>\n\n使用 URP 之前需要先完成配置。为此要创建一个可编程渲染管线资源并调整图形设置。\n\n创建通用渲染管线资源\n通用渲染管线资源控制项目的全局渲染与画质设置，并创建渲染管线实例。渲染管线实例包含中间资源与渲染管线实现。\n\n<b><size=16>创建通用渲染管线资源：</size></b>\n\n1. 在编辑器中打开 Project 窗口。\n2. 在 Project 窗口中右键，选择 Create > Rendering: Universal Render Pipeline: Pipeline Asset。也可以从顶部菜单栏选择 Assets: Create: Rendering: Universal Render Pipeline: Pipeline Asset。\n\n新建的通用渲染管线资源可以沿用默认名称，也可以自行命名。");
            DrawText("<b><size=16>将资源加入图形设置</size></b>\n\n要使用 URP，需要把刚创建的通用渲染管线资源加入 Unity 的图形设置，否则 Unity 仍会尝试使用内置渲染管线。\n\n将通用渲染管线资源加入图形设置的方法：\n\n打开 <b>Edit > Project Settings... > Graphics</b>。\n在 <b>Scriptable Render Pipeline Settings</b> 字段中加入之前创建的通用渲染管线资源。加入后图形设置会立即变化，项目即开始使用 URP。\n\n<b>此时你会看到一些乃至大量粉色物体</b>，原因是内置渲染管线的着色器无法在 URP 或 HDRP 下工作，必须升级材质着色器。下一步会说明如何转换。");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=22>升级着色器</size></b>\n\n如果项目使用的是内置渲染管线的着色器，而要改用通用渲染管线，就必须把这些着色器转换为 URP 着色器，因为内置的 Lit 着色器与 URP 着色器不兼容。内置着色器与 URP 着色器的对应关系可参考着色器映射表。\n\n升级内置着色器的方法：\n\n1. 在 Unity 中打开项目，进入 Edit > Render Pipeline > Universal Render Pipeline。\n2. 选择 <b>Upgrade Project Materials to URP Materials</b>\n\n\n<b>注意：</b>该操作不可撤销，升级前请先备份项目。\n\n<b>提示：</b>升级后若 Project 视图中的预览缩略图显示异常，可在 Project 视图窗口内右键并选择 Reimport All。");
            Space(10);
            DrawText("完成之后可能仍有一些粉色物体，它们是使用了自定义着色器的对象。修复方式很简单，选中这些对象，把其材质着色器改为通用渲染管线着色器即可。\n");
            DrawServerImage(31);
            DrawText("还有最后一步要做，见下一步。");
        }
        else if (subStep == 3)
        {
            DrawText("最后还有一处需要设置。\nURP 和 HDRP 中的相机工作机制与内置渲染管线不同。URP/HDRP 中有一个 Base 相机，若想同时渲染其他相机，必须把它设为 <b>Overlay Camera</b> 并加入 <b>Base Camera</b> 的相机 <b>Stack</b> 列表。\n\nMFPS 的玩家使用两个相机，一个只渲染第一人称武器，另一个渲染其余所有内容。因此需要把渲染第一人称武器的那个相机配置为 Overlay Camera，<b>你使用的每个玩家预制体都要照此处理：</b>\n\n1. 在 <b>Project 窗口</b>中选中玩家预制体 <i>MFPS 默认的玩家预制体位于其 Resources 文件夹中</i>，点击 <b>Open Prefab</b> 按钮。");

            DrawNote("<color=#FFFC01FF>注意：</color>选中玩家预制体时可能出现警告信息，原因是移除 Post-Processing 包后 Weapon Camera 上留下了一个空组件。解决方法是从 Weapon Camera 上删除该空组件。");
            DrawText("打开玩家预制体后执行以下操作：");
            DrawAnimatedImage(4);
            DrawText("至此即可在 URP 下使用 MFPS。记住最后这一步要在你使用的所有玩家预制体上重复执行。");
        }
    }

    void HDRPDoc()
    {
        if (subStep == 0)
        {
            DrawText("HDRP 是 Unity 面向高端平台的新渲染管线之一，可以调用前沿的实时 3D 渲染技术，提供高保真画质与不妥协的 GPU 性能。MFPS 默认使用内置渲染管线，你也可以手动把项目转换为 HDRP，本指南说明具体做法。");
            DrawSuperText("<b><size=16>将 MFPS 转换为 HDRP</size></b>\n \n本教程假定你的 MFPS 项目基于内置渲染管线，Unity 版本为 2020.1 或更高。教程中提到的部分选项在新版编辑器中位置或名称可能不同，遇到这种情况可查阅对应 Unity 版本的官方指南：\n<?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Upgrading-To-HDRP.html>https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Upgrading-To-HDRP.html</link>");
            DrawText("配置渲染管线之前必须先移除 <b>Post Processing</b> 包，因为 HDRP 不支持它，HDRP 自带了后处理系统。自动移除的方式为 Unity 顶部菜单 MFPS ➔ Tools ➔ <b>Delete Post-Processing</b>，等待脚本编译完成后继续下一步。");
        }
        else if (subStep == 1)
        {
            DrawSuperText("<b><size=16>配置 HDRP</size></b>\n\n首先为 Unity 项目添加 High Definition RP 包以安装 HDRP：\n \n<?list=•>打开 Unity 项目。\n通过 Window > Package Manager 打开包管理器窗口。\n在包管理器窗口的 Packages: 字段中，从菜单选择 <b>Unity Registry</b>。\n在包列表中选择 <b>High Definition RP</b>。\n点击包管理器窗口右下角的 <b>Install</b>。</list>");
            DrawNote("<i><size=8><color=#76767694>注意：</color></size></i> 安装 HDRP 后，Unity 会自动为场景中的 GameObject 附加两个 HDRP 专用组件：为灯光附加 <b>HD Additional Light Data</b>，为相机附加 <b>HD Additional Camera Data</b>。如果项目未设置为使用 HDRP，而场景中又存在 HDRP 组件，Unity 会报错。要解决这些错误，按下面的说明在项目中配置 HDRP。");
            DrawSuperText("在项目中配置 HDRP 请使用 <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Render-Pipeline-Wizard.html>HDRP 向导</link>。\n \n通过 <b>Window > Rendering > HD Render Pipeline Wizard</b> 打开 <b>HD Render Pipeline Wizard</b> 窗口。\n在 <b>Configuration Checking</b> 区域切到 <b>HDRP</b> 选项卡，点击 <b>Fix All</b>，这会修复项目全部 HDRP 配置问题。\n配置问题修复之后场景可能仍渲染不正常，因为场景中的 GameObject 依然在使用内置渲染管线的着色器。内置着色器升级为 HDRP 着色器的方法见下一步的「升级材质」。");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=16>升级材质</size></b>\n\n将场景中的材质升级为 HDRP 兼容材质的方法：\n \n1. 打开 <b>Edit > Rendering > Materials</b>\n2. 选择以下选项之一：\n \n ■ <b>Convert All Built-in Materials to HDRP</b>：把项目中所有可转换的材质转为 HDRP 材质。\n\n ■ <b>Convert Selected Built-in Materials to HDRP</b>：把 Project 窗口中当前选中的可转换材质转为 HDRP 材质。\n\n ■ <b>Convert Scene Terrains to HDRP Terrains</b>：把场景中每个地形的内置默认标准地形材质替换为 HDRP 默认地形材质。");
            DrawText("<b><size=16>限制</size></b>\n\n上述自动升级选项无法把所有材质都正确转换到 HDRP：\n \n自定义材质与着色器无法自动升级，必须手动转换。MFPS 中的水面着色器属于自定义着色器，无法自动升级，你需要把它替换为 HDRP 水面着色器，或者从场景中移除水面。\n\n使用高度图的材质可能显示异常。原因是 HDRP 支持的高度图置换技术与压缩选项比内置渲染管线更多。要升级使用高度图的材质，请调整材质的 Amplitude 与 Base 属性，直到效果尽量接近内置渲染管线下的表现。\n\n<b>粒子着色器无法升级</b>。HDRP 不支持粒子着色器，但提供了与内置粒子系统兼容的 Shader Graph。这些 Shader Graph 的工作方式与内置粒子着色器类似。要使用它们，请导入 Particle System Shader Samples 示例：\n \n 1. 打开 <b>Package Manager</b> 窗口，菜单为 <b>Window > Package Manager</b>。\n 2. 找到并点击 <b>High Definition RP</b> 条目。\n 3. 在 <b>High Definition RP</b> 的包信息中进入 <b>Samples</b> 区域，点击 <b>Particle System Shader Samples</b> 旁的 <b>Import into Project</b> 按钮。");
        }
        else if (subStep == 3)
        {
            DrawSuperText("<b><size=18>调整光照</size></b>\n\nHDRP 使用 <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Physical-Light-Units.html>物理光照单位</link> 控制灯光强度，这些单位与内置渲染管线使用的任意单位并不对应。\n \n就光强单位而言，平行光使用 Lux，其余灯光类型可使用 Lumen、Candela、EV，或模拟特定距离处的 Lux。\n \n在 HDRP 项目中配置光照的步骤：\n \n1. 为场景添加默认天空 Volume 并配置环境光，操作路径为 <b>GameObject > Volume > Sky and Fog Global Volume</b>。\n\n2. 让 <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Environment-Lighting.html>环境光照</link> 使用这个新天空：\n \n - 打开光照窗口，菜单为 <b>Window > Rendering > Lighting Settings</b>。\n - 在 <b>Environment</b> 选项卡中，把 <b>Profile</b> 属性设为 Sky and Fog Global Volume 所用的同一个 Volume Profile。\n - 把 <b>Static Lighting Sky</b> 属性设为 <b>PhysicallyBasedSky</b>。\n - 可选：如果不想让 Unity 在本节后续改动时重新烘焙场景光照，可以取消勾选窗口底部的 <b>Auto Generate</b>。");
            DrawText("3. 当前阴影画质偏低，提升阴影画质的方法：\n \n新建一个 <b>Global Volume</b> GameObject，菜单为 <b>GameObject > Volume > Global Volume</b>，命名为 <b>Global Settings</b>。\n为该 Global Volume 新建 Volume Profile：\n \n - 打开 Global Volume 的 Inspector 窗口，找到 Volume 组件。\n - 进入 <b>Profile</b>，选择 <b>New</b>。\n - 添加阴影覆盖项：\n \n  - 进入 Add <b>Override > Shadowing > Shadows</b>。\n  - 启用 Max Distance。\n  - 把 Max Distance 设为 50。\n\n4. 配置代表太阳的灯光 GameObject。\n \n - 在场景中选中代表太阳的 Light GameObject，在 Inspector 中查看。\n - 进入 <b>Emmision</b>，把 Intensity 设为 100000。\n - 把 <b>Light Appearance</b> 设为 <b>Color</b>。\n - 把 <b>Color</b> 设为白色。\n - 若要在天空中看到太阳，进入 <b>Shape</b>，把 <b>Angular Diameter</b> 设为 3。\n\n5. 此时场景会过曝，修正方式如下：\n \n - 选中第 3 步创建的 <b>Global Settings</b> GameObject。\n - 为其 Volume 组件添加 <b>Exposure</b> 覆盖项，菜单为 Add <b>Override > Exposure</b>。\n - 启用 <b>Mode</b> 并设为 <b>Automatic</b>。\n - 若需刷新曝光，进入 Scene 视图并启用 <b>Always Refresh</b>。");
            DrawServerImage("img-50.png");
        }
        else if (subStep == 4)
        {
            DrawText("最后还需要手动为所有 MFPS 玩家预制体配置相机堆叠：\n \n1. 在预制体编辑器中打开玩家预制体，或把其拖入场景层级。\n \n2. 在玩家预制体层级中，选中位于 <b>Local > Mouse > Animations > Main Camera > WeaponCamera</b> 的 <b>WeaponCamera</b>。\n \n3. 在 <b>Camera</b> 组件的 Inspector 窗口中展开 <b>Output</b> 选项卡，把 <b>Depth</b> 设为 2。\n \n应用并保存玩家预制体的改动，对游戏中其余所有玩家预制体重复此流程。全部完成后，项目向 HDRP 的基础转换就完成了。");
            DrawServerImage("img-51.png");
        }
    }

    void KillFeedDoc()
    {
        if (subStep == 0)
        {
            DrawTitleText("KILL FEED");
            DrawText("<i>击杀提示</i> 又称 <i>谁击杀了谁</i>，是显示对局中玩家击杀及其他淘汰事件的 UI 文本面板，向玩家展示谁被淘汰、又是谁淘汰了他。\n \n该面板通常放在屏幕角落，既不影响操作又便于查看。\n  \n下面介绍在不改动代码的前提下自定义这套系统的一些选项，以及如何显示你自己的事件。");
            DownArrow();
            DrawText("• MFPS 提供两种在击杀提示中显示击杀事件的模式。一条击杀事件包含三部分信息：被淘汰的玩家名、完成淘汰的玩家名、以及淘汰所用的武器或原因。\n \n淘汰原因有两种显示方式：\n \n<b>武器名称：</b>");
            DrawServerImage(0);
            DrawText("<b>Weapon Icon:</b>");
            DrawServerImage(1);
            DrawText("默认选项为 <b>武器图标</b>，可在 <b>GameData</b> -> KillFeedWeaponShowMode 中修改。\n \n另一个可自定义项是本地玩家名出现在击杀提示中时 " +
                "用于高亮的颜色。击杀提示中的玩家名一般以所属阵营的颜色表示，但为了让本地玩家一眼看出哪条事件与自己有关，他的名字 " +
                "应当用另一种颜色高亮，<b>选择该颜色</b>的路径为 GameData -> <b>HighLightColor</b>。\n \n以上是前端可自定义的选项。如果你想自定义 UI 的外观" +
                "，需要在 UI 预制体中操作，位置为 <i>Assets -> Prefabs -> UI -> Instances -> <b>KillFeed</b></i>。把这个预制体拖入 Canvas 中的击杀提示面板，该面板默认位于：");
            DrawServerImage(2);
            DrawText("以上就是前端可用的全部自定义选项。如果你想创建自己的显示事件，见下一步。");
        }
        else if (subStep == 1)
        {
            DrawTitleText("创建击杀提示事件");
            DrawText("击杀提示系统支持多种事件类型，按你的需求选用：\n \n<b>击杀事件：</b>\n \n• 用于发生了涉及两名角色的击杀时 " +
                "即击杀者与被击杀者，调用方式如下：");
            DrawCodeText("bl_KillFeed.Instance.SendKillMessageEvent(string killer, string killed, int gunID, Team killerTeam, bool byHeadshot);");
            DrawText("<b>消息：</b>\n \n• 若要显示一条不涉及特定玩家的简单文本事件，使用：");
            DrawCodeText("bl_KillFeed.Instance.SendMessageEvent(string message);");
            DrawText("<b>阵营高亮：</b>\n \n• 若要显示一条主体为特定阵营的文本事件，并用阵营颜色高亮其中一部分文本，使用：");
            DrawCodeText("bl_KillFeed.Instance.SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam);");
        }
    }

    void PlayerClassesDoc()
    {
        DrawText("MFPS 使用 <b>兵种</b> 系统来丰富武器配置，兵种包括 <b>侦察兵、支援兵、突击兵和工程兵</b>，各自配备不同武器。\n\n每个玩家预制体各兵种的武器配置，可在玩家预制体内 <b>WeaponsManager</b> 对象上挂载的 <b>bl_GunManager</b> 脚本中设置。\n\n每个兵种需要 4 件武器 <i>主武器、副武器、技能和投掷物</i>。设置各兵种默认武器有两种方式：新建一个 Present 即 ScriptableObject，或直接编辑默认的那一个。只有在你需要保留当前兵种配置的备份，或想让某个玩家预制体使用不同配置时，才需要新建 ScriptableObject，否则直接编辑默认实例即可。\n\n无论新建还是编辑，都先打开玩家预制体，或你要修改兵种配置的那个玩家预制体，然后进入 <b>WeaponsManager</b> 对象 -> <b>bl_GunManager</b> 检视面板 -> 展开目标兵种 -> 为每个槽位设置武器。\n\n若想在编辑前新建一个 Present，直接点击 <b>New</b> 按钮");
        DrawServerImage(8);
        DrawText("你也可以在 Project 窗口的 <i>Assets->MFPS->Content->Prefabs->Weapons->Loadouts</i> 文件夹中编辑默认兵种武器配置。\n");
    }

    void HeadBobDoc()
    {
        DrawText("<b>头部晃动</b> 是模拟玩家行走或奔跑时头部反应的相机运动。MFPS 中该运动由代码程序化生成，你可以调整数值得到想要的效果。\n\n为获得更真实的表现，MFPS 把武器晃动与头部晃动做了同步，因此设置会同时作用于两者。\n\n数值可在 bl_WeaponBob.cs 中修改<i>，该脚本挂载在玩家预制体内的 WeaponsManager 对象上</i>。你可以在运行时编辑并即时预览效果。\n");
        DrawServerImage(9);
        DownArrow();
        DrawText("若想让不同玩家使用不同运动参数，或只是想备份当前运动设置，可以新建一份设置的 Present 并在脚本中替换原有的那份。\n\n新建 Present 的方式：在 <i>Project 视图</i> 中选中目标文件夹 -> 右键 -> MFPS -> Weapons -> Bob -> Settings -> 把生成的配置拖到 bl_WeaponBob 的 Settings 中 -> 然后按需修改。\n");
    }

    void AfkDoc()
    {
        DrawTitleText("AFK");
        DrawText("AFK 是 <i>away from keyboard</i> 的缩写，指玩家长时间未与游戏交互的状态。在多人游戏中，AFK 玩家会带来问题，" +
            "例如 MFPS 采用阵营对抗，AFK 玩家相当于白送对方分数；在另一些场景中，AFK 玩家被用来刷等级。因此不少游戏都会配备一套 " +
            "检测 AFK 玩家并在其持续 AFK 超过一定时间后移出服务器或房间的机制。MFPS 内置了该系统，但 <b>默认关闭</b>。\n \n" +
            "启用 AFK 检测：进入 GameData -> 打开 <b>Detect AFK</b> -> 在 <b>AFK Time Limit</b> 中设置判定为 AFK 后多少秒将玩家移出。");
    }

    void KickVotationDoc()
    {
        DrawTitleText("KICK VOTATION");
        DrawText("为了让玩家能以民主方式处理捣乱、作弊、违规的玩家，由一名玩家发起提议，房间内多数玩家 " +
            "表决是否将其移出或驳回提议，MFPS 内置了投票系统。\n \n在游戏中发起投票：玩家打开菜单 -> 在记分板上点击或触摸要表决的玩家 -> " +
            "在弹出的菜单中选择 <b>Request Kick</b> 按钮。\n \n默认投票按键为 F1 表示同意、F2 表示反对，可在 <b>GameManager</b> 上挂载的 bl_KickVotation.cs 中修改 " +
            "该脚本位于地图场景中。");
        DownArrow();
        DrawText("若想自行实现发起投票的方式，可调用：");
        DrawCodeText("bl_KickVotation.Instance.RequestKick(Photon.Realtime.Player playerToKick);");
    }

    private AssetStoreAffiliate soundsAssets;
    void AudioDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b><size=22>背景音乐</size></b>\n \nMFPS 默认只在 <b>大厅与主菜单</b> 场景使用背景音乐。要更换或移除该音乐：\n \n■ 打开 <b>MainMenu</b> 场景 -> Lobby -> Scene -> AudioController -> 在检视面板中找到 bl_AudioController -> 在 <b>Background Clip</b> 字段中指定或移除音频片段");
            Space(20);
            DrawText("<b><size=22>子弹命中</size></b>\n \n子弹命中粒子生成时会播放一个命中音效。出于设计考虑该音量非常低，因为音量过大会让部分玩家感到不适。要更换这些命中音效或调整音量，<b>需要打开子弹命中粒子预制体</b>，其默认位置为 <i>Assets -> Prefabs -> Level -> Particles -> WeaponEffects -> Prefabs->*</i>。\n \n打开要修改的命中效果预制体 -> 在其上的 <b>Audio Source</b> 组件中指定或替换音频片段，并按需调整音量。");
            Space(20);
            DrawText("<b><size=22>玩家受击</size></b>\n \n另一个会播放的受击音效是本地玩家被击中时。这里区分两种声音：被子弹击中，以及其他类型的受伤。\n \n这些音效可按玩家预制体分别设置。选中要修改的 <b>玩家预制体</b> -> bl_PlayerHealthManager -> 在以下列表中设置：\n \n<b>Hits Sounds：</b>子弹命中音效\n<b>Injure Sounds：</b>受伤音效");
        }
        else if (subStep == 1)
        {
            DrawText("你可能会遇到这种情况：在自定义地图中，远处玩家或机器人的枪声、脚步、爆炸等听起来像就在身边。原因是默认音频范围不适合你的地图尺寸，这个问题很容易调整。");
            DrawText("<b><size=16>调整音频范围</size></b>\n \n在特定地图中调整音频范围：在编辑器中打开地图场景 -> 在层级窗口进入 <b>GameManager</b> > <b>Audio Manager</b> -> 在该对象的检视面板中找到 <b>bl_AudioController</b> -> 你会看到若干滑块参数，用于调整特定类型声音的范围。范围以米为单位，设为 50 表示玩家距离音源 50 米以内才能听到。\n \n音量会随距离从声源处向最大距离逐渐衰减。");
            DrawServerImage("img-47.png");
        }
        else if (subStep == 2)
        {
            DrawText("如果你想寻找音效来替换游戏内的默认音效，或为武器、玩家补充新的音效，下面是一份人工筛选的资源合集，可在资源商店获取");
            Space(20);
            if (soundsAssets == null)
            {
                soundsAssets = new AssetStoreAffiliate();
                soundsAssets.randomize = true;
                soundsAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555517340/widget-medium");
                soundsAssets.FixedHeight = 400;
            }
            else
            {
                soundsAssets.OnGUI();
            }
        }
    }

    void AntiCheatDoc()
    {
        DrawSuperText("作弊是游戏开发者迟早要面对的问题，在竞技类多人游戏中尤为突出。你不得不应对那些试图利用漏洞获取优势的修改者和作弊者。因此从项目一开始就要采取防护措施。\n \nMFPS 默认不包含任何反作弊系统，因为这类系统体量大、复杂度高，其价值本身就超过 MFPS 核心的价格。但从 1.9.2 版本起，<b>MFPS 内置了第三方反作弊资源的基础集成</b>，可解决当下大多数常见作弊手段，该资源为资源商店的\n<?link=https://assetstore.unity.com/packages/tools/utilities/anti-cheat-toolkit-2021-202695?aid=1101lJFi>Anti-Cheat Toolkit (ACTk)</link>\n借助它可以阻止玩家修改生命值、弹药、掉落物、金币等数据。\n \nMFPS 核心只包含能阻止玩家修改这些数值的基础集成，另有一个扩展功能的插件 <b>可自动封禁使用加速、内存注入、代码注入或穿墙类作弊的玩家</b>，即 <?link=https://www.lovattostudio.com/en/shop/addons/anti-cheat-and-reporting/>MFPS Anti Cheat 插件</link>。\n \n这能劝退大多数业余作弊者，但对经验丰富的修改者仍不够。事实上没有绝对无法破解的程序，你只能不断提高门槛让对方放弃。基于这一点，以下还有若干实现与调整可以让你的游戏在各平台上更安全：");
        Space(20);
        DrawSuperText("<b><size=14>1. 使用 IL2CPP</size></b>\n \nUnity 目前支持两种脚本后端：<b>Mono</b> 与 <b>IL2CPP</b>，后者更新也更安全。IL2CPP 生成的是带元数据的原生二进制代码，而非 Mono 的 IL 字节码，所有 IL 反编译工具都会失效，想得到可读性好的游戏代码反编译结果会难上加难。\n \n该功能 Unity 内置，只需在 <b>Project Settings > Player > Scripting Backend</b> 中切换脚本后端。\n \n\n<b><size=14>2. 代码混淆</size></b>\n \n代码混淆是在构建过程中为脚本参数、函数、属性等随机生成名称，使反编译出的二进制代码几乎无法阅读。代码会变成一堆无意义名称的组合，重建出的 IL 程序集极难逆向分析。\n \n遗憾的是 Unity 并未内置该功能，需要借助第三方方案。Unity 生态中最流行且易用的混淆器可在资源商店获取：\n<?link=https://assetstore.unity.com/packages/tools/utilities/obfuscator-48919?aid=1101lJFi>Obfuscator</link>");
        DrawHorizontalSeparator();
        DrawText("<b><size=16>启用反作弊</size></b>\n \n启用反作弊集成的方式：若使用插件进行进阶集成，请参照插件文档；若未使用插件但已安装 Anti-Cheat Toolkit 资源，想启用核心包中的基础集成，则进入编辑器顶部菜单 Tools > Code Stage > Anti-Cheat Toolkit > Settings... > Conditional Compilation Symbols，勾选 <b>ACTK_IS_HERE</b> 即可。");
    }

    void FPArmsMaterial()
    {
        DrawText("通常所有武器模型共用同一个手部模型，只是为不同阵营使用不同材质与贴图。若要在玩家预制体中逐把武器修改手部贴图会很繁琐，MFPS 已经处理了这件事。\n无需手动逐个修改手臂、袖套、手套等材质，你只需" +
            "创建一个配置资源，列出全部手臂材质以及各阵营对应的不同贴图即可。");
        DownArrow();
        DrawText("首先新建一个手臂材质资源：在 <b>Project 窗口</b>中选中要保存该资源的文件夹，<b>右键</b> -> MFPS -> Player -> <b>Arm Material</b>");
        DrawServerImage(4);
        DownArrow();
        DrawText("然后选中新建的资源，在检视面板中会看到一个列表。需要把手臂模型中 <b>随玩家阵营变化贴图</b> 的材质全部加入该列表。以 MFPS 默认手臂模型为例，它有 3 个材质：袖套、皮肤和手套，但只有袖套和手套的贴图会变化，皮肤保持不变，因此列表中只包含这两个材质。\n多数情况下你只需添加手套材质，即新增一个列表项，指定材质，并按阵营添加不同贴图。\n \n设置完成后，材质会在运行时根据玩家出生所属阵营自动切换贴图。");
        DrawServerImage(5);
    }

    void RoomPropertiesDoc()
    {
        DrawHyperlinkText("除游戏模式之外，还有一些逐房间即逐对局可调的属性，例如最大玩家数选项、单局时间上限、游戏目标等。这些属性的可选值可随游戏模式不同而不同，修改方式如下：\n\n► 打开 <link=asset:Assets/Resources/GameData.asset>GameData</link> ➔ Game Modes ➔ <i>展开某个游戏模式</i> ➔ 在这里可以看到相关列表与选项。");

        DrawHorizontalColumn("Max Players", "该游戏模式下房间可容纳的最大玩家数可选值。在双阵营模式下，每队最大人数为总上限的一半。");
        DrawHorizontalColumn("游戏目标选项：", "该游戏模式下分数、积分数或击杀数的目标可选值。");
        DrawHorizontalColumn("Time Limits:", "该游戏模式的单局时间上限可选值，单位为秒。");
        DrawServerImage("img-45.png");
    }

    void DrawTeamsDoc()
    {
        DrawText("MFPS 中有多个使用阵营系统的游戏模式，例如 CTF 夺旗与 TDM 团队死斗。默认阵营名为 Delta 与 Recon，你可以修改这两个名称及其代表色：进入 <b>Game Data</b>，找到 Team 区域：\n");
        DrawServerImage(13);
    }

    void DrawCoins()
    {
        if (subStep == 0)
        {
            DrawHyperlinkText("MFPS 内置虚拟货币系统，包含两种货币：一种通过游戏获得 <i>随经验值</i>，另一种只能用真实货币购买 <i>可通过 <link=https://www.lovattostudio.com/en/shop/network/shop/>商店插件</link> 或你自建的内购实现</i>。\n \n无需改动代码即可自定义货币的大量属性，例如 <b>货币名称、颜色、图标和面值</b>。\n \n修改这些属性请进入 <link=asset:Assets/Resources/GameData.asset>GameData</link> ➔ Game Coins ➔ 展开要修改的货币。多数属性名已能自解释，以下说明容易混淆的几个：");
            DrawHorizontalSeparator();
            DrawHorizontalColumn("Acronym", "货币名称的缩写。");
            DrawHorizontalColumn("Coin Value", "货币相对于 1 的面值。举例说明：假设某武器的游戏内标价为 100，若该货币面值为 0.25，则玩家需要 400 枚该货币才能购买 <i>0.25 = 1/4，故 400/4 = 100</i>；若面值为 2，则只需 50 枚。\n \n这样设计的目的是：你只需为游戏内物品设置一个标价，各货币对应的实际数量由游戏自动换算。");
            DrawHorizontalColumn("Initial Coins", "新玩家首次进入游戏时获得的该货币数量。");
            Space(10);
            DrawServerImage("img-38.png", TextAlignment.Center);
            DownArrow();
            DrawSuperText("<?title=20>如何使用这些货币？</title>\n \n在核心包中，MFPS 默认不将货币用于任何用途，怎么用由你决定：可用于购买武器、特殊皮肤、干员等游戏内物品，也可实现开箱系统，或任何你能想到的玩法。\n \n若想实现商店系统、让玩家购买武器与货币包，有相应插件：\n \n<?link=https://www.lovattostudio.com/en/shop/network/shop/>商店系统插件</link>\n \n若要接入支付系统以支持真实货币购买，也有对应插件：\n \n<?link=https://www.lovattostudio.com/en/shop/addons/unity-iap-for-shop/>Unity IAP 插件</link>\n<?link=https://www.lovattostudio.com/en/shop/network/paypal-for-shop/>Paypal 插件</link>");
        }
        else if (subStep == 1)
        {
            DrawSuperText("若要对玩家钱包执行 <b>增加</b> 或 <b>扣减</b> 指定数量货币这类基础操作，一行代码即可完成：\n \n<?title=#20>增加货币</title>");
            DrawCodeText("bl_MFPS.Coins.GetCoinData(0).Add(100);");
            DrawSuperText("其中 0 表示该货币在 GameData ➔ GameCoins 列表中的索引。\n默认为 0 是经验货币，1 是金币。\n \n<?title=20>扣减货币</title>");
            DrawCodeText("bl_MFPS.Coins.GetCoinData(0).Deduct(100);");
            DrawText("若使用 ULogin Pro 插件，货币操作会在服务端执行并存入库中。若未使用，货币数据会通过 <b>PlayerPrefs 存储在本地，并不安全</b>。因此建议像 ULogin Pro 那样把货币数据保存到独立数据库中。");
        }
    }

    void GameModesDoc()
    {
        if (subStep == 0)
        {
            DrawText("MFPS 提供 3 种游戏模式：<b>团队死斗、夺旗和自由混战</b>，各自逻辑位于独立脚本中，路径为 <i>Assets ➔ MFPS ➔ Scripts ➔ GamePlay ➔ GameModes➔*</i>。\n \n各模式都有一些通用属性可在 GameData ➔ Game Modes ➔ * 的检视面板中调整，展开任一模式即可看到全部可配置项。");
            DrawServerImage("img-36.png");
            DrawText("各项含义如下：");
            DrawPropertieInfo("Mode Name", "string", "该模式的名称，用于游戏内显示。");
            DrawPropertieInfo("Game Mode", "enum", "该模式的内部标识，用于在代码中识别特定模式。Game Modes 列表中不允许存在标识相同的模式。");
            DrawPropertieInfo("Is Enable", "bool", "该模式是否在游戏中可用。");
            DrawPropertieInfo("Support Bots", "bool", "是否可创建该模式的房间并加入机器人？<b>注意：</b>默认情况下仅 TDM 与 FFA 模式支持机器人。");
            DrawPropertieInfo("自动分配阵营", "bool", "是否强制自动分配阵营以保持双方人数均衡？");
            DrawPropertieInfo("开始所需玩家数", "int", "该模式下开始游戏所需的最小加入人数。");
            DrawPropertieInfo("回合中途加入时的出生方式", "enum", "在该模式的房间中，回合已开始后玩家加入时如何处理。");
            DrawPropertieInfo("玩家死亡时", "enum", "该模式下玩家死亡后发生什么。");
            DrawPropertieInfo("Goal Name", "string", "该模式中计分目标的名称，例如击杀、夺取、得分等。");
            DrawPropertieInfo("允许拾取武器", "bool", "该模式下玩家能否拾取武器。");
            DrawPropertieInfo("Max Players", "int[]", "该模式可选的最大玩家数，这些选项会出现在大厅的创建房间菜单中。");
            DrawPropertieInfo("游戏目标选项", "int[]", "该模式目标的可选值，如击杀数、夺取数、分数等，这些选项会出现在大厅的创建房间菜单中。");
            DrawPropertieInfo("Time Limits", "int[]", "该模式可选的对局时长，单位为秒，这些选项会出现在大厅的创建房间菜单中。");
        }
        else if (subStep == 1)
        {
            DrawHyperlinkText("除 MFPS 内置的 3 种模式外，还有更多热门模式以插件形式提供，可另行购买，例如 <i><b>爆破拆除、占点模式、枪战竞速、淘汰模式和确认击杀</b></i>，均可在 <link=https://www.lovattostudio.com/en/shop/>官方商店</link> 获取。\n \n假设你想做一个自定义游戏模式，该从哪里入手？\n这个问题不好一概而论，因为每个模式的需求、逻辑和玩法都不同，做法自然各异。因此我无法直接教你如何从零设计模式，但可以告诉你怎么把它接入 MFPS。\n \n第一步是为你的游戏模式创建枚举标识，用于在代码中识别该模式。只需在 <b>GameMode.cs</b> 脚本中按模式名添加一个代称，例如：");
            DrawCodeText("public enum GameMode\n{\n    TDM,\n    FFA,\n    CTF,\n    SND,\n    CP,\n    GR,\n    BR,\n    ELIM,\n    DM,\n    KC,\n    <color=#0D5400FF>MyCustomMode, </color>\n}");
            DrawText("上例中的 <i>MyCustomMode</i> 只是示意，你可以用任何名称，也可以像其他模式那样只取首字母缩写。");
            DownArrow();
            DrawHyperlinkText("接下来要编写负责把 MFPS 与你的游戏模式对接的脚本。MFPS 通过 <color=#DFFF2AFF>IGameMode</color> 接口规定模式主脚本必须实现哪些函数。因此你需要在模式主脚本中 <link=https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces>实现该接口</link>，并重写其中要求的函数。为便于上手，可参考默认模式脚本 <i>bl_TeamDeathMatch.cs、bl_FreeForAll.cs 等</i> 的写法。\n \n实现 <b>IGameMode</b> 接口及其函数之后，还有一段必须编写的代码：在脚本 <i>即实现接口的那个</i> 的 Awake() 中调用 Initialize()，并在 Initialize() 中用枚举标识注册该模式，如下所示：\n");
            DrawCodeText("if(bl_GameManager.Instance.IsGameMode(GameMode.MyCustomMode, this))\n        {\n            // 启用你的游戏模式\n            // 启用该模式专属的所有 GameObject、道具、UI 等\n            // 订阅该模式需要的游戏事件\n        }\n        else\n        {\n            // 禁用该模式专属的对象与 UI\n        }");
            DownArrow();
            DrawText("下面是一份完整脚本，包含上述内容的默认骨架。你可以以此为基础，按你的模式需求进行改造：");
            DrawCodeText("using Photon.Realtime;\nusing UnityEngine;\n \npublic class MyCustomModeScript : MonoBehaviour, IGameMode\n{\n \n    void Awake()\n    {\n        if (!bl_PhotonNetwork.IsConnected) return;\n \n        // 必须调用\n        Initialize();\n    }\n \n    #region Interface Overrides\n    public void Initialize()\n    {\n        if(bl_GameManager.Instance.IsGameMode(GameMode.MyCustomMode, this)) // 把 MyCustomMode 替换为你的模式标识\n        {\n            // 启用你的游戏模式\n            // 启用该模式专属的所有 GameObject、道具、UI 等\n            // 订阅该模式需要的游戏事件\n        }\n        else\n        {\n            // 禁用该模式专属的对象与 UI\n        }\n    }\n \n    // 依据你模式的逻辑判断本地玩家是否获胜\n    // 该代码在对局结束后调用。\n    public bool isLocalPlayerWinner => throw new System.NotImplementedException();\n \n    public void OnFinishTime(bool gameOver)\n    {\n        // 对局时间结束时自动调用。\n    }\n \n    public void OnLocalPlayerDeath()\n    {\n        // 本地玩家死亡时自动调用\n    }\n \n    public void OnLocalPlayerKill()\n    {\n        // 本地玩家击杀敌人时自动调用\n    }\n \n    public void OnLocalPoint(int points, Team teamToAddPoint)\n    {\n        // 当你通过以下方式为阵营加分时调用：\n        // bl_GameManager.Instance.SetPointFromLocalPlayer(1, GameMode.MyCustomMode);\n    }\n \n    public void OnOtherPlayerEnter(Player newPlayer)\n    {\n \n    }\n \n    public void OnOtherPlayerLeave(Player otherPlayer)\n    {\n \n    }\n \n    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)\n    {\n        // 房间属性变化时自动调用，如分数、目标、阵营人数等\n    }\n    #endregion\n}");
        }
        else if (subStep == 2)
        {
            DrawText("若你想指定某些地图允许或禁止哪些游戏模式，可在各地图信息中的「禁止的游戏模式」列表里设置。位置为 <i>GameData ➔ All Scenes ➔ 某场景信息 ➔ <b>No Allowed Game Modes</b></i>。\n \n加入该列表的模式在该地图上不可用，未加入的则全部可用。");
            DrawServerImage("img-37.png");
        }
    }

    void DrawLobbyChat()
    {
        DrawText("Game Framework 内置 <b>大厅聊天系统</b>，玩家在搜索或等待加入对局期间可以互相交流。该聊天使用 Photon Chat 插件，使用时需要拥有 Photon Chat 的 AppID <i>它与 Photon PUN 的 AppID 不同</i>。AppID 可在 Photon 控制台获取：\n\n在 Chat 控制台获取你的 AppID：");
        if (Buttons.FlowButton("Chat Dashboard"))
        {
            Application.OpenURL("https://www.photonengine.com/en-US/Chat");
        }
        DrawText("拿到 Chat AppID 后，粘贴到 PhotonServerSettings 中：");
        DrawServerImage(6);
    }

    public AnimatorController playerAnimatorController;
    public int customWeaponStep = 0;
    public string[] weaponNames;
    public int customWeaponID, selectedSubMachine = 0;
    public ChildAnimatorStateMachine[] upperSubMachineStates;
    public string[] upperSubMachineStatesNames;
    public string customAnimationFireName;
    private AssetStoreAffiliate playerAnimationAssets;
    void DrawPlayerAnimationDoc()
    {
        if (subStep == 0)
        {
            DrawSuperText("Game Framework 使用 Mecanim 系统处理第三人称玩家动画。更换动画片段只需把动画片段拖到 Animator 窗口对应的运动状态上即可，你只需要一个 humanoid 动画片段，并在 Animator Override Controller 中覆盖它。\n\n\n<?title=16>更换玩家动画</title>\n\n更换指定动画片段的步骤：\n\n- 打开要修改动画的玩家预制体 <i>玩家预制体位于 Resources 文件夹中</i>。\n\n- 在玩家预制体士兵模型的 Animator 组件上，<b>双击</b> Controller 字段：");
            DrawServerImage("img-27.png");
            DrawText("- 此时在 <b>Inspector</b> 窗口中会看到玩家动画控制器使用的动画片段列表。每个默认动画片段名旁边都有一个字段，用于指定自定义动画片段来覆盖或替换默认动画。\n \n你只需在对应框中指定自定义动画，<b>根据默认动画名称即可判断每个动画对应哪种玩家动作</b>。");
            DrawServerImage("img-28.png");
            DownArrow();
            DrawSuperText("如果你想让某个玩家预制体使用不同的动画，<b>例如</b> <i>每个玩家预制体有各自的士兵模型，并配有该模型专属的动作动画</i>，处理方式很简单。\n\n只需新建或复制一份 <?underline=>Player Animations [Override]</underline> 控制器，并把它指定到玩家预制体中士兵模型的 Animator 组件上：");
            DrawAnimatedImage(6);
        }
        else if (subStep == 1)
        {
            DrawSuperText("<?title=18>高级动画更换</title>\n\n- 基础部分介绍的 <?underline=>Animator Override Control</underline> 更换动画存在限制：它只能替换动画片段，而默认情况下有些动画被动画状态机的多个状态共用。例如 <i>装弹动画 reload-ar 同时用于步枪、手枪和狙击枪</i>。若想让每类武器使用不同动画，就必须修改基础 <?underline=>Animator Controller</underline>。\n\n1. 复制默认的玩家动画控制器资源 <?link=asset:Assets/Art/Animations/Player/Controllers/Player [Controller].controller>Player [Controller]</link> ➔ 在 Project 窗口中选中该资源 ➔ 按 Ctrl + D，Mac 上为 Command + D。\n\n2. 把复制出的 <b>Animator Controller</b> 指定到你要修改动画的玩家预制体士兵模型的 Animator 组件上。\n\n3. 在 Animator 窗口中打开复制出的动画控制器 <i><size=9><color=#76767694>双击该动画控制器即可</color></size></i> ➔ 找到要更换动画片段的状态。在 Animator 视图中先判断该动画属于身体下半身还是上半身 <i>腿部或手臂</i>，例如 <b>步枪装弹</b> 动作属于手臂即上半身，因此进入 <b>Layers</b> 并选择 <?underline=>Upper</underline> 层，这里会看到以武器类型命名的若干状态机 ➔ 打开对应武器的状态机：");
            DrawServerImage("img-29.png");
            DrawSuperText("此时会看到其他动画状态，它们代表武器动作片段。本例中要找的是 <i>装弹</i> 状态，选中该状态 ➔ 在检视视图中会显示该状态的设置，其中我们关心的是 <?underline=>Motion</underline> 字段，在该字段中指定用来替换默认动画的片段。\n\n完成后即可，其他动画片段也可按同样方式替换。");
            DrawServerImage(16);
        }
        else if (subStep == 2)
        {
            DrawText("MFPS 中第一人称与第三人称动画是不同的，武器动画同样如此。第三人称武器不能使用第一人称武器的动画，它需要 humanoid 动画。");
            DrawNote("第三人称武器动画集即一把武器所需的玩家动画，包括待机、装弹、奔跑和开火。");
            DrawText("默认情况下，MFPS 按武器类型 <i>机枪、手枪、狙击枪、手雷等</i> 区分武器动画，同一类型的所有武器共用一套动画。也就是说若你有多把 <b>狙击枪</b>，它们都会播放同一套狙击枪第三人称动画。\n \n如果你想让 <b>某把特定的第三人称武器使用自定义动画集</b>，就需要在玩家 <b>Animator Controller</b> 中新建一个 <b>子状态机</b> 并建立必要的过渡。下面提供一份操作指引，说明如何通过子状态机为第三人称武器应用自定义动画。");
            DrawHorizontalSeparator();
            DrawTitleText("添加自定义武器动画");
            DrawSuperText("首先需要确定要把子状态机加入到哪个 <b>Animator Controller</b>。MFPS 所有玩家预制体默认使用的动画控制器位于 <i>Assets ➔ Art ➔ Animations ➔ Player ➔ Controllers ➔ Player [Controller]</i> <?link=asset:Assets/Art/Animations/Player/Controllers/Player [Controller].controller>点击此处定位</link>。\n \n如果你没有修改过任何玩家预制体的动画控制器，把上方的默认动画控制器拖入下方字段即可；否则拖入你实际使用的那个。");
            Space(10);
            var lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.BeginHorizontal("box");
            Space(20);
            playerAnimatorController = EditorGUILayout.ObjectField("玩家动画控制器", playerAnimatorController, typeof(AnimatorController), true) as AnimatorController;
            if (customWeaponStep == 0)
            {
                GUI.enabled = playerAnimatorController != null;
                if (Buttons.FlowButton("Continue"))
                {
                    customWeaponStep++;
                }
                GUI.enabled = true;
            }
            Space(20);
            EditorGUILayout.EndHorizontal();

            if (customWeaponStep > 0 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("在下方下拉框中选择要添加自定义动画的 <b>武器信息</b>，然后点击 <b>Select</b> 按钮。");
                if (weaponNames == null)
                {
                    weaponNames = bl_GameData.Instance.AllWeaponStringList();
                }
                EditorGUILayout.BeginHorizontal("box");
                Space(20);
                customWeaponID = EditorGUILayout.Popup("Weapon", customWeaponID, weaponNames);
                if (Buttons.FlowButton("Select"))
                {
                    customWeaponStep++;
                }
                Space(20);
                EditorGUILayout.EndHorizontal();
            }

            if (customWeaponStep > 1 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("接下来需要手动操作：\n首先在 <b>Animator 窗口</b>中打开该 <b>Animator Controller</b>，方式是在 <b>Project 视图</b> 中选中该控制器 <i>即上面字段中指定的那个</i> ➔ 双击它，或右键 > <b>Open</b>。\n \n在打开的 Animator 窗口中，点击左上角的 <b>Layers</b> 标签 ➔ 选择第二个层 <b>Upper</b> ➔");
                DrawServerImage("img-43.png");
                Space(10);
                DrawSuperText("<?background=#CCCCCCFF>复制其中一个子状态机</background>\n \n复制一个与你要添加动画的武器同类型的子状态机，<b>例如</b> 自定义动画是给霰弹枪用的，就复制霰弹枪的子状态机。\n \n复制方式：选中该子状态机 > 右键 > Copy > 在空白处右键 > Paste：");
                DrawAnimatedImage(7);
                DownArrow();
                DrawText("复制完成后，点击下方按钮以获取动画控制器数据并继续配置");
                if (GUILayout.Button("获取动画控制器数据"))
                {
                    var rootStateMachine = playerAnimatorController.layers[1].stateMachine;
                    upperSubMachineStates = rootStateMachine.stateMachines;
                    upperSubMachineStatesNames = upperSubMachineStates.Select(x => x.stateMachine.name).ToArray();
                    customWeaponStep++;
                }
            }

            if (customWeaponStep > 2 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("在下方下拉框中选择你刚复制出的 <b>子状态机</b>，然后点击 <b>Setup</b> 按钮。");
                GUI.enabled = customWeaponStep == 3;
                EditorGUILayout.BeginHorizontal("box");
                Space(20);
                selectedSubMachine = EditorGUILayout.Popup("SubMachineState", selectedSubMachine, upperSubMachineStatesNames);
                if (Buttons.FlowButton("SetUp"))
                {
                    if (SetupWeaponSubMachine(upperSubMachineStates[selectedSubMachine].stateMachine, playerAnimatorController))
                    {
                        customWeaponStep++;
                    }
                }
                Space(20);
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            if (customWeaponStep > 3 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("此时子状态机已可使用。下一步是指定玩家武器动画 ➔ 在 Animator 窗口中选择以目标武器命名的 <b>子状态机</b> <i><b>你刚复制的子状态机名称已自动改为你选中的武器名</b></i> ➔ 双击该子状态机 ➔ 会看到若干动画状态 <b>奔跑、待机、装弹和开火</b> ➔ 要替换某个状态的动画片段，选中该状态 ➔ 转到检视面板 ➔ 在 <b>Motion</b> 字段中指定动画片段。");
                DrawAnimatedImage(8);
                DownArrow();

                DrawText("动画指定完成后基本就绪，最后一步是在 <b>TPWeapon > bl_NetworkGun</b> 检视面板中填写自定义动画信息。打开一个玩家预制体 <i>你使用的每个玩家预制体都要做一次</i> ➔ 选中预制体内的第三人称武器 ➔ 在检视面板勾选 <b>Use Custom Player Animations</b> ➔ 填写以下字段：");
                Space(10);
                if (string.IsNullOrEmpty(customAnimationFireName))
                {
                    var weaponInfo = bl_GameData.Instance.GetWeapon(customWeaponID);
                    customAnimationFireName = weaponInfo.Name;
                }
                DrawHorizontalColumn("自定义动画状态 ID", (20 + customWeaponID).ToString(), 175);
                DrawHorizontalColumn("自定义开火动画名称", customAnimationFireName + "Fire", 175);
                DrawServerImage("img-44.png");
                DrawText("That's");
            }

            EditorGUIUtility.labelWidth = lw;
        }
        else if (subStep == 3)
        {
            DrawText("如果你想寻找玩家动画来替换游戏内的默认动画，下面是一份人工筛选的资源合集，可在资源商店获取");
            Space(20);
            if (playerAnimationAssets == null)
            {
                playerAnimationAssets = new AssetStoreAffiliate();
                playerAnimationAssets.randomize = true;
                playerAnimationAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555464133/widget-medium");
                playerAnimationAssets.FixedHeight = 400;
            }
            else
            {
                playerAnimationAssets.OnGUI();
            }
        }
    }

    private bool SetupWeaponSubMachine(AnimatorStateMachine stateMachine, AnimatorController animatorController)
    {
        if (stateMachine == null)
        {
            Debug.LogWarning($"在该动画控制器中找不到该状态机。");
            return false;
        }

        var weaponInfo = bl_GameData.Instance.GetWeapon(customWeaponID);
        int upperID = 20 + customWeaponID;
        var rootStateMachine = animatorController.layers[1].stateMachine;
        var equipState = rootStateMachine.states.ToList().Find(x => x.state.name == "Equip").state;

        if (equipState == null)
        {
            Debug.LogWarning($"在动画控制器中找不到装备动画状态。");
            return false;
        }

        stateMachine.name = weaponInfo.Name;

        var subMachineStates = stateMachine.states.ToList();
        var idleState = subMachineStates.Find(x => x.state.name == "Idle");

        if (idleState.state == null)
        {
            Debug.LogWarning($"在动画控制器中找不到待机动画状态。");
            return false;
        }

        var transition = equipState.AddTransition(idleState.state);
        transition.AddCondition(AnimatorConditionMode.Equals, upperID, "GunType");

        foreach (var state in subMachineStates)
        {
            if (state.state.name.Contains("Fire"))
            {
                state.state.name = $"{weaponInfo.Name}Fire";
            }
            var ts = state.state.transitions;
            foreach (var transit in ts)
            {
                var conditions = transit.conditions;
                var newConditions = new AnimatorCondition[conditions.Length];
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (conditions[i].parameter == "GunType" && conditions[i].mode == AnimatorConditionMode.NotEqual)
                    {
                        var outCondition = new AnimatorCondition();
                        outCondition.parameter = "GunType";
                        outCondition.mode = AnimatorConditionMode.NotEqual;
                        outCondition.threshold = upperID;
                        newConditions[i] = outCondition;

                    }
                    else
                    {
                        newConditions[i] = conditions[i];
                    }
                }
                transit.conditions = newConditions;
            }
        }

        EditorUtility.SetDirty(animatorController);
        return true;
    }

    void NamePlatesDoc()
    {
        DrawSuperText("名称牌即 <b>头顶玩家名</b>，是游戏中显示在队友头顶的 GUI 文本。这是所有多人游戏里的基础功能。MFPS 中该 GUI 使用 Unity 旧版 OnGUI 系统渲染，因为对该用途而言它足够简单。\n \n自定义这个 GUI 的设计相当简单：\n \n每个玩家预制体上都挂有 <?underline=>bl_NamePlateDrawer.cs</underline> 脚本。为了方便调整名称牌 UI 的外观，你可以在编辑器 <i>编辑模式</i> 下预览：把玩家预制体拖入场景 ➔ 选中该实例 ➔ bl_NamePlaterDrawer ➔ 点击 <b>Simulate [OFF]</b> 按钮。");
        DrawServerImage("img-30.png");
        DrawText("预览开启后即可通过前端属性编辑外观：在 bl_NamePlateDrawer 的检视面板中 ➔ 点击 Edit Present 按钮 ➔ 会打开名称牌的 GUI 样式属性。");
        DrawServerImage("img-31.png");
        DrawText("按需调整属性，改动会在 Scene 视图中实时预览 ➔ 完成后再次点击 <b>Simulation [On]</b> 按钮关闭模拟 ➔ 把改动应用到玩家预制体。");
        DownArrow();
        DrawHyperlinkText("<b><size=16>隐藏血条</size></b>\n\n若想隐藏队友名称牌上的血条，只显示玩家名，关闭 <link=asset:Assets/Resources/GameData.asset>GameData</link> 中的 <b>Show Teammates Health Bar</b> 开关即可。");
    }

    void PlayerPrefabsDoc()
    {
        DrawHyperlinkText("<b>玩家预制体</b> 指包含玩家控制器所需的全部脚本、对象与结构的 Unity 预制体。\n\nMFPS 的玩家预制体放在名为 <b>Resources</b> 的 Unity 特殊文件夹中 <i>位于项目的 Resources 文件夹内</i>。若要修改与玩家相关的任何内容，例如武器位置、动画、脚本属性等，都必须改动这些预制体。\n\n<b>MFPS 默认使用 2 个玩家预制体</b>，在 <b><link=asset:Assets/Resources/GameData.asset>GameData</link></b> 中指定，即 <b>Player1</b> 与 <b>Player2</b>。Player1 用于阵营 1，Player2 用于阵营 2；若游戏模式不是阵营制，例如自由混战，则使用 Player1。\n\n自 1.8 版本起，你还可以按场景覆盖 Player1 与 Player2，也就是说可以给每张地图的每个阵营指定不同的玩家预制体。做法是在地图场景的任意对象上挂载 <b>bl_OverridePlayerPrefab.cs</b> 脚本 ➔ 在该脚本的检视面板中指定玩家预制体即可。");
        DownArrow();
        DrawHyperlinkText("如果你需要更进阶的玩家选择方案，或想加入更多玩家预制体让玩家在游戏内选择角色，可以看看 <link=https://www.lovattostudio.com/en/shop/addons/player-selector/>Player Selector</link> 插件。\n");
    }

    void DrawBullets()
    {
        if (subStep == 0)
        {
            DrawText("MFPS 中的子弹使用对象池。与 MFPS 其他池化对象一样，它们登记在 <b>bl_ObjectPooling</b> 脚本中，该脚本挂在每张地图场景的 <b>GameManager</b> 上。\n在 <b>bl_Gun</b> 中你只需填写池化名称。\n\n\n你可能想为某把武器添加新子弹，例如想要不同的拖尾效果，做法如下：\n\n<size=18>复制子弹预制体：</size>\n\n在 MFPS ➔ Content ➔ Prefabs ➔ Weapon ➔ Projectiles ➔ * 中选中某个已有预制体，按 Ctrl + D 复制，Mac 上为 Command + D。\n\n然后在复制出的预制体上做你想要的修改，之后在 <b>bl_ObjectPooling</b> <i>该脚本挂在房间场景的 GameManager 对象上</i> 中新增一个列表项，把该子弹预制体拖入并设置池化名称：");
            DrawServerImage(17);
            DrawText("然后打开玩家预制体，选中要指定子弹的 <b>第一人称武器</b>，在该武器的 bl_Gun 脚本中把子弹的池化名称填入 Bullet 字段。\n");
            DrawServerImage(18);
            DrawText("把改动应用到玩家预制体即可。");
        }
        else if (subStep == 1)
        {
            DrawSuperText("子弹弹痕同样使用对象池，在子弹击中碰撞体时自动放置，弹痕材质会根据碰撞体的标签选择。弹痕数量不限。\n \n<b>Bullet Decal Manager</b> 位于每张地图场景的 <b>GameManager</b> 对象下，即 <i><b>GameManager ➔ Bullet Decal Manager ➔ bl_BulletDecalManager</b></i>。\n \n<?title=18>修改、新增或移除弹痕：</title>\n \n<?list=■>用简单的 alpha 着色器新建一个材质\n把弹痕贴图指定给该材质。\n在 <b>Bullet Decal Manager ➔ Decal List ➔ Surface Decals</b> 中，若弹痕对应新的标签则新增一个标签项，若对应已有标签直接展开该项即可。\n把弹痕材质加入 <b>Decal Materials</b> 列表。\n完成。</list>");
            DrawServerImage("img-39.png");
        }
        else if (subStep == 2)
        {
            DrawText("若你需要另一种类型的子弹，不只是换拖尾，而是不同的弹道或命中判定，也可以在不破坏原有功能的前提下实现。\n \n子弹与投射物支持继承扩展，可自定义类。<b>不要直接修改默认的子弹与投射物脚本</b>，\n而是新建一个类继承自 <color=#FFCC2AFF>bl_ProjectileBase</color>，重写所需函数，把默认脚本仅作参考，按需实现你自己的子弹或投射物逻辑。\n \n把自定义脚本挂到 GameObject 上并做成预制体，再把这个预制体加入池化列表即可使用。");
        }
    }

    void DrawKitsSystem()
    {
        DrawText("MFPS 有一套简单但实用的补给包系统，玩家可以在对局中投掷和拾取弹药包或医疗包。默认使用 <b>H</b> 键投掷，投出的是弹药包还是医疗包 <i>取决于玩家兵种</i>。");
        DrawServerImage(7);
        DownArrow();

        DrawTitleText("修改投掷补给包的按键");
        DrawText("- 玩家预制体根节点上有一个脚本 <b>bl_ThrowKits</b>，其中有 <b>Throw Key</b> 属性，在此设置投掷补给包的按键。\n");
        DownArrow();
        DrawTitleText("修改补给包的模型");
        DrawServerImage(19);
        DrawText("•  补给包预制体位于 <i>Assets -> Prefabs -> Level -> Items->*</i>\n\n•  选中要换模型的补给包，即 MedKit 或 AmmoKit，拖入场景层级。\n\n•  用你的新模型替换网格，应用改动并保存预制体。\n\n");
        DownArrow();
        DrawTitleText("修改补给包投放指示器模型");
        DrawServerImage(20);
        DrawText("预制体位于 <i>Assets -> Prefabs -> Level -> AirDrop->*</i>\n");
    }

    void DrawKillZones()
    {
        DrawText("你的地图可能存在不希望玩家越过的边界。MFPS 为此提供的方案是 <b>击杀区</b>：玩家进入后会弹出带倒计时的警告，若在计时归零前没有离开该区域，游戏会自动将其击杀并送回出生点。\n");
        DrawServerImage(21);
        DrawText("添加击杀区的方法：创建一个带 <b>Box Collider</b> 的对象 <i>该碰撞体即代表区域范围</i>，然后挂载 bl_DeathZone.cs 脚本，设置玩家离开该区域的时限，以及玩家位于区域内时屏幕上显示的提示文本。\n");
    }

    void DrawGameSettings()
    {
        DrawText("MFPS 允许玩家在运行时修改部分游戏设置，例如画质与控制设置。作为开发者，你需要为这些设置指定默认值，也就是玩家首次进入游戏时采用的值。");

        DrawHyperlinkText("设置默认值的路径：<link=asset:Assets/Resources/GameData.asset>GameData</link> ➔ Default Settings ➔ Setting Values ➔ *");
        DrawServerImage(28);
        DrawText("该列表包含全部可配置项，展开要修改的设置并设定所需的值即可。");
        DownArrow();
        DrawTitleText("新增设置项");
        DrawText("新增设置项很简单：在同一个列表 <i><b>Setting Values</b></i> 中新增一项，设置一个唯一名称用于标识该设置 ➔ 选择设置类型 <i>float、integer、bool 或 string</i> ➔ 设定默认值。\n\n在游戏内读取该值的方式：");
        DrawCodeText("var val = bl_MFPS.Settings.GetSettingOf('THE_SETTING_NAME');");
        DrawText("设置项加入列表后，玩家在游戏内应用设置时 <i>点击 <b>Save</b> 按钮</i> 会自动保存。若想按自己的规则保存，可用：");
        DrawCodeText("bl_MFPS.Settings.SetSettingOf('THE_SETTING_NAME', THE_SETTING_VALUE);");
        DrawText("想了解在游戏内的实际用法，可参考脚本 <b><color=#00E9FFFF>bl_SingleSettingsBinding.cs</color></b>\n");
    }

    void MouseLookDoc()
    {
        DrawHyperlinkText("视角控制即鼠标视角与相机控制是射击游戏或任何快节奏动作游戏的核心功能。\n \nMFPS 采用了几种技术来提升视角移动的精准度与顺滑度，例如 <b>帧平滑</b> 与 <b>移动平滑</b>。\n \n若要个性化该运动，可在检视面板中修改若干属性：\n \n通用设置位于 <link=asset:Assets/Resources/GameData.asset>GameData</link> -> Mouse Look Settings。");
        DrawPropertieInfo("Use Smoothing", "bool", "是否启用帧平滑。该技术通过计算过去若干帧鼠标移动的平均值，让视角移动更顺滑。");
        DrawPropertieInfo("平滑帧数", "int", "参与平均值计算的缓冲帧数。数值越大越顺滑，但精度越低。");
        DrawPropertieInfo("Lerp Movement", "bool", "是否为移动额外叠加一层平滑。适合触屏设备，不推荐用于鼠标或手柄操作。");
        DrawPropertieInfo("瞄准灵敏度调整方式", "enum", "决定鼠标灵敏度如何过渡到瞄准灵敏度。<b>Fixed</b> 表示直接采用瞄准灵敏度的确切值，<b>Relative</b> 表示按相机视野变化量换算。");
        DrawHorizontalSeparator();
        DrawText("<b><size=22>逐玩家属性</size></b>\n\n还有一些属性可以按玩家分别设置，位置为玩家预制体 ➔ bl_FirstPersonController ➔ Mouse Look ➔ *，");
        DrawHorizontalSeparator();
        DrawHyperlinkText("<b><size=22>灵敏度</size></b>\n\n玩家可以在游戏内调整鼠标或手柄灵敏度，但默认值由你设定，位置为 <link=asset:Assets/Resources/GameData.asset>GameData</link> ➔ Default Settings ➔ Settings Values ➔ <b>Sensitivity</b> 与 <b>Aim Sensitivity</b>。");
    }

    void DrawObjectPooling()
    {
        DrawText("<b><size=15>什么是对象池？</size></b>\n\n<b>Instantiate()</b> 与 <b>Destroy()</b> 是游戏运行中常用且必要的方法，单次调用消耗的 CPU 时间通常很少。\n\n但对于运行期创建、生命周期短、每秒销毁数量巨大的对象，例如子弹，CPU 需要花费的时间就相当可观。\n\n这正是对象池发挥作用的地方。<b>对象池</b> 指在游戏开始前预先实例化所有可能用到的对象。MFPS 中子弹、弹痕和命中粒子都使用对象池。\n\nbl_ObjectPooling.cs 使用起来很简单。把对象加入池中只需在 bl_ObjectPooling 检视面板的 <b>pooledPrefabs</b> 列表中登记该预制体，该脚本挂在各地图场景的 <b>GameManager</b> 对象上。加入预制体后设置一个池化名称，再估计需要多少个实例即可。\n\n在脚本中取用该预制体时，原先通常写法如下：\n\n");
        DrawCodeText("GameObject ob = Instantiate(MyPrefab, position, rotation);");
        DrawText("使用 bl_ObjectPooling 后，替换为：");
        DrawCodeText("GameObject ob = bl_ObjectPooling.Instance.Instantiate(\"PrefabKey\", position, rotation);");
    }

    void AddNewMenu()
    {
        DrawText("若想在大厅 UI 中新增菜单或窗口，按以下步骤操作：\n\n1 - 制作菜单或窗口 UI：按需求设计界面，但要把全部内容放在 Canvas 下的一个父对象之下。例如新建一个空 GameObject 命名为 <i>MyNewWindow</i>，置于 Canvas 下，把新菜单或窗口的所有按钮、文本、图片等放到该对象下。\n\n");
        DrawAnimatedImage(0);
        DrawText("2 - 在 Lobby -> Canvas [Default Menu] -> bl_LobbyUI -> Windows 的 <b>Windows</b> 列表中新增一项，把 <i>MyNewWindow</i> 对象填入该字段并指定一个唯一名称。\n");
        DrawAnimatedImage(1);
        DrawText("3 - 创建菜单按钮：新增一个用于打开该菜单或窗口的按钮。其他按钮都位于 Lobby -> Canvas -> Lobby -> Content -> Top Menu -> Buttons -> *，可以复制其中一个并修改标题文本。\n\n在新按钮上把 bl_Lobby -> ChangeWindow(string) 函数注册为监听，并把 Windows 列表中新窗口的名称作为参数传入即可。\n");
        DrawAnimatedImage(2);
    }

    void DrawAddonsDoc()
    {
        DrawText("Game Framework 的核心功能设计得相当完整，但不包含 FPS 游戏常见的一些标准功能，例如小地图、登录系统、等级系统、载具、商店等。MFPS 确实提供其中许多功能，以扩展或插件形式提供，让你按需选择。\n \n这些功能不放进默认配置，主要出于价格与灵活性的考虑。若把所有可能的功能都塞进核心包，价格必然被推高，可能超出很多人的承受范围。提供精简的核心包可以把入门价格控制在低位，让从爱好者到专业开发者的更广泛人群都能负担。\n \n模块化方式也尊重不同项目的独特需求与限制。并非每个游戏都需要复杂的账号系统、载具或连杀奖励。让开发者只挑选并集成自己需要的功能，可以避免为不需要的东西付费或耗费精力。这让 MFPS 不仅更可定制，也更轻量高效。\n\n<b>核心包本身功能完整，插件全部可选。</b>\n \n尽管是可选的，这些插件都经过细致设计，能与核心系统无缝衔接。大多数支持自动集成，无需手动编码或配置。你只需选择需要的功能，系统会平滑接入你的项目。");

        DrawText("<b><size=22>如何集成插件？</size></b>\n\n所有插件的根目录下都有 <b>ReadMe.txt</b> 说明文件，而且几乎所有插件都<b>支持自动集成</b>，你只需启用插件并点击 Integrate 菜单项：\n");
        DrawAnimatedImage(3);
    }

    void EditorMenusDoc()
    {
        DrawText("MFPS 提供了一系列独立的 <b>编辑器窗口</b>，位于 Unity 编辑器顶部菜单的 <b>MFPS</b> 根菜单下：");
        DrawServerImage("img-26.png");
        DrawText("以下简要说明各项用途：\n \n<b><size=18>MFPS ➔ Tutorials</size></b>\n \n- 该子菜单列出了 MFPS 的内置编辑器文档与教程，其中 <b>Tutorials ➔ Documentation</b> 是 MFPS 的主文档。\n \n<b><size=18>MFPS ➔ Tools</size></b>\n \n- 该子菜单提供多种操作与辅助窗口，为 MFPS 提供实用的自动化操作。");
        DrawText("<b><size=18>MFPS ➔ Manager</size></b>\n \n- 打开 MFPS 管理器窗口，可查看 MFPS 的前端设置。该窗口还包含一组实用工具窗口，可管理 MFPS 的一些主要设置，例如玩家默认武器配置、武器信息、关卡等。\n \n<b><size=18>MFPS ➔ MFPS</size></b>\n \n- 打开 MFPS 窗口，也就是首次导入 MFPS 时自动弹出的那个窗口。其中包含当前 MFPS 项目的实用信息，例如所用 MFPS 版本、该版本的更新日志、教程链接与联系方式。");
    }

    void DrawGameTexts()
    {
        DrawText("若要修改游戏中的某些文本，或仅调整文字表述，大部分文本直接设置在 UI Text 组件中，这些组件位于各场景的 Canvas 对象内。同时还有一部分文本由代码在运行时设置或修改。为方便你查找，我们把这些文本统一放在 <b>bl_GameTexts.cs</b> 这一个脚本中。\n \n该脚本集中了所有由代码在运行时赋值的文本，可直接在此修改，也便于你接入自己的本地化系统。");
        DrawServerImage("img-22.png");
    }

    void GameInputDoc()
    {
        if (subStep == 0)
        {
            DrawText("射击游戏 <b><size=8>或者说所有游戏</size></b> 的一个关键要素是操作控制。在动作游戏中这一点尤其特殊，因为玩家往往有自己习惯的按键设置。MFPS 默认采用多数 FPS 游戏通用的标准按键方案。\n \n自 1.9 版本起，MFPS 内置了自定义输入管理器，可在运行时通过设置窗口中的菜单重新绑定按键。你可以在 <i>Input Mapped</i> 中定义默认按键，是否允许玩家在游戏内修改由你决定。\n \n如果你想使用第三方输入管理器，MFPS 同样支持。游戏中使用的全部输入都定义在 bl_GameInput.cs 脚本中，修改这些函数中的代码即可指向你自己的输入系统。\n \n通过函数名即可判断某个输入对应哪个游戏动作 <i>开火、装弹、跳跃等</i>。");
        }
        else if (subStep == 1)
        {
            DrawText("要修改键盘或手柄的默认按键映射，只需改动对应的映射资源。\n\n选中该映射 ScriptableObject，若使用的是默认资源，其位置为 <i>Assets ➔ Prefabs ➔ Presents ➔ Input Mappeds➔*</i>。选中后在检视面板中可看到全部已配置的输入项，展开要修改的项并编辑其中的信息 <i>按键码、轴名称、描述等</i>。\n");
            DrawServerImage("img-40.png");
            DownArrow();
            DrawText("你还可以调整输入项的排列顺序，列表顺序即游戏内的显示顺序。");
        }
        else if (subStep == 2)
        {
            DrawText("新增输入很简单，基本只需在输入映射中加一项并设置按键码。\n\n1 - 选中要新增输入的映射资源。默认只有两个：Keyboard 与 Xbox Controller，选择你要修改的那个，其位置为 <i>Assets ➔ Prefabs ➔ Presents ➔ Input Mappeds➔*</i>。\n\n2 - 在该映射的检视面板中会看到名为 <b>Button Map</b> 的列表，其中是当前全部输入项。在列表中新增一项并填写信息：");

            DrawPropertieInfo("KeyName", "string", "该输入的自定义按键名称");
            DrawPropertieInfo("PrimaryKey", "KeyCode", "该输入对应的 Unity 按键码");
            DrawPropertieInfo("PrimaryAxis", "string", "若该输入对应的是轴而非按键，在此填写轴名称。");

            DownArrow();

            DrawText("按键配置完成后即可在代码中使用，用法与 Unity 默认 Input 非常接近。原先写法为：\n");
            DrawCodeText("Input.GetKeyDown('keyName'){...}");
            DrawText("现在改用：");
            DrawCodeText("bl_Input.isButtonDown('keyName'){...}");
            DrawText("or");
            DrawCodeText("bl_Input.isButton('keyName'){...}\nbl_Input.isButtonUp('keyName'){...}");
            DrawText("其中 <i>keyName</i> 的值是你为该输入在 Input Mapped 中设置的 <b>KeyName</b>。\n");
        }
        else if (subStep == 3)
        {
            DrawText("要在 MFPS 与输入管理器中用手柄，需要额外几步操作。\n \n- 首先，若安装 MFPS 时 <i>在安装窗口里</i> 没有覆盖输入设置，你需要修改 Unity 输入设置以添加所需的控制轴。输入管理器附带了一份已配置好的 <b>Input Settings.asset</b>，点击下方按钮即可应用。");

            if (!File.Exists("ProjectSettings/InputManager-backup.asset"))
            {
                if (Buttons.FlowButton("配置 Unity 输入管理器"))
                {
                    string sourcePath = "Assets/Prefabs/Presents/Input Mappeds/InputManager.txt";
                    if (!File.Exists(sourcePath))
                    {
                        Debug.LogWarning("未找到 MFPS InputSettings 数据。");
                        return;
                    }
                    string imFile = "ProjectSettings/InputManager.asset";
                    if (!File.Exists(imFile))
                    {
                        Debug.LogWarning("未找到 InputManager 数据。");
                        return;
                    }
                    File.Move(imFile, imFile.Replace("InputManager", "InputManager-backup"));
                    File.Copy(sourcePath, "ProjectSettings/InputManager.asset");
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                GUILayout.Label("MFPS 输入设置已集成完成。");
            }

            DownArrow();

            DrawSuperText("在 <?link=asset:Assets/Resources/InputManager.asset>InputManager</link> 的 <b>Mapped</b> 字段中指定你的手柄对应的输入映射。默认附带 Xbox 手柄的映射，把该映射或你自建的映射拖入 <b>Mapped</b> 字段即可");
            DrawServerImage("img-41.png");
        }
        else if (subStep == 4)
        {
            DrawText("新建输入映射的方法：在 <i>Project 视图</i> 中进入目标文件夹 -> 右键 -> Create -> MFPS -> Input -> Input Mapped，随后即可看到新建的资源对象，选中它并配置你的手柄或键盘的全部输入。");
            DrawServerImage("img-42.png");
            DrawText("也可以直接复制一个默认映射再修改其中的输入项。");
        }
    }

    private AssetStoreAffiliate uiAssets;
    void GameUIDoc()
    {
        if (subStep == 0)
        {
            DrawSuperText("界面重设计是一项重要改动，但常常被跳过或投入不足。修改默认 UI 极为重要，不只是换颜色，还要改实际布局，条件允许的话连精灵图和整体设计一起改。这不仅让游戏看起来更独特，也能避免成为又一个快速拼凑的仿制品，进而规避由此带来的差评。\n \nMFPS 中修改 UI 没有特殊步骤，与任何使用 UGUI 的 Unity 项目一样操作即可。全部 UI 都在各场景的 Canvas 中组织与设计，可在此修改图片、文本、字体、精灵图、布局等。若你不熟悉 Unity UI 系统，先看看这个教程：\n<?link=https://learn.unity.com/tutorial/ui-components#>https://learn.unity.com/tutorial/ui-components#</link>");

            DrawText("若你已熟悉 Unity 的 UI 系统，默认 UI 几乎任何部分都可以改。只需注意 <b>不要删除被脚本引用或依赖的组件</b>。如果不确定某个 UI 对象能否删除，而你的目的只是隐藏它，那直接禁用即可。");
            DrawText("<b><size=18>大厅 UI</size></b>\n \n大厅的全部 UI 都在 <b>MainMenu</b> 场景层级中的 <b>Canvas</b> 里。常被问到的一个问题是：\n \n<b><size=14>在哪里更换背景图？</size></b>\n \n大厅背景图可在层级中该对象上挂载的图片组件的检视面板中更换：");
            DrawServerImage("img-48.png");
            DrawText("<b><size=18>地图 UI</size></b>\n \n修改某张地图场景的 UI 后，无需在其他地图场景重复操作。只要把改动应用到 UI 预制体，其他所有场景会同步生效。");
        }
        else if (subStep == 1)
        {
            DrawText("如果你想寻找 UI 套件来替换游戏的默认 UI，下面是一份人工筛选的资源合集，可在资源商店获取");
            Space(20);
            if (uiAssets == null)
            {
                uiAssets = new AssetStoreAffiliate();
                uiAssets.randomize = true;
                uiAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555480555/widget-medium");
                uiAssets.FixedHeight = 400;
            }
            else
            {
                uiAssets.OnGUI();
            }
        }
    }

    void DrawFriendListDoc()
    {
        DrawText("Photon 提供 <b>好友列表</b> 功能，可查询同一服务器上其他用户的状态。你只需提交要查询玩家的 UserID。该系统能力有限，只能知道玩家是否在线，并且只有当对方在某个房间中时才能加入其房间。\n\nMFPS 已集成该 Photon 功能。在主菜单场景中可以添加好友并保存在本地 <i>若使用 ULogin Pro 则存入库中</i>。添加后即可看到对方是否在线，并在对方处于某房间时加入该房间。\n\n添加好友无需对方确认，只要准确填写玩家名即可。\n\n添加好友的方式是点击大厅右上角带人物图标的按钮：\n\n");
        DrawServerImage(10);
        DrawText("好友列表有可添加好友数上限，默认为 25。这个限制只在把好友保存到数据库时才真正有意义 <i>例如使用 ULogin Pro</i>，因为每个玩家添加的好友越多，其数据在库中占用的体积越大。\n\n上限可在 GameData -> MaxFriendsAdded 中修改。\n");
    }

    void CrosshairDoc()
    {
        DrawText("准星是多数射击游戏的基础功能。MFPS 中可以方便地修改准星形状、颜色和大小，还能为每类武器使用不同准星。");
        DownArrow();
        DrawTitleText("Modify crosshair");
        DrawText("打开任意地图场景 ➔ 在层级窗口中进入 <b>UI ➔ PlayerUI ➔ Crosshair ➔ Crosshairs ➔ *</b>，这里可以看到全部准星配置，默认每种用于不同类别武器 <i>机枪、霰弹枪、匕首等</i>。\n\n在此打开某个准星配置或样式，按需任意修改即可，UI 组件可增可删，只要确保所有内容都位于该准星样式的根对象之下。");
        DrawServerImage(32);
        DownArrow();
        DrawTitleText("Hit Marker");
        DrawText("<b>命中标记</b> 是本地玩家击中敌人时出现的小十字。\n\n其外观可在 <b>UI ➔ PlayerUI ➔ Crosshair ➔ Crosshairs ➔ Hitmarker</b> 中自定义。\n\n命中标记以简单的放大动画出现，动画的最终尺寸可在 <b>bl_UCrosshair ➔ Increase Amount</b> 中设定。");
    }

    void FootStepsDoc()
    {
        DrawText("MFPS 的脚步声由地表标签驱动，预定义标签包括 <i>金属、混凝土、泥土、木头和水面</i>。玩家移动时会根据脚下的地表播放对应脚步声。这些音效可以更换，也可以新增地表标签。脚步声还能按玩家预制体分别设置，从而让不同玩家模型有不同音效。");
        DrawTitleText("Change Sounds");
        DrawHyperlinkText("更换 MFPS 默认脚步声的方式：直接替换默认 <link=asset:Assets/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link> 中的音频片段。展开 <b>Groups</b> 列表 ➔ 展开对应标签组 ➔ 替换其中的音频片段。");
        DrawServerImage(29);
        DownArrow();
        DrawTitleText("Add Surfaces");
        DrawHyperlinkText("若要新增一种地表 <i>用不同的标签区分</i> 并为其指定专属脚步声，只需在 <link=asset:Assets/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link> 的 Groups 列表中新增一项，在其中的 <b>Tag</b> 属性里填写该地表对应的标签。");
        DownArrow();
        DrawTitleText("Terrain Surfaces");
        DrawText("由于脚步声系统由 <b>标签</b> 驱动，每个对象或网格只能设置一个标签，这对 <b>Unity 地形系统</b> 会造成问题：地形是单一对象与网格，只能设一个标签。但地形通常由多个图层构成，各图层用不同贴图模拟不同地表，因此不够用。要让脚步声系统与 Unity 地形正确配合 <i>按地形图层播放不同音效</i>，需要按以下方式设置地形图层。\n\n•  首先，在场景层级中选中 Terrain 对象，挂载 <color=#00E9FFFF><b>bl_TerrainSurfaces.cs</b></color> 脚本\n\n•  挂载后会看到该脚本检视面板中的 <b>TerrainSurfaces</b> 列表有若干项，每一项对应 <b>Terrain</b> 中的一个图层 <i>贴图</i>。你需要为每个图层设置 <b>Tag</b> 名称。展开某项即可看到该图层的贴图，据此在 <b>Tag</b> 属性中指定应使用的标签。");
        DrawServerImage(30);
    }

    void DoorsDoc()
    {
        DrawSuperText("MFPS 内置一套经过优化的门系统，可为地图增添动态元素。\n \n该系统针对单张地图门数量较多的情况做了性能优化，支持网络同步，使用也很简单。\n \n<?title=18>如何添加门？</title>\n \n- 给地图添加一扇门只需两步：\n \n<b>1. 在地图中放置门：</b> 把门预制体拖入场景层级并按设计摆放。默认门预制体为 <?link=asset:Assets/Prefabs/Level/Items/Door.prefab>default door prefab</link>，位于 <i>Assets ➔ Prefabs ➔ Level ➔ Items ➔ Door</i>。\n \n<b>2. 注册新门：</b> 在地图场景层级中进入 ItemManager ➔ bl_DoorManager ➔ 点击 <b>Collect all active doors in scene</b> 按钮即可。");
        DrawServerImage("img-32.png");
        Space(20);
        DrawSuperText("<?title=18>如何创建新门？</title>\n \n- 若想使用自定义门模型，只需在 <?link=asset:Assets/Prefabs/Level/Items/Door.prefab>default MFPS door</link> 预制体基础上替换模型。\n \n1. 在任意场景中实例化 <?link=asset:Assets/Prefabs/Level/Items/Door.prefab>default MFPS door</link> 预制体，并解除该实例的预制体关联 <i>右键该门的实例 ➔ Unpack Prefab Completely</i>。\n \n2. 把自定义门模型拖入该实例的 Door ➔ Door Model ➔ * 下，并手动把新门模型的位置、旋转和缩放调整到与默认模型一致。\n \n3. 选中该门实例的根节点 ➔ <b>bl_BasicDoor</b> ➔ 在 <b>Door Pivot</b> 字段中拖入你新门模型的 Transform，该点将作为门的旋转轴心。\n \n4. 删除默认模型 <i>Door ➔ Door Model ➔ <b>Default Door Model</b></i>，然后把门拖到 Project 视图的任意文件夹中保存为新预制体。\n \n若门模型旋转不正确，新建一份 Door Settings <i>Project 视图 ➔ 右键 ➔ MFPS ➔ Level ➔ Door Settings</i> ➔ 把它指定给门预制体的 bl_BasicDoor ➔ <b>Door Settings</b>，并在其中设置新门模型轴心的旋转值。");
    }

    void GameStaffDoc()
    {
        DrawText("你可能想为参与开发的人员加上身份标识，例如让 <b>Lovatto <color=#FF0000FF>[Admin]</color></b> 以不同于普通玩家的颜色显示，让其他用户看出这是游戏的工作人员。Game Framework 提供了简单的做法，直接在检视面板中配置即可。\n\n进入 <b>Game Data</b>，在检视面板底部找到 Game Team 区域：\n");
        DrawServerImage(23);
        DrawText("该列表可添加任意数量的成员，每项设置都很简单：\n");
        DrawHorizontalColumn("UserName", "工作人员登录该账号时需要输入的名称。");
        DrawHorizontalColumn("Role", "该成员在团队中的职级或角色。");
        DrawHorizontalColumn("Password", "使用该账号名称登录时会弹出密码窗口要求输入密码 <i>其他名称不会弹出</i>，因此普通玩家无法冒充身份。");
        DrawHorizontalColumn("Color", "该名称在游戏中显示的文本颜色。");
    }

    void DrawMobileDoc()
    {
        DrawText("如果你的目标平台是移动端，例如 Android 或 iOS，需要注意：MFPS 虽然能在移动平台运行，但构建前还有一些准备工作。Game Framework 默认为了演示效果，按高端设备的高画质来配置，因此默认并未针对移动端优化，而且核心包也不包含任何移动端操作方案。");

        DrawHyperlinkText("因此首先需要接入移动端操作方案。官方为 Game Framework 专门提供了插件 <link=https://www.lovattostudio.com/en/shop/addons/mfps-mobile-control/>Mobile Control</link>，其中包含移动端与触屏设备所需的全部按钮与输入，集成是自动的。当然你也可以改用自己偏好的第三方方案。");

        DrawText("其次需要做一些手动优化，与任何移动端项目一样：先移除后处理效果 <b><size=8><i>可在 Unity 包管理器中移除 Post Processing Stack 包</i></size></b>，把 Standard 着色器换成移动端友好的着色器，降低贴图质量与分辨率，当然你新增的关卡、地图、模型也要适配移动端，等等。<b>无需改动任何代码</b>，MFPS 的代码本身已适配移动端并针对低端平台优化过。\n");
        DrawText("总的来说，构建移动端版本需要做的主要是图形优化。下面给出一些有用的教程链接，帮助了解图形优化及相关注意事项：");
        DrawLinkText("https://docs.unity3d.com/2020.1/Documentation/Manual/MobileOptimizationPracticalGuide.html", true);
        DrawLinkText("https://learn.unity.com/search/?k=%5B%22tag%3A5816095d0909150016dc7b17%22%2C%22lang%3Aen%22%2C%22q%3Aoptimization%22%5D", true);
        DrawLinkText("https://cgcookie.com/articles/maximizing-your-unity-games-performance", true);
        DrawLinkText("http://www.theappguruz.com/blog/graphics-optimization-in-unity", true);

        DrawSuperText("如果你刚开始做移动端开发并希望省些时间，还有一个可直接用于移动平台的 <b>MFPS Mobile</b> 资源，已集成移动端输入控制器，游戏优化也已做好。感兴趣可在此了解：<?link=https://www.lovattostudio.com/en/shop/mobile/mfps-mobile/>MFPS Mobile</link>");
    }

    void PlayerIKDoc()
    {
        DrawText("第三人称或远程玩家的上半身姿态由动画控制器中的动画片段决定，但模型骨骼中有一部分由反向动力学即 IK 额外控制。这样做有多重目的，其中之一是 <b>让你不必为每把武器单独制作玩家动画</b>，因为左手的位置通常随武器不同而变化。\n \nMFPS 通过对这些特定骨骼使用 IK 来解决，且这些骨骼可以按武器逐个调整。\n \n编辑某把第三人称武器的左臂位置或姿态，步骤如下：\n \n1. 在编辑器 <i>编辑模式</i> 下把任一玩家预制体拖入场景 <i>最好是干净场景以便专注</i>。\n \n2. 在层级中找到位于玩家模型右手下的远程武器组 -> 选中要调整姿态的第三人称武器并启用它。\n \n3. 选中该武器后 -> 在检视面板中会看到 <b>Edit Hand Position</b> 按钮 -> 点击它，在 Scene 视图中移动或旋转 IK 目标，直到得到想要的位置。");
        DrawAnimatedImage(5);
    }

    void PostProcessingDoc()
    {
        if (subStep == 0)
        {
            DrawText("MFPS 默认使用 Unity 的 <b>Post-Processing Stack v2</b>，这是一组作用于相机、用于提升画面效果的滤镜与图像特效。\n\n图像特效确实能显著改善画面观感，但也会带来性能开销。因此建议只在 PC 或主机这类高端平台使用，<b>不要用于移动平台</b>。若目标平台是移动端，应当删除这个包。\n\n首次在项目中导入 MFPS 时，该系统会自动从 Unity 包管理器 UPM 导入。若要 <b>删除</b>，点击以下右键菜单即可：");
            DrawServerImage(11);
            DrawText("Post-Processing Stack 官方文档见：");
            DrawLinkText("https://docs.unity3d.com/Packages/com.unity.postprocessing@2.3/manual/index.html", true);
        }
        else if (subStep == 1)
        {
            DrawSuperText("自 1.9 版本起，可以方便地 <b>为每张地图场景定义后处理配置</b>。\nPost Process Profile 是配置文件，其中定义后处理 Volume 中要渲染的图像特效。若需更多细节，或不清楚如何创建与使用，可参阅官方文档：<?link=https://docs.unity3d.com/Packages/com.unity.postprocessing@3.1/manual/Quick-start.html>后处理快速上手</link>\n\n<?title=18>在地图场景中指定配置</title>\n\n在特定地图场景使用自定义后处理配置：在编辑器中打开该地图场景 ➔ 在层级窗口中进入 GameManager ➔ Post Process Volume ➔ 在 bl_PostProcessEffects 的检视面板中 ➔ 把你自定义的 Post Process Profile 指定到 Process Profile 字段即可。");
        }
        else
        {
            DrawSuperText("与后处理包相关的若干常见问题会在控制台报错，原因是缺少对该包的引用。处理方式如下：\n \n<?title=18>不想使用该包：</title>\n \n如果你不打算使用后处理包，却仍因缺少引用而报错，需要移除该包的脚本定义符号。进入 Unity Player Settings ➔ Other Settings ➔ Script Define Symbols ➔ 在输入框中找到并删除 <?underline=><b>UNITY_POST_PROCESSING_STACK_V2;</b></underline> 这一段 ➔ 回车并等待编译。\n \n \n<?title=18>想使用该包：</title>\n \n如果你想使用后处理功能，却因缺少引用而报错，说明该包尚未导入。\n \n导入方式：Window ➔ Package Manager ➔ 在左侧面板找到 <?underline=>Post Processing</underline> 包 ➔ 点击窗口右下角的 Import 按钮。");
        }
    }

    private AssetStoreAffiliate fxAssets;
    void ParticlesDecalsDoc()
    {
        if (subStep == 0)
        {
            DrawHyperlinkText("<b><size=22>粒子</size></b>\n\n粒子效果是让游戏画面出彩的重要一环。MFPS 虽然只在少数事件中使用粒子，但出现频率高，所以光好看还不够，还必须经过优化。MFPS 的默认粒子基本只是占位，虽非必须，但推荐替换成更好的效果。\n \n替换或修改特定粒子只需改动其预制体。MFPS 在以下游戏效果中使用 <b>Particle System</b>：\n \n•  枪口火焰\n•  爆炸\n•  子弹命中\n•  <link=asset:Assets/Prefabs/Level/Particles/Prefabs/Impacts/BloodFast.prefab>血液</link>\n•  火焰\n \n \n除位于各第一人称武器内部的 <b>枪口火焰</b> 粒子外，其余粒子都可在项目 Assets 文件夹的 <i>Assets -> Prefabs -> Level -> Particles->*</i> 中找到。");
            DrawNote("修改粒子预制体时请以默认预制体为参照。除 Particle System 组件外，它们还挂有自定义脚本，这些脚本是游戏正常运行所必需的。");
            Space(50);
            DrawText("<b><size=22>弹痕</size></b>\n \nMFPS 没有使用专门的自定义弹痕系统。因为弹痕仅用于子弹痕迹，且实例化极为频繁，专用系统只会带来不必要的性能开销。因此 MFPS 使用简单的 Quad 网格配合简单的透明着色器作为弹痕。\n \n如前所述，MFPS 唯一的弹痕用途是 <b>子弹痕迹</b>。子弹击中碰撞体时生成，显示哪种痕迹取决于被击中碰撞体的标签。MFPS 默认使用 5 种子弹痕迹，对应不同地表 <i>金属、木头、沙地、混凝土以及一种通用痕迹</i>。更换痕迹贴图的方式是修改子弹命中预制体的材质，这些预制体位于 <i>Assets -> Prefabs -> Level -> Particles -> WeaponEffects -> Prefabs->*</i>。");
        }
        else if (subStep == 1)
        {
            DrawText("如果你想寻找粒子或弹痕来替换游戏内的默认效果，下面是一份人工筛选的资源合集，可在资源商店获取");
            Space(20);
            if (fxAssets == null)
            {
                fxAssets = new AssetStoreAffiliate();
                fxAssets.randomize = true;
                fxAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555492698/widget-medium");
                fxAssets.FixedHeight = 400;
            }
            else
            {
                fxAssets.OnGUI();
            }
        }
    }

    void InGameChatDoc()
    {
        DrawText("游戏中玩家彼此交流的方式之一自然是文字聊天，MFPS 自然也实现了这一功能。\n \n在对局中玩家进入地图后，按 <b>回车或提交键</b> 即可打开文本输入框开始输入，再用同一个键或输入框旁的按钮发送消息。消息默认显示在屏幕左下角。这是基础功能，但不止于此：聊天还支持队伍频道，玩家只能与队友交流，按默认的 <b>T 键</b> 打开对应聊天框即可。\n \n出于某些原因，你可能 <b>不希望开放玩家之间的交流</b>。这种情况下直接删除或禁用 Chat UI 即可关闭文字聊天，该 UI 位于地图场景的 UI Canvas 中，路径为 UI -> MenuUI -> <b>Chat</b>。");
        DrawHorizontalSeparator();
        Space(10);
        DrawText("若想通过代码在聊天面板向房间内所有玩家显示自定义文本，可以这样写：");
        DrawCodeText("bl_ChatRoom.Instance.SetChat('YOU TEXT HERE');");
    }

    void LadderDoc()
    {
        DrawText("该资源包内置一套即拖即用的梯子系统，可为地图增添更灵活的移动路线。\n \n给地图添加梯子只需把梯子预制体 <i>默认位于 Assets ➔ Prefabs ➔ Level ➔ Items ➔ <b>Ladder</b></i> 拖入地图场景层级，摆放到目标位置即可。");
        DrawHorizontalSeparator();
        DrawText("<b><size=16>替换梯子模型</size></b>\n \n若要替换或新增梯子模型，使用默认梯子预制体并删掉默认模型、换成自己的模型即可。\n \n1. 把默认梯子预制体 <i>位于 Assets ➔ Prefabs ➔ Level ➔ Items ➔ <b>Ladder</b></i> 拖入任意场景层级。\n2. 把新梯子模型拖入该预制体实例中，并调整到与默认模型相同的位置，即 Ladder > Model > * 下。\n3. 删除该预制体实例内的原模型。\n4. 保存为新预制体，即可在地图场景中使用。");
        DrawServerImage("img-46.png");
    }

    void MFPSEventsDoc()
    {
        DrawText("如果你想在自己的脚本中实现自定义功能或改动，可以使用一组特殊事件，例如本地玩家出生、本地玩家死亡、受到伤害等。\n\n使用这些事件很简单，只需在脚本中订阅一个函数，运行时事件派发时即可收到回调。\n\n订阅写在 <b>OnEnable()</b> 中，取消订阅写在 <b>OnDisable()</b> 中：");
        DrawCodeText("void OnEnable()\n{\nbl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;\n}\n\nprivate void OnDisable()\n{\nbl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;\n}\n\nvoid OnLocalPlayerSpawn()\n{\n//execute your code\n}");
        DownArrow();
        DrawText("以下是全部可用事件及其简要说明\n");
        DownArrow();

        DrawCodeText("bl_EventHandler.onLocalPlayerDeath");
        DrawText("本地玩家在游戏中死亡时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalPlayerSpawn");
        DrawText("本地玩家出生时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpGun");
        DrawText("本地玩家拾取武器时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onChangeWeapon");
        DrawText("本地玩家切换武器时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalAimChanged");
        DrawText("本地玩家切换瞄准状态时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onMatchStart");
        DrawText("房间对局开始时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onFall");
        DrawText("本地玩家从空中坠落或落地时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpHealth");
        DrawText("本地玩家在游戏中拾取生命值时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAirKit");
        DrawText("本地玩家呼叫空投时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAmmoPickUp");
        DrawText("本地玩家在游戏中拾取弹药时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalKill");
        DrawText("本地玩家在游戏中击杀或被击杀时调用。");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.OnRoundEnd");
        DrawText("游戏回合结束时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPlayerLand");
        DrawText("本地玩家坠落后落到地面时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onRemoteActorChange");
        DrawText("非本地玩家的其他玩家出生或死亡时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("本地玩家修改游戏内设置或选项时调用");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onEffectChange");
        DrawText("本地玩家在游戏中修改一项或多项后处理效果选项时调用。");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("本地玩家通过游戏内设置菜单修改游戏设置时调用。");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGamePause");
        DrawText("本地玩家暂停或恢复游戏时调用。");
        DrawHorizontalSeparator();
    }

    void UpdateMFPSDoc()
    {
        DrawText("MFPS 大约每 3 到 4 个月发布一次大版本更新，这些更新带来新功能、修复与改进。如果你已在使用 MFPS，多半会希望把改进与修复同步到自己的项目中。遗憾的是，受资源本身的性质限制，直接把新版本包导入到旧版本项目上会覆盖你所有的改动，导致此前的工作与进度全部丢失。\n\n把旧版 MFPS 项目更新到新版本所需的工作量取决于多个因素，例如你改动了多少内容、项目所用版本与新版本的差距、新版本中变更的数量等。\n\n下面针对不同情况给出更新 MFPS 版本的方法：");
        Space(10);
        DrawNote("<b><color=#FF0002FF>重要提示：</color>在尝试以下任何方法或对项目做任何更新操作之前，务必先备份整个项目</b>");
        Space(10);
        DrawTitleText("前端改动");
        DrawText("- 如果你只做了前端改动，例如替换 <i>玩家模型、调整属性、添加地图、添加武器、修改 UI 等</i>，而未改动后端代码，更新就简单些：可以在现有项目上导入新版 MFPS 包，只是需要 <b>取消勾选</b> 下列资源。\n\n首先从资源商店或本地磁盘导入 MFPS 更新包。\n\n在 <b>Unity 包导入窗口</b> 中可以勾选要导入的资源，默认全部选中。为避免覆盖你的改动，只需 <b>取消勾选</b> 以下内容：\n\n•  <b>Resources</b> 文件夹 <i>包含其中全部预制体与 GameData</i>\n•  MainMenu 场景 <i>若你修改过 UI</i>\n•  任何你改动过的预制体。\n\nExampleLevel 场景则要导入，因为它几乎肯定包含变更，可以作为你其他地图场景的参照。\n");
        DrawServerImage(12);
        DownArrow();
        DrawTitleText("后端改动");
        DrawText("如果你对 MFPS 核心脚本做过 <b>代码改动</b>，或修改过 <b>GameManager、GameModes、Lobby 等</b> 主要预制体，更新流程就更复杂。\n\n首先，无法直接在当前项目上导入新版本，因为这会还原你所有的改动、丢掉全部工作。要应用新版本的变更，必须手动合并：查看新版本中脚本与预制体的改动，再与自己的项目逐一比对。\n\n这个过程耗时较长，而且合并失误很容易导致游戏报错。因此我给出一个能减少工作量的方法：\n\n•  首先新建一个干净的 Unity 项目，在其中按常规方式导入新版 MFPS 包 <i>同时导入 Photon PUN</i>。这个项目作为参照，用来查看场景、预制体、属性等前端层面的改动。\n\n•  然后在你的既有项目 <i>即修改过的旧版 MFPS 项目</i> 中 <b>模拟</b> 导入新版 MFPS 包。注意是 <b>模拟</b>，<b>不要真正导入</b>。你只需要让 Unity 包导入窗口弹出来，以便查看有哪些文件发生变化。该窗口会列出包中所有将导入的资源，若某个文件在项目中已存在，还会标明包内文件与项目内文件是否不同，这些有改动的文件右侧会显示一个 Refresh 图标：\n");
        DrawServerImage(26);
        DrawText("接下来要做的是记录这些变动过的文件，把文件名写在一个简单的文本文件里，方便后面按名称查找。\n\n收集好全部变动文件名之后就要开始合并。遗憾的是没有自动化方式，只能手动处理，逐一查看脚本、预制体等内容的改动。不过不必逐行核对，可以借助工具自动比对两个文件的差异。\n\n比对脚本改动可以使用以下工具：\n");
        DrawHyperlinkText("Standalone program: <link=https://sourcegear.com/diffmerge/>Diffmerge</link>\nOnline System: <link=https://www.diffchecker.com/>Diffchecker</link>\n");
        DrawText("这两个工具用起来都很直接：有两个文本框，一个放入旧文件的内容，另一个放入新文件，程序会分析并高亮出有差异的行：\n");
        DrawServerImage(27);
        DrawText("把原始文件 <i>即你项目中的那个</i> 放在左侧，新版 MFPS 的文件 <i>即你之前新建项目中的那个</i> 放在右侧。\n\n差异高亮出来后，只需把这些改动合并进你的项目文件 <i>即旧版 MFPS 的文件</i>。\n\n使用独立程序 <b>Diffmerge</b> 还可以比对整个 MFPS 文件夹、分析所有文件，操作为选择 File -> Open Folder Diff... -> 在第一个框中填入旧版 MFPS 文件夹路径，第二个框中填入新版 MFPS 文件夹路径。\n");
        DownArrow();
        DrawText("上述方法对脚本这类文本文件很高效，但比对预制体与场景的差异需要另想办法，得同时打开两个项目对比。好在 Unity 允许同时打开多个编辑器实例，因此可以把旧版项目与新版 MFPS 项目分别在不同编辑器窗口中打开，逐一检视两个项目中预制体与场景的差异。\n");


    }

    void ServerRegionDoc()
    {
        DrawText("Photon PUN 支持 <b>连接多个服务器区域</b>。MFPS 实现了全部可用服务器，并让玩家自行决定是否连接特定区域。\n \n默认情况下，Photon PUN 会根据玩家网络选择 <b><i>最佳区域</i></b> 连接。这对同时在线人数不多的游戏会造成问题：玩家被分散到不同区域，更难匹配到对局。因此 MFPS 定义了 <b>固定区域</b>，即所有玩家默认连接的区域，玩家如有需要也可以在游戏内手动切换到其他服务器。\n \n<b><b><size=18>定义固定区域</size></b></b>\n \n进入 MainMenu 场景 -> Lobby -> Lobby -> 在检视面板中找到 <b>Default Server</b> -> 选择作为默认连接区域的区域代码。\n \n<b><b><size=18>运行时更换服务器区域</size></b></b>\n \n玩家可在游戏内通过大厅菜单右下角的下拉框更换服务器区域。");
        DrawServerImage("img-23.png");
    }

    void LocalNotificationsDoc()
    {
        DrawText("MFPS 中有一些游戏内本地提示，会在特定事件发生后出现，例如 <i>完成击杀、拾取物品之后等</i>。这些提示包括右上角的 <b>谁击杀了谁</b>、屏幕中央的 <b>本地击杀提示</b>，以及左侧用于非重要信息的提示。\n \n关于击杀提示的详细说明，见本教程对应章节。\n \n<b><size=16>本地击杀提示</size></b>\n \n- 即本地玩家在游戏中淘汰敌人后出现在屏幕中央的提示。MFPS 提供两种显示方式：\n \n1. <b>队列：</b> 一次显示一条。若连续发生多次击杀，先显示其中一条 ➔ 等待动画与延迟时间 ➔ 隐藏 ➔ 再显示下一条，直到没有待显示的击杀。\n \n2. <b>列表：</b> 按需全部显示，以列表形式呈现，新的击杀会在发生时追加并显示在屏幕上。\n \n默认使用 <i>队列</i> 模式，可在 <b>GameData ➔ Local Kills Show Mode</b> 中修改。\n\n该系统是模块化且基于事件的，因此你也可以改用自建系统，只需从 <b>UI ➔ MenuUI ➔ Local Notifications ➔ Center Local Notifier</b> 中删除或禁用 Center Local Notifier 对象即可。");
        Space(10);
        DrawText("<b><size=16>左侧提示</size></b>\n \n- 通常用于非重要事件，例如拾取弹药、拾取武器、切换武器开火模式等。\n\n该系统同样是模块化的，你也可以移除或替换成自建系统，只需从 <i>UI ➔ MenuUI ➔ Local Notifications ➔ <b>Left Local Notifier</b></i> 中移除 Left Local Notifier 即可。\n\n调用自定义提示的代码如下：");
        DrawCodeText("using MFPS.Runtime.UI;\n            ...\n            void ShowNotificationSample()\n            {\n                new MFPSLocalNotification(\"MY NOTIFICATION TEXT HERE\");\n            }");
    }

    void NetworkStats()
    {
        DrawText("MFPS 网络框架可以向客户端广播应用与大厅的网络统计数据，可用于调试游戏网络，也可以把这些数据展示出来以体现游戏的活跃度。\n \nMFPS 中可以查看两类网络数据：\n \n<b><size=22>网络传输统计</size></b>\n \n用于显示当前网络收发数据包以及本地玩家延迟等基础但实用的信息，显示在屏幕左上角。\n \n该功能默认关闭，可在 GameData -> Show Network Stats 中方便地开启。");
        DrawServerImage("img-25.png");
        DrawText("<b><size=22>大厅统计</size></b>\n \n大厅统计可用于展示游戏的活跃度。统计按区域区分，因此你只能看到自己所在服务器区域的统计。\n \n可获取的信息包括：\n \n活跃房间数量\n加入大厅或大厅内各房间的玩家总数\n \n该功能默认开启，可在 MainMenu 场景 -> Lobby -> bl_Lobby -> Show Photon Statistics 中关闭。");
        DrawServerImage("img-24.png");
    }

    void PlayerHitboxDoc()
    {
        if (subStep == 0)
        {
            DrawText("受击盒是用于检测外部物体与玩家碰撞的基础形状碰撞体，通常为长方体、球体或胶囊这类简单形状。MFPS 中它们位于玩家预制体模型的标准人形骨骼上。\n \n使用 <b>Add Player Tutorial</b> 创建新玩家时会自动配置这些受击盒，但有时碰撞体与玩家模型骨骼并不贴合，可能过大、过小或位置偏移。遇到这种情况需要手动微调，把碰撞体范围调整到贴合骨骼：选中带有 Collider 组件的骨骼 Transform，在检视面板中调整 center 与 size 属性即可。\n \n<b>目标是让每个受击盒碰撞体尽可能贴合模型。</b>");
            DrawServerImage("img-33.png");
        }
        else if (subStep == 1)
        {
            DrawText("玩家受击盒的主要用途是 <b>检测子弹命中玩家</b>。MFPS 中可以为每个受击盒设置不同的伤害基数，从而根据命中部位产生不同伤害。\n \n为此可使用 <b>bl_HitboxManager</b> 脚本，该脚本挂在所有玩家与机器人预制体上。玩家预制体中有个名为 <b>Remote</b> 的子对象，选中它后，<i>bl_HitboxManager</i> 脚本的检视面板中会显示若干伤害倍率，可用来调整特定身体部位受到的伤害大小。默认这些倍率按部位分组应用 <i>头部、胸部、手臂和腿部</i>：");
            DrawServerImage("img-34.png");
            DownArrow();
            DrawText("若想为每个受击盒单独设置伤害倍率而非按部位分组，关闭检视面板中的 <b>Multiply value per segment?</b> 开关 ➔ 点击受击盒名称展开它 ➔ 设置伤害倍率。");
            DrawServerImage("img-35.png");
        }
    }

    void PlayerDamageDoc()
    {
        if (subStep == 0)
        {
            DrawSuperText("玩家默认会受到武器伤害、坠落伤害和载具碰撞伤害，这些伤害由玩家受击盒接收，或由 bl_PlayerHealthManagerBase 脚本处理。\n \n若要向玩家造成伤害，做法如下：\n \n<?background=#D1D1D1FF><b><size=16>对本地玩家造成伤害</size></b></background>\n \n- 对本地玩家造成伤害可调用 <b>bl_PlayerHealthManagerBase</b> 脚本的 <b>DoDamage(...)</b> 函数，该脚本挂在玩家实例的根节点上。\n \n该函数需要 <b>DamageData</b> 参数，其中包含本次伤害的全部信息。\n \n你还需要拿到目标玩家的引用。获取方式取决于你的伤害是如何触发的。例如用射线检测时，射线命中本地玩家后即可从该玩家取得 <i>bl_PlayerHealthManagerBase</i> 引用，再调用 <b>DoDamage(...)</b> 并传入 <b>DamageData</b>，例如：");
            DrawCodeText("void DealDamageFunction()\n    {\n        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit raycast, 10))\n        {\n            if (raycast.collider.isLocalPlayerCollider())\n            {\n                var playerReferences = raycast.transform.GetComponent<bl_PlayerReferences>();\n \n                DamageData damageData = new DamageData()\n                {\n                    Damage = 20, // base damage to apply\n                    Direction = transform.position,\n                    Cause = DamageCause.Player,\n                    MFPSActor = bl_MFPS.LocalPlayer.MFPSActor, // MFPS actor that cause this damage\n                    // Check the other DamageData properties\n                };\n \n                playerReferences.playerHealthManager.DoDamage(damageData);\n            }\n        }\n    }");
            DrawText("这样就完成了，上例会向目标玩家造成 20 点伤害。\n \n如果你只是想立刻杀死本地玩家，直接调用 <b>bl_MFPS.LocalPlayer</b> 中的 <b>Suicide()</b> 函数即可：");
            DrawCodeText("bl_MFPS.LocalPlayer.Suicide();");
            DrawHorizontalSeparator();
            DrawSuperText("以上方式只适用于本地玩家。若要对远程玩家 <i>即不由本地客户端控制的玩家</i> 造成伤害，做法如下\n\n<?background=#D1D1D1FF><b><size=16>对远程玩家造成伤害</size></b></background>");
            DrawSuperText("对远程玩家造成伤害只能通过其受击盒引用。受击盒是包裹玩家模型的碰撞体，上面挂有继承自 <b>bl_HitBoxBase</b> 的脚本，其中包含 <b>ReceiveDamage(...)</b> 函数，调用它即可造成伤害。\n \n你需要拿到目标远程玩家的受击盒引用。获取方式取决于你希望如何施加伤害。例如通过爆炸造成伤害时，检测爆炸原点一定半径内的碰撞体，伤害函数可以这样实现：");
            DrawCodeText("void DealDamageFunction()\n    {\n        Collider[] hittedColliders = Physics.OverlapSphere(transform.position, 10);\n        foreach (Collider collider in hittedColliders)\n        {\n            // if you want to apply the damage only to players\n            // if (!collider.CompareTag(bl_MFPS.HITBOX_TAG)) continue;\n \n            var damageable = collider.GetComponent<IMFPSDamageable>();\n            if (damageable == null) continue;\n \n            DamageData damageData = new DamageData()\n            {\n                Damage = 50,\n                Direction = transform.position,\n                MFPSActor = bl_MFPS.LocalPlayer.MFPSActor,\n                Cause = DamageCause.Explosion\n                // see the other DamageData properties that you can use.\n            };\n \n            // send the damage to the hit box.\n            damageable.ReceiveDamage(damageData);\n        }\n    }");
        }
        else if (subStep == 1)
        {
            DrawSuperText("<b><?background=#D1D1D1FF><b><size=16>对物体造成伤害</size></b></background></b>\n \n若要对玩家或机器人以外的 GameObject 造成伤害，只需在你的自定义脚本中实现 <b>IMFPSDamageable</b> 接口即可。\n \n假设你有一个油桶，希望被子弹击中时受损，无需改动 MFPS 核心脚本，只要在自定义脚本中实现 <b>IMFPSDamageable</b> 接口并重写 <b>ReceiveDamage(...)</b> 函数，例如：\n \n你的自定义脚本原本可能长这样：");
            DrawCodeText("public class bl_Test : MonoBehaviour\n{\n    public int Health = 100;\n \n    public void ReduceHealth(int damage)\n    {\n        Health -= damage;\n \n        if(Health <= 0)\n        {\n            // Destroy or wherever happens when run out of health\n        }\n    }\n}");
            DrawText("你需要实现 <b>IMFPSDamageable</b> 接口并重写接口函数 <b>ReceiveDamage()</b>，如下所示：");
            DrawCodeText("public class bl_Test : MonoBehaviour, IMFPSDamageable\n{\n\n    public int Health = 100;\n \n    void IMFPSDamageable.ReceiveDamage(DamageData damageData)\n    {\n        ReduceHealth(damageData.Damage);\n    }\n \n    public void ReduceHealth(int damage)\n    {\n        Health -= damage;\n \n        if(Health <= 0)\n        {\n            // Destroy or wherever happens when run out of health\n        }\n    }\n}");
            DrawText("这样就完成了。注意该对象必须带有碰撞体，才能被子弹命中。");
        }
    }

    void CommonQADoc()
    {
        DrawSpoilerBox("机器人会穿墙或穿过物体", "需要在地图场景中烘焙 <b>Navmesh</b>，机器人才知道地图中哪些位置可以通行。\n\n如果你不了解 Navmesh 或不知道如何烘焙，可参考：<link=https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html>https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html</link>");

        DrawSpoilerBox("地图物件在特定区域随机消失。", "原因是你启用了 <b>遮挡剔除</b> 但未在地图场景中烘焙。关于遮挡剔除的更多信息见：<link=https://docs.unity3d.com/Manual/OcclusionCulling.html>https://docs.unity3d.com/Manual/OcclusionCulling.html</link>\n\n烘焙方法见：\n<link=https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html>https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html</link>");

        DrawSpoilerBox("每个房间有玩家数上限吗？", "同一房间能容纳的玩家数没有固定上限。但每增加一名玩家都会增加服务端压力，也会占用本地客户端设备资源。超过一定人数后，游戏会开始出现卡顿，表现为帧率下降和网络延迟升高。\n\n出现性能问题的人数阈值取决于多种因素，例如运行平台、设备配置、网络状况等。\n\n我们用默认的 MFPS 1.5 做过测试以取得参考基准，结果如下：\n\n<b>PC</b> 配置为：\n<i>Intel i7 3.70GHz\n16Gb 内存\nNvidia GTX 1070</i>\n\n同房间 18 名玩家\n中等画质\n平均 60 到 80 帧\n\n<b>移动端</b>：\n使用三星 S8 Plus\n\n同房间 12 名玩家\n使用针对移动端优化过的 MFPS <i>非默认场景</i>\n平均 60 到 75 帧\n\n这些数据可供参考，但影响结果的因素很多，建议自行测试。");

        DrawSpoilerBox("有 Photon 之外的网络方案吗？", "MFPS 提供完整源码，你可以做任何改动，包括接入其他网络方案。\n\n但默认并不存在前端开关 <i>例如一键切换不同库</i> 来实现这件事。\n接入其他网络库需要大量代码改动，把 Photon 的写法替换为对应 SDK 的写法，而且 Photon 的部分方法或功能在你的网络库中可能并不存在，或实现方式不同。\n");

        DrawSpoilerBox("我可以自建独立服务器吗？", "可以。Photon 提供自建服务器方案 <b>Photon OnPremise</b> <i>即 Photon Server</i>。从默认的 Photon PUN 云端切换到它无需改动代码，只需搭建 Photon Server SDK 并在 PhotonServerSettings 中填写 IP。\n\n详见：<link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min>Photon Server 资料</link>\n");

        DrawSpoilerBox("房间对其他玩家不显示？", "与他人联机测试时，你或某位测试者创建的房间有时不会出现在房间或服务器列表中，最常见的原因是你与测试者 <b>不在同一服务器区域</b>。\n\n在 MainMenu 或大厅场景的右下角有一个显示区域名称的下拉框，可用它选择并切换服务器。请确认你与所有测试者连接的是同一服务器区域。");
    }

    void KnownIssuesDoc()
    {
        DrawText("本页列出 MFPS 的已知问题。这里关注的不只是缺陷或错误，也包括我们目前无法修复或规避的问题，以及 <b>影响 MFPS 开发流程的问题</b>。以下回答更偏向个人视角而非官方口径，<b>出自主要开发者 Lovatto</b>。");
        DownArrow();

        DrawText("<b><size=22>抽象层</size></b>\n \n对经验丰富的程序员来说，翻阅 MFPS 源码后很快会注意到的一点是，大多类几乎没有抽象层。这在实现或修改游戏功能时尤其成问题，因为唯一的办法是改动原始脚本，于是又引出另一个问题：你再也无法把 MFPS 升级到新版本，因为升级会覆盖你的改动。\n \n那为什么会这样？为什么不写更整洁、可维护、可扩展、便于继承的代码？\n这个问题需要一些背景，我尽量简短说明：\n \n- MFPS 最初并非作为分发给其他开发者的游戏模板，它原本是一个个人项目，甚至算不上游戏，而是一个学习项目。当时的目标是先跑通功能，代码整洁度往后放。直到 2018 年作为游戏模板发布，核心代码的重构才开始，并且至今仍在进行。每次更新不仅加入新功能，也在用更整洁、支持抽象的代码改善既有功能，让模板更模块化、更便于程序员修改。\n \n当然，这项工作远未结束。我很清楚这一点，但 MFPS 功能太多，要为现有功能提供更好的实现，或者至少在不改变行为的前提下让代码更易推断，工作量很大。而且几乎只有我一个人在写代码，因此重写全部内容需要时间，考虑到我并非全职投入 MFPS，所需时间会更长。");
        Space(10);
        DrawText("<b><size=22>URP、HDRP、TMP、输入系统支持</size></b>\n \nUnity 在不断改进，我们也尽量采用其最新技术。Unity 发布的新特性会在将来取代现有方案，例如新的渲染管线 URP 与 HDRP、Text Mesh Pro、新版输入系统等。\n \n但这些特性大多是在去年年中 <i><size=8><color=#76767694>2020 年</color></size></i> 才脱离预览或测试阶段的。在此之前它们不推荐用于正式项目，因此 MFPS 并未采用。如今它们已可用于生产，正是开始使用的时机，当然这需要一定工作量。由于我当时已在开发 Game Framework Next，就先把这些新特性实现在了它里面 <i><size=8><color=#76767694>指 Game Framework Next</color></size></i>。不过这里讨论的是 Game Framework，这些特性是否也会加入它？我不会承诺全部，但 Text Mesh Pro 与新版输入系统的支持已列入后续更新计划。\n \n那 URP 和 HDRP 呢？\nGame Framework 已经支持它们，只是并非默认，需要手动转换项目。更多信息见本文档的通用渲染管线章节。");
        Space(10);
        DrawText("<b><size=22>所有游戏模式支持机器人</size></b>\n \nMFPS 用户另一个常问的问题是 <b>为什么机器人不是所有游戏模式都支持？</b>\n \n答案可能让你失望：单纯因为工作量太大。\n是的。人工智能本就是棘手的领域，再加上多人联机就更复杂。即便是 MFPS 这类基础机器人，开发也很费功夫。不过 MFPS 核心包确实内置了多人 AI 射击系统，支持两个最常玩的游戏模式。问题在于其余模式都要求机器人按该模式的规则做出定制化行为，这才是复杂且工作量巨大的部分。这让我陷入两难：你可能也能理解，为了持续开发和维护 MFPS，我需要有与投入的工作量和时间相称的收入。而如前所述，AI 开发是我个人觉得最令人沮丧、也最需要时间才能看到进展的方向之一。因此为了收回投入，我只能把它作为插件出售 <i><size=8><color=#76767694>指其他游戏模式的 AI 支持</color></size></i>。\n相比之下，我更愿意把时间放在我认为更合适的其他功能上。\n \n这是否意味着其他游戏模式永远不会支持机器人？\n不是，只是目前还没有排期。");
        Space(10);
        DrawText("<b><size=22>语法</size></b>\n \n容我解释一下。\n从文档、代码和注释中不难看出，英语语法并非我的强项。\n原因很简单：我不是英语母语者。但这不是借口，因为大多数人也不是母语者，出错却没这么多，我明白这一点。不过你可能已经知道 <i>更可能不知道</i>，我来自萨尔瓦多，一个不去谷歌地图上放大中美洲就注意不到的小国。在这里，能提升外语能力的地方实在不多。我已经尽力了，请别怪我。\n \n同时，如果你发现任何语法错误，欢迎反馈，我会非常感激。");
    }

    [MenuItem("游戏框架/教程/文档", false, 111)]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(MFPSGeneralDoc));
    }
}