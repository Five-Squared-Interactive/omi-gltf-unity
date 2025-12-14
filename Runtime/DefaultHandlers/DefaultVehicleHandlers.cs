// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.Vehicle
{
    /// <summary>
    /// Default handler for OMI_vehicle_body extension.
    /// Creates OMIVehicleBody components from glTF data.
    /// </summary>
    public class DefaultVehicleBodyHandler : IOMINodeExtensionHandler<OMIVehicleBodyNode>
    {
        /// <summary>
        /// Extension name for OMI_vehicle_body.
        /// </summary>
        public const string ExtensionNameConst = "OMI_vehicle_body";

        /// <inheritdoc/>
        public string ExtensionName => ExtensionNameConst;

        /// <inheritdoc/>
        public int Priority => 25; // Process after physics body but before wheels/thrusters

        /// <inheritdoc/>
        public Task OnImportAsync(OMIVehicleBodyNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            // Node extension - import happens via OnNodeImportAsync
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleBodyNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Node extension - export happens via OnNodeExportAsync
            return Task.FromResult<OMIVehicleBodyNode>(null);
        }

        /// <inheritdoc/>
        public Task OnNodeImportAsync(OMIVehicleBodyNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            ProcessNodeExtensionAsync(data, targetObject, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleBodyNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportNodeExtension(sourceObject, context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> ProcessNodeExtensionAsync(OMIVehicleBodyNode data, GameObject target, OMIImportContext context)
        {
            if (data == null || target == null)
            {
                return Task.FromResult(false);
            }

            // Ensure Rigidbody exists (should be created by physics body handler)
            var rigidbody = target.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = target.AddComponent<Rigidbody>();
                Debug.LogWarning($"[OMI] OMI_vehicle_body on '{target.name}' has no Rigidbody. " +
                               "Vehicle body should be used with OMI_physics_body.");
            }

            var vehicleBody = target.AddComponent<OMIVehicleBody>();
            
            vehicleBody.AngularActivation = data.AngularActivationVector;
            vehicleBody.LinearActivation = data.LinearActivationVector;
            vehicleBody.GyroTorque = data.GyroTorqueVector;
            vehicleBody.MaxSpeed = data.maxSpeed;
            vehicleBody.AngularDampeners = data.angularDampeners;
            vehicleBody.LinearDampeners = data.linearDampeners;
            vehicleBody.UseThrottle = data.useThrottle;

            // Store pilot seat index for later resolution
            if (data.pilotSeat >= 0)
            {
                context.RegisterDeferredAction(() => ResolvePilotSeat(vehicleBody, data.pilotSeat, context));
            }

            Debug.Log($"[OMI] OMI_vehicle_body: Created vehicle body on '{target.name}'");
            return Task.FromResult(true);
        }

        private void ResolvePilotSeat(OMIVehicleBody vehicleBody, int seatIndex, OMIImportContext context)
        {
            // The pilot seat node should have OMI_seat component
            var seatObject = context.GetNodeByIndex(seatIndex);
            if (seatObject != null)
            {
                var seatComponent = seatObject.GetComponent<Seat.OMISeat>();
                if (seatComponent != null)
                {
                    vehicleBody.PilotSeat = seatComponent;
                }
                else
                {
                    Debug.LogWarning($"[OMI] OMI_vehicle_body: Pilot seat node {seatIndex} has no OMI_seat component");
                }
            }
        }

        /// <inheritdoc/>
        public OMIVehicleBodyNode ExportNodeExtension(GameObject source, OMIExportContext context)
        {
            var vehicleBody = source.GetComponent<OMIVehicleBody>();
            if (vehicleBody == null)
            {
                return null;
            }

            var data = new OMIVehicleBodyNode
            {
                angularActivation = new[] { 
                    vehicleBody.AngularActivation.x, 
                    vehicleBody.AngularActivation.y, 
                    vehicleBody.AngularActivation.z 
                },
                linearActivation = new[] { 
                    vehicleBody.LinearActivation.x, 
                    vehicleBody.LinearActivation.y, 
                    vehicleBody.LinearActivation.z 
                },
                gyroTorque = new[] { 
                    vehicleBody.GyroTorque.x, 
                    vehicleBody.GyroTorque.y, 
                    vehicleBody.GyroTorque.z 
                },
                maxSpeed = vehicleBody.MaxSpeed,
                angularDampeners = vehicleBody.AngularDampeners,
                linearDampeners = vehicleBody.LinearDampeners,
                useThrottle = vehicleBody.UseThrottle
            };

            // Export pilot seat reference
            if (vehicleBody.PilotSeat != null)
            {
                data.pilotSeat = context.GetNodeIndex(vehicleBody.PilotSeat.gameObject);
            }

            return data;
        }
    }

    /// <summary>
    /// Default handler for OMI_vehicle_wheel extension.
    /// Creates OMIVehicleWheel components from glTF data.
    /// </summary>
    public class DefaultVehicleWheelHandler : IOMIDocumentExtensionHandler<OMIVehicleWheelRoot>,
                                               IOMINodeExtensionHandler<OMIVehicleWheelNode>
    {
        /// <summary>
        /// Extension name for OMI_vehicle_wheel.
        /// </summary>
        public const string ExtensionNameConst = "OMI_vehicle_wheel";

        /// <inheritdoc/>
        public string ExtensionName => ExtensionNameConst;

        /// <inheritdoc/>
        public int Priority => 20; // Process after vehicle body

        private OMIVehicleWheelRoot _rootData;

        // Explicit interface implementation for IOMIExtensionHandler<OMIVehicleWheelRoot>
        Task IOMIExtensionHandler<OMIVehicleWheelRoot>.OnImportAsync(OMIVehicleWheelRoot data, OMIImportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        Task<OMIVehicleWheelRoot> IOMIExtensionHandler<OMIVehicleWheelRoot>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        // Explicit interface implementation for IOMIExtensionHandler<OMIVehicleWheelNode>
        Task IOMIExtensionHandler<OMIVehicleWheelNode>.OnImportAsync(OMIVehicleWheelNode data, OMIImportContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task<OMIVehicleWheelNode> IOMIExtensionHandler<OMIVehicleWheelNode>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult<OMIVehicleWheelNode>(null);
        }

        /// <inheritdoc/>
        public Task OnDocumentImportAsync(OMIVehicleWheelRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            ProcessDocumentExtensionAsync(data, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleWheelRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportDocumentExtension(context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task OnNodeImportAsync(OMIVehicleWheelNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            ProcessNodeExtensionAsync(data, targetObject, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleWheelNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportNodeExtension(sourceObject, context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> ProcessDocumentExtensionAsync(OMIVehicleWheelRoot data, OMIImportContext context)
        {
            _rootData = data;
            context.SetExtensionData(ExtensionName, data);
            
            Debug.Log($"[OMI] OMI_vehicle_wheel: Loaded {data?.wheels?.Length ?? 0} wheel definitions");
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> ProcessNodeExtensionAsync(OMIVehicleWheelNode data, GameObject target, OMIImportContext context)
        {
            if (data == null || target == null || _rootData == null)
            {
                return Task.FromResult(false);
            }

            if (data.wheel < 0 || data.wheel >= (_rootData.wheels?.Length ?? 0))
            {
                Debug.LogWarning($"[OMI] OMI_vehicle_wheel: Invalid wheel index {data.wheel}");
                return Task.FromResult(false);
            }

            var wheelSettings = _rootData.wheels[data.wheel];
            var wheelComponent = target.AddComponent<OMIVehicleWheel>();

            wheelComponent.CurrentForceRatio = wheelSettings.currentForceRatio;
            wheelComponent.CurrentSteeringRatio = wheelSettings.currentSteeringRatio;
            wheelComponent.MaxForce = wheelSettings.maxForce;
            wheelComponent.MaxSteeringAngle = wheelSettings.maxSteeringAngle;
            wheelComponent.Radius = wheelSettings.radius;
            wheelComponent.Width = wheelSettings.width;
            wheelComponent.SuspensionStiffness = wheelSettings.suspensionStiffness;
            wheelComponent.SuspensionTravel = wheelSettings.suspensionTravel;

            wheelComponent.SetupWheelCollider();

            Debug.Log($"[OMI] OMI_vehicle_wheel: Created wheel on '{target.name}' (radius={wheelSettings.radius}m)");
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public OMIVehicleWheelRoot ExportDocumentExtension(OMIExportContext context)
        {
            var wheels = new List<OMIVehicleWheelSettings>();
            var wheelComponents = context.RootObjects != null
                ? new List<OMIVehicleWheel>()
                : new List<OMIVehicleWheel>(Object.FindObjectsOfType<OMIVehicleWheel>());

            if (context.RootObjects != null)
            {
                foreach (var root in context.RootObjects)
                {
                    wheelComponents.AddRange(root.GetComponentsInChildren<OMIVehicleWheel>(true));
                }
            }

            foreach (var wheel in wheelComponents)
            {
                // Register the wheel index for node-level export
                int wheelIndex = wheels.Count;
                context.RegisterWheelIndex(wheel.gameObject, wheelIndex);

                wheels.Add(new OMIVehicleWheelSettings
                {
                    currentForceRatio = wheel.CurrentForceRatio,
                    currentSteeringRatio = wheel.CurrentSteeringRatio,
                    maxForce = wheel.MaxForce,
                    maxSteeringAngle = wheel.MaxSteeringAngle,
                    radius = wheel.Radius,
                    width = wheel.Width,
                    suspensionStiffness = wheel.SuspensionStiffness,
                    suspensionTravel = wheel.SuspensionTravel
                });
            }

            if (wheels.Count == 0)
            {
                return null;
            }

            return new OMIVehicleWheelRoot { wheels = wheels.ToArray() };
        }

        /// <inheritdoc/>
        public OMIVehicleWheelNode ExportNodeExtension(GameObject source, OMIExportContext context)
        {
            var wheel = source.GetComponent<OMIVehicleWheel>();
            if (wheel == null)
            {
                return null;
            }

            int wheelIndex = context.GetWheelIndex(source);
            if (wheelIndex < 0)
            {
                return null;
            }

            return new OMIVehicleWheelNode { wheel = wheelIndex };
        }
    }

    /// <summary>
    /// Default handler for OMI_vehicle_thruster extension.
    /// Creates OMIVehicleThruster components from glTF data.
    /// </summary>
    public class DefaultVehicleThrusterHandler : IOMIDocumentExtensionHandler<OMIVehicleThrusterRoot>,
                                                  IOMINodeExtensionHandler<OMIVehicleThrusterNode>
    {
        /// <summary>
        /// Extension name for OMI_vehicle_thruster.
        /// </summary>
        public const string ExtensionNameConst = "OMI_vehicle_thruster";

        /// <inheritdoc/>
        public string ExtensionName => ExtensionNameConst;

        /// <inheritdoc/>
        public int Priority => 20;

        private OMIVehicleThrusterRoot _rootData;

        // Explicit interface implementation for IOMIExtensionHandler<OMIVehicleThrusterRoot>
        Task IOMIExtensionHandler<OMIVehicleThrusterRoot>.OnImportAsync(OMIVehicleThrusterRoot data, OMIImportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        Task<OMIVehicleThrusterRoot> IOMIExtensionHandler<OMIVehicleThrusterRoot>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        // Explicit interface implementation for IOMIExtensionHandler<OMIVehicleThrusterNode>
        Task IOMIExtensionHandler<OMIVehicleThrusterNode>.OnImportAsync(OMIVehicleThrusterNode data, OMIImportContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task<OMIVehicleThrusterNode> IOMIExtensionHandler<OMIVehicleThrusterNode>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult<OMIVehicleThrusterNode>(null);
        }

        /// <inheritdoc/>
        public Task OnDocumentImportAsync(OMIVehicleThrusterRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            ProcessDocumentExtensionAsync(data, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleThrusterRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportDocumentExtension(context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task OnNodeImportAsync(OMIVehicleThrusterNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            ProcessNodeExtensionAsync(data, targetObject, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleThrusterNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportNodeExtension(sourceObject, context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> ProcessDocumentExtensionAsync(OMIVehicleThrusterRoot data, OMIImportContext context)
        {
            _rootData = data;
            context.SetExtensionData(ExtensionName, data);

            Debug.Log($"[OMI] OMI_vehicle_thruster: Loaded {data?.thrusters?.Length ?? 0} thruster definitions");
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> ProcessNodeExtensionAsync(OMIVehicleThrusterNode data, GameObject target, OMIImportContext context)
        {
            if (data == null || target == null || _rootData == null)
            {
                return Task.FromResult(false);
            }

            if (data.thruster < 0 || data.thruster >= (_rootData.thrusters?.Length ?? 0))
            {
                Debug.LogWarning($"[OMI] OMI_vehicle_thruster: Invalid thruster index {data.thruster}");
                return Task.FromResult(false);
            }

            var thrusterSettings = _rootData.thrusters[data.thruster];
            var thrusterComponent = target.AddComponent<OMIVehicleThruster>();

            thrusterComponent.CurrentForceRatio = thrusterSettings.currentForceRatio;
            thrusterComponent.CurrentGimbalRatio = thrusterSettings.CurrentGimbalRatioVector;
            thrusterComponent.MaxForce = thrusterSettings.maxForce;
            thrusterComponent.MaxGimbal = thrusterSettings.maxGimbal;

            Debug.Log($"[OMI] OMI_vehicle_thruster: Created thruster on '{target.name}' (maxForce={thrusterSettings.maxForce}N)");
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public OMIVehicleThrusterRoot ExportDocumentExtension(OMIExportContext context)
        {
            var thrusters = new List<OMIVehicleThrusterSettings>();
            var thrusterComponents = context.RootObjects != null
                ? new List<OMIVehicleThruster>()
                : new List<OMIVehicleThruster>(Object.FindObjectsOfType<OMIVehicleThruster>());

            if (context.RootObjects != null)
            {
                foreach (var root in context.RootObjects)
                {
                    thrusterComponents.AddRange(root.GetComponentsInChildren<OMIVehicleThruster>(true));
                }
            }

            foreach (var thruster in thrusterComponents)
            {
                // Register the thruster index for node-level export
                int thrusterIndex = thrusters.Count;
                context.RegisterThrusterIndex(thruster.gameObject, thrusterIndex);

                thrusters.Add(new OMIVehicleThrusterSettings
                {
                    currentForceRatio = thruster.CurrentForceRatio,
                    currentGimbalRatio = new[] { thruster.CurrentGimbalRatio.x, thruster.CurrentGimbalRatio.y },
                    maxForce = thruster.MaxForce,
                    maxGimbal = thruster.MaxGimbal
                });
            }

            if (thrusters.Count == 0)
            {
                return null;
            }

            return new OMIVehicleThrusterRoot { thrusters = thrusters.ToArray() };
        }

        /// <inheritdoc/>
        public OMIVehicleThrusterNode ExportNodeExtension(GameObject source, OMIExportContext context)
        {
            var thruster = source.GetComponent<OMIVehicleThruster>();
            if (thruster == null)
            {
                return null;
            }

            int thrusterIndex = context.GetThrusterIndex(source);
            if (thrusterIndex < 0)
            {
                return null;
            }

            return new OMIVehicleThrusterNode { thruster = thrusterIndex };
        }
    }

    /// <summary>
    /// Default handler for OMI_vehicle_hover_thruster extension.
    /// Creates OMIVehicleHoverThruster components from glTF data.
    /// </summary>
    public class DefaultVehicleHoverThrusterHandler : IOMIDocumentExtensionHandler<OMIVehicleHoverThrusterRoot>,
                                                       IOMINodeExtensionHandler<OMIVehicleHoverThrusterNode>
    {
        /// <summary>
        /// Extension name for OMI_vehicle_hover_thruster.
        /// </summary>
        public const string ExtensionNameConst = "OMI_vehicle_hover_thruster";

        /// <inheritdoc/>
        public string ExtensionName => ExtensionNameConst;

        /// <inheritdoc/>
        public int Priority => 20;

        private OMIVehicleHoverThrusterRoot _rootData;

        // Explicit interface implementation for IOMIExtensionHandler<OMIVehicleHoverThrusterRoot>
        Task IOMIExtensionHandler<OMIVehicleHoverThrusterRoot>.OnImportAsync(OMIVehicleHoverThrusterRoot data, OMIImportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        Task<OMIVehicleHoverThrusterRoot> IOMIExtensionHandler<OMIVehicleHoverThrusterRoot>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        // Explicit interface implementation for IOMIExtensionHandler<OMIVehicleHoverThrusterNode>
        Task IOMIExtensionHandler<OMIVehicleHoverThrusterNode>.OnImportAsync(OMIVehicleHoverThrusterNode data, OMIImportContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task<OMIVehicleHoverThrusterNode> IOMIExtensionHandler<OMIVehicleHoverThrusterNode>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult<OMIVehicleHoverThrusterNode>(null);
        }

        /// <inheritdoc/>
        public Task OnDocumentImportAsync(OMIVehicleHoverThrusterRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            ProcessDocumentExtensionAsync(data, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleHoverThrusterRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportDocumentExtension(context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task OnNodeImportAsync(OMIVehicleHoverThrusterNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            ProcessNodeExtensionAsync(data, targetObject, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIVehicleHoverThrusterNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportNodeExtension(sourceObject, context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> ProcessDocumentExtensionAsync(OMIVehicleHoverThrusterRoot data, OMIImportContext context)
        {
            _rootData = data;
            context.SetExtensionData(ExtensionName, data);

            Debug.Log($"[OMI] OMI_vehicle_hover_thruster: Loaded {data?.hoverThrusters?.Length ?? 0} hover thruster definitions");
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> ProcessNodeExtensionAsync(OMIVehicleHoverThrusterNode data, GameObject target, OMIImportContext context)
        {
            if (data == null || target == null || _rootData == null)
            {
                return Task.FromResult(false);
            }

            if (data.hoverThruster < 0 || data.hoverThruster >= (_rootData.hoverThrusters?.Length ?? 0))
            {
                Debug.LogWarning($"[OMI] OMI_vehicle_hover_thruster: Invalid hover thruster index {data.hoverThruster}");
                return Task.FromResult(false);
            }

            var hoverSettings = _rootData.hoverThrusters[data.hoverThruster];
            var hoverComponent = target.AddComponent<OMIVehicleHoverThruster>();

            hoverComponent.CurrentHoverRatio = hoverSettings.currentHoverRatio;
            hoverComponent.CurrentGimbalRatio = hoverSettings.CurrentGimbalRatioVector;
            hoverComponent.MaxHoverEnergy = hoverSettings.maxHoverEnergy;
            hoverComponent.MaxGimbal = hoverSettings.maxGimbal;

            Debug.Log($"[OMI] OMI_vehicle_hover_thruster: Created hover thruster on '{target.name}' (maxEnergy={hoverSettings.maxHoverEnergy}N⋅m)");
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public OMIVehicleHoverThrusterRoot ExportDocumentExtension(OMIExportContext context)
        {
            var hoverThrusters = new List<OMIVehicleHoverThrusterSettings>();
            var hoverComponents = context.RootObjects != null
                ? new List<OMIVehicleHoverThruster>()
                : new List<OMIVehicleHoverThruster>(Object.FindObjectsOfType<OMIVehicleHoverThruster>());

            if (context.RootObjects != null)
            {
                foreach (var root in context.RootObjects)
                {
                    hoverComponents.AddRange(root.GetComponentsInChildren<OMIVehicleHoverThruster>(true));
                }
            }

            foreach (var hover in hoverComponents)
            {
                // Register the hover thruster index for node-level export
                int hoverIndex = hoverThrusters.Count;
                context.RegisterHoverThrusterIndex(hover.gameObject, hoverIndex);

                hoverThrusters.Add(new OMIVehicleHoverThrusterSettings
                {
                    currentHoverRatio = hover.CurrentHoverRatio,
                    currentGimbalRatio = new[] { hover.CurrentGimbalRatio.x, hover.CurrentGimbalRatio.y },
                    maxHoverEnergy = hover.MaxHoverEnergy,
                    maxGimbal = hover.MaxGimbal
                });
            }

            if (hoverThrusters.Count == 0)
            {
                return null;
            }

            return new OMIVehicleHoverThrusterRoot { hoverThrusters = hoverThrusters.ToArray() };
        }

        /// <inheritdoc/>
        public OMIVehicleHoverThrusterNode ExportNodeExtension(GameObject source, OMIExportContext context)
        {
            var hoverThruster = source.GetComponent<OMIVehicleHoverThruster>();
            if (hoverThruster == null)
            {
                return null;
            }

            int hoverIndex = context.GetHoverThrusterIndex(source);
            if (hoverIndex < 0)
            {
                return null;
            }

            return new OMIVehicleHoverThrusterNode { hoverThruster = hoverIndex };
        }
    }
}
