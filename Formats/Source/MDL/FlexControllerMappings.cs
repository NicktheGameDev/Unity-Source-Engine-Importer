using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SourceEngineIntegration
{
    /// <summary>
    /// Represents a flex controller mapping for Source Engine.
    /// </summary>
    [Serializable]
    public class FlexControllerMapping
    {
        public string flexName;
        public string[] associatedBlendShapes;
        public float[] blendShapeWeights;

        public FlexControllerMapping(string flexName, IEnumerable<string> blendShapes, IEnumerable<float> weights)
        {
            this.flexName = flexName;
            this.associatedBlendShapes = blendShapes.ToArray();
            this.blendShapeWeights = weights.ToArray();
        }

        public FlexControllerMapping(string flexName, IEnumerable<string> blendShapes)
        {
            this.flexName = flexName;
            this.associatedBlendShapes = blendShapes.ToArray();
            this.blendShapeWeights = Enumerable.Repeat(1.0f, blendShapes.Count()).ToArray();
        }
    }

    [AddComponentMenu("Source Engine/Flex Controller Mappings")]
    [DisallowMultipleComponent]
    public class FlexControllerMappings : MonoBehaviour
    {
        [SerializeField]
        private List<FlexControllerMapping> flexMappings = new List<FlexControllerMapping>();

        public Dictionary<string, List<(string, float)>> FlexToBlendShapeMap { get; private set; } = new Dictionary<string, List<(string, float)>>();

        private void Awake()
        {
            DeserializeMappings();
        }

        public void AddFlexMapping(string flexName, string blendShape, float weight = 1.0f)
        {
            if (!FlexToBlendShapeMap.ContainsKey(flexName))
            {
                FlexToBlendShapeMap[flexName] = new List<(string, float)>();
            }
            FlexToBlendShapeMap[flexName].Add((blendShape, weight));
        }

        public void RemoveFlexMapping(string flexName, string blendShape)
        {
            if (FlexToBlendShapeMap.ContainsKey(flexName))
            {
                FlexToBlendShapeMap[flexName].RemoveAll(mapping => mapping.Item1 == blendShape);
                if (FlexToBlendShapeMap[flexName].Count == 0)
                {
                    FlexToBlendShapeMap.Remove(flexName);
                }
            }
        }

        public void SerializeMappings()
        {
            flexMappings.Clear();
            foreach (var entry in FlexToBlendShapeMap)
            {
                flexMappings.Add(new FlexControllerMapping(entry.Key, entry.Value.Select(v => v.Item1), entry.Value.Select(v => v.Item2)));
            }
        }

        public void DeserializeMappings()
        {
            FlexToBlendShapeMap.Clear();
            foreach (var mapping in flexMappings)
            {
                if (!FlexToBlendShapeMap.ContainsKey(mapping.flexName))
                {
                    FlexToBlendShapeMap[mapping.flexName] = new List<(string, float)>();
                }
                for (int i = 0; i < mapping.associatedBlendShapes.Length; i++)
                {
                    string blendShape = mapping.associatedBlendShapes[i];
                    float weight = i < mapping.blendShapeWeights.Length ? mapping.blendShapeWeights[i] : 1.0f;
                    FlexToBlendShapeMap[mapping.flexName].Add((blendShape, weight));
                }
            }
        }

        public void UpdateFlexMapping(string flexName, string[] blendShapes, float[] weights)
        {
            if (FlexToBlendShapeMap.ContainsKey(flexName))
            {
                FlexToBlendShapeMap[flexName].Clear();
                for (int i = 0; i < blendShapes.Length; i++)
                {
                    float weight = i < weights.Length ? weights[i] : 1.0f;
                    FlexToBlendShapeMap[flexName].Add((blendShapes[i], weight));
                }
            }
            else
            {
                AddFlexMapping(flexName, blendShapes, weights);
            }
        }

        public List<(string, float)> GetFlexMappings(string flexName)
        {
            if (FlexToBlendShapeMap.TryGetValue(flexName, out var mappings))
            {
                return mappings;
            }
            return new List<(string, float)>();
        }

        public void AddFlexMapping(string flexName, string[] blendShapes, float[] weights)
        {
            if (!FlexToBlendShapeMap.ContainsKey(flexName))
            {
                FlexToBlendShapeMap[flexName] = new List<(string, float)>();
            }
            for (int i = 0; i < blendShapes.Length; i++)
            {
                float weight = i < weights.Length ? weights[i] : 1.0f;
                FlexToBlendShapeMap[flexName].Add((blendShapes[i], weight));
            }
        }
    }
}
