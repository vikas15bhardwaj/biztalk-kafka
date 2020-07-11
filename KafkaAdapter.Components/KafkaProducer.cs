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
    public class KafkaProducer : IKafkaProducer
    {
        IProducer<string, byte[]> _producer;
        public KafkaProducerConfig Config { get; internal set; }
        IKafkaAdmin _admin;

        internal KafkaProducer(KafkaProducerConfig configProperties)
        {
            Config = configProperties;

            var config = KafkaClientConfig.GetConfig<ProducerConfig>(configProperties);

            Acks kafkaAcks;
            if (Enum.TryParse<Acks>(configProperties.Acks, true, out kafkaAcks))
                config.Acks = kafkaAcks;

            config.MessageSendMaxRetries = 0;
            config.MessageTimeoutMs = configProperties.MessageTimeout;
            config.BatchNumMessages = configProperties.BatchSize;
            config.MessageMaxBytes = configProperties.MessageMaxSizeMb * 1024 * 1024;
            var builder = new ProducerBuilder<string, byte[]>(config);

            builder.SetLogHandler((producer, message) => 
            {
                Trace.Logger.TraceInfo(message.Message);
                Trace.WriteToEventLog(message, "Producer");
            });

            _producer = builder.Build();
            _admin = KafkaAdminFactory.GetAdmin(Config);
        }

        public ProduceResult PublishSync(string messageId, string topic, string partitionKey, byte[] message)
        {
            ManualResetEvent waitForResponse = new ManualResetEvent(false);
            ProduceResult pr = null;
            try
            {
                Headers headers = new Headers();
                headers.Add(new Header("messageid", Encoding.UTF8.GetBytes(messageId)));
                
                
                if (String.IsNullOrEmpty(partitionKey))
                {
                    _producer.Produce(topic, new Message<string, byte[]> { Key = null, Value = message, Headers = headers }, (result) =>
                    {
                        pr = ProduceCallBack(result, messageId);
                        waitForResponse.Set();
                    });
                }
                else
                {
                    _producer.Produce(
                        new TopicPartition(topic, new Partition(_admin.GetPartitionId(topic, partitionKey))),
                        new Message<string, byte[]> { Key = null, Value = message, Headers = headers }, (result) =>
                        {
                            pr = ProduceCallBack(result, messageId);
                            waitForResponse.Set();
                        });
                }

                waitForResponse.WaitOne();
            }
            catch (ProduceException<string, byte[]> e)
            {
                Trace.Logger.TraceError(e, true);
                pr = HandleException(e, messageId);
            }
            catch (Exception ex)
            {
                Trace.Logger.TraceError(ex, true);
                pr = new ProduceResult
                {
                    MessageId = messageId,
                    IsError = true,
                    Error = $"unknown exception occurred while sending message to kafka {messageId} {ex.ToString()}"
                };
            }
            finally
            {
                waitForResponse.Reset();
                waitForResponse.Dispose();
            }

            return pr;
        }
        public Task Publish(string messageId, string topic, string partitionKey, byte[] message)
        {
            Headers headers = new Headers();
            headers.Add(new Header("messageid", Encoding.UTF8.GetBytes(messageId)));
            
            if (String.IsNullOrEmpty(partitionKey))
                return _producer.ProduceAsync(topic, new Message<string, byte[]> { Key = null, Value = message, Headers = headers });
            else
            {
                return _producer.ProduceAsync(
                    new TopicPartition(topic, new Partition(_admin.GetPartitionId(topic, partitionKey))),
                    new Message<string, byte[]> { Key = null, Value = message, Headers = headers });
            }

        }

        public async Task<TopicPartitionOffset> PublishAsync(string messageId, string topic, string partitionKey, byte[] message)
        {
            Headers headers = new Headers();
            headers.Add(new Header("messageid", Encoding.UTF8.GetBytes(messageId)));

            DeliveryResult<string, byte[]> result;
            if (String.IsNullOrEmpty(partitionKey))
                result = await _producer.ProduceAsync(topic, new Message<string, byte[]> { Key = null, Value = message, Headers = headers });
            else
            {
                result = await _producer.ProduceAsync(
                    new TopicPartition(topic, new Partition(_admin.GetPartitionId(topic, partitionKey))),
                    new Message<string, byte[]> { Key = null, Value = message, Headers = headers });
            }

            if (result.Status == PersistenceStatus.Persisted)
            {
                return new KafkaAdapter.Components.TopicPartitionOffset
                {
                    Offset = result.Offset.Value,
                    Partition = result.Partition.Value,
                    Topic = topic
                };
            }
            else
                return null;
        }
        public static KafkaException GetExceptionForMessage(AggregateException aggregateException, string messageId)
        {
            return (from e in aggregateException.InnerExceptions
                            let ke = e as ProduceException<string, byte[]>
                            where ke.DeliveryResult.Headers.Where(h=>h.Key == "messageid" && Encoding.UTF8.GetString(h.GetValueBytes()) == messageId).Count() > 0
                            select new KafkaException($"failed to send message: {ke.Message}")).FirstOrDefault();

        }

        public void Dispose()
        {
            if (_producer != null)
                _producer.Dispose();
        }


        private ProduceResult ProduceCallBack(DeliveryReport<string, byte[]> result, string messageId)
        {
            var pr = new ProduceResult { MessageId = messageId};
            if (result.Status == PersistenceStatus.Persisted)
            {
                pr.TopicPartitionOffset = new TopicPartitionOffset
                {
                    Offset = result.Offset.Value,
                    Partition = result.Partition.Value,
                    Topic = result.Topic
                };
            }
            else
            {
                Trace.Logger.TraceInfo(result.Status.ToString());
                Trace.Logger.TraceInfo(result.Error?.ToString());
                pr.IsError = true;
                pr.Error = result.Error?.IsError == true ? result.Error.ToString() : result.Status.ToString();
            }

            return pr;
        }

        private ProduceResult HandleException(ProduceException<string, byte[]> exception, string messageId)
        {
            var pr = new ProduceResult()
            {
                MessageId = messageId,
                IsError = true,
                Error = exception.Error.ToString()
            };

            return pr;

        }
    }
}
