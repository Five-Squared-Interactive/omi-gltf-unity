// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.SpawnPoint
{
    /// <summary>
    /// Extension name constant for OMI_spawn_point.
    /// </summary>
    public static class OMISpawnPointExtension
    {
        public const string ExtensionName = "OMI_spawn_point";
    }

    /// <summary>
    /// Node-level OMI_spawn_point extension data.
    /// Defines a spawn point where spawnable objects can be created.
    /// </summary>
    [Serializable]
    public class OMISpawnPointNode
    {
        /// <summary>
        /// The title of the spawn point.
        /// Max length: 128 characters.
        /// </summary>
        [JsonProperty("title")]
        public string Title;

        /// <summary>
        /// The team that this spawn point belongs to, if any.
        /// Max length: 128 characters.
        /// </summary>
        [JsonProperty("team")]
        public string Team;

        /// <summary>
        /// The group that this spawn point belongs to, if any.
        /// Max length: 128 characters.
        /// </summary>
        [JsonProperty("group")]
        public string Group;
    }
}
