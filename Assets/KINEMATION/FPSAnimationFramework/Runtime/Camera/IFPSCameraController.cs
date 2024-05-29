using System;

namespace KINEMATION.FPSAnimationFramework.Runtime.Camera
{
    [Obsolete("use `FPSCameraController` instead.")]
    public interface IFPSCameraController
    {
        public void UpdateCamera();
        public void PlayCameraShake(FPSCameraShake newShake);
        public void UpdateTargetFOV(float newFov);
    }
}