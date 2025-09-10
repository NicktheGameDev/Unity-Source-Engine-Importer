using UnityEngine;
using uSource.Formats.Source.MDL;
using uSource.Formats.Source.VBSP;
using System.IO;

namespace uSource.Runtime
{
    /// <summary>
    /// Hook into BSP entity spawn to replace placeholder with flex-ready model.
    /// Assumes entities spawn with MDL path in `model` key.
    /// </summary>
    public class BspModelBind : MonoBehaviour
    {
        // Called by BSP loader for every entity created.
        public static void OnEntitySpawned(GameObject entity, string mdlPath)
        {
            if (string.IsNullOrEmpty(mdlPath) || !File.Exists(mdlPath)) return;

            try
            {
                MDLFile mdl = MDLFile.Load(mdlPath, parseAnims: true, parseHitboxes: false);
                string vta = Path.ChangeExtension(mdlPath, ".vta");
                Stream vtaS = File.Exists(vta) ? File.OpenRead(vta) : null;

                var modelGO = ValveModelImporter.Import(mdl, null, vtaS, vtaS == null ? vta : null);
                modelGO.transform.parent = entity.transform;
                modelGO.transform.localPosition = Vector3.zero;
                modelGO.transform.localRotation = Quaternion.identity;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BspModelBind failed: {ex.Message}");
            }
        }
    }
}
