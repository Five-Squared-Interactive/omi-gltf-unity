// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace OMI.Extensions.Link
{
    /// <summary>
    /// Unity component representing an OMI link.
    /// Attached to GameObjects that define navigation/teleportation targets.
    /// </summary>
    [AddComponentMenu("OMI/Link")]
    [DisallowMultipleComponent]
    public class OMILink : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The URI to link to. Can be a URL, relative path, or fragment reference.")]
        internal string uri;

        [SerializeField]
        [Tooltip("Optional display title for the link.")]
        internal string title;

        [Header("Events")]
        [Tooltip("Called when the link is activated.")]
        public UnityEvent<OMILink> OnLinkActivated;

        /// <summary>
        /// Gets or sets the link URI.
        /// </summary>
        public string Uri
        {
            get => uri;
            set => uri = value;
        }

        /// <summary>
        /// Gets or sets the link title.
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }

        /// <summary>
        /// Gets the display name (title if available, otherwise the URI).
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(title) ? uri : title;

        /// <summary>
        /// Whether this link points to an external URL.
        /// </summary>
        public bool IsExternalUrl => !string.IsNullOrEmpty(uri) && 
            (uri.StartsWith("http://") || uri.StartsWith("https://"));

        /// <summary>
        /// Whether this link is a relative path.
        /// </summary>
        public bool IsRelativePath => !string.IsNullOrEmpty(uri) && 
            (uri.StartsWith("./") || uri.StartsWith("../"));

        /// <summary>
        /// Whether this link is a fragment reference (internal node reference).
        /// </summary>
        public bool IsFragment => !string.IsNullOrEmpty(uri) && uri.StartsWith("#");

        /// <summary>
        /// Gets the fragment identifier if this is a fragment link.
        /// </summary>
        public string FragmentId
        {
            get
            {
                if (string.IsNullOrEmpty(uri)) return null;
                var hashIndex = uri.LastIndexOf('#');
                return hashIndex >= 0 ? uri.Substring(hashIndex + 1) : null;
            }
        }

        /// <summary>
        /// Activates the link. Override or subscribe to OnLinkActivated for custom behavior.
        /// </summary>
        public virtual void Activate()
        {
            OnLinkActivated?.Invoke(this);
            
            // Default behavior: open external URLs
            if (IsExternalUrl)
            {
                Application.OpenURL(uri);
            }
        }

        /// <summary>
        /// Converts this component to OMI extension data.
        /// </summary>
        public OMILinkNode ToExtensionData()
        {
            return new OMILinkNode
            {
                Uri = uri,
                Title = string.IsNullOrEmpty(title) ? null : title
            };
        }

        /// <summary>
        /// Applies extension data to this component.
        /// </summary>
        public void FromExtensionData(OMILinkNode data)
        {
            if (data == null) return;
            uri = data.Uri;
            title = data.Title;
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
            Gizmos.color = selected ? Color.cyan : new Color(0.3f, 0.6f, 0.9f, 0.6f);
            
            var position = transform.position;
            var size = 0.15f;
            
            // Draw link icon (two connected rings)
            Gizmos.DrawWireSphere(position + Vector3.up * size * 0.4f, size * 0.5f);
            Gizmos.DrawWireSphere(position - Vector3.up * size * 0.4f, size * 0.5f);
            
            // Draw connection
            Gizmos.DrawLine(
                position + Vector3.up * size * 0.1f, 
                position - Vector3.up * size * 0.1f);
        }
#endif
    }
}
