
using System;
using UnityEngine;
using uSource.Formats.Source.VTA;

namespace uSource.Formats.Source.MDL
{
    /// <summary>
    /// Utility that converts VTA frames into Unity blend shapes.
    /// </summary>
    public static class BlendShapeBuilder
    {
        /// <summary>
        /// Adds Unity blend shapes to the supplied mesh from VTA frames.
        /// </summary>
        /// <param name="mesh">Target mesh (must match vertex order of VTA).</param>
        /// <param name="vta">Parsed VTA file.</param>
        /// <param name="flexDescs">Optional names from MDL flex descriptors; if null, generic names are used.</param>
        public static void ApplyBlendShapes(Mesh mesh, VTAFile vta, mstudioflexdesc_t[] flexDescs = null)
        {
            if (mesh.vertexCount != vta.VertexCount)
                throw new ArgumentException($"Mesh vertex count {mesh.vertexCount} differs from VTA {vta.VertexCount}");

            // rest pose (frame 0) is baseline; we compute deltas relative to it
            var restPos = vta.Frames[0].Positions;
            var restNorm = vta.Frames[0].Normals;

            for (int i = 1; i < vta.Frames.Length; i++)
            {
                var frame = vta.Frames[i];

                var deltaPos = new Vector3[vta.VertexCount];
                var deltaNorm = new Vector3[vta.VertexCount];

                for (int v = 0; v < vta.VertexCount; v++)
                {
                    deltaPos[v] = frame.Positions[v] - restPos[v];
                    deltaNorm[v] = frame.Normals[v] - restNorm[v];
                }

                var shapeName = flexDescs != null && i - 1 < flexDescs.Length
                    ? flexDescs[i - 1].GetFlexName()
                    : $"frame_{frame.Time}";
                mesh.AddBlendShapeFrame(shapeName, 100f, deltaPos, deltaNorm, null);
            }
        }
    }
}
