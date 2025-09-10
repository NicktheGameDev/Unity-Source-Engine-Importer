// SourceCG_HDRP.hlsl
// Converted from SourceCG.cginc for HDRP

#ifndef SOURCE_CG_HDRP
#define SOURCE_CG_HDRP

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/Common.hlsl"

// Decodes lightmaps (doubleLDR encoded on GLES)
inline float3 SourceDecodeLightmap(float4 color)
{
#if defined(UNITY_COLORSPACE_GAMMA)
    #if defined(SHADER_API_GLES) && defined(SHADER_API_MOBILE)
        return 1.75 * color.rgb;
    #else
        return (1.75 * color.a) * color.rgb;
    #endif
#else
    #if defined(SHADER_API_GLES) && defined(SHADER_API_MOBILE)
        return 3.75 * color.rgb;
    #else
        return (3.75 * color.a) * color.rgb;
    #endif
#endif
}

// Normal decompression modes to match NormalDecodeMode_t enum
static const int NORM_DECODE_NONE       = 0;
static const int NORM_DECODE_ATI2N     = 1;
static const int NORM_DECODE_ATI2N_ALPHA = 2;

// Decompress a normal with optional separate alpha sampler
float4 DecompressNormal(Texture2D normalTex       , SamplerState normalSamp,
                        float2 uv                 ,
                        int    mode               ,
                        Texture2D alphaTex        , SamplerState alphaSamp)
{
    float4 nt = normalTex.Sample(normalSamp, uv);
    float3 n;
    if (mode == NORM_DECODE_NONE)
    {
        n = nt.xyz * 2.0 - 1.0;
    }
    else if (mode == NORM_DECODE_ATI2N)
    {
        n.xy = nt.xy * 2.0 - 1.0;
        n.z  = sqrt(max(0.0, 1.0 - dot(n.xy, n.xy)));
    }
    else // ATI2N + separate alpha
    {
        n.xy = nt.xy * 2.0 - 1.0;
        n.z  = sqrt(max(0.0, 1.0 - dot(n.xy, n.xy)));
        nt.w = alphaTex.Sample(alphaSamp, uv).r;
    }
    return float4(n, nt.w);
}

// Overload: use same sampler for alpha
float4 DecompressNormal(Texture2D normalTex, SamplerState normalSamp, float2 uv, int mode)
{
    return DecompressNormal(normalTex, normalSamp, uv, mode, normalTex, normalSamp);
}

// Texture combine modes
static const int TCOMBINE_RGB_EQUALS_BASE_x_DETAILx2       = 0;
static const int TCOMBINE_RGB_ADDITIVE                     = 1;
static const int TCOMBINE_DETAIL_OVER_BASE                 = 2;
static const int TCOMBINE_FADE                             = 3;
static const int TCOMBINE_BASE_OVER_DETAIL                 = 4;
static const int TCOMBINE_RGB_ADDITIVE_SELFILLUM           = 5;
static const int TCOMBINE_RGB_ADDITIVE_SELFILLUM_THRESHOLD = 6;
static const int TCOMBINE_MOD2X_SELECT_TWO_PATTERNS        = 7;
static const int TCOMBINE_MULTIPLY                         = 8;
static const int TCOMBINE_MASK_BASE_BY_DETAIL_ALPHA        = 9;
static const int TCOMBINE_SSBUMP_BUMP                      = 10;
static const int TCOMBINE_SSBUMP_NOBUMP                    = 11;

// Combines two textures according to Source’s modes
inline float4 TextureCombine(
    float4 baseColor,
    float4 detailColor,
    int    combineMode,
    float  blendFactor)
{
    if (combineMode == TCOMBINE_MOD2X_SELECT_TWO_PATTERNS)
    {
        float dc = lerp(detailColor.r, detailColor.a, baseColor.a);
        baseColor.rgb *= lerp(1, 2 * dc, blendFactor);
    }
    else if (combineMode == TCOMBINE_RGB_EQUALS_BASE_x_DETAILx2)
    {
        baseColor.rgb *= lerp(1, 2 * detailColor.rgb, blendFactor);
    }
    else if (combineMode == TCOMBINE_RGB_ADDITIVE)
    {
        baseColor.rgb += blendFactor * detailColor.rgb;
    }
    else if (combineMode == TCOMBINE_DETAIL_OVER_BASE)
    {
        float fb = blendFactor * detailColor.a;
        baseColor.rgb = lerp(baseColor.rgb, detailColor.rgb, fb);
    }
    else if (combineMode == TCOMBINE_FADE)
    {
        baseColor = lerp(baseColor, detailColor, blendFactor);
    }
    else if (combineMode == TCOMBINE_BASE_OVER_DETAIL)
    {
        float fb = blendFactor * (1 - baseColor.a);
        baseColor.rgb = lerp(baseColor.rgb, detailColor.rgb, fb);
        baseColor.a   = detailColor.a;
    }
    else if (combineMode == TCOMBINE_MULTIPLY)
    {
        baseColor = lerp(baseColor, baseColor * detailColor, blendFactor);
    }
    else if (combineMode == TCOMBINE_MASK_BASE_BY_DETAIL_ALPHA)
    {
        baseColor.a = lerp(baseColor.a, baseColor.a * detailColor.a, blendFactor);
    }
    else if (combineMode == TCOMBINE_SSBUMP_NOBUMP)
    {
        baseColor.rgb *= dot(detailColor.rgb, 2.0 / 3.0);
    }
    return baseColor;
}

#endif // SOURCE_CG_HDRP
