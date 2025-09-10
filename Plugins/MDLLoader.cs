
using UnityEngine;

namespace uSource
{
    // Wrapper for existing uLoader to simplify MDL loading
    public static class MDLLoader
    {
        public static GameObject LoadModel(string mdlPath)
        {
            var model = uResourceManager.LoadModel(mdlPath, WithAnims: true, withHitboxes: true);
            return model.transform.gameObject;
        }
    }
}
