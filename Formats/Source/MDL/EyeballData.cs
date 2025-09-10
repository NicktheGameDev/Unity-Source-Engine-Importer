using UnityEngine;

public class EyeballData
{
    public string Name;
    public int BoneIndex;
    public Vector3 Position;
    public float Radius;
    public Material Material;

    public void Initialize(Transform parentBone, Shader eyeShader, Vector3 targetDirection)
    {
        GameObject eyeballObject = new GameObject(Name);
        Transform eyeballTransform = eyeballObject.transform;

        eyeballTransform.SetParent(parentBone);
        eyeballTransform.localPosition = Position;

        Material = new Material(eyeShader)
        {
            name = $"{Name}_Material"
        };
        Renderer renderer = eyeballObject.AddComponent<MeshRenderer>();
        renderer.material = Material;

        // Apply rotation towards the target direction (for example, a camera or point of interest)
        Vector3 direction = targetDirection - Position;
        if (direction.sqrMagnitude > 0)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            eyeballTransform.localRotation = lookRotation;
        }
    }
}
