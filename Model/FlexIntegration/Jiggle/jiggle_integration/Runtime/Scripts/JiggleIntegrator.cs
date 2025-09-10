using System.Linq;
using UnityEngine;
using uSource;
using uSource.Formats.Source.MDL;

namespace Assets_.Assets.USource.Model.FlexIntegration.Jiggle.jiggle_integration.Runtime.Scripts
{
    public static class JiggleIntegrator
    {
        /// <summary>
        /// Attaches JiggleBoneBehaviour to each bone marked as jiggle in the MDL,
        /// mapping Valve's parameters into spring/damper physics.
        /// </summary>
        public static void Setup(GameObject root, MDLFile mdl)
        {
            if (mdl.MDL_JiggleBones == null || mdl.MDL_JiggleBones.Length == 0)
                return;

            var boneMap = root.GetComponentsInChildren<Transform>()
                .ToDictionary(t => t.name, t => t);

            foreach (var jb in mdl.MDL_JiggleBones)
            {
                if (jb.baseMass < 0 || jb.baseMass >= mdl.MDL_BoneNames.Length) continue;
                string boneName = mdl.MDL_BoneNames[(int)jb.baseMass];

                if (!boneMap.TryGetValue(boneName, out var boneT))
                    continue;

                var beh = boneT.gameObject.GetComponent<JiggleBoneBehaviour>()
                          ?? boneT.gameObject.AddComponent<JiggleBoneBehaviour>();

                beh.stiffness       = jb.baseStiffness;
                beh.damping         = jb.baseDamping;
                beh.maxDisplacement = Mathf.Lerp(jb.baseMinLeft, jb.baseMaxLeft, beh.fadeToBone) * uLoader.UnitScale;
                beh.fadeToBone      = 1f; // default, can override per-instance
            }
        }
    }
}
