// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.Link
{
    /// <summary>
    /// Extension name constant for OMI_link.
    /// </summary>
    public static class OMILinkExtension
    {
        public const string ExtensionName = "OMI_link";
    }

    /// <summary>
    /// Node-level OMI_link extension data.
    /// Defines a link/portal for world traversal.
    /// </summary>
    [Serializable]
    public class OMILinkNode
    {
        /// <summary>
        /// The URI to link to. Can be:
        /// - Absolute URL: "https://example.com/worlds/world1"
        /// - Relative path: "./room1"
        /// - Fragment: "#spawn-point-1"
        /// Required.
        /// </summary>
        [JsonProperty("uri")]
        public string Uri;

        /// <summary>
        /// Optional title describing the destination.
        /// </summary>
        [JsonProperty("title")]
        public string Title;
    }
}
