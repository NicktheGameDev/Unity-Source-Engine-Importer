
Shader "HDRP/UnlitGeneric"
{
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        UsePass "HDRP/Custom/AdvancedLit/ForwardLit"
    }
}
