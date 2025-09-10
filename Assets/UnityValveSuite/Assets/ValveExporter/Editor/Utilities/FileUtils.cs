using UnityEditor;
using System.IO;
namespace ValveExporter.Editor.Utilities
{
    public static class FileUtils
    {
        public static string SaveFile(string title,string dir,string name,string ext)
            => EditorUtility.SaveFilePanel(title,dir,name,ext);
    }
}
