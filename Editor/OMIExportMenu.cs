// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OMI.Editor
{
    /// <summary>
    /// Editor menu items and utilities for OMI glTF export.
    /// </summary>
    public static class OMIExportMenu
    {
        private const string MenuRoot = "OMI/Export/";

        /// <summary>
        /// Exports the selected GameObjects to a glTF file.
        /// </summary>
        [MenuItem(MenuRoot + "Export Selected to glTF...", false, 100)]
        public static async void ExportSelectedToGltf()
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                EditorUtility.DisplayDialog("Export Error", "Please select one or more GameObjects to export.", "OK");
                return;
            }

            var defaultName = selection[0].name + ".gltf";
            var path = EditorUtility.SaveFilePanel(
                "Export glTF with OMI Extensions",
                GetLastExportDirectory(),
                defaultName,
                "gltf"
            );

            if (string.IsNullOrEmpty(path)) return;

            SaveLastExportDirectory(Path.GetDirectoryName(path));

            try
            {
                EditorUtility.DisplayProgressBar("Exporting glTF", "Preparing export...", 0.1f);

                using var exporter = new Integration.OMIGltfExporter();
                var success = await exporter.ExportAsync(selection, path);

                EditorUtility.ClearProgressBar();

                if (success)
                {
                    EditorUtility.DisplayDialog(
                        "Export Complete",
                        $"Successfully exported to:\n{path}",
                        "OK"
                    );
                    EditorUtility.RevealInFinder(path);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Export Failed",
                        "Export failed. Check the console for details.",
                        "OK"
                    );
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[OMI Export] Error: {ex}");
                EditorUtility.DisplayDialog("Export Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Exports the selected GameObjects to a GLB (binary glTF) file.
        /// </summary>
        [MenuItem(MenuRoot + "Export Selected to GLB...", false, 101)]
        public static async void ExportSelectedToGlb()
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                EditorUtility.DisplayDialog("Export Error", "Please select one or more GameObjects to export.", "OK");
                return;
            }

            var defaultName = selection[0].name + ".glb";
            var path = EditorUtility.SaveFilePanel(
                "Export GLB with OMI Extensions",
                GetLastExportDirectory(),
                defaultName,
                "glb"
            );

            if (string.IsNullOrEmpty(path)) return;

            SaveLastExportDirectory(Path.GetDirectoryName(path));

            try
            {
                EditorUtility.DisplayProgressBar("Exporting GLB", "Preparing export...", 0.1f);

                using var exporter = new Integration.OMIGltfExporter();
                var success = await exporter.ExportAsync(selection, path);

                EditorUtility.ClearProgressBar();

                if (success)
                {
                    EditorUtility.DisplayDialog(
                        "Export Complete",
                        $"Successfully exported to:\n{path}",
                        "OK"
                    );
                    EditorUtility.RevealInFinder(path);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Export Failed",
                        "Export failed. Check the console for details.",
                        "OK"
                    );
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[OMI Export] Error: {ex}");
                EditorUtility.DisplayDialog("Export Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Validates that the Export Selected menu item should be enabled.
        /// </summary>
        [MenuItem(MenuRoot + "Export Selected to glTF...", true)]
        [MenuItem(MenuRoot + "Export Selected to GLB...", true)]
        public static bool ValidateExportSelected()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        /// <summary>
        /// Exports the entire active scene to glTF.
        /// </summary>
        [MenuItem(MenuRoot + "Export Active Scene to glTF...", false, 200)]
        public static async void ExportActiveSceneToGltf()
        {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .Where(go => go.activeInHierarchy)
                .ToArray();

            if (rootObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Export Error", "No active GameObjects in the scene.", "OK");
                return;
            }

            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var defaultName = sceneName + ".gltf";
            var path = EditorUtility.SaveFilePanel(
                "Export Scene to glTF with OMI Extensions",
                GetLastExportDirectory(),
                defaultName,
                "gltf"
            );

            if (string.IsNullOrEmpty(path)) return;

            SaveLastExportDirectory(Path.GetDirectoryName(path));

            try
            {
                EditorUtility.DisplayProgressBar("Exporting Scene", "Preparing export...", 0.1f);

                using var exporter = new Integration.OMIGltfExporter();
                var success = await exporter.ExportAsync(rootObjects, path);

                EditorUtility.ClearProgressBar();

                if (success)
                {
                    EditorUtility.DisplayDialog(
                        "Export Complete",
                        $"Successfully exported to:\n{path}",
                        "OK"
                    );
                    EditorUtility.RevealInFinder(path);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Export Failed",
                        "Export failed. Check the console for details.",
                        "OK"
                    );
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[OMI Export] Error: {ex}");
                EditorUtility.DisplayDialog("Export Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Opens the OMI Export Settings in the Inspector.
        /// </summary>
        [MenuItem(MenuRoot + "Export Settings...", false, 300)]
        public static void OpenExportSettings()
        {
            // Find or create the settings asset
            var settings = FindOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorUtility.FocusProjectWindow();
            }
        }

        /// <summary>
        /// Adds an OMIGltfExporterComponent to the selected GameObject.
        /// </summary>
        [MenuItem("GameObject/OMI/Add Export Component", false, 10)]
        public static void AddExportComponentToSelected()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject.", "OK");
                return;
            }

            if (selected.GetComponent<Integration.OMIGltfExporterComponent>() == null)
            {
                Undo.AddComponent<Integration.OMIGltfExporterComponent>(selected);
            }
        }

        [MenuItem("GameObject/OMI/Add Export Component", true)]
        public static bool ValidateAddExportComponent()
        {
            return Selection.activeGameObject != null;
        }

        #region Helper Methods

        private const string LastExportDirectoryKey = "OMI_LastExportDirectory";

        private static string GetLastExportDirectory()
        {
            var dir = EditorPrefs.GetString(LastExportDirectoryKey, Application.dataPath);
            if (!Directory.Exists(dir))
            {
                dir = Application.dataPath;
            }
            return dir;
        }

        private static void SaveLastExportDirectory(string directory)
        {
            if (!string.IsNullOrEmpty(directory))
            {
                EditorPrefs.SetString(LastExportDirectoryKey, directory);
            }
        }

        private static OMISettings FindOrCreateSettings()
        {
            // First, try to find an existing settings asset
            var guids = AssetDatabase.FindAssets("t:OMISettings");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var settings = AssetDatabase.LoadAssetAtPath<OMISettings>(path);
                if (settings != null)
                {
                    return settings;
                }
            }

            // If not found, create one
            var settingsPath = "Assets/OMISettings.asset";
            var newSettings = ScriptableObject.CreateInstance<OMISettings>();
            AssetDatabase.CreateAsset(newSettings, settingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[OMI] Created settings asset at {settingsPath}");
            return newSettings;
        }

        #endregion
    }

    /// <summary>
    /// Editor window for advanced OMI export options.
    /// </summary>
    public class OMIExportWindow : EditorWindow
    {
        private OMIExportSettings _exportSettings;
        private Vector2 _scrollPosition;
        private GameObject[] _selectedObjects;

        [MenuItem("OMI/Export/Export Window...", false, 50)]
        public static void ShowWindow()
        {
            var window = GetWindow<OMIExportWindow>("OMI Export");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            _exportSettings = new OMIExportSettings();
            RefreshSelection();
        }

        private void OnSelectionChange()
        {
            RefreshSelection();
            Repaint();
        }

        private void RefreshSelection()
        {
            _selectedObjects = Selection.gameObjects;
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("OMI glTF Export", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Selection info
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
            if (_selectedObjects != null && _selectedObjects.Length > 0)
            {
                EditorGUILayout.LabelField($"Selected: {_selectedObjects.Length} object(s)");
                foreach (var obj in _selectedObjects)
                {
                    EditorGUILayout.LabelField($"  • {obj.name}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select GameObjects to export.", MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Physics Extensions", EditorStyles.boldLabel);
            _exportSettings.ExportPhysicsShapes = EditorGUILayout.Toggle("Export Physics Shapes", _exportSettings.ExportPhysicsShapes);
            _exportSettings.ExportPhysicsBodies = EditorGUILayout.Toggle("Export Physics Bodies", _exportSettings.ExportPhysicsBodies);
            _exportSettings.ExportPhysicsJoints = EditorGUILayout.Toggle("Export Physics Joints", _exportSettings.ExportPhysicsJoints);
            _exportSettings.ExportPhysicsGravity = EditorGUILayout.Toggle("Export Physics Gravity", _exportSettings.ExportPhysicsGravity);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Interaction Extensions", EditorStyles.boldLabel);
            _exportSettings.ExportSpawnPoints = EditorGUILayout.Toggle("Export Spawn Points", _exportSettings.ExportSpawnPoints);
            _exportSettings.ExportSeats = EditorGUILayout.Toggle("Export Seats", _exportSettings.ExportSeats);
            _exportSettings.ExportLinks = EditorGUILayout.Toggle("Export Links", _exportSettings.ExportLinks);
            _exportSettings.ExportPersonality = EditorGUILayout.Toggle("Export Personality", _exportSettings.ExportPersonality);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Extensions", EditorStyles.boldLabel);
            _exportSettings.ExportAudioEmitters = EditorGUILayout.Toggle("Export Audio Emitters", _exportSettings.ExportAudioEmitters);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Vehicle Extensions", EditorStyles.boldLabel);
            _exportSettings.ExportVehicleBodies = EditorGUILayout.Toggle("Export Vehicle Bodies", _exportSettings.ExportVehicleBodies);
            _exportSettings.ExportVehicleWheels = EditorGUILayout.Toggle("Export Vehicle Wheels", _exportSettings.ExportVehicleWheels);
            _exportSettings.ExportVehicleThrusters = EditorGUILayout.Toggle("Export Vehicle Thrusters", _exportSettings.ExportVehicleThrusters);
            _exportSettings.ExportVehicleHoverThrusters = EditorGUILayout.Toggle("Export Hover Thrusters", _exportSettings.ExportVehicleHoverThrusters);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            _exportSettings.MarkExtensionsRequired = EditorGUILayout.Toggle("Mark Extensions Required", _exportSettings.MarkExtensionsRequired);
            _exportSettings.VerboseLogging = EditorGUILayout.Toggle("Verbose Logging", _exportSettings.VerboseLogging);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUI.enabled = _selectedObjects != null && _selectedObjects.Length > 0;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export to glTF...", GUILayout.Height(30)))
            {
                ExportToGltf();
            }
            if (GUILayout.Button("Export to GLB...", GUILayout.Height(30)))
            {
                ExportToGlb();
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private async void ExportToGltf()
        {
            var defaultName = _selectedObjects[0].name + ".gltf";
            var path = EditorUtility.SaveFilePanel(
                "Export glTF with OMI Extensions",
                Application.dataPath,
                defaultName,
                "gltf"
            );

            if (string.IsNullOrEmpty(path)) return;

            await DoExport(path);
        }

        private async void ExportToGlb()
        {
            var defaultName = _selectedObjects[0].name + ".glb";
            var path = EditorUtility.SaveFilePanel(
                "Export GLB with OMI Extensions",
                Application.dataPath,
                defaultName,
                "glb"
            );

            if (string.IsNullOrEmpty(path)) return;

            await DoExport(path);
        }

        private async System.Threading.Tasks.Task DoExport(string path)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Exporting", "Preparing export...", 0.1f);

                using var exporter = new Integration.OMIGltfExporter(settings: _exportSettings);
                var success = await exporter.ExportAsync(_selectedObjects, path);

                EditorUtility.ClearProgressBar();

                if (success)
                {
                    EditorUtility.DisplayDialog(
                        "Export Complete",
                        $"Successfully exported to:\n{path}",
                        "OK"
                    );
                    EditorUtility.RevealInFinder(path);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Export Failed",
                        "Export failed. Check the console for details.",
                        "OK"
                    );
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[OMI Export] Error: {ex}");
                EditorUtility.DisplayDialog("Export Error", ex.Message, "OK");
            }
        }
    }
}
#endif
