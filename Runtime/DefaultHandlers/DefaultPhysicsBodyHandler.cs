// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OMI.Extensions.PhysicsShape;
using UnityEngine;

namespace OMI.Extensions.PhysicsBody
{
    /// <summary>
    /// Default Unity implementation for handling OMI_physics_body.
    /// Creates Unity Rigidbodies and configures colliders.
    /// </summary>
    public class DefaultPhysicsBodyHandler : IPhysicsBodyHandler, IPhysicsBodyDocumentHandler
    {
        public string ExtensionName => OMIPhysicsBodyExtension.ExtensionName;
        public int Priority => 90; // Lower than shape handler to ensure shapes are processed first

        private List<OMIPhysicsMaterial> _importedMaterials;
        private List<OMICollisionFilter> _importedFilters;
        private List<OMIPhysicsMaterial> _exportMaterials;
        private List<OMICollisionFilter> _exportFilters;

        #region IOMIExtensionHandler

        public Task OnImportAsync(OMIPhysicsBodyNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            // Node-level import is handled by OnNodeImportAsync
            return Task.CompletedTask;
        }

        public Task<OMIPhysicsBodyNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Node-level export is handled by OnNodeExportAsync
            return Task.FromResult<OMIPhysicsBodyNode>(null);
        }

        #endregion

        #region IPhysicsBodyDocumentHandler

        public Task OnImportAsync(OMIPhysicsBodyRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        Task<OMIPhysicsBodyRoot> IOMIExtensionHandler<OMIPhysicsBodyRoot>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        public Task OnDocumentImportAsync(OMIPhysicsBodyRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null) return Task.CompletedTask;

            _importedMaterials = data.PhysicsMaterials != null 
                ? new List<OMIPhysicsMaterial>(data.PhysicsMaterials) 
                : new List<OMIPhysicsMaterial>();
                
            _importedFilters = data.CollisionFilters != null 
                ? new List<OMICollisionFilter>(data.CollisionFilters) 
                : new List<OMICollisionFilter>();

            context.CustomData["OMI_physics_materials"] = _importedMaterials;
            context.CustomData["OMI_collision_filters"] = _importedFilters;

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Imported {_importedMaterials.Count} physics materials and {_importedFilters.Count} collision filters");
            }

            return Task.CompletedTask;
        }

        public Task<OMIPhysicsBodyRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            if ((_exportMaterials == null || _exportMaterials.Count == 0) &&
                (_exportFilters == null || _exportFilters.Count == 0))
            {
                return Task.FromResult<OMIPhysicsBodyRoot>(null);
            }

            var root = new OMIPhysicsBodyRoot
            {
                PhysicsMaterials = _exportMaterials?.Count > 0 ? _exportMaterials.ToArray() : null,
                CollisionFilters = _exportFilters?.Count > 0 ? _exportFilters.ToArray() : null
            };

            return Task.FromResult(root);
        }

        public PhysicsMaterial CreatePhysicMaterial(OMIPhysicsMaterial material, int materialIndex, OMIImportContext context)
        {
            if (material == null) return null;

            var physicMaterial = new PhysicsMaterial
            {
                name = $"OMI_Material_{materialIndex}",
                staticFriction = material.StaticFriction,
                dynamicFriction = material.DynamicFriction,
                bounciness = material.Restitution
            };

            // Convert combine modes
            physicMaterial.frictionCombine = ConvertCombineMode(material.FrictionCombine);
            physicMaterial.bounceCombine = ConvertCombineMode(material.RestitutionCombine);

            return physicMaterial;
        }

        public OMIPhysicsMaterial ExtractPhysicMaterial(PhysicsMaterial material, OMIExportContext context)
        {
            if (material == null) return null;

            return new OMIPhysicsMaterial
            {
                StaticFriction = material.staticFriction,
                DynamicFriction = material.dynamicFriction,
                Restitution = material.bounciness,
                FrictionCombine = ConvertCombineMode(material.frictionCombine),
                RestitutionCombine = ConvertCombineMode(material.bounceCombine)
            };
        }

        #endregion

        #region IPhysicsBodyHandler

        public Task OnNodeImportAsync(OMIPhysicsBodyNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null) return Task.CompletedTask;

            // Process motion (Rigidbody)
            if (data.Motion != null)
            {
                CreateRigidbody(data.Motion, targetObject, context);
            }

            // Process collider
            if (data.Collider != null)
            {
                ConfigureCollider(data.Collider, targetObject, context);
            }

            // Process trigger
            if (data.Trigger != null)
            {
                ConfigureTrigger(data.Trigger, targetObject, context);
            }

            return Task.CompletedTask;
        }

        public Task<OMIPhysicsBodyNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            if (sourceObject == null) return Task.FromResult<OMIPhysicsBodyNode>(null);

            OMIPhysicsBodyNode node = null;

            // Check for Rigidbody
            var rigidbody = sourceObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                node ??= new OMIPhysicsBodyNode();
                node.Motion = ExtractMotion(rigidbody, context);
            }

            // Check for colliders
            var colliders = sourceObject.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    node ??= new OMIPhysicsBodyNode();
                    node.Trigger = ExtractTrigger(collider, context);
                }
                else
                {
                    node ??= new OMIPhysicsBodyNode();
                    node.Collider = ExtractCollider(collider, context);
                }
            }

            return Task.FromResult(node);
        }

        public Rigidbody CreateRigidbody(OMIPhysicsBodyMotion motion, GameObject targetObject, OMIImportContext context)
        {
            if (motion == null || targetObject == null) return null;

            var rigidbody = targetObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = targetObject.AddComponent<Rigidbody>();
            }

            // Set motion type
            switch (motion.Type)
            {
                case OMIPhysicsMotionType.Static:
                    rigidbody.isKinematic = true;
                    rigidbody.useGravity = false;
                    break;

                case OMIPhysicsMotionType.Kinematic:
                    rigidbody.isKinematic = true;
                    rigidbody.useGravity = false;
                    break;

                case OMIPhysicsMotionType.Dynamic:
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                    break;
            }

            // Set mass
            rigidbody.mass = motion.Mass;

            // Set center of mass
            if (motion.CenterOfMass != null && motion.CenterOfMass.Length >= 3)
            {
                rigidbody.centerOfMass = new Vector3(
                    motion.CenterOfMass[0],
                    motion.CenterOfMass[1],
                    -motion.CenterOfMass[2] // Convert from glTF to Unity coordinate system
                );
            }

            // Set inertia tensor
            if (motion.InertiaDiagonal != null && motion.InertiaDiagonal.Length >= 3)
            {
                var inertia = new Vector3(
                    motion.InertiaDiagonal[0],
                    motion.InertiaDiagonal[1],
                    motion.InertiaDiagonal[2]
                );
                
                if (inertia.sqrMagnitude > 0)
                {
                    rigidbody.inertiaTensor = inertia;
                }
            }

            // Set inertia tensor rotation
            if (motion.InertiaOrientation != null && motion.InertiaOrientation.Length >= 4)
            {
                rigidbody.inertiaTensorRotation = new Quaternion(
                    motion.InertiaOrientation[0],
                    motion.InertiaOrientation[1],
                    -motion.InertiaOrientation[2], // Convert handedness
                    -motion.InertiaOrientation[3]
                );
            }

            // Set initial velocities
            if (motion.LinearVelocity != null && motion.LinearVelocity.Length >= 3)
            {
                rigidbody.linearVelocity = new Vector3(
                    motion.LinearVelocity[0],
                    motion.LinearVelocity[1],
                    -motion.LinearVelocity[2]
                );
            }

            if (motion.AngularVelocity != null && motion.AngularVelocity.Length >= 3)
            {
                rigidbody.angularVelocity = new Vector3(
                    -motion.AngularVelocity[0],
                    -motion.AngularVelocity[1],
                    motion.AngularVelocity[2]
                );
            }

            // Note: Unity doesn't have a direct gravityFactor, but we can fake it
            // by storing it in a component or using gravity scale in newer Unity versions

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created Rigidbody with motion type: {motion.Type}, mass: {motion.Mass}");
            }

            return rigidbody;
        }

        public void ConfigureCollider(OMIPhysicsBodyCollider colliderData, GameObject targetObject, OMIImportContext context)
        {
            if (colliderData == null || targetObject == null) return;

            // Get the shape handler to create the collider
            var shapeHandler = context.ExtensionManager.GetHandler<IPhysicsShapeHandler>();
            if (shapeHandler == null)
            {
                Debug.LogWarning("[OMI] No physics shape handler registered");
                return;
            }

            // Get the shape from the shapes array
            if (!context.TryGetCustomData<List<OMIPhysicsShape>>("OMI_physics_shapes", out var shapes) ||
                colliderData.Shape < 0 || colliderData.Shape >= shapes.Count)
            {
                Debug.LogWarning($"[OMI] Invalid shape index: {colliderData.Shape}");
                return;
            }

            var shape = shapes[colliderData.Shape];
            var collider = shapeHandler.CreateCollider(shape, colliderData.Shape, targetObject, context);

            if (collider != null)
            {
                collider.isTrigger = false;

                // Apply physics material
                if (colliderData.PhysicsMaterial >= 0 && _importedMaterials != null &&
                    colliderData.PhysicsMaterial < _importedMaterials.Count)
                {
                    var material = CreatePhysicMaterial(_importedMaterials[colliderData.PhysicsMaterial], 
                        colliderData.PhysicsMaterial, context);
                    collider.material = material;
                }
            }
        }

        public void ConfigureTrigger(OMIPhysicsBodyTrigger triggerData, GameObject targetObject, OMIImportContext context)
        {
            if (triggerData == null || targetObject == null) return;

            // Handle compound triggers
            if (triggerData.Nodes != null && triggerData.Nodes.Length > 0)
            {
                // Compound trigger - the child nodes define the trigger shapes
                // Mark this node as a compound trigger root
                var marker = targetObject.AddComponent<OMICompoundTriggerMarker>();
                marker.ChildNodeIndices = triggerData.Nodes;
                return;
            }

            // Single shape trigger
            var shapeHandler = context.ExtensionManager.GetHandler<IPhysicsShapeHandler>();
            if (shapeHandler == null)
            {
                Debug.LogWarning("[OMI] No physics shape handler registered");
                return;
            }

            if (!context.TryGetCustomData<List<OMIPhysicsShape>>("OMI_physics_shapes", out var shapes) ||
                triggerData.Shape < 0 || triggerData.Shape >= shapes.Count)
            {
                Debug.LogWarning($"[OMI] Invalid shape index for trigger: {triggerData.Shape}");
                return;
            }

            var shape = shapes[triggerData.Shape];
            var collider = shapeHandler.CreateCollider(shape, triggerData.Shape, targetObject, context);

            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        public OMIPhysicsBodyMotion ExtractMotion(Rigidbody rigidbody, OMIExportContext context)
        {
            if (rigidbody == null) return null;

            string motionType;
            if (rigidbody.isKinematic)
            {
                motionType = rigidbody.useGravity ? OMIPhysicsMotionType.Kinematic : OMIPhysicsMotionType.Static;
            }
            else
            {
                motionType = OMIPhysicsMotionType.Dynamic;
            }

            return new OMIPhysicsBodyMotion
            {
                Type = motionType,
                Mass = rigidbody.mass,
                CenterOfMass = new float[] 
                { 
                    rigidbody.centerOfMass.x, 
                    rigidbody.centerOfMass.y, 
                    -rigidbody.centerOfMass.z 
                },
                InertiaDiagonal = new float[]
                {
                    rigidbody.inertiaTensor.x,
                    rigidbody.inertiaTensor.y,
                    rigidbody.inertiaTensor.z
                },
                LinearVelocity = new float[]
                {
                    rigidbody.linearVelocity.x,
                    rigidbody.linearVelocity.y,
                    -rigidbody.linearVelocity.z
                },
                AngularVelocity = new float[]
                {
                    -rigidbody.angularVelocity.x,
                    -rigidbody.angularVelocity.y,
                    rigidbody.angularVelocity.z
                }
            };
        }

        public OMIPhysicsBodyCollider ExtractCollider(Collider collider, OMIExportContext context)
        {
            if (collider == null || collider.isTrigger) return null;

            var shapeHandler = context.ExtensionManager.GetHandler<IPhysicsShapeHandler>();
            if (shapeHandler == null) return null;

            var shape = shapeHandler.ExtractShape(collider, context);
            if (shape == null) return null;

            var shapeIndex = shapeHandler.GetOrRegisterShape(shape, context);

            var colliderData = new OMIPhysicsBodyCollider
            {
                Shape = shapeIndex
            };

            // Export physics material
            if (collider.material != null)
            {
                var material = ExtractPhysicMaterial(collider.material, context);
                if (material != null)
                {
                    _exportMaterials ??= new List<OMIPhysicsMaterial>();
                    colliderData.PhysicsMaterial = _exportMaterials.Count;
                    _exportMaterials.Add(material);
                }
            }

            return colliderData;
        }

        public OMIPhysicsBodyTrigger ExtractTrigger(Collider collider, OMIExportContext context)
        {
            if (collider == null || !collider.isTrigger) return null;

            var shapeHandler = context.ExtensionManager.GetHandler<IPhysicsShapeHandler>();
            if (shapeHandler == null) return null;

            var shape = shapeHandler.ExtractShape(collider, context);
            if (shape == null) return null;

            var shapeIndex = shapeHandler.GetOrRegisterShape(shape, context);

            return new OMIPhysicsBodyTrigger
            {
                Shape = shapeIndex
            };
        }

        #endregion

        #region Private Helpers

        private PhysicsMaterialCombine ConvertCombineMode(string mode)
        {
            return mode switch
            {
                OMICombineMode.Average => PhysicsMaterialCombine.Average,
                OMICombineMode.Minimum => PhysicsMaterialCombine.Minimum,
                OMICombineMode.Maximum => PhysicsMaterialCombine.Maximum,
                OMICombineMode.Multiply => PhysicsMaterialCombine.Multiply,
                _ => PhysicsMaterialCombine.Average
            };
        }

        private string ConvertCombineMode(PhysicsMaterialCombine mode)
        {
            return mode switch
            {
                PhysicsMaterialCombine.Average => OMICombineMode.Average,
                PhysicsMaterialCombine.Minimum => OMICombineMode.Minimum,
                PhysicsMaterialCombine.Maximum => OMICombineMode.Maximum,
                PhysicsMaterialCombine.Multiply => OMICombineMode.Multiply,
                _ => OMICombineMode.Average
            };
        }

        #endregion
    }

    /// <summary>
    /// Marker component for compound triggers.
    /// </summary>
    public class OMICompoundTriggerMarker : MonoBehaviour
    {
        public int[] ChildNodeIndices;
    }
}
