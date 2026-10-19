using UnityEditor;

namespace TND.Upscaling.Framework
{
    [CustomEditor(typeof(UpscalerSettingsBase), true)]
    public class UpscalerSettingsEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
