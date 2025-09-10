#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace uSource.Editor
{
    /// <summary>
    /// Ensures that every texture imported through uSource has the correct type
    /// and that streaming mip‑maps are always enabled.
    /// </summary>
    public sealed class uSourceTexturePostprocessor : AssetPostprocessor
    {
        private static readonly string[] AlbedoKeywords =
        {
            "albedo", "diffuse", "basecolor", "base_color"
        };

        private static readonly string[] NormalSuffixes =
        {
            "_n",
            "_normal",
            "_norm",
            "_normals",
            "_nrm",
            "_nm",
            "_bump",
            "_bumpmap",
            "_ssbump",
            "normal"
            // if you want you can also add partials like "normal" without underscore
        };

        private static readonly string[] MaskSuffixes =
        {
            "_mra",
            "_mask",
            "_m"
        };

        private static readonly string[] SpecularSuffixes =
        {
            "_phong",
            "_phongexponent"
        };

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;

            // don't touch sprites
            if (importer.textureType == TextureImporterType.Sprite)
                return;

            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;

            var name = Path.GetFileNameWithoutExtension(assetPath)
                           .ToLowerInvariant();

            // 1) Albedo/diffuse → default + sRGB
            if (AlbedoKeywords.Any(k => name.Contains(k)))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                return;
            }

            // 2) Normal map → NormalMap + linear
            //    *Now we match ANY occurrence of the normal suffix*
            if (NormalSuffixes.Any(sfx => name.Contains(sfx)))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
                return;
            }

            // 3) Specular (phong exponent) → default + linear
            if (SpecularSuffixes.Any(sfx => name.EndsWith(sfx)))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                return;
            }

            // 4) Metal‑Roughness‑AO pack → default + linear
            if (MaskSuffixes.Any(sfx => name.EndsWith(sfx)))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                return;
            }

            // 5) Everything else → default + sRGB
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
        }
    }
}
#endif
