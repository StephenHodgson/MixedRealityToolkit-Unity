// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Core.Interfaces;

namespace Microsoft.MixedReality.Toolkit.Core.Definitions
{
    public static class DataModelConfigurationExtensions
    {
        public static bool TryGetConfigurationName<T>(this DataModelConfiguration<T>[] configurations, out string name) where T : IMixedRealityDataProvider
        {
            name = string.Empty;

            for (int i = 0; i < configurations.Length; i++)
            {
                if (typeof(T).IsAssignableFrom(configurations[i].ComponentType.Type))
                {
                    name = configurations[i].ComponentName;
                    return true;
                }
            }

            return false;
        }
    }
}