Shader "HDRP/Custom/AdvancedLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "bump" {}
        _MaskMap ("Mask (Metal R | AO G | Smooth B | Emit A)", 2D) = "black" {}
        _ParallaxMap("Height", 2D) = "gray" {}
        _ParallaxScale("Parallax Height", Range(0,0.10)) = 0.02
        _DetailMap ("Detail", 2D) = "gray" {}
        _DetailScale("Detail Scale", Range(0,8)) = 4
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
    }

    HLSLINCLUDE
#ifndef unity_ObjectToWorld
#define unity_ObjectToWorld UNITY_MATRIX_M
#endif
#ifndef UNITY_MATRIX_M
#define UNITY_MATRIX_M unity_ObjectToWorld
#endif

#include "USource_HDRPCompat.cginc"
        #pragma target 5.0
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma only_renderers d3d11 ps5 xboxone xboxseries vulkan metal
        #pragma multi_compile _ _ENABLE_PARALLAX
        #pragma multi_compile _ _ENABLE_DETAIL
        #pragma multi_compile _ _ENABLE_RIM

        // Core + HDRP includes (paths valid for HDRP >= 17)
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"

        struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            float4 tangentOS  : TANGENT;
            float2 uv0        : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv0        : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS   : TEXCOORD2;
            float4 tangentWS  : TEXCOORD3;
        };

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _RimColor;
            float  _RimPower;
            float  _ParallaxScale;
            float  _DetailScale;
        CBUFFER_END

        TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);   SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MaskMap);     SAMPLER(sampler_MaskMap);
        TEXTURE2D(_ParallaxMap); SAMPLER(sampler_ParallaxMap);
        TEXTURE2D(_DetailMap);   SAMPLER(sampler_DetailMap);

        Varyings Vert (Attributes IN)
        {
            Varyings output;
                output = (Varyings)0;
            float3 posWS  = TransformObjectToWorld(IN.positionOS);
            float3 nrmWS  = TransformObjectToWorldNormal(IN.normalOS);
            float4 tanWS  = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);

            o.positionCS = TransformWorldToHClip(posWS);
            o.positionWS = posWS;
            o.normalWS   = nrmWS;
            o.tangentWS  = tanWS;
            o.uv0        = IN.uv0;
            return o;
        }

        void Frag (Varyings IN, out half4 outColor : SV_Target0)
        {
            float2 uv = IN.uv0;

            #ifdef _ENABLE_PARALLAX
                float height = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, uv).r;
                float3 viewDirTS = mul((float3x3)UNITY_MATRIX_IT_MV,
                                        normalize(IN.positionWS - _WorldSpaceCameraPos)).xyz;
                uv += viewDirTS.xy * (_ParallaxScale * (height - 0.5));
            #endif

            float4  baseSample   = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
            float4  maskSample   = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);

            #ifdef _ENABLE_DETAIL
                float4 detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uv * _DetailScale);
                baseSample.rgb = lerp(baseSample.rgb,
                                      baseSample.rgb * detail.rgb * 2,
                                      detail.a);
            #endif

            half3 baseCol   = baseSample.rgb * _BaseColor.rgb;
            half  metallic  = maskSample.r;
            half  occlusion = maskSample.g;
            half  smooth    = maskSample.b;
            half  emitMask  = maskSample.a;

            half3 nrmTS = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(_NormalMap,
                                                                 sampler_NormalMap, uv));
            half3 nrmWS = TransformTangentToWorld(nrmTS, IN.normalWS,
                                                  IN.tangentWS.xyz, IN.tangentWS.w);

            SurfaceData surfaceData = (SurfaceData)0;
            InitializeStandardLitSurfaceData(baseCol, metallic, smooth,
                                             nrmWS, occlusion, surfaceData);

            #ifdef _ENABLE_RIM
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float  rim     = saturate(1.0 - dot(viewDir, IN.normalWS));
                surfaceData.emissiveColor += _RimColor.rgb * pow(rim, _RimPower);
            #endif

            surfaceData.emissiveColor += baseCol * emitMask;

            BSDFData bsdf;
            InitializeStandardLitBSDFData(surfaceData, bsdf);

            outColor = float4(EvaluateBSDF_Pbr(bsdf, IN.positionWS,
                                               normalize(_WorldSpaceCameraPos - IN.positionWS)).rgb, 1);
        }
    ENDHLSLINCLUDE

    SubShader
    {
        Tags{ "RenderPipeline"="HDRenderPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "Forward" }
            HLSLPROGRAM
#ifndef unity_ObjectToWorld
#define unity_ObjectToWorld UNITY_MATRIX_M
#endif
#ifndef UNITY_MATRIX_M
#define UNITY_MATRIX_M unity_ObjectToWorld
#endif

#include "USource_HDRPCompat.cginc"
            #define SHADERPASS SHADERPASS_FORWARD
            ENDHLSL
        }
    }
    Fallback "HDRenderPipeline/Lit"
}