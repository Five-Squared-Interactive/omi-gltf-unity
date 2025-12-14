// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Helper methods for extracting OMI extension data from glTF JSON.
    /// Uses direct Newtonsoft.Json parsing - works with any version of glTFast.
    /// </summary>
    public static class OMIJsonExtensions
    {
        /// <summary>
        /// Extension name constants for all supported OMI extensions.
        /// </summary>
        public static class ExtensionNames
        {
            public const string PhysicsShape = "OMI_physics_shape";
            public const string PhysicsBody = "OMI_physics_body";
            public const string PhysicsJoint = "OMI_physics_joint";
            public const string PhysicsGravity = "OMI_physics_gravity";
            public const string SpawnPoint = "OMI_spawn_point";
            public const string Seat = "OMI_seat";
            public const string Link = "OMI_link";
            public const string Personality = "OMI_personality";
            public const string AudioEmitter = "KHR_audio_emitter";
            public const string AudioOggVorbis = "OMI_audio_ogg_vorbis";
            public const string AudioOpus = "OMI_audio_opus";
            public const string VehicleBody = "OMI_vehicle_body";
            public const string VehicleWheel = "OMI_vehicle_wheel";
            public const string VehicleThruster = "OMI_vehicle_thruster";
            public const string VehicleHoverThruster = "OMI_vehicle_hover_thruster";
            public const string EnvironmentSky = "OMI_environment_sky";
        }

        /// <summary>
        /// Parses a glTF file and returns the root JSON object.
        /// Supports both .gltf (JSON) and .glb (binary) files.
        /// </summary>
        /// <param name="path">Path to the glTF file.</param>
        /// <returns>The parsed JObject, or null on failure.</returns>
        public static JObject ParseGltfFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError($"[OMI] File not found: {path}");
                return null;
            }

            try
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                
                if (extension == ".glb")
                {
                    return ParseGlbFile(path);
                }
                else
                {
                    string json = File.ReadAllText(path);
                    return JObject.Parse(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OMI] Failed to parse glTF file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses a glTF JSON string and returns the root JSON object.
        /// </summary>
        /// <param name="json">The glTF JSON string.</param>
        /// <returns>The parsed JObject, or null on failure.</returns>
        public static JObject ParseGltfJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OMI] Failed to parse glTF JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses glTF data from a byte array.
        /// Supports both .gltf (JSON) and .glb (binary) formats.
        /// </summary>
        /// <param name="data">The glTF data.</param>
        /// <returns>The parsed JObject, or null on failure.</returns>
        public static JObject ParseGltfData(byte[] data)
        {
            if (data == null || data.Length < 4)
            {
                return null;
            }

            try
            {
                // Check for GLB magic number (0x46546C67 = "glTF")
                if (data.Length >= 12 && 
                    data[0] == 0x67 && data[1] == 0x6C && data[2] == 0x54 && data[3] == 0x46)
                {
                    return ParseGlbData(data);
                }
                else
                {
                    // Assume JSON
                    string json = Encoding.UTF8.GetString(data);
                    return JObject.Parse(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OMI] Failed to parse glTF data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses a GLB file and extracts the JSON chunk.
        /// </summary>
        private static JObject ParseGlbFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return ParseGlbData(data);
        }

        /// <summary>
        /// Parses GLB binary data and extracts the JSON chunk.
        /// </summary>
        private static JObject ParseGlbData(byte[] data)
        {
            if (data.Length < 12)
            {
                Debug.LogError("[OMI] GLB file too small");
                return null;
            }

            // GLB Header (12 bytes):
            // - magic (4 bytes): 0x46546C67 ("glTF")
            // - version (4 bytes): 2
            // - length (4 bytes): total file length

            uint magic = BitConverter.ToUInt32(data, 0);
            if (magic != 0x46546C67)
            {
                Debug.LogError("[OMI] Invalid GLB magic number");
                return null;
            }

            uint version = BitConverter.ToUInt32(data, 4);
            if (version != 2)
            {
                Debug.LogError($"[OMI] Unsupported GLB version: {version}");
                return null;
            }

            // First chunk should be JSON (type 0x4E4F534A = "JSON")
            if (data.Length < 20)
            {
                Debug.LogError("[OMI] GLB file missing JSON chunk");
                return null;
            }

            uint chunkLength = BitConverter.ToUInt32(data, 12);
            uint chunkType = BitConverter.ToUInt32(data, 16);

            if (chunkType != 0x4E4F534A) // "JSON"
            {
                Debug.LogError("[OMI] GLB first chunk is not JSON");
                return null;
            }

            if (data.Length < 20 + chunkLength)
            {
                Debug.LogError("[OMI] GLB JSON chunk truncated");
                return null;
            }

            string json = Encoding.UTF8.GetString(data, 20, (int)chunkLength);
            return JObject.Parse(json);
        }

        /// <summary>
        /// Gets document-level extensions from the glTF root.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <returns>The extensions JObject, or null if not present.</returns>
        public static JObject GetDocumentExtensions(JObject root)
        {
            return root?["extensions"] as JObject;
        }

        /// <summary>
        /// Gets all nodes from the glTF root.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <returns>The nodes JArray, or null if not present.</returns>
        public static JArray GetNodes(JObject root)
        {
            return root?["nodes"] as JArray;
        }

        /// <summary>
        /// Gets a specific node by index.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <param name="nodeIndex">The node index.</param>
        /// <returns>The node JObject, or null if not found.</returns>
        public static JObject GetNode(JObject root, int nodeIndex)
        {
            var nodes = GetNodes(root);
            if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count)
            {
                return null;
            }
            return nodes[nodeIndex] as JObject;
        }

        /// <summary>
        /// Gets extensions from a node.
        /// </summary>
        /// <param name="node">The node JObject.</param>
        /// <returns>The extensions JObject, or null if not present.</returns>
        public static JObject GetNodeExtensions(JObject node)
        {
            return node?["extensions"] as JObject;
        }

        /// <summary>
        /// Tries to get a document-level extension.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="root">The glTF root JObject.</param>
        /// <param name="extensionName">The extension name.</param>
        /// <param name="result">The deserialized extension data.</param>
        /// <returns>True if found and deserialized successfully.</returns>
        public static bool TryGetDocumentExtension<T>(JObject root, string extensionName, out T result) where T : class
        {
            result = null;
            
            var extensions = GetDocumentExtensions(root);
            if (extensions == null)
            {
                return false;
            }

            var extensionToken = extensions[extensionName];
            if (extensionToken == null)
            {
                return false;
            }

            try
            {
                result = extensionToken.ToObject<T>();
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OMI] Failed to deserialize extension {extensionName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tries to get a node-level extension.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="node">The node JObject.</param>
        /// <param name="extensionName">The extension name.</param>
        /// <param name="result">The deserialized extension data.</param>
        /// <returns>True if found and deserialized successfully.</returns>
        public static bool TryGetNodeExtension<T>(JObject node, string extensionName, out T result) where T : class
        {
            result = null;
            
            var extensions = GetNodeExtensions(node);
            if (extensions == null)
            {
                return false;
            }

            var extensionToken = extensions[extensionName];
            if (extensionToken == null)
            {
                return false;
            }

            try
            {
                result = extensionToken.ToObject<T>();
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OMI] Failed to deserialize node extension {extensionName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets all extension names present on the document.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <returns>List of extension names.</returns>
        public static IEnumerable<string> GetDocumentExtensionNames(JObject root)
        {
            var names = new List<string>();
            var extensions = GetDocumentExtensions(root);
            
            if (extensions != null)
            {
                foreach (var property in extensions.Properties())
                {
                    names.Add(property.Name);
                }
            }

            return names;
        }

        /// <summary>
        /// Gets all extension names present on a node.
        /// </summary>
        /// <param name="node">The node JObject.</param>
        /// <returns>List of extension names.</returns>
        public static IEnumerable<string> GetNodeExtensionNames(JObject node)
        {
            var names = new List<string>();
            var extensions = GetNodeExtensions(node);
            
            if (extensions != null)
            {
                foreach (var property in extensions.Properties())
                {
                    names.Add(property.Name);
                }
            }

            return names;
        }

        /// <summary>
        /// Checks if a document-level extension is present.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <param name="extensionName">The extension name.</param>
        /// <returns>True if the extension is present.</returns>
        public static bool HasDocumentExtension(JObject root, string extensionName)
        {
            var extensions = GetDocumentExtensions(root);
            return extensions != null && extensions[extensionName] != null;
        }

        /// <summary>
        /// Checks if a node-level extension is present.
        /// </summary>
        /// <param name="node">The node JObject.</param>
        /// <param name="extensionName">The extension name.</param>
        /// <returns>True if the extension is present.</returns>
        public static bool HasNodeExtension(JObject node, string extensionName)
        {
            var extensions = GetNodeExtensions(node);
            return extensions != null && extensions[extensionName] != null;
        }

        /// <summary>
        /// Gets the name of a node.
        /// </summary>
        /// <param name="node">The node JObject.</param>
        /// <returns>The node name, or null if not set.</returns>
        public static string GetNodeName(JObject node)
        {
            return node?["name"]?.Value<string>();
        }

        /// <summary>
        /// Gets the child indices of a node.
        /// </summary>
        /// <param name="node">The node JObject.</param>
        /// <returns>Array of child node indices, or empty array if none.</returns>
        public static int[] GetNodeChildren(JObject node)
        {
            var children = node?["children"] as JArray;
            if (children == null || children.Count == 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[children.Count];
            for (int i = 0; i < children.Count; i++)
            {
                result[i] = children[i].Value<int>();
            }
            return result;
        }

        /// <summary>
        /// Gets scenes from the glTF root.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <returns>The scenes JArray, or null if not present.</returns>
        public static JArray GetScenes(JObject root)
        {
            return root?["scenes"] as JArray;
        }

        /// <summary>
        /// Gets the default scene index.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <returns>The default scene index, or 0 if not specified.</returns>
        public static int GetDefaultSceneIndex(JObject root)
        {
            return root?["scene"]?.Value<int>() ?? 0;
        }

        /// <summary>
        /// Gets the root node indices of a scene.
        /// </summary>
        /// <param name="root">The glTF root JObject.</param>
        /// <param name="sceneIndex">The scene index.</param>
        /// <returns>Array of root node indices, or empty array if none.</returns>
        public static int[] GetSceneRootNodes(JObject root, int sceneIndex)
        {
            var scenes = GetScenes(root);
            if (scenes == null || sceneIndex < 0 || sceneIndex >= scenes.Count)
            {
                return Array.Empty<int>();
            }

            var scene = scenes[sceneIndex] as JObject;
            var nodes = scene?["nodes"] as JArray;
            
            if (nodes == null || nodes.Count == 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                result[i] = nodes[i].Value<int>();
            }
            return result;
        }
    }
}
