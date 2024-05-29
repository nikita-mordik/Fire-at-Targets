using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Editor.Playables
{
    public class PlayablesControllerBaseEditor : UnityEditor.Editor
    {
        private IPlayablesController _controller;

        private void OnEnable()
        {
            _controller = (FPSPlayablesController) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Start Preview"))
            {
                _controller.StartEditorPreview();
            }
            
            if (GUILayout.Button("Stop Preview"))
            {
                _controller.StopEditorPreview();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}