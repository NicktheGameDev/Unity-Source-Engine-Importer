// Assets/Editor/BlendshapeFilterPostProcessor.cs
using UnityEngine;
using UnityEditor;
using System.Linq;

public class BlendshapeFilterPostProcessor : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        var skinned = g.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinned)
        {
            var mesh = smr.sharedMesh;
            if (mesh == null || mesh.blendShapeCount == 0)
                continue;

            bool isHeadMesh = smr.bones.Any(b => b != null && b.name.ToLower().Contains("head"));
            if (isHeadMesh)
                continue;

            var newMesh = Object.Instantiate(mesh);
            newMesh.name = mesh.name + "_NoBlendshapes";
            StripBlendshapes(newMesh, Enumerable.Empty<string>());
            smr.sharedMesh = newMesh;
        }
    }

    static void StripBlendshapes(Mesh mesh, System.Collections.Generic.IEnumerable<string> keepNames)
    {
        var verts = mesh.vertices;
        var tris  = mesh.triangles;
        var norms = mesh.normals;
        var uvs   = mesh.uv;
        var mat   = mesh.bindposes;
        var bones = mesh.boneWeights;

        var clean = new Mesh { name = mesh.name };
        clean.vertices    = verts;
        clean.triangles   = tris;
        clean.normals     = norms;
        clean.uv          = uvs;
        clean.bindposes   = mat;
        clean.boneWeights = bones;

        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string bsName = mesh.GetBlendShapeName(i);
            if (!keepNames.Contains(bsName))
                continue;

            int frameCount = mesh.GetBlendShapeFrameCount(i);
            for (int f = 0; f < frameCount; f++)
            {
                float weight = mesh.GetBlendShapeFrameWeight(i, f);
                var deltaV = new Vector3[mesh.vertexCount];
                var deltaN = new Vector3[mesh.vertexCount];
                var deltaT = new Vector3[mesh.vertexCount];
                mesh.GetBlendShapeFrameVertices(i, f, deltaV, deltaN, deltaT);
                clean.AddBlendShapeFrame(bsName, weight, deltaV, deltaN, deltaT);
            }
        }

        mesh.Clear();
        mesh.vertices     = clean.vertices;
        mesh.triangles    = clean.triangles;
        mesh.normals      = clean.normals;
        mesh.uv           = clean.uv;
        mesh.bindposes    = clean.bindposes;
        mesh.boneWeights  = clean.boneWeights;
        for (int i = 0; i < clean.blendShapeCount; i++)
        {
            string name = clean.GetBlendShapeName(i);
            int frames  = clean.GetBlendShapeFrameCount(i);
            for (int f = 0; f < frames; f++)
            {
                float w = clean.GetBlendShapeFrameWeight(i, f);
                var dV = new Vector3[clean.vertexCount];
                var dN = new Vector3[clean.vertexCount];
                var dT = new Vector3[clean.vertexCount];
                clean.GetBlendShapeFrameVertices(i, f, dV, dN, dT);
                mesh.AddBlendShapeFrame(name, w, dV, dN, dT);
            }
        }
    }
}
