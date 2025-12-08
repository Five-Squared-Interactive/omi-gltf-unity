// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.Seat
{
    /// <summary>
    /// Interface for handling OMI_seat data.
    /// Implement this to customize how seats are represented in your project.
    /// </summary>
    public interface ISeatHandler : IOMINodeExtensionHandler<OMISeatNode>
    {
        /// <summary>
        /// Creates a seat representation on the target object.
        /// </summary>
        /// <param name="data">The seat data.</param>
        /// <param name="targetObject">The GameObject at the seat location.</param>
        /// <param name="context">Import context.</param>
        void CreateSeat(OMISeatNode data, GameObject targetObject, OMIImportContext context);

        /// <summary>
        /// Extracts seat data from a GameObject.
        /// </summary>
        /// <param name="sourceObject">The GameObject to extract from.</param>
        /// <param name="context">Export context.</param>
        /// <returns>The seat data, or null if not a seat.</returns>
        OMISeatNode ExtractSeat(GameObject sourceObject, OMIExportContext context);
    }
}
