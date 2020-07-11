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
    public interface IKafkaAdmin : IDisposable
    {
        KafkaConfig Config { get; }
        /// <summary>
        /// get the partition count for a topic.
        /// </summary>
        /// <param name="topic">kafka topic</param>
        /// <returns>count of partitions</returns>
        int GetTopicPartitionCount(string topic);

        /// <summary>
        /// Get Parition Id for a partition key of a topic
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="partitionKey">partition key to map to a partition id</param>
        /// <returns>returns partition id</returns>
        int GetPartitionId(string topic, string partitionKey);

        /// <summary>
        /// Gets the partition ids for given partition keys
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="partitionKeys">list of keys to map</param>
        /// <returns></returns>
        Dictionary<string, int> GetPartitionIds(string topic, List<string> partitionKeys);

    }
}
