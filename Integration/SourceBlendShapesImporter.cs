// File: Assets/Editor/SourceIOFlexDeltaAllLODsImporter.cs

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using uSource;
using uSource.Formats.Source.MDL;

public class SourceIOFlexDeltaAllLODsImporter : EditorWindow
{
    private string mdlPath = "";

    [MenuItem("uSource/Import Model (FlexDelta All LODs)")]
    public static void ShowWindow() =>
        GetWindow<SourceIOFlexDeltaAllLODsImporter>("FlexDelta All LODs Importer");

    private void OnGUI()
    {
        GUILayout.Label("1) Wybierz .mdl", EditorStyles.boldLabel);
        if (GUILayout.Button("Pick .mdl File…"))
        {
            var p = EditorUtility.OpenFilePanel("Select MDL", "", "mdl");
            if (!string.IsNullOrEmpty(p)) mdlPath = p;
        }
        EditorGUILayout.LabelField("Selected:", mdlPath);

        GUI.enabled = File.Exists(mdlPath);
        if (GUILayout.Button("2) Importuj wszystkie LOD-y z GetFlexDelta"))
            ImportAllLODs();
        GUI.enabled = true;
    }

    private void ImportAllLODs()
    {
        // 0) companion files
        string dir = Path.GetDirectoryName(mdlPath);
        string baseName = Path.GetFileNameWithoutExtension(mdlPath);
        string vvdPath = Path.Combine(dir, baseName + ".vvd");
        string vtxPath = new[] { ".dx80.vtx", ".dx90.vtx", ".sw.vtx", ".vtx" }
                           .Select(ext => Path.Combine(dir, baseName + ext))
                           .FirstOrDefault(File.Exists);

        if (!File.Exists(mdlPath) || !File.Exists(vvdPath) || string.IsNullOrEmpty(vtxPath))
        {
            Debug.LogError($"❌ Brakuje plików:\n • MDL: {mdlPath}\n • VVD: {vvdPath}\n • VTX: {vtxPath}");
            return;
        }

        // 1) Load MDL + raw flex-data
        MDLFile mdl;
        try
        {
            mdl = MDLFile.Load(mdlPath,parseAnims: true);
            using var ms = new MemoryStream(buffer: MDLFile.mdldata);
            using var reader = new uReader(ms);
            mdl.LoadFlexData(reader);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ MDL lub LoadFlexData failed: {e.Message}");
            return;
        }

        // 2) Load geometry
        var vvd = new VVDFile(File.OpenRead(vvdPath), mdl);
        var vtx = new VTXFile(File.OpenRead(vtxPath), mdl, vvd);

        // 3) Iterate bodyparts → models → LODs
        for (int bp = 0; bp < mdl.MDL_Header.bodypart_count; bp++)
        {
            var part = mdl.MDL_Bodyparts[bp];
            for (int mi = 0; mi < part.Models.Length; mi++)
            {
                var model = part.Models[mi];
                int lodCount = model.NumLODs;
                for (int lod = 0; lod < lodCount; lod++)
                {
                    // build raw mesh
                    var verts = model.VerticesPerLod[lod]
                                .Select(v => v.m_vecPosition)
                                .ToArray();
                    var tris = model.IndicesPerLod[lod]
                                .Values.SelectMany(l => l)
                                .ToArray();

                    var mesh = new Mesh { name = $"{baseName}_bp{bp}_m{mi}_lod{lod}" };
                    mesh.vertices = verts;
                    mesh.triangles = tris;
                    mesh.RecalculateNormals();

                    // single bind-pose + boneWeight
                    mesh.bindposes = new[] { Matrix4x4.identity };
                    var bw = new BoneWeight[verts.Length];
                    for (int i = 0; i < bw.Length; i++)
                    {
                        bw[i].boneIndex0 = 0;
                        bw[i].weight0 = 1f;
                    }
                    mesh.boneWeights = bw;

                    // blendshapes via GetFlexDelta
                    int added = 0;
                    for (int fi = 0; fi < mdl.MDL_FlexDescs.Length; fi++)
                    {
                        // bezpieczne pobranie nazwy:
                        string name;
                        try
                        {
                            // zamiast
// string name = desc.pszFACS(MDLFile.mdldata, 0);

name = mdl.GetFlexName(0);

                        }
                        catch
                        {
                            // nieprawidłowy offset – pomiń
                            continue;
                        }

                        // unikamy duplikatów
                        if (mesh.GetBlendShapeIndex(name) != -1) continue;

                        var dv = new Vector3[verts.Length];
                        var dn = new Vector3[verts.Length];
                        bool hasDelta = false;
                        for (int vi = 0; vi < verts.Length; vi++)
                        {
                            var d = mdl.GetFlexDelta(fi, vi, 1f);
                            dv[vi] = d;
                            if (d != Vector3.zero) hasDelta = true;
                        }
                        if (hasDelta)
                        {
                            mesh.AddBlendShapeFrame(name, 100f, dv, dn, null);
                            added++;
                        }
                    }

                    Debug.Log($"[DBG] {mesh.name}: blendShapeCount={mesh.blendShapeCount}");

                    // save mesh asset
                    string meshPath = $"Assets/{mesh.name}.asset";
                    if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) != null)
                        AssetDatabase.DeleteAsset(meshPath);
                    AssetDatabase.CreateAsset(mesh, meshPath);
                    AssetDatabase.SaveAssets();

                    // create prefab + SkinnedMeshRenderer
                    var go = new GameObject(mesh.name + "_GO");
                    var bone = new GameObject("Bone_0");
                    bone.transform.SetParent(go.transform, false);

                    var smr = go.AddComponent<SkinnedMeshRenderer>();
                    smr.sharedMesh = mesh;
                    smr.bones = new[] { bone.transform };
                    smr.rootBone = bone.transform;
                    smr.sharedMaterial = new Material(Shader.Find("Standard"));

                    string prefabPath = $"Assets/{mesh.name}.prefab";
                    if (File.Exists(prefabPath))
                        AssetDatabase.DeleteAsset(prefabPath);
                    PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                    DestroyImmediate(go);
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Zaimportowano wszystkie LOD-y z blendshape’ami (FlexDelta).");
    }
    // w MDLFile:
    



}

