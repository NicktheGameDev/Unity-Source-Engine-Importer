
/*
 * AutomaticBlendShapeImporter.cs
 * Automatically converts Source Engine flex controllers (VTA) to Unity BlendShapes
 * during .mdl import so that the SkinnedMeshRenderer inspector shows sliders
 * without needing conversion to FBX.  Drop your .mdl/.vvd/.vtx/.vta files into
 * the Assets folder and they will import as a GameObject with BlendShapes ready.
 *
 * 2025‑05‑15 – uSource enhanced edition
 */
#if UNITY_EDITOR
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using uSource.Formats.Source.MDL;
using uSource.Formats.Source.VTA;
using Debug = UnityEngine.Debug;

public class AutomaticBlendShapeImporter : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        // Importer only cares about .mdl sources
        if (!assetPath.EndsWith(".mdl", System.StringComparison.OrdinalIgnoreCase))
            return;

        // Locate accompanying .vta
        string vtaPath = Path.ChangeExtension(assetPath, ".vta");
if (!File.Exists(vtaPath))
{
    GenerateVTA(assetPath, vtaPath);
    if (!File.Exists(vtaPath))
        return;
}


        // Parse .vta
        using (FileStream fs = File.OpenRead(vtaPath))
        {
            VTAFile vta = new VTAFile();

            if (vta.Frames == null || vta.Frames.Length < 2)
            {
                Debug.LogWarning($"[uSource Enhanced] VTA '{Path.GetFileName(vtaPath)}' does not contain enough frames for blend‑shapes.");
                return;
            }

            Vector3[] basePositions = vta.Frames[0].Positions;

            // Work out names – classic convention is that each frame after 0 is a flex
            string[] flexNames = new string[vta.Frames.Length - 1];
            for (int i = 1; i < vta.Frames.Length; i++)
            {
                flexNames[i - 1] = $"flex_{i}"; // we'll replace if we later parse $flex_filename blocks
            }

            // Add the blendShapes to each SkinnedMeshRenderer below the imported asset
            foreach (var smr in g.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Mesh mesh = smr.sharedMesh;
                if (mesh == null || mesh.vertexCount != basePositions.Length)
                {
                    Debug.LogWarning($"[uSource Enhanced] Vertex count mismatch between '{Path.GetFileName(assetPath)}' and '{Path.GetFileName(vtaPath)}' – skipping.");
                    continue;
                }

                for (int frameIndex = 1; frameIndex < vta.Frames.Length; frameIndex++)
                {
                    string blendShapeName = flexNames[frameIndex - 1];
                    Vector3[] deltaVertices = new Vector3[mesh.vertexCount];
                    Vector3[] deltaNormals = new Vector3[mesh.vertexCount]; // not used
                    Vector3[] deltaTangents = new Vector3[mesh.vertexCount]; // not used

                    Vector3[] framePositions = vta.Frames[frameIndex].Positions;

                    for (int v = 0; v < mesh.vertexCount; v++)
                    {
                        deltaVertices[v] = framePositions[v] - basePositions[v];
                    }

                    mesh.AddBlendShapeFrame(blendShapeName, 100f, deltaVertices, deltaNormals, deltaTangents);
                }

                mesh.UploadMeshData(false);
                EditorUtility.SetDirty(mesh);
            }

            Debug.Log($"[uSource Enhanced] Added {vta.Frames.Length - 1} BlendShapes from '{Path.GetFileName(vtaPath)}' to '{Path.GetFileName(assetPath)}'");
        }
        static void GenerateVTA(string assetPath, string vtaPath)
        {
            string qcPath = Path.ChangeExtension(assetPath, ".qc");
            if (!File.Exists(qcPath))
            {
                Debug.LogWarning($"[uSource Enhanced] QC file not found for '{assetPath}'. Cannot generate VTA.");
                return;
            }
            string crowbarDir = Path.Combine(Application.dataPath, "Crowbar");
            string crowbarExe = Path.Combine(crowbarDir, "crowbar.exe");
            if (!File.Exists(crowbarExe))
            {
                DownloadCrowbar(crowbarDir);
            }
            string qcText = File.ReadAllText(qcPath);
            var match = Regex.Match(qcText, @"\$game\s+(\S+)", RegexOptions.IgnoreCase);
            string gameName = match.Success ? match.Groups[1].Value : "hl2";
            Directory.CreateDirectory(Path.GetDirectoryName(vtaPath));
            var psi = new ProcessStartInfo
            {
                FileName = crowbarExe,
                Arguments = $"-game {gameName} -inputqc \"{qcPath}\" -outputfolder \"{Path.GetDirectoryName(vtaPath)}\" -vta",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var proc = Process.Start(psi);
            proc.WaitForExit();
        }
        
        // Pobieranie Crowbar, jeśli nie ma w Assets/Crowbar
        static void DownloadCrowbar(string crowbarDir)
        {
            Directory.CreateDirectory(crowbarDir);
            string downloadUrl = "https://github.com/MattPocock/Crowbar/releases/download/v0.2.2/CrowbarAllInOne.zip";
            using (UnityWebRequest uwr = UnityWebRequest.Get(downloadUrl))
            {
                var dl = uwr.SendWebRequest();
                while (!dl.isDone) { }
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    string zipPath = Path.Combine(crowbarDir, "CrowbarAllInOne.zip");
                    File.WriteAllBytes(zipPath, uwr.downloadHandler.data);
                    ZipFile.ExtractToDirectory(zipPath, crowbarDir);
                    File.Delete(zipPath);
                }
                else
                {
                    Debug.LogError("[uSource Enhanced] Failed to download Crowbar: " + uwr.error);
                }
            }
        }
        
        
        
    }
}
// --------------------------------------------------
// Auto
// --------------------------------------------------
#endif










