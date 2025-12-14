// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.PhysicsGravity
{
    /// <summary>
    /// Default Unity implementation for handling OMI_physics_gravity.
    /// Creates OMIPhysicsGravity components on gravity volume nodes.
    /// </summary>
    public class DefaultPhysicsGravityHandler : IPhysicsGravityHandler, IPhysicsGravityDocumentHandler
    {
        public string ExtensionName => OMIPhysicsGravityExtension.ExtensionName;
        public int Priority => 80; // After physics body

        private OMIPhysicsGravityRoot _documentGravity;

        #region IOMIExtensionHandler<OMIPhysicsGravityNode>

        public Task OnImportAsync(OMIPhysicsGravityNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<OMIPhysicsGravityNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<OMIPhysicsGravityNode>(null);
        }

        #endregion

        #region IOMIExtensionHandler<OMIPhysicsGravityRoot>

        public Task OnImportAsync(OMIPhysicsGravityRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        Task<OMIPhysicsGravityRoot> IOMIExtensionHandler<OMIPhysicsGravityRoot>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        #endregion

        #region IPhysicsGravityDocumentHandler

        public Task OnDocumentImportAsync(OMIPhysicsGravityRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null) return Task.CompletedTask;

            _documentGravity = data;

            // Apply world gravity to Unity physics
            Vector3 direction = new Vector3(
                data.Direction?[0] ?? 0f,
                data.Direction?[1] ?? -1f,
                -(data.Direction?[2] ?? 0f) // Convert from glTF to Unity coordinate system
            ).normalized;

            Physics.gravity = direction * data.Gravity;

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Set world gravity to {Physics.gravity}");
            }

            return Task.CompletedTask;
        }

        public Task<OMIPhysicsGravityRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Export current Unity gravity as world gravity
            var unityGravity = Physics.gravity;
            
            return Task.FromResult(new OMIPhysicsGravityRoot
            {
                Gravity = unityGravity.magnitude,
                Direction = new float[]
                {
                    unityGravity.normalized.x,
                    unityGravity.normalized.y,
                    -unityGravity.normalized.z // Convert from Unity to glTF coordinate system
                }
            });
        }

        #endregion

        #region IPhysicsGravityHandler

        public Task OnNodeImportAsync(OMIPhysicsGravityNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null) return Task.CompletedTask;

            CreateGravityVolume(data, targetObject, context);
            return Task.CompletedTask;
        }

        public Task<OMIPhysicsGravityNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExtractGravityVolume(sourceObject, context));
        }

        #endregion

        public void CreateGravityVolume(OMIPhysicsGravityNode data, GameObject targetObject, OMIImportContext context)
        {
            var gravityComponent = targetObject.GetComponent<OMIPhysicsGravity>();
            if (gravityComponent == null)
            {
                gravityComponent = targetObject.AddComponent<OMIPhysicsGravity>();
            }

            gravityComponent.Gravity = data.Gravity;
            gravityComponent.Priority = data.Priority;
            gravityComponent.Replace = data.Replace;
            gravityComponent.Stop = data.Stop;

            // Set type and type-specific parameters
            switch (data.Type?.ToLowerInvariant())
            {
                case OMIGravityType.Directional:
                    gravityComponent.Type = OMIPhysicsGravity.GravityType.Directional;
                    if (data.Directional?.Direction != null && data.Directional.Direction.Length >= 3)
                    {
                        gravityComponent.Direction = new Vector3(
                            data.Directional.Direction[0],
                            data.Directional.Direction[1],
                            -data.Directional.Direction[2] // glTF to Unity
                        );
                    }
                    break;

                case OMIGravityType.Point:
                    gravityComponent.Type = OMIPhysicsGravity.GravityType.Point;
                    gravityComponent.UnitDistance = data.Point?.UnitDistance ?? 0f;
                    break;

                case OMIGravityType.Disc:
                    gravityComponent.Type = OMIPhysicsGravity.GravityType.Disc;
                    gravityComponent.Radius = data.Disc?.Radius ?? 1f;
                    gravityComponent.UnitDistance = data.Disc?.UnitDistance ?? 0f;
                    break;

                case OMIGravityType.Torus:
                    gravityComponent.Type = OMIPhysicsGravity.GravityType.Torus;
                    gravityComponent.Radius = data.Torus?.Radius ?? 1f;
                    gravityComponent.UnitDistance = data.Torus?.UnitDistance ?? 0f;
                    break;

                case OMIGravityType.Line:
                    gravityComponent.Type = OMIPhysicsGravity.GravityType.Line;
                    gravityComponent.UnitDistance = data.Line?.UnitDistance ?? 0f;
                    if (data.Line?.Points != null && data.Line.Points.Length >= 6)
                    {
                        int pointCount = data.Line.Points.Length / 3;
                        gravityComponent.LinePoints = new Vector3[pointCount];
                        for (int i = 0; i < pointCount; i++)
                        {
                            gravityComponent.LinePoints[i] = new Vector3(
                                data.Line.Points[i * 3],
                                data.Line.Points[i * 3 + 1],
                                -data.Line.Points[i * 3 + 2] // glTF to Unity
                            );
                        }
                    }
                    break;

                case OMIGravityType.Shaped:
                    gravityComponent.Type = OMIPhysicsGravity.GravityType.Shaped;
                    gravityComponent.UnitDistance = data.Shaped?.UnitDistance ?? 0f;
                    // Shape reference would need to be resolved through the shape handler
                    // Store the index for later resolution
                    gravityComponent.ShapeCollider = null; // Will be resolved later
                    break;

                default:
                    Debug.LogWarning($"[OMI] Unknown gravity type: {data.Type}");
                    break;
            }

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created gravity volume ({data.Type}) on {targetObject.name}");
            }
        }

        public OMIPhysicsGravityNode ExtractGravityVolume(GameObject sourceObject, OMIExportContext context)
        {
            if (sourceObject == null) return null;

            var gravityComponent = sourceObject.GetComponent<OMIPhysicsGravity>();
            if (gravityComponent == null) return null;

            var node = new OMIPhysicsGravityNode
            {
                Gravity = gravityComponent.Gravity,
                Priority = gravityComponent.Priority,
                Replace = gravityComponent.Replace,
                Stop = gravityComponent.Stop
            };

            switch (gravityComponent.Type)
            {
                case OMIPhysicsGravity.GravityType.Directional:
                    node.Type = OMIGravityType.Directional;
                    node.Directional = new OMIGravityDirectional
                    {
                        Direction = new float[]
                        {
                            gravityComponent.Direction.x,
                            gravityComponent.Direction.y,
                            -gravityComponent.Direction.z // Unity to glTF
                        }
                    };
                    break;

                case OMIPhysicsGravity.GravityType.Point:
                    node.Type = OMIGravityType.Point;
                    node.Point = new OMIGravityPoint
                    {
                        UnitDistance = gravityComponent.UnitDistance
                    };
                    break;

                case OMIPhysicsGravity.GravityType.Disc:
                    node.Type = OMIGravityType.Disc;
                    node.Disc = new OMIGravityDisc
                    {
                        Radius = gravityComponent.Radius,
                        UnitDistance = gravityComponent.UnitDistance
                    };
                    break;

                case OMIPhysicsGravity.GravityType.Torus:
                    node.Type = OMIGravityType.Torus;
                    node.Torus = new OMIGravityTorus
                    {
                        Radius = gravityComponent.Radius,
                        UnitDistance = gravityComponent.UnitDistance
                    };
                    break;

                case OMIPhysicsGravity.GravityType.Line:
                    node.Type = OMIGravityType.Line;
                    var linePoints = new float[gravityComponent.LinePoints?.Length * 3 ?? 0];
                    if (gravityComponent.LinePoints != null)
                    {
                        for (int i = 0; i < gravityComponent.LinePoints.Length; i++)
                        {
                            linePoints[i * 3] = gravityComponent.LinePoints[i].x;
                            linePoints[i * 3 + 1] = gravityComponent.LinePoints[i].y;
                            linePoints[i * 3 + 2] = -gravityComponent.LinePoints[i].z; // Unity to glTF
                        }
                    }
                    node.Line = new OMIGravityLine
                    {
                        Points = linePoints,
                        UnitDistance = gravityComponent.UnitDistance
                    };
                    break;

                case OMIPhysicsGravity.GravityType.Shaped:
                    node.Type = OMIGravityType.Shaped;
                    node.Shaped = new OMIGravityShaped
                    {
                        Shape = -1, // Would need shape registration
                        UnitDistance = gravityComponent.UnitDistance
                    };
                    break;
            }

            return node;
        }
    }
}
