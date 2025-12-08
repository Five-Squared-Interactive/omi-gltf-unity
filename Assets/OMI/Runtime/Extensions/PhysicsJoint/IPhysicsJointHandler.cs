// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.PhysicsJoint
{
    /// <summary>
    /// Interface for handling OMI_physics_joint document-level data.
    /// </summary>
    public interface IPhysicsJointDocumentHandler : IOMIDocumentExtensionHandler<OMIPhysicsJointRoot>
    {
        /// <summary>
        /// Gets the index of a joint settings, registering it if necessary.
        /// </summary>
        int GetOrRegisterJointSettings(OMIPhysicsJointSettings settings, OMIExportContext context);
    }

    /// <summary>
    /// Interface for handling OMI_physics_joint node data.
    /// Implement this to customize how physics joints are created in Unity.
    /// </summary>
    public interface IPhysicsJointHandler : IOMINodeExtensionHandler<OMIPhysicsJointNode>
    {
        /// <summary>
        /// Creates a Unity Joint from OMI joint data.
        /// </summary>
        /// <param name="jointNode">The joint node data.</param>
        /// <param name="jointSettings">The joint settings.</param>
        /// <param name="targetObject">The GameObject to add the joint to.</param>
        /// <param name="connectedObject">The GameObject this joint connects to.</param>
        /// <param name="context">Import context.</param>
        /// <returns>The created joint, or null if creation failed.</returns>
        Joint CreateJoint(
            OMIPhysicsJointNode jointNode,
            OMIPhysicsJointSettings jointSettings,
            GameObject targetObject,
            GameObject connectedObject,
            OMIImportContext context);

        /// <summary>
        /// Extracts OMI joint data from a Unity Joint.
        /// </summary>
        /// <param name="joint">The Unity joint.</param>
        /// <param name="context">Export context.</param>
        /// <returns>The joint node data, or null if not applicable.</returns>
        OMIPhysicsJointNode ExtractJoint(Joint joint, OMIExportContext context);

        /// <summary>
        /// Extracts joint settings from a Unity Joint.
        /// </summary>
        OMIPhysicsJointSettings ExtractJointSettings(Joint joint, OMIExportContext context);
    }
}
