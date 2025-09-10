using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using uSource.Formats.Source.VPK;
using uSource.Formats.Source.MDL;

public class MDLProcessor : MonoBehaviour
{
    [Header("Model Files (relative to StreamingAssets or full path)")]
    [Tooltip("Ścieżka do pliku .mdl (może być w StreamingAssets lub pełna absolutna)")]
    public string mdlFilePath;

    [Tooltip("Jeśli masz .vvd/.vta obok .mdl, zostaną automatycznie załadowane.")]
    public bool autoLoadVvdAndVta = true;

    [Header("Features")]
    public bool enableJiggleBones = true;
    public bool enableEyeMovement = true;

    private MDLFile _mdl;
    private Transform[] _boneTransforms;
    private List<JiggleBone> _jiggles = new List<JiggleBone>();

    void Start()
    {
        if (string.IsNullOrEmpty(mdlFilePath))
        {
            Debug.LogError("MDLProcessor: nie ustawiono mdlFilePath!");
            enabled = false;
            return;
        }

        try
        {
            if (autoLoadVvdAndVta)
                _mdl = MDLFile.Load(mdlFilePath,parseAnims: true, parseHitboxes: false);
            else
            {
                using var fs = File.OpenRead(mdlFilePath);
               _mdl = MDLFile.Load(Path.GetDirectoryName(mdlFilePath),parseAnims: true, parseHitboxes: false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"MDLProcessor: nie udało się wczytać MDL: {ex.Message}");
            enabled = false;
            return;
        }

        _boneTransforms = new Transform[_mdl.MDL_Header.bone_count];
        for (int i = 0; i < _mdl.MDL_Header.bone_count; i++)
        {
            var name = _mdl.MDL_BoneNames[i];
            var t = transform.Find(name);
            _boneTransforms[i] = t; 
            if (t == null)
                Debug.LogWarning($"Bone '{name}' nie znaleziony w hierarchii.");
        }

        if (enableJiggleBones)
            InitializeJiggleBones();

        Debug.Log("MDLProcessor: model załadowany poprawnie.");
    }

    void InitializeJiggleBones()
    {
        _jiggles.Clear();
        foreach (var j in _mdl.MDL_JiggleBones)
        {
            int idx = (int)j.baseMass;
            var boneTx = _boneTransforms[idx];
            if (boneTx == null) continue;

            var jb = new JiggleBone(boneTx, j.baseMass, j.baseStiffness, j.baseDamping, j.length);
            _jiggles.Add(jb);
        }
        Debug.Log($"MDLProcessor: zainicjowano {_jiggles.Count} jiggle-bones.");
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (enableJiggleBones)
        {
            foreach (var jb in _jiggles)
                jb.UpdateJiggleBone(dt, jb.BoneTransform.parent.position, transform.position);
        }

        if (enableEyeMovement && Camera.main != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            foreach (var eye in _mdl.MDL_Eyeballs)
            {
                var t = _boneTransforms[eye.bone];
                if (t == null) continue;
                var dir = (camPos - t.position).normalized;
                var targetRot = Quaternion.LookRotation(dir);
                t.rotation = Quaternion.Slerp(t.rotation, targetRot, 0.1f);
            }
        }
    }

    /// <summary>
    /// Static import wrapper: creates GameObject from MDLFile and optional VVD/VTA streams.
    /// </summary>
    public static Transform Import(MDLFile model, Stream vvdStream, Stream vtaStream)
    {
        // Use existing importer to build GameObject
        var go = model.BuildModel();
        return go;
    }
}