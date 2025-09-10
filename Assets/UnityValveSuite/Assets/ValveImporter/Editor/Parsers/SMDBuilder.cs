using System.Linq;
using UnityEngine;

namespace ValveImporter.Editor.Parsers
{
    internal static class SMDBuilder
    {
        /// <summary>
        /// Builds skeleton GameObjects, bindposes and attaches SkinnedMeshRenderer.
        /// </summary>
        public static void BuildSkeleton(SMDModel model, Mesh mesh, out GameObject rootGO)
        {
            rootGO = new GameObject(mesh.name + "_Armature");
            var bonesT = new Transform[model.Bones.Count];

            // create transforms
            foreach (var b in model.Bones)
            {
                var t = new GameObject(b.Name).transform;
                bonesT[b.Id] = t;
            }

            // parenting + local pose
            foreach (var b in model.Bones)
            {
                Transform t = bonesT[b.Id];
                t.localPosition   = b.Position;
                t.localEulerAngles = b.Rotation;

                t.parent = b.Parent >= 0 ? bonesT[b.Parent] : rootGO.transform;
            }

            // bindposes
            var bindposes = new Matrix4x4[bonesT.Length];
            for (int i = 0; i < bonesT.Length; i++)
                bindposes[i] = bonesT[i].worldToLocalMatrix * rootGO.transform.localToWorldMatrix;

            mesh.bindposes = bindposes;

            // Mesh GO
            var meshGO = new GameObject(mesh.name);
            meshGO.transform.SetParent(rootGO.transform, false);

            var smr = meshGO.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh;
            smr.bones      = bonesT;
            var o = rootGO;
            smr.rootBone   = bonesT.FirstOrDefault(t => t.parent == o.transform) ?? bonesT[0];
        }
    }
}
