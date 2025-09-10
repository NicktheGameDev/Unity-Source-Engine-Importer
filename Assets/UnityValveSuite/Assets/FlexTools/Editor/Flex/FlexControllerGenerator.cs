using UnityEngine;
using UnityEditor;
using System.IO;
using FlexTools.Runtime.Data;
using FlexTools.Editor.Utils;
namespace FlexTools.Editor.Flex
{
    public static class FlexControllerGenerator
    {
        [MenuItem("Tools/Flex Tools/Generate Flex Controller Block")]
        private static void Generate(){
            var go=Selection.activeGameObject;
            if(go==null||!FlexUtils.HasFlexes(go)){
                EditorUtility.DisplayDialog("Flex","Select object with blendshapes.","OK");
                return;
            }
            var dm=new DataModel("flex_"+go.name);
            var json=dm.ToJson();
            string path=$"Assets/flex_{go.name}.json";
            File.WriteAllText(path,json);
            AssetDatabase.ImportAsset(path);
            Selection.activeObject=AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
    }
}
