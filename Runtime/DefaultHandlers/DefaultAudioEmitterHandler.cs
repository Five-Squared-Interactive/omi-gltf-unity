// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OMI.Extensions.Audio
{
    /// <summary>
    /// Default handler for KHR_audio_emitter extension.
    /// Creates Unity AudioSource components from glTF audio emitter data.
    /// </summary>
    public class DefaultAudioEmitterHandler : IOMIDocumentExtensionHandler<KHRAudioEmitterRoot>, 
                                              IOMINodeExtensionHandler<KHRAudioEmitterNode>
    {
        /// <summary>
        /// Extension name for KHR_audio_emitter.
        /// </summary>
        public const string ExtensionNameConst = "KHR_audio_emitter";

        /// <inheritdoc/>
        public string ExtensionName => ExtensionNameConst;

        /// <inheritdoc/>
        public int Priority => 50; // Process after physics but before high-level extensions

        // Cached data from document-level processing
        private KHRAudioEmitterRoot _rootData;
        private List<AudioClip> _loadedClips = new List<AudioClip>();
        private string _basePath;

        /// <summary>
        /// Whether Ogg Vorbis format is supported.
        /// </summary>
        public bool SupportsOggVorbis { get; set; } = true;

        /// <summary>
        /// Whether Opus format is supported.
        /// </summary>
        public bool SupportsOpus { get; set; } = false; // Unity doesn't natively support Opus

        // IOMIExtensionHandler<KHRAudioEmitterRoot> implementation (via IOMIDocumentExtensionHandler)
        Task IOMIExtensionHandler<KHRAudioEmitterRoot>.OnImportAsync(KHRAudioEmitterRoot data, OMIImportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        Task<KHRAudioEmitterRoot> IOMIExtensionHandler<KHRAudioEmitterRoot>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }

        // IOMIExtensionHandler<KHRAudioEmitterNode> implementation (via IOMINodeExtensionHandler)
        Task IOMIExtensionHandler<KHRAudioEmitterNode>.OnImportAsync(KHRAudioEmitterNode data, OMIImportContext context, CancellationToken cancellationToken)
        {
            // Node-level import happens via OnNodeImportAsync
            return Task.CompletedTask;
        }

        Task<KHRAudioEmitterNode> IOMIExtensionHandler<KHRAudioEmitterNode>.OnExportAsync(OMIExportContext context, CancellationToken cancellationToken)
        {
            // Node-level export happens via OnNodeExportAsync
            return Task.FromResult<KHRAudioEmitterNode>(null);
        }

        /// <inheritdoc/>
        public async Task OnDocumentImportAsync(KHRAudioEmitterRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                Debug.LogWarning("[OMI] KHR_audio_emitter: No data provided");
                return;
            }

            _rootData = data;
            _basePath = context.BasePath;
            _loadedClips.Clear();

            // Pre-load all audio clips
            if (data.audio != null)
            {
                foreach (var audioData in data.audio)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var clip = await LoadAudioClipAsync(audioData, context);
                    _loadedClips.Add(clip); // May be null if loading failed
                }
            }

            // Store the root data for node processing
            context.SetExtensionData(ExtensionName, data);

            Debug.Log($"[OMI] KHR_audio_emitter: Loaded {_loadedClips.Count} audio clips, " +
                     $"{data.sources?.Length ?? 0} sources, {data.emitters?.Length ?? 0} emitters");
        }

        /// <inheritdoc/>
        public Task<KHRAudioEmitterRoot> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportDocumentExtension(context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task OnNodeImportAsync(KHRAudioEmitterNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null || _rootData == null)
            {
                return Task.CompletedTask;
            }

            if (data.emitter < 0 || data.emitter >= (_rootData.emitters?.Length ?? 0))
            {
                Debug.LogWarning($"[OMI] KHR_audio_emitter: Invalid emitter index {data.emitter}");
                return Task.CompletedTask;
            }

            var emitterData = _rootData.emitters[data.emitter];
            CreateEmitterComponent(targetObject, emitterData);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<KHRAudioEmitterNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var result = ExportNodeExtension(sourceObject, context);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public async Task<bool> ProcessDocumentExtensionAsync(KHRAudioEmitterRoot data, OMIImportContext context)
        {
            await OnDocumentImportAsync(data, context);
            return true;
        }

        /// <inheritdoc/>
        public Task<bool> ProcessNodeExtensionAsync(KHRAudioEmitterNode data, GameObject target, OMIImportContext context)
        {
            if (data == null || target == null || _rootData == null)
            {
                return Task.FromResult(false);
            }

            if (data.emitter < 0 || data.emitter >= (_rootData.emitters?.Length ?? 0))
            {
                Debug.LogWarning($"[OMI] KHR_audio_emitter: Invalid emitter index {data.emitter}");
                return Task.FromResult(false);
            }

            var emitterData = _rootData.emitters[data.emitter];
            CreateEmitterComponent(target, emitterData);

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public KHRAudioEmitterRoot ExportDocumentExtension(OMIExportContext context)
        {
            var emitters = new List<KHRAudioEmitter>();
            var sources = new List<KHRAudioSource>();
            var audioData = new List<KHRAudioData>();

            // Find all audio emitter components in the scene
            var components = context.RootObjects != null
                ? new List<KHRAudioEmitterComponent>()
                : new List<KHRAudioEmitterComponent>(UnityEngine.Object.FindObjectsOfType<KHRAudioEmitterComponent>());

            if (context.RootObjects != null)
            {
                foreach (var root in context.RootObjects)
                {
                    components.AddRange(root.GetComponentsInChildren<KHRAudioEmitterComponent>(true));
                }
            }

            // Build maps for deduplication
            var clipToAudioIndex = new Dictionary<AudioClip, int>();
            
            foreach (var component in components)
            {
                var audioSource = component.AudioSource;
                if (audioSource == null || audioSource.clip == null) continue;

                // Get or create audio data entry
                int audioIndex;
                if (!clipToAudioIndex.TryGetValue(audioSource.clip, out audioIndex))
                {
                    audioIndex = audioData.Count;
                    clipToAudioIndex[audioSource.clip] = audioIndex;
                    
                    // Note: Actual audio export would require saving the clip data
                    audioData.Add(new KHRAudioData
                    {
                        name = audioSource.clip.name,
                        // URI would need to be set based on export settings
                    });
                }

                // Create source
                int sourceIndex = sources.Count;
                sources.Add(new KHRAudioSource
                {
                    name = component.name,
                    gain = component.SourceGain,
                    playbackRate = component.PlaybackRate,
                    loop = component.Loop,
                    autoplay = component.Autoplay,
                    audio = audioIndex
                });

                // Create emitter
                var emitter = new KHRAudioEmitter
                {
                    name = component.name,
                    type = component.EmitterType == AudioEmitterType.Global ? "global" : "positional",
                    gain = component.Gain,
                    sources = new[] { sourceIndex }
                };

                // Add positional data if applicable - check for separate positional component
                if (component.EmitterType == AudioEmitterType.Positional)
                {
                    var positional = component.GetComponent<KHRPositionalAudioEmitter>();
                    if (positional != null)
                    {
                        emitter.positional = new KHRAudioPositional
                        {
                            shapeType = positional.ShapeType == AudioShapeType.Cone ? "cone" : "omnidirectional",
                            distanceModel = GetDistanceModelString(positional.DistanceModel),
                            refDistance = positional.ReferenceDistance,
                            maxDistance = positional.MaxDistance,
                            rolloffFactor = positional.RolloffFactor,
                            coneInnerAngle = positional.ConeInnerAngle,
                            coneOuterAngle = positional.ConeOuterAngle,
                            coneOuterGain = positional.ConeOuterGain
                        };
                    }
                }

                // Register the emitter index for node-level export
                int emitterIndex = emitters.Count;
                context.RegisterEmitterIndex(component.gameObject, emitterIndex);

                emitters.Add(emitter);
            }

            if (emitters.Count == 0)
            {
                return null;
            }

            return new KHRAudioEmitterRoot
            {
                audio = audioData.ToArray(),
                sources = sources.ToArray(),
                emitters = emitters.ToArray()
            };
        }

        /// <inheritdoc/>
        public KHRAudioEmitterNode ExportNodeExtension(GameObject source, OMIExportContext context)
        {
            var component = source.GetComponent<KHRAudioEmitterComponent>();
            if (component == null)
            {
                return null;
            }

            // The emitter index would need to be determined from the context
            // This requires coordination with document-level export
            int emitterIndex = context.GetEmitterIndex(source);
            if (emitterIndex < 0)
            {
                return null;
            }

            return new KHRAudioEmitterNode
            {
                emitter = emitterIndex
            };
        }

        private void CreateEmitterComponent(GameObject target, KHRAudioEmitter emitterData)
        {
            AudioSource audioSource = target.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = target.AddComponent<AudioSource>();
            }

            bool isPositional = emitterData.TypeEnum == AudioEmitterType.Positional;

            if (isPositional && emitterData.positional != null)
            {
                // Create positional audio component
                var positionalComponent = target.AddComponent<KHRPositionalAudioEmitter>();
                positionalComponent.Gain = emitterData.gain;
                positionalComponent.SourceIndices = emitterData.sources;
                positionalComponent.ShapeType = emitterData.positional.ShapeTypeEnum;
                positionalComponent.DistanceModel = emitterData.positional.DistanceModelEnum;
                positionalComponent.ReferenceDistance = emitterData.positional.refDistance;
                positionalComponent.MaxDistance = emitterData.positional.maxDistance;
                positionalComponent.RolloffFactor = emitterData.positional.rolloffFactor;
                positionalComponent.ConeInnerAngle = emitterData.positional.coneInnerAngle;
                positionalComponent.ConeOuterAngle = emitterData.positional.coneOuterAngle;
                positionalComponent.ConeOuterGain = emitterData.positional.coneOuterGain;
                positionalComponent.ApplyAllSettings();

                // Set up audio clip from first source
                SetupAudioSource(audioSource, emitterData);
            }
            else
            {
                // Create basic emitter component
                var emitterComponent = target.AddComponent<KHRAudioEmitterComponent>();
                emitterComponent.EmitterType = emitterData.TypeEnum;
                emitterComponent.Gain = emitterData.gain;
                emitterComponent.SourceIndices = emitterData.sources;
                emitterComponent.AudioSource = audioSource;

                // Set up audio clip from first source
                SetupAudioSource(audioSource, emitterData);
                
                // Configure for global audio
                if (!isPositional)
                {
                    audioSource.spatialBlend = 0f; // 2D audio
                }

                emitterComponent.ApplySettings();
            }
        }

        private void SetupAudioSource(AudioSource audioSource, KHRAudioEmitter emitterData)
        {
            if (emitterData.sources == null || emitterData.sources.Length == 0)
            {
                return;
            }

            // Use the first source for now
            int sourceIndex = emitterData.sources[0];
            if (_rootData.sources == null || sourceIndex < 0 || sourceIndex >= _rootData.sources.Length)
            {
                return;
            }

            var sourceData = _rootData.sources[sourceIndex];
            
            // Get the preferred audio clip index
            int audioIndex = sourceData.GetPreferredAudioIndex(SupportsOpus, SupportsOggVorbis);
            
            if (audioIndex >= 0 && audioIndex < _loadedClips.Count && _loadedClips[audioIndex] != null)
            {
                audioSource.clip = _loadedClips[audioIndex];
            }

            audioSource.volume = emitterData.gain * sourceData.gain;
            audioSource.pitch = sourceData.playbackRate;
            audioSource.loop = sourceData.loop;
            audioSource.playOnAwake = sourceData.autoplay;

            if (sourceData.autoplay && Application.isPlaying)
            {
                audioSource.Play();
            }
        }

        private async Task<AudioClip> LoadAudioClipAsync(KHRAudioData audioData, OMIImportContext context)
        {
            if (audioData == null)
            {
                return null;
            }

            try
            {
                if (audioData.IsEmbedded)
                {
                    // Load from buffer view (embedded audio)
                    return await LoadEmbeddedAudioAsync(audioData, context);
                }
                else if (!string.IsNullOrEmpty(audioData.uri))
                {
                    // Load from URI
                    return await LoadAudioFromUriAsync(audioData.uri, context);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OMI] KHR_audio_emitter: Failed to load audio '{audioData.name}': {ex.Message}");
            }

            return null;
        }

        private async Task<AudioClip> LoadAudioFromUriAsync(string uri, OMIImportContext context)
        {
            string fullPath;
            
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                fullPath = uri;
            }
            else
            {
                // Relative path
                fullPath = System.IO.Path.Combine(_basePath ?? "", uri);
                if (!fullPath.StartsWith("file://"))
                {
                    fullPath = "file://" + fullPath;
                }
            }

            AudioType audioType = GetAudioType(uri);

            using (var request = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType))
            {
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerAudioClip.GetContent(request);
                }
                else
                {
                    Debug.LogWarning($"[OMI] KHR_audio_emitter: Failed to load audio from {uri}: {request.error}");
                    return null;
                }
            }
        }

        private Task<AudioClip> LoadEmbeddedAudioAsync(KHRAudioData audioData, OMIImportContext context)
        {
            // Embedded audio loading requires access to the glTF buffer data
            // This would be implemented based on how glTFast exposes buffer views
            Debug.LogWarning("[OMI] KHR_audio_emitter: Embedded audio not yet implemented");
            return Task.FromResult<AudioClip>(null);
        }

        private AudioType GetAudioType(string path)
        {
            string extension = System.IO.Path.GetExtension(path)?.ToLowerInvariant();
            
            return extension switch
            {
                ".mp3" => AudioType.MPEG,
                ".ogg" => AudioType.OGGVORBIS,
                ".wav" => AudioType.WAV,
                ".aiff" or ".aif" => AudioType.AIFF,
                // Note: Opus and WebM would need special handling
                ".opus" => AudioType.OGGVORBIS, // Try Vorbis decoder, may not work
                ".webm" => AudioType.UNKNOWN,
                _ => AudioType.UNKNOWN
            };
        }

        private string GetDistanceModelString(AudioDistanceModel model)
        {
            return model switch
            {
                AudioDistanceModel.Linear => "linear",
                AudioDistanceModel.Exponential => "exponential",
                _ => "inverse"
            };
        }
    }
}
