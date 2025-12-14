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
    /// 
    /// Custom handlers can use the CustomData dictionary to store their own
    /// object mappings (e.g., nodeIndex -> YourWrappedObject) and ignore
    /// the Unity-specific properties if they have their own abstraction layer.
    /// </summary>
    public class OMIImportContext : IDisposable
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
        /// Custom handlers may ignore this and use CustomData for their own mappings.
        /// </summary>
        public IReadOnlyDictionary<int, GameObject> NodeToGameObject => _nodeToGameObject;
        private readonly Dictionary<int, GameObject> _nodeToGameObject = new Dictionary<int, GameObject>(64);

        /// <summary>
        /// Mapping from glTF mesh indices to Unity Meshes.
        /// </summary>
        public IReadOnlyDictionary<int, Mesh> MeshToUnityMesh => _meshToUnityMesh;
        private readonly Dictionary<int, Mesh> _meshToUnityMesh = new Dictionary<int, Mesh>(32);

        /// <summary>
        /// Extension manager instance for accessing other handlers.
        /// </summary>
        public OMIExtensionManager ExtensionManager { get; }

        /// <summary>
        /// Custom data storage for sharing information between handlers.
        /// Use this to store your own object mappings if you have a custom framework.
        /// Example: CustomData["MyFramework_NodeMap"] = new Dictionary&lt;int, MyWrappedObject&gt;();
        /// </summary>
        public Dictionary<string, object> CustomData { get; } = new Dictionary<string, object>(16);

        /// <summary>
        /// Logger for reporting warnings and errors.
        /// </summary>
        public ICodeLogger Logger { get; }

        /// <summary>
        /// Settings for the import operation.
        /// </summary>
        public OMIImportSettings Settings { get; }

        /// <summary>
        /// Component cache for avoiding repeated GetComponent calls.
        /// Use this when processing multiple extensions on the same GameObjects.
        /// </summary>
        public OMIComponentCache ComponentCache { get; }

        /// <summary>
        /// Base path for loading external resources (e.g., audio files).
        /// </summary>
        public string BasePath { get; set; }

        private bool _disposed;

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
            ComponentCache = new OMIComponentCache();
        }

        /// <summary>
        /// Builds the node mapping from a root GameObject using optimized traversal.
        /// </summary>
        public void BuildNodeMapping(GameObject root)
        {
            if (root == null) return;
            RootObject = root;
            OMIHierarchyUtility.BuildNodeMapping(root, _nodeToGameObject);
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

        /// <summary>
        /// Sets extension data that can be accessed by other handlers.
        /// </summary>
        public void SetExtensionData<T>(string extensionName, T data) where T : class
        {
            CustomData[$"ExtData_{extensionName}"] = data;
        }

        /// <summary>
        /// Gets extension data set by another handler.
        /// </summary>
        public T GetExtensionData<T>(string extensionName) where T : class
        {
            var key = $"ExtData_{extensionName}";
            return CustomData.TryGetValue(key, out var data) ? data as T : null;
        }

        /// <summary>
        /// Gets a component using the cached lookup.
        /// </summary>
        public T GetComponentCached<T>(GameObject gameObject) where T : Component
        {
            return ComponentCache.GetComponent<T>(gameObject);
        }

        /// <summary>
        /// Gets or adds a component using the cached lookup.
        /// </summary>
        public T GetOrAddComponentCached<T>(GameObject gameObject) where T : Component
        {
            return ComponentCache.GetOrAddComponent<T>(gameObject);
        }

        #region Deferred Actions

        private List<Action> _deferredActions = new List<Action>();

        /// <summary>
        /// Registers an action to be executed after all nodes have been processed.
        /// Use this for resolving cross-references between nodes.
        /// </summary>
        public void RegisterDeferredAction(Action action)
        {
            if (action != null)
            {
                _deferredActions.Add(action);
            }
        }

        /// <summary>
        /// Executes all registered deferred actions.
        /// Called by the import pipeline after all nodes are processed.
        /// </summary>
        public void ExecuteDeferredActions()
        {
            foreach (var action in _deferredActions)
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Logger?.Error($"Error executing deferred action: {ex.Message}");
                    Debug.LogException(ex);
                }
            }
            _deferredActions.Clear();
        }

        /// <summary>
        /// Gets the GameObject for a node index. Alias for GetGameObject.
        /// </summary>
        public GameObject GetNodeByIndex(int nodeIndex)
        {
            return GetGameObject(nodeIndex);
        }

        #endregion

        /// <summary>
        /// Disposes the context and releases cached resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                ComponentCache?.Dispose();
                _nodeToGameObject.Clear();
                _meshToUnityMesh.Clear();
                CustomData.Clear();
                _deferredActions.Clear();
                _disposed = true;
            }
        }
    }
}
