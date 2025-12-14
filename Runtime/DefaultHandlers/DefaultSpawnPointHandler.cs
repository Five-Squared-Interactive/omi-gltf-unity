// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.SpawnPoint
{
    /// <summary>
    /// Default Unity implementation for handling OMI_spawn_point.
    /// Creates OMISpawnPoint components on spawn point nodes.
    /// </summary>
    public class DefaultSpawnPointHandler : ISpawnPointHandler
    {
        public string ExtensionName => OMISpawnPointExtension.ExtensionName;
        public int Priority => 50;

        public Task OnImportAsync(OMISpawnPointNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            // Handled by OnNodeImportAsync
            return Task.CompletedTask;
        }

        public Task<OMISpawnPointNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Handled by OnNodeExportAsync
            return Task.FromResult<OMISpawnPointNode>(null);
        }

        public Task OnNodeImportAsync(OMISpawnPointNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null) return Task.CompletedTask;
            
            CreateSpawnPoint(data, targetObject, context);
            return Task.CompletedTask;
        }

        public Task<OMISpawnPointNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExtractSpawnPoint(sourceObject, context));
        }

        public void CreateSpawnPoint(OMISpawnPointNode data, GameObject targetObject, OMIImportContext context)
        {
            var spawnPoint = targetObject.GetComponent<OMISpawnPoint>();
            if (spawnPoint == null)
            {
                spawnPoint = targetObject.AddComponent<OMISpawnPoint>();
            }

            spawnPoint.Title = data.Title;
            spawnPoint.Team = data.Team;
            spawnPoint.Group = data.Group;

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created spawn point: {data.Title ?? "(unnamed)"} on {targetObject.name}");
            }
        }

        public OMISpawnPointNode ExtractSpawnPoint(GameObject sourceObject, OMIExportContext context)
        {
            if (sourceObject == null) return null;

            var spawnPoint = sourceObject.GetComponent<OMISpawnPoint>();
            if (spawnPoint == null) return null;

            return new OMISpawnPointNode
            {
                Title = string.IsNullOrEmpty(spawnPoint.Title) ? null : spawnPoint.Title,
                Team = string.IsNullOrEmpty(spawnPoint.Team) ? null : spawnPoint.Team,
                Group = string.IsNullOrEmpty(spawnPoint.Group) ? null : spawnPoint.Group
            };
        }
    }
}
