using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public class KafkaProducerFactory
    {
        static object _syncRoot = new object();
        static Dictionary<string, IKafkaProducer> _producers = new Dictionary<string, IKafkaProducer>();


        private KafkaProducerFactory()
        {

        }

        public static IKafkaProducer GetProducer(KafkaProducerConfig config)
        {
            var ticks = Trace.Logger.TraceStartScope("GetProducer");
            if (config == null || config.Broker == null)
                throw new ArgumentNullException("Must provide required parameters config broker");

            //create a new connection only if connection is different in any of the following property
            string key = config.ToString().GetHash();

            IKafkaProducer producer;

            if (_producers.ContainsKey(key))
                producer = _producers[key];
            else
            {
                lock (_syncRoot)
                {
                    if (_producers.ContainsKey(key))
                        producer = _producers[key];
                    else
                    {
                        Trace.Logger.TraceInfo($"Creating new KafkaProducer {key}");
                        producer = new KafkaProducer(config);
                        _producers.Add(key, producer);
                    }
                }
            }
            Trace.Logger.TraceEndScope("GetProducer", ticks);

            return producer;

        }
    }
}
