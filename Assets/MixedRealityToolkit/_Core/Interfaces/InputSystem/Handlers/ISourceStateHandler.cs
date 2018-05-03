﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Interface to implement to react to source state changes, such as when an input source is detected or lost.
    /// </summary>
    public interface ISourceStateHandler : IEventSystemHandler
    {
        void OnSourceDetected(SourceStateEventData eventData);
        void OnSourceLost(SourceStateEventData eventData);
        void OnSourcePositionChanged(SourcePositionEventData eventData);
        void OnSourceRotationChanged(SourceRotationEventData eventData);
    }
}
