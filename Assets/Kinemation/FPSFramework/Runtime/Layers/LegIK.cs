// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class LegIK : AnimLayer
    {
        [Header("Basic Settings")]
        [SerializeField] protected float footTraceLength;
        [SerializeField] protected float heightOffset;
        [SerializeField] protected float footInterpSpeed;
        [SerializeField] protected float pelvisInterpSpeed;
        [SerializeField] protected LayerMask layerName;

        [Header("Predictive Model")]

        protected LocRot smoothRfIK;
        protected LocRot smoothLfIK;
        protected LocRot targetRfIK;
        protected LocRot targetLfIK;
        
        protected float smoothPelvis;

        protected Vector3 traceStart;
        protected Vector3 traceEnd;

        protected Vector2 inputCache;
        
        private void OnDrawGizmos()
        {
        }

        private LocRot TraceFoot(Transform footTransform)
        {
            Vector3 origin = footTransform.position;
            origin.y = GetPelvis().position.y - heightOffset;
            
            traceStart = origin;
            traceEnd = traceStart - GetRootBone().up * footTraceLength;
            
            LocRot target = new LocRot(footTransform.position, footTransform.rotation);
            Quaternion finalRotation = footTransform.rotation;
            
            float animOffset = GetRootBone().InverseTransformPoint(footTransform.position).y;
            if (Physics.Raycast(origin, -GetRootBone().up, out RaycastHit hit, footTraceLength,layerName))
            {
                if (GetRootBone().InverseTransformPoint(hit.point).y < 0f)
                {
                    return LocRot.identity;
                }
                
                var rotation = footTransform.rotation;
                finalRotation = Quaternion.FromToRotation(GetRootBone().up, hit.normal) * rotation;
                finalRotation.Normalize();
                target.position = hit.point;
                
                target.position = new Vector3(target.position.x, target.position.y + animOffset, target.position.z);
            }
            
            target.position -= footTransform.position;
            target.rotation = Quaternion.Inverse(footTransform.rotation) * finalRotation;
            
            return target;
        }

        protected void TraceFeet()
        {
            targetRfIK = TraceFoot(GetRightFoot());
            targetLfIK = TraceFoot(GetLeftFoot());
        }
        
        public override void OnAnimUpdate()
        {
            if (Mathf.Approximately(smoothLayerAlpha, 0f))
            {
                return;
            }

            if ((GetCharData().moveInput - inputCache).magnitude > 0f)
            {
                //todo: call reset function here
            }
            inputCache = GetCharData().moveInput;

            TraceFeet();
            
            var rightFoot = GetRightFoot();
            var leftFoot = GetLeftFoot();
            
            smoothRfIK = CoreToolkitLib.Glerp(smoothRfIK, targetRfIK, footInterpSpeed);
            smoothLfIK = CoreToolkitLib.Glerp(smoothLfIK, targetLfIK, footInterpSpeed);
            
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), rightFoot, smoothRfIK.position, smoothLayerAlpha);
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), leftFoot, smoothLfIK.position, smoothLayerAlpha);

            rightFoot.rotation *= Quaternion.Slerp(Quaternion.identity, smoothRfIK.rotation, smoothLayerAlpha);
            leftFoot.rotation *= Quaternion.Slerp(Quaternion.identity, smoothLfIK.rotation, smoothLayerAlpha);
        }
    }
}