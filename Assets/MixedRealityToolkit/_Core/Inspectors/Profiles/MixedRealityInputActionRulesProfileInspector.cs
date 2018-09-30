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
        private static readonly float LayoutSpace = 20f;
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

        private static List<bool> actionFoldoutBools;
        private static List<List<bool>> deletedActions;
        private static List<List<SerializedProperty>> actionProperties;

        private static bool breakLoop = false;

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
            List<string> keepActionList = new List<string>();
            for(int g = 0; g < actionProperties?.Count; g++)
            {
                var group = actionProperties[g];
                for(int a = 0; a < group.Count; a++)
                {
                    if (!deletedActions[g][a])
                    {
                        keepActionList.Add(group[a].propertyPath);
                    }
                }
            }

            if (inputActionRulesDigital != null)
            {
                for (int i = inputActionRulesDigital.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesDigital.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0 ||
                        !keepActionList.Contains(prop.propertyPath))
                    { inputActionRulesDigital.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesSingleAxis != null)
            {
                for (int i = inputActionRulesSingleAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesSingleAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0 ||
                        !keepActionList.Contains(prop.propertyPath))
                    { inputActionRulesSingleAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesDualAxis != null)
            {
                for (int i = inputActionRulesDualAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesDualAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0 ||
                        !keepActionList.Contains(prop.propertyPath))
                    { inputActionRulesDualAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesDualAxis != null)
            {
                for (int i = inputActionRulesVectorAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesVectorAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0 ||
                        !keepActionList.Contains(prop.propertyPath))
                    { inputActionRulesVectorAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesQuaternionAxis != null)
            {
                for (int i = inputActionRulesQuaternionAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesQuaternionAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0 ||
                        !keepActionList.Contains(prop.propertyPath))
                    { inputActionRulesQuaternionAxis.DeleteArrayElementAtIndex(i); }
                }
            }
            if (inputActionRulesPoseAxis != null)
            {
                for (int i = inputActionRulesPoseAxis.arraySize - 1; i > -1; i--)
                {
                    SerializedProperty prop = inputActionRulesPoseAxis.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == 0 ||
                        !keepActionList.Contains(prop.propertyPath))
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
            EditorGUILayout.HelpBox("The Input Action Rules are used to filter an input action to another input action based on a set criteria.", MessageType.Info);

            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            if (GUILayout.Button(AddRuleContent, EditorStyles.miniButton))
            {
                var prop = AddPropertyToList(inputActionRulesDigital, 0, (int)AxisType.None);
                actionProperties.Insert(0, new List<SerializedProperty> { prop });
                actionFoldoutBools.Insert(0, false);
                deletedActions.Insert(0, new List<bool> { false });
            }

            EditorGUILayout.Space();

            for (int groupIndex = 0; groupIndex < actionProperties.Count; groupIndex++)
            {
                var group = actionProperties[groupIndex];

                if (group.Count > 1)
                {
                    var id = group[0].FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue;
                    var content = new GUIContent($"{allActionLabels[id]}({id}) Action Group");
                    var foldout = EditorGUILayout.Foldout(actionFoldoutBools[groupIndex], content);

                    if (foldout)
                    {
                        int totalDeletedActions = 0;
                        for (int actionIndex = 0; actionIndex < group.Count; actionIndex++)
                        {
                            if (deletedActions[groupIndex][actionIndex])
                            {
                                totalDeletedActions++;
                                continue;
                            }

                            if (!RenderRule(group[actionIndex], actionIndex, group, true, groupIndex))
                            {
                                break;
                            }
                        }

                        if (totalDeletedActions == group.Count)
                        {
                            GUILayout.Space(LayoutSpace);
                        }

                        if (breakLoop)
                        {
                            break;
                        }
                    }else
                    {
                        GUILayout.Space(LayoutSpace);
                    }

                    actionFoldoutBools[groupIndex] = foldout;
                }
                else
                {
                    if (deletedActions[groupIndex][0])
                    {
                        continue;
                    }

                    if (!RenderRule(group[0], groupIndex, group))
                    {
                        break;
                    }
                }
            }

            if (breakLoop)
            {
                breakLoop = false;
            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private bool RenderRule(SerializedProperty inputActionRule, int actionIndex, List<SerializedProperty> group, bool inGroup = false, int groupIndex = -1)
        {
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
                    var oldAxis = baseAction.FindPropertyRelative("axisConstraint");
                    var newBaseActionAxis = axisConstraints[changedBaseId];

                    if (baseActionId.intValue == 0 || oldAxis.intValue != (int)newBaseActionAxis)
                    {
                        SerializedProperty list = null;
                        int ruleActionId = ruleAction.FindPropertyRelative("id").intValue;

                        list = GetListFromAxisConstraint((int)newBaseActionAxis);
                        var newProp = AddPropertyToList(list, changedBaseId, (int)newBaseActionAxis);
                        group[inGroup ? actionIndex : 0] = newProp;

                        breakLoop = true;
                        return false;
                    }

                    SetDefaultForType((int)newBaseActionAxis, criteria);
                }

                baseActionId.intValue = changedBaseId;
                var axisConstraint = (int)axisConstraints[baseActionId.intValue];
                if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                {
                    HideInputAction(actionIndex, inGroup, groupIndex);

                    breakLoop = true;
                    return false;
                }

                EditorGUILayout.EndHorizontal();

                if (baseActionId.intValue != 0)
                {
                    var ruleActionId = ruleAction.FindPropertyRelative("id");
                    var ruleActionDescription = ruleAction.FindPropertyRelative("description");

                    EditorGUIUtility.labelWidth = 64f;
                    if (CriteraChanged(baseActionId.intValue, ruleActionId.intValue, criteria, axisConstraint))
                    {
                        if (CheckDuplicate(groupIndex, actionIndex, baseActionId.intValue, ruleActionId.intValue, criteria, axisConstraint))
                        {
                            ShowDuplicateDialog(actionIndex, inGroup, groupIndex);
                        }
                    }

                    EditorGUIUtility.labelWidth = 78f;
                    var changedRuleId = EditorGUILayout.IntPopup(RuleActionContent, ruleActionId.intValue, GetLabelsFromAxis(axisConstraint), GetIdsFromAxis(axisConstraint));

                    if (changedBaseId == changedRuleId)
                    {
                        changedRuleId = 0;
                        EditorUtility.DisplayDialog("Error", "The base action and rule action can't be the same!", "OK");
                    }

                    
                    if (ruleActionId.intValue != changedRuleId)
                    {
                        if (CheckDuplicate(groupIndex, actionIndex, baseActionId.intValue, changedRuleId, criteria, axisConstraint))
                        {
                            ShowDuplicateDialog(actionIndex, inGroup, groupIndex);
                        }
                    }

                    ruleActionId.intValue = changedRuleId;
                    ruleActionDescription.stringValue = allActionLabels[changedRuleId];
                    ruleAction.FindPropertyRelative("axisConstraint").intValue = axisConstraint;

                    EditorGUIUtility.labelWidth = labelWidth;
                }

                GUILayout.Space(LayoutSpace);
            }

            return true;
        }

        private void RefreshProperties()
        {
            List<SerializedProperty> allProperties = new List<SerializedProperty>();
            SortedDictionary<int, List<SerializedProperty>> sortedProperties = new SortedDictionary<int, List<SerializedProperty>>();
            SortedDictionary<int, List<bool>> sortedDeletedActions = new SortedDictionary<int, List<bool>>();
            actionFoldoutBools = new List<bool>();
            deletedActions = new List<List<bool>>();

            for (int i = 0; i < inputActionRulesDigital?.arraySize; i++)
            { allProperties.Add(inputActionRulesDigital.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesSingleAxis?.arraySize; i++)
            { allProperties.Add(inputActionRulesSingleAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesDualAxis?.arraySize; i++)
            { allProperties.Add(inputActionRulesDualAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesVectorAxis?.arraySize; i++)
            { allProperties.Add(inputActionRulesVectorAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesQuaternionAxis?.arraySize; i++)
            { allProperties.Add(inputActionRulesQuaternionAxis.GetArrayElementAtIndex(i)); }
            for (int i = 0; i < inputActionRulesPoseAxis?.arraySize; i++)
            { allProperties.Add(inputActionRulesPoseAxis.GetArrayElementAtIndex(i)); }

            for (int i = 0; i < allProperties.Count; i++)
            {
                var prop = allProperties[i];
                var id = prop.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue;
                if (sortedProperties.ContainsKey(id))
                {
                    sortedProperties[id].Add(prop);
                    sortedDeletedActions[id].Add(false);
                }
                else
                {
                    var list = new List<SerializedProperty> { prop };
                    sortedProperties.Add(id, list);
                    sortedDeletedActions.Add(id, new List<bool>() { false });
                }
            }
            
            actionProperties = sortedProperties.Select(pair => pair.Value.ToList()).ToList();
            actionProperties.ForEach(prop => actionFoldoutBools.Add(false));
            deletedActions = sortedDeletedActions.Select(pair => pair.Value.ToList()).ToList();
        }

        private SerializedProperty GetListFromAxisConstraint(int axis)
        {
            SerializedProperty list = null;
            switch ((AxisType)axis)
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

        private SerializedProperty AddPropertyToList(SerializedProperty list, int baseActionId, int axis)
        {
            if (list == null)
            {
                return null;
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

            var criteria = newInputActionRule.FindPropertyRelative("criteria");

            baseActionIdProperty.intValue = baseActionId;
            baseActionDescription.stringValue = allActionLabels[baseActionId];
            baseActionAxisConstraint.intValue = axis;

            ruleActionIdProperty.intValue = 0;
            ruleActionDescription.stringValue = "None";
            ruleActionAxisConstraint.intValue = 0;

            SetDefaultForType(axis, criteria);

            return newInputActionRule;
        }
        
        private void HideInputAction(int action, bool inGroup = false, int groupIndex = -1)
        {
            if (inGroup)
            {
                deletedActions[groupIndex][action] = true;
            }
            else
            {
                deletedActions[action][0] = true;
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
            switch ((AxisType)axis)
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

        private void SetDefaultForType(int axis, SerializedProperty prop)
        {
            switch ((AxisType)axis)
            {
                case AxisType.Digital:
                    prop.boolValue = false;
                    break;
                case AxisType.SingleAxis:
                    prop.floatValue = 0;
                    break;
                case AxisType.DualAxis:
                    prop.vector2Value = Vector2.zero;
                    break;
                case AxisType.ThreeDofPosition:
                    prop.vector3Value = Vector3.zero;
                    break;
                case AxisType.ThreeDofRotation:
                    prop.quaternionValue = Quaternion.identity;
                    break;
                case AxisType.SixDof:
                    prop.FindPropertyRelative("position").vector3Value = Vector3.zero;
                    prop.FindPropertyRelative("rotation").quaternionValue = Quaternion.identity;
                    break;
            }
        }

        private bool CriteriaEquals(SerializedProperty c1, SerializedProperty c2, int axis)
        {
            switch((AxisType)axis)
            {
                case AxisType.Digital:
                    return c1.boolValue == c2.boolValue;
                case AxisType.SingleAxis:
                    return c1.floatValue == c2.floatValue;
                case AxisType.DualAxis:
                    return c1.vector2Value == c2.vector2Value;
                case AxisType.ThreeDofPosition:
                    return c1.vector3Value == c2.vector3Value;
                case AxisType.ThreeDofRotation:
                    return c1.quaternionValue == c2.quaternionValue;
                case AxisType.SixDof:
                    return new MixedRealityPose(c1.FindPropertyRelative("position").vector3Value, c1.FindPropertyRelative("rotation").quaternionValue)
                        .Equals(new MixedRealityPose(c2.FindPropertyRelative("position").vector3Value, c2.FindPropertyRelative("rotation").quaternionValue));
            }

            return false;
        }

        private bool CriteraChanged(int baseAction, int ruleAction, SerializedProperty criteria, int axis)
        {
            switch ((AxisType)axis)
            {
                case AxisType.Digital:
                    {
                        bool oldCritera = criteria.boolValue;
                        EditorGUILayout.PropertyField(criteria, CriteriaContent);
                        if (criteria.boolValue != oldCritera)
                        {
                            return true;
                        }
                    }
                    break;
                case AxisType.SingleAxis:
                    {
                        float oldCritera = criteria.floatValue;
                        EditorGUILayout.PropertyField(criteria, CriteriaContent);
                        if (criteria.floatValue != oldCritera)
                        {
                            return true;
                        }
                    }
                    break;
                case AxisType.DualAxis:
                    {
                        Vector2 oldCritera = criteria.vector2Value;
                        EditorGUILayout.PropertyField(criteria, CriteriaContent);
                        if (criteria.vector2Value != oldCritera)
                        {
                            return true;
                        }
                    }
                    break;
                case AxisType.ThreeDofPosition:
                    {
                        Vector3 oldCritera = criteria.vector3Value;
                        EditorGUILayout.PropertyField(criteria, CriteriaContent);
                        if (criteria.vector3Value != oldCritera)
                        {
                            return true;
                        }
                    }
                    break;
                case AxisType.ThreeDofRotation:
                    {
                        Quaternion oldCritera = criteria.quaternionValue;
                        EditorGUILayout.PropertyField(criteria, CriteriaContent);
                        if (criteria.quaternionValue != oldCritera)
                        {
                            return true;
                        }
                    }
                    break;
                case AxisType.SixDof:
                    {
                        MixedRealityPose oldCritera = new MixedRealityPose(criteria.FindPropertyRelative("position").vector3Value,
                            criteria.FindPropertyRelative("rotation").quaternionValue);

                        EditorGUILayout.PropertyField(criteria, CriteriaContent);

                        MixedRealityPose newCriteria = new MixedRealityPose(criteria.FindPropertyRelative("position").vector3Value,
                            criteria.FindPropertyRelative("rotation").quaternionValue);
                        if (newCriteria != oldCritera)
                        {
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        private bool CheckDuplicate(int selfGroup, int selfAction, int baseAction, int ruleAction, SerializedProperty criteria, int axis)
        {
            selfGroup = selfGroup == -1 ? selfAction : selfGroup;

            if (baseAction == 0 || ruleAction == 0)
            {
                return false;
            }

            for (int g = 0; g < actionProperties.Count; g++)
            {
                var group = actionProperties[g];
                for (int a = 0; a < group.Count; a++)
                {
                    if ((selfGroup == g && selfAction == a) || deletedActions[g][a])
                    {
                        continue;
                    }

                    var action = group[a];
                    if (action.FindPropertyRelative("baseAction").FindPropertyRelative("id").intValue == baseAction &&
                        action.FindPropertyRelative("ruleAction").FindPropertyRelative("id").intValue == ruleAction &&
                        CriteriaEquals(action.FindPropertyRelative("criteria"), criteria, axis))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ShowDuplicateDialog(int actionIndex, bool inGroup, int groupIndex)
        {
            if (!EditorUtility.DisplayDialog("Duplicate detected", "A duplicate input rule action has been found.", "OK", "Remove"))
            {
                HideInputAction(actionIndex, inGroup, groupIndex);
            }
        }
    }
}