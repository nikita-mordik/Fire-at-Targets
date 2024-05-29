// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Editor.Misc;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Editor.Tools
{
    public class FPSAnimatorEditorWindow : EditorWindow
    {
        private IEditorTool[] _editorTools;
    
        private int _tab = 0;

        [MenuItem("Window/FPS ANIMATOR/Tools")]
        public static void ShowWindow()
        {
            GetWindow<FPSAnimatorEditorWindow>("FPS ANIMATOR Tools");
        }

        private void OnEnable()
        {
            _editorTools = new IEditorTool[]
            {
                new IKAdditiveGenerator(),
                new CopyBoneModifier(),
                new AvatarMaskModifier()
            };
        }

        private void OnGUI()
        {
            _tab = GUILayout.Toolbar(_tab, new string[]
            {
                "IK Extractor",
                "Copy Bone Modifier",
                "Avatar Mask Modifier"
            });

            _editorTools[_tab].Render();
        }
    }
}