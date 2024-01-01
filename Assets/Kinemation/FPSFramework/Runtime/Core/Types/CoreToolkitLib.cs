// Designed by KINEMATION, 2023

using System;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [Serializable]
    public struct BoneAngle
    {
        public int boneIndex;
        public Vector2 angle;

        public BoneAngle(int boneIndex, Vector2 angle)
        {
            this.boneIndex = boneIndex;
            this.angle = angle;
        }
    }

    [Serializable]
    public struct LocRot
    {
        public static LocRot identity = new(Vector3.zero, Quaternion.identity);
        
        public Vector3 position;
        public Quaternion rotation;
        
        public LocRot(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
        
        public LocRot(Transform t, bool worldSpace = true)
        {
            position = t == null ? Vector3.zero : worldSpace ? t.position : t.localPosition;
            rotation = t == null ? Quaternion.identity : worldSpace ? t.rotation : t.localRotation;
        }

        public static LocRot Lerp(LocRot a, LocRot b, float alpha)
        {
            Vector3 position = Vector3.Lerp(a.position, b.position, alpha);
            Quaternion rotation = Quaternion.Slerp(a.rotation, b.rotation, alpha);
            return new LocRot(position, rotation);
        }

        public LocRot FromSpace(Transform targetSpace)
        {
            if (targetSpace == null)
            {
                return this;
            }

            return new LocRot(targetSpace.TransformPoint(position), targetSpace.rotation * rotation);
        }
        
        public LocRot FromSpace(LocRot targetSpace)
        {
            Vector3 newPosition = targetSpace.rotation * position + targetSpace.position;
            Quaternion newRotation = targetSpace.rotation * rotation;
            
            return new LocRot(newPosition, newRotation);
        }
        
        public LocRot ToSpace(Transform targetSpace)
        {
            if (targetSpace == null)
            {
                return this;
            }

            return new LocRot(targetSpace.InverseTransformPoint(position), 
                Quaternion.Inverse(targetSpace.rotation) * rotation);
        }
        
        public LocRot ToSpace(LocRot targetSpace)
        {
            Vector3 newPosition = Quaternion.Inverse(targetSpace.rotation) * (position - targetSpace.position);
            Quaternion newRotation = Quaternion.Inverse(targetSpace.rotation) * rotation;
            
            return new LocRot(newPosition, newRotation);
        }

        public bool Equals(LocRot b)
        {
            return position.Equals(b.position) && rotation.Equals(b.rotation);
        }
    }

    public struct SpringState
    {
        public float error;
        public float velocity;

        public void Reset()
        {
            error = velocity = 0f;
        }
    }

    public struct VectorSpringState
    {
        public SpringState x;
        public SpringState y;
        public SpringState z;
        
        public void Reset()
        {
            x.Reset();
            y.Reset();
            z.Reset();
        }
    }

    [Serializable]
    public struct SpringData
    {
        public float stiffness;
        public float criticalDamping;
        public float speed;
        public float maxValue;

        public SpringData(float stiffness, float damping, float speed, float mass)
        {
            this.stiffness = stiffness;
            criticalDamping = damping;
            this.speed = speed;
            
            maxValue = 0f;
        }
        
        public SpringData(float stiffness, float damping, float speed)
        {
            this.stiffness = stiffness;
            criticalDamping = damping;
            this.speed = speed;
            maxValue = 0f;
        }
    }

    [Serializable]
    public struct VectorSpringData
    {
        public SpringData x;
        public SpringData y;
        public SpringData z;
        public Vector3 scale;

        public VectorSpringData(float stiffness, float damping, float speed)
        {
            x = y = z = new SpringData(stiffness, damping, speed);
            scale = Vector3.one;
        }
    }

    [Serializable]
    public struct LocRotSpringData
    {
        public VectorSpringData loc;
        public VectorSpringData rot;
        
        public LocRotSpringData(float stiffness, float damping, float speed)
        {
            loc = rot = new VectorSpringData(stiffness, damping, speed);
        }
    }
    
    // General input data used by Anim Layers
    public struct CharAnimData
    {
        // Input
        public Vector2 deltaAimInput;
        public Vector2 totalAimInput;
        public Vector2 moveInput;
        public float leanDirection;
        public LocRot recoilAnim;

        public void AddDeltaInput(Vector2 aimInput)
        {
            deltaAimInput = aimInput;
        }

        public void AddAimInput(Vector2 aimInput)
        {
            deltaAimInput = aimInput;
            totalAimInput += deltaAimInput;
            totalAimInput.x = Mathf.Clamp(totalAimInput.x, -90f, 90f);
            totalAimInput.y = Mathf.Clamp(totalAimInput.y, -90f, 90f);
        }
        
        public void SetAimInput(Vector2 aimInput)
        {
            deltaAimInput = aimInput - totalAimInput;
            totalAimInput.x = Mathf.Clamp(aimInput.x, -90f, 90f);
            totalAimInput.y = Mathf.Clamp(aimInput.y, -90f, 90f);
        }

        public void SetLeanInput(float direction)
        {
            leanDirection = Mathf.Clamp(direction, -1f, 1f);
        }
        
        public void AddLeanInput(float direction)
        {
            leanDirection += direction;
            leanDirection = Mathf.Clamp(leanDirection, -1f ,1f);
        }
    }
    
    [Serializable]
    public struct AdsBlend
    {
        [Range(0f, 1f)] public float x;
        [Range(0f, 1f)] public float y;
        [Range(0f, 1f)] public float z;
    }
    
    public static class CoreToolkitLib
    {
        private const float FloatMin = 1e-10f;
        private const float SqrEpsilon = 1e-8f;

        public static float SpringInterp(float current, float target, ref SpringData springData, 
            ref SpringState state, float deltaTime)
        {
            float interpSpeed = Mathf.Min(deltaTime * springData.speed, 1f);
            target = Mathf.Clamp(target, -springData.maxValue, springData.maxValue);
            
            if (!Mathf.Approximately(interpSpeed, 0f))
            {
                float damping = 2 * Mathf.Sqrt(springData.stiffness) * springData.criticalDamping;
                float error = target - current;
                float errorDeriv = (error - state.error);
                state.velocity += error * springData.stiffness * interpSpeed + errorDeriv * damping;
                state.error = error;

                float value = current + state.velocity * interpSpeed;
                return value;
            }

            return current;
        }

        public static Vector3 SpringInterp(Vector3 current, Vector3 target, ref VectorSpringData springData, 
            ref VectorSpringState state, float deltaTime)
        {
            Vector3 final = Vector3.zero;

            final.x = SpringInterp(current.x, target.x * springData.scale.x, ref springData.x, ref state.x, deltaTime);
            final.y = SpringInterp(current.y, target.y * springData.scale.y, ref springData.y, ref state.y, deltaTime);
            final.z = SpringInterp(current.z, target.z * springData.scale.z, ref springData.z, ref state.z, deltaTime);

            return final;
        }

        public static float ExpDecay(float value, float deltaTime)
        {
            return 1 - Mathf.Exp(-value * deltaTime);
        }
        
        // Frame-rate independent interpolation
        public static float Interp(float a, float b, float speed, float deltaTime)
        {
            return Mathf.Lerp(a, b, ExpDecay(speed, deltaTime));
        }
        
        public static float InterpLayer(float a, float b, float speed, float deltaTime)
        {
            return Mathf.Approximately(speed, 0f) ? b : Interp(a, b, speed, deltaTime);
        }

        public static Vector3 Interp(Vector3 a, Vector3 b, float speed, float deltaTime)
        {
            return Vector3.Lerp(a, b, 1 - Mathf.Exp(-speed * deltaTime));
        }

        public static Vector2 Interp(Vector2 a, Vector2 b, float speed, float deltaTime)
        {
            return Vector2.Lerp(a, b, 1 - Mathf.Exp(-speed * deltaTime));
        }

        public static Quaternion Interp(Quaternion a, Quaternion b, float speed, float deltaTime)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-speed * deltaTime));
        }

        public static LocRot Interp(LocRot a, LocRot b, float speed, float deltaTime)
        {
            return LocRot.Lerp(a, b, ExpDecay(speed, deltaTime));
        }

        public static Quaternion RotateInBoneSpace(Quaternion parent, Quaternion boneRotation, Quaternion offset)
        {
            return parent * (offset * (Quaternion.Inverse(parent) * boneRotation));
        }
        
        public static void RotateInBoneSpace(Quaternion parent, Transform bone, Quaternion rotation, float alpha)
        {
            Quaternion outRot = rotation * (Quaternion.Inverse(parent) * bone.rotation);
            bone.rotation = Quaternion.Slerp(bone.rotation, parent * outRot, alpha);
        }
        
        public static void MoveInBoneSpace(Transform parent, Transform bone, Vector3 offset, float alpha)
        {
            var root = parent.transform;
            Vector3 finalOffset = root.TransformPoint(offset);
            finalOffset -= root.position;
            bone.position += finalOffset * alpha;
        }
        
        public static void DrawBone(Vector3 start, Vector3 end, float size)
        {
            Vector3 midpoint = (start + end) / 2;
                    
            Vector3 direction = end - start;
            float distance = direction.magnitude;
                    
            Matrix4x4 defaultMatrix = Gizmos.matrix;
                    
            Vector3 sizeVec = new Vector3(size, size, distance);
                    
            Gizmos.matrix = Matrix4x4.TRS(midpoint, Quaternion.LookRotation(direction), sizeVec);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = defaultMatrix;
        }

        public static Vector3 ToEuler(Quaternion rotation)
        {
            Vector3 newVec = rotation.eulerAngles;

            newVec.x = NormalizeAngle(newVec.x);
            newVec.y = NormalizeAngle(newVec.y);
            newVec.z = NormalizeAngle(newVec.z);

            return newVec;
        }

        // Adapted from Two Bone IK constraint, Unity Animation Rigging package
        public static void SolveTwoBoneIK(
            Transform root,
            Transform mid,
            Transform tip,
            Transform target,
            Transform hint,
            float posWeight,
            float rotWeight,
            float hintWeight
        )
        {
            Vector3 aPosition = root.position;
            Vector3 bPosition = mid.position;
            Vector3 cPosition = tip.position;
            Vector3 tPosition = Vector3.Lerp(cPosition, target.position, posWeight);
            Quaternion tRotation = Quaternion.Lerp(tip.rotation, target.rotation, rotWeight);
            bool hasHint = hint != null && hintWeight > 0f;

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 at = tPosition - aPosition;

            float abLen = ab.magnitude;
            float bcLen = bc.magnitude;
            float acLen = ac.magnitude;
            float atLen = at.magnitude;

            float oldAbcAngle = TriangleAngle(acLen, abLen, bcLen);
            float newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

            // Bend normal strategy is to take whatever has been provided in the animation
            // stream to minimize configuration changes, however if this is collinear
            // try computing a bend normal given the desired target position.
            // If this also fails, try resolving axis using hint if provided.
            Vector3 axis = Vector3.Cross(ab, bc);
            if (axis.sqrMagnitude < SqrEpsilon)
            {
                axis = hasHint ? Vector3.Cross(hint.position - aPosition, bc) : Vector3.zero;

                if (axis.sqrMagnitude < SqrEpsilon)
                    axis = Vector3.Cross(at, bc);

                if (axis.sqrMagnitude < SqrEpsilon)
                    axis = Vector3.up;
            }

            axis = Vector3.Normalize(axis);

            float a = 0.5f * (oldAbcAngle - newAbcAngle);
            float sin = Mathf.Sin(a);
            float cos = Mathf.Cos(a);
            Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
            mid.rotation = deltaR * mid.rotation;
            
            cPosition = tip.position;
            ac = cPosition - aPosition;
            root.rotation = FromToRotation(ac, at) * root.rotation;

            if (hasHint)
            {
                float acSqrMag = ac.sqrMagnitude;
                if (acSqrMag > 0f)
                {
                    bPosition = mid.position;
                    cPosition = tip.position;
                    ab = bPosition - aPosition;
                    ac = cPosition - aPosition;

                    Vector3 acNorm = ac / Mathf.Sqrt(acSqrMag);
                    Vector3 ah = hint.position - aPosition;
                    Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                    Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                    float maxReach = abLen + bcLen;
                    if (abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f)
                    {
                        Quaternion hintR = FromToRotation(abProj, ahProj);
                        hintR.x *= hintWeight;
                        hintR.y *= hintWeight;
                        hintR.z *= hintWeight;
                        hintR = NormalizeSafe(hintR);
                        root.rotation = hintR * root.rotation;
                    }
                }
            }

            tip.rotation = tRotation;
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle < -180f)
                angle += 360f;
            while (angle >= 180f)
                angle -= 360f;
            return angle;
        }
        
        public static float TriangleAngle(float aLen, float aLen1, float aLen2)
        {
            float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }

        public static Quaternion FromToRotation(Vector3 from, Vector3 to)
        {
            float theta = Vector3.Dot(from.normalized, to.normalized);
            if (theta >= 1f)
                return Quaternion.identity;

            if (theta <= -1f)
            {
                Vector3 axis = Vector3.Cross(from, Vector3.right);
                if (axis.sqrMagnitude == 0f)
                    axis = Vector3.Cross(from, Vector3.up);

                return Quaternion.AngleAxis(180f, axis);
            }

            return Quaternion.AngleAxis(Mathf.Acos(theta) * Mathf.Rad2Deg, Vector3.Cross(from, to).normalized);
        }

        public static Quaternion NormalizeSafe(Quaternion q)
        {
            float dot = Quaternion.Dot(q, q);
            if (dot > FloatMin)
            {
                float rsqrt = 1.0f / Mathf.Sqrt(dot);
                return new Quaternion(q.x * rsqrt, q.y * rsqrt, q.z * rsqrt, q.w * rsqrt);
            }

            return Quaternion.identity;
        }

        public static float debugStartTime = 0f;

        public static void DebugTimeStart()
        {
            debugStartTime = Time.realtimeSinceStartup;
        }

        public static void DebugTimeEnd(string eventName)
        {
            Debug.LogFormat("{0} took {1:F5} ms", eventName, (Time.realtimeSinceStartup - debugStartTime) * 1000f);
        }
    }
}