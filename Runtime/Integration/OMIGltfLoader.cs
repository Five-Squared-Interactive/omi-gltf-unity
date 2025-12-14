// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json.Linq;
using OMI.Extensions.Link;
using OMI.Extensions.PhysicsBody;
using OMI.Extensions.PhysicsShape;
using OMI.Extensions.Seat;
using OMI.Extensions.SpawnPoint;
using UnityEngine;

namespace OMI.Integration
{
    /// <summary>
    /// High-level API for loading glTF files with OMI extensions.
    /// </summary>
    public class OMIGltfLoader : IDisposable
    {
        private GltfImport _gltfImport;
        private OMIExtensionManager _extensionManager;
        private OMIImportSettings _settings;
        private OMIImportContext _context;
        private JObject _rawJson;

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
        /// Creates a new OMI glTF loader.
        /// </summary>
        /// <param name="extensionManager">Custom extension manager, or null for defaults.</param>
        /// <param name="settings">Import settings, or null for defaults.</param>
        public OMIGltfLoader(OMIExtensionManager extensionManager = null, OMIImportSettings settings = null)
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

            // Process OMI extensions
            await ProcessOMIExtensionsAsync(rootObject, cancellationToken);

            return rootObject;
        }

        /// <summary>
        /// Processes OMI extensions on the instantiated scene.
        /// </summary>
        private async Task ProcessOMIExtensionsAsync(GameObject rootObject, CancellationToken cancellationToken)
        {
            // Build node mapping using optimized traversal
            _context.BuildNodeMapping(rootObject);

            // Try to get the raw JSON from glTFast (this may not be available in all versions)
            // For now, we'll work with what we have

            if (_settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Processing OMI extensions on {_context.NodeToGameObject.Count} nodes");
            }

            // Process any OMI components that were added during import
            await ProcessExistingOMIComponentsAsync(rootObject, cancellationToken);
        }

        private async Task ProcessExistingOMIComponentsAsync(GameObject root, CancellationToken cancellationToken)
        {
            // This processes any OMI components that might have been added by other means
            // (e.g., if the scene was set up manually or by another importer)
            // Reuse lists to minimize allocations
            var spawnPoints = new List<OMISpawnPoint>(8);
            var seats = new List<OMISeat>(8);
            var links = new List<OMILink>(8);

            OMIHierarchyUtility.GetComponentsInChildren(root.transform, spawnPoints, true);
            OMIHierarchyUtility.GetComponentsInChildren(root.transform, seats, true);
            OMIHierarchyUtility.GetComponentsInChildren(root.transform, links, true);

            if (_settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Found {spawnPoints.Count} spawn points, {seats.Count} seats, {links.Count} links");
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
            _gltfImport?.Dispose();
            _gltfImport = null;
            _context = null;
        }
    }

    /// <summary>
    /// Component for loading glTF files with OMI extensions at runtime.
    /// </summary>
    [AddComponentMenu("OMI/glTF Loader")]
    public class OMIGltfLoaderComponent : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("URL or file path to the glTF file.")]
        public string Url;

        [Tooltip("Load the glTF when this component starts.")]
        public bool LoadOnStart = true;

        [Header("Settings")]
        [Tooltip("Import settings for OMI extensions.")]
        public OMIImportSettings ImportSettings;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent<GameObject> OnLoaded;
        public UnityEngine.Events.UnityEvent<string> OnError;

        private OMIGltfLoader _loader;
        private GameObject _loadedRoot;

        /// <summary>
        /// Gets the loaded root GameObject.
        /// </summary>
        public GameObject LoadedRoot => _loadedRoot;

        /// <summary>
        /// Gets the extension manager.
        /// </summary>
        public OMIExtensionManager ExtensionManager => _loader?.ExtensionManager;

        private async void Start()
        {
            if (LoadOnStart && !string.IsNullOrEmpty(Url))
            {
                await LoadAsync();
            }
        }

        /// <summary>
        /// Loads the glTF from the configured URL.
        /// </summary>
        public async Task<GameObject> LoadAsync()
        {
            return await LoadAsync(Url);
        }

        /// <summary>
        /// Loads a glTF from the specified URL.
        /// </summary>
        /// <param name="url">URL to load from.</param>
        public async Task<GameObject> LoadAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                var error = "No URL specified";
                Debug.LogError($"[OMI] {error}");
                OnError?.Invoke(error);
                return null;
            }

            try
            {
                // Clean up previous load
                if (_loadedRoot != null)
                {
                    Destroy(_loadedRoot);
                    _loadedRoot = null;
                }

                _loader?.Dispose();
                _loader = new OMIGltfLoader(settings: ImportSettings);

                var success = await _loader.LoadAsync(url);
                if (!success)
                {
                    var error = "Failed to load glTF";
                    Debug.LogError($"[OMI] {error}: {url}");
                    OnError?.Invoke(error);
                    return null;
                }

                _loadedRoot = await _loader.InstantiateAsync(transform);
                
                if (_loadedRoot != null)
                {
                    OnLoaded?.Invoke(_loadedRoot);
                }

                return _loadedRoot;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                OnError?.Invoke(ex.Message);
                return null;
            }
        }

        private void OnDestroy()
        {
            _loader?.Dispose();
        }
    }
}
