Shader "HDRP/AdditiveGeneric"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/Common.hlsl"
    ENDHLSL
    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "Queue" = "Transparent" }
        Pass
        {
            Name "Additive"
            HLSLPROGRAM
#include "Lightmapped/SourceCG.hlsl"
#pragma raytracing
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv       : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            float _Metallic;
            float _Smoothness;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformWorldToHClip(input.positionOS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                float3 normalWS = UnpackNormal(input.normalWS, _NormalMap, sampler_NormalMap, input.uv);
                half3 color = LitSurface(albedo, normalWS, _Metallic, _Smoothness);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
