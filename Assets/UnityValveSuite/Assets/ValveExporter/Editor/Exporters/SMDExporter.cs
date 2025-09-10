using UnityEditor;
using UnityEngine;
using ValveExporter.Editor.Utilities;
using System.IO;
namespace ValveExporter.Editor.Exporters
{
    public static class SMDExporter
    {
        public static void ExportSelectionToSMD(){
            var sel=Selection.activeGameObject;
            if(sel==null){ Debug.LogError("Select root"); return;}
            string path=FileUtils.SaveFile("Save SMD","Assets",sel.name,"smd");
            if(string.IsNullOrEmpty(path)) return;
            File.WriteAllText(path,"// dummy SMD exported");
            AssetDatabase.Refresh();
        }
    }
}
