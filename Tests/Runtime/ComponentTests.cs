// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OMI.Extensions.EnvironmentSky;
using OMI.Extensions.PhysicsShape;
using OMI.Extensions.PhysicsBody;
using OMI.Extensions.SpawnPoint;
using OMI.Extensions.Seat;
using OMI.Extensions.Link;

namespace OMI.Tests.Runtime
{
    /// <summary>
    /// Runtime tests for OMI component functionality.
    /// </summary>
    [TestFixture]
    public class ComponentTests
    {
        private GameObject _testObject;

        [SetUp]
        public void Setup()
        {
            _testObject = new GameObject("TestObject");
        }

        [TearDown]
        public void TearDown()
        {
            if (_testObject != null)
            {
                Object.DestroyImmediate(_testObject);
            }
        }

        #region Environment Sky Component Tests

        [Test]
        public void EnvironmentSky_Component_CanBeAdded()
        {
            var component = _testObject.AddComponent<OMIEnvironmentSky>();
            Assert.IsNotNull(component);
        }

        [Test]
        public void EnvironmentSky_ToData_PreservesValues()
        {
            var component = _testObject.AddComponent<OMIEnvironmentSky>();
            component.skyType = OMIEnvironmentSkyType.Gradient;
            component.ambientLightColor = new Color(0.8f, 0.9f, 1f);
            component.ambientSkyContribution = 0.75f;
            component.gradient.topColor = new Color(0.2f, 0.4f, 0.9f);

            var data = component.ToData();

            Assert.AreEqual("gradient", data.Type);
            Assert.AreEqual(0.75f, data.AmbientSkyContribution);
            Assert.IsNotNull(data.Gradient);
            Assert.AreEqual(0.2f, data.Gradient.TopColor[0], 0.001f);
        }

        [Test]
        public void EnvironmentSky_FromData_PreservesValues()
        {
            var component = _testObject.AddComponent<OMIEnvironmentSky>();
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "plain",
                Plain = new OMIEnvironmentSkyPlainData
                {
                    Color = new float[] { 0.1f, 0.2f, 0.3f }
                },
                AmbientLightColor = new float[] { 0.5f, 0.5f, 0.5f },
                AmbientSkyContribution = 0.6f
            };

            component.FromData(data);

            Assert.AreEqual(OMIEnvironmentSkyType.Plain, component.skyType);
            Assert.AreEqual(0.6f, component.ambientSkyContribution, 0.001f);
            Assert.AreEqual(0.1f, component.plain.color.r, 0.001f);
        }

        [Test]
        public void EnvironmentSky_RoundTrip_PreservesData()
        {
            var component = _testObject.AddComponent<OMIEnvironmentSky>();
            component.skyType = OMIEnvironmentSkyType.Physical;
            component.physical.groundColor = new Color(0.3f, 0.25f, 0.15f);
            component.physical.mieAnisotropy = 0.75f;
            component.physical.rayleighScale = 0.00002f;

            var data = component.ToData();
            
            var component2 = new GameObject("Test2").AddComponent<OMIEnvironmentSky>();
            component2.FromData(data);

            Assert.AreEqual(component.skyType, component2.skyType);
            Assert.AreEqual(component.physical.mieAnisotropy, component2.physical.mieAnisotropy, 0.001f);
            Assert.AreEqual(component.physical.rayleighScale, component2.physical.rayleighScale, 0.00001f);

            Object.DestroyImmediate(component2.gameObject);
        }

        #endregion

        #region Physics Components Tests

        [Test]
        public void PhysicsShape_Component_CanBeAdded()
        {
            var component = _testObject.AddComponent<OMIPhysicsShapeComponent>();
            Assert.IsNotNull(component);
        }

        [Test]
        public void PhysicsBody_Component_CanBeAdded()
        {
            var component = _testObject.AddComponent<OMIPhysicsBodyComponent>();
            Assert.IsNotNull(component);
        }

        #endregion

        #region Interaction Components Tests

        [Test]
        public void SpawnPoint_Component_CanBeAdded()
        {
            var component = _testObject.AddComponent<OMISpawnPointComponent>();
            Assert.IsNotNull(component);
        }

        [Test]
        public void Seat_Component_CanBeAdded()
        {
            var component = _testObject.AddComponent<OMISeatComponent>();
            Assert.IsNotNull(component);
        }

        [Test]
        public void Link_Component_CanBeAdded()
        {
            var component = _testObject.AddComponent<OMILinkComponent>();
            Assert.IsNotNull(component);
        }

        #endregion
    }
}
