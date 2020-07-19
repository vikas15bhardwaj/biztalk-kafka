using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public class KafkaAdmin : IKafkaAdmin
    {
        IAdminClient _adminClient;
        public KafkaConfig Config { get; internal set; }

        internal KafkaAdmin(KafkaConfig configProperties)
        {

            Config = Config;

            var config = KafkaClientConfig.GetConfig<AdminClientConfig>(configProperties);

            var builder = new AdminClientBuilder(config);

            builder.SetLogHandler((adminClient, message) =>
            {
                Trace.Logger.TraceInfo(message.Message);
            });

            _adminClient = builder.Build();

        }
        public int GetPartitionId(string topic, string partitionKey)
        {
            int totalNoOfPartitions = GetTopicPartitionCount(topic);

            return GetPartitionId(totalNoOfPartitions, topic, partitionKey);
        }
        public Dictionary<string, int> GetPartitionIds(string topic, List<string> partitionKeys)
        {
            int totalNoOfPartitions = GetTopicPartitionCount(topic);
            Dictionary<string, int> partitionIds = new Dictionary<string, int>();
            foreach (var partitionKey in partitionKeys)
                partitionIds.Add(partitionKey, GetPartitionId(totalNoOfPartitions, topic, partitionKey));

            return partitionIds;

        }
        public int GetTopicPartitionCount(string topic)
        {
            var topicMetaData = _adminClient.GetMetadata(topic, new TimeSpan(0, 1, 0));
            if (topicMetaData?.Topics?[0].Error?.IsError == true)
            {
                var exception = topicMetaData?.Topics?[0].Error;
                throw new Exception($"Unable to get topic details: {exception.Reason} + {exception.Code} + {exception.ToString()}");
            }
            else
                return topicMetaData.Topics[0].Partitions.Count();
        }

        public void Dispose()
        {
            if (_adminClient != null)
                _adminClient.Dispose();
        }

        private int GetPartitionId(int totalNoOfPartitions, string topic, string partitionKey)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(partitionKey));

                return hash.Select(b => Convert.ToInt32(b)).Aggregate((b1, b2) => b1 + b2) % totalNoOfPartitions;
            }
        }

    }
}
