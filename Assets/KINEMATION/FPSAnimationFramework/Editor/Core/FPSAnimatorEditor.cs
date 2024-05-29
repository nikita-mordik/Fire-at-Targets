// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Editor.Core
{
    [CustomEditor(typeof(FPSAnimator), true)]
    public class FPSAnimatorEditor : UnityEditor.Editor
    {
        private FPSAnimator _fpsAnimator;
        
        private void OnEnable()
        {
            _fpsAnimator = target as FPSAnimator;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Initialize Component", EditorStyles.miniButton))
            {
                _fpsAnimator.CreateIkElements();
            }
        }
    }
}