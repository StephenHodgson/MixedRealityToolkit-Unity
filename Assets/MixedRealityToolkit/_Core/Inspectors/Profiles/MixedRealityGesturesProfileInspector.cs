﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Core.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Managers;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Inspectors.Profiles
{
    [CustomEditor(typeof(MixedRealityGesturesProfile))]
    public class MixedRealityGesturesProfileInspector : MixedRealityBaseConfigurationProfileInspector
    {
        private static readonly GUIContent MinusButtonContent = new GUIContent("-", "Remove defined Gesture");
        private static readonly GUIContent AddButtonContent = new GUIContent("+ Add a New defined Gesture");
        private static readonly GUIContent DescriptionContent = new GUIContent("Description", "The human readable description of the Gesture.");
        private static readonly GUIContent GestureTypeContent = new GUIContent("Gesture Type", "The type of Gesture that will trigger the action.");
        private static readonly GUIContent ActionContent = new GUIContent("Action", "The action to trigger when a Gesture is recognized.");

        private static GUIContent[] actionLabels;
        private static int[] actionIds;
        private static int screenWidth;

        private SerializedProperty gestures;

        private void OnEnable()
        {
            if (!CheckMixedRealityManager(false)) { return; }

            gestures = serializedObject.FindProperty("gestures");

            if (MixedRealityManager.Instance.ActiveProfile.IsInputSystemEnabled &&
                MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile != null)
            {
                actionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions.Select(action => new GUIContent(action.Description)).Prepend(new GUIContent("None")).ToArray();
                actionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions.Select(action => (int)action.Id).Prepend(0).ToArray();
            }
        }

        public override void OnInspectorGUI()
        {
            RenderMixedRealityToolkitLogo();

            if (!CheckMixedRealityManager()) { return; }

            if (!MixedRealityManager.Instance.ActiveProfile.IsInputSystemEnabled)
            {
                EditorGUILayout.HelpBox("No input system is enabled, or you need to specify the type in the main configuration profile.", MessageType.Error);

                if (GUILayout.Button("Back to Configuration Profile"))
                {
                    Selection.activeObject = MixedRealityManager.Instance.ActiveProfile;
                }

                return;
            }

            if (GUILayout.Button("Back to Input Profile"))
            {
                Selection.activeObject = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gesture Input", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This gesture map is any and all movements of part the user's body, especially a hand or the head, that raise actions through the input system.\n\nNote: Defined controllers can look up the list of gestures and raise the events based on specific criteria.", MessageType.Info);

            if (MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile == null)
            {
                EditorGUILayout.HelpBox("No input actions found, please specify a input action profile in the main configuration.", MessageType.Error);
                return;
            }

            serializedObject.Update();
            RenderList(gestures);
            serializedObject.ApplyModifiedProperties();
        }

        private static void RenderList(SerializedProperty list)
        {
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            if (GUILayout.Button(AddButtonContent, EditorStyles.miniButton))
            {
                list.arraySize += 1;
                var speechCommand = list.GetArrayElementAtIndex(list.arraySize - 1);
                var keyword = speechCommand.FindPropertyRelative("description");
                keyword.stringValue = string.Empty;
                var keyCode = speechCommand.FindPropertyRelative("gestureType");
                keyCode.intValue = (int)KeyCode.None;
                var action = speechCommand.FindPropertyRelative("action");
                var actionId = action.FindPropertyRelative("id");
                actionId.intValue = 0;
                var actionDescription = action.FindPropertyRelative("description");
                actionDescription.stringValue = string.Empty;
                var actionConstraint = action.FindPropertyRelative("axisConstraint");
                actionConstraint.intValue = 0;
            }

            GUILayout.Space(12f);

            if (list == null || list.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Define a new Gesture.", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 24f;
            EditorGUILayout.LabelField(DescriptionContent, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(GestureTypeContent, GUILayout.Width(80f));
            EditorGUILayout.LabelField(ActionContent, GUILayout.Width(64f));
            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(24f));
            EditorGUIUtility.labelWidth = labelWidth;
            GUILayout.EndHorizontal();

            for (int i = 0; i < list.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty gesture = list.GetArrayElementAtIndex(i);
                var keyword = gesture.FindPropertyRelative("description");
                EditorGUILayout.PropertyField(keyword, GUIContent.none, GUILayout.ExpandWidth(true));
                var gestureType = gesture.FindPropertyRelative("gestureType");
                EditorGUILayout.PropertyField(gestureType, GUIContent.none, GUILayout.Width(80f));
                var action = gesture.FindPropertyRelative("action");
                var actionId = action.FindPropertyRelative("id");
                var actionDescription = action.FindPropertyRelative("description");
                var actionConstraint = action.FindPropertyRelative("axisConstraint");

                EditorGUI.BeginChangeCheck();
                actionId.intValue = EditorGUILayout.IntPopup(GUIContent.none, actionId.intValue, actionLabels, actionIds, GUILayout.Width(64f));

                if (EditorGUI.EndChangeCheck())
                {
                    MixedRealityInputAction inputAction = actionId.intValue == 0 ? MixedRealityInputAction.None : MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions[actionId.intValue - 1];
                    actionDescription.stringValue = inputAction.Description;
                    actionConstraint.enumValueIndex = (int)inputAction.AxisConstraint;
                }

                if (GUILayout.Button(MinusButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                {
                    list.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
    }
}