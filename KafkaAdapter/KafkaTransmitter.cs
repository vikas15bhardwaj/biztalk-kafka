using System;
using System.Xml;
using AdapterFramework;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.BizTalk.Component.Interop;
using KafkaAdapter.Components;

namespace KafkaAdapter
{
    //  The TransactionalTransmitter class is a Singleton.
    //
    //  This is the class that is loaded by the BizTalk runtime and so must be public.
    //
    //  The instance of this class used by the runtime is created on demand when messages are available.
    //
    //  The main role of this class is to hand back a "batch" object to BizTalk when it has messages to send.
    //  Note that is name is confusing - this batch object is unrelated to the message-batch class. The message-batch
    //  class is captured in the BaseAdapter framework with the Batch class. This AsyncBatch class is just a way for
    //  the BizTalk runtime to hand the adapter the list of messages it has ready to send.

	public class KafkaTransmitter : AsyncTransmitter
	{
		private int transactionalMaxBatchSize;
        private const int DEFAULT_MAX_BATCH_SIZE = Constants.DefaultBizTalkMessagingEngineTransmitBatchSize;
		public KafkaTransmitter() : base(
			"Kafka Transmitter",
			"1.0",
			"Send transactional messages from BizTalk to Kafka",
			"Kafka",
            new Guid("B3100D72-56D5-4997-A166-E03381AB08A9"),
            "http://biztalk-adapter/kafka-properties/2020/1",
            null,
            DEFAULT_MAX_BATCH_SIZE, Guid.NewGuid().ToString())
		{
            this.transactionalMaxBatchSize = base.MaxBatchSize;
		}

		protected override void HandlerPropertyBagLoaded ()
		{
			IPropertyBag config = this.HandlerPropertyBag;
			if (null != config)
			{
				XmlDocument handlerConfigDom = ConfigProperties.IfExistsExtractConfigDom(config);
				if (null != handlerConfigDom)
				{
                    KafkaTransmitProperties.GetTransmitHandlerConfiguration(handlerConfigDom, DEFAULT_MAX_BATCH_SIZE);
				}
			}
            //override from handler max batch size if that has changed, otherwise default will be the size
            this.transactionalMaxBatchSize = (KafkaTransmitProperties.MaxBatchSize > 0 ? KafkaTransmitProperties.MaxBatchSize : base.MaxBatchSize);
		}

		protected override int MaxBatchSize { get { return this.transactionalMaxBatchSize; } }

        //  Create and return to BizTalk the object it will use to give the adapter the list of messages.
		protected override IBTTransmitterBatch CreateAsyncTransmitterBatch ()
		{
            base.TraceId = Guid.NewGuid().ToString();
            //return new KafkaAsyncBatch(this.TransportProxy, this.ControlledTermination, this.PropertyNamespace, this.MaxBatchSize, this);
            return new KafkaAsyncTransmitterBatch(this.MaxBatchSize, this.EndpointType, this.PropertyNamespace, this.HandlerPropertyBag, this.TransportProxy, this);
        }
	}
}
