using UnityEngine;
namespace FlexTools.Editor.Utils
{
    public static class FlexUtils
    {
        public static bool HasFlexes(GameObject go)
            => go.GetComponentInChildren<SkinnedMeshRenderer>()?.sharedMesh?.blendShapeCount>0;
        public static string GetSeparator()=>"+";
    }
}
