// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using OMI.Extensions.Link;
using OMI.Extensions.PhysicsBody;
using OMI.Extensions.PhysicsShape;
using OMI.Extensions.Seat;
using OMI.Extensions.SpawnPoint;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Helper class to register default Unity handlers for all OMI extensions.
    /// </summary>
    public static class OMIDefaultHandlers
    {
        /// <summary>
        /// Registers all default handlers with the extension manager.
        /// </summary>
        /// <param name="manager">The extension manager to register with.</param>
        public static void RegisterAll(OMIExtensionManager manager)
        {
            if (manager == null)
            {
                Debug.LogError("[OMI] Cannot register handlers: manager is null");
                return;
            }

            // Physics extensions
            RegisterPhysicsShapeHandler(manager);
            RegisterPhysicsBodyHandler(manager);
            
            // Interaction extensions
            RegisterSpawnPointHandler(manager);
            RegisterSeatHandler(manager);
            RegisterLinkHandler(manager);
        }

        /// <summary>
        /// Registers the default physics shape handler.
        /// </summary>
        public static void RegisterPhysicsShapeHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultPhysicsShapeHandler();
            manager.RegisterHandler<OMIPhysicsShapeRoot>(handler);
        }

        /// <summary>
        /// Registers the default physics body handler.
        /// </summary>
        public static void RegisterPhysicsBodyHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultPhysicsBodyHandler();
            manager.RegisterHandler<OMIPhysicsBodyNode>(handler);
            manager.RegisterHandler<OMIPhysicsBodyRoot>(handler);
        }

        /// <summary>
        /// Registers the default spawn point handler.
        /// </summary>
        public static void RegisterSpawnPointHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultSpawnPointHandler();
            manager.RegisterHandler<OMISpawnPointNode>(handler);
        }

        /// <summary>
        /// Registers the default seat handler.
        /// </summary>
        public static void RegisterSeatHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultSeatHandler();
            manager.RegisterHandler<OMISeatNode>(handler);
        }

        /// <summary>
        /// Registers the default link handler.
        /// </summary>
        public static void RegisterLinkHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultLinkHandler();
            manager.RegisterHandler<OMILinkNode>(handler);
        }

        /// <summary>
        /// Registers a custom handler, replacing any existing handler for that extension.
        /// </summary>
        /// <typeparam name="TData">The extension data type.</typeparam>
        /// <param name="manager">The extension manager.</param>
        /// <param name="handler">The custom handler.</param>
        public static void RegisterCustomHandler<TData>(OMIExtensionManager manager, IOMIExtensionHandler<TData> handler) 
            where TData : class
        {
            if (manager == null || handler == null)
            {
                Debug.LogError("[OMI] Cannot register custom handler: null argument");
                return;
            }

            // Unregister existing handler for this extension
            manager.UnregisterHandler(handler.ExtensionName);
            
            // Register the new handler
            manager.RegisterHandler(handler);
        }
    }
}
