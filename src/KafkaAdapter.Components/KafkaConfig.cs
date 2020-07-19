using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public class KafkaConfig
    {
        public string Broker { get; set; }
        public string Debug { get; set; }
        public string SaslKerberosServiceName { get; set; }
        public string SecurityProtocol { get; set; }
        public string SaslMechanism { get; set; }
        public string SslCaLocation { get; set; }
        public int MessageMaxSizeMb { get; set; }
        public override string ToString()
        {
            return $"{Broker}{SaslMechanism}{SecurityProtocol}{SaslKerberosServiceName}{SslCaLocation}{MessageMaxSizeMb}";
        }
    }
    public class KafkaProducerConfig : KafkaConfig
    {
        public int BatchSize { get; set; }
        public int MessageTimeout { get; set; }
        public string Acks { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}{BatchSize}{Acks}{MessageTimeout}";
        }
    }

    public class KafkaConsumerConfig : KafkaConfig
    {
        public string AutoOffsetReset { get; set; }
        public string GroupId { get; set; }

        public string Topic { get; set; }

        public List<int> PartitionIds { get; set; }
        public List<long> Offset { get; set; }

        public List<string> PartitionKeys { get; set; }
        public override string ToString()
        {
            return $@"{base.ToString()}
                    {Topic}{GroupId}{AutoOffsetReset}
                    {PartitionIds?.Select(p => p.ToString())?.Aggregate((s1, s2)=> s1 + "," + s2)}
                    {Offset?.Select(p => p.ToString())?.Aggregate((s1, s2) => s1 + "," + s2)}
                    {PartitionKeys?.Aggregate((s1, s2) => s1 + "," + s2)}";
        }
    }
}
