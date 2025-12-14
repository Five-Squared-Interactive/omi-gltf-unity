using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace OMI.Extensions.EnvironmentSky
{
    /// <summary>
    /// Default handler for OMI_environment_sky extension.
    /// Applies sky settings using Unity's built-in skybox and lighting systems.
    /// 
    /// Supported:
    /// - Plain sky: Sets solid color skybox
    /// - Gradient sky: Uses Unity's procedural skybox shader
    /// - Panorama sky: Uses equirectangular or cubemap textures
    /// - Physical sky: Partially supported (requires HDRP for full support)
    /// </summary>
    public class DefaultEnvironmentSkyHandler : IEnvironmentSkyHandler
    {
        public const string EXTENSION_NAME = "OMI_environment_sky";

        // Unity built-in shader names
        private const string SKYBOX_PROCEDURAL_SHADER = "Skybox/Procedural";
        private const string SKYBOX_CUBEMAP_SHADER = "Skybox/Cubemap";
        private const string SKYBOX_PANORAMIC_SHADER = "Skybox/Panoramic";

        public string ExtensionName => EXTENSION_NAME;
        
        public int Priority => 10;

        /// <inheritdoc/>
        public Task OnImportAsync(OMIEnvironmentSkySkyData data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            // Delegate to document import
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<OMIEnvironmentSkySkyData> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Delegate to document export
            return OnDocumentExportAsync(context, cancellationToken);
        }

        /// <inheritdoc/>
        public Task OnDocumentImportAsync(OMIEnvironmentSkySkyData data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data != null)
            {
                ApplySky(data, 0, context);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<OMIEnvironmentSkySkyData> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            var skyData = GetSkyData(context);
            return Task.FromResult(skyData);
        }

        /// <summary>
        /// Imports the document-level extension. Called once per glTF document.
        /// </summary>
        public void Import(OMIEnvironmentSkySkyData data, OMIImportContext context)
        {
            // Document-level handling is done through scene processing
            // Individual sky data is handled by ApplySky
        }

        /// <summary>
        /// Applies a sky to the Unity scene.
        /// </summary>
        public void ApplySky(OMIEnvironmentSkySkyData skyData, int skyIndex, OMIImportContext context)
        {
            if (skyData == null) return;

            // Set ambient lighting
            SetAmbientLighting(skyData);

            // Apply sky based on type
            var skyType = skyData.GetSkyType();
            switch (skyType)
            {
                case OMIEnvironmentSkyType.Plain:
                    ApplyPlainSky(skyData);
                    break;

                case OMIEnvironmentSkyType.Gradient:
                    ApplyGradientSky(skyData);
                    break;

                case OMIEnvironmentSkyType.Panorama:
                    ApplyPanoramaSky(skyData, context);
                    break;

                case OMIEnvironmentSkyType.Physical:
                    ApplyPhysicalSky(skyData);
                    break;
            }
        }

        /// <summary>
        /// Gets sky data from the current scene for export.
        /// </summary>
        public OMIEnvironmentSkySkyData GetSkyData(OMIExportContext context)
        {
            // Look for OMIEnvironmentSky component in the scene
            var skyComponent = Object.FindFirstObjectByType<OMIEnvironmentSky>();
            if (skyComponent != null)
            {
                return skyComponent.ToData();
            }

            // Alternatively, try to extract from current Unity skybox
            return ExtractSkyDataFromUnity();
        }

        /// <summary>
        /// Exports the sky extension data.
        /// </summary>
        public void Export(OMIEnvironmentSkySkyData data, OMIExportContext context)
        {
            // Export is handled at the document level through GetSkyData
        }

        private void SetAmbientLighting(OMIEnvironmentSkySkyData skyData)
        {
            var (r, g, b) = skyData.GetAmbientLightColor();
            var contribution = skyData.GetAmbientSkyContribution();

            // Set Unity ambient light
            RenderSettings.ambientLight = new Color(r, g, b) * contribution;
            RenderSettings.ambientIntensity = contribution;

            // Try to use skybox ambient mode
            RenderSettings.ambientMode = AmbientMode.Skybox;
        }

        private void ApplyPlainSky(OMIEnvironmentSkySkyData skyData)
        {
            if (skyData.Plain == null) return;

            var (r, g, b) = skyData.Plain.GetColor();
            var color = new Color(r, g, b);

            // Create a simple solid color material
            // Unity doesn't have a built-in solid color skybox, so we use a procedural one
            // with all colors set to the same value
            var shader = Shader.Find(SKYBOX_PROCEDURAL_SHADER);
            if (shader == null)
            {
                Debug.LogWarning("[OMI_environment_sky] Could not find procedural skybox shader. Using fallback.");
                RenderSettings.skybox = null;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = color;
                return;
            }

            var material = new Material(shader);
            material.SetColor("_SkyTint", color);
            material.SetColor("_GroundColor", color);
            material.SetFloat("_AtmosphereThickness", 0f);
            material.SetFloat("_Exposure", 1f);
            material.SetFloat("_SunSize", 0f);
            material.SetFloat("_SunSizeConvergence", 1f);

            RenderSettings.skybox = material;
            DynamicGI.UpdateEnvironment();
        }

        private void ApplyGradientSky(OMIEnvironmentSkySkyData skyData)
        {
            if (skyData.Gradient == null) return;

            var shader = Shader.Find(SKYBOX_PROCEDURAL_SHADER);
            if (shader == null)
            {
                Debug.LogWarning("[OMI_environment_sky] Could not find procedural skybox shader for gradient sky.");
                return;
            }

            var gradientData = skyData.Gradient;
            var (tr, tg, tb) = gradientData.GetTopColor();
            var (hr, hg, hb) = gradientData.GetHorizonColor();
            var (br, bg, bb) = gradientData.GetBottomColor();

            var material = new Material(shader);

            // Unity's procedural skybox uses SkyTint and GroundColor
            // We approximate the gradient using these parameters
            material.SetColor("_SkyTint", new Color(tr, tg, tb));
            material.SetColor("_GroundColor", new Color(br, bg, bb));

            // Atmosphere thickness affects the gradient - use curve values to approximate
            float avgCurve = (gradientData.GetTopCurve() + gradientData.GetBottomCurve()) / 2f;
            material.SetFloat("_AtmosphereThickness", Mathf.Clamp(avgCurve * 5f, 0.1f, 5f));

            material.SetFloat("_Exposure", 1f);

            // Sun settings
            material.SetFloat("_SunSize", gradientData.GetSunAngleMax() * Mathf.Rad2Deg / 180f);
            material.SetFloat("_SunSizeConvergence", 1f / Mathf.Max(gradientData.GetSunCurve(), 0.01f));

            RenderSettings.skybox = material;
            DynamicGI.UpdateEnvironment();
        }

        private void ApplyPanoramaSky(OMIEnvironmentSkySkyData skyData, OMIImportContext context)
        {
            if (skyData.Panorama == null) return;

            var panoramaData = skyData.Panorama;

            // Prefer equirectangular over cubemap for broader support
            if (panoramaData.HasEquirectangular)
            {
                var shader = Shader.Find(SKYBOX_PANORAMIC_SHADER);
                if (shader == null)
                {
                    Debug.LogWarning("[OMI_environment_sky] Could not find panoramic skybox shader.");
                    return;
                }

                var material = new Material(shader);

                // Get texture from glTF - texture would need to be resolved through the import context
                // This is a placeholder - actual implementation needs texture resolution
                Debug.Log($"[OMI_environment_sky] Panorama sky with equirectangular texture index: {panoramaData.Equirectangular}");

                // Material setup for when texture is available
                material.SetFloat("_Rotation", 0f);
                material.SetFloat("_Exposure", 1f);
                material.SetInt("_Mapping", 1); // Latitude Longitude Layout

                RenderSettings.skybox = material;
            }
            else if (panoramaData.HasCubemap)
            {
                var shader = Shader.Find(SKYBOX_CUBEMAP_SHADER);
                if (shader == null)
                {
                    Debug.LogWarning("[OMI_environment_sky] Could not find cubemap skybox shader.");
                    return;
                }

                var material = new Material(shader);

                Debug.Log($"[OMI_environment_sky] Panorama sky with cubemap texture indices: {string.Join(", ", panoramaData.Cubemap)}");

                // Cubemap would need to be constructed from 6 textures
                // This is a placeholder - actual implementation needs texture resolution and cubemap creation
                material.SetFloat("_Rotation", 0f);
                material.SetFloat("_Exposure", 1f);

                RenderSettings.skybox = material;
            }

            DynamicGI.UpdateEnvironment();
        }

        private void ApplyPhysicalSky(OMIEnvironmentSkySkyData skyData)
        {
            if (skyData.Physical == null) return;

            // Physical sky requires HDRP for full support
            // For built-in/URP, we approximate using procedural skybox
#if UNITY_PIPELINE_HDRP
            ApplyPhysicalSkyHDRP(skyData);
#else
            ApplyPhysicalSkyFallback(skyData);
#endif
        }

        private void ApplyPhysicalSkyFallback(OMIEnvironmentSkySkyData skyData)
        {
            Debug.LogWarning("[OMI_environment_sky] Physical sky requires HDRP for full support. Using procedural approximation.");

            var physicalData = skyData.Physical;
            var shader = Shader.Find(SKYBOX_PROCEDURAL_SHADER);
            if (shader == null) return;

            var material = new Material(shader);

            // Approximate physical sky with procedural skybox
            var (rr, rg, rb) = physicalData.GetRayleighColor();
            material.SetColor("_SkyTint", new Color(rr, rg, rb));

            var (gr, gg, gb) = physicalData.GetGroundColor();
            material.SetColor("_GroundColor", new Color(gr, gg, gb));

            // Use Rayleigh scale to influence atmosphere thickness
            // glTF uses inverse meters, Unity's procedural shader uses a unitless 0-5 range
            float atmosphereThickness = Mathf.Clamp(physicalData.GetRayleighScale() * 100000f, 0.1f, 5f);
            material.SetFloat("_AtmosphereThickness", atmosphereThickness);

            material.SetFloat("_Exposure", 1f);
            material.SetFloat("_SunSize", 0.04f); // Default sun size
            material.SetFloat("_SunSizeConvergence", 5f);

            RenderSettings.skybox = material;
            DynamicGI.UpdateEnvironment();
        }

#if UNITY_PIPELINE_HDRP
        private void ApplyPhysicalSkyHDRP(OMIEnvironmentSkySkyData skyData)
        {
            // HDRP-specific implementation would go here
            // This requires the HDRP package and Volume system
            Debug.Log("[OMI_environment_sky] HDRP physical sky implementation pending.");
            ApplyPhysicalSkyFallback(skyData);
        }
#endif

        /// <summary>
        /// Attempts to extract sky data from Unity's current render settings.
        /// </summary>
        private OMIEnvironmentSkySkyData ExtractSkyDataFromUnity()
        {
            if (RenderSettings.skybox == null)
            {
                // No skybox, check camera clear color
                var mainCamera = Camera.main;
                if (mainCamera != null && mainCamera.clearFlags == CameraClearFlags.SolidColor)
                {
                    var bgColor = mainCamera.backgroundColor;
                    return new OMIEnvironmentSkySkyData
                    {
                        Type = "plain",
                        Plain = new OMIEnvironmentSkyPlainData
                        {
                            Color = new[] { bgColor.r, bgColor.g, bgColor.b }
                        },
                        AmbientLightColor = new[] 
                        { 
                            RenderSettings.ambientLight.r, 
                            RenderSettings.ambientLight.g, 
                            RenderSettings.ambientLight.b 
                        },
                        AmbientSkyContribution = RenderSettings.ambientIntensity
                    };
                }
                return null;
            }

            var skyboxMaterial = RenderSettings.skybox;
            var shaderName = skyboxMaterial.shader.name;

            // Try to extract based on shader type
            if (shaderName.Contains("Procedural"))
            {
                return ExtractProceduralSkyData(skyboxMaterial);
            }
            else if (shaderName.Contains("Panoramic") || shaderName.Contains("Cubemap"))
            {
                return ExtractPanoramaSkyData(skyboxMaterial);
            }

            return null;
        }

        private OMIEnvironmentSkySkyData ExtractProceduralSkyData(Material material)
        {
            var skyTint = material.HasProperty("_SkyTint") ? material.GetColor("_SkyTint") : Color.white;
            var groundColor = material.HasProperty("_GroundColor") ? material.GetColor("_GroundColor") : Color.gray;
            var sunSize = material.HasProperty("_SunSize") ? material.GetFloat("_SunSize") : 0.04f;

            return new OMIEnvironmentSkySkyData
            {
                Type = "gradient",
                Gradient = new OMIEnvironmentSkyGradientData
                {
                    TopColor = new[] { skyTint.r, skyTint.g, skyTint.b },
                    HorizonColor = new[] 
                    { 
                        (skyTint.r + groundColor.r) / 2f,
                        (skyTint.g + groundColor.g) / 2f,
                        (skyTint.b + groundColor.b) / 2f
                    },
                    BottomColor = new[] { groundColor.r, groundColor.g, groundColor.b },
                    SunAngleMax = sunSize * 180f * Mathf.Deg2Rad
                },
                AmbientLightColor = new[] 
                { 
                    RenderSettings.ambientLight.r, 
                    RenderSettings.ambientLight.g, 
                    RenderSettings.ambientLight.b 
                },
                AmbientSkyContribution = RenderSettings.ambientIntensity
            };
        }

        private OMIEnvironmentSkySkyData ExtractPanoramaSkyData(Material material)
        {
            // We can't easily extract texture indices for export
            // The texture would need to be exported separately and indexed
            return new OMIEnvironmentSkySkyData
            {
                Type = "panorama",
                Panorama = new OMIEnvironmentSkyPanoramaData
                {
                    // Texture indices would be set during export when textures are processed
                },
                AmbientLightColor = new[] 
                { 
                    RenderSettings.ambientLight.r, 
                    RenderSettings.ambientLight.g, 
                    RenderSettings.ambientLight.b 
                },
                AmbientSkyContribution = RenderSettings.ambientIntensity
            };
        }
    }
}
