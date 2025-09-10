Shader "HDRP/{name}HDRP"
{{
    Properties
    {{
        _MainTex("Base Texture", 2D) = "white" {{}}
        _Color("Tint", Color) = (1,1,1,1)
    }}
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/Common.hlsl"
    ENDHLSL
    SubShader
    {{
        Tags {{ "RenderPipeline"="HDRenderPipeline" "Queue"="Geometry" }}
        Pass
        {{
            Name "Forward"
            HLSLPROGRAM
#include "Lightmapped/SourceCG.hlsl"
#pragma raytracing
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes {{ float3 posOS:POSITION; float2 uv: TEXCOORD0; }};
            struct Varyings {{ float4 posH:SV_POSITION; float2 uv:TEXCOORD0; }};

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _Color;

            Varyings Vert(Attributes IN)
            {{
                Varyings OUT;
                OUT.posH = TransformWorldToHClip(IN.posOS);
                OUT.uv = IN.uv;
                return OUT;
            }}

            half4 Frag(Varyings IN):SV_Target
            {{
                float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                return albedo;
            }}
            ENDHLSL
        }}
    }}
}}