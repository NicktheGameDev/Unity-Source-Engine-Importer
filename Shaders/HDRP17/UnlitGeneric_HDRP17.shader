Shader "HDRP17/UnlitGeneric"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Albedo (RGB) Alpha (A)", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "bump" {}
        _MaskMap ("Mask (Metal R | AO G | Smooth B | Emit A)", 2D) = "black" {}
    }

    SubShader
    {
        Tags {"RenderPipeline"="HDRenderPipeline"}
        UsePass "HDRP/Custom/AdvancedLit/ForwardLit"
    }
}