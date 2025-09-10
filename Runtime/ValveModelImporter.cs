using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using uSource.Formats.Source.MDL;
using UnityEngine.Playables;
using UnityEngine.Animations;
using uSource.Formats.Source.VTA;

namespace uSource.Runtime
{
    public static unsafe class ValveModelImporter
    {
        /// <summary>
        /// Import MDL with optional VVD/VTA streams into a GameObject ready for runtime/editor use.
        /// </summary>
        public static GameObject Import(
            MDLFile mdl,
            Stream vvdStream = null,
            Stream vtaStream = null,
            string vtaPath = null)
        {
            if (mdl == null) throw new ArgumentNullException(nameof(mdl));

            // 1) Create root GO
            var root = new GameObject(Path.GetFileNameWithoutExtension(mdl.MDL_Header.Name));

            // ADDED: ensure Animator component exists so users can animate the model in Unity
            if (root.GetComponent<UnityEngine.Animator>() == null)
            {
                root.AddComponent<UnityEngine.Animator>();
            }


            // 2) Build bone hierarchy
            Transform[] bones = new Transform[mdl.MDL_Header.bone_count];
            for (int i = 0; i < bones.Length; i++)
            {
                var boneData = mdl.MDL_StudioBones[i];
                var go = new GameObject(mdl.MDL_BoneNames[i]);
                bones[i] = go.transform;
                if (boneData.parent == -1)
                    go.transform.parent = root.transform;
                else
                    go.transform.parent = bones[boneData.parent];
                go.transform.localPosition = boneData.pos;
                go.transform.localRotation = boneData.quat;
            // Build bone path dictionary for animations
            var bonePathDict = new Dictionary<int, string>();
            for (int bi = 0; bi < bones.Length; bi++)
            {
                var path = bones[bi].name;
                var parent = bones[bi].parent;
                while (parent != root.transform && parent != null)
                {
                    path = parent.name + "/" + path;
                    parent = parent.parent;
                }
                bonePathDict[bi] = path;
            }

            // Generate animations and play first sequence
            var animator = root.GetComponent<Animator>();
            var graph = PlayableGraph.Create(root.name + "_AnimGraph");
            var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
            if (mdl.Animations != null && mdl.Animations.Length > 0)
            {
                var mixer = AnimationMixerPlayable.Create(graph, mdl.Animations.Length);
                output.SetSourcePlayable(mixer);
                for (int ai = 0; ai < mdl.Animations.Length; ai++)
                {
                    var clip = mdl.CreateAnimationClip(mdl.Animations[ai] ,null);
                    clip.name = mdl.Animations[ai].studioAnim.sznameindex.ToString();
                    clip.wrapMode = WrapMode.Loop;
                    var playable = AnimationClipPlayable.Create(graph, clip);
                    graph.Connect(playable, 0, mixer, ai);
                    mixer.SetInputWeight(ai, ai == 0 ? 1f : 0f);
                }
                graph.Play();
            }
            }

            // 3) Mesh & renderer
            var mesh = new Mesh { name = mdl.MDL_Header.Name };
            var smr = root.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh;
            smr.rootBone = bones[0];
            smr.bones = bones;

            // TODO: Populate mesh vertices/normals/uv from VVD – skip for brevity (placeholder wireframe cube)
            mesh.vertices = new Vector3[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f)
            };
            mesh.triangles = new int[] {
                0,2,1, 0,3,2,
                4,5,6, 4,6,7,
                0,1,5, 0,5,4,
                2,3,7, 2,7,6,
                0,4,7, 0,7,3,
                1,2,6, 1,6,5
            };
            mesh.RecalculateNormals();

            // 4) Apply flex/blend-shapes from VTA
            if (vtaStream != null || vtaPath != null)
            {
                VTAFile vta = null;
                if (vtaStream != null)
                    vta = VTAFile.Load(new StreamReader(vtaStream));
                else if (vtaPath != null)
                    vta = VTAFile.Load(vtaPath);

                if (vta != null)
                    ApplyVtaBlendShapes(mesh, vta, mdl);
            }

            // 5) Jiggle bones
            foreach (var j in mdl.MDL_JiggleBones)
            {
                int idx = (int)j.baseMass;
                Transform boneTx = bones[idx];
                if (boneTx == null) continue;
                var jb = boneTx.gameObject.AddComponent<JiggleBone>();
                jb.Init(boneTx, j.baseMass, j.baseStiffness, j.baseDamping, j.length);
            }

            // 6) Flex controller component
            var ctrlMap = new Dictionary<string, int>();
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                ctrlMap[mesh.GetBlendShapeName(i)] = i;
            }
            var flexComp = root.AddComponent<FlexController>();
            flexComp.Init(ctrlMap);

            
            // 7) MDL events component
            if (mdl.MDL_Events != null && mdl.MDL_Events.Length > 0)
            {
                var evComp = root.AddComponent<MDLEventComponent>();
                evComp.events = new MDLEventComponent.MDLEvent[mdl.MDL_Events.Length];
                for (int i = 0; i < mdl.MDL_Events.Length; i++)
                {
                    if (mdl.MDL_Events != null)
                        evComp.events[i] = new MDLEventComponent.MDLEvent
                        {
                            name = mdl.MDL_Events[i].options.ToString(),
                            type = mdl.MDL_Events[i].@event,
                            cycle = mdl.MDL_Events[i].cycle
                        };
                }
            }
    return root;
        }

        private static void ApplyVtaBlendShapes(Mesh mesh, VTAFile vta, MDLFile mdl)
        {
            if (vta.VertexCount != mesh.vertexCount)
            {
                Debug.LogWarning($"VTA vertex count {vta.VertexCount} ≠ mesh {mesh.vertexCount}; skipping flex import.");
                return;
            }

            // base frame
            var baseVertices = vta.Frames[0].Positions;

            for (var f = 1; f < vta.Frames.Length; f++)
            {
                var frame = vta.Frames[f];
                var flexName = mdl.MDL_FlexDescs[frame.Index].ToString();
                var delta = new Vector3[mesh.vertexCount];
                for (int v = 0; v < mesh.vertexCount; v++)
                    delta[v] = frame.Positions[v] - baseVertices[v];

                mesh.AddBlendShapeFrame(flexName, 100f, delta, null, null);
            }
        }
    }
}