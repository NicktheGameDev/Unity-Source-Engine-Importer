using UnityEngine;

[DisallowMultipleComponent]
public class JiggleBoneBehaviour : MonoBehaviour
{
    [Tooltip("Spring strength")]
    public float stiffness = 10f;
    [Tooltip("Damping factor")]
    public float damping   = 2f;
    [Tooltip("Max distance from rest (in world units)")]
    public float maxDisplacement = 0.2f;
    [Tooltip("Fade factor between Z-min and Z-max influence (0 = Z-min only, 1 = Z-max only)")]
    [Range(0f, 1f)]
    public float fadeToBone = 1f;
    internal Vector3 localRes;
    private Vector3 _restLocalPos;
    private Vector3 _velocity;

    void Awake()
    {
        _restLocalPos = transform.localPosition;
        _velocity     = Vector3.zero;
    }

    void LateUpdate()
    {
        Vector3 worldRest = transform.parent.TransformPoint(_restLocalPos);
        Vector3 worldPos  = transform.position;
        Vector3 toRest    = worldRest - worldPos;

        float fade = Mathf.Lerp(0f, 1f, fadeToBone);
        Vector3 scaledRest = worldRest + toRest * fade;

        Vector3 force = (scaledRest - worldPos) * stiffness - _velocity * damping;
        _velocity += force * Time.deltaTime;
        Vector3 nextPos = worldPos + _velocity * Time.deltaTime;

        Vector3 offset = nextPos - scaledRest;
        if (offset.magnitude > maxDisplacement)
            nextPos = scaledRest + offset.normalized * maxDisplacement;

        transform.position = nextPos;
        transform.localPosition = transform.parent.InverseTransformPoint(nextPos);
    }
}
