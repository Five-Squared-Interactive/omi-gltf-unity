// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace OMI.Extensions.Seat
{
    /// <summary>
    /// Unity component representing an OMI seat.
    /// Attached to GameObjects that define seating positions for characters.
    /// </summary>
    [AddComponentMenu("OMI/Seat")]
    [DisallowMultipleComponent]
    public class OMISeat : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Position limit for the character's back/hips in local space.")]
        internal Vector3 back;

        [SerializeField]
        [Tooltip("Position limit for the character's feet in local space.")]
        internal Vector3 foot;

        [SerializeField]
        [Tooltip("Base position for the character's knees in local space.")]
        internal Vector3 knee;

        [SerializeField]
        [Tooltip("The angle between the spine and back-knee line in degrees.")]
        internal float angle = 90f;

        [SerializeField]
        internal bool hasBack;

        [SerializeField]
        internal bool hasFoot;

        [SerializeField]
        internal bool hasKnee;

        /// <summary>
        /// Gets or sets the back/hip position in local space.
        /// </summary>
        public Vector3 Back
        {
            get => back;
            set
            {
                back = value;
                hasBack = true;
            }
        }

        /// <summary>
        /// Gets or sets the back position. Alias for Back.
        /// </summary>
        public Vector3 BackPosition
        {
            get => back;
            set => Back = value;
        }

        /// <summary>
        /// Gets or sets the foot position in local space.
        /// </summary>
        public Vector3 Foot
        {
            get => foot;
            set
            {
                foot = value;
                hasFoot = true;
            }
        }

        /// <summary>
        /// Gets or sets the foot position. Alias for Foot.
        /// </summary>
        public Vector3 FootPosition
        {
            get => foot;
            set => Foot = value;
        }

        /// <summary>
        /// Gets or sets the knee position in local space.
        /// </summary>
        public Vector3 Knee
        {
            get => knee;
            set
            {
                knee = value;
                hasKnee = true;
            }
        }

        /// <summary>
        /// Gets or sets the knee position. Alias for Knee.
        /// </summary>
        public Vector3 KneePosition
        {
            get => knee;
            set => Knee = value;
        }

        /// <summary>
        /// Gets or sets the angle in degrees.
        /// </summary>
        public float Angle
        {
            get => angle;
            set => angle = value;
        }

        /// <summary>
        /// Gets or sets the angle in radians.
        /// </summary>
        public float AngleRadians
        {
            get => angle * Mathf.Deg2Rad;
            set => angle = value * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Whether the back position is set.
        /// </summary>
        public bool HasBack => hasBack;

        /// <summary>
        /// Whether the foot position is set.
        /// </summary>
        public bool HasFoot => hasFoot;

        /// <summary>
        /// Whether the knee position is set.
        /// </summary>
        public bool HasKnee => hasKnee;

        /// <summary>
        /// Gets the back position in world space.
        /// </summary>
        public Vector3 BackWorldPosition => transform.TransformPoint(back);

        /// <summary>
        /// Gets the foot position in world space.
        /// </summary>
        public Vector3 FootWorldPosition => transform.TransformPoint(foot);

        /// <summary>
        /// Gets the knee position in world space.
        /// </summary>
        public Vector3 KneeWorldPosition => transform.TransformPoint(knee);

        /// <summary>
        /// Converts this component to OMI extension data.
        /// </summary>
        public OMISeatNode ToExtensionData()
        {
            // Convert from Unity's left-handed to glTF's right-handed coordinate system
            return new OMISeatNode
            {
                Back = hasBack ? new[] { back.x, back.y, -back.z } : null,
                Foot = hasFoot ? new[] { foot.x, foot.y, -foot.z } : null,
                Knee = hasKnee ? new[] { knee.x, knee.y, -knee.z } : null,
                Angle = angle * Mathf.Deg2Rad
            };
        }

        /// <summary>
        /// Applies extension data to this component.
        /// </summary>
        public void FromExtensionData(OMISeatNode data)
        {
            if (data == null) return;

            // Convert from glTF's right-handed to Unity's left-handed coordinate system
            if (data.Back != null && data.Back.Length >= 3)
            {
                back = new Vector3(data.Back[0], data.Back[1], -data.Back[2]);
                hasBack = true;
            }

            if (data.Foot != null && data.Foot.Length >= 3)
            {
                foot = new Vector3(data.Foot[0], data.Foot[1], -data.Foot[2]);
                hasFoot = true;
            }

            if (data.Knee != null && data.Knee.Length >= 3)
            {
                knee = new Vector3(data.Knee[0], data.Knee[1], -data.Knee[2]);
                hasKnee = true;
            }

            angle = data.Angle * Mathf.Rad2Deg;
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
            // Draw seat position
            Gizmos.color = selected ? Color.yellow : new Color(0.8f, 0.8f, 0.3f, 0.6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.4f, 0.1f, 0.4f));
            Gizmos.matrix = Matrix4x4.identity;

            // Draw reference positions
            if (hasBack)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(BackWorldPosition, 0.08f);
                Gizmos.DrawLine(transform.position, BackWorldPosition);
            }

            if (hasFoot)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(FootWorldPosition, 0.08f);
            }

            if (hasKnee)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(KneeWorldPosition, 0.08f);
                
                // Draw leg lines
                if (hasFoot)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                    Gizmos.DrawLine(KneeWorldPosition, FootWorldPosition);
                }
                if (hasBack)
                {
                    Gizmos.DrawLine(BackWorldPosition, KneeWorldPosition);
                }
            }
        }
#endif
    }
}
