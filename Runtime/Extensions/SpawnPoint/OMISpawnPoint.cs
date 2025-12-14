// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.SpawnPoint
{
    /// <summary>
    /// Unity component representing an OMI spawn point.
    /// Attached to GameObjects that serve as spawn locations.
    /// </summary>
    [AddComponentMenu("OMI/Spawn Point")]
    [DisallowMultipleComponent]
    public class OMISpawnPoint : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Optional display name for this spawn point.")]
        internal string title;

        [SerializeField]
        [Tooltip("Team identifier for team-based spawn points.")]
        internal string team;

        [SerializeField]
        [Tooltip("Group identifier for grouped spawn points.")]
        internal string group;

        /// <summary>
        /// Gets or sets the spawn point title.
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }

        /// <summary>
        /// Gets or sets the team identifier.
        /// </summary>
        public string Team
        {
            get => team;
            set => team = value;
        }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public string Group
        {
            get => group;
            set => group = value;
        }

        /// <summary>
        /// Gets the spawn position in world space.
        /// </summary>
        public Vector3 SpawnPosition => transform.position;

        /// <summary>
        /// Gets the spawn rotation in world space.
        /// </summary>
        public Quaternion SpawnRotation => transform.rotation;

        /// <summary>
        /// Gets the forward direction of the spawn point.
        /// </summary>
        public Vector3 SpawnForward => transform.forward;

        /// <summary>
        /// Converts this component to OMI extension data.
        /// </summary>
        public OMISpawnPointNode ToExtensionData()
        {
            return new OMISpawnPointNode
            {
                Title = string.IsNullOrEmpty(title) ? null : title,
                Team = string.IsNullOrEmpty(team) ? null : team,
                Group = string.IsNullOrEmpty(group) ? null : group
            };
        }

        /// <summary>
        /// Applies extension data to this component.
        /// </summary>
        public void FromExtensionData(OMISpawnPointNode data)
        {
            if (data == null) return;
            title = data.Title;
            team = data.Team;
            group = data.Group;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawGizmo(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmo(true);
        }

        private void DrawGizmo(bool selected)
        {
            var color = selected ? Color.green : new Color(0.3f, 0.8f, 0.3f, 0.6f);
            Gizmos.color = color;
            
            // Draw position marker
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            
            // Draw direction arrow
            Gizmos.DrawRay(transform.position, transform.forward * 1.0f);
            
            // Draw arrowhead
            var arrowEnd = transform.position + transform.forward * 1.0f;
            var right = transform.right;
            Gizmos.DrawLine(arrowEnd, arrowEnd - transform.forward * 0.2f + right * 0.1f);
            Gizmos.DrawLine(arrowEnd, arrowEnd - transform.forward * 0.2f - right * 0.1f);
            
            // Draw up indicator
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);
        }
#endif
    }
}
