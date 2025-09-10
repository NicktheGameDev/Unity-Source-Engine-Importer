
Shader "HDRP/UnlitGeneric"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap   ("Base Map", 2D) = "white" {}
        _Cutoff    ("Alpha Cutoff", Range(0,1)) = 0.5
        _Cull      ("Cull", Int) = 2
    }

    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull [_Cull]
        ZTest LEqual
        ZWrite Off

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="SRPDefaultUnlit" }

            HLSLPROGRAM
#ifndef unity_ObjectToWorld
#define unity_ObjectToWorld UNITY_MATRIX_M
#endif
#ifndef UNITY_MATRIX_M
#define UNITY_MATRIX_M unity_ObjectToWorld
#endif

#include "USource_HDRPCompat.cginc"
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half _Cutoff;
            CBUFFER_END

            Varyings Vert (Attributes IN)
            {
                Varyings output;
                output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_OUTPUT(Varyings, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                o.positionCS = TransformObjectToHClip(IN.positionOS);
                o.uv = IN.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                return o;
            }

            half4 Frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 col = tex * _BaseColor;
                
                return col;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
