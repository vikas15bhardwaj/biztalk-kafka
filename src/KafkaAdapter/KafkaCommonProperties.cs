using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.BizTalk.Message.Interop;
using System.IO;
using AdapterFramework;
using KafkaAdapter.Components;
//using Microsoft.SSO.Utility;

namespace KafkaAdapter
{
    /// <summary>
    /// The KafkaCommonProperties class represents the properties defined in
    /// </summary>
    public class KafkaCommonProperties : ConfigProperties
    {

        public bool IsDynamic { get; set; }
        public string Uri { get; set; }
        public string Connection { get; set; }
        public string Topic { get; set; }
        public string PartitionKey { get; set; }

        public string SaslKerberosServiceName { get; set; }

        public string SecurityProtocol { get; set; }

        public string SaslMechanism { get; set; }

        public string SslCaLocation { get; set; }

        public int MessageTimeOut { get; set; }

        public int BatchSize { get; set; }

        public string Debug { get; set; }

        public int MessageMaxSizeMb { get; set; }

        protected void GetCommonStaticConfiguration(XmlDocument configDOM)
        {
            Trace.Logger.TraceInfo("Extracting GetCommonStaticConfiguration");

            this.Connection = IfExistsExtract(configDOM, "Config/connection", null);
            Trace.Logger.TraceInfo($"connection: {this.Connection}");

            this.Topic = IfExistsExtract(configDOM, "Config/topic", null);
            Trace.Logger.TraceInfo($"topic: {this.Topic}");

            this.PartitionKey = IfExistsExtract(configDOM, "Config/partitionKey", null);
            Trace.Logger.TraceInfo($"partition key: {this.PartitionKey}");

            this.SaslKerberosServiceName = IfExistsExtract(configDOM, "Config/saslKerberosServiceName", null);
            Trace.Logger.TraceInfo($"SaslKerberosServiceName: {this.SaslKerberosServiceName}");

            this.SecurityProtocol = IfExistsExtract(configDOM, "Config/securityProtocol", null);
            Trace.Logger.TraceInfo($"SecurityProtocol: {this.SecurityProtocol}");

            this.SaslMechanism = IfExistsExtract(configDOM, "Config/saslMechanism", null);
            Trace.Logger.TraceInfo($"SaslMechanism: {this.SaslMechanism}");

            this.SslCaLocation = IfExistsExtract(configDOM, "Config/sslCaLocation", null);
            Trace.Logger.TraceInfo($"SslCaLocation: {this.SslCaLocation}");

            this.MessageTimeOut = IfExistsExtractInt(configDOM, "Config/messageTimeOut", Constants.DefaultMessageTimeout);
            Trace.Logger.TraceInfo($"MessageTimeOut: {this.MessageTimeOut}");

            this.BatchSize = IfExistsExtractInt(configDOM, "Config/batchSize", Constants.DefaultBatchSize);
            Trace.Logger.TraceInfo($"BatchSize: {this.BatchSize}");

            this.Debug = IfExistsExtract(configDOM, "Config/debug", null);
            Trace.Logger.TraceInfo($"Debug: {this.Debug}");

            this.MessageMaxSizeMb = IfExistsExtractInt(configDOM, "Config/messageMaxSizeMb", Constants.DefaultMessageMaxSizeMb);
            Trace.Logger.TraceInfo($"MessageMaxSizeMb: {this.MessageMaxSizeMb}");

        }

        public override string ToString()
        {
            return $@"{Connection}{BatchSize}{Debug}
                        {SaslKerberosServiceName}{SecurityProtocol}{SaslMechanism}{SslCaLocation}
                        {Topic}{MessageMaxSizeMb}";
        }
        #region private method...

        #endregion
    }
}
