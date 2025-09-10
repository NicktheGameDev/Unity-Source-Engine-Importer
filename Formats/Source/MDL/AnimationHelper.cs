using UnityEngine;
using UnityEditor;

public static class AnimationHelper
{
    public static AnimationCurve GetBlendCurve(float blendTime)
    {
        AnimationCurve blendCurve = new AnimationCurve();
        blendCurve.AddKey(0f, 0f);
        blendCurve.AddKey(blendTime, 1f);
        return blendCurve;
    }

    public static AnimationCurve GetCurve(AnimationClip clip, string boneName, string propertyName)
    {
#if UNITY_EDITOR
        var bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in bindings)
        {
            if (binding.path == boneName && binding.propertyName == propertyName)
            {
                return AnimationUtility.GetEditorCurve(clip, binding);
            }
        }
#endif
        return null;
    }

    public static AnimationCurve BlendKeyframes(AnimationCurve curveA, AnimationCurve curveB, AnimationCurve blendCurve)
    {
        AnimationCurve blendedCurve = new AnimationCurve();

        int keyCount = Mathf.Min(curveA.keys.Length, curveB.keys.Length);
        for (int i = 0; i < keyCount; i++)
        {
            Keyframe keyA = curveA.keys[i];
            Keyframe keyB = curveB.keys[i];
            float time = Mathf.Lerp(keyA.time, keyB.time, blendCurve.Evaluate(keyA.time));
            float value = Mathf.Lerp(keyA.value, keyB.value, blendCurve.Evaluate(keyA.time));
            blendedCurve.AddKey(new Keyframe(time, value));
        }
        return blendedCurve;
    }
}
