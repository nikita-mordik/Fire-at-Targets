using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using TND.Common;

namespace TND.Upscaling.Framework
{
    public abstract class UpscalerControllerEditor : Editor
    {
        internal static string onlineDocumentation = "https://docs.google.com/document/d/1X4ayGIDx-7bRk4p_B4vmJNvS9W0UDUuAmkxtlerajwY";
        private const string ShowAdvancedKey = "TND.Upscaling.Framework.Editor_ShowAdvancedSettings";
        private const string FoldoutStateKey = "TND.Upscaling.Framework.Editor_FoldoutState_";

        private class SettingsEditorState
        {
            public Editor editor;
            public string identifier;
            public string displayName;
            public bool foldOut;
        }

        private readonly List<IUpscalerPlugin> _sortedUpscalerPlugins = new();
        private readonly List<SettingsEditorState> _sortedUpscalerSettingsEditors = new();

        private SerializedProperty _upscalerChainProperty;
        private SerializedProperty _upscalerQualityProperty;
        private SerializedProperty _upscalerSharpeningProperty;
        private SerializedProperty _upscalerSharpnessProperty;
        private SerializedProperty _upscalerInjectionPointProperty;
        private SerializedProperty _upscalerAntiLagProperty;
        private SerializedProperty _upscalerSettingsProperty;
        private SerializedProperty _upscalerEnableAutoReactiveProperty;
        private SerializedProperty _upscalerAutoReactiveSettingsProperty;
        private SerializedProperty _upscalerEnableCustomReactiveProperty;
        private SerializedProperty _upscalerCustomReactiveLayerProperty;

        private ReorderableList _upscalerChainList;
        private bool _showAdvancedSettings;
        
        protected virtual void OnEnable()
        {
            var upscalerScript = serializedObject.targetObject as UpscalerController;
            if (upscalerScript == null)
                return;

            serializedObject.Update();

            _upscalerChainProperty = serializedObject.FindProperty(nameof(UpscalerController.upscalerChain));
            _upscalerQualityProperty = serializedObject.FindProperty(nameof(UpscalerController.qualityMode));
            _upscalerSharpeningProperty = serializedObject.FindProperty(nameof(UpscalerController.enableSharpening));
            _upscalerSharpnessProperty = serializedObject.FindProperty(nameof(UpscalerController.sharpness));
            _upscalerInjectionPointProperty = serializedObject.FindProperty(nameof(UpscalerController.injectionPoint));
            _upscalerSettingsProperty = serializedObject.FindProperty(nameof(UpscalerController.upscalerSettings));
            _upscalerEnableAutoReactiveProperty = serializedObject.FindProperty(nameof(UpscalerController.autoGenerateReactiveMask));
            _upscalerAutoReactiveSettingsProperty = serializedObject.FindProperty(nameof(UpscalerController.autoReactiveSettings));
            _upscalerEnableCustomReactiveProperty = serializedObject.FindProperty(nameof(UpscalerController.enableCustomReactiveMask));
            _upscalerCustomReactiveLayerProperty = serializedObject.FindProperty(nameof(UpscalerController.customReactiveMaskLayer));
            //_upscalerAntiLagProperty = serializedObject.FindProperty(nameof(UpscalerController.enableAntiLag));

            _sortedUpscalerPlugins.Clear();
            _sortedUpscalerPlugins.AddRange(UpscalerPluginRegistry.GetUpscalerPlugins());
            _sortedUpscalerPlugins.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCulture));

            _sortedUpscalerSettingsEditors.Clear();
            for (int i = 0; i < _upscalerSettingsProperty.arraySize; ++i)
            {
                var pairProperty = _upscalerSettingsProperty.GetArrayElementAtIndex(i);
                var identifierProperty = pairProperty.FindPropertyRelative(nameof(UpscalerController.UpscalerSettingsPair.identifier));
                var settingsProperty = pairProperty.FindPropertyRelative(nameof(UpscalerController.UpscalerSettingsPair.settings));

                var upscalerPlugin = _sortedUpscalerPlugins.Find(p => p.Identifier == identifierProperty.stringValue);
                if (upscalerPlugin != null)
                {
                    // Ensure there is always a valid settings object
                    if (settingsProperty.objectReferenceValue == null)
                    {
                        var settingsObject = upscalerPlugin.CreateSettings();
                        settingsProperty.objectReferenceValue = settingsObject;
                        serializedObject.ApplyModifiedProperties();
                    }

                    _sortedUpscalerSettingsEditors.Add(new SettingsEditorState
                    {
                        editor = CreateEditor(settingsProperty.objectReferenceValue),
                        identifier = upscalerPlugin.Identifier,
                        displayName = upscalerPlugin.DisplayName,
                        foldOut = EditorPrefs.GetBool(FoldoutStateKey + upscalerPlugin.Identifier),
                    });
                }
            }
            _sortedUpscalerSettingsEditors.Sort((a, b) => string.Compare(a.displayName, b.displayName, StringComparison.CurrentCulture));

            // Create a nice reorderable list view for the fallback chain, with a constrained set of upscaler options that may be added
            _upscalerChainList = new PriorityList<IUpscalerPlugin>(serializedObject, _upscalerChainProperty, "Upscalers by Priority", _sortedUpscalerPlugins, new UpscalerPluginItemAdapter());
            _showAdvancedSettings = EditorPrefs.GetBool(ShowAdvancedKey);
        }

        private struct UpscalerPluginItemAdapter : PriorityList<IUpscalerPlugin>.IItemAdapter
        {
            public string GetIdentifier(IUpscalerPlugin item) => item.Identifier;
            public string GetDisplayName(IUpscalerPlugin item) => item.DisplayName;
        }

        protected virtual void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            EditorVisuals.GenerateHeader(onlineDocumentation);

            if (DrawValidations())
            {
                // Render pipeline-agnostic warnings
                if (_upscalerChainList.count == 0)
                {
                    EditorGUILayout.HelpBox("No upscalers have been added to the 'Upscaler by Priority' list.\r\nAs long as this list is empty, the TND Upscaler Framework will not perform any visual upscaling.", MessageType.Warning);
                }
                else
                {
                    EditorVisuals.GenerateCrossPromo();
                }

                DrawSettings();
            }

            EditorVisuals.GenerateFooter();
        }

        protected virtual bool DrawValidations()
        {
            return true;
        }

        /// <summary>
        /// Generate the actual upscaler settings
        /// </summary>
        private void DrawSettings()
        {
            serializedObject.Update();
            
            DrawUpscalerSettings();
            DrawAdvancedUpscalerSettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUpscalerSettings()
        {
            EditorGUILayout.Space();
            _upscalerChainList.DoLayoutList();

            int prevQualityIndex = _upscalerQualityProperty.enumValueIndex;
            EditorGUILayout.PropertyField(_upscalerQualityProperty);
            if (_upscalerQualityProperty.enumValueIndex != prevQualityIndex)
            {
                // Disable upscaler component when quality mode is set to Off, enable it otherwise
                serializedObject.FindProperty("m_Enabled").boolValue = _upscalerQualityProperty.enumValueIndex > 0;
            }

            EditorGUILayout.PropertyField(_upscalerSharpeningProperty);
            if (_upscalerSharpeningProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_upscalerSharpnessProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_upscalerInjectionPointProperty);

            DrawAdditionalUpscalerSettings();
        }

        private void DrawAdvancedUpscalerSettings()
        {
            EditorGUILayout.Space();
            bool newAdvancedSettings = EditorGUILayout.BeginToggleGroup("Show Expert Settings", _showAdvancedSettings);
            if (newAdvancedSettings != _showAdvancedSettings)
            {
                _showAdvancedSettings = newAdvancedSettings;
                EditorPrefs.SetBool(ShowAdvancedKey, newAdvancedSettings);
            }

            if (_showAdvancedSettings)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Universal Settings:", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                // Auto-generate reactive mask settings
                EditorGUILayout.PropertyField(_upscalerEnableAutoReactiveProperty);
                if (_upscalerEnableAutoReactiveProperty.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_upscalerAutoReactiveSettingsProperty);
                    EditorGUI.indentLevel--;
                }

                // Custom reactive mask settings
                // EditorGUILayout.PropertyField(_upscalerEnableCustomReactiveProperty);
                // if (_upscalerEnableCustomReactiveProperty.boolValue)
                // {
                //     EditorGUI.indentLevel++;
                //     EditorGUILayout.PropertyField(_upscalerCustomReactiveLayerProperty);
                //     EditorGUI.indentLevel--;
                // }

                EditorGUI.indentLevel--;

                DrawAdditionalAdvancedSettings();

                EditorGUILayout.Space();

                if (_sortedUpscalerSettingsEditors.Count > 0)
                {
                    EditorGUILayout.LabelField("Upscaler Specific Settings:", EditorStyles.boldLabel);
                    EditorGUILayout.Space();

                    EditorGUI.indentLevel++;
                    // Per-upscaler advanced settings
                    foreach (var settingsEditorState in _sortedUpscalerSettingsEditors)
                    {
                        bool newFoldoutState = EditorGUILayout.BeginFoldoutHeaderGroup(settingsEditorState.foldOut, settingsEditorState.displayName);
                        if (newFoldoutState != settingsEditorState.foldOut)
                        {
                            settingsEditorState.foldOut = newFoldoutState;
                            EditorPrefs.SetBool(FoldoutStateKey + settingsEditorState.identifier, newFoldoutState);
                        }

                        if (settingsEditorState.foldOut && settingsEditorState.editor != null)
                        {
                            settingsEditorState.editor.OnInspectorGUI();
                        }

                        EditorGUILayout.EndFoldoutHeaderGroup();

                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndToggleGroup();
        }

        private void DrawAntiLagSettings()
        {
            //EditorGUILayout.PropertyField(_upscalerAntiLagProperty);
            //if (_upscalerAntiLagProperty.boolValue)
            //{
            //    EditorGUI.indentLevel++;
            //    var style = new GUIStyle(EditorStyles.label)
            //    {
            //        wordWrap = false,
            //        clipping = TextClipping.Ellipsis,
            //        fixedHeight = 19
            //    };
            //    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            //    var icon = EditorGUIUtility.IconContent("console.erroricon").image;
            //    GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
            //    GUILayout.Label("Anti-Lag package is missing.", style);
            //    if (GUILayout.Button("Import Package", GUILayout.Width(100)))
            //    {
            //        Application.OpenURL("https://assetstore.unity.com/publishers/14941");
            //    }
            //    EditorGUILayout.EndHorizontal();
            //    EditorGUI.indentLevel--;
            //}
        }

        protected virtual void DrawAdditionalUpscalerSettings() { }

        protected virtual void DrawAdditionalAdvancedSettings() { }

        protected void DrawErrorHelpText()
        {
            EditorGUILayout.HelpBox("Something has gone wrong, please contact 'The Naked Dev' for help!", MessageType.Error);
            if (GUILayout.Button("Contact 'The Naked Dev' on Discord"))
            {
                Application.OpenURL(EditorVisuals.discordURL);
            }
        }

        private struct FoldoutGroupScope : IDisposable
        {
            private readonly bool _foldoutState;

            public static implicit operator bool(FoldoutGroupScope self) => self._foldoutState;
            
            public FoldoutGroupScope(ref bool foldout, string title, string prefsKey)
            {
                _foldoutState = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, title);
                if (_foldoutState != foldout)
                {
                    foldout = _foldoutState;
                    EditorPrefs.SetBool(prefsKey, _foldoutState);
                }
            }
            
            public void Dispose()
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }

    public static class UpscalerControllerEditorExtensions
    {
        public static TEnum GetEnumValue<TEnum>(this SerializedProperty serializedProperty)
            where TEnum : Enum
        {
            Array values = Enum.GetValues(typeof(TEnum));
            int index = serializedProperty.enumValueIndex;
            if (index < 0 || index >= values.Length)
                return default;

            return (TEnum)values.GetValue(index);
        }

        public static void SetEnumValue<TEnum>(this SerializedProperty serializedProperty, TEnum enumValue)
            where TEnum : Enum
        {
            serializedProperty.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(TEnum)), enumValue);
        }
    }
}
