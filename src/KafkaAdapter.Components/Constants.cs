using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public static class Constants
    {
        public const int DefaultAssignedPartition = -1; //-1 means none
        public const int DefaultAssignedOffset = -1; //-1 means none
        public const int DefaultMessageTimeout = 10000; //ms
        public const int DefaultBatchSize = 20; //Kafka API batch size
        public const int DefaultBizTalkMessagingEngineTransmitBatchSize = 50; //This is the batch size BizTalk engine submits messages to transmit adapter.

        public const string DefaultAck = "leader"; //from all leaders
        public const string DefaultAutoOffsetReset = "latest"; //being with latest offset.
        public const int DefaultMessageMaxSizeMb = 4;
    }
}
