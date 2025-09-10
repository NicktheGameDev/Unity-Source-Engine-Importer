
// Auto‑generated HDRP compatible variant of TF2Shader - Painted.shader
Shader "TF2Shader - PaintedHDRP"
{
    Properties{
        // Using properties from original shader (commented in): 
    }
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        LOD 200

        HLSLINCLUDE
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            LightMode "ForwardOnly"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForward.hlsl"
            ENDHLSL
        }
    }
    Fallback "HDRP/Lit"
}
