#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using uSource.Formats.Source.MDL;
using uSource.Runtime;

public class AutomaticMdlPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (var path in imported)
        {
            if (!path.EndsWith(".mdl", System.StringComparison.OrdinalIgnoreCase)) continue;

            string full = Path.GetFullPath(path);
            string dir = Path.GetDirectoryName(full);
            string nameNoExt = Path.GetFileNameWithoutExtension(full);

            string vvd = Path.Combine(dir, nameNoExt + ".vvd");
            string vta = Path.Combine(dir, nameNoExt + ".vta");

            try
            {
                MDLFile mdl = MDLFile.Load(full,parseAnims: true , parseHitboxes : false    );
                Stream vvdS = File.Exists(vvd) ? File.OpenRead(vvd) : null;
                Stream vtaS = File.Exists(vta) ? File.OpenRead(vta) : null;

                var go = ValveModelImporter.Import(mdl, vvdS, vtaS, vtaS == null ? vta : null);

                // Save as prefab next to model
                string prefabPath = Path.ChangeExtension(path, ".prefab");
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                GameObject.DestroyImmediate(go);

                Debug.Log($"AutomaticMdlPostprocessor: prefab with flex+jiggle saved at {prefabPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AutomaticMdlPostprocessor failed: {ex.Message}");
            }
        }
    }
}
#endif
