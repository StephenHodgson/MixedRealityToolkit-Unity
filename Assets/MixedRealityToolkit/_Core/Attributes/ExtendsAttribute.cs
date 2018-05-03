// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.MixedReality.Toolkit.Internal.Utilities;

namespace Microsoft.MixedReality.Toolkit.Internal.Attributes
{
    /// <summary>
    /// Constraint that allows selection of classes that extend a specific class when
    /// selecting a <see cref="ClassTypeReference"/> with the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ExtendsAttribute : ClassTypeConstraintAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendsAttribute"/> class.
        /// </summary>
        /// <param name="baseType">Type of class that selectable classes must derive from.</param>
        public ExtendsAttribute(Type baseType)
        {
            BaseType = baseType;
        }

        /// <summary>
        /// Gets the type of class that selectable classes must derive from.
        /// </summary>
        public Type BaseType { get; private set; }

        /// <inheritdoc/>
        public override bool IsConstraintSatisfied(Type type)
        {
            return base.IsConstraintSatisfied(type) &&
                   BaseType.IsAssignableFrom(type) &&
                   type != BaseType;
        }
    }
}