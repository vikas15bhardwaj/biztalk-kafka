using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{

    public class TopicPartitionOffset
    {
        public long Offset { get; set; }
        public int Partition { get; set; }
        public string Topic { get; set; }

    }
    public class KafkaMessage
    {
        public string Key { get; set; }
        public byte[] Message { get; set; }

        public TopicPartitionOffset TopicPartitionOffset { get; set; }
    }

    public class ProduceResult
    {
        public bool IsError { get; set; }
        public string Error { get; set; }
        public TopicPartitionOffset TopicPartitionOffset { get; set; }
        public string MessageId { get; set; }
    }
}
