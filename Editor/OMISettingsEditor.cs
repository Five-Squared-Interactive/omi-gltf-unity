// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace OMI.Editor
{
    /// <summary>
    /// Custom inspector for OMISettings ScriptableObject.
    /// </summary>
    [CustomEditor(typeof(OMISettings))]
    public class OMISettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _enabledExtensionsProp;
        private SerializedProperty _useDefaultHandlersProp;
        private SerializedProperty _debugModeProp;
        private SerializedProperty _autoRegisterHandlersProp;

        private bool _extensionsFoldout = true;

        // All available OMI extensions
        private static readonly string[] AllExtensions = new[]
        {
            // Physics
            "OMI_physics_shape",
            "OMI_physics_body",
            "OMI_physics_joint",
            "OMI_physics_gravity",
            
            // Audio
            "KHR_audio_emitter",
            "OMI_audio_ogg_vorbis",
            "OMI_audio_opus",
            
            // Interaction
            "OMI_spawn_point",
            "OMI_seat",
            "OMI_link",
            "OMI_personality",
            
            // Vehicles
            "OMI_vehicle_body",
            "OMI_vehicle_wheel",
            "OMI_vehicle_thruster",
            "OMI_vehicle_hover_thruster",
            
            // Environment
            "OMI_environment_sky"
        };

        private void OnEnable()
        {
            _enabledExtensionsProp = serializedObject.FindProperty("enabledExtensions");
            _useDefaultHandlersProp = serializedObject.FindProperty("useDefaultHandlers");
            _debugModeProp = serializedObject.FindProperty("debugMode");
            _autoRegisterHandlersProp = serializedObject.FindProperty("autoRegisterHandlers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("OMI Extension Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // General settings
            EditorGUILayout.PropertyField(_useDefaultHandlersProp, 
                new GUIContent("Use Default Handlers", "Use the built-in Unity handlers for OMI extensions."));
            EditorGUILayout.PropertyField(_autoRegisterHandlersProp,
                new GUIContent("Auto Register Handlers", "Automatically register handlers on import."));
            EditorGUILayout.PropertyField(_debugModeProp,
                new GUIContent("Debug Mode", "Enable verbose logging for debugging."));
            
            EditorGUILayout.Space();

            // Extensions list with checkboxes
            _extensionsFoldout = EditorGUILayout.Foldout(_extensionsFoldout, "Enabled Extensions", true);
            if (_extensionsFoldout)
            {
                EditorGUI.indentLevel++;
                
                // Quick actions
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Enable All", EditorStyles.miniButtonLeft))
                {
                    EnableAllExtensions();
                }
                if (GUILayout.Button("Disable All", EditorStyles.miniButtonMid))
                {
                    DisableAllExtensions();
                }
                if (GUILayout.Button("Default", EditorStyles.miniButtonRight))
                {
                    SetDefaultExtensions();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();

                // Physics extensions
                EditorGUILayout.LabelField("Physics", EditorStyles.miniBoldLabel);
                DrawExtensionToggle("OMI_physics_shape");
                DrawExtensionToggle("OMI_physics_body");
                DrawExtensionToggle("OMI_physics_joint");
                DrawExtensionToggle("OMI_physics_gravity");

                EditorGUILayout.Space();

                // Audio extensions
                EditorGUILayout.LabelField("Audio", EditorStyles.miniBoldLabel);
                DrawExtensionToggle("KHR_audio_emitter");
                DrawExtensionToggle("OMI_audio_ogg_vorbis");
                DrawExtensionToggle("OMI_audio_opus");

                EditorGUILayout.Space();

                // Interaction extensions
                EditorGUILayout.LabelField("Interaction", EditorStyles.miniBoldLabel);
                DrawExtensionToggle("OMI_spawn_point");
                DrawExtensionToggle("OMI_seat");
                DrawExtensionToggle("OMI_link");
                DrawExtensionToggle("OMI_personality");

                EditorGUILayout.Space();

                // Vehicle extensions
                EditorGUILayout.LabelField("Vehicles", EditorStyles.miniBoldLabel);
                DrawExtensionToggle("OMI_vehicle_body");
                DrawExtensionToggle("OMI_vehicle_wheel");
                DrawExtensionToggle("OMI_vehicle_thruster");
                DrawExtensionToggle("OMI_vehicle_hover_thruster");

                EditorGUILayout.Space();

                // Environment extensions
                EditorGUILayout.LabelField("Environment", EditorStyles.miniBoldLabel);
                DrawExtensionToggle("OMI_environment_sky");

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawExtensionToggle(string extensionName)
        {
            var enabled = IsExtensionEnabled(extensionName);
            var newEnabled = EditorGUILayout.ToggleLeft(extensionName, enabled);
            
            if (newEnabled != enabled)
            {
                if (newEnabled)
                {
                    EnableExtension(extensionName);
                }
                else
                {
                    DisableExtension(extensionName);
                }
            }
        }

        private bool IsExtensionEnabled(string extensionName)
        {
            for (int i = 0; i < _enabledExtensionsProp.arraySize; i++)
            {
                if (_enabledExtensionsProp.GetArrayElementAtIndex(i).stringValue == extensionName)
                {
                    return true;
                }
            }
            return false;
        }

        private void EnableExtension(string extensionName)
        {
            if (!IsExtensionEnabled(extensionName))
            {
                _enabledExtensionsProp.arraySize++;
                _enabledExtensionsProp.GetArrayElementAtIndex(_enabledExtensionsProp.arraySize - 1).stringValue = extensionName;
            }
        }

        private void DisableExtension(string extensionName)
        {
            for (int i = _enabledExtensionsProp.arraySize - 1; i >= 0; i--)
            {
                if (_enabledExtensionsProp.GetArrayElementAtIndex(i).stringValue == extensionName)
                {
                    _enabledExtensionsProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
        }

        private void EnableAllExtensions()
        {
            _enabledExtensionsProp.ClearArray();
            foreach (var ext in AllExtensions)
            {
                _enabledExtensionsProp.arraySize++;
                _enabledExtensionsProp.GetArrayElementAtIndex(_enabledExtensionsProp.arraySize - 1).stringValue = ext;
            }
        }

        private void DisableAllExtensions()
        {
            _enabledExtensionsProp.ClearArray();
        }

        private void SetDefaultExtensions()
        {
            _enabledExtensionsProp.ClearArray();
            
            // Default: physics, spawn, seat, link
            var defaults = new[] 
            { 
                "OMI_physics_shape", "OMI_physics_body", "OMI_physics_joint",
                "OMI_spawn_point", "OMI_seat", "OMI_link" 
            };
            
            foreach (var ext in defaults)
            {
                _enabledExtensionsProp.arraySize++;
                _enabledExtensionsProp.GetArrayElementAtIndex(_enabledExtensionsProp.arraySize - 1).stringValue = ext;
            }
        }
    }

    /// <summary>
    /// Editor menu items and utilities for OMI.
    /// </summary>
    public static class OMIEditorMenu
    {
        private const string SettingsPath = "Assets/OMI/OMISettings.asset";

        [MenuItem("OMI/Settings", priority = 0)]
        public static void SelectSettings()
        {
            var settings = GetOrCreateSettings();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        [MenuItem("OMI/Create/Settings Asset", priority = 100)]
        public static void CreateSettingsAsset()
        {
            var settings = ScriptableObject.CreateInstance<OMISettings>();
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/OMI"))
            {
                AssetDatabase.CreateFolder("Assets", "OMI");
            }

            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = settings;
            Debug.Log("[OMI] Created settings asset at " + SettingsPath);
        }

        [MenuItem("OMI/Documentation", priority = 200)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/omigroup/gltf-extensions");
        }

        public static OMISettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<OMISettings>(SettingsPath);
            
            if (settings == null)
            {
                // Try to find any existing settings
                var guids = AssetDatabase.FindAssets("t:OMISettings");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    settings = AssetDatabase.LoadAssetAtPath<OMISettings>(path);
                }
            }

            if (settings == null)
            {
                // Create new settings
                CreateSettingsAsset();
                settings = AssetDatabase.LoadAssetAtPath<OMISettings>(SettingsPath);
            }

            return settings;
        }
    }
}
