using System;
using System.Collections.Generic;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace OMI.Extensions.EnvironmentSky
{
    /// <summary>
    /// Document-level extension data for OMI_environment_sky.
    /// Contains an array of sky resources that can be referenced by scenes.
    /// </summary>
    [Serializable]
    public class OMIEnvironmentSkyDocumentData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("skies")]
#endif
        public List<OMIEnvironmentSkySkyData> Skies { get; set; } = new List<OMIEnvironmentSkySkyData>();
    }

    /// <summary>
    /// Scene-level extension data for OMI_environment_sky.
    /// References which sky from the document-level array to use for this scene.
    /// </summary>
    [Serializable]
    public class OMIEnvironmentSkySceneData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("sky")]
#endif
        public int Sky { get; set; } = 0;
    }

    /// <summary>
    /// Data for a single sky definition.
    /// </summary>
    [Serializable]
    public class OMIEnvironmentSkySkyData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("ambientLightColor")]
#endif
        public float[] AmbientLightColor { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("ambientSkyContribution")]
#endif
        public float? AmbientSkyContribution { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("type")]
#endif
        public string Type { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("gradient")]
#endif
        public OMIEnvironmentSkyGradientData Gradient { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("panorama")]
#endif
        public OMIEnvironmentSkyPanoramaData Panorama { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("physical")]
#endif
        public OMIEnvironmentSkyPhysicalData Physical { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("plain")]
#endif
        public OMIEnvironmentSkyPlainData Plain { get; set; }

        /// <summary>
        /// Gets the ambient light color as RGB. Default is [1, 1, 1] (white).
        /// </summary>
        public (float r, float g, float b) GetAmbientLightColor()
        {
            if (AmbientLightColor != null && AmbientLightColor.Length >= 3)
                return (AmbientLightColor[0], AmbientLightColor[1], AmbientLightColor[2]);
            return (1f, 1f, 1f);
        }

        /// <summary>
        /// Gets the ambient sky contribution. Default is 1.0.
        /// </summary>
        public float GetAmbientSkyContribution()
        {
            return AmbientSkyContribution ?? 1.0f;
        }
    }

    /// <summary>
    /// Gradient sky properties.
    /// Uses a color gradient to represent the sky with colors for top, horizon, and bottom.
    /// </summary>
    [Serializable]
    public class OMIEnvironmentSkyGradientData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("bottomColor")]
#endif
        public float[] BottomColor { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("bottomCurve")]
#endif
        public float? BottomCurve { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("horizonColor")]
#endif
        public float[] HorizonColor { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("topColor")]
#endif
        public float[] TopColor { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("topCurve")]
#endif
        public float? TopCurve { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("sunAngleMax")]
#endif
        public float? SunAngleMax { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("sunCurve")]
#endif
        public float? SunCurve { get; set; }

        /// <summary>
        /// Gets the bottom color. Default is [0.3, 0.3, 0.3] (dark gray).
        /// </summary>
        public (float r, float g, float b) GetBottomColor()
        {
            if (BottomColor != null && BottomColor.Length >= 3)
                return (BottomColor[0], BottomColor[1], BottomColor[2]);
            return (0.3f, 0.3f, 0.3f);
        }

        /// <summary>
        /// Gets the bottom curve. Default is 0.2.
        /// </summary>
        public float GetBottomCurve()
        {
            return BottomCurve ?? 0.2f;
        }

        /// <summary>
        /// Gets the horizon color. Default is [0.8, 0.8, 0.9] (light gray-blue).
        /// </summary>
        public (float r, float g, float b) GetHorizonColor()
        {
            if (HorizonColor != null && HorizonColor.Length >= 3)
                return (HorizonColor[0], HorizonColor[1], HorizonColor[2]);
            return (0.8f, 0.8f, 0.9f);
        }

        /// <summary>
        /// Gets the top color. Default is [0.3, 0.5, 1.0] (sky blue).
        /// </summary>
        public (float r, float g, float b) GetTopColor()
        {
            if (TopColor != null && TopColor.Length >= 3)
                return (TopColor[0], TopColor[1], TopColor[2]);
            return (0.3f, 0.5f, 1.0f);
        }

        /// <summary>
        /// Gets the top curve. Default is 0.2.
        /// </summary>
        public float GetTopCurve()
        {
            return TopCurve ?? 0.2f;
        }

        /// <summary>
        /// Gets the sun angle max in radians. Default is 0.5 (~28.6 degrees).
        /// </summary>
        public float GetSunAngleMax()
        {
            return SunAngleMax ?? 0.5f;
        }

        /// <summary>
        /// Gets the sun curve. Default is 0.15.
        /// </summary>
        public float GetSunCurve()
        {
            return SunCurve ?? 0.15f;
        }
    }

    /// <summary>
    /// Panorama sky properties.
    /// Uses a texture or set of textures to represent the sky.
    /// </summary>
    [Serializable]
    public class OMIEnvironmentSkyPanoramaData
    {
        /// <summary>
        /// Array of 6 texture indices for cubemap faces in order: +X, -X, +Y, -Y, +Z, -Z.
        /// </summary>
#if NEWTONSOFT_JSON
        [JsonProperty("cubemap")]
#endif
        public int[] Cubemap { get; set; }

        /// <summary>
        /// Index of an equirectangular panorama texture.
        /// </summary>
#if NEWTONSOFT_JSON
        [JsonProperty("equirectangular")]
#endif
        public int? Equirectangular { get; set; }

        /// <summary>
        /// Returns true if this panorama has a valid cubemap (6 texture indices).
        /// </summary>
        public bool HasCubemap => Cubemap != null && Cubemap.Length == 6;

        /// <summary>
        /// Returns true if this panorama has an equirectangular texture.
        /// </summary>
        public bool HasEquirectangular => Equirectangular.HasValue;
    }

    /// <summary>
    /// Physical sky properties.
    /// Uses physically-based atmospheric scattering simulation.
    /// Note: Requires HDRP in Unity for full support.
    /// </summary>
    [Serializable]
    public class OMIEnvironmentSkyPhysicalData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("groundColor")]
#endif
        public float[] GroundColor { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("mieAnisotropy")]
#endif
        public float? MieAnisotropy { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("mieColor")]
#endif
        public float[] MieColor { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("mieScale")]
#endif
        public float? MieScale { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("rayleighColor")]
#endif
        public float[] RayleighColor { get; set; }

#if NEWTONSOFT_JSON
        [JsonProperty("rayleighScale")]
#endif
        public float? RayleighScale { get; set; }

        /// <summary>
        /// Gets the ground color. Default is [0.3, 0.2, 0.1] (brown).
        /// </summary>
        public (float r, float g, float b) GetGroundColor()
        {
            if (GroundColor != null && GroundColor.Length >= 3)
                return (GroundColor[0], GroundColor[1], GroundColor[2]);
            return (0.3f, 0.2f, 0.1f);
        }

        /// <summary>
        /// Gets the Mie anisotropy (eccentricity). Default is 0.8.
        /// Range: -1.0 to 1.0
        /// </summary>
        public float GetMieAnisotropy()
        {
            return MieAnisotropy ?? 0.8f;
        }

        /// <summary>
        /// Gets the Mie scattering color. Default is [1.0, 1.0, 1.0] (white).
        /// </summary>
        public (float r, float g, float b) GetMieColor()
        {
            if (MieColor != null && MieColor.Length >= 3)
                return (MieColor[0], MieColor[1], MieColor[2]);
            return (1.0f, 1.0f, 1.0f);
        }

        /// <summary>
        /// Gets the Mie scattering scale in inverse meters. Default is 0.000005.
        /// Note: Unity HDRP may use different units.
        /// </summary>
        public float GetMieScale()
        {
            return MieScale ?? 0.000005f;
        }

        /// <summary>
        /// Gets the Rayleigh scattering color. Default is [0.3, 0.5, 1.0] (sky blue).
        /// </summary>
        public (float r, float g, float b) GetRayleighColor()
        {
            if (RayleighColor != null && RayleighColor.Length >= 3)
                return (RayleighColor[0], RayleighColor[1], RayleighColor[2]);
            return (0.3f, 0.5f, 1.0f);
        }

        /// <summary>
        /// Gets the Rayleigh scattering scale in inverse meters. Default is 0.00003.
        /// Note: Unity HDRP may use different units.
        /// </summary>
        public float GetRayleighScale()
        {
            return RayleighScale ?? 0.00003f;
        }
    }

    /// <summary>
    /// Plain sky properties.
    /// Uses a single solid color for the sky.
    /// </summary>
    [Serializable]
    public class OMIEnvironmentSkyPlainData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("color")]
#endif
        public float[] Color { get; set; }

        /// <summary>
        /// Gets the sky color. Default is [0.0, 0.0, 0.0] (black).
        /// </summary>
        public (float r, float g, float b) GetColor()
        {
            if (Color != null && Color.Length >= 3)
                return (Color[0], Color[1], Color[2]);
            return (0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Enumeration of sky types.
    /// </summary>
    public enum OMIEnvironmentSkyType
    {
        /// <summary>
        /// Uses a color gradient with top, horizon, and bottom colors.
        /// </summary>
        Gradient,

        /// <summary>
        /// Uses a panorama texture (equirectangular or cubemap).
        /// </summary>
        Panorama,

        /// <summary>
        /// Uses physically-based atmospheric scattering (requires HDRP in Unity).
        /// </summary>
        Physical,

        /// <summary>
        /// Uses a single solid color.
        /// </summary>
        Plain
    }

    /// <summary>
    /// Extension methods for OMI_environment_sky data.
    /// </summary>
    public static class OMIEnvironmentSkyExtensions
    {
        /// <summary>
        /// Parses the type string to an enum value.
        /// </summary>
        public static OMIEnvironmentSkyType GetSkyType(this OMIEnvironmentSkySkyData skyData)
        {
            if (string.IsNullOrEmpty(skyData.Type))
            {
                // Infer type from which property is present
                if (skyData.Gradient != null) return OMIEnvironmentSkyType.Gradient;
                if (skyData.Panorama != null) return OMIEnvironmentSkyType.Panorama;
                if (skyData.Physical != null) return OMIEnvironmentSkyType.Physical;
                if (skyData.Plain != null) return OMIEnvironmentSkyType.Plain;
                return OMIEnvironmentSkyType.Plain; // Default
            }

            return skyData.Type.ToLowerInvariant() switch
            {
                "gradient" => OMIEnvironmentSkyType.Gradient,
                "panorama" => OMIEnvironmentSkyType.Panorama,
                "physical" => OMIEnvironmentSkyType.Physical,
                "plain" => OMIEnvironmentSkyType.Plain,
                _ => OMIEnvironmentSkyType.Plain
            };
        }

        /// <summary>
        /// Converts sky type enum to string for serialization.
        /// </summary>
        public static string ToTypeString(this OMIEnvironmentSkyType skyType)
        {
            return skyType switch
            {
                OMIEnvironmentSkyType.Gradient => "gradient",
                OMIEnvironmentSkyType.Panorama => "panorama",
                OMIEnvironmentSkyType.Physical => "physical",
                OMIEnvironmentSkyType.Plain => "plain",
                _ => "plain"
            };
        }
    }
}
