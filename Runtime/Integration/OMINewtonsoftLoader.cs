// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json.Linq;
using UnityEngine;
using OMI.Extensions.Link;
using OMI.Extensions.PhysicsBody;
using OMI.Extensions.PhysicsGravity;
using OMI.Extensions.PhysicsJoint;
using OMI.Extensions.PhysicsShape;
using OMI.Extensions.Seat;
using OMI.Extensions.SpawnPoint;
using OMI.Extensions.Personality;

namespace OMI.Integration
{
    /// <summary>
    /// High-level API for loading glTF files with OMI extensions.
    /// Uses direct Newtonsoft.Json parsing for extension data - works with any glTFast version.
    /// </summary>
    public class OMIDirectJsonLoader : IDisposable
    {
        private GltfImport _gltfImport;
        private OMIExtensionManager _extensionManager;
        private OMIImportSettings _settings;
        private OMIImportContext _context;
        private JObject _root;
        private byte[] _rawData;
        private string _sourceUrl;

        /// <summary>
        /// Gets the glTFast import instance.
        /// </summary>
        public GltfImport GltfImport => _gltfImport;

        /// <summary>
        /// Gets the extension manager.
        /// </summary>
        public OMIExtensionManager ExtensionManager => _extensionManager;

        /// <summary>
        /// Gets the import context.
        /// </summary>
        public OMIImportContext Context => _context;

        /// <summary>
        /// Gets the parsed glTF root JSON with all extension data preserved.
        /// </summary>
        public JObject Root => _root;

        /// <summary>
        /// Creates a new OMI Direct JSON loader.
        /// </summary>
        /// <param name="extensionManager">Custom extension manager, or null for defaults.</param>
        /// <param name="settings">Import settings, or null for defaults.</param>
        public OMIDirectJsonLoader(OMIExtensionManager extensionManager = null, OMIImportSettings settings = null)
        {
            _extensionManager = extensionManager ?? OMIExtensionManager.CreateWithDefaults();
            _settings = settings ?? new OMIImportSettings();
            _gltfImport = new GltfImport();
        }

        /// <summary>
        /// Loads a glTF file from a URL.
        /// </summary>
        /// <param name="url">URL to the glTF file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if loading succeeded.</returns>
        public async Task<bool> LoadAsync(string url, CancellationToken cancellationToken = default)
        {
            _sourceUrl = url;
            
            // First, load the raw data to parse extensions
            try
            {
                if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = url.Substring(7);
                    _rawData = File.ReadAllBytes(filePath);
                }
                else if (File.Exists(url))
                {
                    _rawData = File.ReadAllBytes(url);
                }
                else
                {
                    using (var client = new HttpClient())
                    {
                        _rawData = await client.GetByteArrayAsync(url);
                    }
                }

                _root = OMIJsonExtensions.ParseGltfData(_rawData);
                if (_root == null)
                {
                    Debug.LogError("[OMI] Failed to parse glTF JSON");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OMI] Failed to load glTF data: {ex.Message}");
                return false;
            }

            // Now load with glTFast for mesh/material processing
            var success = await _gltfImport.Load(url, cancellationToken: cancellationToken);
            
            if (success)
            {
                _context = new OMIImportContext(_gltfImport, _extensionManager, _settings);
            }

            return success;
        }

        /// <summary>
        /// Loads a glTF file from a byte array.
        /// </summary>
        /// <param name="data">The glTF data.</param>
        /// <param name="uri">Optional base URI for external resources.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if loading succeeded.</returns>
        public async Task<bool> LoadAsync(byte[] data, Uri uri = null, CancellationToken cancellationToken = default)
        {
            _rawData = data;
            _sourceUrl = uri?.ToString();

            // Parse the JSON for extensions
            _root = OMIJsonExtensions.ParseGltfData(data);
            if (_root == null)
            {
                Debug.LogError("[OMI] Failed to parse glTF JSON");
                return false;
            }

            // Load with glTFast for mesh/material processing
            var success = await _gltfImport.Load(data, uri, cancellationToken: cancellationToken);
            
            if (success)
            {
                _context = new OMIImportContext(_gltfImport, _extensionManager, _settings);
            }

            return success;
        }

        /// <summary>
        /// Instantiates the main scene with OMI extensions processed.
        /// </summary>
        /// <param name="parent">Parent transform for the instantiated scene.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The root GameObject of the instantiated scene.</returns>
        public async Task<GameObject> InstantiateAsync(Transform parent = null, CancellationToken cancellationToken = default)
        {
            if (_gltfImport == null || !_gltfImport.LoadingDone)
            {
                Debug.LogError("[OMI] glTF not loaded. Call LoadAsync first.");
                return null;
            }

            // Instantiate using glTFast
            var success = await _gltfImport.InstantiateMainSceneAsync(parent, cancellationToken);
            
            if (!success)
            {
                Debug.LogError("[OMI] Failed to instantiate glTF scene");
                return null;
            }

            // Find the root object
            GameObject rootObject = null;
            if (parent != null && parent.childCount > 0)
            {
                rootObject = parent.GetChild(parent.childCount - 1).gameObject;
            }

            if (rootObject == null)
            {
                Debug.LogError("[OMI] Could not find instantiated root object");
                return null;
            }

            _context.RootObject = rootObject;

            // Build node mapping using optimized traversal
            _context.BuildNodeMapping(rootObject);

            // Process OMI extensions from the parsed JSON
            await ProcessOMIExtensionsAsync(cancellationToken);

            return rootObject;
        }

        /// <summary>
        /// Processes OMI extensions from the parsed glTF JSON.
        /// </summary>
        private async Task ProcessOMIExtensionsAsync(CancellationToken cancellationToken)
        {
            if (_root == null)
            {
                Debug.LogWarning("[OMI] No glTF root available for extension processing");
                return;
            }

            if (_settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Processing OMI extensions from parsed glTF JSON");
            }

            // Process document-level extensions
            await ProcessDocumentExtensionsAsync(cancellationToken);

            // Process node-level extensions
            await ProcessNodeExtensionsAsync(cancellationToken);
        }

        private async Task ProcessDocumentExtensionsAsync(CancellationToken cancellationToken)
        {
            // Process OMI_physics_shape document extension
            if (_settings.ImportPhysicsShapes)
            {
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsShapeRoot>(_root, OMIJsonExtensions.ExtensionNames.PhysicsShape, out var shapesRoot))
                {
                    var handler = _extensionManager.GetHandler<OMIPhysicsShapeRoot>(OMIJsonExtensions.ExtensionNames.PhysicsShape);
                    if (handler != null)
                    {
                        await handler.OnImportAsync(shapesRoot, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_physics_body document extension (materials, filters)
            if (_settings.ImportPhysicsBodies)
            {
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsBodyRoot>(_root, OMIJsonExtensions.ExtensionNames.PhysicsBody, out var bodyRoot))
                {
                    var handler = _extensionManager.GetHandler<OMIPhysicsBodyRoot>(OMIJsonExtensions.ExtensionNames.PhysicsBody);
                    if (handler != null)
                    {
                        await handler.OnImportAsync(bodyRoot, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_physics_joint document extension
            if (_settings.ImportPhysicsJoints)
            {
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsJointRoot>(_root, OMIJsonExtensions.ExtensionNames.PhysicsJoint, out var jointRoot))
                {
                    var handler = _extensionManager.GetHandler<OMIPhysicsJointRoot>(OMIJsonExtensions.ExtensionNames.PhysicsJoint);
                    if (handler != null)
                    {
                        await handler.OnImportAsync(jointRoot, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_physics_gravity document extension (world gravity)
            if (_settings.ImportPhysicsGravity)
            {
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsGravityRoot>(_root, OMIJsonExtensions.ExtensionNames.PhysicsGravity, out var gravityRoot))
                {
                    var handler = _extensionManager.GetHandler<OMIPhysicsGravityRoot>(OMIJsonExtensions.ExtensionNames.PhysicsGravity);
                    if (handler != null)
                    {
                        await handler.OnImportAsync(gravityRoot, _context, cancellationToken);
                    }
                }
            }
        }

        private async Task ProcessNodeExtensionsAsync(CancellationToken cancellationToken)
        {
            var nodes = OMIJsonExtensions.GetNodes(_root);
            if (nodes == null) return;

            for (int i = 0; i < nodes.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var node = nodes[i] as JObject;
                if (node == null) continue;

                var gameObject = _context.GetGameObject(i);
                
                if (gameObject == null)
                {
                    if (_settings.VerboseLogging)
                    {
                        Debug.LogWarning($"[OMI] No GameObject for node {i}");
                    }
                    continue;
                }

                await ProcessNodeExtensionAsync(node, i, gameObject, cancellationToken);
            }
        }

        private async Task ProcessNodeExtensionAsync(JObject node, int nodeIndex, GameObject gameObject, CancellationToken cancellationToken)
        {
            // Process OMI_physics_body node extension
            if (_settings.ImportPhysicsBodies)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPhysicsBodyNode>(node, OMIJsonExtensions.ExtensionNames.PhysicsBody, out var bodyNode))
                {
                    var handler = _extensionManager.GetHandler<OMIPhysicsBodyNode>(OMIJsonExtensions.ExtensionNames.PhysicsBody) as IOMINodeExtensionHandler<OMIPhysicsBodyNode>;
                    if (handler != null)
                    {
                        await handler.OnNodeImportAsync(bodyNode, nodeIndex, gameObject, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_physics_joint node extension
            if (_settings.ImportPhysicsJoints)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPhysicsJointNode>(node, OMIJsonExtensions.ExtensionNames.PhysicsJoint, out var jointNode))
                {
                    var handler = _extensionManager.GetHandler<OMIPhysicsJointNode>(OMIJsonExtensions.ExtensionNames.PhysicsJoint) as IOMINodeExtensionHandler<OMIPhysicsJointNode>;
                    if (handler != null)
                    {
                        await handler.OnNodeImportAsync(jointNode, nodeIndex, gameObject, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_physics_gravity node extension
            if (_settings.ImportPhysicsGravity)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPhysicsGravityNode>(node, OMIJsonExtensions.ExtensionNames.PhysicsGravity, out var gravityNode))
                {
                    var handler = _extensionManager.GetHandler<OMIPhysicsGravityNode>(OMIJsonExtensions.ExtensionNames.PhysicsGravity) as IOMINodeExtensionHandler<OMIPhysicsGravityNode>;
                    if (handler != null)
                    {
                        await handler.OnNodeImportAsync(gravityNode, nodeIndex, gameObject, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_spawn_point node extension
            if (_settings.ImportSpawnPoints)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMISpawnPointNode>(node, OMIJsonExtensions.ExtensionNames.SpawnPoint, out var spawnNode))
                {
                    var handler = _extensionManager.GetHandler<OMISpawnPointNode>(OMIJsonExtensions.ExtensionNames.SpawnPoint) as IOMINodeExtensionHandler<OMISpawnPointNode>;
                    if (handler != null)
                    {
                        await handler.OnNodeImportAsync(spawnNode, nodeIndex, gameObject, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_seat node extension
            if (_settings.ImportSeats)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMISeatNode>(node, OMIJsonExtensions.ExtensionNames.Seat, out var seatNode))
                {
                    var handler = _extensionManager.GetHandler<OMISeatNode>(OMIJsonExtensions.ExtensionNames.Seat) as IOMINodeExtensionHandler<OMISeatNode>;
                    if (handler != null)
                    {
                        await handler.OnNodeImportAsync(seatNode, nodeIndex, gameObject, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_link node extension
            if (_settings.ImportLinks)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMILinkNode>(node, OMIJsonExtensions.ExtensionNames.Link, out var linkNode))
                {
                    var handler = _extensionManager.GetHandler<OMILinkNode>(OMIJsonExtensions.ExtensionNames.Link) as IOMINodeExtensionHandler<OMILinkNode>;
                    if (handler != null)
                    {
                        await handler.OnNodeImportAsync(linkNode, nodeIndex, gameObject, _context, cancellationToken);
                    }
                }
            }

            // Process OMI_personality node extension
            if (_settings.ImportPersonality)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPersonalityNode>(node, OMIJsonExtensions.ExtensionNames.Personality, out var personalityNode))
                {
                    var handler = _extensionManager.GetHandler<OMIPersonalityNode>(OMIJsonExtensions.ExtensionNames.Personality) as IOMINodeExtensionHandler<OMIPersonalityNode>;
                    if (handler != null)
                    {
                        await handler.OnNodeImportAsync(personalityNode, nodeIndex, gameObject, _context, cancellationToken);
                    }
                }
            }
        }

        public void Dispose()
        {
            _gltfImport?.Dispose();
            _gltfImport = null;
            _context = null;
            _root = null;
            _rawData = null;
        }
    }

    /// <summary>
    /// Backwards compatibility alias for OMINewtonsoftLoader.
    /// Use OMIDirectJsonLoader instead.
    /// </summary>
    [Obsolete("Use OMIDirectJsonLoader instead. This class is provided for backwards compatibility.")]
    public class OMINewtonsoftLoader : OMIDirectJsonLoader
    {
        public OMINewtonsoftLoader(OMIExtensionManager extensionManager = null, OMIImportSettings settings = null)
            : base(extensionManager, settings)
        {
        }
    }
}
