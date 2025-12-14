using UnityEngine;

namespace OMI.Extensions.EnvironmentSky
{
    /// <summary>
    /// Handler interface for OMI_environment_sky extension.
    /// This is a document-level extension that applies to the entire scene.
    /// </summary>
    public interface IEnvironmentSkyHandler : IOMIDocumentExtensionHandler<OMIEnvironmentSkySkyData>
    {
        /// <summary>
        /// Applies a sky to the scene.
        /// </summary>
        /// <param name="skyData">The sky data to apply.</param>
        /// <param name="skyIndex">The index of this sky in the document's skies array.</param>
        /// <param name="context">The import context.</param>
        void ApplySky(OMIEnvironmentSkySkyData skyData, int skyIndex, OMIImportContext context);

        /// <summary>
        /// Gets the sky data from the current scene for export.
        /// </summary>
        /// <param name="context">The export context.</param>
        /// <returns>The sky data, or null if no sky is configured.</returns>
        OMIEnvironmentSkySkyData GetSkyData(OMIExportContext context);
    }
}
