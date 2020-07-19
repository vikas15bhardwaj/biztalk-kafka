using KafkaAdapter.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KafkaAdapter
{
    public sealed class KafkaReceiveProperties : KafkaCommonProperties
    {
        public string GroupId { get; set; }
        public string AutoOffsetReset { get; set; } = Constants.DefaultAutoOffsetReset;

        private string _partitionIds;
        public List<int> PartitionIds { get; set; }

        private string _offset;
        public List<long> Offset { get; set; }

        private string _partitionKeys;
        public List<string> PartitionKeys { get; set; }

        public KafkaReceiveProperties(string uri)
        {
            this.Uri = uri;
        }
        public KafkaReceiveProperties()
        {
            this.Uri = "kafka://";
        }
        public void HandlerConfiguration(XmlDocument configDOM)
        {
        }

        public void LocationConfiguration(XmlDocument configDOM)
        {
            Trace.Logger.TraceInfo("Parsing GetStaticSendConfiguration");
            base.GetCommonStaticConfiguration(configDOM);

            this.GroupId = IfExistsExtract(configDOM, "Config/groupId", this.GroupId);
            Trace.Logger.TraceInfo($"groupId: {this.GroupId}");

            this.AutoOffsetReset = IfExistsExtract(configDOM, "Config/autoOffsetReset", this.AutoOffsetReset);
            Trace.Logger.TraceInfo($"autoOffsetReset: {this.AutoOffsetReset}");

            _partitionIds = IfExistsExtract(configDOM, "Config/partitionId", null);

            //when clearing property on receive location, config has node partitionId with empty string vs node not present when property is not touched
            if (_partitionIds == "")
                _partitionIds = null;

            this.PartitionIds = _partitionIds?.Split(',').Select(s => Convert.ToInt32(s)).ToList();

            Trace.Logger.TraceInfo($"partitionIds: {_partitionIds}");

            _partitionKeys = IfExistsExtract(configDOM, "Config/partitionKey", null);
            this.PartitionKeys = _partitionKeys?.Split(',').ToList();

            Trace.Logger.TraceInfo($"partitionKeys: {_partitionKeys}");

            _offset = IfExistsExtract(configDOM, "Config/offset", null);
            if (_offset == "")
                _offset = null;

            this.Offset = _offset?.Split(',').Select(s => Convert.ToInt64(s)).ToList();

            Trace.Logger.TraceInfo($"offset: {_offset}");

            this.Uri = $"kafka://{this.Connection}/{this.Topic}/{this.GroupId}";
            Trace.Logger.TraceInfo($"uri: {this.Uri}");


        }

        public override string ToString()
        {
            return $@"{base.ToString()}{GroupId}{AutoOffsetReset}{_offset}{_partitionIds}{_partitionKeys}";
        }

    }
}
