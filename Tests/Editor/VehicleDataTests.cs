// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI.Extensions.Vehicle;
using UnityEngine;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for OMI_vehicle_* data parsing and conversion.
    /// </summary>
    [TestFixture]
    public class VehicleDataTests
    {
        #region Vehicle Body Tests

        [Test]
        public void VehicleBody_ParsesCorrectly()
        {
            var data = new OMIVehicleBodyData
            {
                GyroStabilize = 0.5f,
                LinearDampening = 0.1f,
                AngularDampening = 0.2f,
                PilotSeat = 0,
                Seats = new int[] { 0, 1, 2 }
            };

            Assert.AreEqual(0.5f, data.GetGyroStabilize());
            Assert.AreEqual(0.1f, data.GetLinearDampening());
            Assert.AreEqual(0.2f, data.GetAngularDampening());
            Assert.AreEqual(0, data.PilotSeat);
            Assert.AreEqual(3, data.Seats.Length);
        }

        [Test]
        public void VehicleBody_DefaultValues_AreCorrect()
        {
            var data = new OMIVehicleBodyData();

            Assert.AreEqual(0f, data.GetGyroStabilize());
            Assert.AreEqual(0f, data.GetLinearDampening());
            Assert.AreEqual(0f, data.GetAngularDampening());
        }

        #endregion

        #region Vehicle Wheel Tests

        [Test]
        public void VehicleWheel_ParsesCorrectly()
        {
            var data = new OMIVehicleWheelData
            {
                Radius = 0.35f,
                MaxSteerAngle = 0.5f,
                Powered = true,
                MaxForce = 5000f,
                SuspensionStiffness = 30000f,
                SuspensionDamping = 4000f,
                SuspensionTravel = 0.2f
            };

            Assert.AreEqual(0.35f, data.GetRadius());
            Assert.AreEqual(0.5f, data.GetMaxSteerAngle());
            Assert.IsTrue(data.GetPowered());
            Assert.AreEqual(5000f, data.GetMaxForce());
            Assert.AreEqual(30000f, data.GetSuspensionStiffness());
            Assert.AreEqual(4000f, data.GetSuspensionDamping());
            Assert.AreEqual(0.2f, data.GetSuspensionTravel());
        }

        [Test]
        public void VehicleWheel_DefaultValues_AreCorrect()
        {
            var data = new OMIVehicleWheelData();

            Assert.AreEqual(0.25f, data.GetRadius());
            Assert.AreEqual(0f, data.GetMaxSteerAngle());
            Assert.IsFalse(data.GetPowered());
            Assert.AreEqual(1000f, data.GetMaxForce());
            Assert.AreEqual(20000f, data.GetSuspensionStiffness());
            Assert.AreEqual(3000f, data.GetSuspensionDamping());
            Assert.AreEqual(0.15f, data.GetSuspensionTravel());
        }

        [Test]
        public void VehicleWheel_MaxSteerAngle_InRadians()
        {
            var data = new OMIVehicleWheelData
            {
                MaxSteerAngle = Mathf.PI / 4f // 45 degrees
            };

            Assert.AreEqual(Mathf.PI / 4f, data.GetMaxSteerAngle(), 0.001f);
        }

        #endregion

        #region Vehicle Thruster Tests

        [Test]
        public void VehicleThruster_ParsesCorrectly()
        {
            var data = new OMIVehicleThrusterData
            {
                MaxForce = 10000f,
                MaxGimbal = 0.2f
            };

            Assert.AreEqual(10000f, data.GetMaxForce());
            Assert.AreEqual(0.2f, data.GetMaxGimbal());
        }

        [Test]
        public void VehicleThruster_DefaultValues_AreCorrect()
        {
            var data = new OMIVehicleThrusterData();

            Assert.AreEqual(1000f, data.GetMaxForce());
            Assert.AreEqual(0f, data.GetMaxGimbal());
        }

        [Test]
        public void VehicleThruster_CurrentThrottle_Clamps()
        {
            var data = new OMIVehicleThrusterData
            {
                CurrentThrottle = 1.5f
            };

            Assert.AreEqual(1.5f, data.GetCurrentThrottle());
        }

        #endregion

        #region Vehicle Hover Thruster Tests

        [Test]
        public void VehicleHoverThruster_ParsesCorrectly()
        {
            var data = new OMIVehicleHoverThrusterData
            {
                MaxForce = 5000f,
                HoverHeight = 2f,
                HoverDamping = 0.5f
            };

            Assert.AreEqual(5000f, data.GetMaxForce());
            Assert.AreEqual(2f, data.GetHoverHeight());
            Assert.AreEqual(0.5f, data.GetHoverDamping());
        }

        [Test]
        public void VehicleHoverThruster_DefaultValues_AreCorrect()
        {
            var data = new OMIVehicleHoverThrusterData();

            Assert.AreEqual(1000f, data.GetMaxForce());
            Assert.AreEqual(1f, data.GetHoverHeight());
            Assert.AreEqual(0.3f, data.GetHoverDamping());
        }

        #endregion
    }
}
