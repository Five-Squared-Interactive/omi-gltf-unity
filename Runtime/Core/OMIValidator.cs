// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using OMI.Extensions.EnvironmentSky;

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

        /// <summary>
        /// Validates environment sky data.
        /// </summary>
        public static ValidationResult ValidateEnvironmentSky(Extensions.EnvironmentSky.OMIEnvironmentSkySkyData data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Environment sky data is null");
                return result;
            }

            var skyType = data.GetSkyType();

            switch (skyType)
            {
                case Extensions.EnvironmentSky.OMIEnvironmentSkyType.Gradient:
                    if (data.Gradient == null)
                    {
                        result.AddError("Gradient sky specified but gradient data is null");
                    }
                    else
                    {
                        ValidateColorArray(ref result, data.Gradient.TopColor, "Top color");
                        ValidateColorArray(ref result, data.Gradient.HorizonColor, "Horizon color");
                        ValidateColorArray(ref result, data.Gradient.BottomColor, "Bottom color");

                        if (data.Gradient.TopCurve.HasValue && data.Gradient.TopCurve.Value <= 0)
                            result.AddError("Top curve must be positive");
                        if (data.Gradient.BottomCurve.HasValue && data.Gradient.BottomCurve.Value <= 0)
                            result.AddError("Bottom curve must be positive");
                        if (data.Gradient.SunCurve.HasValue && data.Gradient.SunCurve.Value <= 0)
                            result.AddError("Sun curve must be positive");
                    }
                    break;

                case Extensions.EnvironmentSky.OMIEnvironmentSkyType.Panorama:
                    if (data.Panorama == null)
                    {
                        result.AddError("Panorama sky specified but panorama data is null");
                    }
                    else
                    {
                        if (!data.Panorama.HasCubemap && !data.Panorama.HasEquirectangular)
                        {
                            result.AddError("Panorama sky must have either cubemap or equirectangular texture");
                        }
                        if (data.Panorama.HasCubemap && data.Panorama.Cubemap.Length != 6)
                        {
                            result.AddError("Cubemap must have exactly 6 texture indices");
                        }
                    }
                    break;

                case Extensions.EnvironmentSky.OMIEnvironmentSkyType.Physical:
                    if (data.Physical == null)
                    {
                        result.AddError("Physical sky specified but physical data is null");
                    }
                    else
                    {
                        var anisotropy = data.Physical.GetMieAnisotropy();
                        if (anisotropy < -1 || anisotropy > 1)
                        {
                            result.AddError("Mie anisotropy must be between -1 and 1");
                        }

                        if (data.Physical.MieScale.HasValue && data.Physical.MieScale.Value < 0)
                            result.AddError("Mie scale must be non-negative");
                        if (data.Physical.RayleighScale.HasValue && data.Physical.RayleighScale.Value < 0)
                            result.AddError("Rayleigh scale must be non-negative");
                    }
                    break;

                case Extensions.EnvironmentSky.OMIEnvironmentSkyType.Plain:
                    if (data.Plain != null)
                    {
                        ValidateColorArray(ref result, data.Plain.Color, "Sky color");
                    }
                    break;
            }

            // Validate ambient settings
            ValidateColorArray(ref result, data.AmbientLightColor, "Ambient light color");
            
            if (data.AmbientSkyContribution.HasValue)
            {
                if (data.AmbientSkyContribution.Value < 0 || data.AmbientSkyContribution.Value > 1)
                {
                    result.AddWarning("Ambient sky contribution should be between 0 and 1");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates vehicle body data.
        /// </summary>
        public static ValidationResult ValidateVehicleBody(Extensions.Vehicle.OMIVehicleBodyNode data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Vehicle body data is null");
                return result;
            }

            // Validate angular activation if provided
            if (data.angularActivation != null && data.angularActivation.Length >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (data.angularActivation[i] < -1 || data.angularActivation[i] > 1)
                    {
                        result.AddWarning($"Angular activation[{i}] should be between -1 and 1");
                    }
                }
            }

            // Validate linear activation if provided
            if (data.linearActivation != null && data.linearActivation.Length >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (data.linearActivation[i] < -1 || data.linearActivation[i] > 1)
                    {
                        result.AddWarning($"Linear activation[{i}] should be between -1 and 1");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Validates vehicle wheel data.
        /// </summary>
        public static ValidationResult ValidateVehicleWheel(Extensions.Vehicle.OMIVehicleWheelSettings data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Vehicle wheel data is null");
                return result;
            }

            if (data.radius <= 0)
            {
                result.AddError("Wheel radius must be positive");
            }

            if (data.maxForce < 0)
            {
                result.AddError("Max force must be non-negative");
            }

            if (data.suspensionStiffness < 0)
            {
                result.AddError("Suspension stiffness must be non-negative");
            }

            if (data.suspensionDampingCompression < 0 || data.suspensionDampingRebound < 0)
            {
                result.AddError("Suspension damping must be non-negative");
            }

            if (data.suspensionTravel < 0)
            {
                result.AddError("Suspension travel must be non-negative");
            }

            return result;
        }

        /// <summary>
        /// Validates vehicle thruster data.
        /// </summary>
        public static ValidationResult ValidateVehicleThruster(Extensions.Vehicle.OMIVehicleThrusterSettings data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Vehicle thruster data is null");
                return result;
            }

            if (data.maxForce < 0)
            {
                result.AddError("Max force must be non-negative");
            }

            if (data.maxGimbal < 0)
            {
                result.AddWarning("Max gimbal angle should be non-negative");
            }

            return result;
        }

        /// <summary>
        /// Validates vehicle hover thruster data.
        /// </summary>
        public static ValidationResult ValidateVehicleHoverThruster(Extensions.Vehicle.OMIVehicleHoverThrusterSettings data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Vehicle hover thruster data is null");
                return result;
            }

            if (data.maxHoverEnergy < 0)
            {
                result.AddError("Max hover energy must be non-negative");
            }

            if (data.maxGimbal < 0)
            {
                result.AddWarning("Max gimbal angle should be non-negative");
            }

            return result;
        }

        /// <summary>
        /// Validates audio emitter data.
        /// </summary>
        public static ValidationResult ValidateAudioEmitter(Extensions.Audio.KHRAudioEmitter data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Audio emitter data is null");
                return result;
            }

            if (data.gain < 0)
            {
                result.AddError("Gain must be non-negative");
            }
            if (data.gain > 10)
            {
                result.AddWarning("Very high gain value - may cause audio clipping");
            }

            if (data.TypeEnum == Extensions.Audio.AudioEmitterType.Positional && data.positional != null)
            {
                var coneInner = data.positional.coneInnerAngle;
                var coneOuter = data.positional.coneOuterAngle;
                
                if (coneInner < 0 || coneInner > 2 * Mathf.PI)
                    result.AddError("Cone inner angle must be between 0 and 2π");
                if (coneOuter < 0 || coneOuter > 2 * Mathf.PI)
                    result.AddError("Cone outer angle must be between 0 and 2π");
                if (coneInner > coneOuter)
                    result.AddWarning("Cone inner angle is greater than outer angle");

                var refDist = data.positional.refDistance;
                var maxDist = data.positional.maxDistance;
                if (refDist < 0)
                    result.AddError("Reference distance must be non-negative");
                if (!float.IsPositiveInfinity(maxDist) && maxDist < refDist)
                    result.AddWarning("Max distance is less than reference distance");
            }

            return result;
        }

        /// <summary>
        /// Validates physics gravity data.
        /// </summary>
        public static ValidationResult ValidatePhysicsGravity(Extensions.PhysicsGravity.OMIPhysicsGravityNode data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Physics gravity data is null");
                return result;
            }

            var gravityType = data.Type;

            switch (gravityType)
            {
                case Extensions.PhysicsGravity.OMIGravityType.Point:
                    if (data.Point == null)
                    {
                        result.AddError("Point gravity specified but point data is null");
                    }
                    else
                    {
                        if (data.Point.UnitDistance <= 0)
                        {
                            result.AddWarning("Point gravity unit distance is zero or negative - gravity will be constant");
                        }
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// Validates personality data.
        /// </summary>
        public static ValidationResult ValidatePersonality(Extensions.Personality.OMIPersonalityNode data)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Personality data is null");
                return result;
            }

            if (string.IsNullOrEmpty(data.Agent))
            {
                result.AddWarning("Personality has no agent specified");
            }

            if (string.IsNullOrEmpty(data.Personality))
            {
                result.AddWarning("Personality has no personality text specified");
            }

            return result;
        }

        /// <summary>
        /// Validates physics joint data.
        /// </summary>
        public static ValidationResult ValidatePhysicsJoint(Extensions.PhysicsJoint.OMIPhysicsJointNode data, int maxNodeIndex = int.MaxValue)
        {
            var result = ValidationResult.Success();

            if (data == null)
            {
                result.AddError("Physics joint data is null");
                return result;
            }

            if (data.ConnectedNode < 0)
            {
                result.AddError("Connected node index must be non-negative");
            }
            else if (data.ConnectedNode > maxNodeIndex)
            {
                result.AddError($"Connected node index {data.ConnectedNode} exceeds maximum node index {maxNodeIndex}");
            }

            if (data.Joint < 0)
            {
                result.AddError("Joint settings index must be non-negative");
            }

            return result;
        }

        private static void ValidateColorArray(ref ValidationResult result, float[] color, string name)
        {
            if (color == null) return;

            if (color.Length < 3)
            {
                result.AddError($"{name} must have at least 3 components (RGB)");
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                if (color[i] < 0 || color[i] > 1)
                {
                    result.AddWarning($"{name} component {i} is outside 0-1 range (value: {color[i]})");
                }
            }
        }
    }
}
