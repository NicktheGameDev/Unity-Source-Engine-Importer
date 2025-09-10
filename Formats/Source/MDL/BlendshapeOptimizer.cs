
using System.Collections.Generic;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    public static class BlendshapeOptimizer
    {
        // cached adjacency and max edge per mesh instance
        private static readonly Dictionary<int, List<int>[]> adjacencyCache = new();
        private static readonly Dictionary<int, float[]> maxEdgeCache = new();

        public static List<int>[] GetAdjacency(Mesh m)
        {
            int id = m.GetInstanceID();
            if (adjacencyCache.TryGetValue(id, out var adj))
                return adj;

            int vCount = m.vertexCount;
            var result = new List<int>[vCount];
            for (int i = 0; i < vCount; ++i) result[i] = new List<int>(6);
            var tris = m.triangles;
            for (int t = 0; t < tris.Length; t += 3)
            {
                int a = tris[t], b = tris[t + 1], c = tris[t + 2];
                AddEdge(result, a, b);
                AddEdge(result, a, c);
                AddEdge(result, b, c);
            }

            adjacencyCache[id] = result;
            return result;
        }

        private static void AddEdge(List<int>[] adj, int x, int y)
        {
            if (!adj[x].Contains(y)) adj[x].Add(y);
            if (!adj[y].Contains(x)) adj[y].Add(x);
        }

        public static float[] GetMaxEdgeLengths(Mesh m, List<int>[] adj)
        {
            int id = m.GetInstanceID();
            if (maxEdgeCache.TryGetValue(id, out var max)) return max;
            var verts = m.vertices;
            int vCount = verts.Length;
            var result = new float[vCount];
            for (int v = 0; v < vCount; ++v)
            {
                float best = 0f;
                foreach (int n in adj[v])
                {
                    float d = (verts[v] - verts[n]).magnitude;
                    if (d > best) best = d;
                }
                result[v] = best;
            }

            maxEdgeCache[id] = result;
            return result;
        }

        public static void Sanitize(Vector3[] delta, Mesh m)
        {
            var adj = GetAdjacency(m);
            var maxEdge = GetMaxEdgeLengths(m, adj);
            int vCount = delta.Length;

            // quantize and average duplicates
            var buckets = new Dictionary<(int, int, int), List<int>>();
            var verts = m.vertices;
            for (int i = 0; i < vCount; ++i)
            {
                var key = Quantize(verts[i]);
                if (!buckets.TryGetValue(key, out var list))
                {
                    list = new List<int>(2);
                    buckets[key] = list;
                }
                list.Add(i);
            }
            foreach (var kv in buckets.Values)
            {
                if (kv.Count < 2) continue;
                Vector3 avg = Vector3.zero;
                foreach (int idx in kv) avg += delta[idx];
                avg /= kv.Count;
                foreach (int idx in kv) delta[idx] = avg;
            }

            for (int v = 0; v < vCount; ++v)
            {
                float max = maxEdge[v] * 0.95f;
                float mag = delta[v].magnitude;
                if (mag > max) delta[v] *= max / mag;
            }
        }

        private static (int, int, int) Quantize(Vector3 p)
            => ((int)(p.x * 10000f), (int)(p.y * 10000f), (int)(p.z * 10000f));
    }
}
