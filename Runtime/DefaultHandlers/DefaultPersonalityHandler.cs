// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.Personality
{
    /// <summary>
    /// Default Unity implementation for handling OMI_personality.
    /// Creates OMIPersonality components on personality nodes.
    /// </summary>
    public class DefaultPersonalityHandler : IPersonalityHandler
    {
        public string ExtensionName => OMIPersonalityExtension.ExtensionName;
        public int Priority => 50;

        public Task OnImportAsync(OMIPersonalityNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<OMIPersonalityNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<OMIPersonalityNode>(null);
        }

        public Task OnNodeImportAsync(OMIPersonalityNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null) return Task.CompletedTask;

            CreatePersonality(data, targetObject, context);
            return Task.CompletedTask;
        }

        public Task<OMIPersonalityNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExtractPersonality(sourceObject, context));
        }

        public void CreatePersonality(OMIPersonalityNode data, GameObject targetObject, OMIImportContext context)
        {
            var personality = targetObject.GetComponent<OMIPersonality>();
            if (personality == null)
            {
                personality = targetObject.AddComponent<OMIPersonality>();
            }

            personality.Agent = data.Agent;
            personality.Personality = data.Personality;
            personality.DefaultMessage = data.DefaultMessage;

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created personality for agent '{data.Agent}' on {targetObject.name}");
            }
        }

        public OMIPersonalityNode ExtractPersonality(GameObject sourceObject, OMIExportContext context)
        {
            if (sourceObject == null) return null;

            var personality = sourceObject.GetComponent<OMIPersonality>();
            if (personality == null) return null;

            // Agent and Personality are required
            if (string.IsNullOrEmpty(personality.Agent) || string.IsNullOrEmpty(personality.Personality))
            {
                return null;
            }

            return new OMIPersonalityNode
            {
                Agent = personality.Agent,
                Personality = personality.Personality,
                DefaultMessage = string.IsNullOrEmpty(personality.DefaultMessage) ? null : personality.DefaultMessage
            };
        }
    }
}
