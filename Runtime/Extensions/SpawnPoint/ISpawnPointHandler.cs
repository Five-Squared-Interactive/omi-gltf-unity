// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.SpawnPoint
{
    /// <summary>
    /// Interface for handling OMI_spawn_point data.
    /// Implement this to customize how spawn points are represented in your project.
    /// </summary>
    public interface ISpawnPointHandler : IOMINodeExtensionHandler<OMISpawnPointNode>
    {
        /// <summary>
        /// Creates a spawn point representation on the target object.
        /// </summary>
        /// <param name="data">The spawn point data.</param>
        /// <param name="targetObject">The GameObject at the spawn location.</param>
        /// <param name="context">Import context.</param>
        void CreateSpawnPoint(OMISpawnPointNode data, GameObject targetObject, OMIImportContext context);

        /// <summary>
        /// Extracts spawn point data from a GameObject.
        /// </summary>
        /// <param name="sourceObject">The GameObject to extract from.</param>
        /// <param name="context">Export context.</param>
        /// <returns>The spawn point data, or null if not a spawn point.</returns>
        OMISpawnPointNode ExtractSpawnPoint(GameObject sourceObject, OMIExportContext context);
    }
}
