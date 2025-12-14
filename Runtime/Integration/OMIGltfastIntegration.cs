// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Addons;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace OMI.Integration
{
    /// <summary>
    /// glTFast import addon for OMI extensions.
    /// This addon hooks into glTFast's import pipeline to process OMI extension data.
    /// </summary>
    public class OMIImportAddon : ImportAddon<OMIImportAddonInstance>
    {
        /// <summary>
        /// The extension manager to use for processing.
        /// </summary>
        public OMIExtensionManager ExtensionManager { get; set; }

        /// <summary>
        /// Import settings for OMI extensions.
        /// </summary>
        public OMIImportSettings Settings { get; set; }
    }

    /// <summary>
    /// glTFast import addon instance for OMI extensions.
    /// </summary>
    public class OMIImportAddonInstance : ImportAddonInstance
    {
        private GltfImportBase _gltfImport;
        private OMIExtensionManager _extensionManager;
        private OMIImportSettings _settings;
        private OMIImportContext _context;

        // Supported OMI extension names
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>
        {
            "OMI_physics_shape",
            "OMI_physics_body",
            "OMI_physics_joint",
            "OMI_physics_gravity",
            "OMI_spawn_point",
            "OMI_seat",
            "OMI_link",
            "OMI_personality",
            "KHR_audio_emitter",
            "OMI_audio_ogg_vorbis",
            "OMI_audio_opus"
        };

        public override bool SupportsGltfExtension(string extensionName)
        {
            return SupportedExtensions.Contains(extensionName);
        }

        public override void Inject(GltfImportBase gltfImport)
        {
            _gltfImport = gltfImport;
            
            // Get configuration from the addon if available
            // For now, create defaults
            _extensionManager = OMIExtensionManager.CreateWithDefaults();
            _settings = new OMIImportSettings();
            
            _context = new OMIImportContext(gltfImport, _extensionManager, _settings);
            
            gltfImport.AddImportAddonInstance(this);
        }

        public override void Inject(IInstantiator instantiator)
        {
            if (instantiator is GameObjectInstantiator goInstantiator)
            {
                var omiInstantiator = new OMIInstantiatorDecorator(goInstantiator, _context);
                // Note: glTFast doesn't allow replacing the instantiator, so we need to
                // process nodes after instantiation completes
            }
        }

        public override void Dispose()
        {
            _gltfImport = null;
            _extensionManager = null;
            _context = null;
        }

        /// <summary>
        /// Gets the import context for external access.
        /// </summary>
        public OMIImportContext GetContext() => _context;

        /// <summary>
        /// Gets the extension manager for external access.
        /// </summary>
        public OMIExtensionManager GetExtensionManager() => _extensionManager;
    }

    /// <summary>
    /// Decorator that wraps the glTFast instantiator to intercept node creation.
    /// </summary>
    public class OMIInstantiatorDecorator
    {
        private readonly GameObjectInstantiator _inner;
        private readonly OMIImportContext _context;

        public OMIInstantiatorDecorator(GameObjectInstantiator inner, OMIImportContext context)
        {
            _inner = inner;
            _context = context;
        }

        // This class would be used to intercept node creation
        // However, glTFast's current architecture doesn't easily support this
        // Instead, we process extensions after import using OMIPostProcessor
    }

    /// <summary>
    /// Helper class to process OMI extensions after glTFast import completes.
    /// </summary>
    public static class OMIPostProcessor
    {
        /// <summary>
        /// Processes OMI extensions on an imported glTF.
        /// Call this after glTFast's InstantiateMainSceneAsync completes.
        /// </summary>
        /// <param name="gltfImport">The glTFast import instance.</param>
        /// <param name="rootObject">The root GameObject of the imported scene.</param>
        /// <param name="extensionManager">The extension manager to use (or null for defaults).</param>
        /// <param name="settings">Import settings (or null for defaults).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task ProcessAsync(
            GltfImportBase gltfImport,
            GameObject rootObject,
            OMIExtensionManager extensionManager = null,
            OMIImportSettings settings = null,
            CancellationToken cancellationToken = default)
        {
            extensionManager ??= OMIExtensionManager.CreateWithDefaults();
            settings ??= new OMIImportSettings();

            var context = new OMIImportContext(gltfImport, extensionManager, settings);
            context.RootObject = rootObject;

            // Build node index to GameObject mapping
            BuildNodeMapping(rootObject, context);

            // Process the import using the JSON data from glTFast
            // Note: This requires access to the raw JSON which may not be available
            // depending on glTFast version

            await ProcessDocumentExtensionsAsync(context, cancellationToken);
            await ProcessNodeExtensionsAsync(context, cancellationToken);

            if (settings.VerboseLogging)
            {
                Debug.Log("[OMI] Post-processing complete");
            }
        }

        private static void BuildNodeMapping(GameObject root, OMIImportContext context)
        {
            // Walk the hierarchy and build mapping
            // This is a simplified approach - in practice you'd need the actual node indices
            int index = 0;
            BuildNodeMappingRecursive(root, context, ref index);
        }

        private static void BuildNodeMappingRecursive(GameObject obj, OMIImportContext context, ref int index)
        {
            context.RegisterNode(index++, obj);
            
            foreach (Transform child in obj.transform)
            {
                BuildNodeMappingRecursive(child.gameObject, context, ref index);
            }
        }

        private static async Task ProcessDocumentExtensionsAsync(OMIImportContext context, CancellationToken cancellationToken)
        {
            // Process document-level extensions
            foreach (var handler in context.ExtensionManager.GetDocumentHandlers())
            {
                if (cancellationToken.IsCancellationRequested) return;
                
                // Document extension processing would happen here
                // This requires access to the glTF JSON which we'd need to extract from glTFast
            }
        }

        private static async Task ProcessNodeExtensionsAsync(OMIImportContext context, CancellationToken cancellationToken)
        {
            // Process node-level extensions
            foreach (var handler in context.ExtensionManager.GetNodeHandlers())
            {
                if (cancellationToken.IsCancellationRequested) return;
                
                // Node extension processing would happen here
                // This requires access to the glTF JSON which we'd need to extract from glTFast
            }
        }
    }
}
