// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.Personality
{
    /// <summary>
    /// Extension name constant for OMI_personality.
    /// </summary>
    public static class OMIPersonalityExtension
    {
        public const string ExtensionName = "OMI_personality";
    }

    /// <summary>
    /// Node-level OMI_personality extension data.
    /// Defines personality information for AI-driven characters/NPCs.
    /// </summary>
    [Serializable]
    public class OMIPersonalityNode
    {
        /// <summary>
        /// The name of the agent or NPC.
        /// Required. Max length: 128 characters.
        /// </summary>
        [JsonProperty("agent")]
        public string Agent;

        /// <summary>
        /// A description of the agent's personality.
        /// This can be injected into language model context.
        /// Required.
        /// </summary>
        [JsonProperty("personality")]
        public string Personality;

        /// <summary>
        /// A default message for this agent to initialize with.
        /// Optional.
        /// </summary>
        [JsonProperty("defaultMessage")]
        public string DefaultMessage;
    }
}
