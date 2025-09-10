Shader "HDRP/{name}HDRP"
{{
    Properties
    {{
        _BaseTex("Base Texture", 2D) = "white" {{}}
        _BlendTex("Blend Texture", 2D) = "white" {{}}
        _BlendMask("Blend Mask", 2D) = "white" {{}}
        _BlendScale("UV Scale", Float) = 10
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
            struct Varyings {{ float4 posH:SV_POSITION; float2 uv0:TEXCOORD0; float2 uv1:TEXCOORD1; float2 uvMask:TEXCOORD2; }};

            TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);
            TEXTURE2D(_BlendTex); SAMPLER(sampler_BlendTex);
            TEXTURE2D(_BlendMask); SAMPLER(sampler_BlendMask);
            float _BlendScale;

            Varyings Vert(Attributes IN)
            {{
                Varyings OUT;
                OUT.posH = TransformWorldToHClip(IN.posOS);
                OUT.uv0 = IN.uv;
                OUT.uv1 = IN.uv * _BlendScale;
                OUT.uvMask = IN.uv;
                return OUT;
            }}

            half4 Frag(Varyings IN):SV_Target
            {{
                float4 c0 = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, IN.uv0);
                float4 c1 = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, IN.uv1);
                float mask = SAMPLE_TEXTURE2D(_BlendMask, sampler_BlendMask, IN.uvMask).r;
                return lerp(c0, c1, mask);
            }}
            ENDHLSL
        }}
    }}
}}