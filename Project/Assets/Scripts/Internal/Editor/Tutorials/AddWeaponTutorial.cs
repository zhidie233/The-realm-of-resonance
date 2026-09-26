using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class AddWeaponTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-0.jpg", Image = null},
        new NetworkImages{Name = "img-5.png", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-11.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
        new NetworkImages{Name = "img-13.jpg", Image = null},
        new NetworkImages{Name = "img-14.jpg", Image = null},
        new NetworkImages{Name = "img-15.jpg", Image = null},
        new NetworkImages{Name = "img-16.jpg", Image = null},
        new NetworkImages{Name = "img-17.jpg", Image = null},
        new NetworkImages{Name = "img-18.jpg", Image = null},
        new NetworkImages{Name = "img-19.jpg", Image = null},
        new NetworkImages{Name = "img-20.jpg", Image = null},
        new NetworkImages{Name = "img-21.jpg", Image = null},
        new NetworkImages{Name = "img-22.jpg", Image = null},
        new NetworkImages{Name = "img-23.jpg", Image = null},
        new NetworkImages{Name = "img-24.png", Image = null},
        new NetworkImages{Name = "img-25.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "gif-1.gif" },
        new GifData{ Path = "gif-2.gif" },
        new GifData{ Path = "gif-3.gif" },
        new GifData{ Path = "gif-4.gif"},
        new GifData{ Path = "gif-5.gif"},
        new GifData{ Path = "gif-6.gif"},
        new GifData{ Path = "gif-7.gif"},
        new GifData{ Path = "gif-8.gif"},
   };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "武器模型", StepsLenght = 0, DrawFunctionName = nameof(DrawWeaponModel) },
    new Steps { Name = "创建信息", StepsLenght = 3, DrawFunctionName = nameof(DrawCreateInfo) },
    new Steps { Name = "第一人称武器", StepsLenght = 9, DrawFunctionName = nameof(DrawFPWeapon) },
    new Steps { Name = "第三人称武器", StepsLenght = 3, DrawFunctionName = nameof(DrawTPWeapon) },
    new Steps { Name = "拾取预制体", StepsLenght = 2, DrawFunctionName = nameof(DrawPickUpPrefab) },
    new Steps { Name = "导出武器", StepsLenght = 0, DrawFunctionName = nameof(DrawExportWeapons) },
    new Steps { Name = "武器动画", StepsLenght = 0, DrawFunctionName = nameof(AnimateWeaponDoc) },
    };
    //final required////////////////////////////////////////////////

    private GameObject PlayerInstantiated;
    private int animationType = 0;
    public AssetStoreAffiliate weaponList;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/GameFrameworkEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        if (weaponList == null)
        {
            weaponList = new AssetStoreAffiliate();
            weaponList.randomize = true;
            weaponList.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/114132/widget-medium");
            weaponList.FixedHeight = 400;
        }
        allowTextSuggestions = true;
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void DrawWeaponModel()
    {
        if (subStep == 0)
        {
            DrawText("添加新武器当然需要武器的 3D 模型。添加新武器有几种做法，有些人只替换武器模型，沿用 MFPS 默认的手部模型和动画<i>（本质上是把新武器模型摆到手中）</i>。这虽然不算错，但绝非最佳方案，因为 MFPS 的手部模型和动画只是示例占位，换成不同武器模型后动画效果并不理想。\n \n强烈建议使用你自己的模型和动画<i>（包括手臂模型）</i>。可以沿用 MFPS 默认手部，但这样需要为每一把要添加的武器单独做动画；若不擅长动画制作，会相当吃力，因为<b>至少需要 4 个动画：掏出、收起、开火、换弹</b>。\n \n<b>另一种选择</b>，若想省时省力，可以购买兼容 MFPS 的武器模型包，自带所需动画和配套手臂模型。下方提供了这类素材的资源商店合集清单：");
            GUILayout.Space(5);
            Rect r = EditorGUILayout.BeginHorizontal();
            MFPSEditorStyles.DrawBackground(r, new Color(0, 0, 0, 0.3f));
            GUILayout.Space(10);
            weaponList.OnGUI();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("<color=yellow>在线查看</color>", EditorStyles.label))
            {
                Application.OpenURL("https://www.lovattostudio.com/en/weapon-packs-for-mfps/");
            }
        }
    }

    void DrawCreateInfo()
    {
        if (subStep == 0)
        {
            DrawText("添加新武器的第一步是创建武器信息，打开 <b>GameData</b>，在 'AllWeapons' 列表中新增一项。");
            GUILayout.Space(10);
            if (DrawButton("打开游戏数据"))
            {
                bl_GameData gm = bl_GameData.Instance;
                Selection.activeObject = gm;
                EditorGUIUtility.PingObject(gm);
                subStep++;
            }
        }
        else if (subStep == 1)
        {
            DrawText("此时 GameData 应在检视面板中打开，在检视面板底部可以看到 'Weapon' 区段及 'AllWeapons' 列表，展开该列表并<b>新增</b>一项。\n<i>（使用下方按钮）</i>");
            if (DrawButton("自动添加字段"))
            {
                bl_GameData gm = bl_GameData.Instance;
                Selection.activeObject = gm;
                EditorGUIUtility.PingObject(gm);
                bl_GunInfo info = new bl_GunInfo();
                info.Name = "新武器";
                gm.AllWeapons.Add(info);
                subStep++;
            }
            DrawAnimatedImage(0);
        }
        else if (subStep == 2)
        {
            DrawText("此时 'AllWeapons' 列表中会出现名为 <b>'New Weapon'</b> 的新项，展开并按所添加武器类型填写信息。");
            DownArrow();
            DrawPropertieInfo("Name", "string", "这把武器的名称。请为每把武器使用唯一名称，便于区分。");
            DrawPropertieInfo("Type", "enum", "该武器的类型，例如狙击枪、步枪、匕首等。");
            DrawPropertieInfo("Damage", "int", "该武器每次命中造成的伤害值。");
            DrawPropertieInfo("射速", "float", "两次射击之间的最短间隔。");
            DrawPropertieInfo("装弹时间", "float", "装填该武器所需时间。");
            DrawPropertieInfo("Range", "int", "该武器子弹在被销毁前可飞行的最大距离，也是可能命中目标的距离。");
            DrawPropertieInfo("Accuracy", "int", "子弹的散布范围。");
            DrawPropertieInfo("Weight", "int", "该武器的重量。启用这把枪时，重量会影响玩家移动速度");
            DrawPropertieInfo("拾取预制体", "bl_GunPickUp", "该武器的拾取预制体。此处暂时留空，本教程后面会设置。");
            DrawPropertieInfo("枪械图标", "Sprite", "代表该武器的精灵图图标。");
            DownArrow();
            DrawImage(GetServerImage(4));
            GUILayout.Label("武器信息设置完成，可以进入下一步。");
        }
    }

    void DrawFPWeapon()
    {
        if (subStep == 0)
        {
            GUILayout.Label("继续这一步之前，先新建一个空场景 \n这样更清晰。可通过菜单栏 File -> New Scene 新建场景。", EditorStyles.miniLabel);
            GUILayout.Space(10);
            DrawText("在场景层级中实例化或拖入要添加武器的<b>玩家预制体</b><i>（点击下方按钮可自动完成）</i>。\n\n<size=10><b>一把武器只需在一个玩家预制体中集成一次</b>，集成后可将其导出并导入到其他玩家预制体，会自动完成全部配置。<i>（参见导出武器章节）</i></size>\n");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Drag: ", GUILayout.Width(50));
            if (DrawButton("玩家 1"))
            {
                PlayerInstantiated = PrefabUtility.InstantiatePrefab(bl_GameData.Instance.Player1.gameObject) as GameObject;
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.UnpackPrefabInstance(PlayerInstantiated, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
                Selection.activeObject = PlayerInstantiated;
                EditorGUIUtility.PingObject(PlayerInstantiated);
                subStep++;
            }
            GUILayout.Label("Or", GUILayout.Width(25));
            if (DrawButton("玩家 2"))
            {
                PlayerInstantiated = PrefabUtility.InstantiatePrefab(bl_GameData.Instance.Player2.gameObject) as GameObject;
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.UnpackPrefabInstance(PlayerInstantiated, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
                Selection.activeObject = PlayerInstantiated;
                EditorGUIUtility.PingObject(PlayerInstantiated);
                subStep++;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else if (subStep == 1)
        {
            DrawText("找到玩家预制体中所有第一人称武器<i>（FP Weapons）</i>的位置，路径为：<i>Player -> Local -> Mouse -> Animations -> Main Camera -> WeaponCamera -> TilEffect -> WeaponsManager -> *</i>\n");
            if (DrawButton("尝试自动打开"))
            {
                bl_PlayerNetwork player = FindObjectOfType<bl_PlayerNetwork>();
                if (player != null)
                {
                    bl_GunManager gm = player.transform.GetComponentInChildren<bl_GunManager>();
                    Selection.activeObject = gm;
                    EditorGUIUtility.PingObject(gm);
                    subStep++;
                }
            }
        }
        else if (subStep == 2)
        {
            DrawText("'WeaponManager' 对象下已有全部配置好的第一人称武器。为省事，我们复制其中一把来改造，复制的那把应属于同一武器类型。例如新武器是狙击枪就复制狙击枪，是手枪就复制手枪。");
            DrawText("复制方法：在层级中选中该武器 -> 右键 -> Duplicate。");
            // DrawImage(GetServerImage(0));
            DrawAnimatedImage(1);
            DownArrow();
            DrawText("选中复制出来的武器，在 <b>检视</b> 面板中 -> <b>bl_Gun</b> -> <b>Gun ID</b> -> 指定你为该武器创建的武器信息。");
            DrawAnimatedImage(7);
        }
        else if (subStep == 3)
        {
            DrawText("把新武器模型<i>（含手部）</i>拖入复制出来的武器对象内。<b>先不要删除旧模型</b>，暂时禁用它，并把新武器模型摆放为你想要的位置。\n\n摆好位置后，选中武器模型的根节点，在检视面板顶部的 'Layer' 下拉中将层改为 <b>Weapons</b>，并应用到所有子节点。\n");
            // DrawImage(GetServerImage(1));
            DrawAnimatedImage(2);
        }
        else if (subStep == 4)
        {
            DrawText("选中复制出来的武器根节点（挂载 bl_Gun 的那个），单击一次 'FirePoint' 的数值（不是属性名）。这样层级面板会定位到 'FirePoint' 的位置。" +
                 "与 Muzzleflash 对象位于旧模型内部。选中它们，即 FirePoint、Muzzleflash 和 CartridgeEjectEffect，移入你的新模型，并把位置摆放正确，其中 FirePoint " +
                 "与 Muzzleflash 放在枪口末端，然后删除旧模型。");
            DownArrow();
            DrawAnimatedImage(3);
            // DrawImage(GetServerImage(2));
        }
        else if (subStep == 5)
        {
            DrawText("选中新武器模型的根节点，即挂有 Animation 或 Animator 组件的那个，添加脚本 <b>'bl_WeaponAnimation'</b>（点击检视面板的 Add Component 按钮，输入 bl_WeaponAnimation 后点击即可）。");
            DrawAnimatedImage(4);
            DownArrow();
            if (animationType == 0)
            {
                DrawText("现在选择武器模型使用的动画系统：");
                Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Animation 旧版", EditorStyles.toolbarButton))
                {
                    animationType = 1;
                }
                GUILayout.Space(2);
                if (GUILayout.Button("Animator Mecanim", EditorStyles.toolbarButton))
                {
                    animationType = 2;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (animationType == 1)
            {
                DrawText("在脚本的检视面板中需要指定武器模型对应的各段动画\n\n-掏枪 = 掏出动画\n-收枪 = 收起动画\n-瞄准开火：可与普通开火相同" +
                   "动画，但建议使用后坐幅度较小的动画。\n- 若该武器是霰弹枪或狙击枪，且采用分段装弹动画，即开始、装弹、结束三段" +
                   " 只需在 bl_Gun 检视面板中把 <b>Reload Per</b> 下拉框设为 <b>Bullet</b>，然后在 bl_WeaponAnimation 脚本中指定动画即可。" +
                   "\n\n<b>注意：</b>在 bl_WeaponAnimation 中指定的所有动画都应列入 Animation 组件的 " +
                   " Animations 列表中");
                DownArrow();
                DrawImage(GetServerImage(3));
            }
            else if (animationType == 2)
            {
                DrawText("请先确认 Animator 组件没有指定 <b>Controller</b>，若有则移除。\n \n" +
                    " 在 bl_WeaponAnimation 的 AnimationType 中选择 <b>Animator</b>，会看到若干空的动画片段字段。在其中指定武器模型对应的动画：\n\n-Draw 即取出，对应 TakeIn\n-Hide 即收起，对应 Take Out\n-Fire Aim 即瞄准开火，可与普通开火使用同一动画" +
                    " 动画，但建议使用后坐幅度较小的动画。\n- 若该武器是霰弹枪或狙击枪，且采用分段装弹动画，即开始、装弹、结束三段" +
                    "只需在 bl_Gun 检视面板中把 <b>Reload Per</b> 下拉框设为 <b>Bullet</b>，然后在 bl_WeaponAnimation 脚本中指定动画即可。\n \n" +
                    "全部所需动画指定完成后，点击 <b>SetUp</b> 按钮 -> 会弹出窗口，选择项目中用于保存动画控制器的文件夹。");
                Space(5);
                DrawImage(GetServerImage(23));
            }
        }
        else if (subStep == 6)
        {
            DrawText("部分武器包自带行走和奔跑动画，但 MFPS 中这些动作由代码程序化生成，无需使用这些动画。" +
                " 接下来需要在刚才挂载 bl_WeaponAnimation 的同一个对象上，再添加 <b>bl_WeaponMovement.cs</b> 脚本。");
            DrawAnimatedImage(5);
            DownArrow();
            DrawText("然后复制武器模型当前的 Transform 数值。");
            DownArrow();
            DrawImage(GetServerImage(5));
            DownArrow();
            DrawText("在编辑器视图中调整武器对象的位置和旋转，模拟玩家奔跑时武器的状态，<i>建议打开 Game 视图以查看玩家视角下武器的效果。</i>");
            DrawImage(GetServerImage(6));
            DownArrow();
            DrawText("调整到位后，打开 bl_WeaponMovements 检视面板，点击第一个按钮 <b>获取实际位置</b>，它会读取当前 Transform 数值并复制到" +
                "会分别自动填入脚本对应的值。");
            DrawImage(GetServerImage(7));
            DrawText("然后对奔跑换弹位置做同样操作，可与普通奔跑位置相同，点击 获取实际位置 按钮即可。");
            DownArrow();
            DrawText("再回到 Transform 检视面板，打开右键菜单并点击 <b>粘贴组件数值</b>，将 Transform 复位到默认位置。");
            DrawImage(GetServerImage(8));
        }
        else if (subStep == 7)
        {
            DrawText("回到新武器的根节点（挂载 bl_Gun 脚本处），在脚本检视面板中按需修改参数。");
            DrawPropertieInfo("GunID", "enum", "然后选择你先前在 GameData 列表中设置的枪械名称。注意每把武器都需要有各自的武器信息，武器之间不能共用同一份 " +
                "GunID");
            DrawPropertieInfo("瞄准位置", "Vector3", "玩家使用该武器瞄准时武器所处的位置。设置方法：");
            DownArrow();
            DrawImage(GetServerImage(9));
            DownArrow();
            DrawText("点击按钮后，Game 视图中会显示一个准星，作为屏幕中心的参照，并记录默认" +
                " 位置，从而得到瞄准位置。\n\n然后把武器，即挂有 bl_Gun 的那个对象，" +
                " 摆到屏幕中央，让武器的瞄准镜或机瞄与准星 <b>在 Game 视图中</b> 精确对齐，如下所示：");
            DrawImage(GetServerImage(10));
            DownArrow();
            DrawText("确认位置正确后，再次点击该按钮，即可自动指定瞄准位置并复位到默认位置。这部分就完成了。");
            DrawImage(GetServerImage(11));
            DownArrow();
            DrawText("以下是 <b>bl_Gun</b> 脚本可调属性的简要说明\n");
            DrawPropertieInfo("瞄准平滑", "float", "从默认位置过渡到瞄准位置的速度。");
            DrawPropertieInfo("瞄准延迟移动", "float", "瞄准时延迟移动效果的程度。");
            DrawPropertieInfo("瞄准视野", "float", "瞄准时相机的视野，即变焦程度。数值越小变焦越大。");
            DrawPropertieInfo("Bullet", "string", "在此填写该武器发射的池化子弹名称。");
            DrawPropertieInfo("Muzzleflash", "ParticleSystem", "该武器射击时的枪口火焰粒子效果。需为默认实例化的对象。");
            DrawPropertieInfo("Shell", "ParticleSystem", "模拟射击时抛出弹壳的粒子效果。");
            DrawPropertieInfo("冲击力", "int", "子弹击中刚体时施加的作用力");
            DrawPropertieInfo("震屏配置", "Scriptable", "ShakerPresent.cs 的 ScriptableObject，其中包含射击时震屏效果的全部设置");
            DrawPropertieInfo("Recoil", "float", "开火时的枪身后坐幅度。");
            DrawPropertieInfo("后坐恢复速度", "float", "相机从后坐状态恢复的速度。");
            DrawPropertieInfo("自动装弹", "bool", "弹药为 0 时是否自动装弹？");
            DrawPropertieInfo("每匣弹药量", "int", "该武器一个弹匣可装多少发子弹。");
            DrawPropertieInfo("Ammo Per Clip", "int", "该武器默认拥有多少个弹匣。");
            DrawPropertieInfo("装弹方式", "enum", "Bullets 表示逐发装填直至装满，适用于部分霰弹枪与狙击枪；Clip 表示一次性更换整个弹匣。");
            DrawPropertieInfo("开火延迟", "float", "仅用于手雷，从按下输入到投出投射物的延迟时间");
            DrawPropertieInfo("散布范围", "MinMax", "子弹散布的范围，实际数值会在给定的最小值和最大值之间随机取值。");
            DrawPropertieInfo("瞄准散布倍率", "float", "使用该武器瞄准时，散布会乘以该值。0.5 表示正常散布的一半");
            DrawPropertieInfo("每秒散布增量", "float", "持续开火时散布每秒增加的量。");
            DrawPropertieInfo("每秒散布衰减量", "float", "停止开火后散布每秒减少的量。");
            DrawPropertieInfo("按动画播放装弹音效", "bool", "装弹音效是按动画关键事件播放 手动，还是按时间计算触发 自动？");
            DrawPropertieInfo("OnNoAmmoDesactive", "Array", "仅用于手雷。把手中持有的所有投射物对象加入该列表。");
            DownArrow();
            DrawText("最后，若该武器是狙击枪，还需添加 bl_SniperScope.cs 脚本，指定瞄准镜贴图，并在 'OnScopeDisable' 列表中加入狙击模型的所有网格，包括手部。");

        }
        else if (subStep == 8)
        {
            DrawText("最后，在武器的 <b>bl_Gun</b> 检视面板底部可看到 <b>添加到列表</b> 按钮，点击后该武器会自动加入 <b>bl_GunManager</b> 的 <b>AllWeapons</b> 列表。");
            DrawImage(GetServerImage(24));
            DrawText("若该按钮未出现，请确认武器已在列表中：选中 <b>WeaponsManager</b> -> <b>bl_GunManager</b> -> <b>Gun List</b> -> <i>检查武器是否在列表内</i>。\n\n若不在列表中，可手动在列表里新增一项并把新武器拖到该字段上。\n");
            DownArrow();
            DrawImage(GetServerImage(25));
            DownArrow();
            DrawText("若想将其设为某个玩家兵种的默认武器，只需（仍在 bl_GunManager 中）" +
                "展开目标兵种区域，即突击兵、支援兵、侦察兵或工程兵，在需要的槽位中选择，槽位包括主武器、副武器、匕首或投射物，" +
                "选择该武器的名称。");
            DownArrow();
            DrawImage(GetServerImage(13));
            DownArrow();
            DrawText("接着保存或应用对玩家预制体的修改（暂时不要从场景中删除玩家，下一步还要用到）。");
            DownArrow();
            DrawText("完成，你已添加了一把新的第一人称武器。\n\n若想更进一步，为玩家提供在所有可用武器中自选配装的菜单，可使用 <b>兵种自定义</b> 扩展。");
            if (DrawButton("兵种自定义"))
            {
                Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/class-cutomization/");
            }
        }
    }

    void DrawTPWeapon()
    {
        if (subStep == 0)
        {
            DrawText("每把武器有两个视角：<b>第一人称视角</b>是本机玩家看到的，<b>第三人称视角</b>是其他玩家看到的。本部分配置后者，即<b>第三人称武器</b>。所需的模型与第一人称武器相同，但不含手部或手臂，只保留武器本体网格。不过<b>建议使用比第一人称模型更精简、面数更低的模型。</b>");
            DrawImage(GetServerImage(14));
            DownArrow();
            DrawText("若你是从上一步继续，场景中应仍有玩家预制体；若没有，请把添加了第一人称武器的那个玩家拖入场景层级。\n\n然后打开 <i><b>bl_PlayerNetwork</b></i> 检视面板，点击 <b>Network Guns</b> 列表中的某个脚本，层级会展开到所有第三人称武器的位置。把武器模型拖入 <b>RemoteWeapons</b> 对象中。");
            DrawImage(GetServerImage(15));
            DownArrow();
            DrawText("调整武器对象的位置，模拟玩家用手握住武器的样子。\n\n若你的玩家模型处于 T-Pose 或某个姿势，" +
                "难以把武器摆到手上时，可以打开 <b>Animation</b> 窗口，选择 idle 待机动画并跳到其中某一帧。这样" +
                "玩家会处于该动画姿态，你就比较容易摆放枪械了。");
            DrawImage(GetServerImage(17));
        }
        else if (subStep == 1)
        {
            DrawText("选中刚拖入玩家的那个武器模型，添加脚本 <b>bl_NetworkGun.cs</b>");
            DownArrow();
            DrawText("在脚本检视面板中可看到一个名为 'Local Weapon' 的空字段，以及一个列出该玩家预制体中所有第一人称武器（bl_Gun）的下拉框。" +
                "按对象名选中你先前设置好的第一人称武器，然后点击 Select 按钮。");
            DrawImage(GetServerImage(16));
            DownArrow();
            DrawText("点击该按钮会自动指定所选的第一人称武器，脚本检视面板中会出现一些新的变量：");
            DrawPropertieInfo("MuzzlefFlash", "粒子系统", "开火粒子效果。");
            DownArrow();
            DrawText("枪口火焰方面，把你的粒子特效拖入武器对象内；若没有自己的，MFPS 默认特效位于：<i>MFPS->" +
                "Content->Prefabs->Particles->WeaponsEffects->Prefabs->MuzzleFlashEffect</i>。把它拖入层级，放进武器内部并摆到枪口末端。" +
                "然后在 MuzzleFlash 字段中指定它。");
            DownArrow();
            DrawText("对于手雷，检视面板会多出一个名为 'Bullet' 的字段，需把 手雷预制体 拖入其中。");

        }
        else if (subStep == 2)
        {
            DrawText("脚本检视面板中有一个 <b>'设置手部 IK'</b> 按钮，点击后会打开一个小编辑器窗口，" +
                "Scene 视图。你会看到选中了一个球形辅助图标。移动它时左手会跟随运动，受 IK 约束。因此把它移动和旋转到 " +
                "左手应在的位置，让持枪姿势更真实。\n 摆好之后，点击先前打开的小窗口中的 DONE 按钮。");
            DownArrow();
            DrawImage(GetServerImage(18));
            DownArrow();
            DrawText("最后点击检视面板中的 登记第三人称武器 按钮，即可将该武器自动加入网络武器列表。");
            DrawImage(GetServerImage(19));
            DownArrow();
            DrawText("完成，第三人称武器已添加。别忘了保存或应用对玩家预制体的修改。");
        }
    }

    void DrawPickUpPrefab()
    {
        if (subStep == 0)
        {
            DrawText("最后需要配置的是武器的拾取预制体。当玩家拾取其他武器，或玩家携带该武器死亡时，会实例化这个预制体。" +
                "这里需要的是武器模型。与设置第三人称武器相同，只需不带手的武器模型网格即可。同样建议使用低多边形模型。");
            DownArrow();
            DrawText("把武器模型拖入层级面板（这一步不需要玩家预制体）。选中它并添加以下组件：\\n\\n-<b>RigidBody</b>\\n-<b>Sphere Collider</b>：在球体碰撞体中勾选" +
                " 'IsTrigger'，该碰撞体是枪械的检测范围，玩家进入即触发，必要时可调整位置和半径。\n-<b>Box Collider</b>：" +
                " 取消勾选 'IsTrigger'，并让该碰撞体的边界与武器网格完全贴合。\n\n检视面板应大致如下：");
            DrawImage(GetServerImage(20));
            DownArrow();
            DrawText("碰撞体边界应大致如下：");
            DownArrow();
            DrawImage(GetServerImage(21));

        }
        else if (subStep == 1)
        {
            DrawText("然后添加脚本 <b>bl_GunPickUp.cs</b> 并配置各变量");
            DownArrow();
            DrawPropertieInfo("GunID", "enum", "选择该武器的武器 ID，即你在 GameData 中设置的那个");
            DrawPropertieInfo("Bullets", "int", "他人拾取时该武器包含的弹药量");
            DrawPropertieInfo("DestroyAfterTime", "bool", "该预制体在实例化后是否经过一段时间自动销毁？");
            DownArrow();
            DrawText("很好，现在为这把武器创建预制体。把对象从层级面板拖到 Project 窗口的某个文件夹即可，记得拖到了哪个文件夹 :)");
            DownArrow();
            DrawText("最后打开 GameData（点击下方按钮）");
            if (DrawButton("打开游戏数据"))
            {
                bl_GameData gm = bl_GameData.Instance;
                Selection.activeObject = gm;
                EditorGUIUtility.PingObject(gm);
            }
            DownArrow();
            DrawText("在 GameData 检视面板中打开 'AllWeapons' 列表，展开之前配置好的武器信息。在 'Pick Up Prefab' 字段中把武器的拾取预制体（Project 文件夹中的那个）拖入，" +
                "不要从层级中拖：");
            DrawImage(GetServerImage(22));
            DownArrow();
            DrawText("大功告成，新武器集成完毕 :) 第一次操作可能显得很复杂，但下次会轻松很多。");
        }
    }

    void DrawExportWeapons()
    {
        DrawText("武器系统提供了一个实用功能，即武器配置的<b>导出</b>与<b>导入</b>。这里所说的武器配置指某个玩家预制体中武器的第一人称武器、第三人称武器、枪械信息、位置、旋转等数据。\n\n当你需要把武器从一个玩家预制体迁移到另一个，甚至迁移到另一个 Unity 工程时，这个功能尤其有用。例如，你刚在玩家预制体 1 中集成了新武器，也想在玩家预制体 2 中集成它；既然在玩家 1 中已配置完成，就不必在玩家 2 中重复全部步骤，只需从玩家 1 导出配置再导入玩家 2 即可，这样一把武器只需集成一次，很省事。\n\n言归正传，第一步是从已集成该武器的玩家预制体中导出武器。打开该玩家预制体，拖入场景层级或在预制体场景中打开均可，然后选中第一人称武器（位于 WeaponManager 下），在 bl_Gun.cs 检视面板顶部可看到 <b>导出</b> 按钮，点击它。随后会弹出一个小窗口，点击其中的 <b>导出武器</b> 按钮，接着弹出对话框选择导出武器的保存文件夹，选择你希望保存的位置即可。\n");
        DrawAnimatedImage(6);

        DrawText("导出完成后，就可以导入到任意玩家预制体了。打开要导入武器的玩家预制体，这次选中 <b>WeaponManager</b> 对象，在 bl_GunManager 检视面板中点击右上角的 <b>导入</b> 按钮。随后会打开新窗口，其中有一个 <b>待导入武器</b> 空字段，把你刚保存的 <b>导出武器</b> 预制体拖入该字段，点击 <b>导入</b> 按钮即可。\n\n这样该武器就完整集成到这个玩家预制体中了。\n");
    }

    void AnimateWeaponDoc()
    {
        DrawText("关于武器最常见的问题之一是如何为武器制作动画。如前所述，要在 MFPS 中为第一人称视角集成一把新武器，你需要：\n \n•  手臂或手部模型\n•  武器模型\n•  4 个动画<b>（掏出、收起、开火、换弹）</b>\n \n无论你使用 MFPS 示例手臂模型还是自定义模型，都需要自己或由美术为它们和新武器模型制作动画；<b>这一过程与 MFPS 无关</b>，简单来说就是<b>按你为游戏中任何其他对象或模型做动画的方式来做即可</b>。MFPS 对武器动画的制作方式没有任何限制，你可以在任何顺手的工具中制作，Unity 内部或 Blender、Maya、3ds Max 等第三方软件皆可。\n \n对于没有相关经验的新手，这里提供一份简易入门指南：");
        Space(10);
        DrawHyperlinkText("<b><size=22>在 UNITY 内制作</size></b>\n\n若想在 Unity 内制作动画以便快速原型开发和高效迭代，Unity 内置动画系统很方便。但对于本例这类骨骼较多、需要用到反向动力学和约束的复杂情况，内置方案未必最佳。此时可使用资源商店中的外部编辑器工具 \"UMotion\"，它有免费版本，可在此查看：<link=https://assetstore.unity.com/packages/tools/animation/umotion-community-animation-editor-95986?aid=1101lJFi>UMotion Pro</link>\n\n以及其免费版：<link=https://assetstore.unity.com/packages/tools/animation/umotion-community-animation-editor-95986?aid=1101lJFi>UMotion Community</link>");
        DrawText("以下视频展示了如何用 UMotion 为第一人称武器制作动画：");
        DrawYoutubeCover("第一人称视角 - UMotion 实战", GetServerImage("https://img.youtube.com/vi/nZPWVPYw41Y/0.jpg"), "https://www.youtube.com/watch?v=nZPWVPYw41Y&ab_channel=SoxwareInteractive");
        Space(10);
        DrawHyperlinkText("<b><size=22>动画软件</size></b>\n \n更进阶的方案，也是美术更常用的做法，是使用专门从事建模和动画的第三方软件，但这类软件学习曲线更长，需要一定练习才能上手。以下是几款最流行的软件：\n \n<link=https://www.blender.org/download/>Blender（免费）</link>\n<link=https://www.autodesk.co.uk/products/maya/free-trial>Maya（付费，或学生版免费）</link>\n<link=https://www.autodesk.co.uk/products/3ds-max/free-trial>3ds Max（付费，或学生版免费）</link>\n\n以下是一些为第一人称武器制作动画的实用教程：");

        DrawYoutubeCover("优秀的第一人称动画是如何制作的", GetServerImage("https://img.youtube.com/vi/dclA9iwZB_s/0.jpg"), "https://www.youtube.com/watch?v=dclA9iwZB_s&ab_channel=CGCookie");
        DrawYoutubeCover("如何在 Blender 2.8+ 中制作第一人称动画", GetServerImage("https://img.youtube.com/vi/IV6XP-EDzw8/0.jpg"), "https://www.youtube.com/watch?v=IV6XP-EDzw8&ab_channel=thriftydonut");
        DrawYoutubeCover("如何在 Blender 2.8 中为第一人称手臂与枪械创建动画及动画组", GetServerImage("https://img.youtube.com/vi/DWOWdZf8MDA/0.jpg"), "https://www.youtube.com/watch?v=DWOWdZf8MDA&ab_channel=SaqibHussain");
    }

    [MenuItem("游戏框架/教程/添加武器", false, 500)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddWeaponTutorial));
    }
}