// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Export;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace OMI.Integration
{
    /// <summary>
    /// High-level API for exporting Unity scenes to glTF with OMI extensions.
    /// This class orchestrates the export pipeline, using glTFast for base export
    /// and injecting OMI extension data as a post-processing step.
    /// </summary>
    public class OMIGltfExporter : IDisposable
    {
        private OMIExtensionManager _extensionManager;
        private OMIExportSettings _settings;
        private OMIExportContext _context;

        // Collected extension data during export
        private readonly Dictionary<string, object> _documentExtensions = new Dictionary<string, object>();
        private readonly Dictionary<int, Dictionary<string, object>> _nodeExtensions = new Dictionary<int, Dictionary<string, object>>();
        private readonly HashSet<string> _extensionsUsed = new HashSet<string>();
        private readonly HashSet<string> _extensionsRequired = new HashSet<string>();

        /// <summary>
        /// Gets the export context.
        /// </summary>
        public OMIExportContext Context => _context;

        /// <summary>
        /// Gets the extension manager.
        /// </summary>
        public OMIExtensionManager ExtensionManager => _extensionManager;

        /// <summary>
        /// Creates a new OMI glTF exporter.
        /// </summary>
        /// <param name="extensionManager">Custom extension manager, or null for defaults.</param>
        /// <param name="settings">Export settings, or null for defaults.</param>
        public OMIGltfExporter(OMIExtensionManager extensionManager = null, OMIExportSettings settings = null)
        {
            _extensionManager = extensionManager ?? OMIExtensionManager.CreateWithDefaults();
            _settings = settings ?? new OMIExportSettings();
            _context = new OMIExportContext(_extensionManager, _settings);
        }

        /// <summary>
        /// Exports GameObjects to a glTF file with OMI extensions.
        /// </summary>
        /// <param name="rootObjects">Root GameObjects to export.</param>
        /// <param name="filePath">Destination file path (.gltf or .glb).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if export succeeded.</returns>
        public async Task<bool> ExportAsync(
            GameObject[] rootObjects,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            if (rootObjects == null || rootObjects.Length == 0)
            {
                Debug.LogError("[OMI Export] No objects to export");
                return false;
            }

            try
            {
                // Phase 1: Pre-export - build node mappings and collect extension data
                await PreExportAsync(rootObjects, cancellationToken);

                // Phase 2: Base export using glTFast
                var tempPath = GetTempPath(filePath);
                var baseExportSuccess = await ExportBaseGltfAsync(rootObjects, tempPath, cancellationToken);

                if (!baseExportSuccess)
                {
                    Debug.LogError("[OMI Export] Base glTF export failed");
                    return false;
                }

                // Phase 3: Inject OMI extensions into the glTF JSON
                var injectSuccess = await InjectExtensionsAsync(tempPath, filePath, cancellationToken);

                // Clean up temp file
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                // Also clean up .bin file if it exists
                var tempBinPath = Path.ChangeExtension(tempPath, ".bin");
                if (File.Exists(tempBinPath))
                {
                    var finalBinPath = Path.ChangeExtension(filePath, ".bin");
                    if (tempBinPath != finalBinPath)
                    {
                        if (File.Exists(finalBinPath))
                        {
                            File.Delete(finalBinPath);
                        }
                        File.Move(tempBinPath, finalBinPath);
                    }
                }

                if (_settings.VerboseLogging)
                {
                    Debug.Log($"[OMI Export] Successfully exported to {filePath}");
                }

                return injectSuccess;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OMI Export] Export failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Exports a single GameObject hierarchy.
        /// </summary>
        public Task<bool> ExportAsync(
            GameObject rootObject,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return ExportAsync(new[] { rootObject }, filePath, cancellationToken);
        }

        #region Pre-Export Phase

        private async Task PreExportAsync(GameObject[] rootObjects, CancellationToken cancellationToken)
        {
            // Clear previous export data
            _documentExtensions.Clear();
            _nodeExtensions.Clear();
            _extensionsUsed.Clear();
            _extensionsRequired.Clear();

            foreach (var rootObject in rootObjects)
            {
                _context.AddRootObject(rootObject);
            }

            // Build node mappings by traversing the hierarchy
            int nodeIndex = 0;
            foreach (var rootObject in rootObjects)
            {
                BuildNodeMappingRecursive(rootObject, ref nodeIndex);
            }

            // Collect node-level extension data
            await CollectNodeExtensionsAsync(cancellationToken);

            // Collect document-level extension data
            await CollectDocumentExtensionsAsync(cancellationToken);

            if (_settings.VerboseLogging)
            {
                Debug.Log($"[OMI Export] Pre-export complete. " +
                    $"Nodes: {_context.GameObjectToNode.Count}, " +
                    $"Document extensions: {_documentExtensions.Count}, " +
                    $"Extensions used: {string.Join(", ", _extensionsUsed)}");
            }
        }

        private void BuildNodeMappingRecursive(GameObject obj, ref int index)
        {
            _context.RegisterNode(obj, index++);

            foreach (Transform child in obj.transform)
            {
                BuildNodeMappingRecursive(child.gameObject, ref index);
            }
        }

        private async Task CollectNodeExtensionsAsync(CancellationToken cancellationToken)
        {
            foreach (var kvp in _context.GameObjectToNode)
            {
                var gameObject = kvp.Key;
                var nodeIndex = kvp.Value;

                foreach (var handler in _extensionManager.GetNodeHandlers())
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    var extensionName = GetExtensionName(handler);
                    if (string.IsNullOrEmpty(extensionName)) continue;
                    if (!IsExtensionEnabled(extensionName)) continue;

                    try
                    {
                        var data = await InvokeNodeExportAsync(handler, gameObject, cancellationToken);
                        if (data != null)
                        {
                            AddNodeExtension(nodeIndex, extensionName, data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[OMI Export] Error in {extensionName} node export for {gameObject.name}: {ex.Message}");
                    }
                }
            }
        }

        private async Task CollectDocumentExtensionsAsync(CancellationToken cancellationToken)
        {
            foreach (var handler in _extensionManager.GetDocumentHandlers())
            {
                if (cancellationToken.IsCancellationRequested) return;

                var extensionName = GetExtensionName(handler);
                if (string.IsNullOrEmpty(extensionName)) continue;
                if (!IsExtensionEnabled(extensionName)) continue;

                try
                {
                    var data = await InvokeDocumentExportAsync(handler, cancellationToken);
                    if (data != null)
                    {
                        AddDocumentExtension(extensionName, data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[OMI Export] Error in {extensionName} document export: {ex.Message}");
                }
            }
        }

        private string GetExtensionName(object handler)
        {
            var type = handler.GetType();
            var property = type.GetProperty("ExtensionName");
            return property?.GetValue(handler) as string;
        }

        private async Task<object> InvokeNodeExportAsync(object handler, GameObject gameObject, CancellationToken cancellationToken)
        {
            // Find the OnNodeExportAsync method via reflection
            var type = handler.GetType();
            var method = type.GetMethod("OnNodeExportAsync", new[] { typeof(GameObject), typeof(OMIExportContext), typeof(CancellationToken) });
            
            if (method == null) return null;

            var task = method.Invoke(handler, new object[] { gameObject, _context, cancellationToken });
            if (task is Task taskObj)
            {
                await taskObj.ConfigureAwait(false);
                // Get the result from the Task<T>
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }
            return null;
        }

        private async Task<object> InvokeDocumentExportAsync(object handler, CancellationToken cancellationToken)
        {
            // Find the OnDocumentExportAsync method via reflection
            var type = handler.GetType();
            var method = type.GetMethod("OnDocumentExportAsync", new[] { typeof(OMIExportContext), typeof(CancellationToken) });
            
            if (method == null) return null;

            var task = method.Invoke(handler, new object[] { _context, cancellationToken });
            if (task is Task taskObj)
            {
                await taskObj.ConfigureAwait(false);
                // Get the result from the Task<T>
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }
            return null;
        }

        private bool IsExtensionEnabled(string extensionName)
        {
            return extensionName switch
            {
                "OMI_physics_shape" => _settings.ExportPhysicsShapes,
                "OMI_physics_body" => _settings.ExportPhysicsBodies,
                "OMI_physics_joint" => _settings.ExportPhysicsJoints,
                "OMI_physics_gravity" => _settings.ExportPhysicsGravity,
                "OMI_spawn_point" => _settings.ExportSpawnPoints,
                "OMI_seat" => _settings.ExportSeats,
                "OMI_link" => _settings.ExportLinks,
                "OMI_personality" => _settings.ExportPersonality,
                "KHR_audio_emitter" => _settings.ExportAudioEmitters,
                "OMI_vehicle_body" => _settings.ExportVehicleBodies,
                "OMI_vehicle_wheel" => _settings.ExportVehicleWheels,
                "OMI_vehicle_thruster" => _settings.ExportVehicleThrusters,
                "OMI_vehicle_hover_thruster" => _settings.ExportVehicleHoverThrusters,
                _ => true
            };
        }

        private void AddNodeExtension(int nodeIndex, string extensionName, object data)
        {
            if (!_nodeExtensions.TryGetValue(nodeIndex, out var extensions))
            {
                extensions = new Dictionary<string, object>();
                _nodeExtensions[nodeIndex] = extensions;
            }

            extensions[extensionName] = data;
            _extensionsUsed.Add(extensionName);

            if (_settings.MarkExtensionsRequired)
            {
                _extensionsRequired.Add(extensionName);
            }
        }

        private void AddDocumentExtension(string extensionName, object data)
        {
            _documentExtensions[extensionName] = data;
            _extensionsUsed.Add(extensionName);

            if (_settings.MarkExtensionsRequired)
            {
                _extensionsRequired.Add(extensionName);
            }
        }

        #endregion

        #region Base Export Phase

        private async Task<bool> ExportBaseGltfAsync(
            GameObject[] rootObjects,
            string tempPath,
            CancellationToken cancellationToken)
        {
            var exportSettings = new ExportSettings
            {
                Format = Path.GetExtension(tempPath).ToLowerInvariant() == ".glb"
                    ? GltfFormat.Binary
                    : GltfFormat.Json,
                FileConflictResolution = FileConflictResolution.Overwrite,
                ComponentMask = GLTFast.ComponentType.Mesh | GLTFast.ComponentType.Camera | GLTFast.ComponentType.Light
            };

            var gameObjectExportSettings = new GameObjectExportSettings
            {
                OnlyActiveInHierarchy = true,
                DisabledComponents = false
            };

            var export = new GameObjectExport(exportSettings, gameObjectExportSettings);
            
            export.AddScene(rootObjects, "Scene");

            var success = await export.SaveToFileAndDispose(tempPath);

            return success;
        }

        #endregion

        #region Extension Injection Phase

        private async Task<bool> InjectExtensionsAsync(
            string sourcePath,
            string destPath,
            CancellationToken cancellationToken)
        {
            try
            {
                // Read the base glTF JSON
                var jsonContent = await File.ReadAllTextAsync(sourcePath, cancellationToken);
                var gltf = JObject.Parse(jsonContent);

                // Inject document-level extensions
                InjectDocumentExtensions(gltf);

                // Inject node-level extensions
                InjectNodeExtensions(gltf);

                // Update extensionsUsed and extensionsRequired
                InjectExtensionLists(gltf);

                // Write the modified glTF
                var serializer = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };
                
                var outputJson = JsonConvert.SerializeObject(gltf, serializer);
                await File.WriteAllTextAsync(destPath, outputJson, Encoding.UTF8, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OMI Export] Failed to inject extensions: {ex}");
                return false;
            }
        }

        private void InjectDocumentExtensions(JObject gltf)
        {
            if (_documentExtensions.Count == 0) return;

            var extensions = gltf["extensions"] as JObject ?? new JObject();

            foreach (var kvp in _documentExtensions)
            {
                var extensionJson = JToken.FromObject(kvp.Value, CreateSerializer());
                extensions[kvp.Key] = extensionJson;
            }

            gltf["extensions"] = extensions;
        }

        private void InjectNodeExtensions(JObject gltf)
        {
            if (_nodeExtensions.Count == 0) return;

            var nodes = gltf["nodes"] as JArray;
            if (nodes == null) return;

            foreach (var kvp in _nodeExtensions)
            {
                var nodeIndex = kvp.Key;
                var nodeExtensions = kvp.Value;

                if (nodeIndex < 0 || nodeIndex >= nodes.Count) continue;

                var node = nodes[nodeIndex] as JObject;
                if (node == null) continue;

                var extensions = node["extensions"] as JObject ?? new JObject();

                foreach (var extKvp in nodeExtensions)
                {
                    var extensionJson = JToken.FromObject(extKvp.Value, CreateSerializer());
                    extensions[extKvp.Key] = extensionJson;
                }

                node["extensions"] = extensions;
            }
        }

        private void InjectExtensionLists(JObject gltf)
        {
            // Add to extensionsUsed
            if (_extensionsUsed.Count > 0)
            {
                var existingUsed = gltf["extensionsUsed"] as JArray ?? new JArray();
                var existingUsedSet = existingUsed.Select(t => t.ToString()).ToHashSet();

                foreach (var ext in _extensionsUsed)
                {
                    if (!existingUsedSet.Contains(ext))
                    {
                        existingUsed.Add(ext);
                    }
                }

                gltf["extensionsUsed"] = existingUsed;
            }

            // Add to extensionsRequired
            if (_extensionsRequired.Count > 0)
            {
                var existingRequired = gltf["extensionsRequired"] as JArray ?? new JArray();
                var existingRequiredSet = existingRequired.Select(t => t.ToString()).ToHashSet();

                foreach (var ext in _extensionsRequired)
                {
                    if (!existingRequiredSet.Contains(ext))
                    {
                        existingRequired.Add(ext);
                    }
                }

                gltf["extensionsRequired"] = existingRequired;
            }
        }

        private JsonSerializer CreateSerializer()
        {
            return JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });
        }

        private string GetTempPath(string finalPath)
        {
            var directory = Path.GetDirectoryName(finalPath);
            var filename = $"_temp_{Guid.NewGuid():N}{Path.GetExtension(finalPath)}";
            return Path.Combine(directory ?? "", filename);
        }

        #endregion

        public void Dispose()
        {
            _context = null;
            _extensionManager = null;
        }
    }

    /// <summary>
    /// Extension methods for easier export operations.
    /// </summary>
    public static class OMIGltfExporterExtensions
    {
        /// <summary>
        /// Exports a GameObject to glTF with OMI extensions using default settings.
        /// </summary>
        public static Task<bool> ExportOMIGltf(
            this GameObject gameObject,
            string filePath,
            OMIExportSettings settings = null,
            CancellationToken cancellationToken = default)
        {
            using var exporter = new OMIGltfExporter(settings: settings);
            return exporter.ExportAsync(gameObject, filePath, cancellationToken);
        }

        /// <summary>
        /// Exports multiple GameObjects to glTF with OMI extensions using default settings.
        /// </summary>
        public static Task<bool> ExportOMIGltf(
            this GameObject[] gameObjects,
            string filePath,
            OMIExportSettings settings = null,
            CancellationToken cancellationToken = default)
        {
            using var exporter = new OMIGltfExporter(settings: settings);
            return exporter.ExportAsync(gameObjects, filePath, cancellationToken);
        }
    }
}
