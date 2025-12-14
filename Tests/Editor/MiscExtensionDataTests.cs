// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI.Extensions.PhysicsGravity;
using OMI.Extensions.SpawnPoint;
using OMI.Extensions.Seat;
using OMI.Extensions.Link;
using OMI.Extensions.Personality;
using UnityEngine;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for smaller OMI extensions data parsing.
    /// </summary>
    [TestFixture]
    public class MiscExtensionDataTests
    {
        #region Physics Gravity Tests

        [Test]
        public void PhysicsGravity_DirectionalGravity_ParsesCorrectly()
        {
            var data = new OMIPhysicsGravityData
            {
                Type = "directional",
                Directional = new OMIPhysicsGravityDirectionalData
                {
                    Gravity = new float[] { 0f, -9.81f, 0f }
                }
            };

            Assert.AreEqual(OMIPhysicsGravityType.Directional, data.GetGravityType());
            Assert.IsNotNull(data.Directional);
            
            var gravity = data.Directional.GetGravity();
            Assert.AreEqual(0f, gravity.x);
            Assert.AreEqual(-9.81f, gravity.y, 0.001f);
            Assert.AreEqual(0f, gravity.z);
        }

        [Test]
        public void PhysicsGravity_PointGravity_ParsesCorrectly()
        {
            var data = new OMIPhysicsGravityData
            {
                Type = "point",
                Point = new OMIPhysicsGravityPointData
                {
                    UnitDistance = 10f
                }
            };

            Assert.AreEqual(OMIPhysicsGravityType.Point, data.GetGravityType());
            Assert.IsNotNull(data.Point);
            Assert.AreEqual(10f, data.Point.GetUnitDistance());
        }

        [Test]
        public void PhysicsGravity_DefaultType_IsDirectional()
        {
            var data = new OMIPhysicsGravityData();
            
            Assert.AreEqual(OMIPhysicsGravityType.Directional, data.GetGravityType());
        }

        [Test]
        public void PhysicsGravity_DefaultGravity_IsEarthlike()
        {
            var data = new OMIPhysicsGravityDirectionalData();
            var gravity = data.GetGravity();
            
            Assert.AreEqual(0f, gravity.x);
            Assert.AreEqual(-9.81f, gravity.y, 0.001f);
            Assert.AreEqual(0f, gravity.z);
        }

        #endregion

        #region Spawn Point Tests

        [Test]
        public void SpawnPoint_ParsesCorrectly()
        {
            var data = new OMISpawnPointData
            {
                Team = "blue",
                Group = "defenders"
            };

            Assert.AreEqual("blue", data.Team);
            Assert.AreEqual("defenders", data.Group);
        }

        [Test]
        public void SpawnPoint_NullValues_AreAllowed()
        {
            var data = new OMISpawnPointData();
            
            Assert.IsNull(data.Team);
            Assert.IsNull(data.Group);
        }

        #endregion

        #region Seat Tests

        [Test]
        public void Seat_ParsesCorrectly()
        {
            var data = new OMISeatData
            {
                Back = 0,
                Foot = 1,
                Knee = 2,
                Angle = 1.57f
            };

            Assert.AreEqual(0, data.Back);
            Assert.AreEqual(1, data.Foot);
            Assert.AreEqual(2, data.Knee);
            Assert.AreEqual(1.57f, data.GetAngle(), 0.001f);
        }

        [Test]
        public void Seat_DefaultAngle_IsCorrect()
        {
            var data = new OMISeatData();
            
            // Default angle (typically 90 degrees in radians = π/2)
            Assert.AreEqual(1.5707963f, data.GetAngle(), 0.001f);
        }

        #endregion

        #region Link Tests

        [Test]
        public void Link_ParsesCorrectly()
        {
            var data = new OMILinkData
            {
                Uri = "https://example.com/resource",
                Title = "Example Resource",
                Description = "A test link"
            };

            Assert.AreEqual("https://example.com/resource", data.Uri);
            Assert.AreEqual("Example Resource", data.Title);
            Assert.AreEqual("A test link", data.Description);
        }

        [Test]
        public void Link_MinimalData_ParsesCorrectly()
        {
            var data = new OMILinkData
            {
                Uri = "https://example.com"
            };

            Assert.AreEqual("https://example.com", data.Uri);
            Assert.IsNull(data.Title);
            Assert.IsNull(data.Description);
        }

        #endregion

        #region Personality Tests

        [Test]
        public void Personality_ParsesCorrectly()
        {
            var data = new OMIPersonalityData
            {
                Agent = "character_ai",
                DefaultMessage = "Hello, I'm a helpful assistant.",
                Traits = new string[] { "friendly", "knowledgeable" }
            };

            Assert.AreEqual("character_ai", data.Agent);
            Assert.AreEqual("Hello, I'm a helpful assistant.", data.DefaultMessage);
            Assert.AreEqual(2, data.Traits.Length);
            Assert.Contains("friendly", data.Traits);
        }

        [Test]
        public void Personality_NullTraits_IsAllowed()
        {
            var data = new OMIPersonalityData
            {
                Agent = "simple_ai"
            };

            Assert.AreEqual("simple_ai", data.Agent);
            Assert.IsNull(data.Traits);
        }

        #endregion
    }
}
