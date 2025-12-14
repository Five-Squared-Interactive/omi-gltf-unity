using UnityEngine;

namespace OMI.Extensions.EnvironmentSky
{
    /// <summary>
    /// Component representing OMI_environment_sky configuration.
    /// Attach to a GameObject to mark it as controlling the scene's sky.
    /// This component stores sky configuration that can be exported to glTF.
    /// </summary>
    [AddComponentMenu("OMI/OMI Environment Sky")]
    public class OMIEnvironmentSky : MonoBehaviour
    {
        [Header("Sky Type")]
        [Tooltip("The type of sky to use.")]
        public OMIEnvironmentSkyType skyType = OMIEnvironmentSkyType.Plain;

        [Header("Ambient Lighting")]
        [Tooltip("The color of ambient light from the sky.")]
        public Color ambientLightColor = Color.white;

        [Tooltip("How much the sky contributes to ambient lighting (0-1).")]
        [Range(0f, 1f)]
        public float ambientSkyContribution = 1.0f;

        [Header("Gradient Sky")]
        [Tooltip("Settings for gradient sky type.")]
        public GradientSkySettings gradient = new GradientSkySettings();

        [Header("Panorama Sky")]
        [Tooltip("Settings for panorama sky type.")]
        public PanoramaSkySettings panorama = new PanoramaSkySettings();

        [Header("Physical Sky")]
        [Tooltip("Settings for physical sky type. Note: Requires HDRP for full support.")]
        public PhysicalSkySettings physical = new PhysicalSkySettings();

        [Header("Plain Sky")]
        [Tooltip("Settings for plain (solid color) sky type.")]
        public PlainSkySettings plain = new PlainSkySettings();

        /// <summary>
        /// Converts this component to OMI_environment_sky data for export.
        /// </summary>
        public OMIEnvironmentSkySkyData ToData()
        {
            var data = new OMIEnvironmentSkySkyData
            {
                Type = skyType.ToTypeString(),
                AmbientLightColor = new[] { ambientLightColor.r, ambientLightColor.g, ambientLightColor.b },
                AmbientSkyContribution = ambientSkyContribution
            };

            switch (skyType)
            {
                case OMIEnvironmentSkyType.Gradient:
                    data.Gradient = new OMIEnvironmentSkyGradientData
                    {
                        TopColor = new[] { gradient.topColor.r, gradient.topColor.g, gradient.topColor.b },
                        HorizonColor = new[] { gradient.horizonColor.r, gradient.horizonColor.g, gradient.horizonColor.b },
                        BottomColor = new[] { gradient.bottomColor.r, gradient.bottomColor.g, gradient.bottomColor.b },
                        TopCurve = gradient.topCurve,
                        BottomCurve = gradient.bottomCurve,
                        SunAngleMax = gradient.sunAngleMax,
                        SunCurve = gradient.sunCurve
                    };
                    break;

                case OMIEnvironmentSkyType.Panorama:
                    data.Panorama = new OMIEnvironmentSkyPanoramaData();
                    // Note: Texture indices will be set during export when textures are added to the glTF
                    break;

                case OMIEnvironmentSkyType.Physical:
                    data.Physical = new OMIEnvironmentSkyPhysicalData
                    {
                        GroundColor = new[] { physical.groundColor.r, physical.groundColor.g, physical.groundColor.b },
                        MieAnisotropy = physical.mieAnisotropy,
                        MieColor = new[] { physical.mieColor.r, physical.mieColor.g, physical.mieColor.b },
                        MieScale = physical.mieScale,
                        RayleighColor = new[] { physical.rayleighColor.r, physical.rayleighColor.g, physical.rayleighColor.b },
                        RayleighScale = physical.rayleighScale
                    };
                    break;

                case OMIEnvironmentSkyType.Plain:
                    data.Plain = new OMIEnvironmentSkyPlainData
                    {
                        Color = new[] { plain.color.r, plain.color.g, plain.color.b }
                    };
                    break;
            }

            return data;
        }

        /// <summary>
        /// Applies data from OMI_environment_sky to this component.
        /// </summary>
        public void FromData(OMIEnvironmentSkySkyData data)
        {
            if (data == null) return;

            skyType = data.GetSkyType();

            var (ar, ag, ab) = data.GetAmbientLightColor();
            ambientLightColor = new Color(ar, ag, ab);
            ambientSkyContribution = data.GetAmbientSkyContribution();

            if (data.Gradient != null)
            {
                var (tr, tg, tb) = data.Gradient.GetTopColor();
                gradient.topColor = new Color(tr, tg, tb);

                var (hr, hg, hb) = data.Gradient.GetHorizonColor();
                gradient.horizonColor = new Color(hr, hg, hb);

                var (br, bg, bb) = data.Gradient.GetBottomColor();
                gradient.bottomColor = new Color(br, bg, bb);

                gradient.topCurve = data.Gradient.GetTopCurve();
                gradient.bottomCurve = data.Gradient.GetBottomCurve();
                gradient.sunAngleMax = data.Gradient.GetSunAngleMax();
                gradient.sunCurve = data.Gradient.GetSunCurve();
            }

            if (data.Panorama != null)
            {
                // Textures are handled separately through the handler
                if (data.Panorama.HasCubemap)
                {
                    panorama.useCubemap = true;
                }
                if (data.Panorama.HasEquirectangular)
                {
                    panorama.useCubemap = false;
                }
            }

            if (data.Physical != null)
            {
                var (gr, gg, gb) = data.Physical.GetGroundColor();
                physical.groundColor = new Color(gr, gg, gb);

                physical.mieAnisotropy = data.Physical.GetMieAnisotropy();

                var (mr, mg, mb) = data.Physical.GetMieColor();
                physical.mieColor = new Color(mr, mg, mb);

                physical.mieScale = data.Physical.GetMieScale();

                var (rr, rg, rb) = data.Physical.GetRayleighColor();
                physical.rayleighColor = new Color(rr, rg, rb);

                physical.rayleighScale = data.Physical.GetRayleighScale();
            }

            if (data.Plain != null)
            {
                var (pr, pg, pb) = data.Plain.GetColor();
                plain.color = new Color(pr, pg, pb);
            }
        }
    }

    /// <summary>
    /// Settings for gradient sky.
    /// </summary>
    [System.Serializable]
    public class GradientSkySettings
    {
        [Tooltip("Color at the top of the sky.")]
        public Color topColor = new Color(0.3f, 0.5f, 1.0f);

        [Tooltip("Color at the horizon.")]
        public Color horizonColor = new Color(0.8f, 0.8f, 0.9f);

        [Tooltip("Color at the bottom of the sky.")]
        public Color bottomColor = new Color(0.3f, 0.3f, 0.3f);

        [Tooltip("Curve controlling the transition from horizon to top.")]
        [Range(0.01f, 10f)]
        public float topCurve = 0.2f;

        [Tooltip("Curve controlling the transition from horizon to bottom.")]
        [Range(0.01f, 10f)]
        public float bottomCurve = 0.2f;

        [Tooltip("Maximum sun angle in radians.")]
        [Range(0.01f, 1.57f)]
        public float sunAngleMax = 0.5f;

        [Tooltip("Transition curve between sun and sky colors.")]
        [Range(0.01f, 10f)]
        public float sunCurve = 0.15f;
    }

    /// <summary>
    /// Settings for panorama sky.
    /// </summary>
    [System.Serializable]
    public class PanoramaSkySettings
    {
        [Tooltip("Whether to use a cubemap (6 textures) or equirectangular texture.")]
        public bool useCubemap = false;

        [Tooltip("Equirectangular panorama texture.")]
        public Texture2D equirectangularTexture;

        [Tooltip("Cubemap for skybox (if useCubemap is true).")]
        public Cubemap cubemapTexture;
    }

    /// <summary>
    /// Settings for physical sky.
    /// Note: Full physical sky requires HDRP in Unity.
    /// </summary>
    [System.Serializable]
    public class PhysicalSkySettings
    {
        [Tooltip("Color of the ground (not part of atmospheric simulation).")]
        public Color groundColor = new Color(0.3f, 0.2f, 0.1f);

        [Tooltip("Mie scattering anisotropy (-1 to 1).")]
        [Range(-1f, 1f)]
        public float mieAnisotropy = 0.8f;

        [Tooltip("Color of Mie scattering (clouds).")]
        public Color mieColor = Color.white;

        [Tooltip("Scale of Mie scattering in inverse meters.")]
        public float mieScale = 0.000005f;

        [Tooltip("Color of Rayleigh scattering (sky blue).")]
        public Color rayleighColor = new Color(0.3f, 0.5f, 1.0f);

        [Tooltip("Scale of Rayleigh scattering in inverse meters.")]
        public float rayleighScale = 0.00003f;
    }

    /// <summary>
    /// Settings for plain (solid color) sky.
    /// </summary>
    [System.Serializable]
    public class PlainSkySettings
    {
        [Tooltip("The solid color of the sky.")]
        public Color color = Color.black;
    }
}
