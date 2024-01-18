using Impact.Interactions;
using Impact.Interactions.Particles;
using Impact.Utility;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactParticleInteraction))]
    public class ImpactMaterialParticleInteractionEditor : Editor
    {
        private SerializedProperty minimumVelocityProperty;
        private SerializedProperty collisionNormalInfluenceProperty;
        private SerializedProperty particlePrefabProperty;
        private SerializedProperty isParticleLoopedProperty;
        private SerializedProperty emissionIntervalProperty;
        private SerializedProperty emissionIntervalTypeProperty;
        private SerializedProperty emitOnCollisionProperty;
        private SerializedProperty emitOnSlideProperty;
        private SerializedProperty emitOnRollProperty;

        private void OnEnable()
        {
            minimumVelocityProperty = serializedObject.FindProperty("_minimumVelocity");
            collisionNormalInfluenceProperty = serializedObject.FindProperty("_collisionNormalInfluence");
            particlePrefabProperty = serializedObject.FindProperty("_particlePrefab");
            isParticleLoopedProperty = serializedObject.FindProperty("_isParticleLooped");
            emissionIntervalProperty = serializedObject.FindProperty("_emissionInterval");
            emissionIntervalTypeProperty = serializedObject.FindProperty("_emissionIntervalType");
            emitOnCollisionProperty = serializedObject.FindProperty("_emitOnCollision");
            emitOnSlideProperty = serializedObject.FindProperty("_emitOnSlide");
            emitOnRollProperty = serializedObject.FindProperty("_emitOnRoll");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Particle Properties", EditorStyles.boldLabel);

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(particlePrefabProperty, new GUIContent("Particle Prefab", "The particle prefab to use."));

            if (particlePrefabProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("You must assign an Particle Prefab for this interaction.", MessageType.Error);
            }

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(isParticleLoopedProperty, new GUIContent("Is Particle Looped", "Is the particle prefab looped?"));

            ImpactEditorUtilities.Separator();

            EditorGUILayout.LabelField("Interaction Properties", EditorStyles.boldLabel);

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(minimumVelocityProperty, new GUIContent("Minimum Velocity", "The minimum velocity magnitude required to show particles."));
            ImpactEditorUtilities.DrawPropertyWithWiderLabel(collisionNormalInfluenceProperty, new GUIContent("Collision Normal Influence", "How much the collision normal should influence the calculated intensity."));

            EditorGUILayout.Separator();

            ImpactEditorUtilities.DrawToggleLeftProperty(emitOnCollisionProperty, new GUIContent("Emit On Collision", "Should particles be emitted on single collisions?"));
            ImpactEditorUtilities.DrawToggleLeftProperty(emitOnSlideProperty, new GUIContent("Emit On Slide", "Should particles be emitted when sliding?"));
            ImpactEditorUtilities.DrawToggleLeftProperty(emitOnRollProperty, new GUIContent("Emit On Roll", "Should particles be emitted when rolling?"));

            GUI.enabled = !isParticleLoopedProperty.boolValue && (emitOnSlideProperty.boolValue || emitOnRollProperty.boolValue);

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(emissionIntervalProperty, new GUIContent("Emission Interval (Min/Max)", "The interval at which particles should be emitted when sliding or rolling."));
            ImpactEditorUtilities.DrawPropertyWithWiderLabel(emissionIntervalTypeProperty, new GUIContent("Interval Type", "Whether the Emission Interval is defined in Time (seconds) or Distance."));

            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ImpactParticles))]
    [CanEditMultipleObjects]
    public class ImpactParticlesEditor : Editor
    {
        private SerializedProperty rotationModeProp;
        private SerializedProperty axisProp;
        private SerializedProperty poolSizeProp;
        private SerializedProperty poolFallbackModeProp;

        private ImpactParticles impactParticles;

        private void OnEnable()
        {
            rotationModeProp = serializedObject.FindProperty("_rotationMode");
            axisProp = serializedObject.FindProperty("_axis");
            poolSizeProp = serializedObject.FindProperty("_poolSize");
            poolFallbackModeProp = serializedObject.FindProperty("_poolFallbackMode");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            serializedObject.Update();

            impactParticles = target as ImpactParticles;

            EditorGUILayout.PropertyField(rotationModeProp, new GUIContent("Rotation Mode", "How should the particles be rotated?"));

            if (impactParticles.RotationMode != ImpactParticles.ParticleRotationMode.NoRotation)
                EditorGUILayout.PropertyField(axisProp, new GUIContent("Axis", "How should the object's axes be aligned to the surface?"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(poolSizeProp, new GUIContent("Pool Size", "The size of the object pool that will be created for these particles."));
            EditorGUILayout.PropertyField(poolFallbackModeProp, new GUIContent("Pool Fallback Mode", "Defines behavior of the object pool when there is no available object to retrieve."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}