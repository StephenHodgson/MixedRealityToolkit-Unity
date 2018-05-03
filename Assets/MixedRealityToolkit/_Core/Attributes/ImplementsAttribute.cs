// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.MixedReality.Toolkit.Internal.Utilities;

namespace Microsoft.MixedReality.Toolkit.Internal.Attributes
{
    /// <summary>
    /// Constraint that allows selection of classes that implement a specific interface
    /// when selecting a <see cref="ClassTypeReference"/> with the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ImplementsAttribute : ClassTypeConstraintAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementsAttribute"/> class.
        /// </summary>
        /// <param name="interfaceType">Type of interface that selectable classes must implement.</param>
        /// <param name="grouping"></param>
        public ImplementsAttribute(Type interfaceType, ClassGrouping grouping = ClassGrouping.ByNamespaceFlat)
        {
            InterfaceType = interfaceType;
            Grouping = grouping;
        }

        /// <summary>
        /// Gets the type of interface that selectable classes must implement.
        /// </summary>
        public Type InterfaceType { get; private set; }

        /// <inheritdoc />
        public override bool IsConstraintSatisfied(Type type)
        {
            if (base.IsConstraintSatisfied(type))
            {
                var interfaces = type.GetInterfaces();
                for (var i = 0; i < interfaces.Length; i++)
                {
                    if (interfaces[i] == InterfaceType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}