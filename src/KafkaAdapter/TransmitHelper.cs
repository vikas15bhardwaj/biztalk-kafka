using KafkaAdapter.Components;
using Microsoft.BizTalk.Message.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.XLANGs.BaseTypes;
using System.Xml;
using System.IO;

namespace KafkaAdapter
{
    public class TransmitHelper
    {
        internal static KafkaTransmitProperties CreateTransmitProperties(IBaseMessage firstMessage, string propertyNamespace)
        {
            KafkaTransmitProperties properties;

            SystemMessageContext systemContext = new SystemMessageContext(firstMessage.Context);
            properties = new KafkaTransmitProperties(systemContext.OutboundTransportLocation);

            var config = firstMessage.Context.Read("AdapterConfig", propertyNamespace);
            if (config == null)
            {
                Trace.Logger.TraceInfo("This is dynamic send port");
                properties.GetDynamicSendConfiguration(properties.Uri);

                properties.Acks = KafkaContextProperties.ReadAcks(firstMessage);
                Trace.Logger.TraceInfo($"Acks: {properties.Acks}");

                properties.BatchSize = KafkaContextProperties.ReadBatchSize(firstMessage);
                Trace.Logger.TraceInfo($"BatchSize: {properties.BatchSize}");

                properties.MessageTimeOut = KafkaContextProperties.ReadMessageTimeout(firstMessage);
                Trace.Logger.TraceInfo($"MessageTimeOut: {properties.MessageTimeOut}");

                properties.SaslKerberosServiceName = KafkaContextProperties.ReadSaslKerberosServiceName(firstMessage);
                Trace.Logger.TraceInfo($"SaslKerberosServiceName: {properties.SaslKerberosServiceName}");

                properties.SaslMechanism = KafkaContextProperties.ReadSaslMechanism(firstMessage);
                Trace.Logger.TraceInfo($"SaslMechanism: {properties.SaslMechanism}");

                properties.SecurityProtocol = KafkaContextProperties.ReadSecurityProtocol(firstMessage);
                Trace.Logger.TraceInfo($"SecurityProtocol: {properties.SecurityProtocol}");

                properties.SslCaLocation = KafkaContextProperties.ReadSslCaLocation(firstMessage);
                Trace.Logger.TraceInfo($"SslCaLocation: {properties.SslCaLocation}");

                properties.Debug = KafkaContextProperties.ReadDebug(firstMessage);
                Trace.Logger.TraceInfo($"Debug: {properties.Debug}");

                properties.PartitionKey = KafkaContextProperties.ReadPartitionKey(firstMessage);
                Trace.Logger.TraceInfo($"PartitionKey: {properties.PartitionKey}");

                properties.MessageMaxSizeMb = KafkaContextProperties.ReadMessageMaxSizeMb(firstMessage);
                Trace.Logger.TraceInfo($"MessageMaxSizeMb: {properties.MessageMaxSizeMb}");

                Trace.Logger.TraceInfo($"Topic {properties.Topic}");
            }
            else
            {
                Trace.Logger.TraceInfo("This is static send port");

                XmlDocument xmlDocument = new XmlDocument()
                {
                    XmlResolver = null
                };
                XmlReaderSettings xmlReaderSetting = new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };
                xmlDocument.Load(XmlReader.Create(new StringReader(config.ToString()), xmlReaderSetting));
                properties.GetStaticSendConfiguration(xmlDocument);
            }
            return properties;
        }

        internal static byte[] ReadStreamToByteArray(Stream sourceStream)
        {
            byte[] array;
            int num = 4096;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] numArray = new byte[num];
                while (true)
                {
                    int num1 = sourceStream.Read(numArray, 0, (int)numArray.Length);
                    if (num1 <= 0)
                        break;

                    memoryStream.Write(numArray, 0, num1);
                }
                array = memoryStream.ToArray();
            }
            return array;
        }

        internal static IBaseMessage CreateBiztalkResponse(IBaseMessageFactory factory, string messageId, string correlationId)
        {
            IBaseMessage baseMessage = factory.CreateMessage();
            baseMessage.AddPart("body", factory.CreateMessagePart(), true);
            baseMessage.BodyPart.Data = MakeResponseStream(messageId, correlationId);
            KafkaContextProperties.PromoteResponseProperties(baseMessage, correlationId, messageId);

            return baseMessage;
        }

        internal static Stream MakeResponseStream(string messageId, string correlationId)
        {
            Stream stream;
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, null);
            xmlTextWriter.WriteStartElement("Kafka-Response");
            xmlTextWriter.WriteElementString("MessageID", messageId);
            xmlTextWriter.WriteElementString("CorrelationID", correlationId);
            xmlTextWriter.WriteEndElement();
            xmlTextWriter.Flush();
            memoryStream.Seek((long)0, SeekOrigin.Begin);
            stream = memoryStream;
            return stream;
        }
    }
}
