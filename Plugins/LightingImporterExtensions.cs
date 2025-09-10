
using UnityEngine;

namespace uSource
{
    // Source-like lighting importer
    public static class LightingImporterExtensions
    {
        public static void ImportSourceLight(GameObject go, Vector3 pos, Color color, float intensity, float range)
        {
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            // SM6 attenuation model: inverse-square
            light.shadows = LightShadows.Soft;
        }
    }
}
