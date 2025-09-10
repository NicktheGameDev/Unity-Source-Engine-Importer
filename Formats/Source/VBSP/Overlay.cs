using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace uSource.Decals
{
    /// <summary>
    /// Handles an Overlay geometry similarly to Decal_, but supporting the
    /// properties of a Source Engine "info_overlay" entity (Basis vectors, 
    /// attachment to multiple faces, partial UV selection, etc.).
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class Overlay : MonoBehaviour
    {
        [FormerlySerializedAs("overlayMaterial")]
        public Material OverlayMaterial;

        public Vector2 StartUV = Vector2.zero; // Combines StartU and StartV
        public Vector2 EndUV = Vector2.one;   // Combines EndU and EndV

        public int RenderOrder = 0;
        public List<int> AttachedFaceIDs = new List<int>();

        public Vector3 BasisOrigin;
        public Vector3 BasisNormal = Vector3.up;
        public Vector3 BasisU = Vector3.right;
        public Vector3 BasisV = Vector3.forward;

        public float UVScale = 1f;
        public float OverlayOffset = 0.01f;

        private Mesh overlayMesh;

        public MeshFilter OverlayMeshFilter
        {
            get
            {
                var mf = GetComponent<MeshFilter>();
                if (!mf) mf = gameObject.AddComponent<MeshFilter>();
                return mf;
            }
        }

        public MeshRenderer OverlayMeshRenderer
        {
            get
            {
                var mr = GetComponent<MeshRenderer>();
                if (!mr) mr = gameObject.AddComponent<MeshRenderer>();
                return mr;
            }
        }

        public void BuildOverlay()
        {
            if (OverlayMaterial == null)
            {
                ClearMesh();
                OverlayMeshRenderer.sharedMaterial = null;
                return;
            }

            // Basic 1x1 quad in XY plane
            Vector3[] vertices =
            {
                Vector3.zero,
                Vector3.right,
                Vector3.up,
                new Vector3(1f, 1f, 0f)
            };

            // Scale the quad by UVScale
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] *= UVScale;
            }

            int[] indices = { 0, 1, 2, 2, 1, 3 };

            // Assign UV coordinates
            Vector2[] uvs =
            {
                new Vector2(StartUV.x, StartUV.y),
                new Vector2(EndUV.x, StartUV.y),
                new Vector2(StartUV.x, EndUV.y),
                new Vector2(EndUV.x, EndUV.y)
            };

            if (overlayMesh == null)
            {
                overlayMesh = new Mesh { name = "OverlayMesh" };
            }
            else
            {
                overlayMesh.Clear();
            }

            overlayMesh.vertices = vertices;
            overlayMesh.triangles = indices;
            overlayMesh.uv = uvs;
            overlayMesh.RecalculateNormals();

            OverlayMeshFilter.sharedMesh = overlayMesh;
            OverlayMeshRenderer.sharedMaterial = OverlayMaterial;

            // Construct a matrix using basis vectors
            Vector3 xAxis = BasisU.normalized;
            Vector3 yAxis = BasisV.normalized;
            Vector3 zAxis = BasisNormal.normalized;

            Matrix4x4 basisMatrix = new Matrix4x4();
            basisMatrix.SetColumn(0, new Vector4(xAxis.x, xAxis.y, xAxis.z, 0f));
            basisMatrix.SetColumn(1, new Vector4(yAxis.x, yAxis.y, yAxis.z, 0f));
            basisMatrix.SetColumn(2, new Vector4(zAxis.x, zAxis.y, zAxis.z, 0f));
            basisMatrix.SetColumn(3, new Vector4(BasisOrigin.x, BasisOrigin.y, BasisOrigin.z, 1f));

            // Position & rotate this object based on the basis matrix
            transform.position = basisMatrix.GetColumn(3);
            Quaternion rot = Quaternion.LookRotation(
                basisMatrix.GetColumn(2),
                basisMatrix.GetColumn(1)
            );
            transform.rotation = rot;

            // Offset slightly along normal to prevent Z-fighting
            transform.position += zAxis * OverlayOffset;
        }

        private void ClearMesh()
        {
            if (overlayMesh != null)
            {
                DestroyImmediate(overlayMesh);
                overlayMesh = null;
            }
        }

        private void OnValidate()
        {
            // Clamp or reset values to valid ranges
            StartUV.x = Mathf.Clamp01(StartUV.x);
            StartUV.y = Mathf.Clamp01(StartUV.y);
            EndUV.x = Mathf.Clamp01(EndUV.x);
            EndUV.y = Mathf.Clamp01(EndUV.y);
            OverlayOffset = Mathf.Max(0f, OverlayOffset);
        }

        private void Awake()
        {
            // If there's a shared mesh assigned, make a unique copy
            if (OverlayMeshFilter.sharedMesh != null && OverlayMeshFilter.sharedMesh != overlayMesh)
            {
                overlayMesh = Instantiate(OverlayMeshFilter.sharedMesh);
                OverlayMeshFilter.sharedMesh = overlayMesh;
            }
        }

        private void OnEnable()
        {
            BuildOverlay();
        }

        private void OnDestroy()
        {
            ClearMesh();
        }

        private void Update()
        {
            // Rebuild the overlay if the transform changes
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                BuildOverlay();
            }
        }
    }
}
