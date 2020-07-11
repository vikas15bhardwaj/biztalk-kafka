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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.BizTalk.CAT.BestPractices.Framework.RulesEngine
{
    [Serializable]
    public sealed class PolicyExecutionInfo
    {
        #region Private members
        private readonly IDictionary<string, object> parameters;
        private readonly string policyName;

        /// <summary>
        /// The policy version number. If version is not provided, the latest version will be invoked.
        /// </summary>
        private readonly Version policyVersion;
        #endregion

        #region Constructors
        public PolicyExecutionInfo(string policyName) : this(policyName, null)
        {
        }

        public PolicyExecutionInfo(string policyName, Version policyVersion)
        {
            Guard.ArgumentNotNullOrEmptyString(policyName, "policyName");

            this.policyName = policyName;
            this.policyVersion = policyVersion;
            this.parameters = new Dictionary<string, object>();
        }
        #endregion

        #region Public properties
        public string PolicyName
        {
            get { return this.policyName; }
        }

        public Version PolicyVersion
        {
            get { return this.policyVersion; }
        }
        #endregion

        #region Public methods
        public object GetParameter(string name)
        {
            Guard.ArgumentNotNullOrEmptyString(policyName, "name");

            object value = null;

            this.parameters.TryGetValue(name, out value);
            return value;
        }

        public void AddParameter(string name, object value)
        {
            Guard.ArgumentNotNullOrEmptyString(policyName, "name");

            this.parameters[name] = value;
        }

        public void AddParameters(NameValueCollection parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                foreach (string key in parameters.Keys)
                {
                    AddParameter(key, parameters[key]);
                }
            }
        }
        #endregion
    }
}