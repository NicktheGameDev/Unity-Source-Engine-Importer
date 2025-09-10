using UnityEngine;
namespace ValveImporter.Editor.Utilities
{
    public static class Logger
    {
        public static void Info(string m)=>Debug.Log($"<b>[ValveImporter]</b> {m}");
        public static void Warn(string m)=>Debug.LogWarning($"<b>[ValveImporter]</b> {m}");
        public static void Error(string m)=>Debug.LogError($"<b>[ValveImporter]</b> {m}");
    }
}
