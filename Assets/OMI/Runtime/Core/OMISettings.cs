// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Base settings class for OMI operations.
    /// </summary>
    [Serializable]
    public abstract class OMISettingsBase
    {
        /// <summary>
        /// Whether to enable verbose logging.
        /// </summary>
        [Tooltip("Enable verbose logging for debugging.")]
        public bool VerboseLogging = false;

        /// <summary>
        /// Whether to validate data against JSON schemas.
        /// </summary>
        [Tooltip("Validate extension data against JSON schemas.")]
        public bool ValidateSchemas = true;
    }

    /// <summary>
    /// Settings for OMI import operations.
    /// </summary>
    [Serializable]
    public class OMIImportSettings : OMISettingsBase
    {
        /// <summary>
        /// Whether to import physics shapes.
        /// </summary>
        [Tooltip("Import OMI_physics_shape extension data.")]
        public bool ImportPhysicsShapes = true;

        /// <summary>
        /// Whether to import physics bodies.
        /// </summary>
        [Tooltip("Import OMI_physics_body extension data.")]
        public bool ImportPhysicsBodies = true;

        /// <summary>
        /// Whether to import physics joints.
        /// </summary>
        [Tooltip("Import OMI_physics_joint extension data.")]
        public bool ImportPhysicsJoints = true;

        /// <summary>
        /// Whether to import spawn points.
        /// </summary>
        [Tooltip("Import OMI_spawn_point extension data.")]
        public bool ImportSpawnPoints = true;

        /// <summary>
        /// Whether to import seats.
        /// </summary>
        [Tooltip("Import OMI_seat extension data.")]
        public bool ImportSeats = true;

        /// <summary>
        /// Whether to import audio emitters.
        /// </summary>
        [Tooltip("Import KHR_audio_emitter extension data.")]
        public bool ImportAudioEmitters = true;

        /// <summary>
        /// Whether to import links.
        /// </summary>
        [Tooltip("Import OMI_link extension data.")]
        public bool ImportLinks = true;

        /// <summary>
        /// The default physics layer for colliders.
        /// </summary>
        [Tooltip("Default Unity layer for imported physics colliders.")]
        public int DefaultPhysicsLayer = 0;

        /// <summary>
        /// Whether to generate convex mesh colliders for convex shapes.
        /// </summary>
        [Tooltip("Generate convex MeshColliders for OMI convex shapes.")]
        public bool GenerateConvexColliders = true;
    }

    /// <summary>
    /// Settings for OMI export operations.
    /// </summary>
    [Serializable]
    public class OMIExportSettings : OMISettingsBase
    {
        /// <summary>
        /// Whether to export physics shapes.
        /// </summary>
        [Tooltip("Export OMI_physics_shape extension data.")]
        public bool ExportPhysicsShapes = true;

        /// <summary>
        /// Whether to export physics bodies.
        /// </summary>
        [Tooltip("Export OMI_physics_body extension data.")]
        public bool ExportPhysicsBodies = true;

        /// <summary>
        /// Whether to export physics joints.
        /// </summary>
        [Tooltip("Export OMI_physics_joint extension data.")]
        public bool ExportPhysicsJoints = true;

        /// <summary>
        /// Whether to export spawn points.
        /// </summary>
        [Tooltip("Export OMI_spawn_point extension data.")]
        public bool ExportSpawnPoints = true;

        /// <summary>
        /// Whether to export seats.
        /// </summary>
        [Tooltip("Export OMI_seat extension data.")]
        public bool ExportSeats = true;

        /// <summary>
        /// Whether to export audio sources as emitters.
        /// </summary>
        [Tooltip("Export Unity AudioSources as KHR_audio_emitter.")]
        public bool ExportAudioEmitters = true;

        /// <summary>
        /// Whether to export links.
        /// </summary>
        [Tooltip("Export OMI_link extension data.")]
        public bool ExportLinks = true;

        /// <summary>
        /// Whether to include extensions in extensionsRequired.
        /// </summary>
        [Tooltip("Mark OMI extensions as required in the glTF.")]
        public bool MarkExtensionsRequired = false;
    }

    /// <summary>
    /// Global settings asset for OMI extensions.
    /// Create via OMI > Create > Settings Asset menu.
    /// </summary>
    [CreateAssetMenu(fileName = "OMISettings", menuName = "OMI/Settings")]
    public class OMISettings : ScriptableObject
    {
        [Header("General")]
        [Tooltip("Use the built-in Unity handlers for OMI extensions.")]
        [SerializeField]
        internal bool useDefaultHandlers = true;

        [Tooltip("Automatically register handlers when loading glTF files.")]
        [SerializeField]
        internal bool autoRegisterHandlers = true;

        [Tooltip("Enable verbose logging for debugging.")]
        [SerializeField]
        internal bool debugMode = false;

        [Header("Enabled Extensions")]
        [Tooltip("List of OMI extensions to process during import/export.")]
        [SerializeField]
        internal string[] enabledExtensions = new[]
        {
            "OMI_physics_shape",
            "OMI_physics_body",
            "OMI_physics_joint",
            "OMI_spawn_point",
            "OMI_seat",
            "OMI_link"
        };

        [Header("Import Settings")]
        [SerializeField]
        internal OMIImportSettings importSettings = new OMIImportSettings();

        [Header("Export Settings")]
        [SerializeField]
        internal OMIExportSettings exportSettings = new OMIExportSettings();

        /// <summary>
        /// Whether to use default Unity handlers.
        /// </summary>
        public bool UseDefaultHandlers => useDefaultHandlers;

        /// <summary>
        /// Whether to auto-register handlers.
        /// </summary>
        public bool AutoRegisterHandlers => autoRegisterHandlers;

        /// <summary>
        /// Whether debug mode is enabled.
        /// </summary>
        public bool DebugMode => debugMode;

        /// <summary>
        /// Gets the list of enabled extensions.
        /// </summary>
        public string[] EnabledExtensions => enabledExtensions;

        /// <summary>
        /// Gets the import settings.
        /// </summary>
        public OMIImportSettings ImportSettings => importSettings;

        /// <summary>
        /// Gets the export settings.
        /// </summary>
        public OMIExportSettings ExportSettings => exportSettings;

        /// <summary>
        /// Checks if an extension is enabled.
        /// </summary>
        public bool IsExtensionEnabled(string extensionName)
        {
            if (enabledExtensions == null) return false;
            return System.Array.IndexOf(enabledExtensions, extensionName) >= 0;
        }

        /// <summary>
        /// Creates a default settings instance (not saved to disk).
        /// </summary>
        public static OMISettings CreateDefault()
        {
            var settings = CreateInstance<OMISettings>();
            return settings;
        }

        private static OMISettings _cachedInstance;

        /// <summary>
        /// Gets the global settings instance.
        /// </summary>
        public static OMISettings Instance
        {
            get
            {
                if (_cachedInstance == null)
                {
                    // Try to load from Resources
                    _cachedInstance = Resources.Load<OMISettings>("OMISettings");
                    
                    if (_cachedInstance == null)
                    {
                        // Create default if not found
                        _cachedInstance = CreateDefault();
                    }
                }
                return _cachedInstance;
            }
        }
    }
}
