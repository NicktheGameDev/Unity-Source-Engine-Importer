using System;
using System.IO;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{

    
            public static partial class BlendShapeBuilderAuto
            {
// ---------------------------------------------
// Helper: recursively search files but ignore
// directories that throw UnauthorizedAccessException
// ---------------------------------------------
private static string[] SafeGetFiles(string root, string searchPattern)
{
    try
    {
        return Directory.GetFiles(root, searchPattern, SearchOption.AllDirectories);
    }
    catch (System.UnauthorizedAccessException)
    {
        // Fallback: manual recursion
        var results = new System.Collections.Generic.List<string>();
        try
        {
            foreach (var file in Directory.GetFiles(root, searchPattern, SearchOption.TopDirectoryOnly))
                results.Add(file);
        }
        catch { }

        try
        {
            foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.TopDirectoryOnly))
                results.AddRange(SafeGetFiles(dir, searchPattern));
        }
        catch { }
        return results.ToArray();
    }
}

                public static void TryApplyAuto(string mdlPath, Mesh mesh, mstudioflexdesc_t[] flexDescs = null)
                {
                    string dir      = Path.GetDirectoryName(mdlPath);
                    string baseName = Path.GetFileNameWithoutExtension(mdlPath);

                    // 1) Obok .mdl
                    string vtaPath = Path.Combine(dir, baseName + ".vta");
                    if (!File.Exists(vtaPath))
                    {
                        // 2) Podfoldery katalogu mdl
                        var matches = SafeGetFiles(dir, baseName + ".vta");
                        if (matches.Length > 0)
                            vtaPath = matches[0];
                    }

                    if (!File.Exists(vtaPath))
                    {
                        // 3) ZAWSZE szukaj w całym Assets
                        var projectMatches = SafeGetFiles(Application.dataPath, baseName + ".vta");
                        if (projectMatches.Length > 0)
                            vtaPath = projectMatches[0];
                    }

                    if (!File.Exists(vtaPath))
                    {
                        Debug.LogWarning($"[uSource] No VTA for {baseName} (checked mdl-folder, subfolders, and Assets).");
                        return;
                    }
            // ------------------------------------------------
            //  Parsowanie VTA i dodanie WSZYSTKICH klatek
            // ------------------------------------------------
            try
            {
                using (var fs = File.OpenRead(vtaPath))
                using (var br = new BinaryReader(fs))
                {
                    int version     = br.ReadInt32();
                    int numFrames   = br.ReadInt32();  // liczba klatek
                    int numFlexDesc = br.ReadInt32();  // ile wpisów flex-desc
                    br.ReadInt32();                    // numVertsTotal (pomijamy)

                    // Tablica opisów flexów
                    var flexIndex   = new int[numFlexDesc];
                    var vertCounts  = new int[numFlexDesc];
                    var vertOffsets = new int[numFlexDesc];
                    for (int i = 0; i < numFlexDesc; i++)
                    {
                        flexIndex[i]   = br.ReadInt32();
                        vertCounts[i]  = br.ReadInt32();
                        vertOffsets[i] = br.ReadInt32();
                        br.ReadInt32(); // reserved
                    }

                    // Dla każdego flexa z MDL
                    foreach (var fd in flexDescs ?? Array.Empty<mstudioflexdesc_t>())
                    {
                        int idx = Array.IndexOf(flexIndex, fd.szFACSindex);
                        if (idx < 0 || vertCounts[idx] == 0) 
                            continue;

                        int count     = vertCounts[idx];
                        int entrySize = sizeof(int) + sizeof(float)*6; // 4 + 24 = 28 bajtów

                        // Iterujemy wszystkie klatki
                        for (int f = 0; f < numFrames; f++)
                        {
                            fs.Seek(vertOffsets[idx] + f*count*entrySize, SeekOrigin.Begin);

                            var deltaVerts   = new Vector3[count];
                            var deltaNormals = new Vector3[count];

                            for (int v = 0; v < count; v++)
                            {
                                br.ReadInt32();            // vertexIndex (niepotrzebny)
                                float dx = br.ReadSingle();
                                float dy = br.ReadSingle();
                                float dz = br.ReadSingle();
                                deltaVerts[v] = new Vector3(dx, dy, dz);

                                float nx = br.ReadSingle();
                                float ny = br.ReadSingle();
                                float nz = br.ReadSingle();
                                deltaNormals[v] = new Vector3(nx, ny, nz);
                            }

                            // Waga klatki: 0–100%
                            var weight = (numFrames > 1)
                                ? 100f * f / (numFrames - 1)
                                : 100f;
                            var mstudioflexdescT = fd;
                            var name = mstudioflexdescT.GetFlexName();
                            mesh.AddBlendShapeFrame(
                                name,      // nazwa blendshape’u (to samo dla wszystkich klatek)
                                weight,
                                deltaVerts,
                                deltaNormals,
                                null
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[uSource] Błąd podczas czytania VTA: {ex.Message}");
            }
        }
    }
}
