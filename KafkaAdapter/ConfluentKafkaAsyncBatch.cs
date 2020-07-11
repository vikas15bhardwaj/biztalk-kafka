using KafkaAdapter.Components;
using AdapterFramework;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaAdapter
{
    public class ConfluentKafkaAsyncBatch
    {
        protected AsyncTransmitter AsyncTransmitter { get; set; }

        protected IBTTransportProxy TransportProxy { get; set; }

        protected KafkaTransmitProperties Properties { get; set; }

        string _traceId;

        public ConfluentKafkaAsyncBatch(IBTTransportProxy transportProxy, AsyncTransmitter asyncTransmitter, KafkaTransmitProperties transmitProperties) 
        {
            this.AsyncTransmitter = asyncTransmitter;
            this.TransportProxy = transportProxy;
            this.Properties = transmitProperties;
            _traceId = asyncTransmitter.TraceId;
        }

        protected void BatchCompleteCallBackHandler()
        {
            Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: batch complete callback handler");
            this.AsyncTransmitter.Leave();
        }

        public void BatchTransmit(List<IBaseMessage> messages)
        {
            
            var ticks = Trace.Logger.TraceStartScope($"{_traceId}: ConfluentKafkaAsyncBatch: BatchTransmit count: {messages.Count}");
            try
            {
                if (!this.AsyncTransmitter.Enter())
                {
                    throw new InvalidOperationException($"{_traceId}: ConfluentKafkaAsyncBatch: EPM called Terminate during a BatchTransmit call.");
                }

                Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Creating connection");
                
                using (TransmitResponseBatch transmitResBatch = new TransmitResponseBatch(this.TransportProxy, new TransmitResponseBatch.AllWorkDoneDelegate(this.BatchCompleteCallBackHandler), this._traceId))
                {
                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: About to enter message processing loop");

                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Sending SendMessage...");
                    //submit batch to kafka api

                    IKafkaProducer kafkaProducer = KafkaProducerFactory.GetProducer(new KafkaProducerConfig
                                                                            {
                                                                                Broker =  this.Properties.Connection,
                                                                                Acks = this.Properties.Acks,
                                                                                BatchSize = this.Properties.BatchSize,
                                                                                Debug = this.Properties.Debug,
                                                                                MessageTimeout = this.Properties.MessageTimeOut,
                                                                                SaslKerberosServiceName = this.Properties.SaslKerberosServiceName,
                                                                                SaslMechanism = this.Properties.SaslMechanism,
                                                                                SecurityProtocol = this.Properties.SecurityProtocol,
                                                                                SslCaLocation = this.Properties.SslCaLocation,
                                                                                MessageMaxSizeMb = this.Properties.MessageMaxSizeMb
                                                                            });

                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Sending messages...");
                    ConcurrentBag<ProduceResult> results = new ConcurrentBag<ProduceResult>();
                    Parallel.ForEach<IBaseMessage>(messages, (message) =>
                    {
                        results.Add(kafkaProducer.PublishSync(message.MessageID.ToString(), this.Properties.Topic, this.Properties.PartitionKey, TransmitHelper.ReadStreamToByteArray(message.BodyPart.Data)));
                    });

                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: completing batch...");

                    //completing bizTalk batch
                    //if all good delete message from biztalk or it goes to exception

                    foreach (var message in messages)
                    {
                        var result = results.Where(r => r.MessageId == message.MessageID.ToString())?.FirstOrDefault();
                        if (result.IsError == false)
                        {
                            SystemMessageContext systemMessageContext = new SystemMessageContext(message.Context);

                            if ((systemMessageContext.Read("IsSolicitResponse") == null ? false : systemMessageContext.IsSolicitResponse))
                            {
                                Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Submitting BizTalk Response Message...");
                                IBaseMessage baseMessage = TransmitHelper.CreateBiztalkResponse(this.TransportProxy.GetMessageFactory(), KafkaContextProperties.ReadCorrelationId(message), KafkaContextProperties.ReadMessageId(message));
                                transmitResBatch.SubmitResponseMessage(message, baseMessage);
                            }
                            Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Deleting BizTalk base message...");
                            transmitResBatch.DeleteMessage(message);

                        }
                        else
                        {
                            Trace.Logger.TraceError($"{_traceId}: message failed with error {result.Error}...");
                            message.SetErrorInfo(new KafkaException(result.Error));
                            transmitResBatch.Resubmit(message, false, null);
                        }
                    }

                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Finish batch. About to call Done for batch...");
                    transmitResBatch.Done(null);

                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Batch Done. Returning from EndBatchCompleteEvent.Wait()");
                }
            }
            catch (Exception ex)
            {
                //handle exception
                Trace.Logger.TraceError(ex, true);
                var aggregateException = ex as AggregateException;

                using (TransmitResponseBatch transmitResBatch = new TransmitResponseBatch(this.TransportProxy, new TransmitResponseBatch.AllWorkDoneDelegate(BatchCompleteCallBackHandler), this._traceId))
                {
                    foreach (IBaseMessage message in messages)
                    {
                        if (aggregateException == null)
                            message.SetErrorInfo(ex);
                        else
                            message.SetErrorInfo(Components.KafkaProducer.GetExceptionForMessage(aggregateException, message.MessageID.ToString()));

                        transmitResBatch.Resubmit(message, false, null);
                    }
                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: About to call Done on acquire error batch...");
                    transmitResBatch.Done();
                    Trace.Logger.TraceInfo($"{_traceId}: ConfluentKafkaAsyncBatch: Returning from EndBatchCompleteEvent.Wait() for failureBatch.");
                }
                
            }
            finally
            {
                Trace.Logger.TraceInfo($"{_traceId}: finally");
            }

            Trace.Logger.TraceEndScope($"{_traceId}: ConfluentKafkaAsyncBatch: BatchTransmit", ticks);
        }


        public void BatchTransmitTxn(List<IBaseMessage> messages)
        {
            
        }
           
    }
}
