//=================================================================================
// Microsoft BizTalk CAT Team Best Practices Samples
//
// The Framework library is a set of general best practices for BizTalk developers.
//
//=================================================================================
// Copyright © Microsoft Corporation. All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=================================================================================
using System;
using System.Globalization;

using Microsoft.RuleEngine;
using Microsoft.BizTalk.CAT.BestPractices.Framework.Properties;
using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;

namespace Microsoft.BizTalk.CAT.BestPractices.Framework.RulesEngine
{
    public sealed class TracingRuleTrackingInterceptor : IRuleSetTrackingInterceptor, IDisposable
    {
        #region Private members
        // Preload all string resource for better performance.
        private static string traceHeaderTrace = TraceLogMessages.Header;
        private static string ruleEngineInstanceTrace = TraceLogMessages.RuleEngineInstance;
        private static string rulesetNameTrace = TraceLogMessages.RulesetName;
        private static string ruleFiredTrace = TraceLogMessages.RuleFired;
        private static string ruleNameTrace = TraceLogMessages.RuleName;
        private static string conflictResolutionCriteriaTrace = TraceLogMessages.ConflictResolutionCriteria;
        private static string workingMemoryUpdateTrace = TraceLogMessages.WorkingMemoryUpdate;
        private static string operationTypeTrace = TraceLogMessages.OperationType;
        private static string assertOperationTrace = TraceLogMessages.AssertOperation;
        private static string retractOperationTrace = TraceLogMessages.RetractOperation;
        private static string updateOperationTrace = TraceLogMessages.UpdateOperation;
        private static string assertUnrecognizedOperationTrace = TraceLogMessages.AssertUnrecognizedOperation;
        private static string retractUnrecognizedOperationTrace = TraceLogMessages.RetractUnrecognizedOperation;
        private static string updateUnrecognizedOperationTrace = TraceLogMessages.UpdateUnrecognizedOperation;
        private static string retractNotPresentOperationTrace = TraceLogMessages.RetractNotPresentOperation;
        private static string updateNotPresentOperationTrace = TraceLogMessages.UpdateNotPresentOperation;
        private static string unrecognizedOperationTrace = TraceLogMessages.UnrecognizedOperation;
        private static string objectTypeTrace = TraceLogMessages.ObjectType;
        private static string objectInstanceTrace = TraceLogMessages.ObjectInstance;
        private static string conditionEvaluationTrace = TraceLogMessages.ConditionEvaluation;
        private static string testExpressionTrace = TraceLogMessages.TestExpression;
        private static string leftOperandValueTrace = TraceLogMessages.LeftOperandValue;
        private static string rightOperandValueTrace = TraceLogMessages.RightOperandValue;
        private static string testResultTrace = TraceLogMessages.TestResult;
        private static string agendaUpdateTrace = TraceLogMessages.AgendaUpdate;
        private static string addOperationTrace = TraceLogMessages.AddOperation;
        private static string removeOperationTrace = TraceLogMessages.RemoveOperation;

        private const string TraceTemplateOneParam = "RULE TRACE: {0} null";
        private const string TraceTemplateTwoParam = "RULE TRACE: {0} {1}";
        private const string TraceTemplateThreeParam = "RULE TRACE: {0} {1} {2}";
        private const string TraceTemplateObjTypeEval = "RULE TRACE: {0} {1} {2} ({3})";
        private const string TraceTemplateValueTypeEval = "RULE TRACE: {0} {1} ({2})";

        private TrackingConfiguration trackingConfig;
        private string ruleSetName;
        private string ruleEngineGuid;
        #endregion

        #region IRuleSetTrackingInterceptor members
        public void SetTrackingConfig(TrackingConfiguration trackingConfig)
        {
            this.trackingConfig = trackingConfig;
        }

        public void TrackAgendaUpdate(bool isAddition, string ruleName, object conflictResolutionCriteria)
        {
            PrintHeader(agendaUpdateTrace);

            if (isAddition)
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, addOperationTrace);
            }
            else
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, removeOperationTrace);
            }

            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, ruleNameTrace, ruleName);

            if (conflictResolutionCriteria == null)
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateOneParam, conflictResolutionCriteriaTrace);
            }
            else
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, conflictResolutionCriteriaTrace, conflictResolutionCriteria);
            }
        }

        public void TrackConditionEvaluation(string testExpression, string leftClassType, int leftClassInstanceId, object leftValue, string rightClassType, int rightClassInstanceId, object rightValue, bool result)
        {
            PrintHeader(conditionEvaluationTrace);

            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, testExpressionTrace, testExpression);

            if (leftValue == null)
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateOneParam, leftOperandValueTrace);
            }
            else
            {
                Type leftValueType = leftValue.GetType();

                if (leftValueType.IsClass && (Type.GetTypeCode(leftValueType) != TypeCode.String))
                {
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateObjTypeEval, leftOperandValueTrace, objectInstanceTrace, leftValue.GetHashCode().ToString(CultureInfo.CurrentCulture), leftValueType.FullName);
                }
                else
                {
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateValueTypeEval, leftOperandValueTrace, leftValue, leftValueType.FullName);
                }
            }

            if (rightValue == null)
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateOneParam, rightOperandValueTrace);
            }
            else
            {
                Type rightValueType = rightValue.GetType();

                if (rightValueType.IsClass && (Type.GetTypeCode(rightValueType) != TypeCode.String))
                {
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateObjTypeEval, rightOperandValueTrace, objectInstanceTrace, rightValue.GetHashCode().ToString(CultureInfo.CurrentCulture), rightValueType.FullName);
                }
                else
                {
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateValueTypeEval, rightOperandValueTrace, rightValue, rightValueType.FullName);
                }
            }

            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, testResultTrace, result);
        }

        public void TrackFactActivity(FactActivityType activityType, string classType, int classInstanceId)
        {
            PrintHeader(workingMemoryUpdateTrace);

            switch (activityType)
            {
                case FactActivityType.Assert:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, assertOperationTrace);
                    break;

                case FactActivityType.Retract:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, retractOperationTrace);
                    break;

                case FactActivityType.Update:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, updateOperationTrace);
                    break;

                case FactActivityType.AssertUnrecognized:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, assertUnrecognizedOperationTrace);
                    break;

                case FactActivityType.RetractUnrecognized:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, retractUnrecognizedOperationTrace);
                    break;

                case FactActivityType.UpdateUnrecognized:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, updateUnrecognizedOperationTrace);
                    break;

                case FactActivityType.RetractNotPresent:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, retractNotPresentOperationTrace);
                    break;

                case FactActivityType.UpdateNotPresent:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, updateNotPresentOperationTrace);
                    break;

                default:
                    TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, operationTypeTrace, unrecognizedOperationTrace);
                    break;
            }

            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, objectTypeTrace, classType);
            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, objectInstanceTrace, classInstanceId.ToString(CultureInfo.CurrentCulture));
        }

        public void TrackRuleFiring(string ruleName, object conflictResolutionCriteria)
        {
            PrintHeader(ruleFiredTrace);

            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, ruleNameTrace, ruleName);

            if (conflictResolutionCriteria == null)
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateOneParam, conflictResolutionCriteriaTrace);
            }
            else
            {
                TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, conflictResolutionCriteriaTrace, conflictResolutionCriteria);
            }

        }

        public void TrackRuleSetEngineAssociation(RuleSetInfo ruleSetInfo, Guid ruleEngineGuid)
        {
            this.ruleSetName = ruleSetInfo.Name;
            this.ruleEngineGuid = ruleEngineGuid.ToString();

            TraceManager.RulesComponent.TraceInfo(TraceTemplateThreeParam, traceHeaderTrace, this.ruleSetName, DateTime.Now.ToString(CultureInfo.CurrentCulture));
        }
        #endregion

        #region IDisposable members
        public void Dispose()
        {
            // We don't really need to do anything here.
        }
        #endregion

        #region Private members
        private void PrintHeader(string hdr)
        {
            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, hdr, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, ruleEngineInstanceTrace, this.ruleEngineGuid);
            TraceManager.RulesComponent.TraceInfo(TraceTemplateTwoParam, rulesetNameTrace, this.ruleSetName);
        }
        #endregion
    }
}
