using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ValveImporter.Editor.Importers
{
    /// <summary>Universal VTA importer – wypełnia brakujące indeksy zerami (Crowbar) i przycina nadmiar (StudioCompiler).</summary>
    public static class VTAImporter
    {
        public static bool InjectBlendShapes(GameObject go, string vtaPath)
        {
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) { Debug.LogWarning("VTAImporter: no SMR"); return false; }

            if (!File.Exists(vtaPath)) { Debug.LogWarning($"VTAImporter: {vtaPath} missing"); return false; }

            var mesh = Object.Instantiate(smr.sharedMesh);
            smr.sharedMesh = mesh;
            int vertCount = mesh.vertexCount;

            var sparse = ValveImporter.Editor.Parsers.VTAParser.ParseSparse(File.ReadAllLines(vtaPath));
            if (sparse.Count == 0) { Debug.LogWarning("VTAImporter: no frames in VTA"); return false; }

            Dictionary<int, Vector3> basis = null;
            if (sparse.TryGetValue("basis", out basis) == false)
                basis = sparse.ContainsKey("reference") ? sparse["reference"] : null;

            // If no explicit basis – treat first frame as basis
            if (basis == null && sparse.Count > 0) basis = sparse.Values.GetEnumerator().Current;

            foreach (var kv in sparse)
            {
                if (kv.Key == "basis" || kv.Value == basis) continue;

                var deltas = new Vector3[vertCount];
                // zero-fill by default
                for (int i = 0; i < vertCount; i++) deltas[i] = Vector3.zero;

                // compute delta = frame - basis (if we have basis)
                foreach (var pair in kv.Value)
                {
                    int idx = pair.Key;
                    if (idx < 0 || idx >= vertCount) continue;
                    Vector3 val = pair.Value;
                    if (basis != null && basis.TryGetValue(idx, out Vector3 b))
                        deltas[idx] = val - b;
                    else
                        deltas[idx] = val; // already delta
                }

                mesh.AddBlendShapeFrame(kv.Key, 100f, deltas, null, null);
            }

            Debug.Log($"VTAImporter: injected {mesh.blendShapeCount} shapes from {Path.GetFileName(vtaPath)}");
            return mesh.blendShapeCount > 0;
        }
    }
}
