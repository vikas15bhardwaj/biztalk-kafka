using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.Threading;

namespace KafkaAdapter.Components
{
    /// <summary>
    /// Captures all methods needed sending or receiving messages to Kafka
    /// </summary>
    public interface IKafkaConsumer : IDisposable
    {
        KafkaConsumerConfig Config { get; }
        /// <summary>
        /// starts a consumer subscription for the topic.
        /// </summary>
        void Start(Func<KafkaMessage, Task> messageReceiveDelagate, ManualResetEvent cancelledEvent, CancellationTokenSource cts);
        /// <summary>
        /// commit offset to kafka for the topic and partition.
        /// </summary>
        /// <returns>returns kafka message</returns>
        void Commit(List<TopicPartitionOffset> offset);

    }
}
