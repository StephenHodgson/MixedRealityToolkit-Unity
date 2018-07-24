﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem.Handlers;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem
{
    public interface ITeleportTarget : IMixedRealityFocusChangedHandler
    {
        /// <summary>
        /// The position the teleport will end at.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The normal of the teleport raycast.
        /// </summary>
        Vector3 Normal { get; }

        /// <summary>
        /// Is the teleport target active?
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Should the target orientation be overridden?
        /// </summary>
        bool OverrideTargetOrientation { get; }

        /// <summary>
        /// Should the destination orientation be overridden?
        /// Useful when you want to orient the user in a specific direction when they teleport to this position.
        /// <remarks>
        /// Override orientation is the transform forward of the GameObject this component is attached to.
        /// </remarks>
        /// </summary>
        float TargetOrientation { get; }
    }
}