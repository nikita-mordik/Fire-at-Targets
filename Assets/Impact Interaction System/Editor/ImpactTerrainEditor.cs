using Impact.Objects;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactTerrain))]
    public class ImpactTerrainEditor : Editor
    {
        private ImpactTerrain impactTerrain;
        private TerrainLayer[] terrainLayers;

        private SerializedProperty terrainProperty;
        private SerializedProperty terrainMaterialsProperty;

        private void OnEnable()
        {
            impactTerrain = target as ImpactTerrain;

            terrainProperty = serializedObject.FindProperty("_terrain");
            terrainMaterialsProperty = serializedObject.FindProperty("_terrainMaterials");

            if (impactTerrain.Terrain != null && impactTerrain.Terrain.terrainData != null)
            {
                impactTerrain.SyncTerrainLayersAndMaterialsList();
                terrainLayers = impactTerrain.Terrain.terrainData.terrainLayers;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Terrain", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(terrainProperty, new GUIContent("", "The Terrain this object is associated with."));

            if (impactTerrain.Terrain == null || impactTerrain.Terrain.terrainData == null)
            {
                EditorGUILayout.HelpBox("Assign a Terrain to begin editing Terrain Materials.", MessageType.Info);
            }
            else
            {
                ImpactEditorUtilities.Separator();

                drawTerrainLayersList();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void drawTerrainLayersList()
        {
            EditorGUILayout.LabelField("Terrain Layer Materials", EditorStyles.boldLabel);

            for (int i = 0; i < terrainMaterialsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical();

                drawTerrainLayerMaterial(i);

                GUILayout.Space(4);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Refresh Terrain Layers", "Manually re-sync the stored materials with the terrain layers.")))
            {
                impactTerrain.SyncTerrainLayersAndMaterialsList();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
        }

        private void drawTerrainLayerMaterial(int index)
        {
            EditorGUILayout.BeginHorizontal();

            TerrainLayer layer = terrainLayers[index];

            if (layer != null)
                GUILayout.Box(layer.diffuseTexture, GUILayout.Height(40), GUILayout.Width(40));
            else
                GUILayout.Box(new GUIContent(), GUILayout.Height(40), GUILayout.Width(40));

            EditorGUILayout.BeginVertical();

            if (layer != null)
                EditorGUILayout.LabelField(layer.diffuseTexture.name);
            else
                EditorGUILayout.LabelField("Missing Terrain Layer");

            SerializedProperty terrainMaterial = terrainMaterialsProperty.GetArrayElementAtIndex(index);
            EditorGUILayout.PropertyField(terrainMaterial, new GUIContent());

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}