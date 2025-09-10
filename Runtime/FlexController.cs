using UnityEngine;
using System.Collections.Generic;

namespace uSource.Runtime
{
    /// <summary>
    /// Maps flex controller channels to blend-shape indices on a SkinnedMeshRenderer.
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class FlexController : MonoBehaviour
    {
        private SkinnedMeshRenderer _smr;
        private Dictionary<string, int> _map;

        public void Init(Dictionary<string, int> controllerToBlendshape)
        {
            _smr = GetComponent<SkinnedMeshRenderer>();
            _map = controllerToBlendshape;
        }

        public void Set(string controller, float value)
        {
            if (_map != null && _map.TryGetValue(controller, out int idx))
            {
                _smr.SetBlendShapeWeight(idx, value * 100f);
            }
        }
    }
}
