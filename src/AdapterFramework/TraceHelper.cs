using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterFramework
{
    internal static class TraceHelper
    {
        public static IComponentTraceProvider Logger = TraceManager.Create(new Guid("A2FB8D4E-FBA5-4188-992B-8807231E8E2C"));
    }
}
