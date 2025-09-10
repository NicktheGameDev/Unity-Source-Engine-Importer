
using UnityEngine;

namespace uSource.Runtime
{
    /// <summary>
    /// Holds MDL animation events extracted from the model.
    /// </summary>
    public class MDLEventComponent : MonoBehaviour
    {
        [System.Serializable]
        public class MDLEvent
        {
            public string name;
            public int type;
            public float cycle;
        }

        public MDLEvent[] events;
    }
}
