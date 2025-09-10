using UnityEditor;
using UnityEngine;
using System.IO;
using uSource.Formats.Source.MDL;

public class BlendShapePostprocessor : AssetPostprocessor
{
    void OnPostprocessModel(GameObject go)
    {
        var assetPath = assetImporter.assetPath;
        var mdlPath = Path.ChangeExtension(assetPath, ".mdl");
        var vtaPath = Path.ChangeExtension(assetPath, ".vta");
        
        // Load flex descriptors if VTA exists
        MDLFile mdlFile = null;
        if (File.Exists(mdlPath))
        {
            mdlFile = MDLFile.Load(mdlPath, parseAnims: true, parseHitboxes: false);
        }
        
        foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
        {
            var mesh = mf.sharedMesh;
            if (mesh == null) continue;
            
            // Apply blendshapes from VTA if available
            if (mdlFile != null && File.Exists(vtaPath))
            {
                var flexDesc = mdlFile.MDL_FlexDescs;
                BlendShapeBuilderAuto.TryApplyAuto(assetPath, mesh, flexDesc);
            }
            
            // Convert to SkinnedMeshRenderer if blendshapes present
            if (mesh.blendShapeCount > 0)
            {
                var goMesh = mf.gameObject;
                var mr = goMesh.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    var mats = mr.sharedMaterials;
                    Object.DestroyImmediate(mr, true);
                    Object.DestroyImmediate(mf, true);
                    var smr = goMesh.AddComponent<SkinnedMeshRenderer>();
                    smr.sharedMesh = mesh;
                    smr.sharedMaterials = mats;
                }
            }
        }
    }
}