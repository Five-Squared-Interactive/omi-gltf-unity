// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.PhysicsGravity
{
    /// <summary>
    /// Component representing an OMI_physics_gravity gravity volume.
    /// Attach to a GameObject with a trigger collider to define a gravity zone.
    /// </summary>
    [AddComponentMenu("OMI/Physics Gravity")]
    public class OMIPhysicsGravity : MonoBehaviour
    {
        [Header("Gravity Settings")]
        [Tooltip("The type of gravity for this volume.")]
        public GravityType Type = GravityType.Directional;

        [Tooltip("The gravity strength in m/s². Can be negative for repulsion.")]
        public float Gravity = 9.80665f;

        [Tooltip("Processing priority. Higher values are processed first.")]
        public int Priority = 0;

        [Tooltip("If true, replace accumulated gravity instead of adding to it.")]
        public bool Replace = false;

        [Tooltip("If true, stop processing lower priority gravity volumes.")]
        public bool Stop = false;

        [Header("Directional")]
        [Tooltip("Direction of gravity relative to this transform (for directional type).")]
        public Vector3 Direction = Vector3.down;

        [Header("Point/Disc/Torus/Line/Shaped")]
        [Tooltip("Distance at which gravity equals the Gravity property. 0 = constant.")]
        public float UnitDistance = 0f;

        [Header("Disc/Torus")]
        [Tooltip("Radius of the circle on the XZ plane.")]
        public float Radius = 1f;

        [Header("Line")]
        [Tooltip("Points defining line segments for line gravity.")]
        public Vector3[] LinePoints;

        [Header("Shaped")]
        [Tooltip("Reference to a collider that defines the gravity shape.")]
        public Collider ShapeCollider;

        /// <summary>
        /// Gravity type enumeration matching OMI_physics_gravity types.
        /// </summary>
        public enum GravityType
        {
            Directional,
            Point,
            Disc,
            Torus,
            Line,
            Shaped
        }

        /// <summary>
        /// Calculates the gravity vector at a given world position.
        /// </summary>
        /// <param name="worldPosition">The position to calculate gravity at.</param>
        /// <returns>The gravity vector in world space.</returns>
        public Vector3 CalculateGravityAt(Vector3 worldPosition)
        {
            Vector3 direction;
            float magnitude = Gravity;

            switch (Type)
            {
                case GravityType.Directional:
                    direction = transform.TransformDirection(Direction.normalized);
                    break;

                case GravityType.Point:
                    direction = (transform.position - worldPosition).normalized;
                    if (UnitDistance > 0)
                    {
                        float distance = Vector3.Distance(worldPosition, transform.position);
                        float ratio = UnitDistance / Mathf.Max(distance, 0.001f);
                        magnitude *= ratio * ratio; // Inverse square law
                    }
                    break;

                case GravityType.Disc:
                    direction = CalculateDiscGravity(worldPosition, out float discDistance);
                    if (UnitDistance > 0)
                    {
                        float ratio = UnitDistance / Mathf.Max(discDistance, 0.001f);
                        magnitude *= ratio * ratio;
                    }
                    break;

                case GravityType.Torus:
                    direction = CalculateTorusGravity(worldPosition, out float torusDistance);
                    if (UnitDistance > 0)
                    {
                        float ratio = UnitDistance / Mathf.Max(torusDistance, 0.001f);
                        magnitude *= ratio * ratio;
                    }
                    break;

                case GravityType.Line:
                    direction = CalculateLineGravity(worldPosition, out float lineDistance);
                    if (UnitDistance > 0)
                    {
                        float ratio = UnitDistance / Mathf.Max(lineDistance, 0.001f);
                        magnitude *= ratio * ratio;
                    }
                    break;

                case GravityType.Shaped:
                    if (ShapeCollider != null)
                    {
                        Vector3 closestPoint = ShapeCollider.ClosestPoint(worldPosition);
                        direction = (closestPoint - worldPosition).normalized;
                        if (UnitDistance > 0)
                        {
                            float distance = Vector3.Distance(worldPosition, closestPoint);
                            float ratio = UnitDistance / Mathf.Max(distance, 0.001f);
                            magnitude *= ratio * ratio;
                        }
                    }
                    else
                    {
                        direction = Vector3.down;
                    }
                    break;

                default:
                    direction = Vector3.down;
                    break;
            }

            return direction * magnitude;
        }

        private Vector3 CalculateDiscGravity(Vector3 worldPosition, out float distance)
        {
            // Transform to local space
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);
            
            // Project onto XZ plane
            Vector2 xzPos = new Vector2(localPos.x, localPos.z);
            float xzDistance = xzPos.magnitude;
            
            // Find closest point on filled circle
            Vector3 closestPoint;
            if (xzDistance <= Radius)
            {
                // Inside the disc - closest point is directly below/above on the disc plane
                closestPoint = new Vector3(localPos.x, 0, localPos.z);
            }
            else
            {
                // Outside the disc - closest point is on the edge
                Vector2 normalizedXZ = xzPos.normalized;
                closestPoint = new Vector3(normalizedXZ.x * Radius, 0, normalizedXZ.y * Radius);
            }

            Vector3 worldClosest = transform.TransformPoint(closestPoint);
            distance = Vector3.Distance(worldPosition, worldClosest);
            return (worldClosest - worldPosition).normalized;
        }

        private Vector3 CalculateTorusGravity(Vector3 worldPosition, out float distance)
        {
            // Transform to local space
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);
            
            // Project onto XZ plane and find point on ring
            Vector2 xzPos = new Vector2(localPos.x, localPos.z);
            float xzDistance = xzPos.magnitude;
            
            Vector3 closestPoint;
            if (xzDistance < 0.001f)
            {
                // Directly on Y axis - pick arbitrary point on ring
                closestPoint = new Vector3(Radius, 0, 0);
            }
            else
            {
                Vector2 normalizedXZ = xzPos.normalized;
                closestPoint = new Vector3(normalizedXZ.x * Radius, 0, normalizedXZ.y * Radius);
            }

            Vector3 worldClosest = transform.TransformPoint(closestPoint);
            distance = Vector3.Distance(worldPosition, worldClosest);
            return (worldClosest - worldPosition).normalized;
        }

        private Vector3 CalculateLineGravity(Vector3 worldPosition, out float distance)
        {
            distance = float.MaxValue;
            Vector3 closestPoint = transform.position;

            if (LinePoints == null || LinePoints.Length < 2)
            {
                distance = Vector3.Distance(worldPosition, closestPoint);
                return (closestPoint - worldPosition).normalized;
            }

            // Check each line segment
            for (int i = 0; i < LinePoints.Length - 1; i++)
            {
                Vector3 a = transform.TransformPoint(LinePoints[i]);
                Vector3 b = transform.TransformPoint(LinePoints[i + 1]);
                
                Vector3 pointOnSegment = ClosestPointOnLineSegment(worldPosition, a, b);
                float segmentDistance = Vector3.Distance(worldPosition, pointOnSegment);
                
                if (segmentDistance < distance)
                {
                    distance = segmentDistance;
                    closestPoint = pointOnSegment;
                }
            }

            return (closestPoint - worldPosition).normalized;
        }

        private static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab));
            return a + t * ab;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.5f);
            
            switch (Type)
            {
                case GravityType.Directional:
                    Gizmos.DrawRay(transform.position, transform.TransformDirection(Direction.normalized) * 2f);
                    break;

                case GravityType.Point:
                    Gizmos.DrawWireSphere(transform.position, UnitDistance > 0 ? UnitDistance : 1f);
                    break;

                case GravityType.Disc:
                case GravityType.Torus:
                    DrawWireCircle(transform.position, transform.up, Radius);
                    break;

                case GravityType.Line:
                    if (LinePoints != null && LinePoints.Length >= 2)
                    {
                        for (int i = 0; i < LinePoints.Length - 1; i++)
                        {
                            Gizmos.DrawLine(
                                transform.TransformPoint(LinePoints[i]),
                                transform.TransformPoint(LinePoints[i + 1])
                            );
                        }
                    }
                    break;
            }
        }

        private void DrawWireCircle(Vector3 center, Vector3 normal, float radius)
        {
            const int segments = 32;
            Vector3 tangent = Vector3.Cross(normal, Vector3.up);
            if (tangent.magnitude < 0.001f)
                tangent = Vector3.Cross(normal, Vector3.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);

            Vector3 prevPoint = center + tangent * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 point = center + (Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent) * radius;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }
    }
}
