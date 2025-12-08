// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using GLTFast;
using GLTFast.Logging;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Context object passed to extension handlers during glTF import.
    /// Provides access to the glTF data, Unity objects, and shared resources.
    /// </summary>
    public class OMIImportContext
    {
        /// <summary>
        /// The glTFast import instance.
        /// </summary>
        public GltfImportBase GltfImport { get; }

        /// <summary>
        /// The root GameObject of the imported scene.
        /// </summary>
        public GameObject RootObject { get; set; }

        /// <summary>
        /// Mapping from glTF node indices to Unity GameObjects.
        /// </summary>
        public IReadOnlyDictionary<int, GameObject> NodeToGameObject => _nodeToGameObject;
        private readonly Dictionary<int, GameObject> _nodeToGameObject = new Dictionary<int, GameObject>();

        /// <summary>
        /// Mapping from glTF mesh indices to Unity Meshes.
        /// </summary>
        public IReadOnlyDictionary<int, Mesh> MeshToUnityMesh => _meshToUnityMesh;
        private readonly Dictionary<int, Mesh> _meshToUnityMesh = new Dictionary<int, Mesh>();

        /// <summary>
        /// Extension manager instance for accessing other handlers.
        /// </summary>
        public OMIExtensionManager ExtensionManager { get; }

        /// <summary>
        /// Custom data storage for sharing information between handlers.
        /// </summary>
        public Dictionary<string, object> CustomData { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Logger for reporting warnings and errors.
        /// </summary>
        public ICodeLogger Logger { get; }

        /// <summary>
        /// Settings for the import operation.
        /// </summary>
        public OMIImportSettings Settings { get; }

        public OMIImportContext(
            GltfImportBase gltfImport,
            OMIExtensionManager extensionManager,
            OMIImportSettings settings = null,
            ICodeLogger logger = null)
        {
            GltfImport = gltfImport ?? throw new ArgumentNullException(nameof(gltfImport));
            ExtensionManager = extensionManager ?? throw new ArgumentNullException(nameof(extensionManager));
            Settings = settings ?? new OMIImportSettings();
            Logger = logger;
        }

        /// <summary>
        /// Registers a mapping from a glTF node index to a Unity GameObject.
        /// </summary>
        public void RegisterNode(int nodeIndex, GameObject gameObject)
        {
            _nodeToGameObject[nodeIndex] = gameObject;
        }

        /// <summary>
        /// Gets the GameObject for a glTF node index.
        /// </summary>
        public GameObject GetGameObject(int nodeIndex)
        {
            return _nodeToGameObject.TryGetValue(nodeIndex, out var go) ? go : null;
        }

        /// <summary>
        /// Registers a mapping from a glTF mesh index to a Unity Mesh.
        /// </summary>
        public void RegisterMesh(int meshIndex, Mesh mesh)
        {
            _meshToUnityMesh[meshIndex] = mesh;
        }

        /// <summary>
        /// Gets the Unity Mesh for a glTF mesh index.
        /// </summary>
        public Mesh GetMesh(int meshIndex)
        {
            return _meshToUnityMesh.TryGetValue(meshIndex, out var mesh) ? mesh : null;
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
