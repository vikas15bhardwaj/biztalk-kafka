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
using System.Text;

namespace Microsoft.BizTalk.CAT.BestPractices.Framework.RulesEngine
{
    [Serializable]
    public sealed class PolicyExecutionResult
    {
        private readonly string policyName;
        private readonly Version policyVersion;
        private readonly bool success;
        private readonly IList<Exception> errors;

        private PolicyExecutionResult(string policyName)
        {
            this.policyName = policyName;
            this.errors = new List<Exception>();
        }

        public PolicyExecutionResult(string policyName, bool success) : this(policyName)
        {
            this.success = success;
        }

        public PolicyExecutionResult(string policyName, int majorRevision, int minorRevision) : this(policyName)
        {
            this.policyVersion = new Version(majorRevision, minorRevision);
            this.success = true;
        }

        public PolicyExecutionResult(string policyName, Exception ex) : this(policyName)
        {
            this.success = false;
            this.errors.Add(ex);
        }

        public string PolicyName
        {
            get { return this.policyName; }
        }

        public Version PolicyVersion
        {
            get { return this.policyVersion; }
        }

        public bool Success
        {
            get { return this.success; }
        }

        public IList<Exception> Errors
        {
            get { return this.errors; }
        } 
    }
}
