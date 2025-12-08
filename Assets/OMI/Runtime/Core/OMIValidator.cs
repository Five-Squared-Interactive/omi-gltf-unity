// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Validation utilities for OMI extension data.
    /// </summary>
    public static class OMIValidator
    {
        /// <summary>
        /// Result of a validation operation.
        /// </summary>
        public struct ValidationResult
        {
            /// <summary>
            /// Whether the validation passed.
            /// </summary>
            public bool IsValid;

            /// <summary>
            /// List of error messages.
            /// </summary>
            public List<string> Errors;

            /// <summary>
            /// List of warning messages.
            /// </summary>
            public List<string> Warnings;

            /// <summary>
            /// Creates a successful validation result.
            /// </summary>
            public static ValidationResult Success()
            {
                return new ValidationResult
                {
                    IsValid = true,
                    Errors = new List<string>(),
                    Warnings = new List<string>()
                };
            }

            /// <summary>
            /// Creates a failed validation result.
            /// </summary>
            public static ValidationResult Failure(string error)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { error },
                    Warnings = new List<string>()
                };
            }

            /// <summary>
            /// Adds an error to this result.
            /// </summary>
            public void AddError(string error)
            {
                Errors ??= new List<string>();
                Errors.Add(error);
                IsValid = false;
            }

            /// <summary>
            /// Adds a warning to this result.
            /// </summary>
            public void AddWarning(string warning)
            {
                Warnings ??= new List<string>();
                Warnings.Add(warning);
            }

            /// <summary>
            /// Merges another result into this one.
            /// </summary>
            public void Merge(ValidationResult other)
            {
                if (!other.IsValid)
                {
                    IsValid = false;
                }
                
                if (other.Errors != null)
                {
                    Errors ??= new List<string>();
                    Errors.AddRange(other.Errors);
                }
                
                if (other.Warnings != null)
                {
                    Warnings ??= new List<string>();
                    Warnings.AddRange(other.Warnings);
                }
            }

            /// <summary>
            /// Logs all errors and warnings.
            /// </summary>
            public void LogAll()
            {
                if (Errors != null)
                {
                    foreach (var error in Errors)
                    {
                        Debug.LogError($"[OMI Validation] {error}");
                    }
                }
                
                if (Warnings != null)
                {
                    foreach (var warning in Warnings)
                    {
                        Debug.LogWarning($"[OMI Validation] {warning}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates a spawn point node.
        /// </summary>
        public static ValidationResult ValidateSpawnPoint(Extensions.SpawnPoint.OMISpawnPointNode data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Spawn point data is null");
                return result;
            }

            // Title validation (max 128 chars)
            if (!string.IsNullOrEmpty(data.Title) && data.Title.Length > 128)
            {
                result.AddError($"Spawn point title exceeds 128 characters (length: {data.Title.Length})");
            }

            // Team validation (max 128 chars)
            if (!string.IsNullOrEmpty(data.Team) && data.Team.Length > 128)
            {
                result.AddError($"Spawn point team exceeds 128 characters (length: {data.Team.Length})");
            }

            // Group validation (max 128 chars)
            if (!string.IsNullOrEmpty(data.Group) && data.Group.Length > 128)
            {
                result.AddError($"Spawn point group exceeds 128 characters (length: {data.Group.Length})");
            }

            return result;
        }

        /// <summary>
        /// Validates a seat node.
        /// </summary>
        public static ValidationResult ValidateSeat(Extensions.Seat.OMISeatNode data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Seat data is null");
                return result;
            }

            // Back position is required
            if (data.Back == null || data.Back.Length < 3)
            {
                result.AddError("Seat 'back' position is required and must have 3 components");
            }

            // Foot position is required
            if (data.Foot == null || data.Foot.Length < 3)
            {
                result.AddError("Seat 'foot' position is required and must have 3 components");
            }

            // Knee position is required
            if (data.Knee == null || data.Knee.Length < 3)
            {
                result.AddError("Seat 'knee' position is required and must have 3 components");
            }

            // Angle validation
            if (data.Angle < 0 || data.Angle > Mathf.PI)
            {
                result.AddWarning($"Seat angle {data.Angle} is outside typical range [0, π]");
            }

            return result;
        }

        /// <summary>
        /// Validates a link node.
        /// </summary>
        public static ValidationResult ValidateLink(Extensions.Link.OMILinkNode data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Link data is null");
                return result;
            }

            // URI is required
            if (string.IsNullOrEmpty(data.Uri))
            {
                result.AddError("Link 'uri' is required");
                return result;
            }

            // Validate URI format
            if (!IsValidUri(data.Uri))
            {
                result.AddWarning($"Link URI '{data.Uri}' may not be a valid format");
            }

            return result;
        }

        /// <summary>
        /// Validates a physics shape.
        /// </summary>
        public static ValidationResult ValidatePhysicsShape(Extensions.PhysicsShape.OMIPhysicsShape data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Physics shape data is null");
                return result;
            }

            switch (data.Type)
            {
                case Extensions.PhysicsShape.OMIPhysicsShapeType.Box:
                    result.Merge(ValidateBoxShape(data.Box));
                    break;
                case Extensions.PhysicsShape.OMIPhysicsShapeType.Sphere:
                    result.Merge(ValidateSphereShape(data.Sphere));
                    break;
                case Extensions.PhysicsShape.OMIPhysicsShapeType.Capsule:
                    result.Merge(ValidateCapsuleShape(data.Capsule));
                    break;
                case Extensions.PhysicsShape.OMIPhysicsShapeType.Cylinder:
                    result.Merge(ValidateCylinderShape(data.Cylinder));
                    break;
                default:
                    result.AddWarning($"Unknown or unvalidated shape type: {data.Type}");
                    break;
            }

            return result;
        }

        private static ValidationResult ValidateBoxShape(Extensions.PhysicsShape.OMIPhysicsShapeBox box)
        {
            var result = ValidationResult.Success();

            if (box == null)
            {
                result.AddError("Box shape data is null");
                return result;
            }

            if (box.Size == null || box.Size.Length < 3)
            {
                result.AddError("Box shape requires 'size' with 3 components");
            }
            else
            {
                if (box.Size[0] <= 0 || box.Size[1] <= 0 || box.Size[2] <= 0)
                {
                    result.AddError("Box shape size components must be positive");
                }
            }

            return result;
        }

        private static ValidationResult ValidateSphereShape(Extensions.PhysicsShape.OMIPhysicsShapeSphere sphere)
        {
            var result = ValidationResult.Success();

            if (sphere == null)
            {
                result.AddError("Sphere shape data is null");
                return result;
            }

            if (sphere.Radius <= 0)
            {
                result.AddError("Sphere shape radius must be positive");
            }

            return result;
        }

        private static ValidationResult ValidateCapsuleShape(Extensions.PhysicsShape.OMIPhysicsShapeCapsule capsule)
        {
            var result = ValidationResult.Success();

            if (capsule == null)
            {
                result.AddError("Capsule shape data is null");
                return result;
            }

            if (capsule.RadiusBottom <= 0)
            {
                result.AddError("Capsule shape radiusBottom must be positive");
            }

            if (capsule.RadiusTop <= 0)
            {
                result.AddError("Capsule shape radiusTop must be positive");
            }

            if (capsule.Height <= 0)
            {
                result.AddError("Capsule shape height must be positive");
            }

            return result;
        }

        private static ValidationResult ValidateCylinderShape(Extensions.PhysicsShape.OMIPhysicsShapeCylinder cylinder)
        {
            var result = ValidationResult.Success();

            if (cylinder == null)
            {
                result.AddError("Cylinder shape data is null");
                return result;
            }

            if (cylinder.RadiusBottom <= 0)
            {
                result.AddError("Cylinder shape radiusBottom must be positive");
            }

            if (cylinder.RadiusTop <= 0)
            {
                result.AddError("Cylinder shape radiusTop must be positive");
            }

            if (cylinder.Height <= 0)
            {
                result.AddError("Cylinder shape height must be positive");
            }

            return result;
        }

        /// <summary>
        /// Validates a physics body node.
        /// </summary>
        public static ValidationResult ValidatePhysicsBody(Extensions.PhysicsBody.OMIPhysicsBodyNode data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Physics body data is null");
                return result;
            }

            // Validate motion if present
            if (data.Motion != null)
            {
                var validMotionTypes = new[] { "static", "kinematic", "dynamic" };
                if (!string.IsNullOrEmpty(data.Motion.Type) && 
                    Array.IndexOf(validMotionTypes, data.Motion.Type.ToLowerInvariant()) < 0)
                {
                    result.AddError($"Invalid motion type '{data.Motion.Type}'. Must be: static, kinematic, or dynamic");
                }

                // Validate mass
                if (data.Motion.Mass <= 0)
                {
                    result.AddError("Physics body mass must be positive");
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a URI string is valid.
        /// </summary>
        private static bool IsValidUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return false;

            // Allow relative paths
            if (uri.StartsWith("./") || uri.StartsWith("../"))
                return true;

            // Allow fragments
            if (uri.StartsWith("#"))
                return true;

            // Check absolute URIs
            return Uri.TryCreate(uri, UriKind.Absolute, out _) || 
                   Uri.TryCreate(uri, UriKind.Relative, out _);
        }
    }
}
