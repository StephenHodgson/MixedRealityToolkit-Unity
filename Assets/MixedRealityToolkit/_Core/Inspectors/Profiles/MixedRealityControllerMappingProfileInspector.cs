﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Devices.OpenVR;
using Microsoft.MixedReality.Toolkit.Internal.Managers;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Inspectors
{
    [CustomEditor(typeof(MixedRealityControllerMappingProfile))]
    public class MixedRealityControllerMappingProfileInspector : MixedRealityBaseConfigurationProfileInspector
    {
        private static readonly GUIContent ControllerAddButtonContent = new GUIContent("+ Add a New Controller Template");
        private static readonly GUIContent ControllerMinusButtonContent = new GUIContent("-", "Remove Controller Template");
        private static readonly GUIContent InteractionAddButtonContent = new GUIContent("+ Add a New Interaction Mapping");
        private static readonly GUIContent InteractionMinusButtonContent = new GUIContent("-", "Remove Interaction Mapping");
        private static readonly GUIContent InteractionContent = new GUIContent("Interaction Mappings");
        private static readonly GUIContent InputDescription = new GUIContent("Description", "The input description");
        private static readonly GUIContent AxisTypeContent = new GUIContent("Axis Type", "The axis type of the button, e.g. Analogue, Digital, etc.");
        private static readonly GUIContent ControllerInputTypeContent = new GUIContent("Input Type", "The primary action of the input as defined by the controller SDK.");
        private static readonly GUIContent ActionContent = new GUIContent("Action", "Action to be raised to the Input Manager when the input data has changed.");
        private static readonly GUIContent KeyCodeContent = new GUIContent("KeyCode", "Unity Input KeyCode id to listen for.");
        private static readonly GUIContent XAxisContent = new GUIContent("X Axis", "Horizontal Axis to listen for.");
        private static readonly GUIContent YAxisContent = new GUIContent("Y Axis", "Vertical Axis to listen for.");

        private static MixedRealityControllerMappingProfile thisProfile;

        private static bool[] controllerFoldouts;
        private static bool[] controllerInteractionFoldouts;

        private static int[] actionIds;
        private static GUIContent[] actionLabels;

        private static int axisId;
        private static int[] axisIds;
        private static GUIContent[] axisLabels;

        private static float defaultLabelWidth;
        private static float defaultFieldWidth;

        private SerializedProperty mixedRealityControllerMappingProfiles;
        private SerializedProperty renderMotionControllers;
        private SerializedProperty useDefaultModels;
        private SerializedProperty globalLeftHandModel;
        private SerializedProperty globalRightHandModel;

        private void OnEnable()
        {
            if (!CheckMixedRealityManager(false))
            {
                return;
            }

            mixedRealityControllerMappingProfiles = serializedObject.FindProperty("mixedRealityControllerMappingProfiles");

            if (controllerInteractionFoldouts == null || controllerInteractionFoldouts.Length != mixedRealityControllerMappingProfiles.arraySize)
            {
                controllerInteractionFoldouts = new bool[mixedRealityControllerMappingProfiles.arraySize];

                // Open all the interaction foldouts by default.
                for (int i = 0; i < controllerInteractionFoldouts.Length; i++)
                {
                    controllerInteractionFoldouts[i] = true;
                }
            }

            if (controllerFoldouts == null || controllerFoldouts.Length != mixedRealityControllerMappingProfiles.arraySize)
            {
                controllerFoldouts = new bool[mixedRealityControllerMappingProfiles.arraySize];
            }

            actionLabels = MixedRealityManager.Instance.ActiveProfile.InputActionsProfile.InputActions.Select(action => new GUIContent(action.Description)).Prepend(new GUIContent("None")).ToArray();
            actionIds = MixedRealityManager.Instance.ActiveProfile.InputActionsProfile.InputActions.Select(action => (int)action.Id).Prepend(0).ToArray();

            axisLabels = ControllerMappingLibrary.UnityInputManagerAxes.Select(axis => new GUIContent(axis.Name)).Prepend(new GUIContent("None")).ToArray();
            axisIds = new int[axisLabels.Length];

            for (int i = 1; i < axisIds.Length; i++)
            {
                axisIds[i] = i;
            }

            renderMotionControllers = serializedObject.FindProperty("renderMotionControllers");
            useDefaultModels = serializedObject.FindProperty("useDefaultModels");
            globalLeftHandModel = serializedObject.FindProperty("globalLeftHandModel");
            globalRightHandModel = serializedObject.FindProperty("globalRightHandModel");

            defaultLabelWidth = EditorGUIUtility.labelWidth;
            defaultFieldWidth = EditorGUIUtility.fieldWidth;

            thisProfile = target as MixedRealityControllerMappingProfile;
        }

        public override void OnInspectorGUI()
        {
            RenderMixedRealityToolkitLogo();
            EditorGUILayout.LabelField("Controller Templates", EditorStyles.boldLabel);

            if (!CheckMixedRealityManager())
            {
                return;
            }

            EditorGUILayout.HelpBox("Controller templates define all the controllers your users will be able to use in your application.\n\n" +
                                    "After defining all your Input Actions, you can then wire them up to hardware sensors, controllers, and other input devices.", MessageType.Info);

            serializedObject.Update();

            EditorGUILayout.PropertyField(renderMotionControllers);

            if (renderMotionControllers.boolValue)
            {
                EditorGUILayout.PropertyField(useDefaultModels);

                if (!useDefaultModels.boolValue)
                {
                    EditorGUILayout.PropertyField(globalLeftHandModel);
                    EditorGUILayout.PropertyField(globalRightHandModel);
                }
            }

            RenderControllerProfilesList(mixedRealityControllerMappingProfiles);

            serializedObject.ApplyModifiedProperties();
        }

        private void RenderControllerProfilesList(SerializedProperty list)
        {
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            if (GUILayout.Button(ControllerAddButtonContent, EditorStyles.miniButton))
            {
                list.InsertArrayElementAtIndex(list.arraySize);
                var mixedRealityControllerMapping = list.GetArrayElementAtIndex(list.arraySize - 1);
                var mixedRealityControllerMappingId = mixedRealityControllerMapping.FindPropertyRelative("id");
                var mixedRealityControllerMappingDescription = mixedRealityControllerMapping.FindPropertyRelative("description");
                mixedRealityControllerMappingDescription.stringValue = $"New Controller Template {mixedRealityControllerMappingId.intValue = list.arraySize}";
                var mixedRealityControllerHandedness = mixedRealityControllerMapping.FindPropertyRelative("handedness");
                mixedRealityControllerHandedness.intValue = 0;
                var mixedRealityControllerInteractions = mixedRealityControllerMapping.FindPropertyRelative("interactions");
                mixedRealityControllerInteractions.ClearArray();
                serializedObject.ApplyModifiedProperties();

                thisProfile.MixedRealityControllerMappingProfiles[list.arraySize - 1].ControllerType.Type = null;

                controllerInteractionFoldouts = new bool[list.arraySize];
                controllerFoldouts = new bool[list.arraySize];

                for (int i = 0; i < controllerInteractionFoldouts?.Length; i++)
                {
                    controllerInteractionFoldouts[i] = true;
                }

                return;
            }

            GUILayout.Space(12f);
            GUILayout.BeginVertical();

            if (list == null || list.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Create a new Controller Template.", MessageType.Warning);
            }

            for (int i = 0; i < list?.arraySize; i++)
            {
                GUILayout.BeginVertical();

                var previousLabelWidth = EditorGUIUtility.labelWidth;
                var mixedRealityControllerMapping = list.GetArrayElementAtIndex(i);
                var mixedRealityControllerMappingDescription = mixedRealityControllerMapping.FindPropertyRelative("description");

                controllerFoldouts[i] = EditorGUILayout.Foldout(controllerFoldouts[i], $"{mixedRealityControllerMappingDescription.stringValue}", true);

                if (controllerFoldouts[i])
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 96f;
                    EditorGUILayout.PropertyField(mixedRealityControllerMappingDescription);
                    EditorGUIUtility.labelWidth = previousLabelWidth;

                    if (GUILayout.Button(ControllerMinusButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                    {
                        list.DeleteArrayElementAtIndex(i);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.EndHorizontal();

                        var controllerType = mixedRealityControllerMapping.FindPropertyRelative("controllerType");
                        var controllerHandedness = mixedRealityControllerMapping.FindPropertyRelative("handedness");
                        var useDefaultModel = mixedRealityControllerMapping.FindPropertyRelative("useDefaultModel");
                        var controllerModel = mixedRealityControllerMapping.FindPropertyRelative("overrideModel");
                        var interactionsList = mixedRealityControllerMapping.FindPropertyRelative("interactions");
                        var useCustomInteractionMappings = mixedRealityControllerMapping.FindPropertyRelative("useCustomInteractionMappings");

                        EditorGUI.indentLevel++;
                        EditorGUIUtility.labelWidth = 128f;

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(controllerType);
                        EditorGUILayout.PropertyField(controllerHandedness);
                        EditorGUIUtility.labelWidth = 224f;

                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();

                            if (thisProfile.MixedRealityControllerMappingProfiles[i].ControllerType.Type == null)
                            {
                                controllerHandedness.intValue = 0;
                            }

                            // Only allow custom interaction mappings on generic controller types.
                            useCustomInteractionMappings.boolValue = thisProfile.MixedRealityControllerMappingProfiles[i].ControllerType.Type == typeof(GenericOpenVRController);
                            interactionsList.ClearArray();
                            serializedObject.ApplyModifiedProperties();
                            thisProfile.MixedRealityControllerMappingProfiles[i].SetDefaultInteractionMapping();
                            serializedObject.ApplyModifiedProperties();

                            // Bail so we can execute our controller type set command.
                            EditorGUI.indentLevel--;
                            GUILayout.EndVertical();
                            GUILayout.Space(12f);
                            GUILayout.EndVertical();
                            GUILayout.EndVertical();
                            return;
                        }

                        EditorGUIUtility.labelWidth = 128f;
                        EditorGUILayout.PropertyField(useDefaultModel);

                        if (!useDefaultModel.boolValue)
                        {
                            EditorGUILayout.PropertyField(controllerModel);
                        }

                        EditorGUIUtility.labelWidth = previousLabelWidth;

                        controllerInteractionFoldouts[i] = EditorGUILayout.Foldout(controllerInteractionFoldouts[i], InteractionContent, true);

                        if (controllerInteractionFoldouts[i])
                        {
                            RenderInteractionList(interactionsList, useCustomInteractionMappings.boolValue);
                        }

                        GUILayout.Space(24f);
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(24f);

                        if (useCustomInteractionMappings.boolValue)
                        {
                            if (GUILayout.Button("Reset Interaction Mappings", EditorStyles.miniButton))
                            {
                                interactionsList.ClearArray();
                                serializedObject.ApplyModifiedProperties();
                                thisProfile.MixedRealityControllerMappingProfiles[i].SetDefaultInteractionMapping();
                                serializedObject.ApplyModifiedProperties();
                            }
                        }

                        GUILayout.EndHorizontal();

                        EditorGUI.indentLevel--;
                    }
                }

                GUILayout.EndVertical();
                GUILayout.Space(12f);
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void RenderInteractionList(SerializedProperty list, bool useCustomInteractionMapping)
        {
            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "TypeReferenceUpdated") { return; }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            if (useCustomInteractionMapping)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(24f);

                if (GUILayout.Button(InteractionAddButtonContent, EditorStyles.miniButton))
                {
                    list.arraySize += 1;
                    var interaction = list.GetArrayElementAtIndex(list.arraySize - 1);
                    var axisType = interaction.FindPropertyRelative("axisType");
                    axisType.enumValueIndex = 0;
                    var inputType = interaction.FindPropertyRelative("inputType");
                    inputType.enumValueIndex = 0;
                    var action = interaction.FindPropertyRelative("inputAction");
                    var actionId = action.FindPropertyRelative("id");
                    var actionDescription = action.FindPropertyRelative("description");
                    actionDescription.stringValue = "None";
                    actionId.intValue = 0;
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(12f);

                if (list == null || list.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("Create an Interaction Mapping.", MessageType.Warning);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    return;
                }
            }

            GUILayout.BeginHorizontal();

            if (useCustomInteractionMapping)
            {
                EditorGUILayout.LabelField("Id", GUILayout.Width(32f));
                EditorGUIUtility.labelWidth = 24f;
                EditorGUIUtility.fieldWidth = 24f;
                EditorGUILayout.LabelField(ControllerInputTypeContent, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(AxisTypeContent, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(ActionContent, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(KeyCodeContent, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(XAxisContent, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(YAxisContent, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(string.Empty, GUILayout.Width(24f));

                EditorGUIUtility.labelWidth = defaultLabelWidth;
                EditorGUIUtility.fieldWidth = defaultFieldWidth;
            }
            else
            {
                EditorGUILayout.LabelField(InputDescription, GUILayout.Width(96f), GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(ActionContent, GUILayout.Width(96f));
            }

            GUILayout.EndHorizontal();

            for (int i = 0; i < list.arraySize; i++)
            {

                EditorGUILayout.BeginHorizontal();
                SerializedProperty interaction = list.GetArrayElementAtIndex(i);

                if (useCustomInteractionMapping)
                {
                    EditorGUILayout.LabelField($"{i + 1}", GUILayout.Width(32f));
                    EditorGUIUtility.labelWidth = 24f;
                    EditorGUIUtility.fieldWidth = 24f;
                    var inputType = interaction.FindPropertyRelative("inputType");
                    EditorGUILayout.PropertyField(inputType, GUIContent.none, GUILayout.ExpandWidth(true));
                    var axisType = interaction.FindPropertyRelative("axisType");
                    EditorGUILayout.PropertyField(axisType, GUIContent.none, GUILayout.ExpandWidth(true));
                    var action = interaction.FindPropertyRelative("inputAction");
                    var actionId = action.FindPropertyRelative("id");
                    var actionDescription = action.FindPropertyRelative("description");
                    var actionConstraint = action.FindPropertyRelative("axisConstraint");

                    EditorGUI.BeginChangeCheck();
                    actionId.intValue = EditorGUILayout.IntPopup(GUIContent.none, CheckValue(actionId.intValue, actionIds.Length - 1), actionLabels, actionIds, GUILayout.ExpandWidth(true));

                    if (EditorGUI.EndChangeCheck())
                    {
                        var inputAction = actionId.intValue == 0 ? MixedRealityInputAction.None : MixedRealityManager.Instance.ActiveProfile.InputActionsProfile.InputActions[actionId.intValue - 1];
                        actionDescription.stringValue = inputAction.Description;
                        actionConstraint.enumValueIndex = (int)inputAction.AxisConstraint;
                    }

                    if ((AxisType)axisType.intValue == AxisType.Digital)
                    {
                        var keyCode = interaction.FindPropertyRelative("keyCode");
                        EditorGUILayout.PropertyField(keyCode, GUIContent.none, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(GUIContent.none, GUILayout.ExpandWidth(true));
                    }

                    if ((AxisType)axisType.intValue == AxisType.SingleAxis ||
                        (AxisType)axisType.intValue == AxisType.DualAxis)
                    {
                        var axisCodeX = interaction.FindPropertyRelative("axisCodeX");
                        RenderAxisPopup(axisCodeX);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(GUIContent.none, GUILayout.ExpandWidth(true));
                    }

                    if ((AxisType)axisType.intValue == AxisType.DualAxis)
                    {
                        var axisCodeY = interaction.FindPropertyRelative("axisCodeY");
                        RenderAxisPopup(axisCodeY);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(GUIContent.none, GUILayout.ExpandWidth(true));
                    }

                    if (GUILayout.Button(InteractionMinusButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                    {
                        list.DeleteArrayElementAtIndex(i);
                    }

                    EditorGUIUtility.labelWidth = defaultLabelWidth;
                    EditorGUIUtility.fieldWidth = defaultFieldWidth;
                }
                else
                {
                    var interactionDescription = interaction.FindPropertyRelative("description");
                    EditorGUILayout.LabelField(interactionDescription.stringValue, GUILayout.Width(96f), GUILayout.ExpandWidth(true));
                    var action = interaction.FindPropertyRelative("inputAction");
                    var actionId = action.FindPropertyRelative("id");
                    var actionDescription = action.FindPropertyRelative("description");
                    var actionConstraint = action.FindPropertyRelative("axisConstraint");

                    EditorGUI.BeginChangeCheck();
                    actionId.intValue = EditorGUILayout.IntPopup(GUIContent.none, CheckValue(actionId.intValue, actionIds.Length - 1), actionLabels, actionIds, GUILayout.Width(96f));

                    if (EditorGUI.EndChangeCheck())
                    {
                        MixedRealityInputAction inputAction = actionId.intValue == 0 ? MixedRealityInputAction.None : MixedRealityManager.Instance.ActiveProfile.InputActionsProfile.InputActions[actionId.intValue - 1];
                        actionDescription.stringValue = inputAction.Description;
                        actionConstraint.enumValueIndex = (int)inputAction.AxisConstraint;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void RenderAxisPopup(SerializedProperty axisCode)
        {
            axisId = -1;

            for (int j = 0; j < ControllerMappingLibrary.UnityInputManagerAxes.Length; j++)
            {
                if (ControllerMappingLibrary.UnityInputManagerAxes[j].Name == axisCode.stringValue)
                {
                    axisId = j + 1;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            axisId = EditorGUILayout.IntPopup(GUIContent.none, axisId, axisLabels, axisIds, GUILayout.ExpandWidth(true));

            if (EditorGUI.EndChangeCheck())
            {
                if (axisId == 0)
                {
                    axisCode.stringValue = string.Empty;
                    serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    for (int j = 0; j < ControllerMappingLibrary.UnityInputManagerAxes.Length; j++)
                    {
                        if (axisId - 1 == j)
                        {
                            axisCode.stringValue = ControllerMappingLibrary.UnityInputManagerAxes[j].Name;
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }
                    }
                }
            }
        }

        private static int CheckValue(int value, int against)
        {
            if (value > against)
            {
                value = 0;
            }

            return value;
        }
    }
}