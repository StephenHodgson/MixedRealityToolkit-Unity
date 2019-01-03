// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Interfaces.DataProviders.Controllers;
using Microsoft.MixedReality.Toolkit.SDK.Input.Handlers;
using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Controllers
{
    /// <summary>
    /// The Mixed Reality Visualization component is primarily responsible for synchronizing the user's current input with controller models.
    /// </summary>
    /// <seealso cref="Core.DataProviders.Controllers.MixedRealityControllerVisualizationProfile"/>
    public class DefaultMixedRealityControllerVisualizer : ControllerPoseSynchronizer, IMixedRealityControllerVisualizer
    {
        /// <inheritdoc />
        public GameObject GameObjectProxy
        {
            get
            {
                try
                {
                    return gameObject;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}