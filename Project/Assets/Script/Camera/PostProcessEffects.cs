#define USE_PPV2 // comment this line to stop using PostProcess references.
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2 && USE_PPV2
using UnityEngine.Rendering.PostProcessing;
#endif

/// <summary>
/// 如果控制台报错指向本脚本
/// 多半是因为项目没有导入 Post Processing 包。
/// 如果是刻意不用它，按以下方式消除报错：Unity Player Settings -> Other Settings -> Script Define Symbols ->
/// 在输入框里找到并删除这个字符串：UNITY_POST_PROCESSING_STACK_V2; -> 回车/提交。
/// 如果并非刻意，去 Window -> Package Manager -> 在左侧面板找到 'Post Processing' 包 -> Import 导入它。
/// </summary>
public class PostProcessEffects : MonoBehaviour
{
#if UNITY_POST_PROCESSING_STACK_V2 && USE_PPV2
    public PostProcessProfile processProfile;

    private void Awake()
    {
        var sceneVolume = GetComponent<PostProcessVolume>();
        if (sceneVolume != null) sceneVolume.profile = processProfile;
    }

    private void OnEnable()
    {
        GameEvents.onEffectChange += OnPostEffect;
    }

    private void OnDisable()
    {
        GameEvents.onEffectChange -= OnPostEffect;
    }

    void OnPostEffect(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
    {
        if (processProfile == null) return;

        if (processProfile.HasSettings(typeof(ChromaticAberration)))
        {
            processProfile.GetSetting<ChromaticAberration>().active = chrab;
        }
        if (processProfile.HasSettings(typeof(Bloom)))
        {
            processProfile.GetSetting<Bloom>().active = bloom;
        }
        if (processProfile.HasSettings(typeof(AmbientOcclusion)))
        {
            processProfile.GetSetting<AmbientOcclusion>().active = ssao;
        }
        if (processProfile.HasSettings(typeof(MotionBlur)))
        {
            processProfile.GetSetting<MotionBlur>().active = motionBlur;
        }
    }
#endif
}