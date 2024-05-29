// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.PoseOffsetLayer
{
    public class PoseOffsetLayerState : FPSAnimatorLayerState
    {
        private PoseOffsetLayerSettings _settings;
        private Transform[] _boneReferences;
        
        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (PoseOffsetLayerSettings) newSettings;
            _boneReferences = new Transform[_settings.poseOffsets.Count];

            int count = _boneReferences.Length;

            for (int i = 0; i < count; i++)
            {
                _boneReferences[i] = _rigComponent.GetRigTransform(_settings.poseOffsets[i].pose.element);
            }
        }

        public override void OnEvaluatePose()
        {
            int count = _boneReferences.Length;

            for (int i = 0; i < count; i++)
            {
                bool affectChildren = _settings.poseOffsets[i].keepChildrenPose;
                int childCount = _boneReferences[i].childCount;
                KTransform[] children = null;

                if (affectChildren)
                {
                    children = new KTransform[childCount];

                    for (int j = 0; j < childCount; j++)
                    {
                        children[j] = new KTransform(_boneReferences[i].GetChild(j));
                    }
                }
                
                PoseOffset poseOffset = _settings.poseOffsets[i];

                float weight = Weight * GetCurveBlendValue(poseOffset.blend);
                
                KAnimationMath.ModifyTransform(_owner.transform, _boneReferences[i], poseOffset.pose,
                    weight);

                if (!affectChildren) continue;

                for (int j = 0; j < childCount; j++)
                {
                    var childTransform = _boneReferences[i].GetChild(j);

                    childTransform.position = Vector3.Lerp(childTransform.position, children[j].position, weight);
                    childTransform.rotation = Quaternion.Slerp(childTransform.rotation, children[j].rotation, weight);
                }
            }
        }
    }
}