using Impact.Interactions.Audio;
using Impact.Utility;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactAudioInteraction))]
    public class ImpactMaterialAudioInteractionEditor : Editor
    {
        private ReorderableList collisionAudioClipsList;

        private SerializedProperty collisionAudioSelectionModeProperty;
        private SerializedProperty collisionAudioClipsProperty;
        private SerializedProperty slideAudioProperty;
        private SerializedProperty rollAudioProperty;
        private SerializedProperty audioSourceTemplateProperty;

        private SerializedProperty velocityRangeProperty;
        private SerializedProperty collisionNormalInfluenceProperty;
        private SerializedProperty scaleVolumeWithVelocityProperty;
        private SerializedProperty volumeScaleCurveProperty;
        private SerializedProperty randomPitchRangeProperty;
        private SerializedProperty randomVolumeRangeProperty;
        private SerializedProperty slideVelocityPitchMultiplierProperty;

        private void OnEnable()
        {
            velocityRangeProperty = serializedObject.FindProperty("_velocityRange");
            randomPitchRangeProperty = serializedObject.FindProperty("_randomPitchRange");
            randomVolumeRangeProperty = serializedObject.FindProperty("_randomVolumeRange");
            scaleVolumeWithVelocityProperty = serializedObject.FindProperty("_scaleVolumeWithVelocity");
            volumeScaleCurveProperty = serializedObject.FindProperty("_velocityVolumeScaleCurve");
            collisionNormalInfluenceProperty = serializedObject.FindProperty("_collisionNormalInfluence");
            slideVelocityPitchMultiplierProperty = serializedObject.FindProperty("_slideVelocityPitchMultiplier");

            collisionAudioSelectionModeProperty = serializedObject.FindProperty("_collisionAudioSelectionMode");
            collisionAudioClipsProperty = serializedObject.FindProperty("_collisionAudioClips");
            slideAudioProperty = serializedObject.FindProperty("_slideAudioClip");
            rollAudioProperty = serializedObject.FindProperty("_rollAudioClip");
            audioSourceTemplateProperty = serializedObject.FindProperty("_audioSourceTemplate");

            collisionAudioClipsList = new ReorderableList(serializedObject, serializedObject.FindProperty("_collisionAudioClips"), true, true, true, true);
            collisionAudioClipsList.drawHeaderCallback = drawCollisionAudioClipHeader;
            collisionAudioClipsList.drawElementCallback = drawCollisionAudioClipListItem;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            drawAudioProperties();

            ImpactEditorUtilities.Separator();

            drawInteractionProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void drawCollisionAudioClipHeader(Rect rect)
        {
            string name = "Collision Audio Clips";
            EditorGUI.LabelField(rect, name);
        }

        private void drawCollisionAudioClipListItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = collisionAudioClipsProperty.GetArrayElementAtIndex(index);

            ImpactAudioInteraction.CollisionAudioClipSelectionMode collisionAudioClipSelectionMode = (ImpactAudioInteraction.CollisionAudioClipSelectionMode)collisionAudioSelectionModeProperty.enumValueIndex;

            float leftMargin = 0;
            if (collisionAudioClipSelectionMode == ImpactAudioInteraction.CollisionAudioClipSelectionMode.Velocity)
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), (index + 1) + ")");
                leftMargin = 20;
            }

            EditorGUI.PropertyField(new Rect(rect.x + leftMargin, rect.y, rect.width - leftMargin, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        private void drawInteractionProperties()
        {
            EditorGUILayout.LabelField("Interaction Properties", EditorStyles.boldLabel);

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(velocityRangeProperty, new GUIContent("Velocity Range (Min/Max)", "The velocity magnitude range to use when calculating collision intensity."));

            EditorGUILayout.Separator();

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(collisionNormalInfluenceProperty, new GUIContent("Collision Normal Influence", "How much the normal should affect the intensity."));

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(scaleVolumeWithVelocityProperty, new GUIContent("Scale Volume With Velocity", "Should volume be scaled based on the velocity?"));

            if (scaleVolumeWithVelocityProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(volumeScaleCurveProperty, new GUIContent("Volume Scale Curve", ""));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(randomPitchRangeProperty, new GUIContent("Pitch Randomness (Min/Max)", "Random multiplier for the pitch."));
            ImpactEditorUtilities.DrawPropertyWithWiderLabel(randomVolumeRangeProperty, new GUIContent("Volume Randomness (Min/Max)", "Random multiplier for the volume."));

            EditorGUILayout.Separator();

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(slideVelocityPitchMultiplierProperty, new GUIContent("Slide Velocity Pitch Modifier", "How much to increase the pitch as sliding and rolling velocity increases."));
        }

        private void drawAudioProperties()
        {
            EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Space(2);

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(collisionAudioSelectionModeProperty, new GUIContent("Audio Clip Selection Mode", "Should audio clips be chosen based on Velocity or be chosen Randomly?"));

            GUILayout.Space(2);

            collisionAudioClipsList.DoLayoutList();

            GUILayout.Space(2);

            EditorGUILayout.HelpBox("You can drag-and-drop Audio Clips here to add them.", MessageType.Info);

            EditorGUILayout.EndVertical();

            //Drag and drop
            Rect listRect = GUILayoutUtility.GetLastRect();
            string[] paths;
            bool drop = ImpactEditorUtilities.DragAndDropArea(listRect, out paths);

            if (drop && paths != null)
            {
                ImpactAudioInteraction impactAudioInteraction = target as ImpactAudioInteraction;
                Undo.RecordObject(target, "Drag-and-drop Collision Audio Clips");

                for (int i = 0; i < paths.Length; i++)
                {
                    AudioClip a = AssetDatabase.LoadAssetAtPath<AudioClip>(paths[i]);
                    if (a != null)
                        impactAudioInteraction.CollisionAudioClips.Add(a);
                }

                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Separator();

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(slideAudioProperty, new GUIContent("Slide Audio", "The AudioClip to play when sliding."));
            ImpactEditorUtilities.DrawPropertyWithWiderLabel(rollAudioProperty, new GUIContent("Roll Audio", "The AudioClip to play when rolling."));

            EditorGUILayout.Separator();

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(audioSourceTemplateProperty, new GUIContent("Audio Source Template", "The audio source whose properties will be used when playing sounds from this interaction."));

            if (audioSourceTemplateProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("You must assign an Audio Source Template for sounds to play for this interaction.", MessageType.Error);
            }
        }
    }

    [CustomEditor(typeof(ImpactAudioSource))]
    [CanEditMultipleObjects]
    public class ImpactAudioSourceEditor : Editor
    {
        private SerializedProperty audioSourceProp;

        private SerializedProperty poolSizeProp;
        private SerializedProperty poolFallbackModeProp;

        private void OnEnable()
        {
            poolSizeProp = serializedObject.FindProperty("_poolSize");
            poolFallbackModeProp = serializedObject.FindProperty("_poolFallbackMode");
            audioSourceProp = serializedObject.FindProperty("_audioSource");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            serializedObject.Update();

            EditorGUILayout.PropertyField(audioSourceProp, new GUIContent("Audio Source", "The audio source to use for playing sounds."));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(poolSizeProp, new GUIContent("Pool Size", "The size of the object pool that will be created for these particles."));
            EditorGUILayout.PropertyField(poolFallbackModeProp, new GUIContent("Pool Fallback Mode", "Defines behavior of the object pool when there is no available object to retrieve."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}