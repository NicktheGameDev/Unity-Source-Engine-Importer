
using System;
using UnityEngine;

namespace uSource.Tools
{
    public static class ProfilerHelper
    {
        // Simple timing wrapper. Usage:
        // using(var t = ProfilerHelper.Sample("MySection")) { ... }
        public struct Sample : IDisposable
        {
            private readonly string _name;
            private readonly float _start;
            public Sample(string name)
            {
                _name = name;
                _start = Time.realtimeSinceStartup;
                Debug.Log($"[Profiler] BEGIN {_name}");
            }

            public void Dispose()
            {
                float end = Time.realtimeSinceStartup;
                Debug.Log($"[Profiler] END {_name}: {(end - _start) * 1000f:F2} ms");
            }
        }

        public static Sample Measure(string name) => new Sample(name);
    }
}
