
using System.Collections.Generic;
using TF2Ls.FaceFlex;
using UnityEngine;

namespace uSource.FlexSystem
{
    public class FlexManager : MonoBehaviour
    {
        public List<FlexController> FlexControllers { get; private set; }
        public Dictionary<string, FlexPreset> FlexPresets { get; private set; }
        private SkinnedMeshRenderer meshRenderer;

        void Awake()
        {
            FlexControllers = new List<FlexController>();
            FlexPresets = new Dictionary<string, FlexPreset>();
            meshRenderer = GetComponent<SkinnedMeshRenderer>();

            InitializeFlexControllers();
        }

        void InitializeFlexControllers()
        {
            // Example FlexControllers initialization, can be extended dynamically.
            FlexControllers.Add(new FlexController());
            FlexControllers.Add(new FlexController());
        }
        public void ApplyFlexPreset(string presetName)
        {
            if (FlexPresets.ContainsKey(presetName))
            {
                var preset = FlexPresets[presetName];
                for (int i = 0; i < preset.flexControllerNames.Length; i++)
                {
                    var controller = FlexControllers.Find(fc => fc.Name == preset.flexControllerNames[i]);
                    if (controller != null)
                    {
                        controller.Current = preset.values[i];
                        UpdateBlendShape(controller.Name, controller.Current);
                    }
                }
            }
        }

        void UpdateBlendShape(string blendShapeName, float value)
        {
            if (meshRenderer && meshRenderer.sharedMesh != null)
            {
                int blendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex >= 0)
                {
                    meshRenderer.SetBlendShapeWeight(blendShapeIndex, value * 100f);
                }
            }
        }
    }
}
