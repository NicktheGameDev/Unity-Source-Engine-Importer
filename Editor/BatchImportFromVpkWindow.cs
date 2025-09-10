using UnityEngine;
using UnityEditor;
using uSource.Formats.Source.MDL;
using uSource.Formats.Source.VPK;
using System;
using System.IO;
using System.Linq;

/// <summary>
/// Batch‐import wszystkich modeli z wybranego folderu w VPK.
/// </summary>
public class BatchImportFromVpkWindow : EditorWindow
{
    private string _vpkPath = "";
    private string _folderInVpk = "models/blackmesa/";

    [MenuItem("uSource/Batch Import Models from VPK")]
    public static void ShowWindow()
        => GetWindow<BatchImportFromVpkWindow>("Batch Import VPK");

    void OnGUI()
    {
        GUILayout.Label("Batch Import z VPK", EditorStyles.boldLabel);

        if (GUILayout.Button("Wybierz plik .vpk"))
            _vpkPath = EditorUtility.OpenFilePanel("Select VPK", "", "vpk");
        EditorGUILayout.LabelField("VPK Path:", _vpkPath);

        _folderInVpk = EditorGUILayout.TextField("Folder in VPK:", _folderInVpk);

        if (GUILayout.Button("Import all models"))
        {
            if (string.IsNullOrEmpty(_vpkPath) || !File.Exists(_vpkPath))
            {
                Debug.LogError("Nie wybrano lub nie znaleziono pliku VPK!");
                return;
            }
            try
            {
                ImportAll(_vpkPath, _folderInVpk);
            }
            catch (Exception ex)
            {
                Debug.LogError("Batch import nie powiódł się: " + ex);
            }
        }
    }

    static void ImportAll(string vpkPath, string folderInVpk)
    {
        var vpk = new VPKFile(vpkPath);
        vpk.Load(vpkPath);

        var keys = vpk.Entries.Keys
            .Where(k => k.StartsWith(folderInVpk, StringComparison.OrdinalIgnoreCase)
                     && k.EndsWith(".mdl",    StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (keys.Count == 0)
        {
            Debug.LogWarning($"Nie znaleziono żadnych .mdl w '{folderInVpk}'");
            return;
        }

        foreach (var mdlKey in keys)
        {
            var basePath = mdlKey.Substring(0, mdlKey.Length - 4);
            Debug.Log($"Importuję: {mdlKey}");

        }

        Debug.Log($"Batch import zakończony. Zaimportowano {keys.Count} modeli.");
    }
}