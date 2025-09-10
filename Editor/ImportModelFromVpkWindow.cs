using UnityEngine;
using UnityEditor;
using uSource.Formats.Source.MDL;
using uSource.Formats.Source.VPK;
using System;
using System.IO;

public class ImportModelFromVpkWindow : EditorWindow
{
    private string _vpkFilePath = "";
    private string _mdlPathInVpk = "models/yourmodel.mdl";
    private MDLProcessor MDLProcessor;


    [MenuItem("uSource/Import Model from VPK")]
    public static void ShowWindow() => GetWindow<ImportModelFromVpkWindow>("Import from VPK");

    void OnGUI()
    {
        GUILayout.Label("Import model from VPK", EditorStyles.boldLabel);
        if (GUILayout.Button("Select VPK file"))
            _vpkFilePath = EditorUtility.OpenFilePanel("Select VPK", "", "vpk");
        EditorGUILayout.LabelField("VPK Path", _vpkFilePath);
        _mdlPathInVpk = EditorGUILayout.TextField("MDL Path in VPK", _mdlPathInVpk);

        if (GUILayout.Button("Import Model"))
        {
            if (string.IsNullOrEmpty(_vpkFilePath))
            {
                Debug.LogError("Select a VPK file first.");
                return;
            }
            try
            {
                var vpk = new VPKFile(_vpkFilePath);
                vpk.Load(_vpkFilePath);

                var key = _mdlPathInVpk.ToLower();
              

               
            }
            catch (Exception ex)
            {
                Debug.LogError("Import failed: " + ex.Message);
            }
        }
    }
}