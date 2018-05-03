﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;

namespace Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Interface to implement to react to focus enter/exit.
    /// </summary>
    public interface IFocusHandler : IFocusChangedHandler
    {
        bool HasFocus { get; }

        bool FocusEnabled { get; set; }

        List<IPointer> Focusers { get; }

        void OnFocusEnter(FocusEventData eventData);

        void OnFocusExit(FocusEventData eventData);
    }
}
