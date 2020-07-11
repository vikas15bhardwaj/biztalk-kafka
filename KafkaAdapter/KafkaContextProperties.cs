using KafkaAdapter.Components;
using KafkaAdapter.Schemas;
using Microsoft.BizTalk.Message.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter
{
    internal class KafkaContextProperties
    {
        static Acks Acks = new Acks();
        static MessageId MessageId = new MessageId();
        static CorrelationId CorrelationId = new CorrelationId();
        static MessageTimeout MessageTimeout = new MessageTimeout();
        static BatchSize BatchSize = new BatchSize();
        static SaslKerberosServiceName SaslKerberosServiceName = new SaslKerberosServiceName();
        static SaslMechanism SaslMechanism = new SaslMechanism();
        static SslCaLocation SslCaLocation = new SslCaLocation();
        static SecurityProtocol SecurityProtocol = new SecurityProtocol();
        static PartitionKey PartitionKey = new PartitionKey();
        static Debug Debug = new Debug();
        static Topic Topic = new Topic();
        static GroupId GroupId = new GroupId();
        static Offset Offset = new Offset();
        static Partition Partition = new Partition();
        static MessageMaxSizeMb MessageMaxSizeMb = new MessageMaxSizeMb();
        internal static int ReadMessageTimeout(IBaseMessage message)
        {
            var value = message.Context.Read(MessageTimeout.Name.Name, MessageTimeout.Name.Namespace);
            if (value != null)
                return Convert.ToInt32(value);

            return Constants.DefaultMessageTimeout;
        }

        internal static int ReadBatchSize(IBaseMessage message)
        {
            var value = message.Context.Read(BatchSize.Name.Name, BatchSize.Name.Namespace);
            if (value != null)
                return Convert.ToInt32(value);

            return Constants.DefaultBatchSize;
        }

        internal static string ReadSaslKerberosServiceName(IBaseMessage message)
        {
            var value = message.Context.Read(SaslKerberosServiceName.Name.Name, SaslKerberosServiceName.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadSaslMechanism(IBaseMessage message)
        {
            var value = message.Context.Read(SaslMechanism.Name.Name, SaslMechanism.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadSslCaLocation(IBaseMessage message)
        {
            var value = message.Context.Read(SslCaLocation.Name.Name, SslCaLocation.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadSecurityProtocol(IBaseMessage message)
        {
            var value = message.Context.Read(SecurityProtocol.Name.Name, SecurityProtocol.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadPartitionKey(IBaseMessage message)
        {
            var value = message.Context.Read(PartitionKey.Name.Name, PartitionKey.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadDebug(IBaseMessage message)
        {
            var value = message.Context.Read(Debug.Name.Name, Debug.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadAcks(IBaseMessage message)
        {
            var value = message.Context.Read(Acks.Name.Name, Acks.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadMessageId(IBaseMessage message)
        {
            var value = message.Context.Read(MessageId.Name.Name, MessageId.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadCorrelationId(IBaseMessage message)
        {
            var value = message.Context.Read(CorrelationId.Name.Name, CorrelationId.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadTopic(IBaseMessage message)
        {
            var value = message.Context.Read(Topic.Name.Name, Topic.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }

        internal static string ReadGroupId(IBaseMessage message)
        {
            var value = message.Context.Read(GroupId.Name.Name, GroupId.Name.Namespace);
            if (value != null)
                return value.ToString();

            return "";
        }
        internal static int ReadPartition(IBaseMessage message)
        {
            var value = message.Context.Read(Partition.Name.Name, Partition.Name.Namespace);
            if (value != null)
                return Convert.ToInt32(value);

            return Constants.DefaultAssignedPartition;
        }
        internal static long ReadOffset(IBaseMessage message)
        {
            var value = message.Context.Read(Offset.Name.Name, Offset.Name.Namespace);
            if (value != null)
                return Convert.ToInt64(value);

            return Constants.DefaultAssignedOffset;
        }
        internal static int ReadMessageMaxSizeMb(IBaseMessage message)
        {
            var value = message.Context.Read(MessageMaxSizeMb.Name.Name, MessageMaxSizeMb.Name.Namespace);
            if (value != null)
                return Convert.ToInt32(value);

            return Constants.DefaultMessageMaxSizeMb;
        }
        public static void PromoteResponseProperties(IBaseMessage biztalkMessage, string correlationId, string messageId)
        {
            IBaseMessageContext context = biztalkMessage.Context;
            context.Promote(CorrelationId.Name.Name, CorrelationId.Name.Namespace, correlationId);
            context.Promote(MessageId.Name.Name, MessageId.Name.Namespace, messageId);
        }

        public static void PromoteRcvProperties(IBaseMessageContext context, KafkaReceiveProperties properties, int partition, long offset)
        {
            context.Promote(Topic.Name.Name, Topic.Name.Namespace, properties.Topic);
            context.Promote(GroupId.Name.Name, GroupId.Name.Namespace, properties.GroupId);
            context.Write(Offset.Name.Name, Offset.Name.Namespace, offset);
            context.Promote(Partition.Name.Name, Partition.Name.Namespace, partition);

        }
    }

}
