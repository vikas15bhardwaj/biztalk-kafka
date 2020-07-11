using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using Confluent.Kafka;


namespace KafkaAdapter.Components
{
    /// <summary>
    /// Captures all methods needed sending or receiving messages to Kafka
    /// </summary>
    public interface IKafkaProducer : IDisposable
    {
        KafkaProducerConfig Config { get; }
        /// <summary>
        /// publish a message on specific topic and partition asynchronously.
        /// </summary>
        ///<param name="messageId">Message Id</param>
        /// <param name="topic">kafka topic</param>
        /// <param name="partitionKey">partition key, use as message key to decide partition.</param>
        /// <param name="message">message content</param>
        /// <returns>task to control the wait on asynchronous</returns>
        Task Publish(string messageId, string topic, string partitionKey, byte[] message);

        /// <summary>
        /// This method sends message to a topic and returns the topic offset and partition
        /// </summary>
        /// <param name="messageId">Message Id</param>
        /// <param name="topic">kafka topic</param>
        /// <param name="partitionKey">partition key, use as message key to decide partition.</param>
        /// <param name="message">message content</param>
        /// <returns>returns topic offset and parition of the message</returns>
        Task<TopicPartitionOffset> PublishAsync(string messageId, string topic, string partitionKey, byte[] message);

        ProduceResult PublishSync(string messageId, string topic, string partitionKey, byte[] message);
    }
}
