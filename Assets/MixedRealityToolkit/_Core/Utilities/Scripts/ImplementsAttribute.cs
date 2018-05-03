using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Utilities
{
    public class ImplementsAttribute : PropertyAttribute
    {
        public Type InterfaceType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementsAttribute"/> class.
        /// </summary>
        /// <param name="interfaceType">Type of interface that the type must implement.</param>
        public ImplementsAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }

        public bool IsType(Type type)
        {
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType == InterfaceType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
