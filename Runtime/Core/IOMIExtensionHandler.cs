// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Base interface for handling OMI GLTF extension data during import and export.
    /// 
    /// The default handlers (in DefaultHandlers/) directly manipulate Unity GameObjects.
    /// If you have your own framework wrapping Unity (e.g., WebVerse), implement custom
    /// handlers that receive the parsed extension data and apply it to your own objects.
    /// 
    /// Register custom handlers via OMIExtensionManager to override default behavior.
    /// </summary>
    /// <typeparam name="TData">The data class representing the extension's JSON structure.</typeparam>
    public interface IOMIExtensionHandler<TData> where TData : class
    {
        /// <summary>
        /// Called during import to process extension data.
        /// </summary>
        /// <param name="data">The deserialized extension data (POCO).</param>
        /// <param name="context">Import context providing access to glTF data and node mappings.</param>
        /// <param name="cancellationToken">Cancellation token for async operations.</param>
        /// <returns>Task that completes when import handling is done.</returns>
        Task OnImportAsync(TData data, OMIImportContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called during export to extract extension data from the scene.
        /// </summary>
        /// <param name="context">Export context providing access to the scene and glTF being built.</param>
        /// <param name="cancellationToken">Cancellation token for async operations.</param>
        /// <returns>The extension data to serialize, or null if no data should be exported.</returns>
        Task<TData> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the name of the glTF extension this handler processes (e.g., "OMI_physics_shape").
        /// </summary>
        string ExtensionName { get; }

        /// <summary>
        /// Gets the priority of this handler. Higher priority handlers are processed first.
        /// </summary>
        int Priority { get; }
    }

    /// <summary>
    /// Interface for handlers that process node-level extension data.
    /// 
    /// Custom implementations can ignore the GameObject parameter and use the nodeIndex
    /// to look up their own wrapped objects via the context's CustomData dictionary.
    /// </summary>
    /// <typeparam name="TData">The data class representing the node extension's JSON structure.</typeparam>
    public interface IOMINodeExtensionHandler<TData> : IOMIExtensionHandler<TData> where TData : class
    {
        /// <summary>
        /// Called during import to process node extension data.
        /// </summary>
        /// <param name="data">The deserialized extension data (POCO).</param>
        /// <param name="nodeIndex">The glTF node index - use this to correlate with your own objects.</param>
        /// <param name="targetObject">The Unity GameObject for this node (may be ignored by custom handlers).</param>
        /// <param name="context">Import context. Use CustomData to store/retrieve your own object mappings.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task OnNodeImportAsync(TData data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called during export to extract node extension data.
        /// </summary>
        /// <param name="sourceObject">The Unity GameObject being exported (may be ignored by custom handlers).</param>
        /// <param name="context">Export context. Use CustomData to access your own object mappings.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The extension data to serialize for this node, or null if none.</returns>
        Task<TData> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for handlers that process document-level extension data.
    /// </summary>
    /// <typeparam name="TData">The data class representing the document extension's JSON structure.</typeparam>
    public interface IOMIDocumentExtensionHandler<TData> : IOMIExtensionHandler<TData> where TData : class
    {
        /// <summary>
        /// Called during import after document-level extension data is parsed but before nodes are processed.
        /// Use this to cache shared data (shapes, materials, audio sources, etc.) that nodes reference by index.
        /// </summary>
        /// <param name="data">The deserialized document extension data (POCO).</param>
        /// <param name="context">Import context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task OnDocumentImportAsync(TData data, OMIImportContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called during export to build document-level extension data.
        /// </summary>
        /// <param name="context">Export context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The extension data to serialize at document level, or null if none.</returns>
        Task<TData> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default);
    }
}
