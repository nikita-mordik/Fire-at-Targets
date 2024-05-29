// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Input;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.KAnimationCore.Editor.Input
{
    [CustomEditor(typeof(UserInputController), true)]
    public class UserInputControllerInspector : UnityEditor.Editor
    {
        private UserInputController _controller;
       
        private void OnEnable()
        {
            _controller = (UserInputController) target;
        }
        
        private static bool IsInspectorFocused() 
        {
            var focusedWindow = EditorWindow.focusedWindow;
            
            if (focusedWindow != null && focusedWindow.GetType().Name == "InspectorWindow") 
            {
                return true;
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var properties = _controller.GetPropertyBindings();

            if (properties == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var property in properties)
            {
                string label = property.Item1;
                object value = property.Item2;

                GUI.enabled = false;
                if (value is bool)
                {
                    EditorGUILayout.Toggle(label, (bool) value);
                }
                else if (value is int)
                {
                    EditorGUILayout.IntField(label, (int) value);
                }
                else if (value is float)
                {
                    EditorGUILayout.FloatField(label, (float) value);
                }
                else if (value is Vector4)
                {
                    EditorGUILayout.Vector4Field(label, (Vector4) value);
                }
                GUI.enabled = true;
            }
            
            EditorGUILayout.EndVertical();
            if(!IsInspectorFocused()) Repaint();
        }
    }
}