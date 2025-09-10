using System;
using System.IO;
using UnityEngine;
using uSource.Formats.Source.MDL;
using uSource.Formats.Source.VPK;  // ← the VPK importer namespace

public class MDLFileHolder : MonoBehaviour
{
    [Tooltip("Path under StreamingAssets, e.g. 'models/pak01_dir.vpk'")]
    public string vpkArchivePath;

    [Tooltip("Path inside the VPK, e.g. 'models/characters/hostage.mdl'")]
    public string mdlEntryPath;

    [HideInInspector] public MDLFile mdlFile;

    void Awake()
    {
        if (string.IsNullOrEmpty(vpkArchivePath) || string.IsNullOrEmpty(mdlEntryPath))
        {
            Debug.LogError("MDLFileHolder: both vpkArchivePath and mdlEntryPath must be set!");
            return;
        }

        // 1) open the VPK
        string fullVpk = vpkArchivePath; // patched: direct path, no StreamingAssets dependency
        if (!File.Exists(fullVpk))
        {
            Debug.LogError($"MDLFileHolder: cannot find VPK at {fullVpk}");
            return;
        }

        VPKFile vpk;
        try
        {
            vpk = new VPKFile(fullVpk);
        }
        catch (Exception e)
        {
            Debug.LogError($"MDLFileHolder: failed to open VPK: {e}");
            return;
        }

        // 2) look up the .mdl entry (VPK keys are all lowercase, forward-slashes)
        string key = mdlEntryPath.Replace('\\','/').ToLowerInvariant();
        if (!vpk.Entries.TryGetValue(key, out var entry))
        {
            Debug.LogError($"MDLFileHolder: '{mdlEntryPath}' not found inside {vpkArchivePath}");
            return;
        }

        // 3) pull raw bytes
        //byte[] mdlBytes = vpk.ReadFileData(entry);

        // 4) hand them off to your MDLFile parser
       // using (var ms = new MemoryStream(mdlBytes))
        {
            //      mdlFile = new MDLFile(ms, parseAnims: true, parseHitboxes: true);
        }

        Debug.Log($"Loaded MDL from VPK: {mdlEntryPath}");
    }
}