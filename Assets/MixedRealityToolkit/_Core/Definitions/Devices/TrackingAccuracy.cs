// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices
{
    /// <summary>
    /// The Tracking Accuracy defines how well a device (ex: controller or headset) is currently 
    /// being tracked. This enables developers to respond to various situations and react accordingly.
    /// </summary>
    public enum TrackingAccuracy
    {
        /// <summary>
        /// There is no accuracy data for this device.
        /// </summary>
        None = 0,
        /// <summary>
        /// The device is returning it's most accurate data.
        /// </summary>
        High,
        /// <summary>
        /// The device is returning it's medium level of accuracy.
        /// </summary>
        Medium,
        /// <summary>
        /// The device is returning it's low level of accuracy.
        /// </summary>
        Low,
        /// <summary>
        /// The device is returning approximate tracking data.
        /// </summary>
        /// <remarks>
        /// Approximate accuracy generally implies that the device is not
        /// visible to sensors and that the system is inferring the data by
        /// other means.
        /// </remarks>
        Approximate
    }
}