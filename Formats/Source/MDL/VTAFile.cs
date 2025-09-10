// Assets/uSource/Formats/Source/VTA/VTAFile.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace uSource.Formats.Source.VTA
{
    /// <summary>
    /// ASCII VTA parser (HL2, Black Mesa, Source 2013).
    /// Czyta pozycje, normale i zachowuje indeks klatki.
    /// </summary>
    public class VTAFile : MonoBehaviour
    {
        public struct Frame
        {
            public int Index;           // kolejność w pliku (0,1,2…)
            public int Time;            // wartość z „time n”
            public Vector3[] Positions; // PX PY PZ
            public Vector3[] Normals;   // NX NY NZ
        }

        public Frame[] Frames { get; private set; }
        public int VertexCount => _vertexCount;

        private int _vertexCount;
        private static readonly char[] _splitter = { ' ', '\t' };

        #region ── Public API ──────────────────────────────────────────────

        public static VTAFile Load(string path)
        {
            using var sr = new StreamReader(path);
            return Load(sr);
        }

        public static VTAFile Load(StreamReader sr)
        {
            var vta  = new VTAFile();
            var list = ParseFrames(sr);

            if (list.Count < 2)
                throw new Exception("VTA musi zawierać przynajmniej klatkę bazową i jedną flex-klatkę.");

            vta._vertexCount = list[0].Positions.Length;
            foreach (var f in list)
                if (f.Positions.Length != vta._vertexCount)
                    throw new Exception("Wszystkie klatki VTA muszą mieć tę samą liczbę wierzchołków.");

            vta.Frames = list.ToArray();
            return vta;
        }

        #endregion

        #region ── Parsing internals ───────────────────────────────────────

        private static List<Frame> ParseFrames(StreamReader sr)
        {
            var frames        = new List<Frame>();
            var positions     = new List<Vector3>();
            var normals       = new List<Vector3>();
            int  currentTime  = -1;
            bool inVertexAnim = false;
            int  frameIndex   = 0;

            string line;
            var inv = CultureInfo.InvariantCulture;

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("vertexanimation", StringComparison.OrdinalIgnoreCase))
                {
                    inVertexAnim = true;
                    continue;
                }
                if (!inVertexAnim) continue;

                if (line.StartsWith("time ", StringComparison.OrdinalIgnoreCase))
                {
                    FlushFrame();
                    currentTime = int.Parse(line.Split()[1], inv);
                    continue;
                }

                if (line.StartsWith("end", StringComparison.OrdinalIgnoreCase))
                {
                    FlushFrame();
                    break;
                }

                if (char.IsDigit(line[0]))
                {
                    var p = line.Split(_splitter, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length < 7) continue;

                    positions.Add(new Vector3(
                        float.Parse(p[1], inv),
                        float.Parse(p[2], inv),
                        float.Parse(p[3], inv)));

                    normals.Add(new Vector3(
                        float.Parse(p[4], inv),
                        float.Parse(p[5], inv),
                        float.Parse(p[6], inv)));
                }
            }
            return frames;

            // --- lok. flush --------------------------------------------------
            void FlushFrame()
            {
                if (positions.Count == 0) return;

                frames.Add(new Frame
                {
                    Index     = frameIndex++,
                    Time      = currentTime,
                    Positions = positions.ToArray(),
                    Normals   = normals.ToArray()
                });

                positions.Clear();
                normals.Clear();
            }
        }

        #endregion
    }
}
