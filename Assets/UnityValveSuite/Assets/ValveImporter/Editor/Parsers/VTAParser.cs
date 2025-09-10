using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ValveImporter.Editor.Parsers
{
    /// <summary>Universal VTA parser (BST, Crowbar, StudioCompiler) – supports 'frame' i 'vertexanimation/time'. Returns sparse deltas.</summary>
    public static class VTAParser
    {
        private static readonly Regex frameRx  = new Regex(@"frame\s+\d+\s+""([^""]+)""", RegexOptions.IgnoreCase);
        private static readonly Regex timeRx   = new Regex(@"time\s+(\d+)(?:\s+""([^""]+)""|)", RegexOptions.IgnoreCase);

        public static Dictionary<string, Dictionary<int, Vector3>> ParseSparse(string[] lines)
        {
            var shapes = new Dictionary<string, Dictionary<int, Vector3>>(StringComparer.OrdinalIgnoreCase);

            bool inAnim = false;
            string curName = null;
            Dictionary<int, Vector3> cur = null;

            void Flush()
            {
                if (curName != null && cur != null && !shapes.ContainsKey(curName))
                    shapes[curName] = cur;
            }

            foreach (var raw in lines)
            {
                var l = raw.Trim();
                if (l.Length == 0 || l.StartsWith("#")) continue;

                if (!inAnim && l.StartsWith("vertexanimation", StringComparison.OrdinalIgnoreCase))
                { inAnim = true; continue; }

                if (l.Equals("end", StringComparison.OrdinalIgnoreCase))
                { Flush(); break; }

                if (inAnim && l.StartsWith("time", StringComparison.OrdinalIgnoreCase))
                {
                    Flush();
                    var m = timeRx.Match(l);
                    int frame = m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : -1;
                    curName = m.Groups[2].Success ? m.Groups[2].Value : (frame == 0 ? "basis" : $"frame_{frame}");
                    cur = new Dictionary<int, Vector3>();
                    continue;
                }

                if (l.StartsWith("frame", StringComparison.OrdinalIgnoreCase))
                {
                    // support old 'frame' syntax
                    Flush();
                    var m = frameRx.Match(l);
                    curName = m.Success ? m.Groups[1].Value : "frame?";
                    cur = new Dictionary<int, Vector3>();
                    continue;
                }

                if (char.IsDigit(l[0]) && cur != null)
                {
                    var sp = l.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (sp.Length < 4) continue;
                    int idx;
                    if (!int.TryParse(sp[0], out idx)) continue;
                    float x = float.Parse(sp[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(sp[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(sp[3], CultureInfo.InvariantCulture);
                    cur[idx] = new Vector3(x, y, z);
                }
            }

            return shapes;
        }
    }
}
