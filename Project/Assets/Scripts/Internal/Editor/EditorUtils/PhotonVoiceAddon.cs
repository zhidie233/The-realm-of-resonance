using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using MFPSEditor;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif

public class PhotonVoiceAddon : MonoBehaviour
{

    private const string DEFINE_KEY = "PVOICE";

#if !PVOICE
    [MenuItem("游戏框架/扩展/语音/启用")]
    private static void Enable()
    {
        bl_GameData.Instance.UseVoiceChat = true;
        EditorUtility.SetDirty(bl_GameData.Instance);
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if PVOICE
    [MenuItem("游戏框架/扩展/语音/禁用")]
    private static void Disable()
    {
        bl_GameData.Instance.UseVoiceChat = false;
        EditorUtility.SetDirty(bl_GameData.Instance);
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("游戏框架/扩展/语音/集成")]
    private static void Instegrate()
    {

#if PVOICE
        //setup the player 1
        SetUpPlayerPrefab(bl_GameData.Instance.Player1.gameObject);
        SetUpPlayerPrefab(bl_GameData.Instance.Player2.gameObject);


#if PSELECTOR
        foreach(var p in MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers)
        {
            if (p.Prefab == null ) continue;
         SetUpPlayerPrefab(p.Prefab.gameObject);
        }
#endif

        if (AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            string path = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bl_Lobby lb = FindObjectOfType<bl_Lobby>();
            if (lb != null)
            {
                GameObject old = GameObject.Find("PhotonVoice");
                if (old != null)
                {
                    DestroyImmediate(old);
                    Debug.Log("已移除旧的语音设置");
                }
                if (FindObjectOfType<PunVoiceClient>() == null)
                {
                    GameObject nobj = new GameObject("PhotonVoice");
                    var pvs = nobj.AddComponent<PunVoiceClient>();
                    pvs.AutoConnectAndJoin = true;
                    pvs.ApplyDontDestroyOnLoad = true;
                    pvs.KeepAliveInBackground = 5000;
                    pvs.ApplyDontDestroyOnLoad = true;

                    Recorder r = nobj.AddComponent<Recorder>();
                    r.MicrophoneType = Recorder.MicType.Unity;
                    r.TransmitEnabled = true;
                    r.VoiceDetection = true;
                    r.DebugEchoMode = false;

                    pvs.PrimaryRecorder = r;
                    nobj.AddComponent<bl_PhotonAudioDisabler>().isGlobal = true;
                    EditorUtility.SetDirty(nobj);
                    EditorUtility.SetDirty(pvs);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                Debug.Log("Photon 语音已集成，请在 GameData 中启用它。");
            }
            else
            {
                Debug.Log("未找到 Menu 场景。");
            }
        }
        else
        {
            Debug.LogWarning("无法自动完成扩展集成，因为 MFPS 目录结构已被修改，请手动集成。");
        }
#else
        Debug.LogWarning("请先启用 Photon 语音扩展再执行集成。");
#endif
    }

#if PVOICE
    static void SetUpPlayerPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject p1 = prefab;
        PhotonVoiceView pvv = p1.GetComponent<PhotonVoiceView>();
        if (pvv == null)
        {
            pvv = p1.AddComponent<PhotonVoiceView>();
        }
        Speaker speaker = p1.GetComponent<Speaker>();
        if (speaker == null)
        {
            speaker = p1.AddComponent<Speaker>();
        }
        EditorUtility.SetDirty(p1);
    }
#endif

    [MenuItem("游戏框架/扩展/语音/安装包")]
    private static void OpenPackagePage()
    {
        AssetStore.Open("content/130518");
    }

    [MenuItem("游戏框架/工具/修复编译符号")]
    private static void FixDefineSymbols()
    {
        bool defines = EditorUtils.CompilerIsDefine("LM");
        if (!defines)
        {
            EditorUtils.SetEnabled(DEFINE_KEY, false);
        }
    }

#if UNITY_POST_PROCESSING_STACK_V2
    [MenuItem("游戏框架/工具/删除后处理")]
    private static void DeletePP()
    {
        UnityEditor.PackageManager.Client.Remove("com.unity.postprocessing");
        EditorUtils.SetEnabled("UNITY_POST_PROCESSING_STACK_V2", false);
    }
#endif

}