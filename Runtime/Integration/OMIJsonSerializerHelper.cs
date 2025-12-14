// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace OMI.Integration
{
    /// <summary>
    /// Custom JSON contract resolver for OMI extension data.
    /// Handles Unity types and glTF conventions.
    /// </summary>
    public class OMIJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        private static readonly HashSet<Type> IgnoredTypes = new HashSet<Type>
        {
            typeof(UnityEngine.Object),
            typeof(GameObject),
            typeof(Component),
            typeof(Transform)
        };

        protected override JsonProperty CreateProperty(
            System.Reflection.MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // Skip Unity types that shouldn't be serialized
            if (property.PropertyType != null && IgnoredTypes.Contains(property.PropertyType))
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }

    /// <summary>
    /// JSON converter for Unity Vector3 to glTF float array.
    /// glTF uses right-handed coordinate system, Unity uses left-handed.
    /// </summary>
    public class Vector3JsonConverter : JsonConverter<Vector3>
    {
        private readonly bool _convertCoordinates;

        public Vector3JsonConverter(bool convertCoordinates = true)
        {
            _convertCoordinates = convertCoordinates;
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var array = serializer.Deserialize<float[]>(reader);
                if (array != null && array.Length >= 3)
                {
                    var v = new Vector3(array[0], array[1], array[2]);
                    return _convertCoordinates ? ConvertFromGltf(v) : v;
                }
            }
            return Vector3.zero;
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            var v = _convertCoordinates ? ConvertToGltf(value) : value;
            writer.WriteStartArray();
            writer.WriteValue(v.x);
            writer.WriteValue(v.y);
            writer.WriteValue(v.z);
            writer.WriteEndArray();
        }

        /// <summary>
        /// Converts Unity left-handed to glTF right-handed (flip Z).
        /// </summary>
        public static Vector3 ConvertToGltf(Vector3 unity)
        {
            return new Vector3(unity.x, unity.y, -unity.z);
        }

        /// <summary>
        /// Converts glTF right-handed to Unity left-handed (flip Z).
        /// </summary>
        public static Vector3 ConvertFromGltf(Vector3 gltf)
        {
            return new Vector3(gltf.x, gltf.y, -gltf.z);
        }
    }

    /// <summary>
    /// JSON converter for Unity Quaternion to glTF float array.
    /// </summary>
    public class QuaternionJsonConverter : JsonConverter<Quaternion>
    {
        private readonly bool _convertCoordinates;

        public QuaternionJsonConverter(bool convertCoordinates = true)
        {
            _convertCoordinates = convertCoordinates;
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var array = serializer.Deserialize<float[]>(reader);
                if (array != null && array.Length >= 4)
                {
                    var q = new Quaternion(array[0], array[1], array[2], array[3]);
                    return _convertCoordinates ? ConvertFromGltf(q) : q;
                }
            }
            return Quaternion.identity;
        }

        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            var q = _convertCoordinates ? ConvertToGltf(value) : value;
            writer.WriteStartArray();
            writer.WriteValue(q.x);
            writer.WriteValue(q.y);
            writer.WriteValue(q.z);
            writer.WriteValue(q.w);
            writer.WriteEndArray();
        }

        /// <summary>
        /// Converts Unity quaternion to glTF (flip Y and Z rotation axes).
        /// </summary>
        public static Quaternion ConvertToGltf(Quaternion unity)
        {
            return new Quaternion(-unity.x, -unity.y, unity.z, unity.w);
        }

        /// <summary>
        /// Converts glTF quaternion to Unity (flip Y and Z rotation axes).
        /// </summary>
        public static Quaternion ConvertFromGltf(Quaternion gltf)
        {
            return new Quaternion(-gltf.x, -gltf.y, gltf.z, gltf.w);
        }
    }

    /// <summary>
    /// JSON converter for Unity Color to glTF float array (RGBA or RGB).
    /// </summary>
    public class ColorJsonConverter : JsonConverter<Color>
    {
        private readonly bool _includeAlpha;

        public ColorJsonConverter(bool includeAlpha = true)
        {
            _includeAlpha = includeAlpha;
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var array = serializer.Deserialize<float[]>(reader);
                if (array != null && array.Length >= 3)
                {
                    float alpha = array.Length >= 4 ? array[3] : 1f;
                    return new Color(array[0], array[1], array[2], alpha);
                }
            }
            return Color.white;
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.r);
            writer.WriteValue(value.g);
            writer.WriteValue(value.b);
            if (_includeAlpha)
            {
                writer.WriteValue(value.a);
            }
            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Helper methods for OMI JSON serialization.
    /// </summary>
    public static class OMIJsonSerializerHelper
    {
        private static JsonSerializerSettings _defaultSettings;
        private static JsonSerializerSettings _noCoordConvertSettings;

        /// <summary>
        /// Gets the default JSON serializer settings for OMI extensions.
        /// Includes coordinate system conversion.
        /// </summary>
        public static JsonSerializerSettings DefaultSettings
        {
            get
            {
                if (_defaultSettings == null)
                {
                    _defaultSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        ContractResolver = new OMIJsonContractResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new Vector3JsonConverter(convertCoordinates: true),
                            new QuaternionJsonConverter(convertCoordinates: true),
                            new ColorJsonConverter(includeAlpha: true)
                        }
                    };
                }
                return _defaultSettings;
            }
        }

        /// <summary>
        /// Gets JSON serializer settings without coordinate conversion.
        /// Use for data that's already in glTF coordinate system.
        /// </summary>
        public static JsonSerializerSettings NoCoordConvertSettings
        {
            get
            {
                if (_noCoordConvertSettings == null)
                {
                    _noCoordConvertSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        ContractResolver = new OMIJsonContractResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new Vector3JsonConverter(convertCoordinates: false),
                            new QuaternionJsonConverter(convertCoordinates: false),
                            new ColorJsonConverter(includeAlpha: true)
                        }
                    };
                }
                return _noCoordConvertSettings;
            }
        }

        /// <summary>
        /// Serializes an object to a JSON string using OMI settings.
        /// </summary>
        public static string Serialize(object obj, bool convertCoordinates = true)
        {
            var settings = convertCoordinates ? DefaultSettings : NoCoordConvertSettings;
            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// Deserializes a JSON string to an object using OMI settings.
        /// </summary>
        public static T Deserialize<T>(string json, bool convertCoordinates = true)
        {
            var settings = convertCoordinates ? DefaultSettings : NoCoordConvertSettings;
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        /// <summary>
        /// Creates a JsonSerializer with OMI settings.
        /// </summary>
        public static JsonSerializer CreateSerializer(bool convertCoordinates = true)
        {
            var settings = convertCoordinates ? DefaultSettings : NoCoordConvertSettings;
            return JsonSerializer.Create(settings);
        }
    }

    /// <summary>
    /// Extension methods for coordinate conversion during export.
    /// </summary>
    public static class OMICoordinateConversion
    {
        /// <summary>
        /// Converts a Unity Vector3 to a glTF-compatible float array.
        /// </summary>
        public static float[] ToGltfArray(this Vector3 vector, bool convertCoordinates = true)
        {
            if (convertCoordinates)
            {
                return new[] { vector.x, vector.y, -vector.z };
            }
            return new[] { vector.x, vector.y, vector.z };
        }

        /// <summary>
        /// Converts a Unity Quaternion to a glTF-compatible float array.
        /// </summary>
        public static float[] ToGltfArray(this Quaternion quaternion, bool convertCoordinates = true)
        {
            if (convertCoordinates)
            {
                return new[] { -quaternion.x, -quaternion.y, quaternion.z, quaternion.w };
            }
            return new[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }

        /// <summary>
        /// Converts a Unity Color to a float array (RGBA).
        /// </summary>
        public static float[] ToArray(this Color color, bool includeAlpha = true)
        {
            if (includeAlpha)
            {
                return new[] { color.r, color.g, color.b, color.a };
            }
            return new[] { color.r, color.g, color.b };
        }

        /// <summary>
        /// Creates a Vector3 from a glTF float array.
        /// </summary>
        public static Vector3 ToUnityVector3(this float[] array, bool convertCoordinates = true)
        {
            if (array == null || array.Length < 3) return Vector3.zero;
            if (convertCoordinates)
            {
                return new Vector3(array[0], array[1], -array[2]);
            }
            return new Vector3(array[0], array[1], array[2]);
        }

        /// <summary>
        /// Creates a Quaternion from a glTF float array.
        /// </summary>
        public static Quaternion ToUnityQuaternion(this float[] array, bool convertCoordinates = true)
        {
            if (array == null || array.Length < 4) return Quaternion.identity;
            if (convertCoordinates)
            {
                return new Quaternion(-array[0], -array[1], array[2], array[3]);
            }
            return new Quaternion(array[0], array[1], array[2], array[3]);
        }

        /// <summary>
        /// Creates a Color from a float array.
        /// </summary>
        public static Color ToUnityColor(this float[] array)
        {
            if (array == null || array.Length < 3) return Color.white;
            float a = array.Length >= 4 ? array[3] : 1f;
            return new Color(array[0], array[1], array[2], a);
        }
    }
}
