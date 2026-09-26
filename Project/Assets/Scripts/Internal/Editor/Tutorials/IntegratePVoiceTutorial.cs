using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using MFPSEditor;

public class IntegratePVoiceTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/voice/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "https://assetstorev1-prd-cdn.unity3d.com/key-image/2b5edb30-8595-48e4-9dd9-1913f423b7bd.png", Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},

    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Photon 语音", StepsLenght = 3 },
    };
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/GameFrameworkEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawTutorial();
        }
    }

    void DrawTutorial()
    {
        if (subStep == 0)
        {
            DrawText("MFPS 已支持 Photon Voice 插件，这是 Photon Cloud 专为多人游戏语音聊天提供的另一项服务，可让玩家在游戏内与队友实时通话。\n \n该功能并非支持所有平台，可使用 Photon Voice 的平台如下：\n\n■ Windows\n■ UWP\n■ macOS\n■ Linux\n■ Android（64 位支持请查阅相关说明）\n■ iOS\n■ PlayStation 4（需额外组件）\n■ PlayStation 5（需额外组件）\n■ Nintendo Switch（需额外组件）\n■ MagicLeap（Lumin OS，需额外组件）\n■ HoloLens 2（ARM64 需额外组件）\n■ Xbox One（需额外组件）\n■ Xbox Series X 与 Xbox Series S（需额外组件）");
            DrawImage(GetServerImage(0), TextAlignment.Center);
            DrawText("要使用该功能，需要先导入 Photon Voice 2 包，可在资源商店免费获取，点击下方按钮跳转到该包页面：");
            GUILayout.Space(5);
            if (DrawButton("<color=yellow>打开 Photon Voice 2</color>"))
            {
                AssetStore.Open("content/130518");
                NextStep();
            }
            if (DrawButton("<color=yellow>在浏览器中打开 Photon Voice 2</color>"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/audio/photon-voice-2-130518");
                NextStep();
            }
        }
        else if (subStep == 1)
        {
            DrawText("在资源商店页面下载并导入该包，等待导入完成。");
            DownArrow();
            DrawText("接着需要启用集成代码，前往顶部菜单 游戏框架 -> 扩展 -> 语音 -> <b>启用</b>，等待脚本编译完成。");
            DrawImage(GetServerImage(1));
            DownArrow();
            DrawText("编译完成后重复上述步骤，但这次点击 游戏框架 -> 扩展 -> 语音 -> <b>集成</b> 按钮。");
            DownArrow();
            DrawText("完成，Photon 语音已集成完毕。");
        }
        else if (subStep == 2)
        {
            DrawText("默认语音设置为按键才传输，即按住说话，推荐保持该方式，可在 bl_PlayerVoice.cs 中修改按键" +
                "，该脚本挂载于 Resources 文件夹中每个玩家预制体的根节点");
            DrawImage(GetServerImage(2));
        }
    }

    [MenuItem("游戏框架/教程/Photon 语音")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(IntegratePVoiceTutorial));
    }
}