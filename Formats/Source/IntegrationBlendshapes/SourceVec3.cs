using UnityEngine;

namespace uSource.Formats.Source
{
    /// <summary>
    /// Simple 3-component vector for Source Engine data (e.g. flex deltas).
    /// </summary>
    public struct SourceVec3
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public SourceVec3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public Vector3 ToUnity() => new Vector3(X, Y, Z);
        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}