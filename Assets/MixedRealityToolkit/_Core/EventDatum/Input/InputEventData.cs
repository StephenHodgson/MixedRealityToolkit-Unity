﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input
{
    /// <summary>
    /// Describes an input event that has a source id.
    /// </summary>
    public class InputEventData : BaseInputEventData
    {
        public Handedness Handedness { get; private set; }

        public KeyCode KeyCode { get; private set; }

        public InputType InputType { get; private set; }

        public InputEventData(UnityEngine.EventSystems.EventSystem eventSystem) : base(eventSystem) { }

        public void Initialize(IInputSource inputSource, object[] tags)
        {
            BaseInitialize(inputSource, tags);
            Handedness = Handedness.None;
            KeyCode = KeyCode.None;
            InputType = InputType.None;
        }

        public void Initialize(IInputSource inputSource, KeyCode keyCode, object[] tags)
        {
            BaseInitialize(inputSource, tags);
            Handedness = Handedness.None;
            KeyCode = keyCode;
            InputType = InputType.None;
        }

        public void Initialize(IInputSource inputSource, InputType inputType, object[] tags)
        {
            BaseInitialize(inputSource, tags);
            Handedness = Handedness.None;
            KeyCode = KeyCode.None;
            InputType = inputType;
        }

        public void Initialize(IInputSource inputSource, Handedness handedness, object[] tags)
        {
            BaseInitialize(inputSource, tags);
            Handedness = handedness;
            KeyCode = KeyCode.None;
            InputType = InputType.None;
        }

        public void Initialize(IInputSource inputSource, KeyCode keyCode, Handedness handedness, object[] tags)
        {
            BaseInitialize(inputSource, tags);
            Handedness = handedness;
            KeyCode = keyCode;
            InputType = InputType.None;
        }

        public void Initialize(IInputSource inputSource, InputType inputType, Handedness handedness, object[] tags)
        {
            BaseInitialize(inputSource, tags);
            Handedness = handedness;
            KeyCode = KeyCode.None;
            InputType = inputType;
        }
    }
}
