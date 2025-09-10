
using System.Collections;
using UnityEngine;
using uSource.Formats.Source.MDL;
using uSource.Tools;

public class ModelLoadBenchmark : MonoBehaviour
{
    [Tooltip("Absolute path to .mdl file")]
    public string MdlPath;
    public bool ParseAnimations = true;
    public bool ParseHitboxes = true;

    private IEnumerator Start()
    {
        if (string.IsNullOrEmpty(MdlPath))
        {
            Debug.LogError("MdlPath not set.");
            yield break;
        }

        yield return null;

        Debug.Log("[Benchmark] Starting MDL load");

        using (var s = ProfilerHelper.Measure("Total MDL Load"))
        {
            var t0 = Time.realtimeSinceStartup;

            byte[] data = System.IO.File.ReadAllBytes(MdlPath);
            using var ms = new System.IO.MemoryStream(data);
            var mdl = new MDLFile(ms, ParseAnimations, ParseHitboxes);

            Debug.Log("[Benchmark] Parsed, building model");

            using (var s2 = ProfilerHelper.Measure("BuildModel"))
            {
                var root = mdl.BuildModel();
                if (root != null)
                    root.gameObject.SetActive(true);
            }

            var dt = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[Benchmark] Total load+build time: {dt:F2} ms");
        }

        Debug.Log("[Benchmark] Done.");
    }
}
