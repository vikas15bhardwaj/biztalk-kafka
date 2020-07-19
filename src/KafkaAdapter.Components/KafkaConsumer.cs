using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public class KafkaConsumer : IKafkaConsumer
    {
        IConsumer<string, byte[]> _consumer;
        IKafkaAdmin _admin;

        public KafkaConsumerConfig Config { get; internal set; }

        internal KafkaConsumer(KafkaConsumerConfig configProperties)
        {

            Config = configProperties;
            var config = KafkaClientConfig.GetConfig<ConsumerConfig>(configProperties);

            AutoOffsetReset offsetReset;
            if (Enum.TryParse<AutoOffsetReset>(configProperties.AutoOffsetReset, true, out offsetReset))
                config.AutoOffsetReset = offsetReset;

            config.GroupId = configProperties.GroupId;
            config.EnableAutoCommit = false;
            config.FetchMaxBytes = configProperties.MessageMaxSizeMb * 1024 * 1024;
            config.ReceiveMessageMaxBytes = config.FetchMaxBytes + 1024;
            var builder = new ConsumerBuilder<string, byte[]>(config);

            builder.SetLogHandler((producer, message) => 
            {
                Trace.Logger.TraceInfo(message.Message);
                Trace.WriteToEventLog(message, "Consumer");
            });

            _consumer = builder.Build();
            _admin = KafkaAdminFactory.GetAdmin(this.Config);
        }

        public async void Start(Func<KafkaMessage, Task> messageReceiveDelagate, ManualResetEvent cancelledEvent, CancellationTokenSource cts)
        {
            try
            {
                //get the topic partition offset based on config. if there are specific partition offset, then subscribe to everything
                //otherwise use specific topic partition offset.
                var ktpo = GetTopicPartitionOffset();
                if (ktpo == null)
                    _consumer.Subscribe(this.Config.Topic);
                else
                    _consumer.Assign(ktpo);

                while (true)
                {
                    try
                    {
                        Trace.Logger.TraceInfo("KafkaConsumer.Start waiting for messages");
                        var cr = _consumer.Consume(cts.Token);
                        var message = new KafkaMessage
                        {
                            Key = cr.Key,
                            Message = cr.Message.Value,
                            TopicPartitionOffset = new TopicPartitionOffset
                            {
                                Offset = cr.Offset.Value,
                                Partition = cr.Partition.Value,
                                Topic = cr.Topic
                            }
                        };
                        await messageReceiveDelagate(message);
                    }
                    catch (ConsumeException e)
                    {
                        Trace.Logger.TraceError(e, true);
                        throw;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Trace.Logger.TraceInfo("KafkaConsumer.Start unsubscribing the current topic set");
                _consumer.Unsubscribe();
                cancelledEvent.Set();
            }
        }

        public void Commit(List<TopicPartitionOffset> topicPartitionOffsets)
        {
            try
            {
                var offset = topicPartitionOffsets.GroupBy(t => new { t.Topic, t.Partition })
                               .Select(t => new Confluent.Kafka.TopicPartitionOffset
                                                                    (t.Key.Topic,
                                                                    new Partition(t.Key.Partition),
                                                                    new Offset(t.Max(t1 => t1.Offset) + 1))
                                                                    );

                _consumer.Commit(offset);
            }
            catch (TopicPartitionOffsetException tpx)
            {
                Trace.Logger.TraceInfo("KafkaConsumer.Commit handling TopicPartitionOffsetException");
                if (tpx.Results != null)
                {
                    StringBuilder error = new StringBuilder("Error in committing offset, duplicate messages may be submitted");
                    foreach (var tp in tpx.Results)
                        error.AppendLine($"{tp.TopicPartitionOffset?.Topic}{tp.TopicPartitionOffset?.Partition.Value}/{tp.TopicPartitionOffset?.Offset.Value}: {tp.Error?.ToString()}");

                    throw new KafkaException(error.ToString());
                }
            }
            catch (Confluent.Kafka.KafkaException kx)
            {
                Trace.Logger.TraceInfo("KafkaConsumer.Commit handling Confluent.Kafka.KafkaException");
                throw new KafkaException($"Error in committing offset, duplicate messages may be submitted {kx.Error?.ToString()}: {kx.ToString()}");
            }

        }

        public void Dispose()
        {
            if (_admin != null)
                KafkaAdminFactory.Close(Config);

            if (_consumer != null)
                _consumer.Close();

        }


        private List<Confluent.Kafka.TopicPartitionOffset> GetTopicPartitionOffset()
        {
            Trace.Logger.TraceInfo("Entering KafkaConsumer.GetTopicPartitionOffset");
            var partitionIds = this.Config.PartitionIds;
            var partitionKeys = this.Config.PartitionKeys;
            var offset = this.Config.Offset;
            List<Confluent.Kafka.TopicPartitionOffset> ktpos = null;

            //if partition ids are not provided, check if keys are supplied and then get ids from keys
            if((partitionIds == null || partitionIds.Count() <= 0) && partitionKeys != null && partitionKeys.Count() > 0)
            {
                var partitionIdsForKeys = _admin.GetPartitionIds(Config.Topic, partitionKeys);
                partitionIds = (from p in partitionKeys
                               select partitionIdsForKeys[p]).ToList();
            }
            //if got some partition ids, create TopicPartitionOffset. if offset are supplied use them otherwise assign unset offset
            if (partitionIds != null && partitionIds.Count() > 0)
            {
                ktpos = new List<Confluent.Kafka.TopicPartitionOffset>();
                for (int i = 0; i < partitionIds.Count; i++)
                {
                    Confluent.Kafka.TopicPartitionOffset ktpo;
                    if (offset != null && offset.Count > 0)
                        ktpo = new Confluent.Kafka.TopicPartitionOffset(Config.Topic, new Partition(partitionIds[i]), new Offset(offset[i]));
                    else
                        ktpo = new Confluent.Kafka.TopicPartitionOffset(Config.Topic, new Partition(partitionIds[i]), Offset.Unset);

                    ktpos.Add(ktpo);
                }
            }

            Trace.Logger.TraceInfo("Leaving KafkaConsumer.GetTopicPartitionOffset");
            return ktpos;

        }
    }
}
