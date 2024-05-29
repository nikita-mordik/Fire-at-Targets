using System;

namespace KINEMATION.FPSAnimationFramework.Runtime.Core
{
    [Obsolete("use `FPSBoneController` instead.")]
    public interface IBoneController
    {
        public void Initialize();
        public void GameThreadUpdate();
        public void CachePose();
        public void EvaluatePose();
        public void PostEvaluatePose();
        public void ApplyCachedPose();
        public void LinkAnimatorProfile(FPSAnimatorProfile newProfile);
        public void UnlinkAnimatorProfile();
        public void LinkAnimatorLayer(FPSAnimatorLayerSettings newSettings);
        public void UpdateEntity(FPSAnimatorEntity newEntity);
        public void Dispose();
    }
}