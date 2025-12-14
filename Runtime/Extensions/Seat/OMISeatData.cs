// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using Newtonsoft.Json;

namespace OMI.Extensions.Seat
{
    /// <summary>
    /// Extension name constant for OMI_seat.
    /// </summary>
    public static class OMISeatExtension
    {
        public const string ExtensionName = "OMI_seat";
    }

    /// <summary>
    /// Node-level OMI_seat extension data.
    /// Defines a seat for humanoid characters with IK positioning data.
    /// </summary>
    [Serializable]
    public class OMISeatNode
    {
        /// <summary>
        /// Position limit for the character's back/hips in local space [x, y, z].
        /// Required.
        /// </summary>
        [JsonProperty("back")]
        public float[] Back;

        /// <summary>
        /// Position limit for the character's feet in local space [x, y, z].
        /// Required.
        /// </summary>
        [JsonProperty("foot")]
        public float[] Foot;

        /// <summary>
        /// Base position for the character's knees in local space [x, y, z].
        /// Required.
        /// </summary>
        [JsonProperty("knee")]
        public float[] Knee;

        /// <summary>
        /// The angle between the spine and back-knee line in radians.
        /// Default: PI/2 (90 degrees)
        /// </summary>
        [JsonProperty("angle")]
        public float Angle = 1.5707963267948966f; // PI/2
    }
}
