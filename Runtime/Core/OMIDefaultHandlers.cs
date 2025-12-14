// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using OMI.Extensions.Audio;
using OMI.Extensions.EnvironmentSky;
using OMI.Extensions.Link;
using OMI.Extensions.Personality;
using OMI.Extensions.PhysicsBody;
using OMI.Extensions.PhysicsGravity;
using OMI.Extensions.PhysicsJoint;
using OMI.Extensions.PhysicsShape;
using OMI.Extensions.Seat;
using OMI.Extensions.SpawnPoint;
using OMI.Extensions.Vehicle;
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
            RegisterPhysicsJointHandler(manager);
            RegisterPhysicsGravityHandler(manager);
            
            // Interaction extensions
            RegisterSpawnPointHandler(manager);
            RegisterSeatHandler(manager);
            RegisterLinkHandler(manager);
            
            // AI/Character extensions
            RegisterPersonalityHandler(manager);
            
            // Audio extensions
            RegisterAudioEmitterHandler(manager);
            
            // Vehicle extensions
            RegisterVehicleBodyHandler(manager);
            RegisterVehicleWheelHandler(manager);
            RegisterVehicleThrusterHandler(manager);
            RegisterVehicleHoverThrusterHandler(manager);
            
            // Environment extensions
            RegisterEnvironmentSkyHandler(manager);
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
        /// Registers the default physics joint handler.
        /// </summary>
        public static void RegisterPhysicsJointHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultPhysicsJointHandler();
            manager.RegisterHandler<OMIPhysicsJointNode>(handler);
            manager.RegisterHandler<OMIPhysicsJointRoot>(handler);
        }

        /// <summary>
        /// Registers the default physics gravity handler.
        /// </summary>
        public static void RegisterPhysicsGravityHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultPhysicsGravityHandler();
            manager.RegisterHandler<OMIPhysicsGravityNode>(handler);
            manager.RegisterHandler<OMIPhysicsGravityRoot>(handler);
        }

        /// <summary>
        /// Registers the default personality handler.
        /// </summary>
        public static void RegisterPersonalityHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultPersonalityHandler();
            manager.RegisterHandler<OMIPersonalityNode>(handler);
        }

        /// <summary>
        /// Registers the default audio emitter handler.
        /// </summary>
        public static void RegisterAudioEmitterHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultAudioEmitterHandler();
            manager.RegisterHandler<KHRAudioEmitterRoot>(handler);
            manager.RegisterHandler<KHRAudioEmitterNode>(handler);
        }

        /// <summary>
        /// Registers the default vehicle body handler.
        /// </summary>
        public static void RegisterVehicleBodyHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultVehicleBodyHandler();
            manager.RegisterHandler<OMIVehicleBodyNode>(handler);
        }

        /// <summary>
        /// Registers the default vehicle wheel handler.
        /// </summary>
        public static void RegisterVehicleWheelHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultVehicleWheelHandler();
            manager.RegisterHandler<OMIVehicleWheelRoot>(handler);
            manager.RegisterHandler<OMIVehicleWheelNode>(handler);
        }

        /// <summary>
        /// Registers the default vehicle thruster handler.
        /// </summary>
        public static void RegisterVehicleThrusterHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultVehicleThrusterHandler();
            manager.RegisterHandler<OMIVehicleThrusterRoot>(handler);
            manager.RegisterHandler<OMIVehicleThrusterNode>(handler);
        }

        /// <summary>
        /// Registers the default vehicle hover thruster handler.
        /// </summary>
        public static void RegisterVehicleHoverThrusterHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultVehicleHoverThrusterHandler();
            manager.RegisterHandler<OMIVehicleHoverThrusterRoot>(handler);
            manager.RegisterHandler<OMIVehicleHoverThrusterNode>(handler);
        }

        /// <summary>
        /// Registers the default environment sky handler.
        /// </summary>
        public static void RegisterEnvironmentSkyHandler(OMIExtensionManager manager)
        {
            var handler = new DefaultEnvironmentSkyHandler();
            manager.RegisterHandler<OMIEnvironmentSkySkyData>(handler);
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
