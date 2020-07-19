
//---------------------------------------------------------------------
// File: KafkaReceiverEndpoint.cs
// 
// Summary: Implementation of an adapter framework sample adapter. 
//
// Sample: Adapter framework transactional adapter.
//
//---------------------------------------------------------------------
// This file is part of the Microsoft BizTalk Server SDK
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// This source code is intended only as a supplement to Microsoft BizTalk
// Server release and/or on-line documentation. See these other
// materials for detailed information regarding Microsoft code samples.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Text;
using System.Transactions;
using System.Data.SqlClient;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using AdapterFramework;
using KafkaAdapter.Components;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace KafkaAdapter
{
    //  An instance of this class should exist for every enabled receive location.
    //
    //  This is where all the receive location specific work goes. For example, any listening or polling generally goes in here.
    //
    //  If you want to have multiple independent threads listening or working per receive location that might effectively
    //  be done as member objects from this endpoint class. In this particular example there is only a simgle timer doing some
    //  polling and it is contained in here.
    //
    //  After the Receiver creates a member of this class it calls Open and when the receive location is deleted Dispose is called.
    //  Otherwise all Updates are plumbed through to the Update function on this class.

    internal class KafkaReceiverEndpoint : ReceiverEndpoint
    {

        //  properties
        private KafkaReceiveProperties _properties;
        List<KafkaMessage> _messageList = new List<KafkaMessage>();
        //  handle to the EPM
        private IBTTransportProxy _transportProxy;
        private IBaseMessageFactory _messageFactory;
        private ControlledTermination _control;

        private string _uri;
        private string _transportType;
        private string _propertyNamespace;

        CancellationTokenSource _cts;
        IKafkaConsumer _consumer;
        KafkaConsumerConfig _consumerConfig;
        ManualResetEvent _consumerCancellationWait;
        string _traceId;
        public KafkaReceiverEndpoint()
        {
            _traceId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// This method is called when an receive location is enabled. BizTalk engine creates a new instance of this class
        /// and call Open method with all required parameters, configs required to open a connection and be able to receive messages and submit them
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="config"></param>
        /// <param name="bizTalkConfig"></param>
        /// <param name="handlerPropertyBag"></param>
        /// <param name="transportProxy"></param>
        /// <param name="transportType"></param>
        /// <param name="propertyNamespace"></param>
        /// <param name="control"></param>
        public override void Open(string uri, IPropertyBag config, IPropertyBag bizTalkConfig, IPropertyBag handlerPropertyBag, IBTTransportProxy transportProxy, string transportType, string propertyNamespace, ControlledTermination control)
        {
            long ticks = Trace.Logger.TraceStartScope($"{_traceId}: Entering KafkaReceiveEndpoint.Open");
            try
            {
                this._properties = new KafkaReceiveProperties(uri);
                //  Handler properties
                XmlDocument handlerConfigDom = ConfigProperties.IfExistsExtractConfigDom(handlerPropertyBag);
                if (null != handlerConfigDom)
                    this._properties.HandlerConfiguration(handlerConfigDom);

                //  Location properties - possibly override some Handler properties
                XmlDocument locationConfigDom = ConfigProperties.ExtractConfigDom(config);
                this._properties.LocationConfiguration(locationConfigDom);

                //  this is our handle back to the EPM
                this._transportProxy = transportProxy;

                //  used to control whether the EPM can unload us
                this._control = control;

                this._uri = uri;
                this._transportType = transportType;
                this._propertyNamespace = propertyNamespace;
                this._messageFactory = this._transportProxy.GetMessageFactory();
                this._cts = new CancellationTokenSource();
                this._consumerCancellationWait = new ManualResetEvent(false);

                Start();
                Trace.Logger.TraceEndScope($"{_traceId}: Leaving KafkaReceiveEndpoint.Open", ticks);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// This method is called when config are update for a receive location
        /// If you have multiple receive locations in same port and you are making change to only
        /// one receive location of port, it is called one by one for each receive locations 
        /// even though no changes to other locations are made. To avoid stopping and starting the receive location
        /// check for config changes and if there are not any simply return.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="bizTalkConfig"></param>
        /// <param name="handlerPropertyBag"></param>
        public override void Update(IPropertyBag config, IPropertyBag bizTalkConfig, IPropertyBag handlerPropertyBag)
        {
            long ticks = Trace.Logger.TraceStartScope($"{_traceId}: Entering KafkaReceiveEndpoint.Update");
            try
            {
                var updatedProperties = new KafkaReceiveProperties();

                //  Handler properties
                XmlDocument handlerConfigDom = ConfigProperties.IfExistsExtractConfigDom(handlerPropertyBag);
                if (null != handlerConfigDom)
                    updatedProperties.HandlerConfiguration(handlerConfigDom);

                //  Location properties - possibly override some Handler properties
                XmlDocument locationConfigDom = ConfigProperties.ExtractConfigDom(config);
                updatedProperties.LocationConfiguration(locationConfigDom);

                //if no change to properties, just return without change.
                if (this._properties.ToString().GetHash() == updatedProperties.ToString().GetHash())
                {
                    Trace.Logger.TraceInfo($"{_traceId}: Update(), No changing detected in config.");
                    return;
                }
                Stop();

                this._properties = updatedProperties;
                this._cts = new CancellationTokenSource();
                this._consumerCancellationWait = new ManualResetEvent(false);

                Start();

                Trace.Logger.TraceEndScope($"{_traceId}: Leaving KafkaReceiveEndpoint.Update", ticks);
            }
            finally
            {
            }
        }
        /// <summary>
        /// Dispose is called when a receive location is disabled as part of RemoveEndpoint in base Receiver class
        /// clear any state as part of this
        /// </summary>
        public override void Dispose ()
        {
            Trace.Logger.TraceInfo($"{_traceId}: KafkaReceiveEndpoint.Dispose");
            Stop();
        }

        private void Start()
        {
            Trace.Logger.TraceInfo($"{_traceId}: KafkaReceiveEndpoint.Start");
            this._consumerConfig = new KafkaConsumerConfig
            {
                Broker = this._properties.Connection,
                Debug = this._properties.Debug,
                SaslKerberosServiceName = this._properties.SaslKerberosServiceName,
                SaslMechanism = this._properties.SaslMechanism,
                SecurityProtocol = this._properties.SecurityProtocol,
                SslCaLocation = this._properties.SslCaLocation,
                AutoOffsetReset = this._properties.AutoOffsetReset,
                GroupId = this._properties.GroupId,
                Topic = this._properties.Topic,
                PartitionIds = this._properties.PartitionIds,
                Offset = this._properties.Offset,
                PartitionKeys = this._properties.PartitionKeys,
                MessageMaxSizeMb = this._properties.MessageMaxSizeMb
            };
            this._consumer = KafkaConsumerFactory.GetConsumer(this._consumerConfig);
            Task.Run(() => this._consumer.Start(MessageReceived, _consumerCancellationWait, this._cts));
        }

        private void Stop()
        {
            Trace.Logger.TraceInfo($"{_traceId}: KafkaReceiveEndpoint.Stop");
            try
            {
                this._cts.Cancel();
                Trace.Logger.TraceInfo($"{_traceId}: waiting for unsubscribe");
                _consumerCancellationWait.WaitOne();
                _consumerCancellationWait.Reset();
                _consumerCancellationWait.Dispose();
                _messageList.Clear();
                KafkaConsumerFactory.Close(this._consumerConfig);
                this._cts.Dispose();
            }
            catch(Exception e)
            {
                Trace.Logger.TraceError(e, true);
                throw;
            }
        }
        public async Task MessageReceived(KafkaMessage message)
        {
            Trace.Logger.TraceInfo($"{_traceId}: KafkaReceiveEndpoint.MessageReceived");
            _messageList.Add(message);
            if (_messageList.Count >= this._properties.BatchSize)
            {
                await Task.Run(() => { SubmitBatch(); });
                _messageList.Clear();
            }

        }
        public void SubmitBatch()
        {
            bool needToLeave = false;
            var ticks = Trace.Logger.TraceStartScope($"{_traceId}: Entering Submitbatch");
            try
            {
                // used to block the Terminate from BizTalk 
                if (!this._control.Enter())
                {
                    needToLeave = false;
                    return;
                }
                needToLeave = true;
                ManualResetEvent orderedEvent = new ManualResetEvent(false);
                using (Batch batch = new ReceiveBatch(this._transportProxy, this._control, orderedEvent, this._properties.BatchSize, this._traceId))
                {

                    foreach (var message in _messageList)
                    {
                        MemoryStream stream = new MemoryStream(message.Message);
                        if (message != null)
                            batch.SubmitMessage(CreateMessage(stream, message));
                    }
                    batch.Done();
                }
                orderedEvent.WaitOne();

                //commit the offset
                OnBatchComplete(true);
                //  no exception in Done so we will be getting. Its done as part of base class BatchComplete process already.
                needToLeave = false;
            }
            catch (Exception e)
            {
                this._transportProxy.SetErrorInfo(e);

            }
            finally
            {
                //  if this is true there must have been some exception in or before Done
                if (needToLeave)
                    this._control.Leave();
            }

            Trace.Logger.TraceEndScope($"{_traceId}: Leaving SubmitBatch", ticks);
        }

        private IBaseMessage CreateMessage(Stream stream, KafkaMessage kMessage)
        {
            Trace.Logger.TraceInfo($"{_traceId}: Entering KafkaReceiveEndpoint.CreateMessage");
            stream.Seek(0, SeekOrigin.Begin);

            IBaseMessagePart part = this._messageFactory.CreateMessagePart();
            part.Data = stream;

            IBaseMessage message = this._messageFactory.CreateMessage();
            message.AddPart("body", part, true);

            //  We must add these context properties
            SystemMessageContext context = new SystemMessageContext(message.Context);
            context.InboundTransportLocation = this._uri;
            context.InboundTransportType = this._transportType;

            //  Any particular application specific context properties
            //  you want on the message add them here...

            KafkaContextProperties.PromoteRcvProperties(message.Context, this._properties, kMessage.TopicPartitionOffset.Partition, kMessage.TopicPartitionOffset.Offset);

            Trace.Logger.TraceInfo($"{_traceId}: Leaving KafkaReceiveEndpoint.CreateMessage");
            return message;
        }
        public void OnBatchComplete(bool overallStatus)
        {
            Trace.Logger.TraceInfo($"{_traceId}: KafkaReceiveEndpoint.OnBatchComplete");
            try
            {
                if (overallStatus)
                {
                    _consumer.Commit(_messageList.Select(m => m.TopicPartitionOffset).ToList());
                }
            }
            catch(Exception ex)
            {
                Trace.Logger.TraceInfo($"ERROR {_traceId}: KafkaReceiveEndpoint.OnBatchComplete: {ex.ToString()}");
                Trace.WriteToEventLog(ex, "KafkaReceiveEndpoint.OnBatchComplete");
                throw;
            }
        }

    }
}
