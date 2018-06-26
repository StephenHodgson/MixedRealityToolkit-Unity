﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input
{
    /// <summary>
    /// Describes an Input Event that involves an Input Source's spatial position AND rotation.
    /// </summary>
    public class PoseInputEventData : ThreeDofInputEventData
    {
        /// <summary>
        /// The <see cref="Vector3"/> and <see cref="Quaternion"/> input data.
        /// </summary>
        public SixDof InputData { get; private set; } = new SixDof(Vector3.zero, Quaternion.identity);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventSystem"></param>
        public PoseInputEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="inputSource"></param>
        /// <param name="inputAction"></param>
        /// <param name="inputData"></param>
        public void Initialize(IMixedRealityInputSource inputSource, MixedRealityInputAction inputAction, SixDof inputData)
        {
            Initialize(inputSource, inputAction);
            Position = inputData.Position;
            Rotation = inputData.Rotation;
            InputData = inputData;
        }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="inputSource"></param>
        /// <param name="handedness"></param>
        /// <param name="inputAction"></param>
        /// <param name="inputData"></param>
        public void Initialize(IMixedRealityInputSource inputSource, Handedness handedness, MixedRealityInputAction inputAction, SixDof inputData)
        {
            Initialize(inputSource, handedness, inputAction);
            Position = inputData.Position;
            Rotation = inputData.Rotation;
            InputData = inputData;
        }
    }
}