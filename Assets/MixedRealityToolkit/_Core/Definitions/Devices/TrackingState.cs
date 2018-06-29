// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices
{
    /// <summary>
    /// The Tracking State defines how a device is currently being tracked.
    /// This enables developers to be able to handle non-tracked situations and react accordingly.
    /// </summary>
    public enum TrackingState
    {
        /// <summary>
        /// The device does not support the concept of tracking.
        /// </summary>
        None = 0,
        /// <summary>
        /// Reserved, for systems that provide alternate tracking.
        /// </summary>
        Other,
        /// <summary>
        /// The device is currently not tracked.
        /// </summary>
        NotTracked,
        /// <summary>
        /// The device is currently tracked.
        /// </summary>
        Tracked,
    }
}