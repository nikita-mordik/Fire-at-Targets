using Kinemation.FPSFramework.Runtime.Core.Types;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Jobs
{
    public struct TwoBoneIkJobData
    {
        public float effectorWeight;
        public float hintWeight;

        public LocRot root;
        public LocRot mid;
        public LocRot tip;
        
        public LocRot effectorTarget;
        public LocRot hintTarget;
        public bool hasValidHint;
    }
    
    public struct TwoBoneIKJob : IJobParallelFor
    {
        public NativeArray<TwoBoneIkJobData> jobData;
        
        public void Execute(int index)
        {
            var data = jobData[index];
            ExecuteTwoBoneIk(ref data);
            jobData[index] = data;
        }

        public static void ExecuteTwoBoneIk(ref TwoBoneIkJobData data)
        {
            LocRot midLocal = data.mid.ToSpace(data.root);
            LocRot tipLocal = data.tip.ToSpace(data.mid);
            
            float sqrEpsilon = 1e-8f;

            Vector3 aPosition = data.root.position;
            Vector3 bPosition = data.mid.position;
            Vector3 cPosition = data.tip.position;
            
            Vector3 tPosition = Vector3.Lerp(cPosition, data.effectorTarget.position, data.effectorWeight);
            Quaternion tRotation = Quaternion.Lerp(data.tip.rotation, data.effectorTarget.rotation, data.effectorWeight);
            bool hasHint = data.hasValidHint && data.hintWeight > 0f;

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 at = tPosition - aPosition;

            float abLen = ab.magnitude;
            float bcLen = bc.magnitude;
            float acLen = ac.magnitude;
            float atLen = at.magnitude;

            float oldAbcAngle = CoreToolkitLib.TriangleAngle(acLen, abLen, bcLen);
            float newAbcAngle = CoreToolkitLib.TriangleAngle(atLen, abLen, bcLen);

            // Bend normal strategy is to take whatever has been provided in the animation
            // stream to minimize configuration changes, however if this is collinear
            // try computing a bend normal given the desired target position.
            // If this also fails, try resolving axis using hint if provided.
            Vector3 axis = Vector3.Cross(ab, bc);
            if (axis.sqrMagnitude < sqrEpsilon)
            {
                axis = hasHint ? Vector3.Cross(data.hintTarget.position - aPosition, bc) : Vector3.zero;

                if (axis.sqrMagnitude < sqrEpsilon)
                    axis = Vector3.Cross(at, bc);

                if (axis.sqrMagnitude < sqrEpsilon)
                    axis = Vector3.up;
            }

            axis = Vector3.Normalize(axis);

            float a = 0.5f * (oldAbcAngle - newAbcAngle);
            float sin = Mathf.Sin(a);
            float cos = Mathf.Cos(a);
            Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
            
            data.mid.rotation = deltaR * data.mid.rotation;
            midLocal = data.mid.ToSpace(data.root);
            
            data.tip = tipLocal.FromSpace(data.mid);

            cPosition = data.tip.position;
            ac = cPosition - aPosition;
            data.root.rotation = CoreToolkitLib.FromToRotation(ac, at) * data.root.rotation;

            data.mid = midLocal.FromSpace(data.root);
            data.tip = tipLocal.FromSpace(data.mid);

            if (hasHint)
            {
                float acSqrMag = ac.sqrMagnitude;
                if (acSqrMag > 0f)
                {
                    bPosition = data.mid.position;
                    cPosition = data.tip.position;
                    ab = bPosition - aPosition;
                    ac = cPosition - aPosition;

                    Vector3 acNorm = ac / Mathf.Sqrt(acSqrMag);
                    Vector3 ah = data.hintTarget.position - aPosition;
                    Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                    Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                    float maxReach = abLen + bcLen;
                    if (abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f)
                    {
                        Quaternion hintR = CoreToolkitLib.FromToRotation(abProj, ahProj);
                        hintR.x *= data.hintWeight;
                        hintR.y *= data.hintWeight;
                        hintR.z *= data.hintWeight;
                        hintR = CoreToolkitLib.NormalizeSafe(hintR);
                        data.root.rotation = hintR * data.root.rotation;
                        
                        data.mid = midLocal.FromSpace(data.root);
                        data.tip = tipLocal.FromSpace(data.mid);
                    }
                }
            }

            data.tip.rotation = tRotation;
        }
    }
}