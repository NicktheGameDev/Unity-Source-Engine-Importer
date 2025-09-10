using UnityEditor;
using System.IO;
namespace ValveImporter.Editor.Utilities
{
    public static class FileUtils
    {
        public static string OpenFilePanel(string title,string dir,string ext)
            => EditorUtility.OpenFilePanel(title, dir, ext);
        public static string[] ReadAllLines(string path)
            => File.ReadAllLines(path,System.Text.Encoding.UTF8);
        public static string SaveFilePanel(string title,string dir,string name,string ext)
            => EditorUtility.SaveFilePanel(title, dir, name, ext);
    }
}
