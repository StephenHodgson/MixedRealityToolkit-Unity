// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using Microsoft.MixedReality.Toolkit.Core.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.Managers;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Inspectors.Profiles
{
    [CustomEditor(typeof(MixedRealityInputActionRulesProfile))]
    public class MixedRealityInputActionRulesInspector : MixedRealityBaseConfigurationProfileInspector
    {
        private static readonly GUIContent DigitalContent = new GUIContent("Digital");
        private static readonly GUIContent SingleAxisContent = new GUIContent("Single Axis");
        private static readonly GUIContent DualAxisContent = new GUIContent("Dual Axis");
        private static readonly GUIContent VectorContent = new GUIContent("Three DoF Position");
        private static readonly GUIContent QuaternionContent = new GUIContent("Three DoF Rotation");
        private static readonly GUIContent PoseContent = new GUIContent("Six DoF Pose");
        private static readonly GUIContent DigitalAddRuleContent = new GUIContent("Add Digital Rule");
        private static readonly GUIContent SingleAxisAddRuleContent = new GUIContent("Add Single Axis Rule");
        private static readonly GUIContent DualAxisAddRuleContent = new GUIContent("Add Dual Axis Rule");
        private static readonly GUIContent VectorAddRuleContent = new GUIContent("Add Vector Rule");
        private static readonly GUIContent QuaternionAddRuleContent = new GUIContent("Add Quaternion Rule");
        private static readonly GUIContent PoseAddRuleContent = new GUIContent("Add Pose Rule");
        private static readonly GUIContent BaseActionContent = new GUIContent("Base Action: ");
        private static readonly GUIContent RuleActionContent = new GUIContent("Rule Action: ");

        private SerializedProperty inputActionRulesDigital;
        private SerializedProperty inputActionRulesSingleAxis;
        private SerializedProperty inputActionRulesDualAxis;
        private SerializedProperty inputActionRulesVectorAxis;
        private SerializedProperty inputActionRulesQuaternionAxis;
        private SerializedProperty inputActionRulesPoseAxis;

        private static int[] digitalActionIds;
        private static GUIContent[] digitalActionLabels;
        private static int[] singleAxisActionIds;
        private static GUIContent[] singleAxisActionLabels;
        private static int[] dualAxisActionIds;
        private static GUIContent[] dualAxisActionLabels;
        private static int[] threeDofPositionActionIds;
        private static GUIContent[] threeDofPositionActionLabels;
        private static int[] threeDofRotationActionIds;
        private static GUIContent[] threeDofRotationActionLabels;
        private static int[] sixDofActionIds;
        private static GUIContent[] sixDofActionLabels;

        private void OnEnable()
        {
            if (!MixedRealityManager.ConfirmInitialized())
            {
                return;
            }

            if (!MixedRealityManager.HasActiveProfile)
            {
                return;
            }

            inputActionRulesDigital = serializedObject.FindProperty("inputActionRulesDigital");
            inputActionRulesSingleAxis = serializedObject.FindProperty("inputActionRulesSingleAxis");
            inputActionRulesDualAxis = serializedObject.FindProperty("inputActionRulesDualAxis");
            inputActionRulesVectorAxis = serializedObject.FindProperty("inputActionRulesVectorAxis");
            inputActionRulesQuaternionAxis = serializedObject.FindProperty("inputActionRulesQuaternionAxis");
            inputActionRulesPoseAxis = serializedObject.FindProperty("inputActionRulesPoseAxis");

            #region Interaction Constraint

            digitalActionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.Digital)
                .Select(action => (int)action.Id)
                .Prepend(0).ToArray();

            digitalActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.Digital)
                .Select(inputAction => new GUIContent(inputAction.Description))
                .Prepend(new GUIContent("None")).ToArray();

            singleAxisActionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.SingleAxis)
                .Select(action => (int)action.Id)
                .Prepend(0).ToArray();

            singleAxisActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.SingleAxis)
                .Select(inputAction => new GUIContent(inputAction.Description))
                .Prepend(new GUIContent("None")).ToArray();

            dualAxisActionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.DualAxis)
                .Select(action => (int)action.Id).Prepend(0).ToArray();

            dualAxisActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.DualAxis)
                .Select(inputAction => new GUIContent(inputAction.Description))
                .Prepend(new GUIContent("None")).ToArray();

            threeDofPositionActionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.ThreeDofPosition)
                .Select(action => (int)action.Id)
                .Prepend(0).ToArray();

            threeDofPositionActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.ThreeDofPosition)
                .Select(inputAction => new GUIContent(inputAction.Description))
                .Prepend(new GUIContent("None")).ToArray();

            threeDofRotationActionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.ThreeDofRotation)
                .Select(action => (int)action.Id)
                .Prepend(0).ToArray();

            threeDofRotationActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.ThreeDofRotation)
                .Select(inputAction => new GUIContent(inputAction.Description))
                .Prepend(new GUIContent("None")).ToArray();

            sixDofActionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.SixDof)
                .Select(action => (int)action.Id)
                .Prepend(0).ToArray();

            sixDofActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                .Where(inputAction => inputAction.AxisConstraint == AxisType.SixDof)
                .Select(inputAction => new GUIContent(inputAction.Description))
                .Prepend(new GUIContent("None")).ToArray();

            #endregion
        }

        public override void OnInspectorGUI()
        {
            RenderMixedRealityToolkitLogo();
            if (!CheckMixedRealityManager())
            {
                return;
            }

            if (GUILayout.Button("Back to Input Profile"))
            {
                Selection.activeObject = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input Action Rules Profile", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The Input Action Rules...", MessageType.Info);

            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(DigitalContent, EditorStyles.boldLabel);
            if (GUILayout.Button(DigitalAddRuleContent, EditorStyles.miniButton))
            {
                var prop = CreateNewAction(inputActionRulesDigital);
                if (prop != null)
                {
                    prop.boolValue = false;
                }
            }
            RenderList(inputActionRulesDigital, digitalActionIds, digitalActionLabels);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(SingleAxisContent, EditorStyles.boldLabel);
            if (GUILayout.Button(SingleAxisAddRuleContent, EditorStyles.miniButton))
            {
                var prop = CreateNewAction(inputActionRulesSingleAxis);
                if (prop != null)
                {
                    prop.floatValue = 0f;
                }
            }
            RenderList(inputActionRulesSingleAxis, singleAxisActionIds, singleAxisActionLabels);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(DualAxisContent, EditorStyles.boldLabel);
            if (GUILayout.Button(DualAxisAddRuleContent, EditorStyles.miniButton))
            {
                var prop = CreateNewAction(inputActionRulesDualAxis);
                if (prop != null)
                {
                    prop.vector2Value = Vector2.zero;
                }
            }
            RenderList(inputActionRulesDualAxis, dualAxisActionIds, dualAxisActionLabels);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(VectorContent, EditorStyles.boldLabel);
            if (GUILayout.Button(VectorAddRuleContent, EditorStyles.miniButton))
            {
                var prop = CreateNewAction(inputActionRulesVectorAxis);
                if (prop != null)
                {
                    prop.vector3Value = Vector3.zero;
                }
            }
            RenderList(inputActionRulesVectorAxis, threeDofPositionActionIds, threeDofPositionActionLabels);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(QuaternionContent, EditorStyles.boldLabel);
            if (GUILayout.Button(QuaternionAddRuleContent, EditorStyles.miniButton))
            {
                var prop = CreateNewAction(inputActionRulesQuaternionAxis);
                if (prop != null)
                {
                    prop.quaternionValue = Quaternion.identity;
                }
            }
            RenderList(inputActionRulesQuaternionAxis, threeDofRotationActionIds, threeDofRotationActionLabels);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(PoseContent, EditorStyles.boldLabel);
            if (GUILayout.Button(PoseAddRuleContent, EditorStyles.miniButton))
            {
                var prop = CreateNewAction(inputActionRulesPoseAxis);
                if (prop != null)
                {
                    prop.intValue = 0;
                }
            }
            RenderList(inputActionRulesPoseAxis, sixDofActionIds, sixDofActionLabels);
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderList(SerializedProperty list, int[] actionIds, GUIContent[] actionLabels)
        {
            if (list == null || list.arraySize == 0)
            {
                return;
            }

            for (int i = 0; i < list.arraySize; i++)
            {
                var inputActionRule = list.GetArrayElementAtIndex(i);
                var baseAction = inputActionRule.FindPropertyRelative("baseAction");
                var ruleAction = inputActionRule.FindPropertyRelative("ruleAction");
                var criteria = inputActionRule.FindPropertyRelative("criteria");

                if (baseAction != null && ruleAction != null && criteria != null)
                {
                    var baseActionId = baseAction.FindPropertyRelative("id");
                    var ruleActionId = ruleAction.FindPropertyRelative("id");

                    EditorGUILayout.BeginHorizontal();

                    var labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 78f;
                    var changedBaseId = EditorGUILayout.IntPopup(new GUIContent(BaseActionContent), baseActionId.intValue, actionLabels, actionIds);
                    var changedRuleId = EditorGUILayout.IntPopup(new GUIContent(RuleActionContent), ruleActionId.intValue, actionLabels, actionIds);

                    if (changedBaseId == changedRuleId)
                    {
                        changedRuleId = 0;
                        Debug.LogWarning("The base action and rule action can't be the same!");
                    }

                    baseActionId.intValue = changedBaseId;
                    ruleActionId.intValue = changedRuleId;

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();

                    EditorGUIUtility.labelWidth = 64f;
                    EditorGUILayout.PropertyField(criteria);
                    if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                    {
                        list.DeleteArrayElementAtIndex(i);
                    }
                    EditorGUIUtility.labelWidth = labelWidth;

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }
            }
        }

        private SerializedProperty CreateNewAction(SerializedProperty list)
        {
            if (list == null)
            {
                return null;
            }

            list.arraySize += 1;
            var inputActionRule = list.GetArrayElementAtIndex(list.arraySize - 1);
            var baseActionId = inputActionRule.FindPropertyRelative("baseAction").FindPropertyRelative("id");
            var ruleActionId = inputActionRule.FindPropertyRelative("ruleAction").FindPropertyRelative("id");
            baseActionId.intValue = 0;
            ruleActionId.intValue = 0;

            return list.FindPropertyRelative("criteria");
        }
    }
}