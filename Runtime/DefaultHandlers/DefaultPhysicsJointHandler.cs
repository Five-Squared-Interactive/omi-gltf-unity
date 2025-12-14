// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.PhysicsJoint
{
    /// <summary>
    /// Default Unity implementation for handling OMI_physics_joint.
    /// Creates Unity ConfigurableJoints from OMI physics joints.
    /// </summary>
    public class DefaultPhysicsJointHandler : IPhysicsJointHandler, IPhysicsJointDocumentHandler
    {
        public string ExtensionName => OMIPhysicsJointExtension.ExtensionName;
        public int Priority => 70; // After physics body and gravity

        private List<OMIPhysicsJointSettings> _importedSettings;
        private List<OMIPhysicsJointSettings> _exportSettings;
        private Dictionary<string, int> _exportSettingsIndices;

        #region IOMIExtensionHandler

        public Task OnImportAsync(OMIPhysicsJointNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<OMIPhysicsJointNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<OMIPhysicsJointNode>(null);
        }

        public Task OnImportAsync(OMIPhysicsJointRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        Task<OMIPhysicsJointRoot> IOMIExtensionHandler<OMIPhysicsJointRoot>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        #endregion

        #region IPhysicsJointDocumentHandler

        public Task OnDocumentImportAsync(OMIPhysicsJointRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data?.PhysicsJoints == null) return Task.CompletedTask;

            _importedSettings = new List<OMIPhysicsJointSettings>(data.PhysicsJoints);
            context.CustomData["OMI_physics_joints"] = _importedSettings;

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Imported {_importedSettings.Count} physics joint settings");
            }

            return Task.CompletedTask;
        }

        public Task<OMIPhysicsJointRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            if (_exportSettings == null || _exportSettings.Count == 0)
            {
                return Task.FromResult<OMIPhysicsJointRoot>(null);
            }

            var root = new OMIPhysicsJointRoot
            {
                PhysicsJoints = _exportSettings.ToArray()
            };

            return Task.FromResult(root);
        }

        public int GetOrRegisterJointSettings(OMIPhysicsJointSettings settings, OMIExportContext context)
        {
            if (settings == null) return -1;

            _exportSettings ??= new List<OMIPhysicsJointSettings>();
            _exportSettingsIndices ??= new Dictionary<string, int>();

            // Create a key for deduplication
            var key = GetSettingsKey(settings);
            
            if (_exportSettingsIndices.TryGetValue(key, out var existingIndex))
            {
                return existingIndex;
            }

            int index = _exportSettings.Count;
            _exportSettings.Add(settings);
            _exportSettingsIndices[key] = index;

            return index;
        }

        private string GetSettingsKey(OMIPhysicsJointSettings settings)
        {
            // Create a simple hash key for deduplication
            return $"limits:{settings.Limits?.Length ?? 0}_drives:{settings.Drives?.Length ?? 0}";
        }

        #endregion

        #region IPhysicsJointHandler

        public Task OnNodeImportAsync(OMIPhysicsJointNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null) return Task.CompletedTask;

            // Get joint settings
            if (data.Joint < 0 || _importedSettings == null || data.Joint >= _importedSettings.Count)
            {
                Debug.LogWarning($"[OMI] Invalid joint settings index: {data.Joint}");
                return Task.CompletedTask;
            }

            // Get connected node
            var connectedObject = context.GetGameObject(data.ConnectedNode);
            if (connectedObject == null)
            {
                Debug.LogWarning($"[OMI] Cannot find connected node {data.ConnectedNode} for joint on {targetObject.name}");
                return Task.CompletedTask;
            }

            var jointSettings = _importedSettings[data.Joint];
            CreateJoint(data, jointSettings, targetObject, connectedObject, context);

            return Task.CompletedTask;
        }

        public Task<OMIPhysicsJointNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExtractJoint(sourceObject.GetComponent<Joint>(), context));
        }

        public Joint CreateJoint(
            OMIPhysicsJointNode jointNode,
            OMIPhysicsJointSettings jointSettings,
            GameObject targetObject,
            GameObject connectedObject,
            OMIImportContext context)
        {
            if (jointNode == null || jointSettings == null || targetObject == null)
            {
                return null;
            }

            // Use ConfigurableJoint as it's the most flexible
            var joint = targetObject.AddComponent<ConfigurableJoint>();
            
            // Connect to the other object's rigidbody
            var connectedRigidbody = connectedObject?.GetComponent<Rigidbody>();
            joint.connectedBody = connectedRigidbody;

            // Configure collision between connected bodies
            joint.enableCollision = jointNode.EnableCollision;

            // Apply limits
            if (jointSettings.Limits != null)
            {
                foreach (var limit in jointSettings.Limits)
                {
                    ApplyLimit(joint, limit);
                }
            }

            // Apply drives
            if (jointSettings.Drives != null)
            {
                foreach (var drive in jointSettings.Drives)
                {
                    ApplyDrive(joint, drive);
                }
            }

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created ConfigurableJoint on {targetObject.name} connected to {connectedObject?.name}");
            }

            return joint;
        }

        private void ApplyLimit(ConfigurableJoint joint, OMIPhysicsJointLimit limit)
        {
            if (limit == null) return;

            bool hasLinear = limit.LinearAxes != null && limit.LinearAxes.Length > 0;
            bool hasAngular = limit.AngularAxes != null && limit.AngularAxes.Length > 0;

            if (hasLinear)
            {
                ApplyLinearLimit(joint, limit);
            }

            if (hasAngular)
            {
                ApplyAngularLimit(joint, limit);
            }
        }

        private void ApplyLinearLimit(ConfigurableJoint joint, OMIPhysicsJointLimit limit)
        {
            bool isLocked = limit.Min.HasValue && limit.Max.HasValue && 
                           Mathf.Approximately(limit.Min.Value, limit.Max.Value) && 
                           Mathf.Approximately(limit.Min.Value, 0);

            bool isLimited = limit.Min.HasValue || limit.Max.HasValue;

            foreach (var axis in limit.LinearAxes)
            {
                var motion = isLocked ? ConfigurableJointMotion.Locked : 
                            (isLimited ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Free);

                switch (axis)
                {
                    case 0: // X
                        joint.xMotion = motion;
                        break;
                    case 1: // Y
                        joint.yMotion = motion;
                        break;
                    case 2: // Z
                        joint.zMotion = motion;
                        break;
                }
            }

            if (isLimited && !isLocked)
            {
                var linearLimit = joint.linearLimit;
                float maxDist = 0;
                if (limit.Max.HasValue) maxDist = Mathf.Max(maxDist, Mathf.Abs(limit.Max.Value));
                if (limit.Min.HasValue) maxDist = Mathf.Max(maxDist, Mathf.Abs(limit.Min.Value));
                linearLimit.limit = maxDist;
                joint.linearLimit = linearLimit;

                if (limit.Stiffness.HasValue || limit.Damping > 0)
                {
                    var spring = joint.linearLimitSpring;
                    spring.spring = limit.Stiffness ?? float.PositiveInfinity;
                    spring.damper = limit.Damping;
                    joint.linearLimitSpring = spring;
                }
            }
        }

        private void ApplyAngularLimit(ConfigurableJoint joint, OMIPhysicsJointLimit limit)
        {
            bool isLocked = limit.Min.HasValue && limit.Max.HasValue && 
                           Mathf.Approximately(limit.Min.Value, limit.Max.Value) && 
                           Mathf.Approximately(limit.Min.Value, 0);

            bool isLimited = limit.Min.HasValue || limit.Max.HasValue;

            foreach (var axis in limit.AngularAxes)
            {
                var motion = isLocked ? ConfigurableJointMotion.Locked : 
                            (isLimited ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Free);

                switch (axis)
                {
                    case 0: // X - Primary axis
                        joint.angularXMotion = motion;
                        if (isLimited && !isLocked)
                        {
                            var lowAngularXLimit = joint.lowAngularXLimit;
                            var highAngularXLimit = joint.highAngularXLimit;
                            lowAngularXLimit.limit = (limit.Min ?? -180f) * Mathf.Rad2Deg;
                            highAngularXLimit.limit = (limit.Max ?? 180f) * Mathf.Rad2Deg;
                            joint.lowAngularXLimit = lowAngularXLimit;
                            joint.highAngularXLimit = highAngularXLimit;
                        }
                        break;
                    case 1: // Y
                        joint.angularYMotion = motion;
                        if (isLimited && !isLocked)
                        {
                            var angularYLimit = joint.angularYLimit;
                            float maxAngle = Mathf.Max(
                                Mathf.Abs(limit.Min ?? 0) * Mathf.Rad2Deg,
                                Mathf.Abs(limit.Max ?? 0) * Mathf.Rad2Deg
                            );
                            angularYLimit.limit = maxAngle;
                            joint.angularYLimit = angularYLimit;
                        }
                        break;
                    case 2: // Z
                        joint.angularZMotion = motion;
                        if (isLimited && !isLocked)
                        {
                            var angularZLimit = joint.angularZLimit;
                            float maxAngle = Mathf.Max(
                                Mathf.Abs(limit.Min ?? 0) * Mathf.Rad2Deg,
                                Mathf.Abs(limit.Max ?? 0) * Mathf.Rad2Deg
                            );
                            angularZLimit.limit = maxAngle;
                            joint.angularZLimit = angularZLimit;
                        }
                        break;
                }
            }

            if (limit.Stiffness.HasValue || limit.Damping > 0)
            {
                var spring = joint.angularXLimitSpring;
                spring.spring = limit.Stiffness ?? float.PositiveInfinity;
                spring.damper = limit.Damping;
                joint.angularXLimitSpring = spring;
                joint.angularYZLimitSpring = spring;
            }
        }

        private void ApplyDrive(ConfigurableJoint joint, OMIPhysicsJointDrive drive)
        {
            if (drive == null) return;

            var jointDrive = new JointDrive
            {
                positionSpring = drive.Stiffness,
                positionDamper = drive.Damping,
                maximumForce = drive.MaxForce ?? float.MaxValue
            };

            bool isLinear = drive.Type == OMIJointDriveType.Linear;

            if (isLinear)
            {
                switch (drive.Axis)
                {
                    case 0: joint.xDrive = jointDrive; break;
                    case 1: joint.yDrive = jointDrive; break;
                    case 2: joint.zDrive = jointDrive; break;
                }

                if (drive.PositionTarget.HasValue)
                {
                    var target = joint.targetPosition;
                    switch (drive.Axis)
                    {
                        case 0: target.x = drive.PositionTarget.Value; break;
                        case 1: target.y = drive.PositionTarget.Value; break;
                        case 2: target.z = -drive.PositionTarget.Value; break; // glTF to Unity Z
                    }
                    joint.targetPosition = target;
                }

                if (drive.VelocityTarget.HasValue)
                {
                    var velocity = joint.targetVelocity;
                    switch (drive.Axis)
                    {
                        case 0: velocity.x = drive.VelocityTarget.Value; break;
                        case 1: velocity.y = drive.VelocityTarget.Value; break;
                        case 2: velocity.z = -drive.VelocityTarget.Value; break;
                    }
                    joint.targetVelocity = velocity;
                }
            }
            else // Angular
            {
                switch (drive.Axis)
                {
                    case 0: joint.angularXDrive = jointDrive; break;
                    case 1:
                    case 2:
                        joint.angularYZDrive = jointDrive;
                        break;
                }

                if (drive.PositionTarget.HasValue)
                {
                    var rotation = joint.targetRotation;
                    float angleDeg = drive.PositionTarget.Value * Mathf.Rad2Deg;
                    switch (drive.Axis)
                    {
                        case 0: rotation = Quaternion.Euler(angleDeg, 0, 0) * rotation; break;
                        case 1: rotation = Quaternion.Euler(0, angleDeg, 0) * rotation; break;
                        case 2: rotation = Quaternion.Euler(0, 0, -angleDeg) * rotation; break;
                    }
                    joint.targetRotation = rotation;
                }

                if (drive.VelocityTarget.HasValue)
                {
                    var angularVelocity = joint.targetAngularVelocity;
                    float velRad = drive.VelocityTarget.Value;
                    switch (drive.Axis)
                    {
                        case 0: angularVelocity.x = velRad; break;
                        case 1: angularVelocity.y = velRad; break;
                        case 2: angularVelocity.z = -velRad; break;
                    }
                    joint.targetAngularVelocity = angularVelocity;
                }
            }
        }

        public OMIPhysicsJointNode ExtractJoint(Joint joint, OMIExportContext context)
        {
            if (joint == null) return null;

            var connectedBody = joint.connectedBody;
            if (connectedBody == null) return null;

            // Find the node index for the connected body
            if (!context.GameObjectToNode.TryGetValue(connectedBody.gameObject, out var connectedNodeIndex))
            {
                Debug.LogWarning($"[OMI] Could not find node index for connected body {connectedBody.name}");
                return null;
            }

            var settings = ExtractJointSettings(joint, context);
            int settingsIndex = GetOrRegisterJointSettings(settings, context);

            return new OMIPhysicsJointNode
            {
                ConnectedNode = connectedNodeIndex,
                Joint = settingsIndex,
                EnableCollision = joint.enableCollision
            };
        }

        public OMIPhysicsJointSettings ExtractJointSettings(Joint joint, OMIExportContext context)
        {
            var settings = new OMIPhysicsJointSettings();
            var limits = new List<OMIPhysicsJointLimit>();
            var drives = new List<OMIPhysicsJointDrive>();

            if (joint is ConfigurableJoint configJoint)
            {
                // Extract linear limits
                ExtractLinearLimits(configJoint, limits);
                
                // Extract angular limits
                ExtractAngularLimits(configJoint, limits);

                // Extract drives
                ExtractDrives(configJoint, drives);
            }
            else if (joint is HingeJoint hingeJoint)
            {
                ExtractHingeJointLimits(hingeJoint, limits);
            }
            else if (joint is FixedJoint)
            {
                // Weld joint - lock all axes
                limits.Add(new OMIPhysicsJointLimit
                {
                    LinearAxes = new int[] { 0, 1, 2 },
                    Min = 0,
                    Max = 0
                });
                limits.Add(new OMIPhysicsJointLimit
                {
                    AngularAxes = new int[] { 0, 1, 2 },
                    Min = 0,
                    Max = 0
                });
            }

            settings.Limits = limits.Count > 0 ? limits.ToArray() : null;
            settings.Drives = drives.Count > 0 ? drives.ToArray() : null;

            return settings;
        }

        private void ExtractLinearLimits(ConfigurableJoint joint, List<OMIPhysicsJointLimit> limits)
        {
            var lockedAxes = new List<int>();
            var limitedAxes = new List<int>();

            if (joint.xMotion == ConfigurableJointMotion.Locked) lockedAxes.Add(0);
            else if (joint.xMotion == ConfigurableJointMotion.Limited) limitedAxes.Add(0);

            if (joint.yMotion == ConfigurableJointMotion.Locked) lockedAxes.Add(1);
            else if (joint.yMotion == ConfigurableJointMotion.Limited) limitedAxes.Add(1);

            if (joint.zMotion == ConfigurableJointMotion.Locked) lockedAxes.Add(2);
            else if (joint.zMotion == ConfigurableJointMotion.Limited) limitedAxes.Add(2);

            if (lockedAxes.Count > 0)
            {
                limits.Add(new OMIPhysicsJointLimit
                {
                    LinearAxes = lockedAxes.ToArray(),
                    Min = 0,
                    Max = 0
                });
            }

            if (limitedAxes.Count > 0)
            {
                var limit = joint.linearLimit;
                var spring = joint.linearLimitSpring;
                limits.Add(new OMIPhysicsJointLimit
                {
                    LinearAxes = limitedAxes.ToArray(),
                    Min = -limit.limit,
                    Max = limit.limit,
                    Stiffness = spring.spring > 0 ? spring.spring : null,
                    Damping = spring.damper
                });
            }
        }

        private void ExtractAngularLimits(ConfigurableJoint joint, List<OMIPhysicsJointLimit> limits)
        {
            // X axis
            if (joint.angularXMotion == ConfigurableJointMotion.Locked)
            {
                limits.Add(new OMIPhysicsJointLimit
                {
                    AngularAxes = new int[] { 0 },
                    Min = 0,
                    Max = 0
                });
            }
            else if (joint.angularXMotion == ConfigurableJointMotion.Limited)
            {
                var spring = joint.angularXLimitSpring;
                limits.Add(new OMIPhysicsJointLimit
                {
                    AngularAxes = new int[] { 0 },
                    Min = joint.lowAngularXLimit.limit * Mathf.Deg2Rad,
                    Max = joint.highAngularXLimit.limit * Mathf.Deg2Rad,
                    Stiffness = spring.spring > 0 ? spring.spring : null,
                    Damping = spring.damper
                });
            }

            // Y and Z axes
            var lockedYZ = new List<int>();
            var limitedYZ = new List<int>();

            if (joint.angularYMotion == ConfigurableJointMotion.Locked) lockedYZ.Add(1);
            else if (joint.angularYMotion == ConfigurableJointMotion.Limited) limitedYZ.Add(1);

            if (joint.angularZMotion == ConfigurableJointMotion.Locked) lockedYZ.Add(2);
            else if (joint.angularZMotion == ConfigurableJointMotion.Limited) limitedYZ.Add(2);

            if (lockedYZ.Count > 0)
            {
                limits.Add(new OMIPhysicsJointLimit
                {
                    AngularAxes = lockedYZ.ToArray(),
                    Min = 0,
                    Max = 0
                });
            }

            if (limitedYZ.Count > 0)
            {
                var spring = joint.angularYZLimitSpring;
                float yLimit = joint.angularYLimit.limit * Mathf.Deg2Rad;
                float zLimit = joint.angularZLimit.limit * Mathf.Deg2Rad;
                float maxLimit = Mathf.Max(yLimit, zLimit);
                
                limits.Add(new OMIPhysicsJointLimit
                {
                    AngularAxes = limitedYZ.ToArray(),
                    Min = -maxLimit,
                    Max = maxLimit,
                    Stiffness = spring.spring > 0 ? spring.spring : null,
                    Damping = spring.damper
                });
            }
        }

        private void ExtractDrives(ConfigurableJoint joint, List<OMIPhysicsJointDrive> drives)
        {
            // Linear drives
            if (joint.xDrive.positionSpring > 0 || joint.xDrive.positionDamper > 0)
            {
                drives.Add(CreateDriveFromJointDrive(joint.xDrive, OMIJointDriveType.Linear, 0, 
                    joint.targetPosition.x, joint.targetVelocity.x));
            }
            if (joint.yDrive.positionSpring > 0 || joint.yDrive.positionDamper > 0)
            {
                drives.Add(CreateDriveFromJointDrive(joint.yDrive, OMIJointDriveType.Linear, 1,
                    joint.targetPosition.y, joint.targetVelocity.y));
            }
            if (joint.zDrive.positionSpring > 0 || joint.zDrive.positionDamper > 0)
            {
                drives.Add(CreateDriveFromJointDrive(joint.zDrive, OMIJointDriveType.Linear, 2,
                    -joint.targetPosition.z, -joint.targetVelocity.z)); // Unity to glTF Z
            }

            // Angular drives
            if (joint.angularXDrive.positionSpring > 0 || joint.angularXDrive.positionDamper > 0)
            {
                var euler = joint.targetRotation.eulerAngles;
                drives.Add(CreateDriveFromJointDrive(joint.angularXDrive, OMIJointDriveType.Angular, 0,
                    euler.x * Mathf.Deg2Rad, joint.targetAngularVelocity.x));
            }
            if (joint.angularYZDrive.positionSpring > 0 || joint.angularYZDrive.positionDamper > 0)
            {
                var euler = joint.targetRotation.eulerAngles;
                // Add Y drive
                drives.Add(CreateDriveFromJointDrive(joint.angularYZDrive, OMIJointDriveType.Angular, 1,
                    euler.y * Mathf.Deg2Rad, joint.targetAngularVelocity.y));
                // Add Z drive
                drives.Add(CreateDriveFromJointDrive(joint.angularYZDrive, OMIJointDriveType.Angular, 2,
                    -euler.z * Mathf.Deg2Rad, -joint.targetAngularVelocity.z)); // Unity to glTF Z
            }
        }

        private OMIPhysicsJointDrive CreateDriveFromJointDrive(JointDrive drive, string type, int axis, 
            float positionTarget, float velocityTarget)
        {
            return new OMIPhysicsJointDrive
            {
                Type = type,
                Mode = OMIJointDriveMode.Force,
                Axis = axis,
                Stiffness = drive.positionSpring,
                Damping = drive.positionDamper,
                MaxForce = drive.maximumForce < float.MaxValue ? drive.maximumForce : null,
                PositionTarget = positionTarget != 0 ? positionTarget : null,
                VelocityTarget = velocityTarget != 0 ? velocityTarget : null
            };
        }

        private void ExtractHingeJointLimits(HingeJoint hinge, List<OMIPhysicsJointLimit> limits)
        {
            // Lock all linear axes
            limits.Add(new OMIPhysicsJointLimit
            {
                LinearAxes = new int[] { 0, 1, 2 },
                Min = 0,
                Max = 0
            });

            // Lock Y and Z angular axes (hinge only rotates around X in Unity's local space)
            limits.Add(new OMIPhysicsJointLimit
            {
                AngularAxes = new int[] { 1, 2 },
                Min = 0,
                Max = 0
            });

            // X axis angular limit
            if (hinge.useLimits)
            {
                var hingeLimits = hinge.limits;
                var spring = hinge.useSpring ? hinge.spring : default;
                
                limits.Add(new OMIPhysicsJointLimit
                {
                    AngularAxes = new int[] { 0 },
                    Min = hingeLimits.min * Mathf.Deg2Rad,
                    Max = hingeLimits.max * Mathf.Deg2Rad,
                    Stiffness = hinge.useSpring ? spring.spring : null,
                    Damping = hinge.useSpring ? spring.damper : 0
                });
            }
        }

        #endregion
    }
}
