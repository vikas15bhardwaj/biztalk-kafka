using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public class KafkaAdminFactory
    {
        static object _syncRoot = new object();
        static Dictionary<string, IKafkaAdmin> _adminClients = new Dictionary<string, IKafkaAdmin>();


        private KafkaAdminFactory()
        {

        }
        public static IKafkaAdmin GetAdmin(KafkaConfig config)
        {
            var ticks = Trace.Logger.TraceStartScope("GetAdmin");
            if (config == null || config.Broker == null)
                throw new ArgumentNullException("Must provide required parameters config broker");

            //create a new connection only if connection is different in any of the following property
            string key = GetKey(config);
            IKafkaAdmin admin;

            if (_adminClients.ContainsKey(key))
                admin = _adminClients[key];
            else
            {
                lock (_syncRoot)
                {
                    if (_adminClients.ContainsKey(key))
                        admin = _adminClients[key];
                    else
                    {
                        Trace.Logger.TraceInfo($"Creating new KafkaAdmin {key}");
                        admin = new KafkaAdmin(config);
                        _adminClients.Add(key, admin);
                    }
                }
            }
            Trace.Logger.TraceEndScope("GetAdmin", ticks);

            return admin;

        }

        public static void Close(KafkaConfig config)
        {
            string key = GetKey(config);
            if (_adminClients.ContainsKey(key))
            {
                _adminClients[key].Dispose();
                _adminClients.Remove(key);
            }
        }

        private static string GetKey(KafkaConfig config)
        {
            //create a new connection only if connection is different in any of the following property
            return config.ToString().GetHash();
        }
    }
}
