// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.PhysicsJoint
{
    /// <summary>
    /// Extension name constant for OMI_physics_joint.
    /// </summary>
    public static class OMIPhysicsJointExtension
    {
        public const string ExtensionName = "OMI_physics_joint";
    }

    /// <summary>
    /// Document-level OMI_physics_joint extension data.
    /// Contains an array of joint settings resources.
    /// </summary>
    [Serializable]
    public class OMIPhysicsJointRoot
    {
        /// <summary>
        /// An array of physics joint settings resources.
        /// </summary>
        [JsonProperty("physicsJoints")]
        public OMIPhysicsJointSettings[] PhysicsJoints;
    }

    /// <summary>
    /// Node-level OMI_physics_joint extension data.
    /// </summary>
    [Serializable]
    public class OMIPhysicsJointNode
    {
        /// <summary>
        /// Index of the node this joint is connected to.
        /// Required.
        /// </summary>
        [JsonProperty("connectedNode")]
        public int ConnectedNode = -1;

        /// <summary>
        /// Index of the joint settings in the physicsJoints array.
        /// Required.
        /// </summary>
        [JsonProperty("joint")]
        public int Joint = -1;

        /// <summary>
        /// Whether to allow connected objects to collide.
        /// Default: false
        /// </summary>
        [JsonProperty("enableCollision")]
        public bool EnableCollision = false;
    }

    /// <summary>
    /// Joint settings resource.
    /// </summary>
    [Serializable]
    public class OMIPhysicsJointSettings
    {
        /// <summary>
        /// Array of joint limits constraining relative motion.
        /// </summary>
        [JsonProperty("limits")]
        public OMIPhysicsJointLimit[] Limits;

        /// <summary>
        /// Array of joint drives applying forces.
        /// </summary>
        [JsonProperty("drives")]
        public OMIPhysicsJointDrive[] Drives;
    }

    /// <summary>
    /// Joint limit constraining relative motion.
    /// </summary>
    [Serializable]
    public class OMIPhysicsJointLimit
    {
        /// <summary>
        /// Linear axes constrained (0=X, 1=Y, 2=Z).
        /// </summary>
        [JsonProperty("linearAxes")]
        public int[] LinearAxes;

        /// <summary>
        /// Angular axes constrained (0=X, 1=Y, 2=Z).
        /// </summary>
        [JsonProperty("angularAxes")]
        public int[] AngularAxes;

        /// <summary>
        /// Minimum allowed value (meters or radians).
        /// </summary>
        [JsonProperty("min")]
        public float? Min;

        /// <summary>
        /// Maximum allowed value (meters or radians).
        /// </summary>
        [JsonProperty("max")]
        public float? Max;

        /// <summary>
        /// Stiffness for soft limits.
        /// </summary>
        [JsonProperty("stiffness")]
        public float? Stiffness;

        /// <summary>
        /// Damping for soft limits.
        /// Default: 0.0
        /// </summary>
        [JsonProperty("damping")]
        public float Damping = 0.0f;
    }

    /// <summary>
    /// Joint drive for motors and springs.
    /// </summary>
    [Serializable]
    public class OMIPhysicsJointDrive
    {
        /// <summary>
        /// Drive type: "linear" or "angular".
        /// Required.
        /// </summary>
        [JsonProperty("type")]
        public string Type;

        /// <summary>
        /// Force calculation mode: "force" or "acceleration".
        /// Required.
        /// </summary>
        [JsonProperty("mode")]
        public string Mode;

        /// <summary>
        /// Axis index (0=X, 1=Y, 2=Z).
        /// Required.
        /// </summary>
        [JsonProperty("axis")]
        public int Axis;

        /// <summary>
        /// Maximum force/torque the drive can apply.
        /// </summary>
        [JsonProperty("maxForce")]
        public float? MaxForce;

        /// <summary>
        /// Target position/angle.
        /// </summary>
        [JsonProperty("positionTarget")]
        public float? PositionTarget;

        /// <summary>
        /// Target velocity.
        /// </summary>
        [JsonProperty("velocityTarget")]
        public float? VelocityTarget;

        /// <summary>
        /// Stiffness for position-based drives.
        /// Default: 0.0
        /// </summary>
        [JsonProperty("stiffness")]
        public float Stiffness = 0.0f;

        /// <summary>
        /// Damping for velocity-based drives.
        /// Default: 0.0
        /// </summary>
        [JsonProperty("damping")]
        public float Damping = 0.0f;
    }

    /// <summary>
    /// Drive type constants.
    /// </summary>
    public static class OMIJointDriveType
    {
        public const string Linear = "linear";
        public const string Angular = "angular";
    }

    /// <summary>
    /// Drive mode constants.
    /// </summary>
    public static class OMIJointDriveMode
    {
        public const string Force = "force";
        public const string Acceleration = "acceleration";
    }
}
