using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ValveImporter.Editor.Importers
{
    public static class SMDImporter
    {
        /// <summary>
        /// Import .smd. If bonesOverride != null, mesh is skinned to those bones instead of generating a new skeleton.
        /// </summary>
        public static void ImportSMDPath(string path, out GameObject modelGO) => ImportSMDPath(path, null, out modelGO);

        public static void ImportSMDPath(string path, Transform[] bonesOverride, out GameObject modelGO)
        {
            modelGO = null;
            if (!File.Exists(path))
            {
                Debug.LogError($"SMDImporter: file not found: {path}");
                return;
            }

            var lines = File.ReadAllLines(path);
            var model = ValveImporter.Editor.Parsers.SMDParser.Parse(lines);

            int triCount  = model.Tris.Count;
            int vertCount = triCount * 3;

            var vertices = new Vector3[vertCount];
            var normals  = new Vector3[vertCount];
            var uvs      = new Vector2[vertCount];
            var tris     = new int[vertCount];
            var weights  = new BoneWeight[vertCount];

            for (int i = 0; i < triCount; i++)
            {
                var t = model.Tris[i];
                int b = i * 3;
                Pack(t.V0, b + 0);
                Pack(t.V1, b + 1);
                Pack(t.V2, b + 2);

                void Pack(ValveImporter.Editor.Parsers.SMDVertex v, int dst)
                {
                    vertices[dst] = v.Pos;
                    normals[dst]  = v.Normal;
                    uvs[dst]      = v.UV;
                    tris[dst]     = dst;

                    var bw = new BoneWeight();
                    for (int k = 0; k < v.BoneIds.Length && k < 4; k++)
                    {
                        switch (k)
                        {
                            case 0: bw.boneIndex0 = v.BoneIds[k]; bw.weight0 = v.Weights[k]; break;
                            case 1: bw.boneIndex1 = v.BoneIds[k]; bw.weight1 = v.Weights[k]; break;
                            case 2: bw.boneIndex2 = v.BoneIds[k]; bw.weight2 = v.Weights[k]; break;
                            case 3: bw.boneIndex3 = v.BoneIds[k]; bw.weight3 = v.Weights[k]; break;
                        }
                    }
                    weights[dst] = bw;
                }
            }

            var mesh = new Mesh
            {
                name       = Path.GetFileNameWithoutExtension(path),
                vertices   = vertices,
                normals    = normals,
                uv         = uvs,
                triangles  = tris,
                boneWeights = weights
            };
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            Shader hdrp = Shader.Find("HDRP/Lit") ?? Shader.Find("Lit") ?? Shader.Find("Standard");

            // CASE 1: create fresh skeleton
            if (bonesOverride == null)
            {
                ValveImporter.Editor.Parsers.SMDBuilder.BuildSkeleton(model, mesh, out modelGO);
                var smr = modelGO.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.sharedMaterial = new Material(hdrp);
            }
            else
            {
                // reuse
                modelGO = new GameObject(mesh.name);
                var smr = modelGO.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = mesh;
                smr.bones      = bonesOverride;
                smr.rootBone   = bonesOverride.FirstOrDefault(b => b.parent == null) ?? bonesOverride[0];
                mesh.bindposes = bonesOverride.Select(b => b.worldToLocalMatrix * bonesOverride[0].parent.localToWorldMatrix).ToArray();
                smr.sharedMaterial = new Material(hdrp);
            }

            Debug.Log($"SMDImporter: Imported '{mesh.name}' ({triCount} tris).");
        }
    }
}
