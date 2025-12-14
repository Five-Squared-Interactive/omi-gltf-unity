// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI.Extensions.EnvironmentSky;
using UnityEngine;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for OMI_environment_sky data parsing and conversion.
    /// </summary>
    [TestFixture]
    public class EnvironmentSkyDataTests
    {
        [Test]
        public void GradientSky_ParsesCorrectly()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "gradient",
                Gradient = new OMIEnvironmentSkyGradientData
                {
                    TopColor = new float[] { 0.2f, 0.4f, 0.9f },
                    HorizonColor = new float[] { 0.7f, 0.8f, 0.95f },
                    BottomColor = new float[] { 0.4f, 0.35f, 0.3f }
                }
            };

            Assert.AreEqual(OMIEnvironmentSkyType.Gradient, data.GetSkyType());
            Assert.IsNotNull(data.Gradient);
            
            var (tr, tg, tb) = data.Gradient.GetTopColor();
            Assert.AreEqual(0.2f, tr, 0.001f);
            Assert.AreEqual(0.4f, tg, 0.001f);
            Assert.AreEqual(0.9f, tb, 0.001f);
        }

        [Test]
        public void PanoramaSky_ParsesCorrectly()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "panorama",
                Panorama = new OMIEnvironmentSkyPanoramaData
                {
                    Equirectangular = 0
                }
            };

            Assert.AreEqual(OMIEnvironmentSkyType.Panorama, data.GetSkyType());
            Assert.IsNotNull(data.Panorama);
            Assert.IsTrue(data.Panorama.HasEquirectangular);
            Assert.AreEqual(0, data.Panorama.Equirectangular);
        }

        [Test]
        public void PanoramaSky_Cubemap_ParsesCorrectly()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "panorama",
                Panorama = new OMIEnvironmentSkyPanoramaData
                {
                    Cubemap = new int[] { 0, 1, 2, 3, 4, 5 }
                }
            };

            Assert.IsTrue(data.Panorama.HasCubemap);
            Assert.AreEqual(6, data.Panorama.Cubemap.Length);
        }

        [Test]
        public void PhysicalSky_ParsesCorrectly()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "physical",
                Physical = new OMIEnvironmentSkyPhysicalData
                {
                    GroundColor = new float[] { 0.3f, 0.2f, 0.1f },
                    MieAnisotropy = 0.8f,
                    MieColor = new float[] { 1f, 1f, 1f },
                    MieScale = 0.000005f,
                    RayleighColor = new float[] { 0.3f, 0.5f, 1f },
                    RayleighScale = 0.00003f
                }
            };

            Assert.AreEqual(OMIEnvironmentSkyType.Physical, data.GetSkyType());
            Assert.IsNotNull(data.Physical);
            Assert.AreEqual(0.8f, data.Physical.GetMieAnisotropy());
            Assert.AreEqual(0.000005f, data.Physical.GetMieScale());
            Assert.AreEqual(0.00003f, data.Physical.GetRayleighScale());
        }

        [Test]
        public void PlainSky_ParsesCorrectly()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = "plain",
                Plain = new OMIEnvironmentSkyPlainData
                {
                    Color = new float[] { 0.1f, 0.1f, 0.15f }
                }
            };

            Assert.AreEqual(OMIEnvironmentSkyType.Plain, data.GetSkyType());
            Assert.IsNotNull(data.Plain);
            
            var (r, g, b) = data.Plain.GetColor();
            Assert.AreEqual(0.1f, r, 0.001f);
            Assert.AreEqual(0.1f, g, 0.001f);
            Assert.AreEqual(0.15f, b, 0.001f);
        }

        [Test]
        public void AmbientLight_ParsesCorrectly()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                AmbientLightColor = new float[] { 0.8f, 0.9f, 1f },
                AmbientSkyContribution = 0.75f
            };

            var (r, g, b) = data.GetAmbientLightColor();
            Assert.AreEqual(0.8f, r, 0.001f);
            Assert.AreEqual(0.9f, g, 0.001f);
            Assert.AreEqual(1f, b, 0.001f);
            Assert.AreEqual(0.75f, data.GetAmbientSkyContribution());
        }

        [Test]
        public void DefaultAmbientLight_IsWhite()
        {
            var data = new OMIEnvironmentSkySkyData();
            var (r, g, b) = data.GetAmbientLightColor();
            
            Assert.AreEqual(1f, r);
            Assert.AreEqual(1f, g);
            Assert.AreEqual(1f, b);
        }

        [Test]
        public void DefaultAmbientContribution_IsOne()
        {
            var data = new OMIEnvironmentSkySkyData();
            
            Assert.AreEqual(1f, data.GetAmbientSkyContribution());
        }

        [Test]
        public void GradientSky_DefaultColors_AreCorrect()
        {
            var gradient = new OMIEnvironmentSkyGradientData();

            var (tr, tg, tb) = gradient.GetTopColor();
            Assert.AreEqual(0.3f, tr);
            Assert.AreEqual(0.5f, tg);
            Assert.AreEqual(1f, tb);

            var (hr, hg, hb) = gradient.GetHorizonColor();
            Assert.AreEqual(0.8f, hr);
            Assert.AreEqual(0.8f, hg);
            Assert.AreEqual(0.9f, hb);

            var (br, bg, bb) = gradient.GetBottomColor();
            Assert.AreEqual(0.3f, br);
            Assert.AreEqual(0.3f, bg);
            Assert.AreEqual(0.3f, bb);
        }

        [Test]
        public void GradientSky_DefaultCurves_AreCorrect()
        {
            var gradient = new OMIEnvironmentSkyGradientData();

            Assert.AreEqual(0.2f, gradient.GetTopCurve());
            Assert.AreEqual(0.2f, gradient.GetBottomCurve());
            Assert.AreEqual(0.5f, gradient.GetSunAngleMax());
            Assert.AreEqual(0.15f, gradient.GetSunCurve());
        }

        [Test]
        public void PhysicalSky_DefaultValues_AreCorrect()
        {
            var physical = new OMIEnvironmentSkyPhysicalData();

            Assert.AreEqual(0.8f, physical.GetMieAnisotropy());
            Assert.AreEqual(0.000005f, physical.GetMieScale());
            Assert.AreEqual(0.00003f, physical.GetRayleighScale());

            var (gr, gg, gb) = physical.GetGroundColor();
            Assert.AreEqual(0.3f, gr);
            Assert.AreEqual(0.2f, gg);
            Assert.AreEqual(0.1f, gb);
        }

        [Test]
        public void PlainSky_DefaultColor_IsBlack()
        {
            var plain = new OMIEnvironmentSkyPlainData();
            var (r, g, b) = plain.GetColor();
            
            Assert.AreEqual(0f, r);
            Assert.AreEqual(0f, g);
            Assert.AreEqual(0f, b);
        }

        [Test]
        public void SkyType_InfersFromData_WhenTypeNull()
        {
            // Gradient
            var gradientData = new OMIEnvironmentSkySkyData { Gradient = new OMIEnvironmentSkyGradientData() };
            Assert.AreEqual(OMIEnvironmentSkyType.Gradient, gradientData.GetSkyType());

            // Panorama
            var panoramaData = new OMIEnvironmentSkySkyData { Panorama = new OMIEnvironmentSkyPanoramaData() };
            Assert.AreEqual(OMIEnvironmentSkyType.Panorama, panoramaData.GetSkyType());

            // Physical
            var physicalData = new OMIEnvironmentSkySkyData { Physical = new OMIEnvironmentSkyPhysicalData() };
            Assert.AreEqual(OMIEnvironmentSkyType.Physical, physicalData.GetSkyType());

            // Plain
            var plainData = new OMIEnvironmentSkySkyData { Plain = new OMIEnvironmentSkyPlainData() };
            Assert.AreEqual(OMIEnvironmentSkyType.Plain, plainData.GetSkyType());
        }

        [Test]
        public void SkyTypeToString_ConvertsCorrectly()
        {
            Assert.AreEqual("gradient", OMIEnvironmentSkyType.Gradient.ToTypeString());
            Assert.AreEqual("panorama", OMIEnvironmentSkyType.Panorama.ToTypeString());
            Assert.AreEqual("physical", OMIEnvironmentSkyType.Physical.ToTypeString());
            Assert.AreEqual("plain", OMIEnvironmentSkyType.Plain.ToTypeString());
        }

        [Test]
        public void DocumentData_Skies_DefaultsToEmptyList()
        {
            var docData = new OMIEnvironmentSkyDocumentData();
            
            Assert.IsNotNull(docData.Skies);
            Assert.AreEqual(0, docData.Skies.Count);
        }

        [Test]
        public void SceneData_DefaultSkyIndex_IsZero()
        {
            var sceneData = new OMIEnvironmentSkySceneData();
            
            Assert.AreEqual(0, sceneData.Sky);
        }
    }
}
