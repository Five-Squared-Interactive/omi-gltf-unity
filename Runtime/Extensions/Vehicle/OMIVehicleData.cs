// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace OMI.Extensions.Vehicle
{
    /// <summary>
    /// Node-level extension data for OMI_vehicle_body.
    /// </summary>
    [Serializable]
    public class OMIVehicleBodyNode
    {
        /// <summary>
        /// The input value controlling the ratio of the vehicle's angular forces.
        /// Values between -1.0 and 1.0 for each axis (X=pitch, Y=yaw, Z=roll).
        /// </summary>
        public float[] angularActivation;

        /// <summary>
        /// The input value controlling the ratio of the vehicle's linear forces.
        /// Values between -1.0 and 1.0 for each axis (X=strafe, Y=vertical, Z=forward).
        /// </summary>
        public float[] linearActivation;

        /// <summary>
        /// The gyroscope torque intrinsic to the vehicle in Newton-meters per radian.
        /// </summary>
        public float[] gyroTorque;

        /// <summary>
        /// The speed at which the vehicle should stop driving acceleration further.
        /// -1 means no limit.
        /// </summary>
        public float maxSpeed = -1.0f;

        /// <summary>
        /// The index of the OMI_seat glTF node to use as the pilot seat.
        /// -1 means no pilot seat.
        /// </summary>
        public int pilotSeat = -1;

        /// <summary>
        /// Whether the vehicle should slow its rotation when not given angular activation.
        /// </summary>
        public bool angularDampeners = true;

        /// <summary>
        /// Whether the vehicle should slow down when not given linear activation.
        /// </summary>
        public bool linearDampeners = true;

        /// <summary>
        /// Whether the vehicle should use a throttle for linear movement.
        /// </summary>
        public bool useThrottle = false;

        /// <summary>
        /// Gets the angular activation as a Vector3.
        /// </summary>
        public Vector3 AngularActivationVector =>
            angularActivation != null && angularActivation.Length >= 3
                ? new Vector3(angularActivation[0], angularActivation[1], angularActivation[2])
                : Vector3.zero;

        /// <summary>
        /// Gets the linear activation as a Vector3.
        /// </summary>
        public Vector3 LinearActivationVector =>
            linearActivation != null && linearActivation.Length >= 3
                ? new Vector3(linearActivation[0], linearActivation[1], linearActivation[2])
                : Vector3.zero;

        /// <summary>
        /// Gets the gyro torque as a Vector3.
        /// </summary>
        public Vector3 GyroTorqueVector =>
            gyroTorque != null && gyroTorque.Length >= 3
                ? new Vector3(gyroTorque[0], gyroTorque[1], gyroTorque[2])
                : Vector3.zero;
    }

    /// <summary>
    /// Wheel settings definition for OMI_vehicle_wheel.
    /// </summary>
    [Serializable]
    public class OMIVehicleWheelSettings
    {
        /// <summary>
        /// The ratio of the maximum force the wheel is using for propulsion (0 to 1).
        /// </summary>
        public float currentForceRatio = 0.0f;

        /// <summary>
        /// The ratio of the maximum steering angle the wheel is rotated to (-1 to 1).
        /// </summary>
        public float currentSteeringRatio = 0.0f;

        /// <summary>
        /// The maximum thrust force in Newtons of this wheel.
        /// </summary>
        public float maxForce = 0.0f;

        /// <summary>
        /// The maximum angle in radians that the wheel can steer.
        /// </summary>
        public float maxSteeringAngle = 0.0f;

        /// <summary>
        /// The index of the physics material in the physicsMaterials array.
        /// </summary>
        public int physicsMaterial = -1;

        /// <summary>
        /// The radius of the wheel in meters.
        /// </summary>
        public float radius = 0.25f;

        /// <summary>
        /// The damping of suspension when compressing (kg/s).
        /// </summary>
        public float suspensionDampingCompression = 2000.0f;

        /// <summary>
        /// The damping of suspension when rebounding (kg/s).
        /// </summary>
        public float suspensionDampingRebound = 2000.0f;

        /// <summary>
        /// The stiffness of the suspension (kg/s²).
        /// </summary>
        public float suspensionStiffness = 20000.0f;

        /// <summary>
        /// The distance the suspension can travel up or down in meters.
        /// </summary>
        public float suspensionTravel = 0.25f;

        /// <summary>
        /// The width of the wheel in meters.
        /// </summary>
        public float width = 0.125f;
    }

    /// <summary>
    /// Document-level root data for OMI_vehicle_wheel extension.
    /// </summary>
    [Serializable]
    public class OMIVehicleWheelRoot
    {
        /// <summary>
        /// Array of wheel settings definitions.
        /// </summary>
        public OMIVehicleWheelSettings[] wheels;
    }

    /// <summary>
    /// Node-level extension data for OMI_vehicle_wheel.
    /// </summary>
    [Serializable]
    public class OMIVehicleWheelNode
    {
        /// <summary>
        /// Index of the wheel settings in the root wheels array.
        /// </summary>
        public int wheel = -1;
    }

    /// <summary>
    /// Thruster settings definition for OMI_vehicle_thruster.
    /// </summary>
    [Serializable]
    public class OMIVehicleThrusterSettings
    {
        /// <summary>
        /// The ratio of the maximum force the thruster is using (0 to 1).
        /// </summary>
        public float currentForceRatio = 0.0f;

        /// <summary>
        /// The ratios of the maximum gimbal angle the thruster is rotated to (XY).
        /// </summary>
        public float[] currentGimbalRatio;

        /// <summary>
        /// The maximum thrust force in Newtons of this thruster.
        /// </summary>
        public float maxForce;

        /// <summary>
        /// The maximum angle the thruster can rotate in radians.
        /// </summary>
        public float maxGimbal = 0.0f;

        /// <summary>
        /// Gets the current gimbal ratio as a Vector2.
        /// </summary>
        public Vector2 CurrentGimbalRatioVector =>
            currentGimbalRatio != null && currentGimbalRatio.Length >= 2
                ? new Vector2(currentGimbalRatio[0], currentGimbalRatio[1])
                : Vector2.zero;
    }

    /// <summary>
    /// Document-level root data for OMI_vehicle_thruster extension.
    /// </summary>
    [Serializable]
    public class OMIVehicleThrusterRoot
    {
        /// <summary>
        /// Array of thruster settings definitions.
        /// </summary>
        public OMIVehicleThrusterSettings[] thrusters;
    }

    /// <summary>
    /// Node-level extension data for OMI_vehicle_thruster.
    /// </summary>
    [Serializable]
    public class OMIVehicleThrusterNode
    {
        /// <summary>
        /// Index of the thruster settings in the root thrusters array.
        /// </summary>
        public int thruster = -1;
    }

    /// <summary>
    /// Hover thruster settings definition for OMI_vehicle_hover_thruster.
    /// </summary>
    [Serializable]
    public class OMIVehicleHoverThrusterSettings
    {
        /// <summary>
        /// The ratio of the maximum hover energy the thruster is using (0 to 1).
        /// </summary>
        public float currentHoverRatio = 0.0f;

        /// <summary>
        /// The ratios of the maximum gimbal angle the thruster is rotated to (XY).
        /// </summary>
        public float[] currentGimbalRatio;

        /// <summary>
        /// The maximum hover energy in Newton-meters of this thruster.
        /// </summary>
        public float maxHoverEnergy;

        /// <summary>
        /// The maximum angle the thruster can rotate in radians.
        /// </summary>
        public float maxGimbal = 0.0f;

        /// <summary>
        /// Gets the current gimbal ratio as a Vector2.
        /// </summary>
        public Vector2 CurrentGimbalRatioVector =>
            currentGimbalRatio != null && currentGimbalRatio.Length >= 2
                ? new Vector2(currentGimbalRatio[0], currentGimbalRatio[1])
                : Vector2.zero;
    }

    /// <summary>
    /// Document-level root data for OMI_vehicle_hover_thruster extension.
    /// </summary>
    [Serializable]
    public class OMIVehicleHoverThrusterRoot
    {
        /// <summary>
        /// Array of hover thruster settings definitions.
        /// </summary>
        public OMIVehicleHoverThrusterSettings[] hoverThrusters;
    }

    /// <summary>
    /// Node-level extension data for OMI_vehicle_hover_thruster.
    /// </summary>
    [Serializable]
    public class OMIVehicleHoverThrusterNode
    {
        /// <summary>
        /// Index of the hover thruster settings in the root hoverThrusters array.
        /// </summary>
        public int hoverThruster = -1;
    }
}
