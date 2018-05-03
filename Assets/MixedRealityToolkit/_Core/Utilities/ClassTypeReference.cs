// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Utilities
{
    /// <summary>
    /// Reference to a class <see cref="System.Type"/> with support for Unity serialization.
    /// </summary>
    [Serializable]
    public sealed class ClassTypeReference : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string classReference = string.Empty;

        private Type type;

        public static string GetClassRef(Type type)
        {
            return type != null ? $"{type.FullName}, {type.Assembly.GetName().Name}" : string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeReference"/>class.
        /// </summary>
        /// <param name="assemblyQualifiedClassName">Assembly qualified class name.</param>
        public ClassTypeReference(string assemblyQualifiedClassName)
        {
            Type = !string.IsNullOrEmpty(assemblyQualifiedClassName)
                ? Type.GetType(assemblyQualifiedClassName)
                : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
        /// </summary>
        /// <param name="type">Class type.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="type"/> is not a class type.
        /// </exception>
        public ClassTypeReference(Type type)
        {
            Type = type;
        }

        #region ISerializationCallbackReceiver Members

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(classReference))
            {
                type = Type.GetType(classReference);

                if (type == null)
                {
                    Debug.LogWarning($"'{classReference}' was referenced but class type was not found.");
                }
            }
            else
            {
                type = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        #endregion ISerializationCallbackReceiver Members

        /// <summary>
        /// Gets or sets type of class reference.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is not a class type.
        /// </exception>
        public Type Type
        {
            get { return type; }
            set
            {
                if (value != null && !value.IsClass)
                {
                    throw new ArgumentException($"'{value.FullName}' is not a class type.", nameof(value));
                }

                type = value;
                classReference = GetClassRef(value);
            }
        }

        public static implicit operator string(ClassTypeReference typeReference)
        {
            return typeReference.classReference;
        }

        public static implicit operator Type(ClassTypeReference typeReference)
        {
            return typeReference.Type;
        }

        public static implicit operator ClassTypeReference(Type type)
        {
            return new ClassTypeReference(type);
        }

        public override string ToString()
        {
            if (Type.FullName != null)
            {
                if (Type != null)
                {
                    return Type.FullName;
                }
            }

            return "(None)";
        }
    }
}