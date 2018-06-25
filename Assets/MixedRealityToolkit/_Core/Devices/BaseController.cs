﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Managers;

namespace Microsoft.MixedReality.Toolkit.Internal.Devices
{
    /// <summary>
    /// Base Controller class to inherit from for all controllers.
    /// </summary>
    public class BaseController : IMixedRealityController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="controllerState"></param>
        /// <param name="controllerHandedness"></param>
        /// <param name="inputSource"></param>
        /// <param name="interactions"></param>
        public BaseController(ControllerState controllerState,
                              Handedness controllerHandedness,
                              IMixedRealityInputSource inputSource = null,
                              MixedRealityInteractionMapping[] interactions = null)
        {
            ControllerState = controllerState;
            ControllerHandedness = controllerHandedness;
            InputSource = inputSource;
            Interactions = interactions;
        }

        /// <inheritdoc />
        public ControllerState ControllerState { get; protected set; }

        /// <inheritdoc />
        public Handedness ControllerHandedness { get; }

        /// <inheritdoc />
        public IMixedRealityInputSource InputSource { get; }

        /// <inheritdoc />
        public MixedRealityInteractionMapping[] Interactions { get; protected set; }

        /// <inheritdoc />
        public virtual void SetupConfiguration() { }

        /// <summary>
        /// Returns the current Input System if enabled, otherwise null.
        /// </summary>
        protected IMixedRealityInputSystem InputSystem
        {
            get
            {
                if (inputSystem == null && MixedRealityManager.Instance.ActiveProfile.EnableInputSystem)
                {
                    inputSystem = MixedRealityManager.Instance.GetManager<IMixedRealityInputSystem>();
                }

                return inputSystem;
            }
        }

        private IMixedRealityInputSystem inputSystem;

        /// <summary>
        /// Load the Interaction mappings for this controller from the configured Controller Mapping profile
        /// </summary>
        /// <param name="mappings">Configured mappings from a controller mapping profile</param>
        protected void SetupFromMapping(MixedRealityInteractionMapping[] mappings)
        {
            var interactions = new System.Collections.Generic.List<MixedRealityInteractionMapping>();
            for (int i = 0; i < mappings.Length; i++)
            {
                switch (mappings[i].AxisType)
                {
                    case AxisType.Digital:
                        interactions.Add(new MixedRealityInteractionMapping((uint)i, mappings[i].AxisType, mappings[i].InputType, mappings[i].InputAction));
                        break;
                    case AxisType.SingleAxis:
                        interactions.Add(new MixedRealityInteractionMapping((uint)i, mappings[i].AxisType, mappings[i].InputType, mappings[i].InputAction));
                        break;
                    case AxisType.DualAxis:
                        interactions.Add(new MixedRealityInteractionMapping((uint)i, mappings[i].AxisType, mappings[i].InputType, mappings[i].InputAction));
                        break;
                    case AxisType.ThreeDofPosition:
                        interactions.Add(new MixedRealityInteractionMapping((uint)i, mappings[i].AxisType, mappings[i].InputType, mappings[i].InputAction));
                        break;
                    case AxisType.ThreeDofRotation:
                        interactions.Add(new MixedRealityInteractionMapping((uint)i, mappings[i].AxisType, mappings[i].InputType, mappings[i].InputAction));
                        break;
                    case AxisType.SixDof:
                        interactions.Add(new MixedRealityInteractionMapping((uint)i, mappings[i].AxisType, mappings[i].InputType, mappings[i].InputAction));
                        break;
                    case AxisType.None:
                    case AxisType.Raw:
                        interactions.Add(new MixedRealityInteractionMapping((uint)i, mappings[i].AxisType, mappings[i].InputType, mappings[i].InputAction));
                        break;
                    default:
                        throw new System.ArgumentOutOfRangeException();
                }
            }

            Interactions = interactions.ToArray();
        }
    }
}
