// Assets/UnityValveSuite/Assets/ValveImporter/Editor/AutoFlexBinder.cs
// W pełni działające – kopiuj, nadpisz, gotowe.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ValveImporter.Editor.Importers;   // VTAImporter.InjectBlendShapes

public sealed class AutoFlexBinder : AssetPostprocessor
{
    void OnPostprocessModel(GameObject go)
    {
        if (!assetPath.EndsWith(".smd", StringComparison.OrdinalIgnoreCase)) return;

        string dir = Path.GetDirectoryName(assetPath);

        foreach (string vta in Directory.GetFiles(dir, "*.vta", SearchOption.TopDirectoryOnly))
        {
            // VTAImporter.InjectBlendShapes() zwraca true, jeśli vertexy pasują
            if (VTAImporter.InjectBlendShapes(go, vta))
                Debug.Log($"[AutoFlexBinder] ✓ {Path.GetFileName(vta)} → {go.name}");
        }
    }
}