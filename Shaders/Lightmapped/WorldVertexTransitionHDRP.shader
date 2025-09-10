Shader "HDRP/{name}HDRP"
{{
    Properties
    {{
        _MainTex("Texture", 2D) = "white" {{}}
        _TimeScale("Time Scale", Float) = 1
    }}
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/Common.hlsl"
    ENDHLSL
    SubShader
    {{
        Tags {{ "RenderPipeline"="HDRenderPipeline" "Queue"="Geometry" }}
        Pass
        {{
            Name "VertexTransition"
            HLSLPROGRAM
#include "Lightmapped/SourceCG.hlsl"
#pragma raytracing
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes {{ float3 posOS:POSITION; float2 uv: TEXCOORD0; float3 normalOS:NORMAL; }};
            struct Varyings {{ float4 posH:SV_POSITION; float2 uv:TEXCOORD0; }};

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float _TimeScale;

            Varyings Vert(Attributes IN)
            {{
                Varyings OUT;
                float3 offset = IN.normalOS * sin(_TimeScale * _Time.y) * 0.1;
                OUT.posH = TransformWorldToHClip(IN.posOS + offset);
                OUT.uv = IN.uv;
                return OUT;
            }}

            half4 Frag(Varyings IN):SV_Target
            {{
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
            }}
            ENDHLSL
        }}
    }}
}}