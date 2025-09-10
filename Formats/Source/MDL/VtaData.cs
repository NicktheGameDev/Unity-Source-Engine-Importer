using System;
using System.Collections.Generic;
using UnityEngine;
using USource.Model.Flex;

[CreateAssetMenu(menuName = "SourceIO/Vta Data (Advanced)")]
public class VtaData : ScriptableObject
{
    [Header("Metadata")]
    [Tooltip("Frames‑per‑second recorded in the original .vta (defaults to 30)")]
    public float fps = 30f;

    [Tooltip("Path of the source .vta file (for reference only)")]
    public string sourceFile;

    [Header("Raw list of flex frames (each item knows do którego flexa należy)")]
    public List<VtaSeparatorSmooth.FlexFrame> flexFrames = new List<VtaSeparatorSmooth.FlexFrame>();

    // ----------- CACHED LOOKUP ------------
    private Dictionary<int, List<VtaSeparatorSmooth.FlexFrame>> _byFlex;

    /// <summary>Grouped view: key = flex index, value = chronologically ordered frames.</summary>
    public IReadOnlyDictionary<int, List<VtaSeparatorSmooth.FlexFrame>> FramesByFlex
    {
        get
        {
            if (_byFlex == null) BuildCache();
            return _byFlex;
        }
    }

    public IEnumerable<int> FlexIndices => FramesByFlex.Keys;

    /// <summary>Get all frames for a given flex index (or null).</summary>
    public List<VtaSeparatorSmooth.FlexFrame> GetFrames(int flexIndex)
    {
        FramesByFlex.TryGetValue(flexIndex, out var list);
        return list;
    }

    /// <summary>Return specific frame or null if out of range.</summary>
    public VtaSeparatorSmooth.FlexFrame GetFrame(int flexIndex, int frameNumber)
    {
        var list = GetFrames(flexIndex);
        if (list == null || frameNumber < 0 || frameNumber >= list.Count) return null;
        return list[frameNumber];
    }

    private void BuildCache()
    {
        _byFlex = new Dictionary<int, List<VtaSeparatorSmooth.FlexFrame>>();
        foreach (var fr in flexFrames)
        {
            if (!_byFlex.TryGetValue(fr.FlexIndex, out var list))
                _byFlex[fr.FlexIndex] = list = new List<VtaSeparatorSmooth.FlexFrame>();
            list.Add(fr);
        }
        foreach (var kv in _byFlex)
            kv.Value.Sort((a,b) => a.FlexIndex.CompareTo(b.FlexIndex)); // keep deterministic order
    }

#if UNITY_EDITOR
    private void OnValidate() => _byFlex = null;
#endif
}
