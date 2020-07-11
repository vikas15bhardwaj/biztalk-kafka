using KafkaAdapter.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KafkaAdapter
{
    public sealed class KafkaTransmitProperties : KafkaCommonProperties
    {
        public static int MaxBatchSize { get; private set; }


        public string Acks { get; set; } = Constants.DefaultAck;

        public KafkaTransmitProperties(string uri)
        {
            this.Uri = uri;
        }

        public void GetStaticSendConfiguration(XmlDocument configDOM)
        {
            Trace.Logger.TraceInfo("Parsing GetStaticSendConfiguration");
            base.GetCommonStaticConfiguration(configDOM);

            this.Acks = IfExistsExtract(configDOM, "Config/acks", this.Acks);
            Trace.Logger.TraceInfo($"acks: {this.Acks}");

            this.Uri = $"kafka://{this.Connection}/{this.Topic}";
            Trace.Logger.TraceInfo($"uri: {this.Uri}");


        }

        public void GetDynamicSendConfiguration(string dynamicUri)
        {
            Trace.Logger.TraceInfo($"dynamicuri {dynamicUri}");

            if (!dynamicUri.Substring(0, 8).Equals("kafka://"))
            {
                if (!dynamicUri.Substring(0, 6).Equals("kafka:"))
                {
                    throw new Exception("Trace: URI must start with kafka://");
                }
                
            }
            else
            {
                var uri = dynamicUri.Substring(8).Split('/');
                this.Connection = uri[0];
                this.Topic = uri[1];
                base.IsDynamic = true;
            }
        }

        public static void GetTransmitHandlerConfiguration(XmlDocument handlerConfigDom, int defaultMaxBatchSize)
        {
            if (handlerConfigDom == null)
            {
                MaxBatchSize = defaultMaxBatchSize;
            }
            else
            {
                int num = KafkaCommonProperties.IfExistsExtractInt(handlerConfigDom, "Config/MaxBatchSize", defaultMaxBatchSize);
                MaxBatchSize = (num > 0 ? num : defaultMaxBatchSize);

            }

            Trace.Logger.TraceInfo($"MaxBatchSize: {MaxBatchSize}");

        }
    }
}
