// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Integration
{
    /// <summary>
    /// Component for exporting the attached GameObject hierarchy to glTF with OMI extensions.
    /// Attach this to a root object to enable export functionality.
    /// </summary>
    [AddComponentMenu("OMI/glTF Exporter")]
    public class OMIGltfExporterComponent : MonoBehaviour
    {
        [Header("Export Configuration")]
        [Tooltip("Custom name for the exported scene. If empty, uses the GameObject name.")]
        [SerializeField]
        private string sceneName;

        [Tooltip("Include inactive GameObjects in the export.")]
        [SerializeField]
        private bool includeInactive = false;

        [Header("OMI Extension Settings")]
        [Tooltip("Export physics shapes from Colliders.")]
        [SerializeField]
        private bool exportPhysicsShapes = true;

        [Tooltip("Export physics bodies from Rigidbodies.")]
        [SerializeField]
        private bool exportPhysicsBodies = true;

        [Tooltip("Export physics joints from Joints.")]
        [SerializeField]
        private bool exportPhysicsJoints = true;

        [Tooltip("Export audio emitters from AudioSources.")]
        [SerializeField]
        private bool exportAudioEmitters = true;

        [Tooltip("Export spawn points.")]
        [SerializeField]
        private bool exportSpawnPoints = true;

        [Tooltip("Export seats.")]
        [SerializeField]
        private bool exportSeats = true;

        [Tooltip("Export links.")]
        [SerializeField]
        private bool exportLinks = true;

        [Tooltip("Export physics gravity zones.")]
        [SerializeField]
        private bool exportPhysicsGravity = true;

        [Tooltip("Export personality data.")]
        [SerializeField]
        private bool exportPersonality = true;

        [Tooltip("Export vehicle bodies.")]
        [SerializeField]
        private bool exportVehicleBodies = true;

        [Tooltip("Export vehicle wheels.")]
        [SerializeField]
        private bool exportVehicleWheels = true;

        [Tooltip("Export vehicle thrusters.")]
        [SerializeField]
        private bool exportVehicleThrusters = true;

        [Tooltip("Export vehicle hover thrusters.")]
        [SerializeField]
        private bool exportVehicleHoverThrusters = true;

        [Header("Advanced")]
        [Tooltip("Mark OMI extensions as required in the glTF file.")]
        [SerializeField]
        private bool markExtensionsRequired = false;

        [Tooltip("Enable verbose logging during export.")]
        [SerializeField]
        private bool verboseLogging = false;

        /// <summary>
        /// Gets the configured scene name for export.
        /// </summary>
        public string SceneName => string.IsNullOrEmpty(sceneName) ? gameObject.name : sceneName;

        /// <summary>
        /// Gets export settings based on the component configuration.
        /// </summary>
        public OMIExportSettings GetExportSettings()
        {
            return new OMIExportSettings
            {
                ExportPhysicsShapes = exportPhysicsShapes,
                ExportPhysicsBodies = exportPhysicsBodies,
                ExportPhysicsJoints = exportPhysicsJoints,
                ExportAudioEmitters = exportAudioEmitters,
                ExportSpawnPoints = exportSpawnPoints,
                ExportSeats = exportSeats,
                ExportLinks = exportLinks,
                ExportPhysicsGravity = exportPhysicsGravity,
                ExportPersonality = exportPersonality,
                ExportVehicleBodies = exportVehicleBodies,
                ExportVehicleWheels = exportVehicleWheels,
                ExportVehicleThrusters = exportVehicleThrusters,
                ExportVehicleHoverThrusters = exportVehicleHoverThrusters,
                MarkExtensionsRequired = markExtensionsRequired,
                VerboseLogging = verboseLogging
            };
        }

        /// <summary>
        /// Exports this GameObject hierarchy to the specified path.
        /// </summary>
        /// <param name="filePath">Destination file path (.gltf or .glb).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if export succeeded.</returns>
        public async Task<bool> ExportAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var settings = GetExportSettings();
            using var exporter = new OMIGltfExporter(settings: settings);
            return await exporter.ExportAsync(gameObject, filePath, cancellationToken);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Opens a save dialog and exports the hierarchy.
        /// </summary>
        [ContextMenu("Export to glTF...")]
        private void ExportFromContextMenu()
        {
            var defaultName = SceneName + ".gltf";
            var path = UnityEditor.EditorUtility.SaveFilePanel(
                "Export glTF with OMI Extensions",
                Application.dataPath,
                defaultName,
                "gltf"
            );

            if (!string.IsNullOrEmpty(path))
            {
                _ = ExportAndShowResult(path);
            }
        }

        private async Task ExportAndShowResult(string path)
        {
            var success = await ExportAsync(path);
            if (success)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Export Complete",
                    $"Successfully exported to:\n{path}",
                    "OK"
                );
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Export Failed",
                    "Export failed. Check the console for details.",
                    "OK"
                );
            }
        }
#endif
    }

    /// <summary>
    /// Static utility class for quick glTF export operations.
    /// </summary>
    public static class OMIExport
    {
        /// <summary>
        /// Exports GameObjects to a glTF file with OMI extensions.
        /// </summary>
        /// <param name="gameObjects">GameObjects to export.</param>
        /// <param name="filePath">Destination file path.</param>
        /// <param name="settings">Optional export settings.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if export succeeded.</returns>
        public static async Task<bool> ToFileAsync(
            GameObject[] gameObjects,
            string filePath,
            OMIExportSettings settings = null,
            CancellationToken cancellationToken = default)
        {
            using var exporter = new OMIGltfExporter(settings: settings);
            return await exporter.ExportAsync(gameObjects, filePath, cancellationToken);
        }

        /// <summary>
        /// Exports a single GameObject to a glTF file with OMI extensions.
        /// </summary>
        public static Task<bool> ToFileAsync(
            GameObject gameObject,
            string filePath,
            OMIExportSettings settings = null,
            CancellationToken cancellationToken = default)
        {
            return ToFileAsync(new[] { gameObject }, filePath, settings, cancellationToken);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Opens a save dialog and exports the selected GameObjects.
        /// </summary>
        /// <param name="gameObjects">GameObjects to export.</param>
        /// <param name="settings">Optional export settings.</param>
        public static async Task ExportWithDialog(GameObject[] gameObjects, OMIExportSettings settings = null)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("Export Error", "No GameObjects selected.", "OK");
                return;
            }

            var defaultName = gameObjects[0].name + ".gltf";
            var path = UnityEditor.EditorUtility.SaveFilePanel(
                "Export glTF with OMI Extensions",
                Application.dataPath,
                defaultName,
                "gltf"
            );

            if (string.IsNullOrEmpty(path)) return;

            var success = await ToFileAsync(gameObjects, path, settings);
            
            if (success)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Export Complete",
                    $"Successfully exported to:\n{path}",
                    "OK"
                );
                UnityEditor.EditorUtility.RevealInFinder(path);
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Export Failed",
                    "Export failed. Check the console for details.",
                    "OK"
                );
            }
        }
#endif
    }
}
