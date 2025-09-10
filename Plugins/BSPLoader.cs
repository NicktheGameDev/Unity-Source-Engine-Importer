
using UnityEngine;
using System.IO;

namespace uSource
{
    // Basic BSP loader for entity lump extraction
    public static class BSPLoader
    {
        public static string LoadEntities(string bspPath)
        {
            byte[] data = File.ReadAllBytes(bspPath);
            // TODO: Implement header parsing and entity lump extraction.
            // For now, just return raw bytes as string.
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
