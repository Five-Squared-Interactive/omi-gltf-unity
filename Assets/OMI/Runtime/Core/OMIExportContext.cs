// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using GLTFast.Export;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Context object passed to extension handlers during glTF export.
    /// Provides access to Unity objects and the glTF being built.
    /// </summary>
    public class OMIExportContext
    {
        /// <summary>
        /// The glTFast writer instance for building the glTF.
        /// </summary>
        public GltfWriter GltfWriter { get; }

        /// <summary>
        /// The root GameObjects being exported.
        /// </summary>
        public IReadOnlyList<GameObject> RootObjects => _rootObjects;
        private readonly List<GameObject> _rootObjects = new List<GameObject>();

        /// <summary>
        /// Mapping from Unity GameObjects to glTF node indices.
        /// </summary>
        public IReadOnlyDictionary<GameObject, int> GameObjectToNode => _gameObjectToNode;
        private readonly Dictionary<GameObject, int> _gameObjectToNode = new Dictionary<GameObject, int>();

        /// <summary>
        /// Mapping from Unity Meshes to glTF mesh indices.
        /// </summary>
        public IReadOnlyDictionary<Mesh, int> UnityMeshToMesh => _unityMeshToMesh;
        private readonly Dictionary<Mesh, int> _unityMeshToMesh = new Dictionary<Mesh, int>();

        /// <summary>
        /// Extension manager instance for accessing other handlers.
        /// </summary>
        public OMIExtensionManager ExtensionManager { get; }

        /// <summary>
        /// Custom data storage for sharing information between handlers.
        /// </summary>
        public Dictionary<string, object> CustomData { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Settings for the export operation.
        /// </summary>
        public OMIExportSettings Settings { get; }

        public OMIExportContext(
            GltfWriter gltfWriter,
            OMIExtensionManager extensionManager,
            OMIExportSettings settings = null)
        {
            GltfWriter = gltfWriter;
            ExtensionManager = extensionManager ?? throw new ArgumentNullException(nameof(extensionManager));
            Settings = settings ?? new OMIExportSettings();
        }

        /// <summary>
        /// Creates an export context without a glTF writer (for pre-export processing).
        /// </summary>
        public OMIExportContext(
            OMIExtensionManager extensionManager,
            OMIExportSettings settings = null)
        {
            GltfWriter = null;
            ExtensionManager = extensionManager ?? throw new ArgumentNullException(nameof(extensionManager));
            Settings = settings ?? new OMIExportSettings();
        }

        /// <summary>
        /// Adds a root GameObject to the export.
        /// </summary>
        public void AddRootObject(GameObject rootObject)
        {
            if (rootObject != null && !_rootObjects.Contains(rootObject))
            {
                _rootObjects.Add(rootObject);
            }
        }

        /// <summary>
        /// Registers a mapping from a Unity GameObject to a glTF node index.
        /// </summary>
        public void RegisterNode(GameObject gameObject, int nodeIndex)
        {
            _gameObjectToNode[gameObject] = nodeIndex;
        }

        /// <summary>
        /// Gets the glTF node index for a Unity GameObject.
        /// </summary>
        public int GetNodeIndex(GameObject gameObject)
        {
            return _gameObjectToNode.TryGetValue(gameObject, out var index) ? index : -1;
        }

        /// <summary>
        /// Registers a mapping from a Unity Mesh to a glTF mesh index.
        /// </summary>
        public void RegisterMesh(Mesh mesh, int meshIndex)
        {
            _unityMeshToMesh[mesh] = meshIndex;
        }

        /// <summary>
        /// Gets the glTF mesh index for a Unity Mesh.
        /// </summary>
        public int GetMeshIndex(Mesh mesh)
        {
            return _unityMeshToMesh.TryGetValue(mesh, out var index) ? index : -1;
        }

        /// <summary>
        /// Gets or creates typed custom data.
        /// </summary>
        public T GetOrCreateCustomData<T>(string key) where T : class, new()
        {
            if (!CustomData.TryGetValue(key, out var data))
            {
                data = new T();
                CustomData[key] = data;
            }
            return (T)data;
        }

        /// <summary>
        /// Gets typed custom data if it exists.
        /// </summary>
        public bool TryGetCustomData<T>(string key, out T data) where T : class
        {
            if (CustomData.TryGetValue(key, out var obj) && obj is T typed)
            {
                data = typed;
                return true;
            }
            data = null;
            return false;
        }
    }
}
