using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uSource.Formats.Source.VTF
{
    public class VMTFile
    {
        public string FileName = "";

        public VMTFile Include;
        public string ShaderType;
        public string SurfaceProp;
        public Material Material;
        public Material DefaultMaterial;
        public static int TransparentQueue = 3001;

        private static Texture2D _errorTexture;
        private static Texture2D ErrorTexture
        {
            get
            {
                if (_errorTexture != null) return _errorTexture;
                _errorTexture = new Texture2D(4, 4);
                Color magenta = new Color(1f, 0f, 1f);
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        bool checker = ((x + y) % 2) == 0;
                        _errorTexture.SetPixel(x, y, checker ? magenta : Color.black);
                    }
                }
                _errorTexture.Apply();
                _errorTexture.name = "vmt_error_texture";
                return _errorTexture;
            }
        }

        #region KeyValues
        public KeyValues.Entry this[string shader] => KeyValues[shader];
        public KeyValues KeyValues;

        public bool ContainsParam(string param)
        {
            try
            {
                return !string.IsNullOrEmpty(ShaderType) && this[ShaderType].ContainsKey(param);
            }
            catch
            {
                return false;
            }
        }

        public string GetParam(string param)
        {
            try
            {
                return this[ShaderType][param];
            }
            catch
            {
                return string.Empty;
            }
        }

        public float GetSingle(string param)
        {
            string raw = GetParam(param);
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                return v;
            return 0f;
        }

        private static readonly Regex FloatRegex = new Regex(@"[+-]?([0-9]*[.])?[0-9]+", RegexOptions.Compiled);
        public Vector4 GetVector4(string param, bool swap = false)
        {
            string raw = GetParam(param);
            var matches = FloatRegex.Matches(raw);
            float ParseOr(int index, float fallback)
            {
                if (matches.Count > index &&
                    float.TryParse(matches[index].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                    return val;
                return fallback;
            }

            var result = new Vector4();
            result.x = ParseOr(0, 0f);
            result.y = ParseOr(1, swap ? result.x : 0f);
            result.z = ParseOr(2, swap ? result.y : 0f);
            result.w = ParseOr(3, swap ? result.z : 0f);
            return result;
        }

        public Vector3 GetVector3(string param, bool swap = false)
        {
            var v4 = GetVector4(param, swap);
            return new Vector3(v4.x, v4.y, v4.z);
        }

        public Vector2 GetVector2(string param, bool swap = false)
        {
            var v4 = GetVector4(param, swap);
            return new Vector2(v4.x, v4.y);
        }

        public int GetInteger(string param)
        {
            string raw = GetParam(param);
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                return v;
            return 0;
        }

        public Color GetColor()
        {
            Color matColor = Color.white;

            if (ContainsParam("$color"))
            {
                var v = GetVector3("$color");
                matColor.r = v.x;
                matColor.g = v.y;
                matColor.b = v.z;
            }

            if (ContainsParam("$alpha"))
            {
                float alpha = GetSingle("$alpha");
                matColor.a = Mathf.Clamp01(alpha);
            }

            return matColor;
        }

        public bool IsTrue(string input, bool containsCheck = true)
        {
            if (containsCheck && ContainsParam(input))
            {
                var v = GetParam(input).ToLowerInvariant();
                return v == "1" || v == "true" || v == "yes" || v == "on";
            }
            return false;
        }
        #endregion

        public bool HasAnimation = false;

        public void SetupAnimations(AnimatedTexture controlScript, Texture2D[] frames)
        {
            if (controlScript == null)
                throw new ArgumentNullException(nameof(controlScript));

            controlScript.Frames = (frames != null && frames.Length > 0) ? frames : new[] { ErrorTexture };
            float fr = Mathf.Max(GetSingle("animatedtextureframerate"), 1f);
            controlScript.AnimatedTextureFramerate = fr;
        }
        void MakeDefaultMaterial()
        {
            if (DefaultMaterial == null)
            {
#if UNITY_EDITOR
                //Try load asset from project (if exist)
                if (uLoader.SaveAssetsToUnity)
                {
                    Material = DefaultMaterial = uResourceManager.LoadAsset<Material>(FileName, uResourceManager.MaterialsExtension[0], ".mat");
                    if (Material != null)
                        return;
                }
#endif

                Material = DefaultMaterial = new Material(Shader.Find("HDRP/Unlit"));
                Material.name = FileName;
#if UNITY_EDITOR
                if (uLoader.SaveAssetsToUnity)
                {
                    uResourceManager.SaveAsset(Material, FileName, uResourceManager.MaterialsExtension[0], ".mat");
                }
#endif
            }
        }

        public VMTFile(Stream stream, string FileName = "")
        {
            this.FileName = FileName;
            if (stream == null)
            {
                MakeDefaultMaterial();
                return;
            }

            try
            {
                KeyValues = KeyValues.FromStream(stream);
            }
            catch (Exception e)
            {
                Debug.LogError($"[VMTFile] B��d parsowania KeyValues dla {FileName}: {e.Message}");
                MakeDefaultMaterial();
                return;
            }

            if (KeyValues == null || !KeyValues.Keys.Any())
            {
                Debug.LogWarning($"[VMTFile] Brakuje danych KeyValues w materiale ({FileName}). Tworz� domy�lny.");
                MakeDefaultMaterial();
                return;
            }

            ShaderType = KeyValues.Keys.First();
            if (string.IsNullOrEmpty(ShaderType))
            {
                Debug.LogWarning($"[VMTFile] Typ shader'a jest pusty w materiale ({FileName}). Tworz� domy�lny.");
                MakeDefaultMaterial();
                return;
            }
        }

        public void CreateHDRPMaterial(GameObject targetForAnimation = null)
        {
            #region Include / replace / insert logic

            if (ContainsParam("replace"))
            {
                try
                {
                    this[ShaderType].MergeFrom(this[ShaderType]["replace"], true);
                }
                catch
                {
                }
            }

            if (ContainsParam("include"))
            {
                string incName = GetParam("include");
                Include = uResourceManager.LoadMaterial(incName) as VMTFile;
                if (Include != null)
                {
                    try
                    {
                        this[ShaderType].MergeFrom(Include[Include.ShaderType], false);
                    }
                    catch
                    {
                    }
                }
            }

            if (ContainsParam("insert"))
            {
                try
                {
                    this[ShaderType].MergeFrom(this[ShaderType]["insert"], false);
                }
                catch
                {
                }
            }

            if (ContainsParam("$fallbackmaterial"))
            {
                string fbName = GetParam("$fallbackmaterial");
                Include = uResourceManager.LoadMaterial(fbName) as VMTFile;
                if (Include != null)
                {
                    try
                    {
                        this[ShaderType].MergeFrom(Include[Include.ShaderType], true);
                    }
                    catch
                    {
                    }
                }
            }

            #endregion

            Shader hdrpLit = Shader.Find("HDRP/Lit");
            if (hdrpLit == null)
            {
                Debug.LogError("HDRP/Lit shader nieznaleziony. Upewnij si� �e HDRP jest zainstalowane.");
                MakeDefaultMaterial();
                return;
            }

#if UNITY_EDITOR
            if (uLoader.SaveAssetsToUnity)
            {
                Material existing =
                    uResourceManager.LoadAsset<Material>(FileName, uResourceManager.MaterialsExtension[0], ".mat");
                if (existing != null)
                {
                    Material = existing;
                }
                else
                {
                    Material = new Material(hdrpLit);
                    Material.name = FileName;
                }
            }
            else
            {
                Material ??= new Material(hdrpLit);
                Material.name = FileName;
            }
#else
            Material ??= new Material(hdrpLit);
            Material.name = FileName;
#endif

            if (Material == null)
            {
                MakeDefaultMaterial();
                return;
            }

            // Base Color
            Material.SetColor("_BaseColor", GetColor());

            // Base Map (single or animated)
            Texture2D baseTex = null;
            if (ContainsParam("$animatedtexture"))
            {
                string animBase = GetParam("$animatedtexture");
                // load frames: expect names like base_0, base_1, ...
                List<Texture2D> frames = new();
                for (int i = 0;; i++)
                {
                    string frameName = $"{animBase}_{i}";
                    Texture2D frame = null;
                    try
                    {
                        frame = uResourceManager.LoadTexture(frameName,
                            ExportData: new string[,] { { FileName, "_Animated" } })[0, 0];
                    }
                    catch
                    {
                        frame = null;
                    }

                    if (frame == null)
                        break;
                    frames.Add(frame);
                }

                Texture2D[] frameArray;
                if (frames.Count > 0)
                    frameArray = frames.ToArray();
                else
                {
                    Debug.LogWarning(
                        $"[VMTFile] Nie znaleziono klatek animacji dla '$animatedtexture' bazuj�cego na '{animBase}' przy {FileName}. Fallback do pojedynczej tekstury.");
                    frameArray = new[] { ErrorTexture };
                }

                // assign first frame as base map so it isn't white
                baseTex = frameArray[0];
                Material.SetTexture("_BaseColorMap", baseTex);

                // if target GameObject provided, try to set up AnimatedTexture
                if (targetForAnimation != null)
                {
                    var anim = targetForAnimation.GetComponent<AnimatedTexture>();
                    if (anim == null)
                        anim = targetForAnimation.AddComponent<AnimatedTexture>();
                    SetupAnimations(anim, frameArray);
                }
            }
            else if (ContainsParam("$basetexture"))
            {
                string texName = GetParam("$basetexture");
                try
                {
                    baseTex = uResourceManager.LoadTexture(texName,
                        ExportData: new string[,] { { FileName, "_BaseColorMap" } })[0, 0];
                }
                catch
                {
                    baseTex = null;
                }

                if (baseTex == null)
                {
                    Debug.LogWarning(
                        $"[VMTFile] Nie uda�o si� za�adowa� $basetexture '{GetParam("$basetexture")}' dla {FileName}.");
                    baseTex = ErrorTexture;
                }

                Material.SetTexture("_BaseColorMap", baseTex);
            }
            else
            {
                Material.SetTexture("_BaseColorMap", ErrorTexture);
            }

            // Normal Map
            // Normal Map + SSBump (secondary/self-shadow bump)
            bool hasPrimaryNormal = false;
            if (ContainsParam("$normalmap") || ContainsParam("$bumpmap"))
            {
                string normalName = ContainsParam("$normalmap") ? GetParam("$normalmap") : GetParam("$bumpmap");
                Texture2D normalTex = null;
                try
                {
                    normalTex = uResourceManager.LoadTexture(normalName,
                        ExportData: new string[,] { { FileName, "_NormalMap" } })[0, 0];
                }
                catch
                {
                    normalTex = null;
                }

                if (normalTex != null)
                {
                    hasPrimaryNormal = true;
                    Material.EnableKeyword("_NORMALMAP");
                    Material.SetTexture("_NormalMap", normalTex);
                    Material.SetFloat("_NormalScale", 1f);
                }
                else
                {
                    Debug.LogWarning($"[VMTFile] Brak normal map '{normalName}' dla {FileName}.");
                }
            }

            // $ssbump: treat as secondary/detail normal if possible
            if (ContainsParam("$ssbump"))
            {
                string ssbumpName = GetParam("$ssbump");
                Texture2D ssbumpTex = null;
                try
                {
                    ssbumpTex = uResourceManager.LoadTexture(ssbumpName,
                        ExportData: new string[,] { { FileName, "_SSBump" } })[0, 0];
                }
                catch
                {
                    ssbumpTex = null;
                }

                if (ssbumpTex != null)
                {
                    // If HDRP shader has a detail-normal-like slot, assign it; otherwise, fallback to using it as the main normal if none exists.
                    if (Material.HasProperty("_DetailNormalMap"))
                    {
                        Material.SetTexture("_DetailNormalMap", ssbumpTex);
                        Material.EnableKeyword("_DETAIL_NORMAL_MAP");
                    }
                    else if (!hasPrimaryNormal)
                    {
                        Material.EnableKeyword("_NORMALMAP");
                        Material.SetTexture("_NormalMap", ssbumpTex);
                        Material.SetFloat("_NormalScale", 1f);
                    }
                }
                else
                {
                    Debug.LogWarning($"[VMTFile] Nie uda�o si� za�adowa� $ssbump '{ssbumpName}' dla {FileName}.");
                }
            }


            // Phong exponent -> smoothness
            if (ContainsParam("$phongexponent") || ContainsParam("$phongexponenttexture"))
            {
                if (ContainsParam("$phongexponenttexture"))
                {
                    string phongTexName = GetParam("$phongexponenttexture");
                    Texture2D smoothnessTex = null;
                    try
                    {
                        smoothnessTex = uResourceManager.LoadTexture(phongTexName,
                            ExportData: new string[,] { { FileName, "_SmoothnessMap" } })[0, 0];
                    }
                    catch
                    {
                        smoothnessTex = null;
                    }

                    if (smoothnessTex != null)
                    {
                        Material.SetTexture("_SmoothnessMap", smoothnessTex);
                        Material.SetFloat("_Smoothness", 1f);
                        Material.SetFloat("_SmoothnessRemapMin", 0f);
                        Material.SetFloat("_SmoothnessRemapMax", 1f);
                        Material.SetFloat("_EnableSmoothnessMap", 1f);
                    }
                    else
                    {
                        Debug.LogWarning($"[VMTFile] Brak $phongexponenttexture '{phongTexName}' dla {FileName}.");
                    }
                }
                else
                {
                    float exponent = GetSingle("$phongexponent");
                    float smoothness = Mathf.Clamp01(exponent / 128f);
                    Material.SetFloat("_Smoothness", smoothness);
                }
            }

            // Reflection / Envmap - do not assign cube to 2D map
            if (ContainsParam("$envmap") || ContainsParam("$reflectioncubemap"))
            {
                string cubemapName = ContainsParam("$reflectioncubemap")
                    ? GetParam("$reflectioncubemap")
                    : GetParam("$envmap");
                try
                {
                    var cubemap = uResourceManager.LoadTextureAsCube(cubemapName);
                    if (cubemap != null)
                    {
                        Debug.Log(
                            $"[VMTFile] Reflection cubemap '{cubemapName}' loaded for {FileName}. Use reflection probes or custom reflection logic.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"[VMTFile] Nie mo�na za�adowa� cubemapy '{cubemapName}' dla {FileName}: {e.Message}");
                }
            }

            // Detail texture
            if (ContainsParam("$detail"))
            {
                string detailName = GetParam("$detail");
                Texture2D detailTex = null;
                try
                {
                    detailTex = uResourceManager.LoadTexture(detailName,
                        ExportData: new string[,] { { FileName, "_DetailMap" } })[0, 0];
                }
                catch
                {
                    detailTex = null;
                }

                if (detailTex != null)
                {
                    Material.SetTexture("_DetailMap", detailTex);
                    if (ContainsParam("$detailscale"))
                    {
                        Vector2 ds = GetVector2("$detailscale", true);
                        Material.SetTextureScale("_DetailMap", ds);
                    }

                    if (ContainsParam("$detailblendfactor"))
                    {
                        float blend = GetSingle("$detailblendfactor") / 2f;
                        Material.SetFloat("_DetailAlbedoScale", blend);
                    }
                }
                else
                {
                    Debug.LogWarning($"[VMTFile] Brak detail texture '{detailName}' dla {FileName}.");
                }
            }

            // Transparent / Cutout
            if (IsTrue("$translucent"))
            {
                Material.SetFloat("_SurfaceType", 1); // Transparent
                Material.SetFloat("_BlendMode", 0); // Alpha
                Material.renderQueue = TransparentQueue++;
            }

            if (IsTrue("$alphatest"))
            {
                Material.SetFloat("_EnableAlphaTest", 1);
                Material.SetFloat("_AlphaCutoffEnable", 1);
                float cutoff = 0.5f;
                if (ContainsParam("$alphatestreference"))
                    cutoff = GetSingle("$alphatestreference");
                Material.SetFloat("_AlphaCutoff", cutoff);
            }

            if (IsTrue("$nocull"))
            {
                Material.SetInt("_CullMode", (int)UnityEngine.Rendering.CullMode.Off);
            }

            if (ContainsParam("$surfaceprop"))
            {
                SurfaceProp = GetParam("$surfaceprop");
            }

            if (IsTrue("$selfillum") || ContainsParam("$selfillum"))
            {
                Material.EnableKeyword("_EMISSION");
                Color emissionColor = Color.white;
                if (ContainsParam("$selfillumtint"))
                {
                    var tint = GetVector3("$selfillumtint");
                    emissionColor = new Color(tint.x, tint.y, tint.z);
                }

                Material.SetColor("_EmissiveColor", emissionColor);
            }

            // Fallback defaults
            if (!Material.HasProperty("_Smoothness"))
            {
                Material.SetFloat("_Smoothness", 0.5f);
            }

         
            //Try load asset from project (if exist)
            if (uLoader.SaveAssetsToUnity)
            {
                Material = uResourceManager.LoadAsset<Material>(FileName, uResourceManager.MaterialsExtension[0],
                    ".mat");
                if (Material != null)
                    return;
            }
        }

        public Shader GetShader(string shader, bool HasAlpha = false)
        {
            if (!string.IsNullOrEmpty(shader))
            {
                if (IsTrue("$additive"))
                    return Shader.Find(uLoader.AdditiveShader);
                if (ContainsParam("$detail"))
                {
                    if (shader.Equals("unlitgeneric", StringComparison.OrdinalIgnoreCase))
                        return Shader.Find(uLoader.DetailUnlitShader);
                    if (shader.Equals("worldtwotextureblend", StringComparison.OrdinalIgnoreCase))
                        return Shader.Find(uLoader.WorldTwoTextureBlend);
                    if (IsTrue("$translucent"))
                        return Shader.Find(uLoader.DetailTranslucentShader);
                    return Shader.Find(uLoader.DetailShader);
                }
                if ((IsTrue("$translucent") && HasAlpha) || (ContainsParam("$alpha") && !IsTrue("$alpha", false)))
                {
                    if (shader.Equals("unlitgeneric", StringComparison.OrdinalIgnoreCase))
                        return Shader.Find(uLoader.TranslucentUnlitShader);
                    return Shader.Find(uLoader.TranslucentShader);
                }
                if (IsTrue("$alphatest"))
                {
                    return Shader.Find(uLoader.AlphaTestShader);
                }
                if (IsTrue("$selfillum") && (HasAlpha || ContainsParam("$envmapmask")))
                {
                    return Shader.Find(uLoader.SelfIllumShader);
                }
                if (shader.Equals("lightmappedgeneric", StringComparison.OrdinalIgnoreCase))
                    return Shader.Find(uLoader.LightmappedGenericShader);
                if (shader.Equals("vertexlitgeneric", StringComparison.OrdinalIgnoreCase))
                    return Shader.Find(uLoader.VertexLitGenericShader);
                if (shader.Equals("worldvertextransition", StringComparison.OrdinalIgnoreCase))
                {
                    if (ContainsParam("$basetexture2"))
                        return Shader.Find(uLoader.WorldVertexTransitionShader);
                    return Shader.Find(uLoader.DefaultShader);
                }
                if (shader.Equals("WorldTwoTextureBlend", StringComparison.OrdinalIgnoreCase))
                    return Shader.Find(uLoader.WorldTwoTextureBlend);
                if (shader.Equals("unlitgeneric", StringComparison.OrdinalIgnoreCase))
                    return Shader.Find(uLoader.UnlitGeneric);
            }
            return Shader.Find(uLoader.DefaultShader);
        }
    }
}
