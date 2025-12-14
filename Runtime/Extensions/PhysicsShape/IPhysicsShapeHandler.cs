// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.PhysicsShape
{
    /// <summary>
    /// Interface for handling OMI_physics_shape data.
    /// Implement this to customize how physics shapes are created in Unity.
    /// </summary>
    public interface IPhysicsShapeHandler : IOMIDocumentExtensionHandler<OMIPhysicsShapeRoot>
    {
        /// <summary>
        /// Creates a Unity collider from an OMI physics shape.
        /// </summary>
        /// <param name="shape">The shape data.</param>
        /// <param name="shapeIndex">The index of the shape in the shapes array.</param>
        /// <param name="targetObject">The GameObject to add the collider to.</param>
        /// <param name="context">Import context.</param>
        /// <returns>The created collider, or null if creation failed.</returns>
        Collider CreateCollider(OMIPhysicsShape shape, int shapeIndex, GameObject targetObject, OMIImportContext context);

        /// <summary>
        /// Extracts an OMI physics shape from a Unity collider.
        /// </summary>
        /// <param name="collider">The Unity collider.</param>
        /// <param name="context">Export context.</param>
        /// <returns>The extracted shape data, or null if not applicable.</returns>
        OMIPhysicsShape ExtractShape(Collider collider, OMIExportContext context);

        /// <summary>
        /// Gets the index of a shape, registering it if necessary.
        /// </summary>
        /// <param name="shape">The shape to get/register.</param>
        /// <param name="context">Export context.</param>
        /// <returns>The shape index.</returns>
        int GetOrRegisterShape(OMIPhysicsShape shape, OMIExportContext context);
    }
}
