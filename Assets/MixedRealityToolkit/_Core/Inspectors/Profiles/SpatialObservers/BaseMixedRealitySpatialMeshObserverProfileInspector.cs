﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.DataProviders.SpatialObservers;
using UnityEditor;

namespace Microsoft.MixedReality.Toolkit.Core.Inspectors.Profiles.SpatialAwareness
{
    [CustomEditor(typeof(BaseMixedRealitySpatialMeshObserverProfile))]
    public abstract class BaseMixedRealitySpatialMeshObserverProfileInspector : BaseMixedRealitySpatialObserverProfileInspector
    {
        private SerializedProperty meshLevelOfDetail;
        private SerializedProperty meshTrianglesPerCubicMeter;
        private SerializedProperty meshRecalculateNormals;
        private SerializedProperty meshDisplayOption;
        private SerializedProperty meshVisibleMaterial;
        private SerializedProperty meshOcclusionMaterial;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            if (!CheckMixedRealityConfigured(false))
            {
                return;
            }

            meshLevelOfDetail = serializedObject.FindProperty("meshLevelOfDetail");
            meshTrianglesPerCubicMeter = serializedObject.FindProperty("meshTrianglesPerCubicMeter");
            meshRecalculateNormals = serializedObject.FindProperty("meshRecalculateNormals");
            meshDisplayOption = serializedObject.FindProperty("meshDisplayOption");
            meshVisibleMaterial = serializedObject.FindProperty("meshVisibleMaterial");
            meshOcclusionMaterial = serializedObject.FindProperty("meshOcclusionMaterial");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!CheckMixedRealityConfigured())
            {
                return;
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(meshLevelOfDetail);
            EditorGUILayout.PropertyField(meshTrianglesPerCubicMeter);
            EditorGUILayout.PropertyField(meshRecalculateNormals);
            EditorGUILayout.PropertyField(meshDisplayOption);
            EditorGUILayout.PropertyField(meshVisibleMaterial);
            EditorGUILayout.PropertyField(meshOcclusionMaterial);

            serializedObject.ApplyModifiedProperties();
        }
    }
}