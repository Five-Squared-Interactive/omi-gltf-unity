// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.PhysicsGravity
{
    /// <summary>
    /// Extension name constant for OMI_physics_gravity.
    /// </summary>
    public static class OMIPhysicsGravityExtension
    {
        public const string ExtensionName = "OMI_physics_gravity";
    }

    /// <summary>
    /// Document-level OMI_physics_gravity extension data.
    /// Defines global world gravity.
    /// </summary>
    [Serializable]
    public class OMIPhysicsGravityRoot
    {
        /// <summary>
        /// The gravity amount in meters per second squared.
        /// Can be zero or negative. Required.
        /// </summary>
        [JsonProperty("gravity")]
        public float Gravity;

        /// <summary>
        /// The normalized direction of the gravity.
        /// World gravity is always directional.
        /// Default: [0, -1, 0]
        /// </summary>
        [JsonProperty("direction")]
        public float[] Direction = new float[] { 0f, -1f, 0f };
    }

    /// <summary>
    /// Node-level OMI_physics_gravity extension data.
    /// Defines a gravity volume on a trigger.
    /// </summary>
    [Serializable]
    public class OMIPhysicsGravityNode
    {
        /// <summary>
        /// The type of gravity: "directional", "point", "disc", "torus", "line", or "shaped".
        /// Required.
        /// </summary>
        [JsonProperty("type")]
        public string Type;

        /// <summary>
        /// The gravity amount in meters per second squared.
        /// Can be zero or negative. Required.
        /// </summary>
        [JsonProperty("gravity")]
        public float Gravity;

        /// <summary>
        /// The process priority of this gravity node. Higher is processed first.
        /// Default: 0
        /// </summary>
        [JsonProperty("priority")]
        public int Priority = 0;

        /// <summary>
        /// If true, replace the current gravity instead of adding to it.
        /// Default: false
        /// </summary>
        [JsonProperty("replace")]
        public bool Replace = false;

        /// <summary>
        /// If true, stop checking more nodes for gravity.
        /// Default: false
        /// </summary>
        [JsonProperty("stop")]
        public bool Stop = false;

        /// <summary>
        /// Directional gravity parameters.
        /// </summary>
        [JsonProperty("directional")]
        public OMIGravityDirectional Directional;

        /// <summary>
        /// Point gravity parameters.
        /// </summary>
        [JsonProperty("point")]
        public OMIGravityPoint Point;

        /// <summary>
        /// Disc gravity parameters.
        /// </summary>
        [JsonProperty("disc")]
        public OMIGravityDisc Disc;

        /// <summary>
        /// Torus gravity parameters.
        /// </summary>
        [JsonProperty("torus")]
        public OMIGravityTorus Torus;

        /// <summary>
        /// Line gravity parameters.
        /// </summary>
        [JsonProperty("line")]
        public OMIGravityLine Line;

        /// <summary>
        /// Shaped gravity parameters.
        /// </summary>
        [JsonProperty("shaped")]
        public OMIGravityShaped Shaped;
    }

    /// <summary>
    /// Gravity type constants.
    /// </summary>
    public static class OMIGravityType
    {
        public const string Directional = "directional";
        public const string Point = "point";
        public const string Disc = "disc";
        public const string Torus = "torus";
        public const string Line = "line";
        public const string Shaped = "shaped";
    }

    /// <summary>
    /// Directional gravity parameters.
    /// </summary>
    [Serializable]
    public class OMIGravityDirectional
    {
        /// <summary>
        /// The normalized direction of the gravity relative to node's transform.
        /// Default: [0, -1, 0]
        /// </summary>
        [JsonProperty("direction")]
        public float[] Direction = new float[] { 0f, -1f, 0f };
    }

    /// <summary>
    /// Point gravity parameters.
    /// </summary>
    [Serializable]
    public class OMIGravityPoint
    {
        /// <summary>
        /// The distance at which the gravity equals the gravity property.
        /// If 0, gravity is constant regardless of distance.
        /// Default: 0
        /// </summary>
        [JsonProperty("unitDistance")]
        public float UnitDistance = 0f;
    }

    /// <summary>
    /// Disc gravity parameters.
    /// </summary>
    [Serializable]
    public class OMIGravityDisc
    {
        /// <summary>
        /// The radius of the circle on the XZ plane.
        /// Default: 1.0
        /// </summary>
        [JsonProperty("radius")]
        public float Radius = 1f;

        /// <summary>
        /// The unit distance for gravity falloff.
        /// Default: 0
        /// </summary>
        [JsonProperty("unitDistance")]
        public float UnitDistance = 0f;
    }

    /// <summary>
    /// Torus gravity parameters.
    /// </summary>
    [Serializable]
    public class OMIGravityTorus
    {
        /// <summary>
        /// The radius of the hollow circle on the XZ plane.
        /// Default: 1.0
        /// </summary>
        [JsonProperty("radius")]
        public float Radius = 1f;

        /// <summary>
        /// The unit distance for gravity falloff.
        /// Default: 0
        /// </summary>
        [JsonProperty("unitDistance")]
        public float UnitDistance = 0f;
    }

    /// <summary>
    /// Line gravity parameters.
    /// </summary>
    [Serializable]
    public class OMIGravityLine
    {
        /// <summary>
        /// The points that make up the line segments.
        /// Size must be a multiple of 3 with at least 6 numbers.
        /// </summary>
        [JsonProperty("points")]
        public float[] Points;

        /// <summary>
        /// The unit distance for gravity falloff.
        /// Default: 0
        /// </summary>
        [JsonProperty("unitDistance")]
        public float UnitDistance = 0f;
    }

    /// <summary>
    /// Shaped gravity parameters.
    /// </summary>
    [Serializable]
    public class OMIGravityShaped
    {
        /// <summary>
        /// Index of the physics shape used to define gravity direction.
        /// Default: -1 (invalid)
        /// </summary>
        [JsonProperty("shape")]
        public int Shape = -1;

        /// <summary>
        /// The unit distance for gravity falloff.
        /// Default: 0
        /// </summary>
        [JsonProperty("unitDistance")]
        public float UnitDistance = 0f;
    }
}
