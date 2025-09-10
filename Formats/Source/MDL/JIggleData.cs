using UnityEngine;

public class JiggleData
{
    public int Bone;

    public float LastUpdate;  // Time of the last update
    public Vector3 BasePos;   // Position of the base of the jiggle bone
    public Vector3 BaseLastPos;
    public Vector3 BaseVel;
    public Vector3 BaseAccel;

    public Vector3 TipPos;    // Position of the tip of the jiggle bone
    public Vector3 TipVel;
    public Vector3 TipAccel;
    public Vector3 LastLeft;  // Previous left vector

    public Vector3 LastBoingPos; // Position of the base of the jiggle bone in the last update for tracking velocity
    public Vector3 BoingDir;     // Current direction along which the boing effect is occurring
    public Vector3 BoingVelDir;  // Current estimation of jiggle bone unit velocity vector for boing effect
    public float BoingSpeed;     // Current estimation of jiggle bone speed for boing effect
    public float BoingTime;

    public JiggleData(int bone, float currentTime, Vector3 initBasePos, Vector3 initTipPos)
    {
        Init(bone, currentTime, initBasePos, initTipPos);
    }

    public void Init(int bone, float currentTime, Vector3 initBasePos, Vector3 initTipPos)
    {
        Bone = bone;
        LastUpdate = currentTime;

        BasePos = initBasePos;
        BaseLastPos = BasePos;
        BaseVel = Vector3.zero;
        BaseAccel = Vector3.zero;

        TipPos = initTipPos;
        TipVel = Vector3.zero;
        TipAccel = Vector3.zero;

        LastLeft = Vector3.zero;

        LastBoingPos = initBasePos;
        BoingDir = new Vector3(0.0f, 0.0f, 1.0f);
        BoingVelDir = Vector3.zero;
        BoingSpeed = 0.0f;
        BoingTime = 0.0f;
    }
}
