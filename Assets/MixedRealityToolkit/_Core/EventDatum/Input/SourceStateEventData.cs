﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input
{
    /// <summary>
    /// Describes an source state event that has a source id. 
    /// </summary>
    public class SourceStateEventData : BaseInputEventData
    {
        public SourceStateEventData(UnityEngine.EventSystems.EventSystem eventSystem) : base(eventSystem) { }

        public void Initialize(IInputSource inputSource, object[] tags)
        {
            BaseInitialize(inputSource, tags);
        }
    }
}
