// Designed by KINEMATION, 2024

namespace KINEMATION.FPSAnimationFramework.Runtime.Playables
{
    public interface IPlayablesController
    {
        public bool InitializeController();
        
        public void SetControllerWeight(float weight);
        
        public bool PlayPose(FPSAnimationAsset asset);
        public bool PlayAnimation(FPSAnimationAsset asset, float startTime = 0f);
        public void StopAnimation(float blendOutTime);
        public bool IsPlaying();

        public float GetCurveValue(string curveName, bool isAnimator = false);
        
#if UNITY_EDITOR
        public void StartEditorPreview();
        public void StopEditorPreview();
#endif
    }
}