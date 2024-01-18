using Impact.Interactions;
using Impact.Interactions.Decals;
using Impact.Utility;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactDecalInteraction))]
    public class ImpactMaterialDecalInteractionEditor : Editor
    {
        private SerializedProperty minimumVelocityProperty;
        private SerializedProperty collisionNormalInfluenceProperty;
        private SerializedProperty decalPrefabProperty;
        private SerializedProperty creationIntervalProperty;
        private SerializedProperty creationIntervalTypeProperty;
        private SerializedProperty createOnCollisionProperty;
        private SerializedProperty createOnSlideProperty;
        private SerializedProperty createOnRollProperty;

        private void OnEnable()
        {
            minimumVelocityProperty = serializedObject.FindProperty("_minimumVelocity");
            collisionNormalInfluenceProperty = serializedObject.FindProperty("_collisionNormalInfluence");
            decalPrefabProperty = serializedObject.FindProperty("_decalPrefab");
            creationIntervalProperty = serializedObject.FindProperty("_creationInterval");
            creationIntervalTypeProperty = serializedObject.FindProperty("_creationIntervalType");
            createOnCollisionProperty = serializedObject.FindProperty("_createOnCollision");
            createOnSlideProperty = serializedObject.FindProperty("_createOnSlide");
            createOnRollProperty = serializedObject.FindProperty("_createOnRoll");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Decal Properties", EditorStyles.boldLabel);

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(decalPrefabProperty, new GUIContent("Decal Prefab", "The decal prefab to use."));

            if (decalPrefabProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("You must assign an Decal Prefab for this interaction.", MessageType.Error);
            }

            ImpactEditorUtilities.Separator();

            EditorGUILayout.LabelField("Interaction Properties", EditorStyles.boldLabel);

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(minimumVelocityProperty, new GUIContent("Minimum Velocity", "The minimum velocity magnitude required to place a decal."));
            ImpactEditorUtilities.DrawPropertyWithWiderLabel(collisionNormalInfluenceProperty, new GUIContent("Collision Normal Influence", "How much the collision normal should influence the calculated intensity."));

            EditorGUILayout.Separator();

            ImpactEditorUtilities.DrawToggleLeftProperty(createOnCollisionProperty, new GUIContent("Create On Collision", "Should decals be placed on single collisions?"));
            ImpactEditorUtilities.DrawToggleLeftProperty(createOnSlideProperty, new GUIContent("Create On Slide", "Should decals be placed when sliding?"));
            ImpactEditorUtilities.DrawToggleLeftProperty(createOnRollProperty, new GUIContent("Create On Roll", "Should decals be placed when rolling?"));

            GUI.enabled = createOnSlideProperty.boolValue || createOnRollProperty.boolValue;

            ImpactEditorUtilities.DrawPropertyWithWiderLabel(creationIntervalProperty, new GUIContent("Creation Interval (Min/Max)", "The interval at which decals should be placed when sliding or rolling."));
            ImpactEditorUtilities.DrawPropertyWithWiderLabel(creationIntervalTypeProperty, new GUIContent("Interval Type", "Whether the Creation Interval is defined in Time (seconds) or Distance."));

            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ImpactDecal))]
    [CanEditMultipleObjects]
    public class ImpactDecalEditor : Editor
    {
        private SerializedProperty poolSizeProp;
        private SerializedProperty poolFallbackModeProp;
        private SerializedProperty decalDistanceProp;
        private SerializedProperty parentToObjectProp;
        private SerializedProperty rotationModeProp;
        private SerializedProperty axisProp;

        private void OnEnable()
        {
            poolSizeProp = serializedObject.FindProperty("_poolSize");
            poolFallbackModeProp = serializedObject.FindProperty("_poolFallbackMode");

            decalDistanceProp = serializedObject.FindProperty("_decalDistance");
            parentToObjectProp = serializedObject.FindProperty("_parentToObject");
            rotationModeProp = serializedObject.FindProperty("_rotationMode");
            axisProp = serializedObject.FindProperty("_axis");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            serializedObject.Update();

            EditorGUILayout.PropertyField(decalDistanceProp, new GUIContent("Decal Distance", "How far the pivot of the decal should be placed from the surface."));
            EditorGUILayout.PropertyField(rotationModeProp, new GUIContent("Rotation Mode", "How should the decal be rotated?"));
            EditorGUILayout.PropertyField(axisProp, new GUIContent("Axis", "Which axis should be pointed towards the surface?"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(parentToObjectProp, new GUIContent("Parent to Object", "Should the decal be parented to the object it is placed on?"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(poolSizeProp, new GUIContent("Pool Size", "The size of the object pool that will be created for this decal."));
            EditorGUILayout.PropertyField(poolFallbackModeProp, new GUIContent("Pool Fallback Mode", "Defines behavior of the object pool when there is no available object to retrieve."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}