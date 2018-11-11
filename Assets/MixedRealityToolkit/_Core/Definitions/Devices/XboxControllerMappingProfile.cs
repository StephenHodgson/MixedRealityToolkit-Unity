﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Devices.UnityInput;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Definitions.Devices
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Mixed Reality Controller Mappings/Xbox Controller Mapping Profile", fileName = "XboxControllerMappingProfile")]
    public class XboxControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.Xbox;

        protected override void Awake()
        {
            ControllerMappings = new[]
            {
                new MixedRealityControllerMapping("Xbox Controller", typeof(XboxController))
            };

            base.Awake();
        }
    }
}