using System.Collections.Generic;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    /// <summary>
    /// ResourceManager handles the loading, caching, and retrieval of game resources such as prefabs, textures, and materials.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        // Dictionary to store cached resources
        private static Dictionary<string, Object> resourceCache = new Dictionary<string, Object>();

        /// <summary>
        /// Load a prefab by name from the Resources folder.
        /// </summary>
        /// <param name="prefabName">The name of the prefab to load.</param>
        /// <returns>The loaded prefab as a GameObject, or null if not found.</returns>
        public static GameObject LoadPrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                Debug.LogError("Prefab name cannot be null or empty.");
                return null;
            }

            // Check if the prefab is already cached
            if (resourceCache.TryGetValue(prefabName, out Object cachedPrefab))
            {
                Debug.Log($"Loaded prefab '{prefabName}' from cache.");
                return cachedPrefab as GameObject;
            }

            // Load the prefab from the Resources folder
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab != null)
            {
                // Cache the loaded prefab for future use
                resourceCache[prefabName] = prefab;
                Debug.Log($"Loaded prefab '{prefabName}' from Resources and cached.");
                return prefab;
            }
            else
            {
                Debug.LogError($"Prefab '{prefabName}' not found in Resources.");
                return null;
            }
        }

        /// <summary>
        /// Load a texture by name from the Resources folder.
        /// </summary>
        /// <param name="textureName">The name of the texture to load.</param>
        /// <returns>The loaded texture as a Texture2D, or null if not found.</returns>
        public static Texture2D LoadTexture(string textureName)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                Debug.LogError("Texture name cannot be null or empty.");
                return null;
            }

            // Check if the texture is already cached
            if (resourceCache.TryGetValue(textureName, out Object cachedTexture))
            {
                Debug.Log($"Loaded texture '{textureName}' from cache.");
                return cachedTexture as Texture2D;
            }

            // Load the texture from the Resources folder
            Texture2D texture = Resources.Load<Texture2D>(textureName);
            if (texture != null)
            {
                // Cache the loaded texture for future use
                resourceCache[textureName] = texture;
                Debug.Log($"Loaded texture '{textureName}' from Resources and cached.");
                return texture;
            }
            else
            {
                Debug.LogError($"Texture '{textureName}' not found in Resources.");
                return null;
            }
        }

        /// <summary>
        /// Unload a resource by name from the cache.
        /// </summary>
        /// <param name="resourceName">The name of the resource to unload.</param>
        public static void UnloadResource(string resourceName)
        {
            if (resourceCache.Remove(resourceName))
            {
                Debug.Log($"Unloaded resource '{resourceName}' from cache.");
            }
            else
            {
                Debug.LogWarning($"Resource '{resourceName}' not found in cache.");
            }
        }

        /// <summary>
        /// Clear all cached resources.
        /// </summary>
        public static void ClearCache()
        {
            resourceCache.Clear();
            Debug.Log("Cleared all cached resources.");
        }

        /// <summary>
        /// Check if a resource is cached.
        /// </summary>
        /// <param name="resourceName">The name of the resource to check.</param>
        /// <returns>True if the resource is cached, otherwise false.</returns>
        public static bool IsResourceCached(string resourceName)
        {
            return resourceCache.ContainsKey(resourceName);
        }
    }
}
