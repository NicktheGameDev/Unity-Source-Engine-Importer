// Matrix4x4Utils.cs
// Place alongside MdlImporterFull.cs

using UnityEngine;

public static class Matrix4x4Utils
{
    public static Matrix4x4[] CreateBindPoses(Transform[] bones)
    {
        var poses = new Matrix4x4[bones.Length];
        for (int i = 0; i < bones.Length; i++)
            poses[i] = bones[i].worldToLocalMatrix;
        return poses;
    }
}
