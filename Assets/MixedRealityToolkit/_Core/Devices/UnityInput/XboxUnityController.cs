﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Devices.UnityInput
{
    /// <summary>
    /// Xbox Controller using Unity Input System
    /// </summary>
    public class XboxUnityController : GenericUnityController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackingState"></param>
        /// <param name="controllerHandedness"></param>
        /// <param name="inputSource"></param>
        /// <param name="interactions"></param>
        public XboxUnityController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        /// <summary>
        /// Default interactions for Xbox Controller using Unity Input System.
        /// </summary>
        public static readonly MixedRealityInteractionMapping[] DefaultInteractions =
        {
            new MixedRealityInteractionMapping(0, "Left Thumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, ControllerMappingLibrary.AXIS_1, ControllerMappingLibrary.AXIS_2, true),
            new MixedRealityInteractionMapping(0, "Left Thumbstick Click", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton8),
            new MixedRealityInteractionMapping(0, "Right Thumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, ControllerMappingLibrary.AXIS_4, ControllerMappingLibrary.AXIS_5, true),
            new MixedRealityInteractionMapping(0, "Right Thumbstick Click", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton9),
            new MixedRealityInteractionMapping(0, "D-Pad", AxisType.DualAxis, DeviceInputType.DirectionalPad, ControllerMappingLibrary.AXIS_6, ControllerMappingLibrary.AXIS_7, true),
            new MixedRealityInteractionMapping(0, "Shared Trigger", AxisType.SingleAxis, DeviceInputType.Trigger, ControllerMappingLibrary.AXIS_3),
            new MixedRealityInteractionMapping(0, "Left Trigger", AxisType.SingleAxis, DeviceInputType.Trigger, ControllerMappingLibrary.AXIS_9),
            new MixedRealityInteractionMapping(0, "Right Trigger", AxisType.SingleAxis, DeviceInputType.Trigger, ControllerMappingLibrary.AXIS_10),
            new MixedRealityInteractionMapping(0, "View", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton6),
            new MixedRealityInteractionMapping(0, "Menu", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton7),
            new MixedRealityInteractionMapping(0, "Left Bumper", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton4),
            new MixedRealityInteractionMapping(0, "Right Bumper", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton5),
            new MixedRealityInteractionMapping(0, "A", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton0),
            new MixedRealityInteractionMapping(0, "B", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton1),
            new MixedRealityInteractionMapping(0, "X", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton2),
            new MixedRealityInteractionMapping(0, "Y", AxisType.Digital, DeviceInputType.ButtonPress,KeyCode.JoystickButton3),
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(DefaultInteractions);
        }
    }
}