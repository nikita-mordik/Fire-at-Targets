// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Recoil;
using KINEMATION.KAnimationCore.Editor.Misc;
using UnityEditor;

namespace KINEMATION.FPSAnimationFramework.Editor.Recoil
{
    [CustomEditor(typeof(RecoilAnimData))]
    public class RecoilAnimDataEditor : UnityEditor.Editor
    {
        private TabInspectorWidget _tabInspectorWidget;

        private void OnEnable()
        {
            _tabInspectorWidget = new TabInspectorWidget(serializedObject);
            _tabInspectorWidget.Init();
        }

        public override void OnInspectorGUI()
        {
            _tabInspectorWidget.Render();
        }
    }
}