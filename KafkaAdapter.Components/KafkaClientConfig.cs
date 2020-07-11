using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    internal class KafkaClientConfig
    {
        public static T GetConfig<T>(KafkaConfig configProperties) where T : ClientConfig, new()
        {
            var config = new T();

            config.BootstrapServers = configProperties.Broker;

            SaslMechanism saslMechanism;
            if (Enum.TryParse<SaslMechanism>(configProperties.SaslMechanism, out saslMechanism))
                config.SaslMechanism = saslMechanism;

            SecurityProtocol securityProtocol;
            if (Enum.TryParse<SecurityProtocol>(configProperties.SecurityProtocol, out securityProtocol))
                config.SecurityProtocol = securityProtocol;

            config.SaslKerberosServiceName = configProperties.SaslKerberosServiceName;
            config.SslCaLocation = configProperties.SslCaLocation;

            if (!String.IsNullOrEmpty(configProperties.Debug))
            {
                config.Debug = configProperties.Debug;
            }

            return config;
        }
    }
}
