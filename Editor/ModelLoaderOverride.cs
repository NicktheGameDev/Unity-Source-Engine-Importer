#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using uSource.Runtime;
using uSource.Formats.Source.MDL;
using System.IO;

public static class ModelLoaderOverride
{
    [MenuItem("uSource/Tools/Load MDL With Flex & Jiggle...")]
    public static void LoadModelWizard()
    {
        string mdlPath = EditorUtility.OpenFilePanel("Select .mdl", "", "mdl");
        if (string.IsNullOrEmpty(mdlPath)) return;

        string vtaPath = Path.ChangeExtension(mdlPath, ".vta");
        string vvdPath = Path.ChangeExtension(mdlPath, ".vvd");

        MDLFile mdl = MDLFile.Load(mdlPath, parseAnims: true, parseHitboxes: false);
        Stream vvdS = File.Exists(vvdPath) ? File.OpenRead(vvdPath) : null;
        Stream vtaS = File.Exists(vtaPath) ? File.OpenRead(vtaPath) : null;

        var go = ValveModelImporter.Import(mdl, vvdS, vtaS, vtaS == null ? vtaPath : null);
        Selection.activeObject = go;
    }
}
#endif
