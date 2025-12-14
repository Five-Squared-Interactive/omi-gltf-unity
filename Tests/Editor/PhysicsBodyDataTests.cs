// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI.Extensions.PhysicsBody;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for OMI_physics_body data parsing and conversion.
    /// </summary>
    [TestFixture]
    public class PhysicsBodyDataTests
    {
        [Test]
        public void StaticBody_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyData
            {
                Motion = new OMIPhysicsBodyMotionData
                {
                    Type = "static"
                }
            };

            Assert.AreEqual(OMIPhysicsBodyMotionType.Static, data.Motion.GetMotionType());
        }

        [Test]
        public void KinematicBody_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyData
            {
                Motion = new OMIPhysicsBodyMotionData
                {
                    Type = "kinematic"
                }
            };

            Assert.AreEqual(OMIPhysicsBodyMotionType.Kinematic, data.Motion.GetMotionType());
        }

        [Test]
        public void DynamicBody_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyData
            {
                Motion = new OMIPhysicsBodyMotionData
                {
                    Type = "dynamic",
                    Mass = 5f
                }
            };

            Assert.AreEqual(OMIPhysicsBodyMotionType.Dynamic, data.Motion.GetMotionType());
            Assert.AreEqual(5f, data.Motion.GetMass());
        }

        [Test]
        public void TriggerBody_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyData
            {
                Trigger = new OMIPhysicsBodyTriggerData()
            };

            Assert.IsNotNull(data.Trigger);
        }

        [Test]
        public void DefaultMass_IsOne()
        {
            var motionData = new OMIPhysicsBodyMotionData();
            
            Assert.AreEqual(1f, motionData.GetMass());
        }

        [Test]
        public void DefaultMotionType_IsStatic()
        {
            var motionData = new OMIPhysicsBodyMotionData();
            
            Assert.AreEqual(OMIPhysicsBodyMotionType.Static, motionData.GetMotionType());
        }

        [Test]
        public void LinearVelocity_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyMotionData
            {
                LinearVelocity = new float[] { 1f, 2f, 3f }
            };

            var velocity = data.GetLinearVelocity();
            Assert.AreEqual(1f, velocity.x);
            Assert.AreEqual(2f, velocity.y);
            Assert.AreEqual(3f, velocity.z);
        }

        [Test]
        public void AngularVelocity_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyMotionData
            {
                AngularVelocity = new float[] { 0.5f, 1f, 1.5f }
            };

            var velocity = data.GetAngularVelocity();
            Assert.AreEqual(0.5f, velocity.x);
            Assert.AreEqual(1f, velocity.y);
            Assert.AreEqual(1.5f, velocity.z);
        }

        [Test]
        public void DefaultLinearVelocity_IsZero()
        {
            var data = new OMIPhysicsBodyMotionData();
            var velocity = data.GetLinearVelocity();
            
            Assert.AreEqual(0f, velocity.x);
            Assert.AreEqual(0f, velocity.y);
            Assert.AreEqual(0f, velocity.z);
        }

        [Test]
        public void CenterOfMass_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyMotionData
            {
                CenterOfMass = new float[] { 0.1f, 0.2f, 0.3f }
            };

            var com = data.GetCenterOfMass();
            Assert.AreEqual(0.1f, com.x);
            Assert.AreEqual(0.2f, com.y);
            Assert.AreEqual(0.3f, com.z);
        }

        [Test]
        public void InertiaDiagonal_ParsesCorrectly()
        {
            var data = new OMIPhysicsBodyMotionData
            {
                InertiaDiagonal = new float[] { 1f, 2f, 3f }
            };

            var inertia = data.GetInertiaDiagonal();
            Assert.AreEqual(1f, inertia.x);
            Assert.AreEqual(2f, inertia.y);
            Assert.AreEqual(3f, inertia.z);
        }
    }
}
