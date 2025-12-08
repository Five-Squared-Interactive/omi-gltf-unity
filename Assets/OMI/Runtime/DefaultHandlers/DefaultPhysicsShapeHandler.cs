// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.PhysicsShape
{
    /// <summary>
    /// Default Unity implementation for handling OMI_physics_shape.
    /// Creates Unity Colliders from OMI physics shapes.
    /// </summary>
    public class DefaultPhysicsShapeHandler : IPhysicsShapeHandler
    {
        public string ExtensionName => OMIPhysicsShapeExtension.ExtensionName;
        public int Priority => 100;

        // Storage for shapes during import/export
        private List<OMIPhysicsShape> _importedShapes;
        private List<OMIPhysicsShape> _exportShapes;
        private Dictionary<string, int> _exportShapeIndices;

        public Task OnImportAsync(OMIPhysicsShapeRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        public Task<OMIPhysicsShapeRoot> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        public Task OnDocumentImportAsync(OMIPhysicsShapeRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data?.Shapes == null) return Task.CompletedTask;

            _importedShapes = new List<OMIPhysicsShape>(data.Shapes);
            
            // Store shapes in context for other handlers to access
            context.CustomData["OMI_physics_shapes"] = _importedShapes;

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Imported {_importedShapes.Count} physics shapes");
            }

            return Task.CompletedTask;
        }

        public Task<OMIPhysicsShapeRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            if (_exportShapes == null || _exportShapes.Count == 0)
            {
                return Task.FromResult<OMIPhysicsShapeRoot>(null);
            }

            var root = new OMIPhysicsShapeRoot
            {
                Shapes = _exportShapes.ToArray()
            };

            return Task.FromResult(root);
        }

        public Collider CreateCollider(OMIPhysicsShape shape, int shapeIndex, GameObject targetObject, OMIImportContext context)
        {
            if (shape == null || targetObject == null) return null;

            Collider collider = null;

            switch (shape.Type)
            {
                case OMIPhysicsShapeType.Box:
                    collider = CreateBoxCollider(shape.Box, targetObject);
                    break;

                case OMIPhysicsShapeType.Sphere:
                    collider = CreateSphereCollider(shape.Sphere, targetObject);
                    break;

                case OMIPhysicsShapeType.Capsule:
                    collider = CreateCapsuleCollider(shape.Capsule, targetObject);
                    break;

                case OMIPhysicsShapeType.Cylinder:
                    // Unity doesn't have native cylinder colliders, use capsule approximation
                    collider = CreateCylinderCollider(shape.Cylinder, targetObject, context);
                    break;

                case OMIPhysicsShapeType.Convex:
                    collider = CreateConvexCollider(shape.Convex, targetObject, context);
                    break;

                case OMIPhysicsShapeType.Trimesh:
                    collider = CreateTrimeshCollider(shape.Trimesh, targetObject, context);
                    break;

                default:
                    Debug.LogWarning($"[OMI] Unknown physics shape type: {shape.Type}");
                    break;
            }

            if (collider != null && context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created {collider.GetType().Name} for shape type: {shape.Type}");
            }

            return collider;
        }

        public OMIPhysicsShape ExtractShape(Collider collider, OMIExportContext context)
        {
            if (collider == null) return null;

            switch (collider)
            {
                case BoxCollider box:
                    return ExtractBoxShape(box);

                case SphereCollider sphere:
                    return ExtractSphereShape(sphere);

                case CapsuleCollider capsule:
                    return ExtractCapsuleShape(capsule);

                case MeshCollider mesh:
                    return ExtractMeshShape(mesh, context);

                default:
                    Debug.LogWarning($"[OMI] Unsupported collider type for export: {collider.GetType().Name}");
                    return null;
            }
        }

        public int GetOrRegisterShape(OMIPhysicsShape shape, OMIExportContext context)
        {
            if (shape == null) return -1;

            _exportShapes ??= new List<OMIPhysicsShape>();
            _exportShapeIndices ??= new Dictionary<string, int>();

            // Create a key for deduplication
            var key = GetShapeKey(shape);
            
            if (_exportShapeIndices.TryGetValue(key, out var existingIndex))
            {
                return existingIndex;
            }

            var index = _exportShapes.Count;
            _exportShapes.Add(shape);
            _exportShapeIndices[key] = index;
            
            return index;
        }

        #region Private Helpers - Import

        private BoxCollider CreateBoxCollider(OMIPhysicsShapeBox box, GameObject target)
        {
            var collider = target.AddComponent<BoxCollider>();
            
            if (box?.Size != null && box.Size.Length >= 3)
            {
                // glTF uses right-handed, Unity uses left-handed, so we need to convert
                // glTF: +X right, +Y up, +Z forward
                // Unity: +X right, +Y up, +Z forward (but coordinate system is different)
                collider.size = new Vector3(box.Size[0], box.Size[1], box.Size[2]);
            }
            else
            {
                collider.size = Vector3.one;
            }

            return collider;
        }

        private SphereCollider CreateSphereCollider(OMIPhysicsShapeSphere sphere, GameObject target)
        {
            var collider = target.AddComponent<SphereCollider>();
            collider.radius = sphere?.Radius ?? 0.5f;
            return collider;
        }

        private CapsuleCollider CreateCapsuleCollider(OMIPhysicsShapeCapsule capsule, GameObject target)
        {
            var collider = target.AddComponent<CapsuleCollider>();
            
            if (capsule != null)
            {
                // OMI capsule height is the mid-section, Unity height is total
                var avgRadius = (capsule.RadiusTop + capsule.RadiusBottom) / 2f;
                collider.radius = avgRadius;
                collider.height = capsule.Height + avgRadius * 2f;
                
                // OMI capsule is Y-aligned, Unity capsule direction: 0=X, 1=Y, 2=Z
                collider.direction = 1;

                // Note: Unity doesn't support different top/bottom radii
                if (Mathf.Abs(capsule.RadiusTop - capsule.RadiusBottom) > 0.001f)
                {
                    Debug.LogWarning("[OMI] Unity CapsuleCollider doesn't support different top/bottom radii. Using average.");
                }
            }

            return collider;
        }

        private Collider CreateCylinderCollider(OMIPhysicsShapeCylinder cylinder, GameObject target, OMIImportContext context)
        {
            // Unity doesn't have native cylinder colliders
            // Option 1: Use a scaled capsule (loses flat ends)
            // Option 2: Use a mesh collider with generated cylinder mesh
            
            Debug.LogWarning("[OMI] Unity doesn't support native cylinder colliders. Using capsule approximation.");

            var collider = target.AddComponent<CapsuleCollider>();
            
            if (cylinder != null)
            {
                var avgRadius = (cylinder.RadiusTop + cylinder.RadiusBottom) / 2f;
                collider.radius = avgRadius;
                collider.height = cylinder.Height;
                collider.direction = 1; // Y-axis
            }

            return collider;
        }

        private MeshCollider CreateConvexCollider(OMIPhysicsShapeConvex convex, GameObject target, OMIImportContext context)
        {
            if (convex?.Mesh < 0)
            {
                Debug.LogWarning("[OMI] Convex shape has invalid mesh reference");
                return null;
            }

            var mesh = context.GetMesh(convex.Mesh);
            if (mesh == null)
            {
                Debug.LogWarning($"[OMI] Could not find mesh {convex.Mesh} for convex shape");
                return null;
            }

            var collider = target.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = true;
            
            return collider;
        }

        private MeshCollider CreateTrimeshCollider(OMIPhysicsShapeTrimesh trimesh, GameObject target, OMIImportContext context)
        {
            if (trimesh?.Mesh < 0)
            {
                Debug.LogWarning("[OMI] Trimesh shape has invalid mesh reference");
                return null;
            }

            var mesh = context.GetMesh(trimesh.Mesh);
            if (mesh == null)
            {
                Debug.LogWarning($"[OMI] Could not find mesh {trimesh.Mesh} for trimesh shape");
                return null;
            }

            var collider = target.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = false;
            
            return collider;
        }

        #endregion

        #region Private Helpers - Export

        private OMIPhysicsShape ExtractBoxShape(BoxCollider box)
        {
            return new OMIPhysicsShape
            {
                Type = OMIPhysicsShapeType.Box,
                Box = new OMIPhysicsShapeBox
                {
                    Size = new float[] { box.size.x, box.size.y, box.size.z }
                }
            };
        }

        private OMIPhysicsShape ExtractSphereShape(SphereCollider sphere)
        {
            return new OMIPhysicsShape
            {
                Type = OMIPhysicsShapeType.Sphere,
                Sphere = new OMIPhysicsShapeSphere
                {
                    Radius = sphere.radius
                }
            };
        }

        private OMIPhysicsShape ExtractCapsuleShape(CapsuleCollider capsule)
        {
            // Convert Unity total height to OMI mid-section height
            var midHeight = capsule.height - capsule.radius * 2f;
            if (midHeight < 0) midHeight = 0;

            return new OMIPhysicsShape
            {
                Type = OMIPhysicsShapeType.Capsule,
                Capsule = new OMIPhysicsShapeCapsule
                {
                    Height = midHeight,
                    RadiusTop = capsule.radius,
                    RadiusBottom = capsule.radius
                }
            };
        }

        private OMIPhysicsShape ExtractMeshShape(MeshCollider meshCollider, OMIExportContext context)
        {
            if (meshCollider.sharedMesh == null) return null;

            var meshIndex = context.GetMeshIndex(meshCollider.sharedMesh);
            if (meshIndex < 0)
            {
                Debug.LogWarning("[OMI] MeshCollider mesh not found in export context");
                return null;
            }

            if (meshCollider.convex)
            {
                return new OMIPhysicsShape
                {
                    Type = OMIPhysicsShapeType.Convex,
                    Convex = new OMIPhysicsShapeConvex { Mesh = meshIndex }
                };
            }
            else
            {
                return new OMIPhysicsShape
                {
                    Type = OMIPhysicsShapeType.Trimesh,
                    Trimesh = new OMIPhysicsShapeTrimesh { Mesh = meshIndex }
                };
            }
        }

        private string GetShapeKey(OMIPhysicsShape shape)
        {
            // Create a simple key for shape deduplication
            switch (shape.Type)
            {
                case OMIPhysicsShapeType.Box:
                    var b = shape.Box;
                    return $"box_{b.Size[0]}_{b.Size[1]}_{b.Size[2]}";
                    
                case OMIPhysicsShapeType.Sphere:
                    return $"sphere_{shape.Sphere.Radius}";
                    
                case OMIPhysicsShapeType.Capsule:
                    var c = shape.Capsule;
                    return $"capsule_{c.Height}_{c.RadiusTop}_{c.RadiusBottom}";
                    
                case OMIPhysicsShapeType.Cylinder:
                    var cy = shape.Cylinder;
                    return $"cylinder_{cy.Height}_{cy.RadiusTop}_{cy.RadiusBottom}";
                    
                case OMIPhysicsShapeType.Convex:
                    return $"convex_{shape.Convex.Mesh}";
                    
                case OMIPhysicsShapeType.Trimesh:
                    return $"trimesh_{shape.Trimesh.Mesh}";
                    
                default:
                    return $"unknown_{System.Guid.NewGuid()}";
            }
        }

        #endregion
    }
}
