// Assets/Editor/SingleMdlImporter.cs

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using uSource.Formats.Source.MDL;

using static uSource.Formats.Source.MDL.StudioStruct;
using static uSource.Formats.Source.MDL.MDLFile;


public static class SingleMdlImporter
{
    // MenuItem to import via file‐picker
    [MenuItem("Tools/Import Single MDL…")]
    public static void ImportSingleViaPicker()
    {
        string mdlPath = EditorUtility.OpenFilePanel("Select a .mdl file", "", "mdl");
        if (string.IsNullOrEmpty(mdlPath)) return;
        ImportMdl(mdlPath);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import Complete", 
            $"Imported {Path.GetFileName(mdlPath)}", "OK");
    }

    // MenuItem to import the selected .mdl in the Project window
    [MenuItem("Assets/Import Selected MDL", true)]
    public static bool ValidateImportSelected() =>
        Selection.activeObject != null &&
        AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".mdl");

    [MenuItem("Assets/Import Selected MDL")]
    public static void ImportSelected()
    {
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        // Convert “Assets/…” to full filesystem path
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        ImportMdl(fullPath);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import Complete",
            $"Imported {Path.GetFileName(assetPath)}", "OK");
    }

    static void ImportMdl(string fullPath)
    {
        // 1) Load MDL (with anims + hitboxes)
        var mdl = MDLFile.Load(fullPath, parseAnims: true, parseHitboxes: true);

        // 2) Compute a clean name (no “.mdl”!)
        string modelName = Path.GetFileNameWithoutExtension(fullPath);

        // 3) Prepare output folder under Assets/GameImported/<modelName>/
        string outDir = $"Assets/GameImported/{modelName}";
        EnsureFolders(outDir);

        // 4) Build bones & bone‐paths
        var bones     = BuildBones(mdl.MDL_StudioBones, mdl.MDL_BoneNames);
        var bonePaths = BuildBonePathDict(mdl.MDL_StudioBones, mdl.MDL_BoneNames);

        // 5) Instantiate root and meshes
        var rootGO = new GameObject(modelName);
        for (int i = 0; i < bones.Length; i++)
            bones[i].parent = rootGO.transform; // temporarily parent all under root

        foreach (var bp in mdl.MDL_Bodyparts)
            foreach (var sm in bp.Models)
                ImportStudioModel(sm, bones, rootGO.transform);

        // 6) Save prefab
        string prefabPath = $"{outDir}/{modelName}.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(
            rootGO, prefabPath, InteractionMode.UserAction
        );
        UnityEngine.Object.DestroyImmediate(rootGO);

        // 7) Create AnimatorController + clips
        string ctrlPath = $"{outDir}/{modelName}_Controller.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        var smachine   = controller.layers[0].stateMachine;

        foreach (var seq in mdl.Sequences)
        {
            for (int ai = 0; ai < seq.ani.Count; ai++)
            {
                var animInfo = seq.ani[ai];
                var clip     = mdl.CreateAnimationClip(animInfo, bonePaths);
                // Unique name: sequenceName[_index]
                string clipName = seq.name.Replace(' ', '_');
                if (seq.ani.Count > 1) clipName += $"_{ai}";
                clip.name = clipName;

                AssetDatabase.CreateAsset(clip, $"{outDir}/{clipName}.anim");
                var st = smachine.AddState(clipName);
                st.motion = clip;
            }
        }

        AssetDatabase.SaveAssets();
    }

    static void EnsureFolders(string dir)
    {
        var parts = dir.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    static Transform[] BuildBones(mstudiobone_t[] bones, string[] names)
    {
        var arr = new Transform[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            var b  = bones[i];
            var go = new GameObject(names[i]);
            go.transform.localPosition = b.pos;
            go.transform.localRotation = b.quat;
            arr[i] = go.transform;
            if (b.parent >= 0) go.transform.parent = arr[b.parent];
        }
        return arr;
    }

    static Dictionary<int, string> BuildBonePathDict(mstudiobone_t[] bones, string[] names)
    {
        var dict = new Dictionary<int, string>(bones.Length);
        for (int i = 0; i < bones.Length; i++)
        {
            string path = names[i];
            for (int p = bones[i].parent; p >= 0; p = bones[p].parent)
                path = names[p] + "/" + path;
            dict[i] = path;
        }
        return dict;
    }

    static void ImportStudioModel(StudioModel sm, Transform[] bones, Transform parent)
    {
        var verts   = sm.VerticesPerLod[0];
        var mesh    = new Mesh { name = sm.Model.Name };
        var pos     = new Vector3[verts.Length];
        var norm    = new Vector3[verts.Length];
        var uvs     = new Vector2[verts.Length];
        var weights = new BoneWeight[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            var v = verts[i];
            pos[i]  = v.m_vecPosition;
            norm[i] = v.m_vecNormal;
            uvs[i]  = v.m_vecTexCoord;

            var bw = v.m_BoneWeights;
            var w  = new BoneWeight();
            for (int j = 0; j < bw.numbones && j < 4; j++)
            {
                switch (j)
                {
                    case 0: w.boneIndex0 = bw.bone[j]; w.weight0 = bw.weight[j]; break;
                    case 1: w.boneIndex1 = bw.bone[j]; w.weight1 = bw.weight[j]; break;
                    case 2: w.boneIndex2 = bw.bone[j]; w.weight2 = bw.weight[j]; break;
                    case 3: w.boneIndex3 = bw.bone[j]; w.weight3 = bw.weight[j]; break;
                }
            }
            weights[i] = w;
        }

        mesh.vertices    = pos;
        mesh.normals     = norm;
        mesh.uv          = uvs;
        mesh.boneWeights = weights;
        mesh.bindposes   = Matrix4x4Utils.CreateBindPoses(bones);

        var allIdx = new List<int>();
        foreach (var kv in sm.IndicesPerLod[0])
            allIdx.AddRange(kv.Value);
        mesh.triangles = allIdx.ToArray();
        mesh.RecalculateBounds();

        var go = new GameObject(sm.Model.Name);
        go.transform.parent = parent;
        var smr = go.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = mesh;
        smr.bones      = bones;
        smr.rootBone   = bones.Length > 0 ? bones[0] : null;
    }
}
