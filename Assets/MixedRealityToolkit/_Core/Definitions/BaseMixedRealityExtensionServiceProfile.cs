// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Definitions
{
    /// <summary>
    /// The base profile to use for custom <see cref="Interfaces.IMixedRealityExtensionService"/>s
    /// </summary>
    public abstract class BaseMixedRealityExtensionServiceProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        private DataModelConfiguration<IMixedRealityDataProvider>[] configurations = null;

        /// <summary>
        /// Currently registered system and manager configurations.
        /// </summary>
        public virtual DataModelConfiguration<IMixedRealityDataProvider>[] Configurations
        {
            get { return configurations; }
            set { configurations = value; }
        }
    }
}