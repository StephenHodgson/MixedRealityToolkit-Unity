// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Core.Definitions;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;

namespace Microsoft.MixedReality.Toolkit.Core.Extensions
{
    public static class DataModelConfigurationExtensions
    {
        public static bool TryGetConfigurationName<T>(this DataModelConfiguration[] configurations, out string name) where T : IMixedRealityDataProvider
        {
            name = string.Empty;

            for (int i = 0; i < configurations.Length; i++)
            {
                if (typeof(T).IsAssignableFrom(configurations[i].DataModelType.Type))
                {
                    name = configurations[i].DataModelName;
                    return true;
                }
            }

            return false;
        }
    }
}