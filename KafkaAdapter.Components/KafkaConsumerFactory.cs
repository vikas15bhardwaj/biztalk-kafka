using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public class KafkaConsumerFactory
    {
        static object _syncRoot = new object();
        static Dictionary<string, IKafkaConsumer> _consumers = new Dictionary<string, IKafkaConsumer>();


        private KafkaConsumerFactory()
        {

        }

        public static IKafkaConsumer GetConsumer(KafkaConsumerConfig config)
        {
            var ticks = Trace.Logger.TraceStartScope("GetConsumer");
            if (config == null || config.Broker == null)
                throw new ArgumentNullException("Must provide required parameters config broker");

            if (config == null || config.Broker == null)
                throw new ArgumentNullException("Must provide required parameters config broker");

            //create a new connection only if connection is different in any of the following property
            string key = GetKey(config);

            IKafkaConsumer consumer;

            if (_consumers.ContainsKey(key))
                consumer = _consumers[key];
            else
            {
                lock (_syncRoot)
                {
                    if (_consumers.ContainsKey(key))
                        consumer = _consumers[key];
                    else
                    {
                        Trace.Logger.TraceInfo($"Creating new KafkaConsumer {key}");
                        consumer = new KafkaConsumer(config);
                        _consumers.Add(key, consumer);
                    }
                }
            }
            Trace.Logger.TraceEndScope("GetConsumer", ticks);

            return consumer;

        }

        public static void Close(KafkaConsumerConfig config)
        {
            string key = GetKey(config);
            if (_consumers.ContainsKey(key))
            {
                _consumers[key].Dispose();
                _consumers.Remove(key);
            }
        }

        private static string GetKey(KafkaConsumerConfig config)
        {
            //create a new connection only if connection is different in any of the following property
            return config.ToString().GetHash();
        }
    }
}
