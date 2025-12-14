// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Base interface for handling OMI GLTF extension data during import and export.
    /// Implementations can customize how extension data maps to Unity objects or custom frameworks.
    /// </summary>
    /// <typeparam name="TData">The data class representing the extension's JSON structure.</typeparam>
    public interface IOMIExtensionHandler<TData> where TData : class
    {
        /// <summary>
        /// Called during import to process extension data and apply it to the target.
        /// </summary>
        /// <param name="data">The deserialized extension data.</param>
        /// <param name="context">Import context providing access to the glTF and Unity objects.</param>
        /// <param name="cancellationToken">Cancellation token for async operations.</param>
        /// <returns>Task that completes when import handling is done.</returns>
        Task OnImportAsync(TData data, OMIImportContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called during export to extract extension data from the source.
        /// </summary>
        /// <param name="context">Export context providing access to Unity objects and the glTF being built.</param>
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
    /// </summary>
    /// <typeparam name="TData">The data class representing the node extension's JSON structure.</typeparam>
    public interface IOMINodeExtensionHandler<TData> : IOMIExtensionHandler<TData> where TData : class
    {
        /// <summary>
        /// Called during import to process node extension data.
        /// </summary>
        /// <param name="data">The deserialized extension data.</param>
        /// <param name="nodeIndex">The glTF node index.</param>
        /// <param name="targetObject">The Unity GameObject created for this node.</param>
        /// <param name="context">Import context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task OnNodeImportAsync(TData data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called during export to extract node extension data.
        /// </summary>
        /// <param name="sourceObject">The Unity GameObject being exported.</param>
        /// <param name="context">Export context.</param>
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
        /// </summary>
        /// <param name="data">The deserialized document extension data.</param>
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
