using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using uSource;
using uSource.Formats.Source.VTF;

[RequireComponent(typeof(RopeSim))]
[RequireComponent(typeof(SkinnedMeshRenderer))]
[ExecuteInEditMode]
public class RopeMeshRenderer : MonoBehaviour
{
    public float radius = 0.05f;
    private RopeSim ropesim;
    private SkinnedMeshRenderer meshRenderer;
    private VMTFile ropeMaterial;
    private string blackMesaMaterialPath = "/materials/cable/cable";

    void LoadRopeMaterial()
    {
        if (ropeMaterial == null)
        {
            string materialName = "cable.vmt"; // Replace with appropriate material name
            ropeMaterial = uResourceManager.LoadMaterial(blackMesaMaterialPath);
            if (ropeMaterial != null)
            {
                meshRenderer.material = ropeMaterial.Material;
                Debug.Log("Rope material loaded from Black Mesa directory: " + materialName);
            }
            else
            {
                Debug.LogWarning("Rope material not found in Black Mesa directory.");
            }
        }
    }

    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void CreateRope(Vector3 startPosition, Vector3 endPosition)
    {
        // Example of creating rope between two points
        Vector3 ropeDirection = (endPosition - startPosition).normalized;
        float ropeLength = Vector3.Distance(startPosition, endPosition);

        transform.position = startPosition;
        transform.localScale = new Vector3(transform.localScale.x, ropeLength, transform.localScale.z);
        transform.rotation = Quaternion.LookRotation(ropeDirection);
    }

    void Start()
    {
        ropesim = GetComponent<RopeSim>();
        LoadRopeMaterial();
        meshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (!ropesim.sane)
        {
            if (meshRenderer.sharedMesh != null)
            {
                meshRenderer.sharedMesh.Clear();
                meshRenderer.sharedMesh = null;
            }
            return;
        }

        // Rope mesh creation logic
        Mesh mesh = new Mesh();
        int parts = (int)(Vector3.Distance(ropesim.start.position, ropesim.end.position) * ropesim.boneDensity);
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        List<BoneWeight> weights = new List<BoneWeight>();
        float distMultiplier = Vector3.Distance(ropesim.start.position, ropesim.end.position) / (float)parts;

        // End cap
        BoneWeight capweight = new BoneWeight();
        capweight.boneIndex0 = ropesim.bones.Count - 1;
        capweight.weight0 = 1f;
        verts.Add(new Vector3(-radius, ((float)parts) * distMultiplier, -radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(0, 0));
        verts.Add(new Vector3(-radius, ((float)parts) * distMultiplier, radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(0, 1));
        verts.Add(new Vector3(radius, ((float)parts) * distMultiplier, radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(1, 1));

        verts.Add(new Vector3(-radius, ((float)parts) * distMultiplier, -radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(0, 0));
        verts.Add(new Vector3(radius, ((float)parts) * distMultiplier, radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(1, 1));
        verts.Add(new Vector3(radius, ((float)parts) * distMultiplier, -radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(1, 0));

        capweight.boneIndex0 = 0;
        capweight.weight0 = 1f;

        verts.Add(new Vector3(-radius, 0, -radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(0, 0));
        verts.Add(new Vector3(radius, 0, radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(1, 1));
        verts.Add(new Vector3(-radius, 0, radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(0, 1));

        verts.Add(new Vector3(-radius, 0, -radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(0, 0));
        verts.Add(new Vector3(radius, 0, -radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(1, 0));
        verts.Add(new Vector3(radius, 0, radius));
        weights.Add(capweight);
        uvs.Add(new Vector2(1, 1));

        for (int i = 0; i < parts; i++)
        {
            BoneWeight w1 = new BoneWeight();
            if (i == parts || i == 0)
            {
                w1.boneIndex0 = i;
                w1.weight0 = 1f;
            }
            else
            {
                w1.boneIndex0 = i - 1;
                w1.weight0 = 1f / 4f;
                w1.boneIndex1 = i;
                w1.weight1 = 2f / 4f;
                w1.boneIndex2 = i + 1;
                w1.weight2 = 1f / 4f;
            }

            BoneWeight w2 = new BoneWeight();
            if (i + 1 == parts)
            {
                w2.boneIndex0 = i + 1;
                w2.weight0 = 1f;
            }
            else
            {
                w2.boneIndex0 = i;
                w2.weight0 = 1f / 4f;
                w2.boneIndex1 = i + 1;
                w2.weight1 = 2f / 4f;
                w2.boneIndex2 = i + 2;
                w2.weight2 = 1f / 4f;
            }

            verts.Add(new Vector3(-radius, i * distMultiplier, -radius));
            weights.Add(w1);
            uvs.Add(new Vector2(0, 0));
            verts.Add(new Vector3(-radius, (i + 1) * distMultiplier, -radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));
            verts.Add(new Vector3(radius, i * distMultiplier, -radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));

            verts.Add(new Vector3(radius, i * distMultiplier, -radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));
            verts.Add(new Vector3(-radius, (i + 1) * distMultiplier, -radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));
            verts.Add(new Vector3(radius, (i + 1) * distMultiplier, -radius));
            weights.Add(w2);
            uvs.Add(new Vector2(1, 1));

            verts.Add(new Vector3(radius, i * distMultiplier, -radius));
            weights.Add(w1);
            uvs.Add(new Vector2(0, 0));
            verts.Add(new Vector3(radius, (i + 1) * distMultiplier, -radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));
            verts.Add(new Vector3(radius, i * distMultiplier, radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));

            verts.Add(new Vector3(radius, i * distMultiplier, radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));
            verts.Add(new Vector3(radius, (i + 1) * distMultiplier, -radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));
            verts.Add(new Vector3(radius, (i + 1) * distMultiplier, radius));
            weights.Add(w2);
            uvs.Add(new Vector2(1, 1));

            verts.Add(new Vector3(-radius, i * distMultiplier, radius));
            weights.Add(w1);
            uvs.Add(new Vector2(0, 0));
            verts.Add(new Vector3(radius, i * distMultiplier, radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));
            verts.Add(new Vector3(-radius, (i + 1) * distMultiplier, radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));

            verts.Add(new Vector3(radius, i * distMultiplier, radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));
            verts.Add(new Vector3(radius, (i + 1) * distMultiplier, radius));
            weights.Add(w2);
            uvs.Add(new Vector2(1, 1));
            verts.Add(new Vector3(-radius, (i + 1) * distMultiplier, radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));

            verts.Add(new Vector3(-radius, i * distMultiplier, -radius));
            weights.Add(w1);
            uvs.Add(new Vector2(0, 0));
            verts.Add(new Vector3(-radius, i * distMultiplier, radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));
            verts.Add(new Vector3(-radius, (i + 1) * distMultiplier, -radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));

            verts.Add(new Vector3(-radius, i * distMultiplier, radius));
            weights.Add(w1);
            uvs.Add(new Vector2(1, 0));
            verts.Add(new Vector3(-radius, (i + 1) * distMultiplier, radius));
            weights.Add(w2);
            uvs.Add(new Vector2(1, 1));
            verts.Add(new Vector3(-radius, (i + 1) * distMultiplier, -radius));
            weights.Add(w2);
            uvs.Add(new Vector2(0, 1));
        }

        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        for (int i = 0; i < verts.Count; i += 3)
        {
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(i + 2);
        }
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.boneWeights = weights.ToArray();

        List<Matrix4x4> bindPoses = new List<Matrix4x4>();

        foreach (Transform bone in ropesim.bones)
        {
            bindPoses.Add(bone.worldToLocalMatrix * transform.localToWorldMatrix);
        }

        mesh.bindposes = bindPoses.ToArray();

        meshRenderer.bones = ropesim.bones.ToArray();
        if (meshRenderer.sharedMesh != null)
        {
            meshRenderer.sharedMesh.Clear();
        }
        meshRenderer.sharedMesh = mesh;
    }

    void Update()
    {
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
#endif
            if (renderer.sharedMesh == null ||
                renderer.sharedMesh.triangles.Length <= 0 ||
                ropesim.transform.hasChanged ||
                renderer.bones[0] != ropesim.bones[0])
            {
                Start();
                return;
            }
#if UNITY_EDITOR
        }
#endif
    }
}
