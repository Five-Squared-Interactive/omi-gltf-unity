// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.Link
{
    /// <summary>
    /// Interface for handling OMI_link data.
    /// Implement this to customize how links/portals are represented in your project.
    /// </summary>
    public interface ILinkHandler : IOMINodeExtensionHandler<OMILinkNode>
    {
        /// <summary>
        /// Creates a link representation on the target object.
        /// </summary>
        /// <param name="data">The link data.</param>
        /// <param name="targetObject">The GameObject at the link location.</param>
        /// <param name="context">Import context.</param>
        void CreateLink(OMILinkNode data, GameObject targetObject, OMIImportContext context);

        /// <summary>
        /// Extracts link data from a GameObject.
        /// </summary>
        /// <param name="sourceObject">The GameObject to extract from.</param>
        /// <param name="context">Export context.</param>
        /// <returns>The link data, or null if not a link.</returns>
        OMILinkNode ExtractLink(GameObject sourceObject, OMIExportContext context);
    }
}
