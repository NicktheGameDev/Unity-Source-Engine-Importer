using UnityEngine;

namespace uSource.Runtime
{
    /// <summary>
    /// Simple critically damped spring jiggle-bone solver.
    /// </summary>
    public class JiggleBone : MonoBehaviour
    {
        public Transform BoneTransform { get; private set; }
        public float Mass { get; private set; } = 1f;
        public float Stiffness { get; private set; } = 350f;
        public float Damping { get; private set; } = 75f;
        public float Length { get; private set; } = 0.1f;

        public Vector3 CurrentOffset { get; private set; }

        private Vector3 _velocity;

        public void Init(Transform bone, float mass, float stiff, float damp, float length)
        {
            BoneTransform = bone;
            Mass = Mathf.Max(0.001f, mass);
            Stiffness = stiff;
            Damping = damp;
            Length = length;
        }

        public void UpdateJiggle(float dt, Vector3 parentPos)
        {
            if (BoneTransform == null) return;
            Vector3 currentPos = BoneTransform.position;
            Vector3 targetPos = parentPos + BoneTransform.parent.rotation * (Vector3.forward * Length);
            Vector3 force = (targetPos - currentPos) * Stiffness - _velocity * Damping;
            Vector3 accel = force / Mass;
            _velocity += accel * dt;
            Vector3 newPos = currentPos + _velocity * dt;
            CurrentOffset = newPos - BoneTransform.position;
            BoneTransform.position = newPos;
        }
    }
}
