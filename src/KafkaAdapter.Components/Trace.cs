using Confluent.Kafka;
using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public class Trace
    {
        public static IComponentTraceProvider Logger = TraceManager.Create(new Guid("A2FB8D4E-FBA5-4188-992B-8807231E8E2C"));
        const string EventSource = "BizTalk Server Kafka Adapter";
        internal static void WriteToEventLog(LogMessage message, string from)
        {
            string msg = $"{from}:{message.Level}:{message.Message}:{message.Facility}";
            switch (message.Level)
            {
                case SyslogLevel.Alert:
                case SyslogLevel.Critical:
                case SyslogLevel.Emergency:
                case SyslogLevel.Error:
                    EventLog.WriteEntry(EventSource, msg, EventLogEntryType.Error, 1001);
                    break;
                case SyslogLevel.Warning:
                    EventLog.WriteEntry(EventSource, msg, EventLogEntryType.Warning, 1002);
                    break;
            }
        }

        public static void WriteToEventLog(Exception ex, string from)
        {
            string msg = $"{from}:{ex.ToString()}";
            EventLog.WriteEntry("BizTalk Server Kafka Adapter", msg, EventLogEntryType.Error, 1001);
        }
    }
}
