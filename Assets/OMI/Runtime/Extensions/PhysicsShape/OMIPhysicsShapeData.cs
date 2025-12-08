// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.PhysicsShape
{
    /// <summary>
    /// Extension name constant for OMI_physics_shape.
    /// </summary>
    public static class OMIPhysicsShapeExtension
    {
        public const string ExtensionName = "OMI_physics_shape";
    }

    /// <summary>
    /// Document-level OMI_physics_shape extension data.
    /// Contains an array of shape resources that can be referenced by nodes.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeRoot
    {
        /// <summary>
        /// An array of physics shape resources that can be referenced by nodes.
        /// </summary>
        [JsonProperty("shapes")]
        public OMIPhysicsShape[] Shapes;
    }

    /// <summary>
    /// A physics shape resource.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShape
    {
        /// <summary>
        /// The type of physics shape.
        /// </summary>
        [JsonProperty("type")]
        public string Type;

        /// <summary>
        /// Box shape parameters. Only present if type is "box".
        /// </summary>
        [JsonProperty("box")]
        public OMIPhysicsShapeBox Box;

        /// <summary>
        /// Sphere shape parameters. Only present if type is "sphere".
        /// </summary>
        [JsonProperty("sphere")]
        public OMIPhysicsShapeSphere Sphere;

        /// <summary>
        /// Capsule shape parameters. Only present if type is "capsule".
        /// </summary>
        [JsonProperty("capsule")]
        public OMIPhysicsShapeCapsule Capsule;

        /// <summary>
        /// Cylinder shape parameters. Only present if type is "cylinder".
        /// </summary>
        [JsonProperty("cylinder")]
        public OMIPhysicsShapeCylinder Cylinder;

        /// <summary>
        /// Convex shape parameters. Only present if type is "convex".
        /// </summary>
        [JsonProperty("convex")]
        public OMIPhysicsShapeConvex Convex;

        /// <summary>
        /// Trimesh shape parameters. Only present if type is "trimesh".
        /// </summary>
        [JsonProperty("trimesh")]
        public OMIPhysicsShapeTrimesh Trimesh;
    }

    /// <summary>
    /// Shape type constants.
    /// </summary>
    public static class OMIPhysicsShapeType
    {
        public const string Box = "box";
        public const string Sphere = "sphere";
        public const string Capsule = "capsule";
        public const string Cylinder = "cylinder";
        public const string Convex = "convex";
        public const string Trimesh = "trimesh";
    }

    /// <summary>
    /// Box shape parameters.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeBox
    {
        /// <summary>
        /// The size of the box (width, height, depth).
        /// Default: [1.0, 1.0, 1.0]
        /// </summary>
        [JsonProperty("size")]
        public float[] Size = new float[] { 1.0f, 1.0f, 1.0f };
    }

    /// <summary>
    /// Sphere shape parameters.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeSphere
    {
        /// <summary>
        /// The radius of the sphere in meters.
        /// Default: 0.5
        /// </summary>
        [JsonProperty("radius")]
        public float Radius = 0.5f;
    }

    /// <summary>
    /// Capsule shape parameters.
    /// Capsule is aligned along the Y axis.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeCapsule
    {
        /// <summary>
        /// The height of the capsule's middle cylinder section in meters.
        /// Default: 1.0
        /// </summary>
        [JsonProperty("height")]
        public float Height = 1.0f;

        /// <summary>
        /// The radius of the bottom hemisphere in meters.
        /// Default: 0.5
        /// </summary>
        [JsonProperty("radiusBottom")]
        public float RadiusBottom = 0.5f;

        /// <summary>
        /// The radius of the top hemisphere in meters.
        /// Default: 0.5
        /// </summary>
        [JsonProperty("radiusTop")]
        public float RadiusTop = 0.5f;
    }

    /// <summary>
    /// Cylinder shape parameters.
    /// Cylinder is aligned along the Y axis.
    /// Note: Cylinder support is limited in some physics engines.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeCylinder
    {
        /// <summary>
        /// The height of the cylinder in meters.
        /// Default: 2.0
        /// </summary>
        [JsonProperty("height")]
        public float Height = 2.0f;

        /// <summary>
        /// The radius of the bottom disc in meters.
        /// Default: 0.5
        /// </summary>
        [JsonProperty("radiusBottom")]
        public float RadiusBottom = 0.5f;

        /// <summary>
        /// The radius of the top disc in meters.
        /// Default: 0.5
        /// </summary>
        [JsonProperty("radiusTop")]
        public float RadiusTop = 0.5f;
    }

    /// <summary>
    /// Convex hull shape parameters.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeConvex
    {
        /// <summary>
        /// Index of the mesh in the glTF meshes array to use for the convex hull.
        /// </summary>
        [JsonProperty("mesh")]
        public int Mesh = -1;
    }

    /// <summary>
    /// Triangle mesh shape parameters.
    /// Used for concave static geometry.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeTrimesh
    {
        /// <summary>
        /// Index of the mesh in the glTF meshes array to use for the trimesh.
        /// </summary>
        [JsonProperty("mesh")]
        public int Mesh = -1;
    }
}
