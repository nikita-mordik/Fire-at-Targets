using Impact.TagLibrary;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactTagLibrary))]
    public class ImpactTagLibraryEditor : Editor
    {
        private SerializedProperty tagsListProperty;

        private void OnEnable()
        {
            tagsListProperty = serializedObject.FindProperty("_tagNames");
        }

        public override void OnInspectorGUI()
        {
            drawTagsList();
        }

        private void drawTagsList()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            for (int i = 0; i < ImpactTagLibraryConstants.TagCount; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i + ":", GUILayout.Width(40));
                SerializedProperty t = tagsListProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(t, new GUIContent());

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}