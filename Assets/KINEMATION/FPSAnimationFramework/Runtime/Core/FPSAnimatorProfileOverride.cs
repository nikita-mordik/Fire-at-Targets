using UnityEditor;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Core
{
    /* Combing in v4.6.1.
    [HelpURL("https://kinemation.gitbook.io/scriptable-animation-system/fundamentals/animator-profiles")]
    public class FPSAnimatorProfileOverride : FPSAnimatorProfile
    {
        public FPSAnimatorProfile baseProfile;
        
#if UNITY_EDITOR
        private FPSAnimatorProfile _cachedBaseProfile;
        
        private void OnValidate()
        {
            if (_cachedBaseProfile == baseProfile)
            {
                return;
            }

            if (_cachedBaseProfile != null)
            {
                int num = settings.Count;
                for (int i = 0; i < num; i++)
                {
                    Undo.DestroyObjectImmediate(settings[0]);
                    settings.RemoveAt(0);
                }
                
                _cachedBaseProfile.UnRegisterOverride(this);
            }

            // Update the references when changing the asset.
            // Useful for different characters.
            if (baseProfile != null)
            {
                baseProfile.RegisterOverride(this);
                rigAsset = baseProfile.rigAsset;
                OnRigUpdated();
            }

            _cachedBaseProfile = baseProfile;
        }
#endif
    }*/
}