// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using Microsoft.MixedReality.Toolkit.Core.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Inspectors.Profiles
{
    [CustomEditor(typeof(MixedRealityInputActionRulesProfile))]
    public class MixedRealityInputActionRulesInspector : MixedRealityBaseConfigurationProfileInspector
    {
        private static readonly GUIContent AddRuleContent = new GUIContent("Add Input Action Rule");
        private static readonly GUIContent BaseActionContent = new GUIContent("Base Action: ");
        private static readonly GUIContent RuleActionContent = new GUIContent("Rule Action: ");
        private static readonly GUIContent CriteriaContent = new GUIContent("Criteria: ");

        private SerializedProperty inputActionRulesDigital;
        private SerializedProperty inputActionRulesSingleAxis;
        private SerializedProperty inputActionRulesDualAxis;
        private SerializedProperty inputActionRulesVectorAxis;
        private SerializedProperty inputActionRulesQuaternionAxis;
        private SerializedProperty inputActionRulesPoseAxis;

        private static string[] allActionLabels;
        private static Dictionary<int, AxisType> axisConstraints;
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
        private static int[] baseActionIds;
        private static GUIContent[] baseActionLabels;
        private static List<SerializedProperty> actionProperties;

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

            #region Interaction Constraints
            
            allActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                 .Select(action => action.Description)
                 .Prepend("None").ToArray();

            axisConstraints = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                 .Prepend(new MixedRealityInputAction(0, "None"))
                 .ToDictionary(action => (int)action.Id, action => action.AxisConstraint);

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

            baseActionIds = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                 .Where(action => action.AxisConstraint != AxisType.None && action.AxisConstraint != AxisType.Raw)
                 .Select(action => (int)action.Id)
                 .Prepend(0).ToArray();

            baseActionLabels = MixedRealityManager.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions
                 .Where(action => action.AxisConstraint != AxisType.None && action.AxisConstraint != AxisType.Raw)
                 .Select(inputAction => new GUIContent(inputAction.Description))
                 .Prepend(new GUIContent("None")).ToArray();

            RefreshProperties();

            #endregion
        }

        private void OnDisable()
        {
            // Any empty base actions get removed.
            if (inputActionRulesDigital != null)
            {
                for (int i = inputActionRulesDigital.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesDigital.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0)
                    { inputActionRulesDigital.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesSingleAxis != null)
            {
                for (int i = inputActionRulesSingleAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesSingleAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0)
                    { inputActionRulesSingleAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesDualAxis != null)
            {
                for (int i = inputActionRulesDualAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesDualAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0)
                    { inputActionRulesDualAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesDualAxis != null)
            {
                for (int i = inputActionRulesVectorAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesVectorAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0)
                    { inputActionRulesVectorAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesQuaternionAxis != null)
            {
                for (int i = inputActionRulesQuaternionAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesQuaternionAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0)
                    { inputActionRulesQuaternionAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesPoseAxis != null)
            {
                for (int i = inputActionRulesPoseAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesPoseAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0)
                    { inputActionRulesPoseAxis.DeleteArrayElementAtIndex(i); }
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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

            bool refresh = false;

            if (GUILayout.Button(AddRuleContent, EditorStyles.miniButton))
            {
                AddPropertyToList(inputActionRulesDigital, 0, (int)AxisType.None); // Nones are put in digital.
                refresh = true;
            }

            EditorGUILayout.Space();

            if (!refresh)
            {
                for (int i = 0; i < actionProperties.Count; i++)
                {
                    var inputActionRule = actionProperties[i];
                    var baseAction = inputActionRule.FindPropertyRelative("baseAction");
                    var ruleAction = inputActionRule.FindPropertyRelative("ruleAction");
                    var criteria = inputActionRule.FindPropertyRelative("criteria");

                    if (baseAction != null)
                    {
                        var baseActionId = baseAction.FindPropertyRelative("id");

                        EditorGUILayout.BeginHorizontal();
                        var labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 78f;
                        var changedBaseId = EditorGUILayout.IntPopup(BaseActionContent, baseActionId.intValue, baseActionLabels, baseActionIds);
                        EditorGUIUtility.labelWidth = labelWidth;

                        if (changedBaseId != baseActionId.intValue)
                        {
                            // If the base action was changed...
                            var oldAxis = baseAction.FindPropertyRelative("axisConstraint");
                            var newBaseActionAxis = axisConstraints[changedBaseId];

                            SerializedProperty list = null;
                            int ruleActionId = ruleAction.FindPropertyRelative("id").intValue;
                            list = GetListFromAxisConstraint(oldAxis.intValue);
                            DeletePropertyFromList(list, inputActionRule); // Delete from old list

                            list = GetListFromAxisConstraint((int)newBaseActionAxis);
                            AddPropertyToList(list, changedBaseId, (int)newBaseActionAxis); // Add to new list

                            refresh = true;
                            break;
                        }

                        baseActionId.intValue = changedBaseId;
                        var axisConstraint = (int)axisConstraints[baseActionId.intValue];
                        if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                        {
                            var list = GetListFromAxisConstraint(axisConstraint);
                            DeletePropertyFromList(list, inputActionRule);

                            refresh = true;
                            break;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (!refresh && baseActionId.intValue != 0)
                        {
                            var ruleActionId = ruleAction.FindPropertyRelative("id");
                            var ruleActionDescription = ruleAction.FindPropertyRelative("description");

                            EditorGUIUtility.labelWidth = 64f;
                            EditorGUILayout.PropertyField(criteria, CriteriaContent);

                            EditorGUIUtility.labelWidth = 78f;
                            var changedRuleId = EditorGUILayout.IntPopup(RuleActionContent, ruleActionId.intValue, GetLabelsFromAxis(axisConstraint), GetIdsFromAxis(axisConstraint));

                            if (changedBaseId == changedRuleId)
                            {
                                changedRuleId = 0;
                                Debug.LogWarning("The base action and rule action can't be the same!");
                            }

                            ruleActionId.intValue = changedRuleId;
                            ruleActionDescription.stringValue = allActionLabels[changedRuleId];
                            ruleAction.FindPropertyRelative("axisConstraint").intValue = axisConstraint;

                            EditorGUIUtility.labelWidth = labelWidth;
                        }

                        GUILayout.Space(18f);
                    }
                }
            }

            if (refresh)
            {
                RefreshProperties();
                refresh = false;
            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshProperties()
        {
            List<SerializedProperty> properties = new List<SerializedProperty>();
            for (int i = 0; i < inputActionRulesDigital?.arraySize; i++)
            { properties.Add(inputActionRulesDigital.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesSingleAxis?.arraySize; i++)
            { properties.Add(inputActionRulesSingleAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesDualAxis?.arraySize; i++)
            { properties.Add(inputActionRulesDualAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesVectorAxis?.arraySize; i++)
            { properties.Add(inputActionRulesVectorAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesQuaternionAxis?.arraySize; i++)
            { properties.Add(inputActionRulesQuaternionAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesPoseAxis?.arraySize; i++)
            { properties.Add(inputActionRulesPoseAxis.GetArrayElementAtIndex(i)); }
            

            actionProperties = properties
                .OrderBy(property => property.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue)
                .ToList();
        }

        private SerializedProperty GetListFromAxisConstraint(int axis)
        {
            SerializedProperty list = null;
            switch((AxisType)axis)
            {
                case AxisType.Digital:
                    list = inputActionRulesDigital;
                    break;
                case AxisType.SingleAxis:
                    list = inputActionRulesSingleAxis;
                    break;
                case AxisType.DualAxis:
                    list = inputActionRulesDualAxis;
                    break;
                case AxisType.ThreeDofPosition:
                    list = inputActionRulesVectorAxis;
                    break;
                case AxisType.ThreeDofRotation:
                    list = inputActionRulesQuaternionAxis;
                    break;
                case AxisType.SixDof:
                    list = inputActionRulesPoseAxis;
                    break;
                case AxisType.None:
                    list = inputActionRulesDigital; // Nones placeholder
                    break;
            }

            return list;
        }

        private void AddPropertyToList(SerializedProperty list, int baseActionId, int axis)
        {
            if (list == null)
            {
                return;
            }

            list.arraySize += 1;
            var newInputActionRule = list.GetArrayElementAtIndex(list.arraySize - 1);

            var baseAction = newInputActionRule.FindPropertyRelative("baseAction");
            var baseActionIdProperty = baseAction.FindPropertyRelative("id");
            var baseActionDescription = baseAction.FindPropertyRelative("description");
            var baseActionAxisConstraint = baseAction.FindPropertyRelative("axisConstraint");

            var ruleAction = newInputActionRule.FindPropertyRelative("ruleAction");
            var ruleActionIdProperty = ruleAction.FindPropertyRelative("id");
            var ruleActionDescription = ruleAction.FindPropertyRelative("description");
            var ruleActionAxisConstraint = ruleAction.FindPropertyRelative("axisConstraint");

            baseActionIdProperty.intValue = baseActionId;
            baseActionDescription.stringValue = allActionLabels[baseActionId];
            baseActionAxisConstraint.intValue = axis;

            ruleActionIdProperty.intValue = 0;
            ruleActionDescription.stringValue = "None";
            ruleActionAxisConstraint.intValue = 0;
        }

        private void DeletePropertyFromList(SerializedProperty list, SerializedProperty property)
        {
            for (int j = 0; j < list.arraySize; j++)
            {
                SerializedProperty testProp = list.GetArrayElementAtIndex(j);
                if (testProp.propertyPath == property.propertyPath)
                {
                    list.DeleteArrayElementAtIndex(j);
                    break;
                }
            }
        }

        private int[] GetIdsFromAxis(int axis)
        {
            switch ((AxisType)axis)
            {
                case AxisType.Digital:
                    return digitalActionIds;
                case AxisType.SingleAxis:
                    return singleAxisActionIds;
                case AxisType.DualAxis:
                    return dualAxisActionIds;
                case AxisType.ThreeDofPosition:
                    return threeDofPositionActionIds;
                case AxisType.ThreeDofRotation:
                    return threeDofRotationActionIds;
                case AxisType.SixDof:
                    return sixDofActionIds;
            }

            return new int[] { };
        }

        private GUIContent[] GetLabelsFromAxis(int axis)
        {
            switch((AxisType)axis)
            {
                case AxisType.Digital:
                    return digitalActionLabels;
                case AxisType.SingleAxis:
                    return singleAxisActionLabels;
                case AxisType.DualAxis:
                    return dualAxisActionLabels;
                case AxisType.ThreeDofPosition:
                    return threeDofPositionActionLabels;
                case AxisType.ThreeDofRotation:
                    return threeDofRotationActionLabels;
                case AxisType.SixDof:
                    return sixDofActionLabels;
            }

            return new GUIContent[] { };
        }
    }
}