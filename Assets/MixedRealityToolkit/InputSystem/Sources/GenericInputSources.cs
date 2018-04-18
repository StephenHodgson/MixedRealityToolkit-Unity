﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces;
using Microsoft.MixedReality.Toolkit.Internal.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.InputSystem.Sources
{
    /// <summary>
    /// This class manages the Input Sources recognized by <see cref="Input.GetJoystickNames"/>.
    /// <para> <remarks>The Windows Mixed Reality Spatial Controllers are handed in the <see cref="InteractionInputSources"/></remarks>.</para>
    /// </summary>
    public class GenericInputSources : MonoBehaviour
    {
        protected const string XboxController = "Xbox Controller";
        protected const string XboxOneForWindows = "Xbox One For Windows";
        protected const string XboxBluetoothGamePad = "Xbox Bluetooth Gamepad";
        protected const string XboxWirelessController = "Xbox Wireless Controller";

        protected const string MotionControllerLeft = "Spatial Controller - Left";
        protected const string MotionControllerRight = "Spatial Controller - Right";

        protected const string OpenVRControllerLeft = "OpenVR Controller - Left";
        protected const string OpenVRControllerRight = "OpenVR Controller - Right";

        protected const string OculusRemote = "Oculus Remote";
        protected const string OculusTouchLeft = "Oculus Touch - Left";
        protected const string OculusTouchRight = "Oculus Touch - Right";

        [SerializeField]
        [Tooltip("Time in seconds to determine if an Input Device has been connected or disconnected")]
        protected float DeviceRefreshInterval = 3.0f;
        protected string[] LastDeviceList;

        protected readonly HashSet<GenericInputSource> InputSources = new HashSet<GenericInputSource>();

        private float deviceRefreshTimer;
        private IMixedRealityInputSystem inputSystem;

        private void Awake()
        {
            inputSystem = MixedRealityManager.Instance.GetManager<IMixedRealityInputSystem>();
        }

        private void Update()
        {
            deviceRefreshTimer += Time.unscaledDeltaTime;

            if (deviceRefreshTimer >= DeviceRefreshInterval)
            {
                deviceRefreshTimer = 0.0f;
                RefreshDevices();
            }
        }

        private void RefreshDevices()
        {
            var joystickNames = Input.GetJoystickNames();

            if (joystickNames.Length <= 0) { return; }

            if (LastDeviceList != null && joystickNames.Length == LastDeviceList.Length)
            {
                for (int i = 0; i < LastDeviceList.Length; i++)
                {
                    if (joystickNames[i].Equals(LastDeviceList[i])) { continue; }

                    foreach (var inputSource in InputSources)
                    {
                        if (inputSource.SourceName.Equals(joystickNames[i]))
                        {
                            inputSystem.RaiseSourceLost(inputSource);
                            InputSources.Remove(inputSource);
                        }
                    }
                }
            }

            for (var i = 0; i < joystickNames.Length; i++)
            {
                if (joystickNames[i].Equals(MotionControllerLeft) ||
                    joystickNames[i].Equals(MotionControllerRight))
                {
                    // Skip any WMR motion controllers connected.
                    // They're handled in InteractionInputSources.
                    continue;
                }

                if (joystickNames[i].Equals(XboxController) ||
                    joystickNames[i].Equals(XboxOneForWindows) ||
                    joystickNames[i].Equals(XboxBluetoothGamePad) ||
                    joystickNames[i].Equals(XboxWirelessController))
                {
                    var inputSource = new GenericInputSource(joystickNames[i], new[]
                    {
                        InputType.ButtonPress,
                        InputType.Trigger,
                        InputType.TriggerPress,
                        InputType.ThumbStick,
                        InputType.ThumbStickPress,
                        InputType.Menu,
                        InputType.Select,
                    });

                    InputSources.Add(inputSource);
                    inputSystem.RaiseSourceDetected(inputSource);
                }
                else if (joystickNames[i].Equals(OculusTouchLeft) ||
                         joystickNames[i].Equals(OculusTouchRight) ||
                         joystickNames[i].Equals(OpenVRControllerLeft) ||
                         joystickNames[i].Equals(OpenVRControllerRight))
                {
                    var inputSource = new GenericInputSource(
                        joystickNames[i], new[]
                        {
                            InputType.Pointer,
                            InputType.PointerPosition,
                            InputType.PointerRotation,
                            InputType.Grip,
                            InputType.GripPress,
                            InputType.GripPosition,
                            InputType.GripRotation,
                            InputType.ButtonPress,
                            InputType.Trigger,
                            InputType.TriggerPress,
                            InputType.ThumbStick,
                            InputType.ThumbStickPress,
                            InputType.Menu,
                            InputType.Select,
                        });
                    InputSources.Add(inputSource);
                    inputSystem.RaiseSourceDetected(inputSource);
                }
                else
                {
                    Debug.LogWarning($"Unimplemented Controller Type: {joystickNames[i]}");
                }
            }

            LastDeviceList = joystickNames;
        }
    }
}
