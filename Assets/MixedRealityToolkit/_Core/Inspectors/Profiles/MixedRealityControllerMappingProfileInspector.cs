﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Core.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.Devices.OpenVR;
using Microsoft.MixedReality.Toolkit.Core.Devices.UnityInput;
using Microsoft.MixedReality.Toolkit.Core.Devices.WindowsMixedReality;
using Microsoft.MixedReality.Toolkit.Core.Extensions;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using Microsoft.MixedReality.Toolkit.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Microsoft.MixedReality.Toolkit.Inspectors.Profiles
{
    [CustomEditor(typeof(MixedRealityControllerMappingProfile))]
    public class MixedRealityControllerMappingProfileInspector : MixedRealityBaseConfigurationProfileInspector
    {
        private struct ControllerRenderProfile
        {
            public SupportedControllerType ControllerType;
            public Handedness Handedness;
            public MixedRealityInteractionMapping[] Interactions;
            public bool UseDefaultModel;
            public Object OverrideModel;

            public ControllerRenderProfile(SupportedControllerType controllerType, Handedness handedness, MixedRealityInteractionMapping[] interactions, bool useDefaultModel, Object overrideModel)
            {
                ControllerType = controllerType;
                Handedness = handedness;
                Interactions = interactions;
                UseDefaultModel = useDefaultModel;
                OverrideModel = overrideModel;
            }
        }

        private const string ModelWarningText = "The Controller model you've specified is missing a IMixedRealityControllerPoseSynchronizer component. Without it the model will not synchronize it's pose with the controller data. Would you like to add one now?";
        private static readonly GUIContent ControllerAddButtonContent = new GUIContent("+ Add a New Controller Definition");
        private static readonly GUIContent ControllerMinusButtonContent = new GUIContent("-", "Remove Controller Template");
        private static readonly GUIContent GenericTypeContent = new GUIContent("Generic Type");

        private static readonly GUIContent[] GenericTypeListContent =
        {
            new GUIContent("Unity Controller"),
            new GUIContent("Open VR Controller")
        };

        private static readonly int[] GenericTypeIds = { 0, 1 };

        private static MixedRealityControllerMappingProfile thisProfile;

        private SerializedProperty mixedRealityControllerMappingProfiles;
        private SerializedProperty renderMotionControllers;
        private SerializedProperty useDefaultModels;
        private SerializedProperty globalLeftHandModel;
        private SerializedProperty globalRightHandModel;
        private float defaultLabelWidth;
        private float defaultFieldWidth;
        private GUIStyle controllerButtonStyle;

        private readonly List<ControllerRenderProfile> controllerRenderList = new List<ControllerRenderProfile>();

        private void OnEnable()
        {
            if (!CheckMixedRealityManager(false))
            {
                return;
            }

            mixedRealityControllerMappingProfiles = serializedObject.FindProperty("mixedRealityControllerMappingProfiles");

            if (!MixedRealityManager.Instance.ActiveProfile.IsInputSystemEnabled ||
                 MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile == null)
            {
                return;
            }

            renderMotionControllers = serializedObject.FindProperty("renderMotionControllers");
            useDefaultModels = serializedObject.FindProperty("useDefaultModels");
            globalLeftHandModel = serializedObject.FindProperty("globalLeftHandModel");
            globalRightHandModel = serializedObject.FindProperty("globalRightHandModel");

            thisProfile = target as MixedRealityControllerMappingProfile;
            defaultLabelWidth = EditorGUIUtility.labelWidth;
            defaultFieldWidth = EditorGUIUtility.fieldWidth;
        }

        public override void OnInspectorGUI()
        {
            RenderMixedRealityToolkitLogo();
            if (!CheckMixedRealityManager())
            {
                return;
            }

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
            EditorGUILayout.LabelField("Controller Templates", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Controller templates define all the controllers your users will be able to use in your application.\n\n" +
                                    "After defining all your Input Actions, you can then wire them up to hardware sensors, controllers, and other input devices.", MessageType.Info);

            if (MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile == null)
            {
                EditorGUILayout.HelpBox("No input actions found, please specify a input action profile in the main configuration.", MessageType.Error);
                return;
            }

            if (controllerButtonStyle == null)
            {
                controllerButtonStyle = new GUIStyle("LargeButton")
                {
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Bold,
                    stretchHeight = true,
                    stretchWidth = true,
                    wordWrap = true,
                    fontSize = 10,
                };
            }

            serializedObject.Update();

            EditorGUIUtility.labelWidth = 152f;
            EditorGUILayout.PropertyField(renderMotionControllers);

            if (renderMotionControllers.boolValue)
            {
                EditorGUILayout.PropertyField(useDefaultModels);

                if (!useDefaultModels.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(globalLeftHandModel);
                    EditorGUILayout.PropertyField(globalRightHandModel);

                    if (EditorGUI.EndChangeCheck())
                    {
                        CheckSynchronizer((GameObject)globalLeftHandModel.objectReferenceValue);
                        CheckSynchronizer((GameObject)globalRightHandModel.objectReferenceValue);
                    }
                }
            }

            EditorGUIUtility.labelWidth = defaultLabelWidth;

            RenderControllerProfilesList(mixedRealityControllerMappingProfiles, renderMotionControllers.boolValue);

            serializedObject.ApplyModifiedProperties();
        }

        private static void CheckSynchronizer(GameObject modelPrefab)
        {
            if (modelPrefab == null) { return; }

            var list = modelPrefab.GetComponentsInChildren<IMixedRealityControllerPoseSynchronizer>();

            if (list == null || list.Length == 0)
            {
                if (EditorUtility.DisplayDialog("Warning!", ModelWarningText, "Add Component", "I'll do it Later"))
                {
                    EditorGUIUtility.PingObject(modelPrefab);
                    Selection.activeObject = modelPrefab;
                }
            }
        }

        private void RenderControllerProfilesList(SerializedProperty controllerList, bool renderControllerModels)
        {
            if (thisProfile.MixedRealityControllerMappingProfiles.Length != controllerList.arraySize) { return; }

            EditorGUILayout.Space();

            if (GUILayout.Button(ControllerAddButtonContent, EditorStyles.miniButton))
            {
                AddController(controllerList, typeof(GenericJoystickController));
                return;
            }

            bool reset = false;
            if (controllerRenderList.Count > 0)
            {
                for (var type = 1; type <= (int)SupportedControllerType.TouchScreen; type++)
                {
                    if (controllerRenderList.All(profile => profile.ControllerType != (SupportedControllerType)type))
                    {
                        if ((SupportedControllerType)type == SupportedControllerType.TouchScreen)
                        {
                            AddController(controllerList, typeof(UnityTouchController));
                            reset = true;
                        }
                    }
                }
            }

            controllerRenderList.Clear();
            if (reset) { return; }

            GUILayout.Space(12f);
            GUILayout.BeginVertical();

            for (int i = 0; i < controllerList.arraySize; i++)
            {
                var controllerType = thisProfile.MixedRealityControllerMappingProfiles[i].ControllerType.Type;
                var supportedControllerType = SupportedControllerType.None;
                var mixedRealityControllerMapping = controllerList.GetArrayElementAtIndex(i);
                var controllerHandedness = mixedRealityControllerMapping.FindPropertyRelative("handedness");
                var handedness = (Handedness)controllerHandedness.intValue;
                var interactionsList = mixedRealityControllerMapping.FindPropertyRelative("interactions");
                var useDefaultModel = mixedRealityControllerMapping.FindPropertyRelative("useDefaultModel");
                var controllerModel = mixedRealityControllerMapping.FindPropertyRelative("overrideModel");
                var useCustomInteractionMappings = mixedRealityControllerMapping.FindPropertyRelative("useCustomInteractionMappings");

                if (controllerType == typeof(XboxController))
                {
                    supportedControllerType = SupportedControllerType.Xbox;
                }
                else if (controllerType == typeof(WindowsMixedRealityController) ||
                         controllerType == typeof(WindowsMixedRealityOpenVRMotionController))
                {
                    supportedControllerType = SupportedControllerType.WindowsMixedReality;
                }
                else if (controllerType == typeof(OculusTouchController))
                {
                    supportedControllerType = SupportedControllerType.OculusTouch;
                }
                else if (controllerType == typeof(OculusRemoteController))
                {
                    supportedControllerType = SupportedControllerType.OculusRemote;
                }
                else if (controllerType == typeof(ViveWandController))
                {
                    supportedControllerType = SupportedControllerType.ViveWand;
                }
                else if (controllerType == typeof(GenericOpenVRController))
                {
                    supportedControllerType = SupportedControllerType.GenericOpenVR;
                }
                else if (controllerType == typeof(GenericJoystickController))
                {
                    supportedControllerType = SupportedControllerType.GenericUnity;
                }
                else if (controllerType == typeof(UnityTouchController))
                {
                    supportedControllerType = SupportedControllerType.TouchScreen;
                }

                bool skip = false;

                for (int j = 0; j < controllerRenderList.Count; j++)
                {
                    if (supportedControllerType == SupportedControllerType.GenericOpenVR ||
                        supportedControllerType == SupportedControllerType.GenericUnity)
                    {
                        continue;
                    }

                    if (controllerRenderList[j].ControllerType == supportedControllerType &&
                        controllerRenderList[j].Handedness == handedness)
                    {
                        thisProfile.MixedRealityControllerMappingProfiles[i].SynchronizeInputActions(controllerRenderList[j].Interactions);
                        useDefaultModel.boolValue = controllerRenderList[j].UseDefaultModel;
                        controllerModel.objectReferenceValue = controllerRenderList[j].OverrideModel;
                        serializedObject.ApplyModifiedProperties();
                        skip = true;
                    }
                }

                if (skip) { continue; }

                controllerRenderList.Add(new ControllerRenderProfile(supportedControllerType, handedness, thisProfile.MixedRealityControllerMappingProfiles[i].Interactions, useDefaultModel.boolValue, controllerModel.objectReferenceValue));

                var handednessTitleText = handedness != Handedness.None ? $"{handedness} Hand " : string.Empty;
                var controllerTitle = $"{supportedControllerType.ToString().ToProperCase()} {handednessTitleText}Controller";

                if (useCustomInteractionMappings.boolValue)
                {
                    GUILayout.Space(24f);

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();

                    EditorGUIUtility.labelWidth = 64f;
                    EditorGUIUtility.fieldWidth = 64f;
                    EditorGUILayout.LabelField(controllerTitle);
                    EditorGUIUtility.fieldWidth = defaultFieldWidth;
                    EditorGUIUtility.labelWidth = defaultLabelWidth;

                    if (GUILayout.Button(ControllerMinusButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                    {
                        controllerList.DeleteArrayElementAtIndex(i);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        return;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;

                    EditorGUIUtility.labelWidth = 128f;
                    EditorGUIUtility.fieldWidth = 64f;

                    EditorGUI.BeginChangeCheck();

                    int currentGenericType = -1;

                    if (controllerType == typeof(GenericJoystickController))
                    {
                        currentGenericType = 0;
                    }

                    if (controllerType == typeof(GenericOpenVRController))
                    {
                        currentGenericType = 1;
                    }

                    Debug.Assert(currentGenericType != -1);

                    currentGenericType = EditorGUILayout.IntPopup(GenericTypeContent, currentGenericType, GenericTypeListContent, GenericTypeIds);

                    if (controllerType != typeof(GenericJoystickController))
                    {
                        EditorGUILayout.PropertyField(controllerHandedness);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        switch (currentGenericType)
                        {
                            case 0:
                                controllerType = typeof(GenericJoystickController);
                                controllerHandedness.intValue = 0;
                                break;
                            case 1:
                                controllerType = typeof(GenericOpenVRController);
                                break;
                        }

                        interactionsList.ClearArray();
                        serializedObject.ApplyModifiedProperties();
                        thisProfile.MixedRealityControllerMappingProfiles[i].ControllerType.Type = controllerType;
                        GUILayout.EndVertical();
                        return;
                    }

                    if (interactionsList.arraySize == 0 && controllerType == typeof(GenericOpenVRController))
                    {
                        thisProfile.MixedRealityControllerMappingProfiles[i].SetDefaultInteractionMapping(true);
                        serializedObject.ApplyModifiedProperties();
                    }

                    if (renderControllerModels && controllerHandedness.intValue != 0)
                    {
                        EditorGUILayout.PropertyField(useDefaultModel);

                        if (!useDefaultModel.boolValue)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(controllerModel);

                            if (EditorGUI.EndChangeCheck())
                            {
                                CheckSynchronizer((GameObject)controllerModel.objectReferenceValue);
                            }
                        }
                    }

                    EditorGUIUtility.labelWidth = defaultLabelWidth;
                    EditorGUIUtility.fieldWidth = defaultFieldWidth;

                    EditorGUI.indentLevel--;

                    if (GUILayout.Button("Edit Input Action Map"))
                    {
                        ControllerPopupWindow.Show(supportedControllerType, interactionsList, (Handedness)controllerHandedness.intValue);
                    }

                    if (GUILayout.Button("Reset Input Actions"))
                    {
                        interactionsList.ClearArray();
                        serializedObject.ApplyModifiedProperties();
                        thisProfile.MixedRealityControllerMappingProfiles[i].SetDefaultInteractionMapping(true);
                        serializedObject.ApplyModifiedProperties();
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    if (supportedControllerType == SupportedControllerType.WindowsMixedReality &&
                        handedness == Handedness.None)
                    {
                        controllerTitle = "HoloLens Gestures";
                    }

                    if (handedness != Handedness.Right)
                    {
                        GUILayout.BeginHorizontal();
                    }

                    var buttonContent = new GUIContent(controllerTitle, ControllerMappingLibrary.GetControllerTextureScaled(supportedControllerType, handedness));

                    if (GUILayout.Button(buttonContent, controllerButtonStyle, GUILayout.Height(128f), GUILayout.MinWidth(32f), GUILayout.ExpandWidth(true)))
                    {
                        ControllerPopupWindow.Show(supportedControllerType, interactionsList, (Handedness)controllerHandedness.intValue);
                    }

                    if (handedness != Handedness.Left)
                    {
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(8f);
            }

            GUILayout.EndVertical();
        }

        private void AddController(SerializedProperty controllerList, Type controllerType)
        {
            controllerList.InsertArrayElementAtIndex(controllerList.arraySize);
            var index = controllerList.arraySize - 1;
            var mixedRealityControllerMapping = controllerList.GetArrayElementAtIndex(index);
            var mixedRealityControllerMappingId = mixedRealityControllerMapping.FindPropertyRelative("id");
            mixedRealityControllerMappingId.intValue = index;
            var mixedRealityControllerMappingDescription = mixedRealityControllerMapping.FindPropertyRelative("description");
            mixedRealityControllerMappingDescription.stringValue = controllerType.Name;
            var mixedRealityControllerHandedness = mixedRealityControllerMapping.FindPropertyRelative("handedness");
            mixedRealityControllerHandedness.intValue = 0;
            var mixedRealityControllerInteractions = mixedRealityControllerMapping.FindPropertyRelative("interactions");
            var useCustomInteractionMappings = mixedRealityControllerMapping.FindPropertyRelative("useCustomInteractionMappings");
            useCustomInteractionMappings.boolValue = controllerType == typeof(GenericOpenVRController) || controllerType == typeof(GenericJoystickController);
            mixedRealityControllerInteractions.ClearArray();
            serializedObject.ApplyModifiedProperties();
            thisProfile.MixedRealityControllerMappingProfiles[index].ControllerType.Type = controllerType;
            thisProfile.MixedRealityControllerMappingProfiles[index].SetDefaultInteractionMapping(true);
        }
    }
}