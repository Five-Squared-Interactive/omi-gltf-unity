// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.Link
{
    /// <summary>
    /// Default Unity implementation for handling OMI_link.
    /// Creates OMILink components on link nodes.
    /// </summary>
    public class DefaultLinkHandler : ILinkHandler
    {
        public string ExtensionName => OMILinkExtension.ExtensionName;
        public int Priority => 50;

        public Task OnImportAsync(OMILinkNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<OMILinkNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<OMILinkNode>(null);
        }

        public Task OnNodeImportAsync(OMILinkNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null) return Task.CompletedTask;
            
            CreateLink(data, targetObject, context);
            return Task.CompletedTask;
        }

        public Task<OMILinkNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExtractLink(sourceObject, context));
        }

        public void CreateLink(OMILinkNode data, GameObject targetObject, OMIImportContext context)
        {
            var link = targetObject.GetComponent<OMILink>();
            if (link == null)
            {
                link = targetObject.AddComponent<OMILink>();
            }

            link.Uri = data.Uri;
            link.Title = data.Title;

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created link to '{data.Uri}' on {targetObject.name}");
            }
        }

        public OMILinkNode ExtractLink(GameObject sourceObject, OMIExportContext context)
        {
            if (sourceObject == null) return null;

            var link = sourceObject.GetComponent<OMILink>();
            if (link == null || string.IsNullOrEmpty(link.Uri)) return null;

            return new OMILinkNode
            {
                Uri = link.Uri,
                Title = string.IsNullOrEmpty(link.Title) ? null : link.Title
            };
        }
    }
}
