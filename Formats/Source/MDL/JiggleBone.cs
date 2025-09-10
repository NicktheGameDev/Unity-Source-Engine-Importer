using UnityEngine;

public class JiggleBone
{
    public int BoneIndex;
    public Transform BoneTransform;
    public Vector3 GoalPosition;
    public Vector3 CurrentPosition;
    public Vector3 Velocity;
    public Vector3 Acceleration;

    // Jiggle parameters
    public float Stiffness;
    public float Damping;
    public float Mass;
    public float MaxDeltaTime = 0.033f; // Max allowed delta time for stable physics

    // Constraints
    public float MinYaw, MaxYaw;
    public float MinPitch, MaxPitch;
    public float AngleLimit;
    public float Length;

    // Constructor
    public JiggleBone(Transform boneTransform, float stiffness, float damping, float mass, float length)
    {
        BoneTransform = boneTransform;
        Stiffness = stiffness;
        Damping = damping;
        Mass = mass;
        Length = length;
        GoalPosition = boneTransform.position;
        CurrentPosition = GoalPosition;
    }

    public void UpdateJiggleBone(float deltaTime, Vector3 basePosition, Vector3 tipPosition)
    {
        // Limit delta time for stability
        deltaTime = Mathf.Min(deltaTime, MaxDeltaTime);

        // Apply gravity
        Acceleration += Vector3.down * Mass;

        // Compute spring forces
        Vector3 displacement = GoalPosition - CurrentPosition;
        Vector3 springForce = Stiffness * displacement - Damping * Velocity;
        Acceleration += springForce / Mass;

        // Update velocity and position
        Velocity += Acceleration * deltaTime;
        CurrentPosition += Velocity * deltaTime;

        // Clear acceleration for the next frame
        Acceleration = Vector3.zero;

        // Apply constraints
        ApplyConstraints(basePosition);

        // Update the bone transform in Unity
        BoneTransform.position = CurrentPosition;
    }

    private void ApplyConstraints(Vector3 basePosition)
    {
        // Enforce constraints like yaw, pitch, length, etc.
        // Example: Constraint the length of the jiggle bone
        Vector3 direction = (CurrentPosition - basePosition).normalized;
        CurrentPosition = basePosition + direction * Length;
    }
}
