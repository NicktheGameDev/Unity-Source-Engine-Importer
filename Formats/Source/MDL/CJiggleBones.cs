using System.Collections.Generic;
using UnityEngine;


    public class CJiggleBones
    {
        private List<JiggleData> JiggleBoneState = new List<JiggleData>();

        public JiggleData GetJiggleData(int bone, float currentTime, Vector3 initBasePos, Vector3 initTipPos)
        {
            foreach (var jiggle in JiggleBoneState)
            {
                if (jiggle.Bone == bone)
                {
                    return jiggle;
                }
            }

            // If not found, create a new JiggleData
            var data = new JiggleData(bone, currentTime, initBasePos, initTipPos);
            JiggleBoneState.Add(data);
            return data;
        }

        public void BuildJiggleTransformations(int boneIndex, float currentTime, JiggleBoneParams jiggleParams, Matrix4x4 goalMX, ref Matrix4x4 boneMX)
        {
            // Get base position and orientation from goal matrix
            Vector3 goalBasePosition = goalMX.GetColumn(3); // Translation part
            Vector3 goalForward = goalMX.GetColumn(2);      // Forward direction
            Vector3 goalUp = goalMX.GetColumn(1);           // Up direction
            Vector3 goalLeft = goalMX.GetColumn(0);         // Left direction

            // Compute goal tip position
            Vector3 goalTip = goalBasePosition + jiggleParams.Length * goalForward;

            // Retrieve or initialize jiggle data
            JiggleData data = GetJiggleData(boneIndex, currentTime, goalBasePosition, goalTip);
            if (data == null) return;

            // Handle frames that were skipped
            float deltaTime = currentTime - data.LastUpdate;
            if (deltaTime > jiggleParams.TimeTolerance)
            {
                data.Init(boneIndex, currentTime, goalBasePosition, goalTip);
            }

            // Update jiggle bone transformation
            PerformJiggleSimulation(ref data, deltaTime, jiggleParams, goalMX, ref boneMX);
        }

        private void PerformJiggleSimulation(ref JiggleData data, float deltaTime, JiggleBoneParams jiggleParams, Matrix4x4 goalMX, ref Matrix4x4 boneMX)
        {
            // Apply gravity in global space
            data.TipAccel += Physics.gravity * jiggleParams.TipMass;

            // Calculate the spring force and damping
            Vector3 error = data.TipPos - data.BasePos;
            Vector3 force = (-jiggleParams.Stiffness * error) - (jiggleParams.Damping * data.TipVel);
            data.TipAccel += force / jiggleParams.Mass;

            // Euler integration for velocity and position
            data.TipVel += data.TipAccel * deltaTime;
            data.TipPos += data.TipVel * deltaTime;

            // Clear acceleration for next frame
            data.TipAccel = Vector3.zero;

            // Apply constraints, if any
            ApplyConstraints(ref data, goalMX, jiggleParams);

            // Update the final bone matrix
            boneMX.SetColumn(3, new Vector4(data.TipPos.x, data.TipPos.y, data.TipPos.z, 1));
        }

        private void ApplyConstraints(ref JiggleData data, Matrix4x4 goalMX, JiggleBoneParams jiggleParams)
        {
            // Example constraint enforcement (length constraint)
            Vector3 baseToTip = data.TipPos - data.BasePos;
            float currentLength = baseToTip.magnitude;
            if (currentLength > jiggleParams.Length)
            {
                data.TipPos = data.BasePos + baseToTip.normalized * jiggleParams.Length;
            }
        }
    }

public class JiggleBoneParams
{
    internal int Stiffness;
    internal float Mass;
    internal float Length;
    internal int TipMass;
    internal float  Damping;
    internal float TimeTolerance;
}