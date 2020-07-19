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
using System.Collections.Generic;

using Microsoft.RuleEngine;
using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;

namespace Microsoft.BizTalk.CAT.BestPractices.Framework.RulesEngine
{
    public static class PolicyHelper
    {
        public static bool IsDeployed(string policyName)
        {
            Guard.ArgumentNotNullOrEmptyString(policyName, "policyName");

            var callToken = TraceManager.RulesComponent.TraceIn(policyName);
            bool deployed = false;
            
            try
            {
                using (Policy policy = new Policy(policyName))
                {
                    deployed = true;
                }
            }
            catch (Exception ex)
            {
                TraceManager.RulesComponent.TraceError(ex);
                deployed = false;
            }

            TraceManager.RulesComponent.TraceOut(callToken, deployed);
            return deployed;
        }

        public static bool IsDeployed(string policyName, Version version)
        {
            Guard.ArgumentNotNullOrEmptyString(policyName, "policyName");
            Guard.ArgumentNotNull(version, "version");

            var callToken = TraceManager.RulesComponent.TraceIn(policyName, version);
            bool deployed = false;

            try
            {
                using (Policy policy = new Policy(policyName, version.Major, version.Minor))
                {
                    deployed = true;
                }
            }
            catch (Exception ex)
            {
                TraceManager.RulesComponent.TraceError(ex);
                deployed = false;
            }

            TraceManager.RulesComponent.TraceOut(callToken, deployed);
            return deployed;
        }

        public static Version GetVersion(string policyName)
        {
            Guard.ArgumentNotNullOrEmptyString(policyName, "policyName");
            
            var callToken = TraceManager.RulesComponent.TraceIn(policyName);
            Version version = null;

            try
            {
                using (Policy policy = new Policy(policyName))
                {
                    version = new Version(policy.MajorRevision, policy.MinorRevision);
                }
            }
            catch (Exception ex)
            {
                TraceManager.RulesComponent.TraceError(ex);
                version = new Version(0, 0);
            }

            TraceManager.RulesComponent.TraceOut(callToken, version);
            return version;
        }

        public static PolicyExecutionResult Execute(PolicyExecutionInfo policyExecInfo, params object[] facts)
        {
            Guard.ArgumentNotNull(policyExecInfo, "policyExecInfo");
            Guard.ArgumentNotNullOrEmptyString(policyExecInfo.PolicyName, "policyExecInfo.PolicyName");

            var callToken = TraceManager.RulesComponent.TraceIn(policyExecInfo.PolicyName);

            Version policyVersion = policyExecInfo.PolicyVersion;            
            PolicyExecutionResult execResult = new PolicyExecutionResult(policyExecInfo.PolicyName, false);

            PolicyFetchErrorHandler errorHandler = delegate(Exception ex)
            {
                TraceManager.RulesComponent.TraceError(ex);
                execResult.Errors.Add(ex); 
            };

            try
            {
                using (Policy policy = policyVersion != null ? new Policy(policyExecInfo.PolicyName, policyVersion.Major, policyVersion.Minor, errorHandler) : new Policy(policyExecInfo.PolicyName, errorHandler))
                {
                    List<object> agendaFacts = new List<object>(facts);
                    agendaFacts.Add(policyExecInfo);

                    using (TracingRuleTrackingInterceptor trackingInterceptor = new TracingRuleTrackingInterceptor())
                    {
                        // Write the Start event to measure how long it takes to execute the BRE policy.
                        var scopeStarted = TraceManager.RulesComponent.TraceStartScope(policyExecInfo.PolicyName, policyVersion);

                        policy.Execute(agendaFacts.ToArray(), trackingInterceptor);

                        // Once finished, write the End event along with calculated duration.
                        TraceManager.RulesComponent.TraceEndScope(policyExecInfo.PolicyName, scopeStarted);
                    }

                    execResult = new PolicyExecutionResult(policyExecInfo.PolicyName, policy.MajorRevision, policy.MinorRevision);
                }
            }
            catch (Exception ex)
            {
                TraceManager.RulesComponent.TraceError(ex);
                execResult.Errors.Add(ex);
            }

            TraceManager.RulesComponent.TraceOut(callToken, execResult.PolicyName, execResult.PolicyVersion, execResult.Success);
            return execResult;
        }
    }
}
