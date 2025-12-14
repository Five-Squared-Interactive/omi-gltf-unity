// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI.Extensions.Seat
{
    /// <summary>
    /// Default Unity implementation for handling OMI_seat.
    /// Creates OMISeat components on seat nodes.
    /// </summary>
    public class DefaultSeatHandler : ISeatHandler
    {
        public string ExtensionName => OMISeatExtension.ExtensionName;
        public int Priority => 50;

        public Task OnImportAsync(OMISeatNode data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<OMISeatNode> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<OMISeatNode>(null);
        }

        public Task OnNodeImportAsync(OMISeatNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null) return Task.CompletedTask;
            
            CreateSeat(data, targetObject, context);
            return Task.CompletedTask;
        }

        public Task<OMISeatNode> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExtractSeat(sourceObject, context));
        }

        public void CreateSeat(OMISeatNode data, GameObject targetObject, OMIImportContext context)
        {
            var seat = targetObject.GetComponent<OMISeat>();
            if (seat == null)
            {
                seat = targetObject.AddComponent<OMISeat>();
            }

            // Convert from glTF arrays to Unity vectors
            if (data.Back != null && data.Back.Length >= 3)
            {
                seat.BackPosition = new Vector3(data.Back[0], data.Back[1], -data.Back[2]);
            }

            if (data.Foot != null && data.Foot.Length >= 3)
            {
                seat.FootPosition = new Vector3(data.Foot[0], data.Foot[1], -data.Foot[2]);
            }

            if (data.Knee != null && data.Knee.Length >= 3)
            {
                seat.KneePosition = new Vector3(data.Knee[0], data.Knee[1], -data.Knee[2]);
            }

            seat.Angle = data.Angle * Mathf.Rad2Deg; // Convert to degrees for Unity

            if (context.Settings.VerboseLogging)
            {
                Debug.Log($"[OMI] Created seat on {targetObject.name}");
            }
        }

        public OMISeatNode ExtractSeat(GameObject sourceObject, OMIExportContext context)
        {
            if (sourceObject == null) return null;

            var seat = sourceObject.GetComponent<OMISeat>();
            if (seat == null) return null;

            return new OMISeatNode
            {
                Back = new float[] { seat.BackPosition.x, seat.BackPosition.y, -seat.BackPosition.z },
                Foot = new float[] { seat.FootPosition.x, seat.FootPosition.y, -seat.FootPosition.z },
                Knee = new float[] { seat.KneePosition.x, seat.KneePosition.y, -seat.KneePosition.z },
                Angle = seat.Angle * Mathf.Deg2Rad // Convert back to radians
            };
        }
    }
}
