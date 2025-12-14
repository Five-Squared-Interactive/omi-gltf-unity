// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

namespace OMI.Extensions.Audio
{
    /// <summary>
    /// Interface for components that provide audio emitter functionality.
    /// </summary>
    public interface IKHRAudioEmitter
    {
        /// <summary>
        /// Gets the type of audio emitter (global or positional).
        /// </summary>
        AudioEmitterType EmitterType { get; }

        /// <summary>
        /// Gets the volume gain multiplier.
        /// </summary>
        float Gain { get; }

        /// <summary>
        /// Gets the source indices for this emitter.
        /// </summary>
        int[] SourceIndices { get; }

        /// <summary>
        /// Plays the audio.
        /// </summary>
        void Play();

        /// <summary>
        /// Stops the audio.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the audio.
        /// </summary>
        void Pause();

        /// <summary>
        /// Gets whether the audio is currently playing.
        /// </summary>
        bool IsPlaying { get; }
    }

    /// <summary>
    /// Interface for components that provide positional audio functionality.
    /// </summary>
    public interface IKHRAudioPositional : IKHRAudioEmitter
    {
        /// <summary>
        /// Gets the shape type of the positional emitter.
        /// </summary>
        AudioShapeType ShapeType { get; }

        /// <summary>
        /// Gets the distance model used for attenuation.
        /// </summary>
        AudioDistanceModel DistanceModel { get; }

        /// <summary>
        /// Gets the reference distance (no attenuation below this distance).
        /// </summary>
        float ReferenceDistance { get; }

        /// <summary>
        /// Gets the maximum distance (audio is silent beyond this, or no max if 0).
        /// </summary>
        float MaxDistance { get; }

        /// <summary>
        /// Gets the rolloff factor for distance attenuation.
        /// </summary>
        float RolloffFactor { get; }

        /// <summary>
        /// Gets the inner cone angle in radians (no attenuation inside).
        /// </summary>
        float ConeInnerAngle { get; }

        /// <summary>
        /// Gets the outer cone angle in radians (full attenuation outside).
        /// </summary>
        float ConeOuterAngle { get; }

        /// <summary>
        /// Gets the volume gain outside the outer cone.
        /// </summary>
        float ConeOuterGain { get; }
    }

    /// <summary>
    /// Interface for components that represent an audio source.
    /// </summary>
    public interface IKHRAudioSource
    {
        /// <summary>
        /// Gets the source name.
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Gets the volume gain for this source.
        /// </summary>
        float Gain { get; }

        /// <summary>
        /// Gets the playback rate (pitch/speed).
        /// </summary>
        float PlaybackRate { get; }

        /// <summary>
        /// Gets whether this source loops.
        /// </summary>
        bool Loop { get; }

        /// <summary>
        /// Gets whether this source auto-plays.
        /// </summary>
        bool Autoplay { get; }
    }
}
