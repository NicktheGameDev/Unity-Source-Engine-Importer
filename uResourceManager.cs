﻿using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Unity.VisualScripting;
using uSource.Formats.Source.VPK;
using uSource.Formats.Source.VBSP;
using uSource.Formats.Source.VTF;
using uSource.Formats.Source.MDL;
using UnityEngine;

namespace uSource
{
    #region Resource Providers
    public interface IResourceProvider
    {
        Boolean ContainsFile(String FilePath);
        Stream OpenFile(String FilePath, bool cacheStream = true);

        void CloseStreams();
    }

    public class DirProvider : IResourceProvider
    {
        FileStream CurrentFile;
        private String root;
        public DirProvider(String directory)
        {
            if (uResourceManager.DirectoryCache == null)
                uResourceManager.DirectoryCache = new Dictionary<String, String>();

            if (!String.IsNullOrEmpty(directory))
                root = directory;
        }

        public Boolean ContainsFile(String FilePath)
        {
            if (uResourceManager.DirectoryCache.ContainsKey(FilePath))
                return true;
            else
            {
                String path = root + PathExtension.SeparatorChar + FilePath;
                if (File.Exists(path))
                {
                    uResourceManager.DirectoryCache.Add(FilePath, path);
                    return true;
                }

                return false;
            }
        }

        public Stream OpenFile(String FilePath, bool cacheStream = true)
        {
            if (ContainsFile(FilePath))
            {
                CloseStreams();
                FileStream file = File.OpenRead(uResourceManager.DirectoryCache[FilePath]);
                return cacheStream ? CurrentFile = file : file;
            }

            return null;
        }

        public void CloseStreams()
        {
            if (CurrentFile != null)
            {
                CurrentFile.Dispose();
                CurrentFile.Close();
                return;
            }
        }
    }

    public class PAKProvider : IResourceProvider
    {
        public ZipFile IPAK;
        public Dictionary<String, Int32> files;

        public PAKProvider(Stream stream)
        {
            IPAK = new ZipFile(stream);
            files = new Dictionary<String, Int32>();

            for (Int32 EntryID = 0; EntryID < IPAK.Count; EntryID++)
            {
                ZipEntry entry = IPAK[EntryID];
                if (entry.IsFile)
                {
                    String fileName = entry.Name.ToLower().Replace("\\", "/");
                    if (ContainsFile(fileName))
                        continue;

                    files.Add(fileName, EntryID);
                }
            }
        }

        public Boolean ContainsFile(String FilePath)
        {
            return files.ContainsKey(FilePath);
        }

        public Stream OpenFile(String FilePath, bool cacheStream = true)
        {
            if (ContainsFile(FilePath))
                return IPAK.GetInputStream(files[FilePath]);

            return null;
        }

        public void CloseStreams()
        {
            files.Clear();
            files = null;
            IPAK.Close();
            IPAK = null;
        }
    }

    public class VPKProvider : IResourceProvider
    {
        VPKFile VPK;
        Stream currentStream;

        public VPKProvider(String file)
        {
            if (VPK == null)
            {
                VPK = new VPKFile(file);
            }
        }

        public Boolean ContainsFile(String FilePath)
        {
            return VPK.Entries.ContainsKey(FilePath);
        }

        public Stream OpenFile(String FilePath, bool cacheStream = true)
        {
            if (ContainsFile(FilePath))
            {
                Stream stream = VPK.Entries[FilePath].ReadAnyDataStream();
                return cacheStream ? currentStream = stream : stream;
            }

            return null;
        }

        public void CloseStreams()
        {
            if (currentStream != null)
                currentStream.Close();

            if (VPK != null)
                VPK.Dispose();

            currentStream = null;
            VPK = null;
        }
    }
    #endregion

    public class uResourceManager
    {
        #region Sub Folders & Extensions
        public static readonly String MapsSubFolder = "maps";
        public static readonly String MapsExtension = ".bsp";

        public static readonly String ModelsSubFolder = "models";
        public static readonly String[] ModelsExtension =
        {
            ".mdl",
            ".vvd",
            ".dx90.vtx",
            ".vtx",
            ".dx80.vtx",
            ".sw.vtx",
            ".ani",
            ".phy"
        };
        public static readonly String MaterialsSubFolder = "materials";
        public static readonly String[] MaterialsExtension =
        {
            ".vmt",
            ".vtf"
        };
        #endregion

        #region Cache
        //Cache
        public static Dictionary<String, String> DirectoryCache;
        public static Dictionary<String, Transform> ModelCache;
        public static Dictionary<String, VMTFile> MaterialCache;
        public static Dictionary<String, Texture2D[,]> TextureCache;
        #endregion

        #region Editor Stuff
#if UNITY_EDITOR
        public static Boolean RefreshAssets;
        public static String ProjectPath;
        public static String uSourceSavePath;
        public static String TexExportType;
        public static List<String[,]> TexExportCache;
        public static List<Mesh> UV2GenerateCache;
#endif
        #endregion

        #region Provider Manager
        public static readonly List<IResourceProvider> Providers = new List<IResourceProvider>();

        public static void Init(Int32 StartIndex = 0, IResourceProvider mainProvider = null)
        {
            if (ModelCache == null)
                ModelCache = new Dictionary<String, Transform>();

            if (MaterialCache == null)
                MaterialCache = new Dictionary<String, VMTFile>();

            if (TextureCache == null)
                TextureCache = new Dictionary<String, Texture2D[,]>();

#if UNITY_EDITOR
            if (uLoader.GenerateUV2StaticProps)
            {
                if (UV2GenerateCache == null)
                    UV2GenerateCache = new List<Mesh>();
            }

            if (uLoader.SaveAssetsToUnity)
            {
                if (TexExportCache == null)
                    TexExportCache = new List<String[,]>();

                RefreshAssets = !UnityEditor.EditorPrefs.GetBool("kAutoRefresh");

                if (ProjectPath == null)
                {
                    ProjectPath = Directory.GetCurrentDirectory().NormalizeSlashes();

                    uSourceSavePath = string.Format("Assets/{0}/{1}/", uLoader.OutputAssetsFolder, uLoader.ModFolders[0]).NormalizeSlashes();
                    TexExportType = uLoader.ExportTextureAsPNG ? ".png" : ".asset";
                    ProjectPath += PathExtension.SeparatorChar + uSourceSavePath;
                }
            }
#endif

            if (mainProvider != null)
            {
                Providers.Insert(0, mainProvider);
                return;
            }

            for (Int32 FolderID = StartIndex; FolderID < uLoader.ModFolders.Length; FolderID++)
                Init(uLoader.RootPath, uLoader.ModFolders[FolderID], uLoader.DirPaks[FolderID]);
        }

        public static void Init(String RootPath, String ModFolder, String[] DirPaks)
        {
            //Initializing mod folder to provider cache (to use find resources from mod folder before)
            String FullPath = string.Format("{0}/{1}/", RootPath, ModFolder).NormalizeSlashes();

            if (Directory.Exists(FullPath))
                Providers.Add(new DirProvider(FullPath));

            //Initializing additional VPK's from mod folder (to use find resources from VPK's after mod folder)
            for (Int32 pakID = 0; pakID < DirPaks.Length; pakID++)
            {
                String vpkFile = DirPaks[pakID];

                String dirPath = FullPath + vpkFile + ".vpk";

                if (File.Exists(dirPath))
                {
                    Providers.Add(new VPKProvider(dirPath));
                    continue;
                }
            }
        }

        public static void AddResourceProvider(IResourceProvider provider)
        {
            Providers.Add(provider);
        }

        public static void RemoveResourceProvider(IResourceProvider provider)
        {
            Providers.Remove(provider);
        }

        public static void RemoveResourceProviders()
        {
            Providers.RemoveRange(0, Providers.Count);
        }

        public static Boolean ContainsFile(String FileName, String SubFolder, String FileExtension)
        {
            String FilePath = FileName.NormalizePath(FileExtension, SubFolder);
            for (Int32 i = 0; i < Providers.Count; i++)
            {
                if (Providers[i].ContainsFile(FilePath))
                    return true;
            }

            return false;
        }

        public static Stream OpenFile(String FilePath, bool cacheStream = true)
        {
            for (Int32 i = 0; i < Providers.Count; i++)
            {
                if (Providers[i].ContainsFile(FilePath))
                {
                    return Providers[i].OpenFile(FilePath, cacheStream);
                }
            }

            return Providers[0].OpenFile(FilePath);
        }

        public static void CloseStreams()
        {
            for (Int32 i = 0; i < Providers.Count; i++)
            {
                Providers[i].CloseStreams();
            }
        }
        #endregion

        #region Resource Manager
        public static void LoadMap(String MapName)
        {
            Init(uLoader.RootPath, uLoader.ModFolders[0], uLoader.DirPaks[0]);

            String FileName = string.Format("{1}{0}{2}", PathExtension.SeparatorChar, MapsSubFolder, MapName).NormalizeSlashes() + MapsExtension;
            Stream TempFile = null;

            try
            {
                using (Stream BSPStream = TempFile = OpenFile(FileName, false))
                {
                    if (BSPStream == null)
                    {
                        CloseStreams();
                        RemoveResourceProviders();
                        throw new FileLoadException(FileName + " NOT FOUND!");
                    }

                    if (uLoader.ModFolders.Length > 1)
                        Init(1);

#if UNITY_EDITOR
                    uLoader.DebugTime.Stop();
                    uLoader.DebugTimeOutput.AppendLine("Init time: " + uLoader.DebugTime.Elapsed);
                    uLoader.DebugTime.Restart();
#endif

                    VBSPFile.Load(BSPStream, MapName);

#if UNITY_EDITOR
                    uLoader.DebugTime.Stop();
                    uLoader.DebugTimeOutput.AppendLine("Load time: " + uLoader.DebugTime.Elapsed);
#endif
                }
            }
            finally
            {
                ExportFromCache();
                CloseStreams();
                RemoveResourceProviders();
                if (TempFile != null)
                {
                    TempFile.Dispose();
                    TempFile.Close();
                    TempFile = null;
                }
            }
        }

        public static Transform LoadModel(String ModelPath, Boolean WithAnims = false, Boolean withHitboxes = false, Boolean GenerateUV2 = false)
        {
            //Normalize path before do magic here 
            //(Cuz some paths uses different separators or levels... so we normalize paths always)
            String TempPath = ModelPath.NormalizePath(ModelsExtension[0], ModelsSubFolder, outputWithExt: false);

            //If model exist in cache, return it
            if (ModelCache.ContainsKey(TempPath))
                return UnityEngine.Object.Instantiate(ModelCache[TempPath]);
            //Else begin try load model

            Transform Model;
            String FileName = TempPath + ModelsExtension[0];
            try
            {
                #region Studio Model
                //Try load model
                MDLFile MDLFile;
                using (Stream mdlStream = OpenFile(FileName))
                {
                    if (mdlStream == null)
                        throw new FileLoadException(FileName + " NOT FOUND!");

                    MDLFile = new MDLFile(mdlStream, WithAnims, withHitboxes);
                }
                #endregion

                //Try load vertexes
                #region Vertexes (Vertices data)
                FileName = TempPath + ModelsExtension[1];
                VVDFile VVDFile;
                using (Stream vvdStream = OpenFile(FileName))
                {
                    if (vvdStream != null)
                        VVDFile = new VVDFile(vvdStream, MDLFile);
                    else
                    {
                        Debug.LogWarning(FileName + " NOT FOUND!");
                        MDLFile.BuildMesh = false;
                        VVDFile = null;
                    }
                }
                #endregion

                #region Meshes
                VTXFile VTXFile = null;
                if (MDLFile.BuildMesh)
                {
                    if (VVDFile != null)
                    {
                        //Here we try find all vtx pattern, from high to low
                        for (Int32 TryVTXID = 0; TryVTXID < 4; TryVTXID++)
                        {
                            FileName = TempPath + ModelsExtension[2 + TryVTXID];
                            using (Stream vtxStream = OpenFile(FileName))
                            {
                                if (vtxStream != null)
                                {
                                    MDLFile.BuildMesh = true;
                                    VTXFile = new VTXFile(vtxStream, MDLFile, VVDFile);
                                    break;
                                }
                            }
                        }

                        //If at least one VTX was not found, notify about that
                        if (VTXFile == null)
                        {
                            Debug.LogWarning(FileName + " NOT FOUND!");
                            MDLFile.BuildMesh = false;
                        }
                    }
                }
                #endregion

                //Try build model
                Model = MDLFile.BuildModel(GenerateUV2);

                //Reset all
                MDLFile = null;
                VVDFile = null;
                VTXFile = null;

                //Add model to cache (to load faster than rebuild models again, again and again...)
                ModelCache.Add(TempPath, Model);
            }
            catch (Exception ex)
            {
                Model = new GameObject(TempPath).transform;
                //notify about error
                Debug.LogError(String.Format("{0}: {1}", TempPath, ex));
                ModelCache.Add(TempPath, Model);
                return Model;
            }

            return Model;
        }

  public static VMTFile LoadMaterial(string MaterialPath)
{
    // Znormalizuj: nazwa bez "materials/" i bez rozszerzenia
    string temp = MaterialPath.NormalizePath(MaterialsExtension[0], MaterialsSubFolder, outputWithExt: false);

    // Jeśli w cache jest wpis, ale z pustym Material – odśwież go
    if (MaterialCache != null && MaterialCache.TryGetValue(temp, out var cached))
    {
        if (cached != null && cached.Material != null)
            return cached;

        MaterialCache.Remove(temp); // wymuś ponowne wczytanie
    }

    string fileName = temp + MaterialsExtension[0]; // .vmt
    VMTFile vmt;

    using (Stream vmtStream = OpenFile(fileName))
    {
        if (vmtStream == null)
        {
            // Druga próba: gdy ktoś poda pełną ścieżkę z "materials/"
            string alt = (MaterialsSubFolder + "/" + temp + MaterialsExtension[0]).NormalizeSlashes();
            using (Stream vmtStream2 = OpenFile(alt))
            {
                if (vmtStream2 == null)
                {
                    Debug.LogWarning(fileName + " NOT FOUND!");
                    vmt = new VMTFile(null, fileName); // placeholder
                }
                else
                {
                    try { vmt = new VMTFile(vmtStream2, fileName); }
                    catch (Exception ex)
                    {
                        Debug.LogError($"{temp}: {ex.Message}");
                        vmt = new VMTFile(null, fileName);
                    }
                }
            }
        }
        else
        {
            try { vmt = new VMTFile(vmtStream, fileName); }
            catch (Exception ex)
            {
                Debug.LogError($"{temp}: {ex.Message}");
                vmt = new VMTFile(null, fileName);
            }
        }
    }

    // ZAWSZE spróbuj stworzyć materiał
    try
    {
        if (vmt != null && vmt.Material == null)
        {
            vmt.CreateHDRPMaterial(); // twoja istniejąca metoda
            if (vmt.Material == null)
            {
                // Twardy fallback: HDRP → URP → Standard
                var shader = Shader.Find("HDRP/Lit")
                             ?? Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Standard")
                             ?? Shader.Find("Sprites/Default");
                var m = new Material(shader) { name = System.IO.Path.GetFileName(temp) };
                vmt.Material = m;
            }
        }
    }
    catch (Exception ex)
    {
        Debug.LogWarning($"[uSource] Create material fallback for '{temp}': {ex.Message}");
        var shader = Shader.Find("HDRP/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Standard")
                     ?? Shader.Find("Sprites/Default");
        var m = new Material(shader) { name = System.IO.Path.GetFileName(temp) };
        vmt.Material = m;
    }

    // Do cache i zwrot
    if (MaterialCache == null) MaterialCache = new Dictionary<string, VMTFile>();
    MaterialCache[temp] = vmt;
    return vmt;
}


        internal static Cubemap LoadTextureAsCube(string cubemapName)
        {
            if (string.IsNullOrEmpty(cubemapName))
                return null;

            // 1. Wczytaj pojedynczą teksturę (np. equirectangular)
            Texture2D[,] loaded = LoadTexture(cubemapName);
            Texture2D source = loaded != null ? loaded[0, 0] : null;

            if (source != null)
            {
                Texture2D readable = EnsureTextureIsReadable(source);
                int size = Mathf.Max(readable.width, readable.height);
                if (size <= 0) size = 16;

                // TODO: tu możesz zastąpić fallback rzeczywistą konwersją equirectangular -> cubemap
                Cubemap fallback = new Cubemap(size, TextureFormat.RGBA32, true);
                Color[] pixels = (readable.width == size && readable.height == size)
                    ? readable.GetPixels()
                    : ScaleTexture(readable, size, size).GetPixels();
                for (int f = 0; f < 6; f++)
                    fallback.SetPixels(pixels, (CubemapFace)f);
                fallback.Apply(true, false);
                return fallback;
            }

            // 2. Ostateczny fallback: czarna cubemapka, żeby nie zwracać null
            int fallbackSize = 16;
            Cubemap empty = new Cubemap(fallbackSize, TextureFormat.RGBA32, true);
            Color[] black = Enumerable.Repeat(Color.black, fallbackSize * fallbackSize).ToArray();
            for (int f = 0; f < 6; f++)
                empty.SetPixels(black, (CubemapFace)f);
            empty.Apply(true, false);
            Debug.LogWarning($"[uResourceManager] Nie udało się załadować cubemapy dla '{cubemapName}', zwrócono czarny placeholder.");
            return empty;
        }


        // Pomocniczo: upewnij się, że tekstura jest readable (kopiujemy przez RenderTexture jeśli nie)
        private static Texture2D EnsureTextureIsReadable(Texture2D src)
        {
#if UNITY_EDITOR
            if (src.isReadable)
                return src;
#endif
            RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, true);
            copy.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            copy.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            copy.name = src.name + "_readable_copy";
            return copy;
        }

        // Pomocniczo: skalowanie tekstury (najprostsze nearest / bilinear)
        private static Texture2D ScaleTexture(Texture2D src, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D scaled = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, true);
            scaled.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            scaled.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            scaled.name = src.name + $"_scaled_{targetWidth}x{targetHeight}";
            return scaled;
        }

   public static void ApplyVtaBlendShapes(UnityEngine.Mesh mesh, uSource.Formats.Source.VTA.VTAFile vta, uSource.Formats.Source.MDL.MDLFile mdl)
        {
            if (vta == null || mesh == null) return;
            if (vta.VertexCount != mesh.vertexCount)
            {
                UnityEngine.Debug.LogWarning($"[uSource] VTA vertex count {{vta.VertexCount}} ≠ mesh {{mesh.vertexCount}}; skipping flex import.");
                return;
            }

            var baseVerts = vta.Frames[0].Positions;

            for (int f = 1; f < vta.Frames.Length; f++)
            {
                var frame = vta.Frames[f];
                if (frame.Positions == null || frame.Positions.Length != mesh.vertexCount) continue;

                var flexName = mdl.MDL_FlexDescs[frame.Index].ToString();
                var deltaVerts = new UnityEngine.Vector3[mesh.vertexCount];
                for (int v = 0; v < mesh.vertexCount; v++)
                    deltaVerts[v] = frame.Positions[v] - baseVerts[v];

                mesh.AddBlendShapeFrame(flexName, 100f, deltaVerts, null, null);
            }
        }


        public static Texture2D[,] LoadTexture(String TexturePath, String AltTexture = null, Boolean ImmediatelyConvert = false, String[,] ExportData = null)
        {
            String TempPath;

            //Normalize paths before do magic here 
            //(Cuz some paths uses different separators or levels... so we normalize paths always)
            TempPath = TexturePath.NormalizePath(MaterialsExtension[1], MaterialsSubFolder, outputWithExt: false);
            if (AltTexture != null)
                AltTexture = TexturePath.NormalizePath(MaterialsExtension[1], MaterialsSubFolder, outputWithExt: false);

#if UNITY_EDITOR
            //Add texture to export process from material (if ImmediatelyConvert is false & save assets option enabled)
            if (uLoader.SaveAssetsToUnity && !ImmediatelyConvert)
            {
                if (ExportData != null)
                    TexExportCache.Add(new String[,] { { ExportData[0, 0].Replace(MaterialsExtension[0], ""), ExportData[0, 1], TempPath } });
            }
#endif

            //If texture exist in cache, return it
            if (TextureCache.ContainsKey(TempPath))
                return TextureCache[TempPath];
            //Else begin try load texture

#if UNITY_EDITOR
            //Try load texture from project (if exist & ImmediatelyConvert is false & save assets option enabled))
            if (uLoader.SaveAssetsToUnity && ImmediatelyConvert)
            {
                String FilePath = uSourceSavePath + TempPath + TexExportType;
                Texture2D[,] Frames = new Texture2D[,] { { UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(FilePath) } };
                if (Frames[0, 0] != null)
                {
                    TextureCache.Add(TempPath, Frames);
                    return Frames;
                }
            }
#endif

            VTFFile VTFFile;
            String FileName = TempPath + MaterialsExtension[1];
            using (Stream vtfStream = OpenFile(FileName))
            {
                //If at least one texture was not found, notify about that
                if (vtfStream == null)
                {
                    Debug.LogWarning(FileName + " NOT FOUND!");

                    if (String.IsNullOrEmpty(AltTexture))
                        return new[,] { { Texture2D.whiteTexture } };
                    else
                        return LoadTexture(AltTexture);
                }
                //Else try load texture

                try
                {
                    VTFFile = new VTFFile(vtfStream, FileName);
                }
                catch (Exception ex)
                {
                    //notify about error
                    Debug.LogError(String.Format("{0}: {1}", TempPath, ex.Message));
                    return new[,] { { Texture2D.whiteTexture } };
                }
            }

#if UNITY_EDITOR
            //Try save texture to project (if ImmediatelyConvert is true & save assets option enabled)
            if (uLoader.SaveAssetsToUnity && ImmediatelyConvert)
            {
                if (uLoader.ExportTextureAsPNG)
                    VTFFile.Frames[0, 0] = SaveTexture(VTFFile.Frames[0, 0], FileName);
                else
                    SaveAsset(VTFFile.Frames[0, 0], FileName, MaterialsExtension[1], ".asset");
            }
#endif

            //Add texture to cache (to load faster than rebuild texture again, again and again...)
            TextureCache.Add(TempPath, VTFFile.Frames);

            return VTFFile.Frames;
        }
        #endregion

        #region Export Manager
public static void ExportFromCache()
{
#if UNITY_EDITOR
    if (uLoader.DebugTime != null)
        uLoader.DebugTime.Restart();

    // ───────── UV2 dla statyk (bez zmian, ale z null-guardami) ─────────
    if (uLoader.GenerateUV2StaticProps && UV2GenerateCache != null && UV2GenerateCache.Count > 0)
    {
        int total = UV2GenerateCache.Count;
        UnityEditor.UnwrapParam unwrap;
        UnityEditor.UnwrapParam.SetDefaults(out unwrap);
        unwrap.hardAngle  = uLoader.UV2HardAngleProps;
        unwrap.packMargin = uLoader.UV2PackMarginProps / uLoader.UV2PackMarginTexSize;
        unwrap.angleError = uLoader.UV2AngleErrorProps / 100f;
        unwrap.areaError  = uLoader.UV2AreaErrorProps  / 100f;

        for (int i = 0; i < total; i++)
        {
            var mesh = UV2GenerateCache[i];
            if (mesh == null) continue;
            UnityEditor.EditorUtility.DisplayProgressBar($"Generate UV2: {i}/{total}", mesh.name, (float)i / total);
            UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh, unwrap);
        }
        UnityEditor.EditorUtility.ClearProgressBar();
    }

    // ───────── Zapis materiałów/tekstur tylko gdy NIE są null ─────────
    if (uLoader.SaveAssetsToUnity)
    {
        bool needRefresh = false;

        // Materiały
        // Materiały
        if (MaterialCache != null && MaterialCache.Count > 0)
        {
            int cur = 0, total = MaterialCache.Count;
            foreach (var kv in MaterialCache)
            {
                cur++;
                string filePath = kv.Key + ".mat";
                UnityEditor.EditorUtility.DisplayProgressBar($"Save Materials: {cur}/{total}", filePath, (float)cur / total);

                var vmt = kv.Value;
                var mat = vmt != null ? vmt.Material : null;

                // jeśli nadal null – spróbuj przeładować „na świeżo” (po poprawnym providerze)
                if (mat == null)
                {
                    var re = LoadMaterial(kv.Key);
                    mat = re?.Material;
                }

                if (mat == null)
                {
                    Debug.LogWarning($"[uSource] Skip save NULL material (still): {kv.Key}");
                    continue;
                }

                if (File.Exists(ProjectPath + filePath)) continue;

                SaveAsset(mat, filePath, UseReplace: false);
                needRefresh = true;
            }
            UnityEditor.EditorUtility.ClearProgressBar();
        }

        // Tekstury eksportowane
        if (TexExportCache != null && TexExportCache.Count > 0)
        {
            int total = TexExportCache.Count;
            for (int i = 0; i < total; i++)
            {
                var rec = TexExportCache[i];
                if (rec == null) continue;

                string materialPath = rec[0, 0];
                string propPath     = rec[0, 1];
                string key          = rec[0, 2]; // bez rozszerzenia

                string assetPath = key + (uLoader.ExportTextureAsPNG ? ".png" : ".asset");

                UnityEditor.EditorUtility.DisplayProgressBar($"Convert Textures: {i}/{total}", assetPath, (float)i / total);

                if (!TextureCache.TryGetValue(key, out var frames) || frames == null || frames.Length == 0)
                {
                    Debug.LogWarning($"[uSource] Skip texture save (cache miss): {key}");
                    continue;
                }

                var tex = frames[0, 0];
                if (tex == null)
                {
                    Debug.LogWarning($"[uSource] Skip texture save (NULL): {key}");
                    continue;
                }

                if (File.Exists(ProjectPath + assetPath))
                    continue;

                needRefresh = true;
                if (uLoader.ExportTextureAsPNG)
                    SaveTexture(tex, assetPath, UseReplace: false, RefreshAssets: false);
                else
                    SaveAsset(tex, assetPath, UseReplace: false);
            }
            UnityEditor.EditorUtility.ClearProgressBar();

            if (needRefresh)
            {
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }

            // Po refresh – podłącz tekstury do materiałów (jeśli są)
            int total2 = TexExportCache.Count;
            for (int i = 0; i < total2; i++)
            {
                var rec = TexExportCache[i];
                if (rec == null) continue;

                string materialPath = rec[0, 0];
                string propPath     = rec[0, 1];
                string key          = rec[0, 2];
                string assetPath    = key + (uLoader.ExportTextureAsPNG ? ".png" : ".asset");

                UnityEditor.EditorUtility.DisplayProgressBar("Reset textures in materials", assetPath, (float)i / total2);

                Texture2D texObj = null;
                if (File.Exists(ProjectPath + assetPath))
                    texObj = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(uSourceSavePath + assetPath);

                if (MaterialCache.TryGetValue(materialPath, out var vmt) && vmt != null && vmt.Material != null && texObj != null)
                    vmt.Material.SetTexture(propPath, texObj);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
        }
    }

    if (uLoader.DebugTime != null)
    {
        uLoader.DebugTime.Stop();
        uLoader.DebugTimeOutput.AppendLine("Export / Convert total time: " + uLoader.DebugTime.Elapsed);
        Debug.Log(uLoader.DebugTimeOutput);
    }
#endif
}


#if UNITY_EDITOR
        public static T LoadAsset<T>(String FilePath, String OriginalType, String ReplaceType) where T : UnityEngine.Object
        {
            FilePath = FilePath.Replace(OriginalType, ReplaceType);

            if (File.Exists(ProjectPath + FilePath))
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(uSourceSavePath + FilePath);
            }
            else
                return null;
        }


#if UNITY_EDITOR
        public static Texture2D SaveTexture(Texture2D texture, string filePath, bool UseReplace = true, bool RefreshAssets = true)
        {
            if (string.IsNullOrEmpty(filePath) || texture == null)
            {
                Debug.LogWarning($"[uResourceManager] SaveTexture skipped (path or texture NULL): '{filePath}'");
                return null;
            }

            CreateProjectDirs(filePath);

            if (UseReplace)
                filePath = filePath.Replace(MaterialsExtension[1], ".png");

            // Flip pionowy, zachowujemy
            Texture2D tmp = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, true);
            var src = texture.GetPixels();
            var dst = new Color[src.Length];
            for (int y = 0; y < texture.height; y++)
                Array.Copy(src, y * texture.width, dst, (texture.height - 1 - y) * texture.width, texture.width);
            tmp.SetPixels(dst);
            tmp.Apply();

            byte[] png = tmp.EncodeToPNG();
            string full = ProjectPath + filePath;
            File.WriteAllBytes(full, png);

            if (RefreshAssets)
            {
                UnityEditor.AssetDatabase.Refresh();
                return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(uSourceSavePath + filePath);
            }
            return null;
        }
#endif


#if UNITY_EDITOR
        public static void SaveAsset(UnityEngine.Object obj, string filePath, string originalType = "", string replaceType = "", bool UseReplace = true)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            if (obj == null)
            {
                Debug.LogWarning($"[uResourceManager] SaveAsset skipped (NULL): {filePath}");
                return;
            }

            CreateProjectDirs(filePath);

            // Zbuduj pełną ścieżkę w obrębie projektu
            string finalPath = UseReplace
                ? (uSourceSavePath + filePath.Replace(originalType, replaceType)).NormalizeSlashes()
                : (uSourceSavePath + filePath).NormalizeSlashes();

            // Gwarancja unikatowej ścieżki
            finalPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(finalPath);

            try
            {
                UnityEditor.AssetDatabase.CreateAsset(obj, finalPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[uResourceManager] CreateAsset failed for '{finalPath}': {ex.Message}");
            }
        }
#endif


        static void CreateProjectDirs(String FilePath)
        {
            String FullPath = ProjectPath + Path.GetDirectoryName(FilePath);

            if (!Directory.Exists(FullPath))
            {
                Directory.CreateDirectory(FullPath);
                if (RefreshAssets)
                    UnityEditor.AssetDatabase.Refresh();
            }
        }
#endif
        #endregion
    }
}