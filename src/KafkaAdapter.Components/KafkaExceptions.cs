using System;
using System.Runtime.Serialization;

namespace KafkaAdapter.Components
{
	public class KafkaException : ApplicationException
	{
		public static string UnhandledTransmit_Error = "The Kafka Adapter encounted an error transmitting a batch of messages.";

        public KafkaException() { }

		public KafkaException(string msg) : base(msg) { }

		public KafkaException(Exception inner) : base(String.Empty, inner) { }

		public KafkaException(string msg, Exception e) : base(msg, e) { }

        protected KafkaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

