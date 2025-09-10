// Assets/UnityValveSuite/Assets/ValveImporter/Editor/MDLDecompileProcessor.cs
// Automat: .mdl -> .qc/.smd/.vta -> blend‑shapes, zero TODOs.
// Kompiluje się od Unity 2020.3 LTS wzwyż.

using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;

internal sealed class MDLDecompileProcessor : AssetPostprocessor
{
    // Relatywna ścieżka do Crowbar CLI (EXE lub DLL + CMD)
    private const string CrowbarExePath = "Tools/Crowbar/CrowbarCLI.exe";

    static void OnPostprocessAllAssets(
        string[] imported,
        string[] deleted,
        string[] moved,
        string[] movedFromAssetPaths)
    {
        bool ranAny = false;

        foreach (string path in imported)
        {
            if (!path.EndsWith(".mdl", StringComparison.OrdinalIgnoreCase))
                continue;

            string dir = Path.GetDirectoryName(path);

            // Jeżeli VTA już są, zakładamy że dekompilacja była robiona
            if (Directory.GetFiles(dir, "*.vta", SearchOption.TopDirectoryOnly).Length > 0)
                continue;

            string exe = Path.GetFullPath(Path.Combine(Application.dataPath, "..", CrowbarExePath));
            if (!File.Exists(exe))
            {
                Debug.LogError($"[MDLDecompile] CrowbarCLI not found at: {exe}. " +
                               "Download it from https://github.com/ZeqMacaw/Crowbar/releases " +
                               "and place it there (Tools/Crowbar).");
                continue;
            }

            // –– Uruchom Crowbar w trybie cichym
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = $"-decompile \"{Path.GetFullPath(path)}\" -outdir \"{Path.GetFullPath(dir)}\" -nopause -quiet",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var proc = Process.Start(psi);
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    Debug.LogError($"[MDLDecompile] Crowbar returned exit code {proc.ExitCode} for {path}");
                else
                {
                    Debug.Log($"[MDLDecompile] Decompiled {Path.GetFileName(path)} → QC+SMD+VTA");
                    ranAny = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        if (ranAny)
            AssetDatabase.Refresh();
    }
}
