using UnityEditor;
using UnityEngine;
using ValveImporter.Editor.Utilities;
using System.IO;
using Logger = ValveImporter.Editor.Utilities.Logger;

namespace ValveImporter.Editor.Importers
{
    public static class DMXImporter
    {
        public static void ImportDMXFile(){
            string path=FileUtils.OpenFilePanel("Select DMX","Assets","dmx");
            if(string.IsNullOrEmpty(path)) return;
            var dest=Path.Combine("Assets",Path.GetFileName(path));
            File.Copy(path,dest,true);
            AssetDatabase.ImportAsset(dest);
            Selection.activeObject=AssetDatabase.LoadAssetAtPath<TextAsset>(dest);
            Logger.Info("Imported DMX "+dest);
        }
    }
}
