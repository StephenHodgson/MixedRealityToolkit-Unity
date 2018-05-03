// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Interfaces;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Definitions
{
    /// <summary>
    /// Configuration profile settings for the Mixed Reality Toolkit
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Mixed Reality Configuration Profile")]
    public class MixedRealityConfigurationProfile : ScriptableObject, ISerializationCallbackReceiver
    {
        #region Manager Registry properties

        /// <summary>
        /// Serialized list of managers for the Mixed Reality manager
        /// </summary>
        [SerializeField]
        private IMixedRealityManager[] initialManagers = null;

        /// <summary>
        /// Serialized list of the Interface types for the Mixed Reality manager
        /// </summary>
        [SerializeField]
        private Type[] initialManagerTypes = null;

        /// <summary>
        /// Dictionary list of active managers used by the Mixed Reality Manager at runtime
        /// </summary>
        public Dictionary<Type, IMixedRealityManager> ActiveManagers { get; } = new Dictionary<Type, IMixedRealityManager>();

        #endregion Manager Registry properties

        #region Mixed Reality Manager configurable properties

        /// <summary>
        /// Enable and configure the Input System component for the Mixed Reality Toolkit
        /// </summary>
        [SerializeField]
        [Header("Input Settings")]
        [Tooltip("Enable the Input System on Startup")]
        private bool enableInputSystem = true;

        public bool EnableInputSystem
        {
            get { return enableInputSystem; }
            private set { enableInputSystem = value; }
        }

        [SerializeField]
        [Tooltip("Input System Script File to instantiate at runtime.")]
        [ClassImplements(typeof(IMixedRealityInputSystem))]
        private ClassTypeReference inputSystemReference;
        public ClassTypeReference InputSystemReference
        {
            get { return inputSystemReference; }
            private set { inputSystemReference = value; }
        }

        /// <summary>
        /// Enable and configure the controller rendering for the Mixed Reality Toolkit
        /// </summary>
        [SerializeField]
        [Tooltip("Enable the Motion Controllers on Startup")]
        private bool enableControllers = true;

        public bool EnableControllers
        {
            get { return enableControllers; }
            private set { enableControllers = value; }
        }

        /// <summary>
        /// Enable and configure the Boundary component on the Mixed Reality Camera
        /// </summary>
        [SerializeField]
        [Header("Boundary Settings")]
        [Tooltip("Enable the Boundary on Startup")]
        private bool enableBoundarySystem = true;

        public bool EnableBoundarySystem
        {
            get { return enableBoundarySystem; }
            private set { enableBoundarySystem = value; }
        }

        #endregion Mixed Reality Manager configurable properties

        #region ISerializationCallbackReceiver Implementation

        /// <summary>
        /// Unity function to prepare data for serialization.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var count = ActiveManagers.Count;
            initialManagers = new IMixedRealityManager[count];
            initialManagerTypes = new Type[count];

            foreach (var manager in ActiveManagers)
            {
                --count;
                initialManagers[count] = manager.Value;
                initialManagerTypes[count] = manager.Key;
            }
        }

        /// <summary>
        /// Unity function to resolve data from serialization when a project is loaded
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // From the serialized fields for the MixedRealityConfigurationProfile, populate the Active managers list
            // *Note This will only take effect once the Mixed Reality Toolkit has a custom editor for the MixedRealityConfigurationProfile

            ActiveManagers.Clear();
            for (int i = 0; i < initialManagers?.Length; i++)
            {
                Managers.MixedRealityManager.Instance.AddManager(initialManagerTypes[i], initialManagers[i]);
            }
        }

        #endregion  ISerializationCallbackReceiver Implementation
    }


    /// <summary>
    /// Indicates how selectable classes should be collated in drop-down menu.
    /// </summary>
    public enum ClassGrouping
    {
        /// <summary>
        /// No grouping, just show type names in a list; for instance, "Some.Nested.Namespace.SpecialClass".
        /// </summary>
        None,
        /// <summary>
        /// Group classes by namespace and show foldout menus for nested namespaces; for
        /// instance, "Some > Nested > Namespace > SpecialClass".
        /// </summary>
        ByNamespace,
        /// <summary>
        /// Group classes by namespace; for instance, "Some.Nested.Namespace > SpecialClass".
        /// </summary>
        ByNamespaceFlat,
        /// <summary>
        /// Group classes in the same way as Unity does for its component menu. This
        /// grouping method must only be used for <see cref="MonoBehaviour"/> types.
        /// </summary>
        ByAddComponentMenu,
    }

    /// <summary>
    /// Base class for class selection constraints that can be applied when selecting
    /// a <see cref="ClassTypeReference"/> with the Unity inspector.
    /// </summary>
    public abstract class ClassTypeConstraintAttribute : PropertyAttribute
    {
        private ClassGrouping _grouping = ClassGrouping.ByNamespaceFlat;
        private bool _allowAbstract = false;

        /// <summary>
        /// Gets or sets grouping of selectable classes. Defaults to <see cref="ClassGrouping.ByNamespaceFlat"/>
        /// unless explicitly specified.
        /// </summary>
        public ClassGrouping Grouping
        {
            get { return _grouping; }
            set { _grouping = value; }
        }

        /// <summary>
        /// Gets or sets whether abstract classes can be selected from drop-down.
        /// Defaults to a value of <c>false</c> unless explicitly specified.
        /// </summary>
        public bool AllowAbstract
        {
            get { return _allowAbstract; }
            set { _allowAbstract = value; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> satisfies filter constraint.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>
        /// A <see cref="bool"/> value indicating if the type specified by <paramref name="type"/>
        /// satisfies this constraint and should thus be selectable.
        /// </returns>
        public virtual bool IsConstraintSatisfied(Type type)
        {
            return AllowAbstract || !type.IsAbstract;
        }

    }

    /// <summary>
    /// Constraint that allows selection of classes that extend a specific class when
    /// selecting a <see cref="ClassTypeReference"/> with the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClassExtendsAttribute : ClassTypeConstraintAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassExtendsAttribute"/> class.
        /// </summary>
        /// <param name="baseType">Type of class that selectable classes must derive from.</param>
        public ClassExtendsAttribute(Type baseType)
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
            return base.IsConstraintSatisfied(type)
                && BaseType.IsAssignableFrom(type) && type != BaseType;
        }

    }

    /// <summary>
    /// Constraint that allows selection of classes that implement a specific interface
    /// when selecting a <see cref="ClassTypeReference"/> with the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClassImplementsAttribute : ClassTypeConstraintAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassImplementsAttribute"/> class.
        /// </summary>
        /// <param name="interfaceType">Type of interface that selectable classes must implement.</param>
        public ClassImplementsAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }

        /// <summary>
        /// Gets the type of interface that selectable classes must implement.
        /// </summary>
        public Type InterfaceType { get; private set; }

        /// <inheritdoc/>
        public override bool IsConstraintSatisfied(Type type)
        {
            if (base.IsConstraintSatisfied(type))
            {
                foreach (var interfaceType in type.GetInterfaces())
                    if (interfaceType == InterfaceType)
                        return true;
            }
            return false;
        }

    }

    /// <summary>
    /// Reference to a class <see cref="System.Type"/> with support for Unity serialization.
    /// </summary>
    [Serializable]
    public sealed class ClassTypeReference : ISerializationCallbackReceiver
    {
        public static string GetClassRef(Type type)
        {
            return type != null ? $"{type.FullName}, {type.Assembly.GetName().Name}" : string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
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
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="type"/> is not a class type.
        /// </exception>
        public ClassTypeReference(Type type)
        {
            Type = type;
        }

        [SerializeField]
        private string _classRef;

        #region ISerializationCallbackReceiver Members

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(_classRef))
            {
                type = System.Type.GetType(_classRef);

                if (type == null)
                    Debug.LogWarning(string.Format("'{0}' was referenced but class type was not found.", _classRef));
            }
            else
            {
                type = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        #endregion

        private Type type;

        /// <summary>
        /// Gets or sets type of class reference.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="value"/> is not a class type.
        /// </exception>
        public Type Type
        {
            get { return type; }
            set
            {
                if (value != null && !value.IsClass)
                    throw new ArgumentException(string.Format("'{0}' is not a class type.", value.FullName), "value");

                type = value;
                _classRef = GetClassRef(value);
            }
        }

        public static implicit operator string(ClassTypeReference typeReference)
        {
            return typeReference._classRef;
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
            return Type != null ? Type.FullName : "(None)";
        }

    }
}