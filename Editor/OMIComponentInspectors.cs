// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace OMI.Editor
{
    /// <summary>
    /// Custom inspector for OMISpawnPoint component.
    /// </summary>
    [CustomEditor(typeof(Extensions.SpawnPoint.OMISpawnPoint))]
    public class OMISpawnPointInspector : UnityEditor.Editor
    {
        private SerializedProperty _titleProp;
        private SerializedProperty _teamProp;
        private SerializedProperty _groupProp;

        private void OnEnable()
        {
            _titleProp = serializedObject.FindProperty("title");
            _teamProp = serializedObject.FindProperty("team");
            _groupProp = serializedObject.FindProperty("group");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("OMI Spawn Point", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Defines a spawn point for players or objects.\n" +
                "The local transform defines the spawn position and orientation.",
                MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_titleProp, new GUIContent("Title", "Optional display name for this spawn point."));
            EditorGUILayout.PropertyField(_teamProp, new GUIContent("Team", "Team identifier for team-based spawn points."));
            EditorGUILayout.PropertyField(_groupProp, new GUIContent("Group", "Group identifier for grouped spawn points."));

            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Custom inspector for OMISeat component.
    /// </summary>
    [CustomEditor(typeof(Extensions.Seat.OMISeat))]
    public class OMISeatInspector : UnityEditor.Editor
    {
        private SerializedProperty _backProp;
        private SerializedProperty _footProp;
        private SerializedProperty _kneeProp;
        private SerializedProperty _angleProp;
        private SerializedProperty _hasBackProp;
        private SerializedProperty _hasFootProp;
        private SerializedProperty _hasKneeProp;

        private void OnEnable()
        {
            _backProp = serializedObject.FindProperty("back");
            _footProp = serializedObject.FindProperty("foot");
            _kneeProp = serializedObject.FindProperty("knee");
            _angleProp = serializedObject.FindProperty("angle");
            _hasBackProp = serializedObject.FindProperty("hasBack");
            _hasFootProp = serializedObject.FindProperty("hasFoot");
            _hasKneeProp = serializedObject.FindProperty("hasKnee");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("OMI Seat", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Defines a seat for characters.\n" +
                "Positions are relative to the seat's transform.",
                MessageType.Info);

            EditorGUILayout.Space();

            // Back position
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_hasBackProp, GUIContent.none, GUILayout.Width(20));
            EditorGUI.BeginDisabledGroup(!_hasBackProp.boolValue);
            EditorGUILayout.PropertyField(_backProp, new GUIContent("Back Position", "Position of the character's back when seated."));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Foot position
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_hasFootProp, GUIContent.none, GUILayout.Width(20));
            EditorGUI.BeginDisabledGroup(!_hasFootProp.boolValue);
            EditorGUILayout.PropertyField(_footProp, new GUIContent("Foot Position", "Position of the character's feet when seated."));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Knee position
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_hasKneeProp, GUIContent.none, GUILayout.Width(20));
            EditorGUI.BeginDisabledGroup(!_hasKneeProp.boolValue);
            EditorGUILayout.PropertyField(_kneeProp, new GUIContent("Knee Position", "Position of the character's knees when seated."));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_angleProp, new GUIContent("Angle (degrees)", "Rotation angle for the seated character."));

            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Custom inspector for OMILink component.
    /// </summary>
    [CustomEditor(typeof(Extensions.Link.OMILink))]
    public class OMILinkInspector : UnityEditor.Editor
    {
        private SerializedProperty _uriProp;
        private SerializedProperty _titleProp;
        private SerializedProperty _onLinkActivatedProp;

        private void OnEnable()
        {
            _uriProp = serializedObject.FindProperty("uri");
            _titleProp = serializedObject.FindProperty("title");
            _onLinkActivatedProp = serializedObject.FindProperty("OnLinkActivated");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("OMI Link", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Defines a hyperlink or navigation target.\n" +
                "Can link to URLs, other glTF files, or internal node references.",
                MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_uriProp, new GUIContent("URI", "The link target URL or reference."));
            EditorGUILayout.PropertyField(_titleProp, new GUIContent("Title", "Optional display title for the link."));

            var link = target as Extensions.Link.OMILink;
            if (link != null && !string.IsNullOrEmpty(link.Uri))
            {
                EditorGUILayout.Space();
                
                // Show link type info
                if (link.IsExternalUrl)
                {
                    EditorGUILayout.HelpBox("External URL link", MessageType.None);
                    if (GUILayout.Button("Open URL"))
                    {
                        Application.OpenURL(link.Uri);
                    }
                }
                else if (link.IsFragment)
                {
                    EditorGUILayout.HelpBox($"Fragment reference: {link.FragmentId}", MessageType.None);
                }
                else if (link.IsRelativePath)
                {
                    EditorGUILayout.HelpBox("Relative path link", MessageType.None);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_onLinkActivatedProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
