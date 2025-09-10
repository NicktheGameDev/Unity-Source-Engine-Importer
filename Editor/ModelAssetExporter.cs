using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class ModelAssetExporter
{
    [MenuItem("uSource/Export Loaded Model as Prefab")]
    public static void ExportSelectedModel()
    {
        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("No GameObject selected.");
            return;
        }
        var root = selected;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>())
        {
            var mr = mf.GetComponent<MeshRenderer>();
            var originalMesh = mf.sharedMesh;
            if (mr != null && originalMesh != null && originalMesh.blendShapeCount > 0)
            {
                // Create a mesh asset so the prefab can reference it
                var meshCopy = Object.Instantiate(originalMesh);
                meshCopy.name = originalMesh.name;
                var meshAssetPath = $"Assets/{root.name}_{mf.gameObject.name}_{originalMesh.name}.asset";
                AssetDatabase.CreateAsset(meshCopy, meshAssetPath);
                
                // Convert to skinned mesh renderer with asset mesh
                var go = mf.gameObject;
                var materials = mr.sharedMaterials;
                Object.DestroyImmediate(mr, true);
                Object.DestroyImmediate(mf, true);
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = meshCopy;
                smr.sharedMaterials = materials;
            }
        }
        var anim = root.GetComponent<Animation>();
        AnimatorController controller = null;
        if (anim != null)
        {
            var clips = new System.Collections.Generic.List<AnimationClip>();
            foreach (AnimationState state in anim)
            {
                if (state.clip != null)
                    clips.Add(state.clip);
            }
            var controllerPath = $"Assets/{root.name}_Controller.controller";
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var rootStateMachine = controller.layers[0].stateMachine;
            foreach (var clip in clips)
            {
                var state = rootStateMachine.AddState(clip.name);
                state.motion = clip;
            }
            // Remove old Animation and add Animator
            Object.DestroyImmediate(anim, true);
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }
        // Save prefab
        var prefabPath = $"Assets/{root.name}.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.UserAction);
        Debug.Log($"Exported prefab at {prefabPath}");
    }
}