// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI.Extensions.Audio;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for KHR_audio_emitter data parsing and conversion.
    /// </summary>
    [TestFixture]
    public class AudioEmitterDataTests
    {
        #region Audio Source Tests

        [Test]
        public void AudioSource_ParsesCorrectly()
        {
            var data = new KHRAudioSourceData
            {
                Name = "TestSound",
                Gain = 0.8f,
                AutoPlay = true,
                Loop = true,
                Audio = 0
            };

            Assert.AreEqual("TestSound", data.Name);
            Assert.AreEqual(0.8f, data.GetGain());
            Assert.IsTrue(data.GetAutoPlay());
            Assert.IsTrue(data.GetLoop());
            Assert.AreEqual(0, data.Audio);
        }

        [Test]
        public void AudioSource_DefaultValues_AreCorrect()
        {
            var data = new KHRAudioSourceData();

            Assert.AreEqual(1f, data.GetGain());
            Assert.IsFalse(data.GetAutoPlay());
            Assert.IsFalse(data.GetLoop());
        }

        #endregion

        #region Audio Emitter Tests

        [Test]
        public void AudioEmitter_Global_ParsesCorrectly()
        {
            var data = new KHRAudioEmitterData
            {
                Type = "global",
                Gain = 0.5f,
                Sources = new int[] { 0, 1 }
            };

            Assert.AreEqual(KHRAudioEmitterType.Global, data.GetEmitterType());
            Assert.AreEqual(0.5f, data.GetGain());
            Assert.AreEqual(2, data.Sources.Length);
        }

        [Test]
        public void AudioEmitter_Positional_ParsesCorrectly()
        {
            var data = new KHRAudioEmitterData
            {
                Type = "positional",
                Positional = new KHRAudioPositionalData
                {
                    ConeInnerAngle = 1.0f,
                    ConeOuterAngle = 2.0f,
                    ConeOuterGain = 0.3f,
                    DistanceModel = "linear",
                    MaxDistance = 100f,
                    RefDistance = 1f,
                    RolloffFactor = 1.5f
                }
            };

            Assert.AreEqual(KHRAudioEmitterType.Positional, data.GetEmitterType());
            Assert.IsNotNull(data.Positional);
            Assert.AreEqual(1.0f, data.Positional.GetConeInnerAngle());
            Assert.AreEqual(2.0f, data.Positional.GetConeOuterAngle());
            Assert.AreEqual(0.3f, data.Positional.GetConeOuterGain());
            Assert.AreEqual(KHRAudioDistanceModel.Linear, data.Positional.GetDistanceModel());
        }

        [Test]
        public void AudioEmitter_DefaultType_IsGlobal()
        {
            var data = new KHRAudioEmitterData();

            Assert.AreEqual(KHRAudioEmitterType.Global, data.GetEmitterType());
        }

        [Test]
        public void AudioEmitter_DefaultGain_IsOne()
        {
            var data = new KHRAudioEmitterData();

            Assert.AreEqual(1f, data.GetGain());
        }

        #endregion

        #region Positional Audio Tests

        [Test]
        public void PositionalAudio_DefaultValues_AreCorrect()
        {
            var data = new KHRAudioPositionalData();

            Assert.AreEqual(6.283185f, data.GetConeInnerAngle(), 0.001f); // 2*PI
            Assert.AreEqual(6.283185f, data.GetConeOuterAngle(), 0.001f);
            Assert.AreEqual(0f, data.GetConeOuterGain());
            Assert.AreEqual(KHRAudioDistanceModel.Inverse, data.GetDistanceModel());
            Assert.AreEqual(float.PositiveInfinity, data.GetMaxDistance());
            Assert.AreEqual(1f, data.GetRefDistance());
            Assert.AreEqual(1f, data.GetRolloffFactor());
        }

        [Test]
        public void DistanceModel_ParsesCorrectly()
        {
            var linearData = new KHRAudioPositionalData { DistanceModel = "linear" };
            Assert.AreEqual(KHRAudioDistanceModel.Linear, linearData.GetDistanceModel());

            var inverseData = new KHRAudioPositionalData { DistanceModel = "inverse" };
            Assert.AreEqual(KHRAudioDistanceModel.Inverse, inverseData.GetDistanceModel());

            var exponentialData = new KHRAudioPositionalData { DistanceModel = "exponential" };
            Assert.AreEqual(KHRAudioDistanceModel.Exponential, exponentialData.GetDistanceModel());
        }

        [Test]
        public void DistanceModel_UnknownValue_DefaultsToInverse()
        {
            var data = new KHRAudioPositionalData { DistanceModel = "unknown" };
            
            Assert.AreEqual(KHRAudioDistanceModel.Inverse, data.GetDistanceModel());
        }

        #endregion

        #region Audio Data Tests

        [Test]
        public void AudioData_ParsesCorrectly()
        {
            var data = new KHRAudioData
            {
                Uri = "sounds/explosion.ogg",
                MimeType = "audio/ogg",
                BufferView = null
            };

            Assert.AreEqual("sounds/explosion.ogg", data.Uri);
            Assert.AreEqual("audio/ogg", data.MimeType);
            Assert.IsNull(data.BufferView);
        }

        [Test]
        public void AudioData_BufferView_ParsesCorrectly()
        {
            var data = new KHRAudioData
            {
                BufferView = 5,
                MimeType = "audio/ogg"
            };

            Assert.AreEqual(5, data.BufferView);
            Assert.IsNull(data.Uri);
        }

        #endregion

        #region Root Extension Tests

        [Test]
        public void RootExtension_ParsesCorrectly()
        {
            var data = new KHRAudioEmitterRoot
            {
                Audio = new KHRAudioData[]
                {
                    new KHRAudioData { Uri = "sound1.ogg" },
                    new KHRAudioData { Uri = "sound2.ogg" }
                },
                Sources = new KHRAudioSourceData[]
                {
                    new KHRAudioSourceData { Audio = 0 },
                    new KHRAudioSourceData { Audio = 1 }
                },
                Emitters = new KHRAudioEmitterData[]
                {
                    new KHRAudioEmitterData { Type = "global", Sources = new int[] { 0 } }
                }
            };

            Assert.AreEqual(2, data.Audio.Length);
            Assert.AreEqual(2, data.Sources.Length);
            Assert.AreEqual(1, data.Emitters.Length);
        }

        #endregion
    }
}
