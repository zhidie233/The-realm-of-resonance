using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Reflection;
using Resonance.Foundation;
using Resonance.Core;
namespace Resonance.Actors
{

    /// <summary>
    /// 把 EventBus 的后处理开关映射到 URP：Bloom、色差、动态模糊走 Volume 覆盖，
    /// 环境光遮蔽走 URP 渲染器上的 ScreenSpaceAmbientOcclusion 特性。
    /// </summary>
    public class PostProcessEffects : MonoBehaviour
    {
        Volume volume;
        ScriptableRendererData rendererData;
        ScriptableRendererFeature ssaoFeature;

        private void Awake()
        {
            volume = GetComponent<Volume>();
            ResolveSSAO();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PostEffectChangedEvent>(OnEvt_PostEffect);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PostEffectChangedEvent>(OnEvt_PostEffect);
        }

        private void OnEvt_PostEffect(PostEffectChangedEvent e) =>
            OnPostEffect(e.ChromaticAberration, e.AntiAliasing, e.Bloom, e.Ssao, e.MotionBlur);

        private void Start()
        {
            DynamicGI.UpdateEnvironment();
        }

        void OnPostEffect(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
        {
            if (volume != null)
            {
                var profile = volume.profile;
                if (profile != null)
                {
                    if (profile.TryGet<ChromaticAberration>(out var chromaticAberration))
                        chromaticAberration.active = chrab;
                    if (profile.TryGet<Bloom>(out var bloomEffect))
                        bloomEffect.active = bloom;
                    if (profile.TryGet<MotionBlur>(out var motionBlurEffect))
                        motionBlurEffect.active = motionBlur;
                }
            }

            SetSSAO(ssao);
        }

        void ResolveSSAO()
        {
            if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset pipeline))
                return;

            var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            if (!(field?.GetValue(pipeline) is ScriptableRendererData[] rendererDataList) || rendererDataList.Length == 0)
                return;

            rendererData = rendererDataList[0];
            ssaoFeature = rendererData.rendererFeatures
                .FirstOrDefault(feature => feature != null && feature.GetType().Name == "ScreenSpaceAmbientOcclusion");
        }

        void SetSSAO(bool enabled)
        {
            if (ssaoFeature == null || ssaoFeature.isActive == enabled)
                return;

            ssaoFeature.SetActive(enabled);
            rendererData.SetDirty();
        }
    }
}
