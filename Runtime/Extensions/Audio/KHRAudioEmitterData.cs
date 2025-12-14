// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;

namespace OMI.Extensions.Audio
{
    /// <summary>
    /// Distance model types for positional audio.
    /// </summary>
    public enum AudioDistanceModel
    {
        /// <summary>
        /// Linear distance model: 1.0 - rolloffFactor * (distance - refDistance) / (maxDistance - refDistance)
        /// </summary>
        Linear,
        
        /// <summary>
        /// Inverse distance model: refDistance / (refDistance + rolloffFactor * (max(distance, refDistance) - refDistance))
        /// </summary>
        Inverse,
        
        /// <summary>
        /// Exponential distance model: pow(max(distance, refDistance) / refDistance, -rolloffFactor)
        /// </summary>
        Exponential
    }

    /// <summary>
    /// Shape type for positional audio emitters.
    /// </summary>
    public enum AudioShapeType
    {
        /// <summary>
        /// Emits audio equally in all directions.
        /// </summary>
        Omnidirectional,
        
        /// <summary>
        /// Emits audio in a cone shape.
        /// </summary>
        Cone
    }

    /// <summary>
    /// Audio emitter type.
    /// </summary>
    public enum AudioEmitterType
    {
        /// <summary>
        /// Global audio - not affected by listener position.
        /// </summary>
        Global,
        
        /// <summary>
        /// Positional audio - affected by listener position and orientation.
        /// </summary>
        Positional
    }

    /// <summary>
    /// Raw audio data reference in KHR_audio_emitter.
    /// </summary>
    [Serializable]
    public class KHRAudioData
    {
        /// <summary>
        /// Optional name for this audio data.
        /// </summary>
        public string name;

        /// <summary>
        /// URI to the audio file. Relative paths are relative to the .gltf file.
        /// </summary>
        public string uri;

        /// <summary>
        /// Index of the buffer view containing the audio data.
        /// </summary>
        public int bufferView = -1;

        /// <summary>
        /// MIME type of the audio data. Required if bufferView is set.
        /// Supported: audio/mpeg (MP3), audio/ogg (Vorbis via extension), audio/opus (via extension)
        /// </summary>
        public string mimeType;

        /// <summary>
        /// Whether this audio data is embedded (bufferView) or external (uri).
        /// </summary>
        public bool IsEmbedded => bufferView >= 0;
    }

    /// <summary>
    /// Extension data for OMI_audio_ogg_vorbis on a source.
    /// </summary>
    [Serializable]
    public class OMIAudioOggVorbisSource
    {
        /// <summary>
        /// Index of the audio data in the audio array (Ogg Vorbis format).
        /// </summary>
        public int audio = -1;
    }

    /// <summary>
    /// Extension data for OMI_audio_opus on a source.
    /// </summary>
    [Serializable]
    public class OMIAudioOpusSource
    {
        /// <summary>
        /// Index of the audio data in the audio array (Opus format).
        /// </summary>
        public int audio = -1;
    }

    /// <summary>
    /// Audio source defining playback properties.
    /// </summary>
    [Serializable]
    public class KHRAudioSource
    {
        /// <summary>
        /// Optional name for this audio source.
        /// </summary>
        public string name;

        /// <summary>
        /// Linear volume multiplier. Default: 1.0
        /// </summary>
        public float gain = 1.0f;

        /// <summary>
        /// Playback speed/pitch multiplier. Default: 1.0
        /// </summary>
        public float playbackRate = 1.0f;

        /// <summary>
        /// Whether to loop the audio. Default: false
        /// </summary>
        public bool loop = false;

        /// <summary>
        /// Whether to play automatically when loaded. Default: false
        /// </summary>
        public bool autoplay = false;

        /// <summary>
        /// Index of the audio data in the audio array.
        /// </summary>
        public int audio = -1;

        /// <summary>
        /// Extension for Ogg Vorbis audio (OMI_audio_ogg_vorbis).
        /// </summary>
        public OMIAudioOggVorbisSource oggVorbisExtension;

        /// <summary>
        /// Extension for Opus audio (OMI_audio_opus).
        /// </summary>
        public OMIAudioOpusSource opusExtension;

        /// <summary>
        /// Gets the preferred audio index, checking extensions first.
        /// Returns Opus > Vorbis > MP3 fallback order.
        /// </summary>
        public int GetPreferredAudioIndex(bool supportsOpus = true, bool supportsVorbis = true)
        {
            // Prefer Opus if supported and available
            if (supportsOpus && opusExtension != null && opusExtension.audio >= 0)
                return opusExtension.audio;

            // Then prefer Ogg Vorbis if supported and available
            if (supportsVorbis && oggVorbisExtension != null && oggVorbisExtension.audio >= 0)
                return oggVorbisExtension.audio;

            // Fall back to base audio (MP3)
            return audio;
        }
    }

    /// <summary>
    /// Positional audio properties for emitters.
    /// </summary>
    [Serializable]
    public class KHRAudioPositional
    {
        /// <summary>
        /// Shape type of the audio emitter. Default: Omnidirectional
        /// </summary>
        public string shapeType = "omnidirectional";

        /// <summary>
        /// Inner cone angle in radians (no attenuation inside). Default: 2π (360°)
        /// </summary>
        public float coneInnerAngle = 6.283185307f;

        /// <summary>
        /// Outer cone angle in radians (full attenuation outside). Default: 2π (360°)
        /// </summary>
        public float coneOuterAngle = 6.283185307f;

        /// <summary>
        /// Volume gain outside the outer cone. Default: 0.0
        /// </summary>
        public float coneOuterGain = 0.0f;

        /// <summary>
        /// Distance model for volume attenuation. Default: inverse
        /// </summary>
        public string distanceModel = "inverse";

        /// <summary>
        /// Maximum distance for audio. 0 means no maximum. Default: 0.0
        /// </summary>
        public float maxDistance = 0.0f;

        /// <summary>
        /// Reference distance (no attenuation below this). Default: 1.0
        /// </summary>
        public float refDistance = 1.0f;

        /// <summary>
        /// Rolloff factor for distance attenuation. Default: 1.0
        /// </summary>
        public float rolloffFactor = 1.0f;

        /// <summary>
        /// Gets the shape type as an enum.
        /// </summary>
        public AudioShapeType ShapeTypeEnum
        {
            get
            {
                return shapeType?.ToLowerInvariant() switch
                {
                    "cone" => AudioShapeType.Cone,
                    _ => AudioShapeType.Omnidirectional
                };
            }
        }

        /// <summary>
        /// Gets the distance model as an enum.
        /// </summary>
        public AudioDistanceModel DistanceModelEnum
        {
            get
            {
                return distanceModel?.ToLowerInvariant() switch
                {
                    "linear" => AudioDistanceModel.Linear,
                    "exponential" => AudioDistanceModel.Exponential,
                    _ => AudioDistanceModel.Inverse
                };
            }
        }
    }

    /// <summary>
    /// Audio emitter definition.
    /// </summary>
    [Serializable]
    public class KHRAudioEmitter
    {
        /// <summary>
        /// Optional name for this emitter.
        /// </summary>
        public string name;

        /// <summary>
        /// Emitter type: "global" or "positional".
        /// </summary>
        public string type;

        /// <summary>
        /// Linear volume multiplier. Default: 1.0
        /// </summary>
        public float gain = 1.0f;

        /// <summary>
        /// Indices of audio sources used by this emitter.
        /// </summary>
        public int[] sources;

        /// <summary>
        /// Positional audio properties. Only valid if type is "positional".
        /// </summary>
        public KHRAudioPositional positional;

        /// <summary>
        /// Gets the emitter type as an enum.
        /// </summary>
        public AudioEmitterType TypeEnum
        {
            get
            {
                return type?.ToLowerInvariant() switch
                {
                    "positional" => AudioEmitterType.Positional,
                    _ => AudioEmitterType.Global
                };
            }
        }
    }

    /// <summary>
    /// Document-level root data for KHR_audio_emitter extension.
    /// </summary>
    [Serializable]
    public class KHRAudioEmitterRoot
    {
        /// <summary>
        /// Array of audio data objects.
        /// </summary>
        public KHRAudioData[] audio;

        /// <summary>
        /// Array of audio source objects.
        /// </summary>
        public KHRAudioSource[] sources;

        /// <summary>
        /// Array of audio emitter objects.
        /// </summary>
        public KHRAudioEmitter[] emitters;
    }

    /// <summary>
    /// Node-level extension data for KHR_audio_emitter (single emitter).
    /// </summary>
    [Serializable]
    public class KHRAudioEmitterNode
    {
        /// <summary>
        /// Index of the emitter in the root emitters array.
        /// </summary>
        public int emitter = -1;
    }

    /// <summary>
    /// Scene-level extension data for KHR_audio_emitter (multiple global emitters).
    /// </summary>
    [Serializable]
    public class KHRAudioEmitterScene
    {
        /// <summary>
        /// Indices of global emitters attached to this scene.
        /// </summary>
        public int[] emitters;
    }
}
