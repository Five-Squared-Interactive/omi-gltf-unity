// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.Audio
{
    /// <summary>
    /// Unity component representing a KHR_audio_emitter.
    /// Wraps Unity's AudioSource with additional OMI-specific functionality.
    /// </summary>
    [AddComponentMenu("OMI/KHR Audio Emitter")]
    public class KHRAudioEmitterComponent : MonoBehaviour, IKHRAudioEmitter
    {
        [Header("Emitter Settings")]
        [Tooltip("Type of audio emitter.")]
        [SerializeField]
        private AudioEmitterType _emitterType = AudioEmitterType.Positional;

        [Tooltip("Volume gain multiplier.")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _gain = 1.0f;

        [Tooltip("Source indices in the root audio data.")]
        [SerializeField]
        private int[] _sourceIndices;

        [Header("References")]
        [Tooltip("The Unity AudioSource component.")]
        [SerializeField]
        private AudioSource _audioSource;

        [Header("Source Settings")]
        [Tooltip("Volume gain for the source.")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _sourceGain = 1.0f;

        [Tooltip("Playback rate/pitch multiplier.")]
        [SerializeField]
        [Range(0.1f, 3f)]
        private float _playbackRate = 1.0f;

        [Tooltip("Whether to loop the audio.")]
        [SerializeField]
        private bool _loop = false;

        [Tooltip("Whether to auto-play on start.")]
        [SerializeField]
        private bool _autoplay = false;

        /// <inheritdoc/>
        public AudioEmitterType EmitterType
        {
            get => _emitterType;
            set => _emitterType = value;
        }

        /// <inheritdoc/>
        public float Gain
        {
            get => _gain;
            set
            {
                _gain = value;
                UpdateAudioSourceVolume();
            }
        }

        /// <inheritdoc/>
        public int[] SourceIndices
        {
            get => _sourceIndices;
            set => _sourceIndices = value;
        }

        /// <summary>
        /// Gets or sets the Unity AudioSource.
        /// </summary>
        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                }
                return _audioSource;
            }
            set => _audioSource = value;
        }

        /// <summary>
        /// Gets or sets the source gain.
        /// </summary>
        public float SourceGain
        {
            get => _sourceGain;
            set
            {
                _sourceGain = value;
                UpdateAudioSourceVolume();
            }
        }

        /// <summary>
        /// Gets or sets the playback rate.
        /// </summary>
        public float PlaybackRate
        {
            get => _playbackRate;
            set
            {
                _playbackRate = value;
                if (AudioSource != null)
                {
                    AudioSource.pitch = _playbackRate;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the audio loops.
        /// </summary>
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                if (AudioSource != null)
                {
                    AudioSource.loop = _loop;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the audio auto-plays.
        /// </summary>
        public bool Autoplay
        {
            get => _autoplay;
            set => _autoplay = value;
        }

        /// <inheritdoc/>
        public bool IsPlaying => AudioSource != null && AudioSource.isPlaying;

        /// <inheritdoc/>
        public void Play()
        {
            if (AudioSource != null)
            {
                AudioSource.Play();
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (AudioSource != null)
            {
                AudioSource.Stop();
            }
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (AudioSource != null)
            {
                AudioSource.Pause();
            }
        }

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        private void Start()
        {
            ApplySettings();

            if (_autoplay && AudioSource != null)
            {
                AudioSource.Play();
            }
        }

        private void UpdateAudioSourceVolume()
        {
            if (AudioSource != null)
            {
                AudioSource.volume = _gain * _sourceGain;
            }
        }

        /// <summary>
        /// Applies all settings to the AudioSource.
        /// </summary>
        public void ApplySettings()
        {
            if (AudioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            AudioSource.volume = _gain * _sourceGain;
            AudioSource.pitch = _playbackRate;
            AudioSource.loop = _loop;
            AudioSource.playOnAwake = _autoplay;
            AudioSource.spatialBlend = (_emitterType == AudioEmitterType.Positional) ? 1.0f : 0.0f;
        }

        /// <summary>
        /// Sets the audio clip to play.
        /// </summary>
        public void SetClip(AudioClip clip)
        {
            if (AudioSource != null)
            {
                AudioSource.clip = clip;
            }
        }
    }

    /// <summary>
    /// Unity component representing a positional KHR_audio_emitter with cone and distance settings.
    /// </summary>
    [AddComponentMenu("OMI/KHR Positional Audio Emitter")]
    [RequireComponent(typeof(AudioSource))]
    public class KHRPositionalAudioEmitter : MonoBehaviour, IKHRAudioPositional
    {
        [Header("Emitter Settings")]
        [Tooltip("Volume gain multiplier.")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _gain = 1.0f;

        [Tooltip("Source indices in the root audio data.")]
        [SerializeField]
        private int[] _sourceIndices;

        [Header("Positional Settings")]
        [Tooltip("Shape type of the positional emitter.")]
        [SerializeField]
        private AudioShapeType _shapeType = AudioShapeType.Omnidirectional;

        [Tooltip("Distance model for volume attenuation.")]
        [SerializeField]
        private AudioDistanceModel _distanceModel = AudioDistanceModel.Inverse;

        [Tooltip("Reference distance (no attenuation below this).")]
        [SerializeField]
        [Min(0.001f)]
        private float _refDistance = 1.0f;

        [Tooltip("Maximum distance (0 = no maximum).")]
        [SerializeField]
        [Min(0f)]
        private float _maxDistance = 0.0f;

        [Tooltip("Rolloff factor for distance attenuation.")]
        [SerializeField]
        [Min(0f)]
        private float _rolloffFactor = 1.0f;

        [Header("Cone Settings (for Cone shape)")]
        [Tooltip("Inner cone angle in degrees (no attenuation inside).")]
        [SerializeField]
        [Range(0f, 360f)]
        private float _coneInnerAngleDegrees = 360f;

        [Tooltip("Outer cone angle in degrees (full attenuation outside).")]
        [SerializeField]
        [Range(0f, 360f)]
        private float _coneOuterAngleDegrees = 360f;

        [Tooltip("Volume gain outside the outer cone.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float _coneOuterGain = 0.0f;

        [Header("References")]
        [SerializeField]
        private AudioSource _audioSource;

        /// <inheritdoc/>
        public AudioEmitterType EmitterType => AudioEmitterType.Positional;

        /// <inheritdoc/>
        public float Gain
        {
            get => _gain;
            set => _gain = value;
        }

        /// <inheritdoc/>
        public int[] SourceIndices
        {
            get => _sourceIndices;
            set => _sourceIndices = value;
        }

        /// <inheritdoc/>
        public AudioShapeType ShapeType
        {
            get => _shapeType;
            set => _shapeType = value;
        }

        /// <inheritdoc/>
        public AudioDistanceModel DistanceModel
        {
            get => _distanceModel;
            set
            {
                _distanceModel = value;
                ApplyDistanceSettings();
            }
        }

        /// <inheritdoc/>
        public float ReferenceDistance
        {
            get => _refDistance;
            set
            {
                _refDistance = Mathf.Max(0.001f, value);
                ApplyDistanceSettings();
            }
        }

        /// <inheritdoc/>
        public float MaxDistance
        {
            get => _maxDistance;
            set
            {
                _maxDistance = Mathf.Max(0f, value);
                ApplyDistanceSettings();
            }
        }

        /// <inheritdoc/>
        public float RolloffFactor
        {
            get => _rolloffFactor;
            set
            {
                _rolloffFactor = Mathf.Max(0f, value);
                ApplyDistanceSettings();
            }
        }

        /// <inheritdoc/>
        public float ConeInnerAngle
        {
            get => _coneInnerAngleDegrees * Mathf.Deg2Rad;
            set
            {
                _coneInnerAngleDegrees = value * Mathf.Rad2Deg;
                ApplyConeSettings();
            }
        }

        /// <inheritdoc/>
        public float ConeOuterAngle
        {
            get => _coneOuterAngleDegrees * Mathf.Deg2Rad;
            set
            {
                _coneOuterAngleDegrees = value * Mathf.Rad2Deg;
                ApplyConeSettings();
            }
        }

        /// <inheritdoc/>
        public float ConeOuterGain
        {
            get => _coneOuterGain;
            set
            {
                _coneOuterGain = Mathf.Clamp01(value);
                ApplyConeSettings();
            }
        }

        /// <summary>
        /// Gets the AudioSource component.
        /// </summary>
        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                }
                return _audioSource;
            }
        }

        /// <inheritdoc/>
        public bool IsPlaying => AudioSource != null && AudioSource.isPlaying;

        /// <inheritdoc/>
        public void Play()
        {
            if (AudioSource != null)
            {
                AudioSource.Play();
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (AudioSource != null)
            {
                AudioSource.Stop();
            }
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (AudioSource != null)
            {
                AudioSource.Pause();
            }
        }

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        /// <summary>
        /// Applies all positional audio settings to the AudioSource.
        /// </summary>
        public void ApplyAllSettings()
        {
            if (AudioSource == null) return;

            AudioSource.spatialBlend = 1.0f; // Full 3D
            AudioSource.volume = _gain;
            
            ApplyDistanceSettings();
            ApplyConeSettings();
        }

        private void ApplyDistanceSettings()
        {
            if (AudioSource == null) return;

            AudioSource.minDistance = _refDistance;
            AudioSource.maxDistance = _maxDistance > 0 ? _maxDistance : 10000f; // Unity needs a max distance

            // Map OMI distance models to Unity rolloff modes
            switch (_distanceModel)
            {
                case AudioDistanceModel.Linear:
                    AudioSource.rolloffMode = AudioRolloffMode.Linear;
                    break;
                case AudioDistanceModel.Inverse:
                case AudioDistanceModel.Exponential:
                    // Unity's logarithmic is closer to inverse/exponential
                    AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                    break;
            }

            // Note: Unity doesn't have a direct rolloff factor setting.
            // For more accurate implementation, use a custom rolloff curve.
        }

        private void ApplyConeSettings()
        {
            if (AudioSource == null) return;

            if (_shapeType == AudioShapeType.Cone)
            {
                // Unity uses half-angle for spread, so divide by 2
                // Convert from full angle (diameter) to half angle
                AudioSource.spread = (_coneOuterAngleDegrees / 2f);
                
                // Note: Unity's spread is different from Web Audio's cone model.
                // For accurate cone simulation, you may need a custom solution.
            }
            else
            {
                // Omnidirectional - full spread
                AudioSource.spread = 0f;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_shapeType == AudioShapeType.Cone)
            {
                // Draw cone visualization
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                
                Vector3 forward = transform.forward;
                float distance = _maxDistance > 0 ? _maxDistance : 10f;
                
                // Draw inner cone
                float innerRadius = distance * Mathf.Tan(_coneInnerAngleDegrees * 0.5f * Mathf.Deg2Rad);
                DrawCone(transform.position, forward, distance, innerRadius, new Color(0f, 1f, 0f, 0.2f));
                
                // Draw outer cone
                float outerRadius = distance * Mathf.Tan(_coneOuterAngleDegrees * 0.5f * Mathf.Deg2Rad);
                DrawCone(transform.position, forward, distance, outerRadius, new Color(1f, 0f, 0f, 0.2f));
            }
            else
            {
                // Draw sphere for omnidirectional
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                float radius = _maxDistance > 0 ? _maxDistance : 10f;
                Gizmos.DrawWireSphere(transform.position, radius);
                
                Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, _refDistance);
            }
        }

        private void DrawCone(Vector3 origin, Vector3 direction, float length, float radius, Color color)
        {
            Gizmos.color = color;
            Vector3 endCenter = origin + direction * length;
            
            // Draw lines from origin to cone base
            Vector3 up = Vector3.Cross(direction, Vector3.right);
            if (up.sqrMagnitude < 0.001f)
                up = Vector3.Cross(direction, Vector3.up);
            up.Normalize();
            
            Vector3 right = Vector3.Cross(direction, up);
            
            int segments = 16;
            Vector3 prevPoint = endCenter + up * radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 point = endCenter + (up * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(origin, point);
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }
#endif
    }
}
