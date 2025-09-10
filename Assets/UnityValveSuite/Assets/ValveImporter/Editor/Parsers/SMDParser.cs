using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ValveImporter.Editor.Parsers
{
    internal sealed class SMDBone
    {
        public int    Id;
        public string Name;
        public int    Parent; // -1 == root
        public Vector3 Position; // bind pose
        public Vector3 Rotation; // Euler (degrees)
    }

    internal struct SMDVertex
    {
        public Vector3 Pos;
        public Vector3 Normal;
        public Vector2 UV;

        public int[]   BoneIds; // up to 4
        public float[] Weights; // up to 4, same length
    }

    internal struct SMDTri
    {
        public string   Material;
        public SMDVertex V0;
        public SMDVertex V1;
        public SMDVertex V2;
    }

    internal sealed class SMDModel
    {
        public readonly List<SMDBone> Bones = new List<SMDBone>();
        public readonly List<SMDTri>  Tris  = new List<SMDTri>();
    }

    /// <summary>
    /// Basic ASCII .SMD parser:
    /// - nodes → bone names & hierarchy
    /// - skeleton time 0 → bind pose
    /// - triangles → mesh + bone assignments
    /// Supports multi-weight vertices (as produced by Blender Source Tools).
    /// </summary>
    internal static class SMDParser
    {
        public static SMDModel Parse(string[] lines)
        {
            var model = new SMDModel();

            int i = 0;
            // -------------------- NODES --------------------
            while (i < lines.Length && !IsToken(lines[i], "nodes")) i++;
            if (i >= lines.Length) throw new Exception("SMDParser: no 'nodes' section.");

            i++; // first line after "nodes"
            while (i < lines.Length && !IsToken(lines[i], "end"))
            {
                var l = lines[i].Trim();
                if (l.Length == 0) { i++; continue; }

                // id "name" parent
                int quote1 = l.IndexOf('"');
                int quote2 = l.LastIndexOf('"');
                if (quote1 == -1 || quote2 == quote1) throw new Exception($"SMDParser: bad node line: {l}");

                int id   = int.Parse(l[..quote1], CultureInfo.InvariantCulture);
                string name = l[(quote1 + 1)..quote2];
                int parent = int.Parse(l[(quote2 + 1)..].Trim(), CultureInfo.InvariantCulture);

                model.Bones.Add(new SMDBone { Id = id, Name = name, Parent = parent });

                i++;
            }

            // -------------------- SKELETON (time 0) --------------------
            while (i < lines.Length && !IsToken(lines[i], "skeleton")) i++;
            if (i >= lines.Length) throw new Exception("SMDParser: no 'skeleton' section.");

            i++; // now inside skeleton
            // find "time 0"
            while (i < lines.Length && !lines[i].TrimStart().StartsWith("time 0", StringComparison.OrdinalIgnoreCase)) i++;
            if (i >= lines.Length) throw new Exception("SMDParser: no 'time 0' in skeleton.");

            i++; // first bone pose line
            while (i < lines.Length && !IsToken(lines[i], "end"))
            {
                var l = lines[i].Trim();
                if (l.Length == 0) { i++; continue; }

                var sp = Split(l);
                if (sp.Length < 7) throw new Exception($"SMDParser: bad skeleton line: {l}");

                int id = int.Parse(sp[0], CultureInfo.InvariantCulture);
                float.TryParse(sp[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
                float.TryParse(sp[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
                float.TryParse(sp[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z);
                float.TryParse(sp[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float rx);
                float.TryParse(sp[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float ry);
                float.TryParse(sp[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float rz);

                var bone = model.Bones.Find(b => b.Id == id);
                if (bone != null)
                {
                    // Source: X forward, Y left, Z up (right‑handed)
                    // Unity:  X right, Y up, Z forward (left‑handed)
                    bone.Position = new Vector3(x, z, -y);
                    bone.Rotation = new Vector3(rx, rz, -ry) * Mathf.Rad2Deg;
                }

                i++;
            }

            // -------------------- TRIANGLES --------------------
            while (i < lines.Length && !IsToken(lines[i], "triangles")) i++;
            if (i >= lines.Length) throw new Exception("SMDParser: no 'triangles' section.");
            i++; // first line after 'triangles'

            while (i < lines.Length && !IsToken(lines[i], "end"))
            {
                var mat = lines[i].Trim(); // material
                i++;

                var tri = new SMDTri { Material = mat };

                tri.V0 = ReadVertex(lines[i++]);
                tri.V1 = ReadVertex(lines[i++]);
                tri.V2 = ReadVertex(lines[i++]);

                model.Tris.Add(tri);
            }

            return model;
        }

        // ---------- helpers ----------
        private static bool IsToken(string line, string token) =>
            line.Trim().Equals(token, StringComparison.OrdinalIgnoreCase);

        private static string[] Split(string line) =>
            line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

        private static SMDVertex ReadVertex(string line)
        {
            var sp = Split(line);
            if (sp.Length < 9) throw new Exception($"SMDParser: bad vertex line: {line}");

            int baseIdx = 0;
            int boneRef = int.Parse(sp[baseIdx++], CultureInfo.InvariantCulture);

            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float z);

            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float nx);
            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float ny);
            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float nz);

            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float u);
            float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out float v);

            var vert = new SMDVertex
            {
                Pos    = new Vector3(x, z, -y),
                Normal = new Vector3(nx, nz, -ny).normalized,
                UV     = new Vector2(u, 1f - v)
            };

            var boneIds = new List<int>();
            var weights = new List<float>();

            // primary
            boneIds.Add(boneRef);
            weights.Add(1f);

            if (baseIdx < sp.Length)
            {
                int links = int.Parse(sp[baseIdx++], CultureInfo.InvariantCulture);
                boneIds.Clear();
                weights.Clear();

                for (int l = 0; l < links; l++)
                {
                    if (baseIdx + 1 >= sp.Length) break;
                    int b  = int.Parse(sp[baseIdx++], CultureInfo.InvariantCulture);
                    float w;
                    float.TryParse(sp[baseIdx++], NumberStyles.Float, CultureInfo.InvariantCulture, out w);

                    boneIds.Add(b);
                    weights.Add(w);
                }
            }

            // clamp to 4
            while (boneIds.Count > 4) { boneIds.RemoveAt(boneIds.Count - 1); weights.RemoveAt(weights.Count - 1); }

            // normalise weights
            float sum = 0f;
            for (int k = 0; k < weights.Count; k++) sum += weights[k];
            if (sum <= 0f) { weights[0] = 1f; sum = 1f; }
            for (int k = 0; k < weights.Count; k++) weights[k] /= sum;

            vert.BoneIds = boneIds.ToArray();
            vert.Weights = weights.ToArray();

            return vert;
        }
    }
}
