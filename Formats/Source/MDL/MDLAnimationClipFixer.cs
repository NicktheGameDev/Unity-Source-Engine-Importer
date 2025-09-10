#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MDLAnimationClipFixer
{
    [MenuItem("Tools/MDL/Fix Animation Clips")]
    public static void FixAll()
    {
        var guids = AssetDatabase.FindAssets("t:AnimationClip");
        int count = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            Undo.RecordObject(clip, "Fix Animation Clip");
            FixClip(clip);
            EditorUtility.SetDirty(clip);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"MDLAnimationClipFixer: processed {count} clips");
    }

    private static void FixClip(AnimationClip clip)
    {
        // 1️⃣  Rotacje – zawsze quaternion, bez skoków
        clip.EnsureQuaternionContinuity();

        // 2️⃣  Pozycje/rotacje – zacięcia → płynne tangenty
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.length == 0) continue;

            bool changed = false;

            for (int i = 0; i < curve.length; i++)
            {
                if (AnimationUtility.GetKeyLeftTangentMode(curve, i) is AnimationUtility.TangentMode.Constant or AnimationUtility.TangentMode.Linear)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                    changed = true;
                }
                if (AnimationUtility.GetKeyRightTangentMode(curve, i) is AnimationUtility.TangentMode.Constant or AnimationUtility.TangentMode.Linear)
                {
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                    changed = true;
                }
            }

            if (changed)
                AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
    }
}
#endif
