// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

namespace OMI.Extensions.PhysicsGravity
{
    /// <summary>
    /// Interface for handlers that process OMI_physics_gravity node-level extension data.
    /// </summary>
    public interface IPhysicsGravityHandler : IOMINodeExtensionHandler<OMIPhysicsGravityNode>
    {
    }

    /// <summary>
    /// Interface for handlers that process OMI_physics_gravity document-level extension data.
    /// </summary>
    public interface IPhysicsGravityDocumentHandler : IOMIDocumentExtensionHandler<OMIPhysicsGravityRoot>
    {
    }
}
