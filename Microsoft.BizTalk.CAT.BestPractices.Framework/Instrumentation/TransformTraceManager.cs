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
namespace Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation
{
    #region Using references
    using System;

    using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;
    #endregion

    /// <summary>
    /// This helper class is intended to be used from inside the BizTalk maps. The BizTalk mapper only supports instance methods when
    /// calling external assemblies. This class relays all calls to a static instance of TraceManager.TransformComponent. The class also
    /// helps map the XSLT string data type into strongly typed parameters (such as Guid) expected by the tracing APIs.
    /// </summary>
    public sealed class TransformTraceManager
    {
        #region Constructor
        public TransformTraceManager()
        {
        }
        #endregion

        #region Public methods
        public string TraceIn(string message)
        {
            return TraceManager.TransformComponent.TraceIn(message).ToString();
        }

        public void TraceOut(string guidString, string message)
        {
            TraceManager.TransformComponent.TraceOut(new Guid(guidString), message);
        }

        public void TraceInfo(string message)
        {
            TraceManager.TransformComponent.TraceInfo(message);
        }

        public long TraceStartScope(string scope, string guidString)
        {
            return TraceManager.TransformComponent.TraceStartScope(scope, new Guid(guidString));
        }

        public void TraceEndScope(string scope, long startTicks, string guidString)
        {
            TraceManager.TransformComponent.TraceEndScope(scope, startTicks, new Guid(guidString));
        }
        #endregion
    }
}
