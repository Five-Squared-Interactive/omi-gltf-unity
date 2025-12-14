// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace OMI.Extensions.Personality
{
    /// <summary>
    /// Component representing an OMI_personality agent/NPC.
    /// Attach to a GameObject to give it AI personality data.
    /// </summary>
    [AddComponentMenu("OMI/Personality")]
    public class OMIPersonality : MonoBehaviour
    {
        [Header("Agent Settings")]
        [Tooltip("The name of this agent/NPC.")]
        public string Agent;

        [TextArea(3, 10)]
        [Tooltip("Description of the agent's personality for AI context.")]
        public string Personality;

        [TextArea(2, 5)]
        [Tooltip("Default message when initializing conversation.")]
        public string DefaultMessage;

        [Header("Events")]
        [Tooltip("Called when a conversation is started with this agent.")]
        public UnityEvent<string> OnConversationStarted;

        [Tooltip("Called when a message is received for this agent.")]
        public UnityEvent<string, string> OnMessageReceived; // speaker, message

        [Tooltip("Called when this agent sends a response.")]
        public UnityEvent<string> OnResponseSent;

        /// <summary>
        /// Formats the personality prompt with the given replacements.
        /// Supports: #agent, #speaker, #input, #conversation
        /// </summary>
        /// <param name="speaker">The name of the speaker.</param>
        /// <param name="input">The user's input message.</param>
        /// <param name="conversation">The conversation history.</param>
        /// <returns>Formatted prompt string.</returns>
        public string FormatPrompt(string speaker = null, string input = null, string conversation = null)
        {
            if (string.IsNullOrEmpty(Personality))
            {
                return string.Empty;
            }

            string prompt = Personality;
            
            if (!string.IsNullOrEmpty(Agent))
                prompt = prompt.Replace("#agent", Agent);
            
            if (!string.IsNullOrEmpty(speaker))
                prompt = prompt.Replace("#speaker", speaker);
            
            if (!string.IsNullOrEmpty(input))
                prompt = prompt.Replace("#input", input);
            
            if (!string.IsNullOrEmpty(conversation))
                prompt = prompt.Replace("#conversation", conversation);

            return prompt;
        }

        /// <summary>
        /// Starts a conversation with this agent.
        /// </summary>
        public void StartConversation()
        {
            OnConversationStarted?.Invoke(DefaultMessage ?? string.Empty);
        }

        /// <summary>
        /// Sends a message to this agent.
        /// </summary>
        /// <param name="speaker">The name of the speaker.</param>
        /// <param name="message">The message content.</param>
        public void SendMessage(string speaker, string message)
        {
            OnMessageReceived?.Invoke(speaker, message);
        }

        /// <summary>
        /// Called when the agent responds (typically by an AI system).
        /// </summary>
        /// <param name="response">The response message.</param>
        public void Respond(string response)
        {
            OnResponseSent?.Invoke(response);
        }

        /// <summary>
        /// Gets a data object representing this personality for serialization.
        /// </summary>
        public OMIPersonalityNode ToData()
        {
            return new OMIPersonalityNode
            {
                Agent = Agent,
                Personality = Personality,
                DefaultMessage = string.IsNullOrEmpty(DefaultMessage) ? null : DefaultMessage
            };
        }

        /// <summary>
        /// Applies data from an OMIPersonalityNode to this component.
        /// </summary>
        public void FromData(OMIPersonalityNode data)
        {
            if (data == null) return;
            
            Agent = data.Agent;
            Personality = data.Personality;
            DefaultMessage = data.DefaultMessage;
        }
    }
}
