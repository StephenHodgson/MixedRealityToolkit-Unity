// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Attributes;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Definitions.Devices
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Mixed Reality Controller Proxy Profile", fileName = "MixedRealityControllerProxyProfile", order = (int)CreateProfileMenuItemIndices.ControllerProxies)]
    public class MixedRealityControllerProxyProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("Enable and configure the controller rendering of the Motion Controllers on Startup.")]
        private bool renderMotionControllers = false;

        /// <summary>
        /// Enable and configure the controller rendering of the Motion Controllers on Startup.
        /// </summary>
        public bool RenderMotionControllers
        {
            get { return renderMotionControllers; }
            private set { renderMotionControllers = value; }
        }

        [SerializeField]
        [Implements(typeof(IMixedRealityControllerProxy), TypeGrouping.ByNamespaceFlat)]
        [Tooltip("The concrete Controller Proxy component to use on the rendered controller model.")]
        private SystemType controllerProxyType;

        /// <summary>
        /// The concrete Controller Proxy component to use on the rendered controller model
        /// </summary>
        public SystemType ControllerProxyType
        {
            get { return controllerProxyType; }
            private set { controllerProxyType = value; }
        }

        [SerializeField]
        [Tooltip("Use the platform SDK to load the default controller models.")]
        private bool useDefaultModels = false;

        /// <summary>
        /// User the controller model loader provided by the SDK, or provide override models.
        /// </summary>
        public bool UseDefaultModels
        {
            get { return useDefaultModels; }
            private set { useDefaultModels = value; }
        }

        [SerializeField]
        [Tooltip("Override Left Controller Model.")]
        private GameObject globalLeftHandModel;

        /// <summary>
        /// The Default controller model when there is no specific controller model for the Left hand or when no hand is specified (Handedness = none)
        /// </summary>
        /// <remarks>
        /// If the default model for the left hand controller can not be found, the controller will fall back and use this for visualization.
        /// </remarks>
        public GameObject GlobalLeftHandModel
        {
            get { return globalLeftHandModel; }
            private set { globalLeftHandModel = value; }
        }

        [SerializeField]
        [Tooltip("Override Right Controller Model.\nNote: If the default model is not found, the fallback is the global right hand model.")]
        private GameObject globalRightHandModel;

        /// <summary>
        /// The Default controller model when there is no specific controller model for the Right hand.
        /// </summary>
        /// <remarks>
        /// If the default model for the right hand controller can not be found, the controller will fall back and use this for visualization.
        /// </remarks>
        public GameObject GlobalRightHandModel
        {
            get { return globalRightHandModel; }
            private set { globalRightHandModel = value; }
        }

        [SerializeField]
        private MixedRealityControllerProxySetting[] controllerProxySettings = new MixedRealityControllerProxySetting[0];

        /// <summary>
        /// The current list of controller proxy settings.
        /// </summary>
        public MixedRealityControllerProxySetting[] ControllerProxySettings => controllerProxySettings;

        /// <summary>
        /// Gets the override model for a specific controller type and hand
        /// </summary>
        /// <param name="controllerType">The type of controller to query for</param>
        /// <param name="hand">The specific hand assigned to the controller</param>
        public GameObject GetControllerModelOverride(Type controllerType, Handedness hand)
        {
            for (int i = 0; i < controllerProxySettings.Length; i++)
            {
                if (controllerProxySettings[i].ControllerType != null &&
                    controllerProxySettings[i].ControllerType.Type == controllerType &&
                   (controllerProxySettings[i].Handedness == hand || controllerProxySettings[i].Handedness == Handedness.Both))
                {
                    return controllerProxySettings[i].OverrideControllerModel;
                }
            }

            return null;
        }
    }
}