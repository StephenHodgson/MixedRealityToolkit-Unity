// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Devices.OpenVR
{
    public class OculusRemoteController : GenericOpenVRController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackingState"></param>
        /// <param name="controllerHandedness"></param>
        /// <param name="inputSource"></param>
        /// <param name="interactions"></param>
        public OculusRemoteController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
                : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "D-Pad Position", AxisType.DualAxis, DeviceInputType.DirectionalPad, ControllerMappingLibrary.AXIS_5, ControllerMappingLibrary.AXIS_6),
            new MixedRealityInteractionMapping(1, "Button.One", AxisType.DualAxis, DeviceInputType.ButtonPress, KeyCode.JoystickButton0),
            new MixedRealityInteractionMapping(2, "Button.Two", AxisType.DualAxis, DeviceInputType.ButtonPress, KeyCode.JoystickButton1),
            new MixedRealityInteractionMapping(3, "D-Pad Up", AxisType.SingleAxis, DeviceInputType.ButtonPress, ControllerMappingLibrary.AXIS_6),
            new MixedRealityInteractionMapping(4, "D-Pad Down", AxisType.NegativeSingleAxis, DeviceInputType.ButtonPress, ControllerMappingLibrary.AXIS_6),
            new MixedRealityInteractionMapping(5, "D-Pad Right", AxisType.SingleAxis, DeviceInputType.ButtonPress, ControllerMappingLibrary.AXIS_5),
            new MixedRealityInteractionMapping(6, "D-Pad Left", AxisType.NegativeSingleAxis, DeviceInputType.ButtonPress, ControllerMappingLibrary.AXIS_5),
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(DefaultInteractions);
        }
    }
}
