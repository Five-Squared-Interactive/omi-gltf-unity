// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.PhysicsBody
{
    /// <summary>
    /// Interface for handling OMI_physics_body data at document level.
    /// </summary>
    public interface IPhysicsBodyDocumentHandler : IOMIDocumentExtensionHandler<OMIPhysicsBodyRoot>
    {
        /// <summary>
        /// Creates a Unity PhysicMaterial from OMI physics material data.
        /// </summary>
        PhysicsMaterial CreatePhysicMaterial(OMIPhysicsMaterial material, int materialIndex, OMIImportContext context);

        /// <summary>
        /// Extracts OMI physics material from a Unity PhysicMaterial.
        /// </summary>
        OMIPhysicsMaterial ExtractPhysicMaterial(PhysicsMaterial material, OMIExportContext context);
    }

    /// <summary>
    /// Interface for handling OMI_physics_body node data.
    /// Implement this to customize how physics bodies are created in Unity.
    /// </summary>
    public interface IPhysicsBodyHandler : IOMINodeExtensionHandler<OMIPhysicsBodyNode>
    {
        /// <summary>
        /// Creates a Rigidbody from motion data.
        /// </summary>
        /// <param name="motion">The motion data.</param>
        /// <param name="targetObject">The GameObject to add the Rigidbody to.</param>
        /// <param name="context">Import context.</param>
        /// <returns>The created Rigidbody.</returns>
        Rigidbody CreateRigidbody(OMIPhysicsBodyMotion motion, GameObject targetObject, OMIImportContext context);

        /// <summary>
        /// Configures a collider from collider data.
        /// </summary>
        /// <param name="colliderData">The collider data.</param>
        /// <param name="targetObject">The GameObject with the collider.</param>
        /// <param name="context">Import context.</param>
        void ConfigureCollider(OMIPhysicsBodyCollider colliderData, GameObject targetObject, OMIImportContext context);

        /// <summary>
        /// Configures a trigger from trigger data.
        /// </summary>
        /// <param name="triggerData">The trigger data.</param>
        /// <param name="targetObject">The GameObject with the trigger.</param>
        /// <param name="context">Import context.</param>
        void ConfigureTrigger(OMIPhysicsBodyTrigger triggerData, GameObject targetObject, OMIImportContext context);

        /// <summary>
        /// Extracts motion data from a Rigidbody.
        /// </summary>
        OMIPhysicsBodyMotion ExtractMotion(Rigidbody rigidbody, OMIExportContext context);

        /// <summary>
        /// Extracts collider data from a Collider.
        /// </summary>
        OMIPhysicsBodyCollider ExtractCollider(Collider collider, OMIExportContext context);

        /// <summary>
        /// Extracts trigger data from a trigger Collider.
        /// </summary>
        OMIPhysicsBodyTrigger ExtractTrigger(Collider collider, OMIExportContext context);
    }
}
