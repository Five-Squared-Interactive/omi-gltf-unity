// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.PhysicsBody
{
    /// <summary>
    /// Extension name constant for OMI_physics_body.
    /// </summary>
    public static class OMIPhysicsBodyExtension
    {
        public const string ExtensionName = "OMI_physics_body";
    }

    /// <summary>
    /// Document-level OMI_physics_body extension data.
    /// Contains arrays of physics materials and collision filters.
    /// </summary>
    [Serializable]
    public class OMIPhysicsBodyRoot
    {
        /// <summary>
        /// An array of physics material resources.
        /// </summary>
        [JsonProperty("physicsMaterials")]
        public OMIPhysicsMaterial[] PhysicsMaterials;

        /// <summary>
        /// An array of collision filter resources.
        /// </summary>
        [JsonProperty("collisionFilters")]
        public OMICollisionFilter[] CollisionFilters;
    }

    /// <summary>
    /// Node-level OMI_physics_body extension data.
    /// </summary>
    [Serializable]
    public class OMIPhysicsBodyNode
    {
        /// <summary>
        /// Motion properties for physics simulation.
        /// </summary>
        [JsonProperty("motion")]
        public OMIPhysicsBodyMotion Motion;

        /// <summary>
        /// Collider properties for solid collision detection.
        /// </summary>
        [JsonProperty("collider")]
        public OMIPhysicsBodyCollider Collider;

        /// <summary>
        /// Trigger properties for overlap detection.
        /// </summary>
        [JsonProperty("trigger")]
        public OMIPhysicsBodyTrigger Trigger;
    }

    /// <summary>
    /// Motion type constants.
    /// </summary>
    public static class OMIPhysicsMotionType
    {
        /// <summary>
        /// Static bodies do not move.
        /// </summary>
        public const string Static = "static";

        /// <summary>
        /// Kinematic bodies are moved by code, not physics.
        /// </summary>
        public const string Kinematic = "kinematic";

        /// <summary>
        /// Dynamic bodies are fully simulated by physics.
        /// </summary>
        public const string Dynamic = "dynamic";
    }

    /// <summary>
    /// Motion properties for a physics body.
    /// </summary>
    [Serializable]
    public class OMIPhysicsBodyMotion
    {
        /// <summary>
        /// The motion type: "static", "kinematic", or "dynamic".
        /// </summary>
        [JsonProperty("type")]
        public string Type;

        /// <summary>
        /// The mass of the body in kilograms.
        /// Default: 1.0
        /// </summary>
        [JsonProperty("mass")]
        public float Mass = 1.0f;

        /// <summary>
        /// The center of mass in local space [x, y, z].
        /// Default: [0, 0, 0]
        /// </summary>
        [JsonProperty("centerOfMass")]
        public float[] CenterOfMass;

        /// <summary>
        /// The inertia tensor diagonal [x, y, z].
        /// If zero or not specified, calculated automatically.
        /// </summary>
        [JsonProperty("inertiaDiagonal")]
        public float[] InertiaDiagonal;

        /// <summary>
        /// The inertia orientation as a quaternion [x, y, z, w].
        /// </summary>
        [JsonProperty("inertiaOrientation")]
        public float[] InertiaOrientation;

        /// <summary>
        /// Initial linear velocity [x, y, z] in m/s.
        /// </summary>
        [JsonProperty("linearVelocity")]
        public float[] LinearVelocity;

        /// <summary>
        /// Initial angular velocity [x, y, z] in rad/s.
        /// </summary>
        [JsonProperty("angularVelocity")]
        public float[] AngularVelocity;

        /// <summary>
        /// Multiplier for gravity acceleration.
        /// Default: 1.0
        /// </summary>
        [JsonProperty("gravityFactor")]
        public float GravityFactor = 1.0f;
    }

    /// <summary>
    /// Collider properties for a physics body.
    /// </summary>
    [Serializable]
    public class OMIPhysicsBodyCollider
    {
        /// <summary>
        /// Index of the shape in the OMI_physics_shape shapes array.
        /// </summary>
        [JsonProperty("shape")]
        public int Shape = -1;

        /// <summary>
        /// Index of the physics material in the physicsMaterials array.
        /// </summary>
        [JsonProperty("physicsMaterial")]
        public int PhysicsMaterial = -1;

        /// <summary>
        /// Index of the collision filter in the collisionFilters array.
        /// </summary>
        [JsonProperty("collisionFilter")]
        public int CollisionFilter = -1;
    }

    /// <summary>
    /// Trigger properties for a physics body.
    /// </summary>
    [Serializable]
    public class OMIPhysicsBodyTrigger
    {
        /// <summary>
        /// Index of the shape in the OMI_physics_shape shapes array.
        /// </summary>
        [JsonProperty("shape")]
        public int Shape = -1;

        /// <summary>
        /// For compound triggers, the set of descendant node indices.
        /// </summary>
        [JsonProperty("nodes")]
        public int[] Nodes;

        /// <summary>
        /// Index of the collision filter in the collisionFilters array.
        /// </summary>
        [JsonProperty("collisionFilter")]
        public int CollisionFilter = -1;
    }

    /// <summary>
    /// Physics material properties.
    /// </summary>
    [Serializable]
    public class OMIPhysicsMaterial
    {
        /// <summary>
        /// Static friction coefficient.
        /// Default: 0.6
        /// </summary>
        [JsonProperty("staticFriction")]
        public float StaticFriction = 0.6f;

        /// <summary>
        /// Dynamic friction coefficient.
        /// Default: 0.6
        /// </summary>
        [JsonProperty("dynamicFriction")]
        public float DynamicFriction = 0.6f;

        /// <summary>
        /// Restitution (bounciness).
        /// Default: 0.0
        /// </summary>
        [JsonProperty("restitution")]
        public float Restitution = 0.0f;

        /// <summary>
        /// How to combine friction values: "average", "minimum", "maximum", "multiply".
        /// </summary>
        [JsonProperty("frictionCombine")]
        public string FrictionCombine;

        /// <summary>
        /// How to combine restitution values: "average", "minimum", "maximum", "multiply".
        /// </summary>
        [JsonProperty("restitutionCombine")]
        public string RestitutionCombine;
    }

    /// <summary>
    /// Collision filter for controlling which objects collide.
    /// </summary>
    [Serializable]
    public class OMICollisionFilter
    {
        /// <summary>
        /// Collision systems this object belongs to.
        /// </summary>
        [JsonProperty("collisionSystems")]
        public string[] CollisionSystems;

        /// <summary>
        /// Collision systems this object can collide with.
        /// </summary>
        [JsonProperty("collideWithSystems")]
        public string[] CollideWithSystems;

        /// <summary>
        /// Collision systems this object cannot collide with.
        /// </summary>
        [JsonProperty("notCollideWithSystems")]
        public string[] NotCollideWithSystems;
    }

    /// <summary>
    /// Combine mode constants.
    /// </summary>
    public static class OMICombineMode
    {
        public const string Average = "average";
        public const string Minimum = "minimum";
        public const string Maximum = "maximum";
        public const string Multiply = "multiply";
    }
}
