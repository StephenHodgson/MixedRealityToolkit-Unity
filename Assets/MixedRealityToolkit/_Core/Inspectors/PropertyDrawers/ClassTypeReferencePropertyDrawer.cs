﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Assembly = System.Reflection.Assembly;
using Microsoft.MixedReality.Toolkit.Internal.Attributes;
using Microsoft.MixedReality.Toolkit.Internal.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Inspectors.PropertyDrawers
{
    /// <summary>
    /// Custom property drawer for <see cref="SystemType"/> properties.
    /// </summary>
    [CustomPropertyDrawer(typeof(SystemType))]
    [CustomPropertyDrawer(typeof(SystemTypeAttribute), true)]
    public class ClassTypeReferencePropertyDrawer : PropertyDrawer
    {
        private static int selectionControlId;
        private static string selectedClassRef;
        private static readonly Dictionary<string, Type> TypeMap = new Dictionary<string, Type>();

        #region Type Filtering

        /// <summary>
        /// Gets or sets a function that returns a collection of types that are
        /// to be excluded from drop-down. A value of <c>null</c> specifies that
        /// no types are to be excluded.
        /// </summary>
        /// <remarks>
        /// <para>This property must be set immediately before presenting a class
        /// type reference property field using <see cref="EditorGUI.PropertyField(Rect,SerializedProperty)"/>
        /// or <see cref="EditorGUILayout.PropertyField(SerializedProperty,UnityEngine.GUILayoutOption[])"/> since the value of this
        /// property is reset to <c>null</c> each time the control is drawn.</para>
        /// <para>Since filtering makes extensive use of <see cref="ICollection{Type}.Contains"/>
        /// it is recommended to use a collection that is optimized for fast
        /// lookups such as <see cref="HashSet{Type}"/> for better performance.</para>
        /// </remarks>
        /// <example>
        /// <para>Exclude a specific type from being selected:</para>
        /// <code language="csharp"><![CDATA[
        /// private SerializedProperty _someClassTypeReferenceProperty;
        /// 
        /// public override void OnInspectorGUI() {
        ///     serializedObject.Update();
        /// 
        ///     ClassTypeReferencePropertyDrawer.ExcludedTypeCollectionGetter = GetExcludedTypeCollection;
        ///     EditorGUILayout.PropertyField(_someClassTypeReferenceProperty);
        /// 
        ///     serializedObject.ApplyModifiedProperties();
        /// }
        /// 
        /// private ICollection<Type> GetExcludedTypeCollection() {
        ///     var set = new HashSet<Type>();
        ///     set.Add(typeof(SpecialClassToHideInDropdown));
        ///     return set;
        /// }
        /// ]]></code>
        /// </example>
        public static Func<ICollection<Type>> ExcludedTypeCollectionGetter { get; set; }

        private static List<Type> GetFilteredTypes(SystemTypeAttribute filter)
        {
            var types = new List<Type>();
            var assemblies = CompilationPipeline.GetAssemblies();
            var excludedTypes = ExcludedTypeCollectionGetter?.Invoke();

            foreach (var assembly in assemblies)
            {
                Assembly compiledAssembly = Assembly.Load(assembly.name);
                FilterTypes(compiledAssembly, filter, excludedTypes, types);
            }

            types.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
            return types;
        }

        private static void FilterTypes(Assembly assembly, SystemTypeAttribute filter, ICollection<Type> excludedTypes, List<Type> output)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsVisible || !type.IsClass)
                {
                    continue;
                }

                if (filter != null && !filter.IsConstraintSatisfied(type))
                {
                    continue;
                }

                if (excludedTypes != null && excludedTypes.Contains(type))
                {
                    continue;
                }

                output.Add(type);
            }
        }

        #endregion Type Filtering

        #region Type Utility

        private static Type ResolveType(string classRef)
        {
            Type type;
            if (!TypeMap.TryGetValue(classRef, out type))
            {
                type = !string.IsNullOrEmpty(classRef) ? Type.GetType(classRef) : null;
                TypeMap[classRef] = type;
            }

            return type;
        }

        #endregion Type Utility

        #region Control Drawing / Event Handling

        private static readonly int ControlHint = typeof(ClassTypeReferencePropertyDrawer).GetHashCode();
        private static readonly GUIContent TempContent = new GUIContent();

        private static string DrawTypeSelectionControl(Rect position, GUIContent label, string classRef, SystemTypeAttribute filter)
        {
            if (label != null && label != GUIContent.none)
            {
                position = EditorGUI.PrefixLabel(position, label);
            }

            int controlId = GUIUtility.GetControlID(ControlHint, FocusType.Keyboard, position);

            bool triggerDropDown = false;

            switch (Event.current.GetTypeForControl(controlId))
            {
                case EventType.ExecuteCommand:
                    if (Event.current.commandName == "TypeReferenceUpdated")
                    {
                        if (selectionControlId == controlId)
                        {
                            if (classRef != selectedClassRef)
                            {
                                classRef = selectedClassRef;
                                GUI.changed = true;
                            }

                            selectionControlId = 0;
                            selectedClassRef = null;
                        }
                    }

                    break;

                case EventType.MouseDown:
                    if (GUI.enabled && position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlId;
                        triggerDropDown = true;
                        Event.current.Use();
                    }

                    break;

                case EventType.KeyDown:
                    if (GUI.enabled && GUIUtility.keyboardControl == controlId)
                    {
                        if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Space)
                        {
                            triggerDropDown = true;
                            Event.current.Use();
                        }
                    }

                    break;

                case EventType.Repaint:
                    // Remove assembly name from content of popup control.
                    var classRefParts = classRef.Split(',');

                    TempContent.text = classRefParts[0].Trim();
                    if (TempContent.text == "")
                    {
                        TempContent.text = "(None)";
                    }
                    else if (ResolveType(classRef) == null)
                    {
                        TempContent.text += " {Missing}";
                    }

                    EditorStyles.popup.Draw(position, TempContent, controlId);
                    break;
            }

            if (triggerDropDown)
            {
                selectionControlId = controlId;
                selectedClassRef = classRef;

                DisplayDropDown(position, GetFilteredTypes(filter), ResolveType(classRef), filter?.Grouping ?? ClassGrouping.ByNamespaceFlat);
            }

            return classRef;
        }

        private static void DrawTypeSelectionControl(Rect position, SerializedProperty property, GUIContent label, SystemTypeAttribute filter)
        {
            try
            {
                bool restoreShowMixedValue = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                property.stringValue = DrawTypeSelectionControl(position, label, property.stringValue, filter);
                EditorGUI.showMixedValue = restoreShowMixedValue;
            }
            finally
            {
                ExcludedTypeCollectionGetter = null;
            }
        }

        private static void DisplayDropDown(Rect position, List<Type> types, Type selectedType, ClassGrouping grouping)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("(None)"), selectedType == null, OnSelectedTypeName, null);
            menu.AddSeparator(string.Empty);

            for (int i = 0; i < types.Count; ++i)
            {
                string menuLabel = FormatGroupedTypeName(types[i], grouping);

                if (string.IsNullOrEmpty(menuLabel)) { continue; }

                var content = new GUIContent(menuLabel);
                menu.AddItem(content, types[i] == selectedType, OnSelectedTypeName, types[i]);
            }

            menu.DropDown(position);
        }

        private static string FormatGroupedTypeName(Type type, ClassGrouping grouping)
        {
            string name = type.FullName;

            switch (grouping)
            {
                case ClassGrouping.None:
                    return name;
                case ClassGrouping.ByNamespace:
                    return string.IsNullOrEmpty(name) ? string.Empty : name.Replace('.', '/');
                case ClassGrouping.ByNamespaceFlat:
                    int lastPeriodIndex = string.IsNullOrEmpty(name) ? -1 : name.LastIndexOf('.');
                    if (lastPeriodIndex != -1)
                    {
                        name = string.IsNullOrEmpty(name)
                            ? string.Empty
                            : $"{name.Substring(0, lastPeriodIndex)}/{name.Substring(lastPeriodIndex + 1)}";
                    }

                    return name;
                case ClassGrouping.ByAddComponentMenu:
                    var addComponentMenuAttributes = type.GetCustomAttributes(typeof(AddComponentMenu), false);
                    if (addComponentMenuAttributes.Length == 1)
                    {
                        return ((AddComponentMenu)addComponentMenuAttributes[0]).componentMenu;
                    }

                    Debug.Assert(type.FullName != null);
                    return $"Scripts/{type.FullName.Replace('.', '/')}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(grouping), grouping, null);
            }
        }

        private static void OnSelectedTypeName(object userData)
        {
            selectedClassRef = SystemType.GetClassRef(userData as Type);
            var typeReferenceUpdatedEvent = EditorGUIUtility.CommandEvent("TypeReferenceUpdated");
            EditorWindow.focusedWindow.SendEvent(typeReferenceUpdatedEvent);
        }

        #endregion Control Drawing / Event Handling

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.popup.CalcHeight(GUIContent.none, 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawTypeSelectionControl(position, property.FindPropertyRelative("classReference"), label, attribute as SystemTypeAttribute);
        }
    }
}
