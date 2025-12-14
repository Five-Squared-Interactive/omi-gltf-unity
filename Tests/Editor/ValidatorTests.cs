// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI;
using OMI.Extensions.PhysicsShape;
using OMI.Extensions.PhysicsBody;
using OMI.Extensions.PhysicsJoint;
using OMI.Extensions.EnvironmentSky;
using OMI.Extensions.Vehicle;
using OMI.Extensions.Audio;
using OMI.Extensions.PhysicsGravity;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for OMIValidator validation methods.
    /// </summary>
    [TestFixture]
    public class ValidatorTests
    {
        #region Environment Sky Validation

        [Test]
        public void ValidateEnvironmentSky_ValidGradient_Passes()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "gradient",
                Gradient = new OMIEnvironmentSkyGradientData
                {
                    TopColor = new float[] { 0.3f, 0.5f, 1.0f },
                    HorizonColor = new float[] { 0.8f, 0.8f, 0.9f },
                    BottomColor = new float[] { 0.3f, 0.3f, 0.3f }
                }
            };

            var result = OMIValidator.ValidateEnvironmentSky(data);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateEnvironmentSky_GradientWithNullData_Fails()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "gradient",
                Gradient = null
            };

            var result = OMIValidator.ValidateEnvironmentSky(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("gradient data is null"));
        }

        [Test]
        public void ValidateEnvironmentSky_NegativeCurve_Fails()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "gradient",
                Gradient = new OMIEnvironmentSkyGradientData
                {
                    TopCurve = -0.5f
                }
            };

            var result = OMIValidator.ValidateEnvironmentSky(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("Top curve must be positive"));
        }

        [Test]
        public void ValidateEnvironmentSky_PanoramaWithoutTexture_Fails()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "panorama",
                Panorama = new OMIEnvironmentSkyPanoramaData()
            };

            var result = OMIValidator.ValidateEnvironmentSky(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("cubemap or equirectangular"));
        }

        [Test]
        public void ValidateEnvironmentSky_InvalidMieAnisotropy_Fails()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "physical",
                Physical = new OMIEnvironmentSkyPhysicalData
                {
                    MieAnisotropy = 1.5f // Out of range
                }
            };

            var result = OMIValidator.ValidateEnvironmentSky(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("Mie anisotropy must be between -1 and 1"));
        }

        [Test]
        public void ValidateEnvironmentSky_ValidPlain_Passes()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "plain",
                Plain = new OMIEnvironmentSkyPlainData
                {
                    Color = new float[] { 0.1f, 0.1f, 0.2f }
                }
            };

            var result = OMIValidator.ValidateEnvironmentSky(data);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateEnvironmentSky_ColorOutOfRange_Warns()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "plain",
                Plain = new OMIEnvironmentSkyPlainData
                {
                    Color = new float[] { 1.5f, 0.5f, 0.5f } // Red > 1
                }
            };

            var result = OMIValidator.ValidateEnvironmentSky(data);
            Assert.IsTrue(result.IsValid); // Warnings don't fail
            Assert.That(result.Warnings, Has.Count.GreaterThan(0));
        }

        #endregion

        #region Vehicle Validation

        [Test]
        public void ValidateVehicleBody_ValidData_Passes()
        {
            var data = new OMIVehicleBodyData
            {
                GyroStabilize = 0.5f,
                LinearDampening = 0.1f,
                AngularDampening = 0.2f
            };

            var result = OMIValidator.ValidateVehicleBody(data);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateVehicleBody_NegativeDampening_Fails()
        {
            var data = new OMIVehicleBodyData
            {
                LinearDampening = -0.1f
            };

            var result = OMIValidator.ValidateVehicleBody(data);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidateVehicleWheel_ValidData_Passes()
        {
            var data = new OMIVehicleWheelData
            {
                Radius = 0.35f,
                MaxForce = 5000f,
                SuspensionStiffness = 30000f
            };

            var result = OMIValidator.ValidateVehicleWheel(data);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateVehicleWheel_NegativeRadius_Fails()
        {
            var data = new OMIVehicleWheelData
            {
                Radius = -0.5f
            };

            var result = OMIValidator.ValidateVehicleWheel(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("radius must be positive"));
        }

        [Test]
        public void ValidateVehicleThruster_ValidData_Passes()
        {
            var data = new OMIVehicleThrusterData
            {
                MaxForce = 10000f,
                MaxGimbal = 0.2f
            };

            var result = OMIValidator.ValidateVehicleThruster(data);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateVehicleHoverThruster_ZeroHeight_Fails()
        {
            var data = new OMIVehicleHoverThrusterData
            {
                HoverHeight = 0f
            };

            var result = OMIValidator.ValidateVehicleHoverThruster(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("Hover height must be positive"));
        }

        #endregion

        #region Audio Validation

        [Test]
        public void ValidateAudioEmitter_ValidGlobal_Passes()
        {
            var data = new KHRAudioEmitterData
            {
                Type = "global",
                Gain = 0.8f
            };

            var result = OMIValidator.ValidateAudioEmitter(data);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateAudioEmitter_NegativeGain_Fails()
        {
            var data = new KHRAudioEmitterData
            {
                Gain = -0.5f
            };

            var result = OMIValidator.ValidateAudioEmitter(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("Gain must be non-negative"));
        }

        [Test]
        public void ValidateAudioEmitter_HighGain_Warns()
        {
            var data = new KHRAudioEmitterData
            {
                Gain = 15f
            };

            var result = OMIValidator.ValidateAudioEmitter(data);
            Assert.IsTrue(result.IsValid);
            Assert.That(result.Warnings, Has.Some.Contains("high gain"));
        }

        [Test]
        public void ValidateAudioEmitter_InvalidConeAngles_Warns()
        {
            var data = new KHRAudioEmitterData
            {
                Type = "positional",
                Positional = new KHRAudioPositionalData
                {
                    ConeInnerAngle = 2f,
                    ConeOuterAngle = 1f // Inner > Outer
                }
            };

            var result = OMIValidator.ValidateAudioEmitter(data);
            Assert.That(result.Warnings, Has.Some.Contains("inner angle is greater than outer"));
        }

        #endregion

        #region Physics Joint Validation

        [Test]
        public void ValidatePhysicsJoint_ValidData_Passes()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = 1,
                Constraints = new OMIPhysicsJointConstraint[]
                {
                    new OMIPhysicsJointConstraint
                    {
                        AngularAxes = new int[] { 0 },
                        LowerLimit = -1.57f,
                        UpperLimit = 1.57f
                    }
                }
            };

            var result = OMIValidator.ValidatePhysicsJoint(data, 10);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidatePhysicsJoint_NegativeNodeIndex_Fails()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = -1
            };

            var result = OMIValidator.ValidatePhysicsJoint(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("non-negative"));
        }

        [Test]
        public void ValidatePhysicsJoint_InvalidLimits_Fails()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = 1,
                Constraints = new OMIPhysicsJointConstraint[]
                {
                    new OMIPhysicsJointConstraint
                    {
                        AngularAxes = new int[] { 0 },
                        LowerLimit = 2f,
                        UpperLimit = 1f // Lower > Upper
                    }
                }
            };

            var result = OMIValidator.ValidatePhysicsJoint(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("lower limit"));
        }

        [Test]
        public void ValidatePhysicsJoint_NegativeStiffness_Fails()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = 1,
                Constraints = new OMIPhysicsJointConstraint[]
                {
                    new OMIPhysicsJointConstraint
                    {
                        AngularAxes = new int[] { 0 },
                        Stiffness = -100f
                    }
                }
            };

            var result = OMIValidator.ValidatePhysicsJoint(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("stiffness must be non-negative"));
        }

        #endregion

        #region Physics Gravity Validation

        [Test]
        public void ValidatePhysicsGravity_ValidDirectional_Passes()
        {
            var data = new OMIPhysicsGravityData
            {
                Type = "directional",
                Directional = new OMIPhysicsGravityDirectionalData
                {
                    Gravity = new float[] { 0f, -9.81f, 0f }
                }
            };

            var result = OMIValidator.ValidatePhysicsGravity(data);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidatePhysicsGravity_PointWithInvalidDistance_Fails()
        {
            var data = new OMIPhysicsGravityData
            {
                Type = "point",
                Point = new OMIPhysicsGravityPointData
                {
                    UnitDistance = -5f
                }
            };

            var result = OMIValidator.ValidatePhysicsGravity(data);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("unit distance must be positive"));
        }

        #endregion

        #region Null Data Tests

        [Test]
        public void ValidateEnvironmentSky_NullData_Fails()
        {
            var result = OMIValidator.ValidateEnvironmentSky(null);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidateVehicleBody_NullData_Fails()
        {
            var result = OMIValidator.ValidateVehicleBody(null);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidateVehicleWheel_NullData_Fails()
        {
            var result = OMIValidator.ValidateVehicleWheel(null);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidateAudioEmitter_NullData_Fails()
        {
            var result = OMIValidator.ValidateAudioEmitter(null);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidatePhysicsJoint_NullData_Fails()
        {
            var result = OMIValidator.ValidatePhysicsJoint(null);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidatePhysicsGravity_NullData_Fails()
        {
            var result = OMIValidator.ValidatePhysicsGravity(null);
            Assert.IsFalse(result.IsValid);
        }

        #endregion
    }
}
