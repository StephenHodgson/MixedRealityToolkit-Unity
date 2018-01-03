﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine.EventSystems;

#if UNITY_WSA
using UnityEngine.XR.WSA.Input;
#endif

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Describes an input event that involves a select press.
    /// </summary>
    public class SelectPressedEventData : InteractionInputEventData
    {
        /// <summary>
        /// The amount, from 0.0 to 1.0, that the select was pressed.
        /// </summary>
        public double PressedAmount { get; private set; }

        public SelectPressedEventData(EventSystem eventSystem) : base(eventSystem)
        {
        }

#if UNITY_WSA
        public void Initialize(IInputSource inputSource, uint sourceId, double pressedAmount, Handedness handedness, object[] tags = null)
        {
            Initialize(inputSource, sourceId, InteractionSourcePressType.Select, handedness, tags);
            PressedAmount = pressedAmount;
        }
#endif
    }
}
