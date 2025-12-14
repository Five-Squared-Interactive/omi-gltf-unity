// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace OMI.Extensions.Vehicle
{
    /// <summary>
    /// Unity component representing an OMI_vehicle_body.
    /// Provides vehicle control and physics simulation.
    /// </summary>
    [AddComponentMenu("OMI/Vehicle Body")]
    [RequireComponent(typeof(Rigidbody))]
    public class OMIVehicleBody : MonoBehaviour, IOMIVehicleBody
    {
        [Header("Input")]
        [Tooltip("Angular activation input (pitch, yaw, roll). Values between -1 and 1.")]
        [SerializeField]
        private Vector3 _angularActivation = Vector3.zero;

        [Tooltip("Linear activation input (strafe, vertical, forward). Values between -1 and 1.")]
        [SerializeField]
        private Vector3 _linearActivation = Vector3.zero;

        [Header("Vehicle Settings")]
        [Tooltip("Gyroscope torque available (Newton-meters per radian).")]
        [SerializeField]
        private Vector3 _gyroTorque = Vector3.zero;

        [Tooltip("Maximum speed in m/s. -1 for no limit.")]
        [SerializeField]
        private float _maxSpeed = -1.0f;

        [Tooltip("Slow rotation when not receiving angular input.")]
        [SerializeField]
        private bool _angularDampeners = true;

        [Tooltip("Slow movement when not receiving linear input.")]
        [SerializeField]
        private bool _linearDampeners = true;

        [Tooltip("Use throttle mode for linear movement.")]
        [SerializeField]
        private bool _useThrottle = false;

        [Header("References")]
        [Tooltip("The pilot seat component.")]
        [SerializeField]
        private Component _pilotSeat;

        [Header("Dampener Settings")]
        [Tooltip("Angular dampening strength when dampeners are active.")]
        [SerializeField]
        private float _angularDampenerStrength = 5.0f;

        [Tooltip("Linear dampening strength when dampeners are active.")]
        [SerializeField]
        private float _linearDampenerStrength = 2.0f;

        private Rigidbody _rigidbody;
        private List<IOMIVehicleWheel> _wheels = new List<IOMIVehicleWheel>();
        private List<IOMIVehicleThruster> _thrusters = new List<IOMIVehicleThruster>();
        private List<IOMIVehicleHoverThruster> _hoverThrusters = new List<IOMIVehicleHoverThruster>();
        private float _throttle = 0f;

        /// <inheritdoc/>
        public Vector3 AngularActivation
        {
            get => _angularActivation;
            set => _angularActivation = Vector3.ClampMagnitude(value, 1f);
        }

        /// <inheritdoc/>
        public Vector3 LinearActivation
        {
            get => _linearActivation;
            set => _linearActivation = Vector3.ClampMagnitude(value, 1f);
        }

        /// <inheritdoc/>
        public Vector3 GyroTorque
        {
            get => _gyroTorque;
            set => _gyroTorque = value;
        }

        /// <inheritdoc/>
        public float MaxSpeed
        {
            get => _maxSpeed;
            set => _maxSpeed = value;
        }

        /// <inheritdoc/>
        public bool AngularDampeners
        {
            get => _angularDampeners;
            set => _angularDampeners = value;
        }

        /// <inheritdoc/>
        public bool LinearDampeners
        {
            get => _linearDampeners;
            set => _linearDampeners = value;
        }

        /// <inheritdoc/>
        public bool UseThrottle
        {
            get => _useThrottle;
            set => _useThrottle = value;
        }

        /// <inheritdoc/>
        public Component PilotSeat
        {
            get => _pilotSeat;
            set => _pilotSeat = value;
        }

        /// <summary>
        /// Gets the current throttle value (0 to 1) when in throttle mode.
        /// </summary>
        public float Throttle
        {
            get => _throttle;
            set => _throttle = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Gets the Rigidbody component.
        /// </summary>
        public Rigidbody Rigidbody => _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Find all vehicle parts in children
            RefreshVehicleParts();
        }

        /// <summary>
        /// Refreshes the list of vehicle parts (wheels, thrusters, etc.)
        /// </summary>
        public void RefreshVehicleParts()
        {
            _wheels.Clear();
            _wheels.AddRange(GetComponentsInChildren<IOMIVehicleWheel>());

            _thrusters.Clear();
            _thrusters.AddRange(GetComponentsInChildren<IOMIVehicleThruster>());

            _hoverThrusters.Clear();
            _hoverThrusters.AddRange(GetComponentsInChildren<IOMIVehicleHoverThruster>());
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null) return;

            ApplyGyroTorque();
            UpdateVehicleParts();
            ApplyDampeners();
            EnforceMaxSpeed();
        }

        private void ApplyGyroTorque()
        {
            if (_gyroTorque == Vector3.zero) return;

            // Apply gyroscope torque based on angular activation
            Vector3 torque = new Vector3(
                _angularActivation.x * _gyroTorque.x,
                _angularActivation.y * _gyroTorque.y,
                _angularActivation.z * _gyroTorque.z
            );

            if (torque.sqrMagnitude > 0.001f)
            {
                _rigidbody.AddRelativeTorque(torque, ForceMode.Force);
            }
        }

        private void UpdateVehicleParts()
        {
            // Determine effective linear activation (considering throttle mode)
            Vector3 effectiveLinear = _linearActivation;
            if (_useThrottle)
            {
                // In throttle mode, W/S adjust throttle, throttle controls forward
                effectiveLinear.z = _throttle;
            }

            // Update wheels
            foreach (var wheel in _wheels)
            {
                // Steering is controlled by angular Y activation (yaw)
                wheel.CurrentSteeringRatio = _angularActivation.y;
                
                // Force is controlled by linear Z activation (forward/back)
                wheel.CurrentForceRatio = effectiveLinear.z;
            }

            // Update thrusters
            foreach (var thruster in _thrusters)
            {
                // Get thruster's local forward direction relative to vehicle
                var thrusterTransform = (thruster as Component)?.transform;
                if (thrusterTransform == null) continue;

                Vector3 localForward = transform.InverseTransformDirection(thrusterTransform.forward);
                
                // Calculate force ratio based on how much the thrust aligns with linear activation
                float alignment = Vector3.Dot(localForward, effectiveLinear);
                thruster.CurrentForceRatio = Mathf.Clamp01(alignment);

                // Gimbal based on angular activation
                thruster.CurrentGimbalRatio = new Vector2(_angularActivation.x, _angularActivation.y);
                thruster.ApplyGimbal();
            }

            // Update hover thrusters
            foreach (var hoverThruster in _hoverThrusters)
            {
                var thrusterTransform = (hoverThruster as Component)?.transform;
                if (thrusterTransform == null) continue;

                Vector3 localForward = transform.InverseTransformDirection(thrusterTransform.forward);
                
                // Hover thrusters typically point down (-Y in vehicle space)
                // Activate based on vertical and forward alignment
                float verticalAlignment = Vector3.Dot(localForward, Vector3.up);
                float forwardAlignment = Vector3.Dot(localForward, Vector3.forward);
                
                // Base hover (counteract gravity)
                float hoverRatio = verticalAlignment > 0.5f ? 1f : 0f;
                
                // Add thrust based on input
                hoverRatio = Mathf.Clamp01(hoverRatio + forwardAlignment * effectiveLinear.z);
                
                hoverThruster.CurrentHoverRatio = hoverRatio;
                hoverThruster.CurrentGimbalRatio = new Vector2(_angularActivation.x, _angularActivation.y);
                hoverThruster.ApplyGimbal();
            }
        }

        private void ApplyDampeners()
        {
            // Angular dampeners
            if (_angularDampeners)
            {
                // For each axis where activation is zero, dampen rotation
                Vector3 angularVel = _rigidbody.angularVelocity;
                Vector3 localAngularVel = transform.InverseTransformDirection(angularVel);
                Vector3 dampTorque = Vector3.zero;

                if (Mathf.Approximately(_angularActivation.x, 0f))
                    dampTorque.x = -localAngularVel.x * _angularDampenerStrength;
                if (Mathf.Approximately(_angularActivation.y, 0f))
                    dampTorque.y = -localAngularVel.y * _angularDampenerStrength;
                if (Mathf.Approximately(_angularActivation.z, 0f))
                    dampTorque.z = -localAngularVel.z * _angularDampenerStrength;

                if (dampTorque.sqrMagnitude > 0.001f)
                {
                    _rigidbody.AddRelativeTorque(dampTorque, ForceMode.Acceleration);
                }
            }

            // Linear dampeners
            if (_linearDampeners)
            {
                Vector3 velocity = _rigidbody.linearVelocity;
                Vector3 localVelocity = transform.InverseTransformDirection(velocity);
                Vector3 dampForce = Vector3.zero;

                Vector3 effectiveLinear = _useThrottle 
                    ? new Vector3(_linearActivation.x, _linearActivation.y, _throttle)
                    : _linearActivation;

                if (Mathf.Approximately(effectiveLinear.x, 0f))
                    dampForce.x = -localVelocity.x * _linearDampenerStrength;
                if (Mathf.Approximately(effectiveLinear.y, 0f))
                    dampForce.y = -localVelocity.y * _linearDampenerStrength;
                if (Mathf.Approximately(effectiveLinear.z, 0f))
                    dampForce.z = -localVelocity.z * _linearDampenerStrength;

                if (dampForce.sqrMagnitude > 0.001f)
                {
                    _rigidbody.AddRelativeForce(dampForce, ForceMode.Acceleration);
                }
            }
        }

        private void EnforceMaxSpeed()
        {
            if (_maxSpeed < 0f) return;

            float currentSpeed = _rigidbody.linearVelocity.magnitude;
            if (currentSpeed > _maxSpeed)
            {
                // Don't add more thrust in the direction of travel
                // This is handled implicitly by thrusters not activating at max speed
            }
        }

        /// <summary>
        /// Increases throttle by the given amount (throttle mode only).
        /// </summary>
        public void IncreaseThrottle(float amount)
        {
            if (_useThrottle)
            {
                _throttle = Mathf.Clamp01(_throttle + amount);
            }
        }

        /// <summary>
        /// Decreases throttle by the given amount (throttle mode only).
        /// </summary>
        public void DecreaseThrottle(float amount)
        {
            if (_useThrottle)
            {
                _throttle = Mathf.Clamp01(_throttle - amount);
            }
        }

        /// <summary>
        /// Cuts throttle to zero immediately (throttle mode only).
        /// </summary>
        public void CutThrottle()
        {
            _throttle = 0f;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw vehicle forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            // Draw center of mass indicator
            if (_rigidbody != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 comWorld = transform.TransformPoint(_rigidbody.centerOfMass);
                Gizmos.DrawWireSphere(comWorld, 0.1f);
            }
        }
#endif
    }

    /// <summary>
    /// Unity component representing an OMI_vehicle_wheel.
    /// Provides wheel physics simulation using Unity's WheelCollider.
    /// </summary>
    [AddComponentMenu("OMI/Vehicle Wheel")]
    public class OMIVehicleWheel : MonoBehaviour, IOMIVehicleWheel
    {
        [Header("Runtime State")]
        [SerializeField]
        [Range(-1f, 1f)]
        private float _currentForceRatio = 0f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float _currentSteeringRatio = 0f;

        [Header("Wheel Settings")]
        [SerializeField]
        private float _maxForce = 0f;

        [SerializeField]
        private float _maxSteeringAngle = 0f;

        [SerializeField]
        private float _radius = 0.25f;

        [SerializeField]
        private float _width = 0.125f;

        [Header("Suspension")]
        [SerializeField]
        private float _suspensionStiffness = 20000f;

        [SerializeField]
        private float _suspensionTravel = 0.25f;

        [SerializeField]
        private float _suspensionDampingCompression = 2000f;

        [SerializeField]
        private float _suspensionDampingRebound = 2000f;

        [Header("References")]
        [SerializeField]
        private WheelCollider _wheelCollider;

        [SerializeField]
        private Transform _wheelVisual;

        private OMIVehicleBody _vehicleBody;

        /// <inheritdoc/>
        public float CurrentForceRatio
        {
            get => _currentForceRatio;
            set => _currentForceRatio = Mathf.Clamp(value, -1f, 1f);
        }

        /// <inheritdoc/>
        public float CurrentSteeringRatio
        {
            get => _currentSteeringRatio;
            set => _currentSteeringRatio = Mathf.Clamp(value, -1f, 1f);
        }

        /// <inheritdoc/>
        public float MaxForce
        {
            get => _maxForce;
            set => _maxForce = Mathf.Max(0f, value);
        }

        /// <inheritdoc/>
        public float MaxSteeringAngle
        {
            get => _maxSteeringAngle;
            set => _maxSteeringAngle = Mathf.Max(0f, value);
        }

        /// <inheritdoc/>
        public float Radius
        {
            get => _radius;
            set => _radius = Mathf.Max(0.01f, value);
        }

        /// <inheritdoc/>
        public float Width
        {
            get => _width;
            set => _width = Mathf.Max(0.01f, value);
        }

        /// <inheritdoc/>
        public float SuspensionStiffness
        {
            get => _suspensionStiffness;
            set => _suspensionStiffness = Mathf.Max(0f, value);
        }

        /// <inheritdoc/>
        public float SuspensionTravel
        {
            get => _suspensionTravel;
            set => _suspensionTravel = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Gets the WheelCollider component.
        /// </summary>
        public WheelCollider WheelCollider => _wheelCollider;

        private void Awake()
        {
            _vehicleBody = GetComponentInParent<OMIVehicleBody>();
            
            if (_wheelCollider == null)
            {
                _wheelCollider = GetComponent<WheelCollider>();
            }
        }

        private void Start()
        {
            SetupWheelCollider();
        }

        /// <summary>
        /// Sets up the WheelCollider with current settings.
        /// </summary>
        public void SetupWheelCollider()
        {
            if (_wheelCollider == null)
            {
                _wheelCollider = gameObject.AddComponent<WheelCollider>();
            }

            _wheelCollider.radius = _radius;
            _wheelCollider.suspensionDistance = _suspensionTravel;

            var suspension = _wheelCollider.suspensionSpring;
            suspension.spring = _suspensionStiffness;
            suspension.damper = (_suspensionDampingCompression + _suspensionDampingRebound) / 2f;
            suspension.targetPosition = 0.5f;
            _wheelCollider.suspensionSpring = suspension;
        }

        private void FixedUpdate()
        {
            if (_wheelCollider == null) return;

            // Apply steering
            _wheelCollider.steerAngle = _currentSteeringRatio * _maxSteeringAngle * Mathf.Rad2Deg;

            // Apply motor torque
            if (_maxForce > 0f)
            {
                // Convert force to torque: T = F * r
                float motorTorque = _currentForceRatio * _maxForce * _radius;
                _wheelCollider.motorTorque = motorTorque;
            }

            // Update visual wheel position/rotation
            UpdateWheelVisual();
        }

        private void UpdateWheelVisual()
        {
            if (_wheelVisual == null || _wheelCollider == null) return;

            _wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            _wheelVisual.position = pos;
            _wheelVisual.rotation = rot;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw wheel circle
            Gizmos.color = Color.green;
            
            Vector3 center = transform.position;
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            Vector3 up = transform.up;

            int segments = 24;
            Vector3 prevPoint = center + up * _radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 point = center + (up * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * _radius;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            // Draw suspension
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center, center - up * _suspensionTravel);
        }
#endif
    }

    /// <summary>
    /// Unity component representing an OMI_vehicle_thruster.
    /// </summary>
    [AddComponentMenu("OMI/Vehicle Thruster")]
    public class OMIVehicleThruster : MonoBehaviour, IOMIVehicleThruster
    {
        [Header("Runtime State")]
        [SerializeField]
        [Range(0f, 1f)]
        private float _currentForceRatio = 0f;

        [SerializeField]
        private Vector2 _currentGimbalRatio = Vector2.zero;

        [Header("Thruster Settings")]
        [SerializeField]
        private float _maxForce = 1000f;

        [SerializeField]
        private float _maxGimbal = 0f;

        private OMIVehicleBody _vehicleBody;
        private Quaternion _baseRotation;
        private Rigidbody _targetRigidbody;

        /// <inheritdoc/>
        public float CurrentForceRatio
        {
            get => _currentForceRatio;
            set => _currentForceRatio = Mathf.Clamp01(value);
        }

        /// <inheritdoc/>
        public Vector2 CurrentGimbalRatio
        {
            get => _currentGimbalRatio;
            set => _currentGimbalRatio = Vector2.ClampMagnitude(value, 1f);
        }

        /// <inheritdoc/>
        public float MaxForce
        {
            get => _maxForce;
            set => _maxForce = Mathf.Max(0f, value);
        }

        /// <inheritdoc/>
        public float MaxGimbal
        {
            get => _maxGimbal;
            set => _maxGimbal = Mathf.Clamp(value, 0f, Mathf.PI);
        }

        /// <inheritdoc/>
        public Vector3 ThrustDirection => transform.forward;

        private void Awake()
        {
            _vehicleBody = GetComponentInParent<OMIVehicleBody>();
            _baseRotation = transform.localRotation;
            
            if (_vehicleBody != null)
            {
                _targetRigidbody = _vehicleBody.Rigidbody;
            }
        }

        private void FixedUpdate()
        {
            if (_targetRigidbody == null || _currentForceRatio <= 0f) return;

            float force = _currentForceRatio * _maxForce;
            
            // Apply force at thruster position in thrust direction
            _targetRigidbody.AddForceAtPosition(
                transform.forward * force,
                transform.position,
                ForceMode.Force
            );
        }

        /// <inheritdoc/>
        public void ApplyGimbal()
        {
            if (_maxGimbal <= 0f)
            {
                transform.localRotation = _baseRotation;
                return;
            }

            if (_currentGimbalRatio.sqrMagnitude < 0.0001f)
            {
                transform.localRotation = _baseRotation;
                return;
            }

            // Calculate gimbal rotation
            Vector2 clampedRatio = Vector2.ClampMagnitude(_currentGimbalRatio, 1f);
            Vector2 rotAngles = clampedRatio * _maxGimbal;
            float angleMag = rotAngles.magnitude;

            if (angleMag < 0.0001f)
            {
                transform.localRotation = _baseRotation;
                return;
            }

            float sinNormAngle = Mathf.Sin(angleMag / 2f) / angleMag;
            float cosHalfAngle = Mathf.Cos(angleMag / 2f);

            Quaternion gimbalRotation = new Quaternion(
                rotAngles.x * sinNormAngle,
                rotAngles.y * sinNormAngle,
                0f,
                cosHalfAngle
            );

            transform.localRotation = _baseRotation * gimbalRotation;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw thrust direction
            Gizmos.color = _currentForceRatio > 0.01f ? Color.red : Color.blue;
            float length = 1f + _currentForceRatio * 2f;
            Gizmos.DrawRay(transform.position, transform.forward * length);

            // Draw gimbal range
            if (_maxGimbal > 0f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                float coneRadius = Mathf.Tan(_maxGimbal) * 2f;
                // Draw simple cone indicator
                Vector3 end = transform.position + transform.forward * 2f;
                Gizmos.DrawLine(transform.position, end + transform.up * coneRadius);
                Gizmos.DrawLine(transform.position, end - transform.up * coneRadius);
                Gizmos.DrawLine(transform.position, end + transform.right * coneRadius);
                Gizmos.DrawLine(transform.position, end - transform.right * coneRadius);
            }
        }
#endif
    }

    /// <summary>
    /// Unity component representing an OMI_vehicle_hover_thruster.
    /// </summary>
    [AddComponentMenu("OMI/Vehicle Hover Thruster")]
    public class OMIVehicleHoverThruster : MonoBehaviour, IOMIVehicleHoverThruster
    {
        [Header("Runtime State")]
        [SerializeField]
        [Range(0f, 1f)]
        private float _currentHoverRatio = 0f;

        [SerializeField]
        private Vector2 _currentGimbalRatio = Vector2.zero;

        [Header("Hover Thruster Settings")]
        [SerializeField]
        private float _maxHoverEnergy = 10000f;

        [SerializeField]
        private float _maxGimbal = 0f;

        [Header("Ground Detection")]
        [SerializeField]
        private LayerMask _groundLayers = ~0;

        [SerializeField]
        private float _maxRaycastDistance = 20f;

        private OMIVehicleBody _vehicleBody;
        private Quaternion _baseRotation;
        private Rigidbody _targetRigidbody;
        private float _currentDistance = float.MaxValue;

        /// <inheritdoc/>
        public float CurrentHoverRatio
        {
            get => _currentHoverRatio;
            set => _currentHoverRatio = Mathf.Clamp01(value);
        }

        /// <inheritdoc/>
        public Vector2 CurrentGimbalRatio
        {
            get => _currentGimbalRatio;
            set => _currentGimbalRatio = Vector2.ClampMagnitude(value, 1f);
        }

        /// <inheritdoc/>
        public float MaxHoverEnergy
        {
            get => _maxHoverEnergy;
            set => _maxHoverEnergy = Mathf.Max(0f, value);
        }

        /// <inheritdoc/>
        public float MaxGimbal
        {
            get => _maxGimbal;
            set => _maxGimbal = Mathf.Clamp(value, 0f, Mathf.PI);
        }

        /// <inheritdoc/>
        public float DistanceToGround => _currentDistance;

        /// <inheritdoc/>
        public float CurrentForce
        {
            get
            {
                if (_currentDistance <= 0f || _currentDistance == float.MaxValue)
                    return 0f;
                
                // Force = HoverEnergy / Distance
                return (_currentHoverRatio * _maxHoverEnergy) / _currentDistance;
            }
        }

        private void Awake()
        {
            _vehicleBody = GetComponentInParent<OMIVehicleBody>();
            _baseRotation = transform.localRotation;

            if (_vehicleBody != null)
            {
                _targetRigidbody = _vehicleBody.Rigidbody;
            }
        }

        private void FixedUpdate()
        {
            UpdateGroundDistance();

            if (_targetRigidbody == null || _currentHoverRatio <= 0f) return;

            float force = CurrentForce;
            if (force > 0f)
            {
                _targetRigidbody.AddForceAtPosition(
                    transform.forward * force,
                    transform.position,
                    ForceMode.Force
                );
            }
        }

        private void UpdateGroundDistance()
        {
            // Raycast in the opposite direction of thrust (-Z local)
            Ray ray = new Ray(transform.position, -transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, _groundLayers))
            {
                _currentDistance = hit.distance;
            }
            else
            {
                _currentDistance = float.MaxValue;
            }
        }

        /// <inheritdoc/>
        public void ApplyGimbal()
        {
            if (_maxGimbal <= 0f)
            {
                transform.localRotation = _baseRotation;
                return;
            }

            if (_currentGimbalRatio.sqrMagnitude < 0.0001f)
            {
                transform.localRotation = _baseRotation;
                return;
            }

            Vector2 clampedRatio = Vector2.ClampMagnitude(_currentGimbalRatio, 1f);
            Vector2 rotAngles = clampedRatio * _maxGimbal;
            float angleMag = rotAngles.magnitude;

            if (angleMag < 0.0001f)
            {
                transform.localRotation = _baseRotation;
                return;
            }

            float sinNormAngle = Mathf.Sin(angleMag / 2f) / angleMag;
            float cosHalfAngle = Mathf.Cos(angleMag / 2f);

            Quaternion gimbalRotation = new Quaternion(
                rotAngles.x * sinNormAngle,
                rotAngles.y * sinNormAngle,
                0f,
                cosHalfAngle
            );

            transform.localRotation = _baseRotation * gimbalRotation;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw thrust direction
            float forceRatio = _currentHoverRatio * Mathf.Min(1f, 5f / Mathf.Max(0.1f, _currentDistance));
            Gizmos.color = forceRatio > 0.01f ? Color.cyan : Color.blue;
            float length = 1f + forceRatio * 2f;
            Gizmos.DrawRay(transform.position, transform.forward * length);

            // Draw ground detection ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, -transform.forward * Mathf.Min(_currentDistance, _maxRaycastDistance));

            // Draw distance indicator
            if (_currentDistance < float.MaxValue)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position - transform.forward * _currentDistance, 0.1f);
            }
        }
#endif
    }
}
