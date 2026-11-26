Shader "Resonance/WaterURP"
{
    Properties
    {
        _Depth("Depth", Float) = 1
        _Strength("Strength", Range(0, 2)) = 0
        _DepthColor("DepthColor", Color) = (0.4575472,0.8941401,1,0.6)
        _ShallowColor("ShallowColor", Color) = (0.365655,0.8946645,0.9339623,0.4901961)
        _MainPattern("MainPattern", 2D) = "bump" {}
        _SecondaryPattern("SecondaryPattern", 2D) = "bump" {}
        _PatternStreght("PatternStreght", Range(0, 1)) = 0.5
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        _Layer1Speed("Layer 1 Speed", Float) = 85
        _Layer2Speed("Layer 2 Speed", Float) = -120
        _Displassment("Displassment", Float) = 0
        _NoiseScale("NoiseScale", Float) = 20
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "UniversalMaterialType" = "Lit"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Depth;
                float _Strength;
                half4 _DepthColor;
                half4 _ShallowColor;
                float4 _MainPattern_ST;
                float4 _SecondaryPattern_ST;
                float _PatternStreght;
                float _Smoothness;
                float _Layer1Speed;
                float _Layer2Speed;
                float _Displassment;
                float _NoiseScale;
            CBUFFER_END

            TEXTURE2D(_MainPattern);
            SAMPLER(sampler_MainPattern);
            TEXTURE2D(_SecondaryPattern);
            SAMPLER(sampler_SecondaryPattern);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
                float fogFactor : TEXCOORD6;
            };

            float3 mod2D289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float2 mod2D289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float3 permute(float3 x) { return mod2D289(((x * 34.0) + 1.0) * x); }

            float snoise(float2 v)
            {
                const float4 C = float4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
                float2 i = floor(v + dot(v, C.yy));
                float2 x0 = v - i + dot(i, C.xx);
                float2 i1;
                i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float4 x12 = x0.xyxy + C.xxzz;
                x12.xy -= i1;
                i = mod2D289(i);
                float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
                float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
                m = m * m;
                m = m * m;
                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;
                m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
                float3 g;
                g.x = a0.x * x0.x + h.x * x0.y;
                g.yz = a0.yz * x12.xz + h.yz * x12.yw;
                return 130.0 * dot(m, g);
            }

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                float2 noiseUV = v.uv + (_Time.y / 100.0).xx;
                float n = snoise(noiseUV * _NoiseScale);
                float3 positionOS = v.positionOS.xyz + float3(0.0, _Displassment * n, 0.0);

                VertexPositionInputs pos = GetVertexPositionInputs(positionOS);
                VertexNormalInputs nrm = GetVertexNormalInputs(v.normalOS, v.tangentOS);

                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.screenPos = ComputeScreenPos(pos.positionCS);
                o.uv = v.uv;
                o.normalWS = nrm.normalWS;
                o.tangentWS = nrm.tangentWS;
                o.bitangentWS = nrm.bitangentWS;
                o.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthFactor = saturate((eyeDepth - (i.screenPos.w + _Depth)) * _Strength);

                float2 uv1 = i.uv * _MainPattern_ST.xy + _MainPattern_ST.zw + (_Time.y / _Layer1Speed).xx;
                float2 uv2 = i.uv * _SecondaryPattern_ST.xy + _SecondaryPattern_ST.zw + (_Time.y / _Layer2Speed).xx;

                half3 n1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_MainPattern, sampler_MainPattern, uv1), 1.0);
                half3 n2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_SecondaryPattern, sampler_SecondaryPattern, uv2), 1.0);
                half normalScale = lerp(0.0h, _PatternStreght, depthFactor);
                half3 normalTS = normalize(half3((n1.xy + n2.xy) * 0.5h * normalScale, 1.0h));

                half4 waterColor = lerp(_ShallowColor, _DepthColor, depthFactor);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = waterColor.rgb;
                surfaceData.specular = 0;
                surfaceData.metallic = 0;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = 0;
                surfaceData.occlusion = 1;
                surfaceData.alpha = waterColor.a;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;

                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                half3x3 tangentToWorld = half3x3(i.tangentWS, i.bitangentWS, i.normalWS);
                inputData.normalWS = NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, tangentToWorld));
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(i.positionWS));
                inputData.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                inputData.fogCoord = i.fogFactor;
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, i.fogFactor);
                return half4(color.rgb, waterColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
