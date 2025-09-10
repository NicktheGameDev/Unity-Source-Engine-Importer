
// RuntimeAssetGuard.cs
// Ensures runtime-generated assets never bloat the project: hide flags + editor save block.
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FormatsFixes
{
    [DefaultExecutionOrder(-10000)]
    public class RuntimeAssetGuard : MonoBehaviour
    {
        public static bool Enabled = true;

        void Awake()
        {
            Application.lowMemory += OnLowMemory;
        }

        void OnDestroy()
        {
            Application.lowMemory -= OnLowMemory;
        }

        static void OnLowMemory()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public static T MarkRuntime<T>(T obj) where T : Object
        {
            if (!obj) return obj;
            obj.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            return obj;
        }
    }

#if UNITY_EDITOR
    // Hard block any runtime asset save attempts made by stray editor scripts while playing.
    [InitializeOnLoad]
    public static class RuntimeAssetSaveBlock
    {
        static RuntimeAssetSaveBlock()
        {
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.EnteredPlayMode)
                    AssetModificationProcessorUtil.BlockSaves = true;
                else
                    AssetModificationProcessorUtil.BlockSaves = false;
            };
        }

        class AssetModificationProcessorUtil : UnityEditor.AssetModificationProcessor
        {
            public static bool BlockSaves = false;
            static string[] OnWillSaveAssets(string[] paths)
            {
                if (BlockSaves)
                {
                    Debug.LogWarning("[Guard] Blocking asset save during play to avoid project bloat.");
                    return new string[0];
                }
                return paths;
            }
        }
    }
#endif
}
