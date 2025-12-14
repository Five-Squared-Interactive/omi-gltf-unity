// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.Vehicle
{
    /// <summary>
    /// Interface for vehicle body components.
    /// </summary>
    public interface IOMIVehicleBody
    {
        /// <summary>
        /// Gets or sets the angular activation input (pitch, yaw, roll).
        /// </summary>
        Vector3 AngularActivation { get; set; }

        /// <summary>
        /// Gets or sets the linear activation input (strafe, vertical, forward).
        /// </summary>
        Vector3 LinearActivation { get; set; }

        /// <summary>
        /// Gets the gyroscope torque available.
        /// </summary>
        Vector3 GyroTorque { get; }

        /// <summary>
        /// Gets the maximum speed (-1 for no limit).
        /// </summary>
        float MaxSpeed { get; }

        /// <summary>
        /// Gets whether angular dampeners are enabled.
        /// </summary>
        bool AngularDampeners { get; }

        /// <summary>
        /// Gets whether linear dampeners are enabled.
        /// </summary>
        bool LinearDampeners { get; }

        /// <summary>
        /// Gets whether the vehicle uses throttle mode.
        /// </summary>
        bool UseThrottle { get; }

        /// <summary>
        /// Gets the pilot seat component, if any.
        /// </summary>
        Component PilotSeat { get; }
    }

    /// <summary>
    /// Interface for vehicle wheel components.
    /// </summary>
    public interface IOMIVehicleWheel
    {
        /// <summary>
        /// Gets or sets the current force ratio (0 to 1).
        /// </summary>
        float CurrentForceRatio { get; set; }

        /// <summary>
        /// Gets or sets the current steering ratio (-1 to 1).
        /// </summary>
        float CurrentSteeringRatio { get; set; }

        /// <summary>
        /// Gets the maximum thrust force in Newtons.
        /// </summary>
        float MaxForce { get; }

        /// <summary>
        /// Gets the maximum steering angle in radians.
        /// </summary>
        float MaxSteeringAngle { get; }

        /// <summary>
        /// Gets the wheel radius in meters.
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// Gets the wheel width in meters.
        /// </summary>
        float Width { get; }

        /// <summary>
        /// Gets the suspension stiffness.
        /// </summary>
        float SuspensionStiffness { get; }

        /// <summary>
        /// Gets the suspension travel distance.
        /// </summary>
        float SuspensionTravel { get; }
    }

    /// <summary>
    /// Interface for vehicle thruster components.
    /// </summary>
    public interface IOMIVehicleThruster
    {
        /// <summary>
        /// Gets or sets the current force ratio (0 to 1).
        /// </summary>
        float CurrentForceRatio { get; set; }

        /// <summary>
        /// Gets or sets the current gimbal ratio (X, Y each -1 to 1).
        /// </summary>
        Vector2 CurrentGimbalRatio { get; set; }

        /// <summary>
        /// Gets the maximum thrust force in Newtons.
        /// </summary>
        float MaxForce { get; }

        /// <summary>
        /// Gets the maximum gimbal angle in radians.
        /// </summary>
        float MaxGimbal { get; }

        /// <summary>
        /// Gets the current thrust direction (local space).
        /// </summary>
        Vector3 ThrustDirection { get; }

        /// <summary>
        /// Applies the current gimbal rotation to the transform.
        /// </summary>
        void ApplyGimbal();
    }

    /// <summary>
    /// Interface for vehicle hover thruster components.
    /// </summary>
    public interface IOMIVehicleHoverThruster
    {
        /// <summary>
        /// Gets or sets the current hover ratio (0 to 1).
        /// </summary>
        float CurrentHoverRatio { get; set; }

        /// <summary>
        /// Gets or sets the current gimbal ratio (X, Y each -1 to 1).
        /// </summary>
        Vector2 CurrentGimbalRatio { get; set; }

        /// <summary>
        /// Gets the maximum hover energy in Newton-meters.
        /// </summary>
        float MaxHoverEnergy { get; }

        /// <summary>
        /// Gets the maximum gimbal angle in radians.
        /// </summary>
        float MaxGimbal { get; }

        /// <summary>
        /// Gets the current distance to ground.
        /// </summary>
        float DistanceToGround { get; }

        /// <summary>
        /// Gets the current effective force based on hover energy and distance.
        /// </summary>
        float CurrentForce { get; }

        /// <summary>
        /// Applies the current gimbal rotation to the transform.
        /// </summary>
        void ApplyGimbal();
    }
}
