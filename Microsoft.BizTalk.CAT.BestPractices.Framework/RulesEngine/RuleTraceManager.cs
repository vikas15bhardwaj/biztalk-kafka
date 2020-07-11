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

using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;

namespace Microsoft.BizTalk.CAT.BestPractices.Framework.RulesEngine
{
    public static class RuleTraceManager
    {
        #region TraceInfo methods
        public static void TraceInfo(string message)
        {
            TraceManager.RulesComponent.TraceInfo(message);
        }

        public static void TraceInfo(string format, object p1)
        {
            TraceManager.RulesComponent.TraceInfo(format, p1);
        }

        public static void TraceInfo(string format, object p1, object p2)
        {
            TraceManager.RulesComponent.TraceInfo(format, p1, p2);
        }

        public static void TraceInfo(string format, object p1, object p2, object p3)
        {
            TraceManager.RulesComponent.TraceInfo(format, p1, p2, p3);
        }

        public static void TraceInfo(string format, object p1, object p2, object p3, object p4)
        {
            TraceManager.RulesComponent.TraceInfo(format, p1, p2, p3, p4);
        }

        public static void TraceInfo(string format, object p1, object p2, object p3, object p4, object p5)
        {
            TraceManager.RulesComponent.TraceInfo(format, p1, p2, p3, p4, p5);
        }

        public static void TraceInfo(string format, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            TraceManager.RulesComponent.TraceInfo(format, p1, p2, p3, p4, p5, p6);
        } 
        #endregion

        #region TraceIn methods
        public static void TraceIn(string message)
        {
            TraceManager.RulesComponent.TraceIn(message);
        }

        public static void TraceIn(object p1)
        {
            TraceManager.RulesComponent.TraceIn(p1);
        }

        public static void TraceIn(object p1, object p2)
        {
            TraceManager.RulesComponent.TraceIn(p1, p2);
        }

        public static void TraceIn(object p1, object p2, object p3)
        {
            TraceManager.RulesComponent.TraceIn(p1, p2, p3);
        }

        public static void TraceIn(object p1, object p2, object p3, object p4)
        {
            TraceManager.RulesComponent.TraceIn(p1, p2, p3, p4);
        }

        public static void TraceIn(object p1, object p2, object p3, object p4, object p5)
        {
            TraceManager.RulesComponent.TraceIn(p1, p2, p3, p4, p5);
        }

        public static void TraceIn(object p1, object p2, object p3, object p4, object p5, object p6)
        {
            TraceManager.RulesComponent.TraceIn(p1, p2, p3, p4, p5, p6);
        }
        
        #endregion

        #region TraceError methods
        public static void TraceError(string message)
        {
            TraceManager.RulesComponent.TraceError(message);
        }

        public static void TraceError(string format, object p1)
        {
            TraceManager.RulesComponent.TraceError(format, p1);
        }

        public static void TraceError(string format, object p1, object p2)
        {
            TraceManager.RulesComponent.TraceError(format, p1, p2);
        }

        public static void TraceError(string format, object p1, object p2, object p3)
        {
            TraceManager.RulesComponent.TraceError(format, p1, p2, p3);
        }

        public static void TraceError(string format, object p1, object p2, object p3, object p4)
        {
            TraceManager.RulesComponent.TraceError(format, p1, p2, p3, p4);
        }

        public static void TraceError(string format, object p1, object p2, object p3, object p4, object p5)
        {
            TraceManager.RulesComponent.TraceError(format, p1, p2, p3, p4, p5);
        }

        public static void TraceError(string format, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            TraceManager.RulesComponent.TraceError(format, p1, p2, p3, p4, p5, p6);
        } 
        #endregion
    }
}
