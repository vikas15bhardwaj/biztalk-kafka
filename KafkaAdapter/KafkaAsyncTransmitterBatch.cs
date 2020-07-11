using AdapterFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using System.Collections;
using Microsoft.BizTalk.Message.Interop;
using KafkaAdapter.Components;

namespace KafkaAdapter
{
    public class KafkaAsyncTransmitterBatch : IBTTransmitterBatch
    {
        private delegate void SortedBatchWorkerDelegate(string endPointKey, List<IBaseMessage> messages);
        private delegate void BatchWorkerDelegate();

        private BatchWorkerDelegate _batchWorker;
        private SortedBatchWorkerDelegate _sortedBatchWorker;

        private string _traceId;

        protected int _maxBatchSize;
        protected Type _endpointType;
        protected string _propertyNamespace;
        protected IPropertyBag _handlerPropertyBag;
        protected IBTTransportProxy _transportProxy;
        protected AsyncTransmitter _asyncTransmitter;

        protected ArrayList Messages { get; private set; }


        public KafkaAsyncTransmitterBatch(int maxBatchSize, Type endpointType, string propertyNamespace, IPropertyBag handlerPropertyBag, 
            IBTTransportProxy transportProxy, AsyncTransmitter asyncTransmitter) 
        {
            this._traceId = asyncTransmitter.TraceId;

            Trace.Logger.TraceInfo($"{this._traceId}: KafkaAsyncTransmitterBatch:");
            
            this._maxBatchSize = maxBatchSize;
            this._endpointType = endpointType;
            this._propertyNamespace = propertyNamespace;
            this._handlerPropertyBag = handlerPropertyBag;
            this._transportProxy = transportProxy;
            this._asyncTransmitter = asyncTransmitter;

            this.Messages = new ArrayList();
            this._batchWorker = new BatchWorkerDelegate(this.BatchWorker);
            this._sortedBatchWorker = new SortedBatchWorkerDelegate(this.SortedBatchWorker);

        }

        #region IBTTransmitterBatch...

        public object BeginBatch(out int maxBatchSize)
        {
            Trace.Logger.TraceInfo($"{this._traceId}: KafkaAsyncTransmitterBatch.BeginBatch");
            maxBatchSize = this._maxBatchSize;
            return null;
        }

        public bool TransmitMessage(IBaseMessage msg)
        {
            Trace.Logger.TraceInfo($"{this._traceId}: KafkaAsyncTransmitterBatch.TransmitMessage");
            this.Messages.Add(msg);
            return false;
        }

        public void Clear()
        {
            Trace.Logger.TraceInfo($"{this._traceId}: KafkaAsyncTransmitterBatch.Clear");
            this.Messages.Clear();
        }
        /// This method overrides base class Done method, which is once messaging t.
        public void Done(IBTDTCCommitConfirm commitConfirm)
        {
            Trace.Logger.TraceInfo($"{this._traceId}: Entering KafkaAsyncTransmitterBatch.Done");
            if (this.Messages.Count == 0)
            {
                Exception ex = new InvalidOperationException("Send adapter received an emtpy batch for transmission from BizTalk");
                this._transportProxy.SetErrorInfo(ex);
                return;
            }

            try
            {
                this._batchWorker.BeginInvoke(null, null);
            }
            catch (Exception ex)
            {
                //  If there was an error we had better do the "Leave" here
                this._transportProxy.SetErrorInfo(ex);
                Trace.Logger.TraceInfo($"{this._traceId}: KafkaAsyncTransmitterBatch.Done ex: {ex.ToString()}");

            }
            Trace.Logger.TraceInfo($"{this._traceId}: Leaving KafkaAsyncTransmitterBatch.Done");
        }

        #endregion

        #region workers...
        private void BatchWorker()
        {
            try
            {
                var ticks = Trace.Logger.TraceStartScope($"{this._traceId}: Entering KafkaAsyncTransmitterBatch.Worker...");
                Hashtable batches = SortMessagesIntoBatches();

                foreach (DictionaryEntry entry in batches)
                {
                    try
                    {
                        this._sortedBatchWorker.BeginInvoke((string)entry.Key, (List<IBaseMessage>)entry.Value, null, null);
                    }
                    catch (Exception ex)
                    {
                        Trace.Logger.TraceError($"{this._traceId}: {ex.ToString()}");
                        this._transportProxy.SetErrorInfo(ex);
                    }
                }
                Trace.Logger.TraceEndScope($"{this._traceId}: Leaving KafkaAsyncTransmitterBatch.Worker", ticks);
            }
            catch (Exception e)
            {
                Trace.Logger.TraceError($"{this._traceId}: {e.ToString()}");
                this._transportProxy.SetErrorInfo(e);
            }
        }

        private void SortedBatchWorker(string outboundTransportLocation, List<IBaseMessage> batch)
        {
            var ticks = Trace.Logger.TraceStartScope($"{this._traceId}: Entering KafkaAsyncTransmitterBatch.BatchWorker... {batch.Count}");
            //  we did an enter for every message - so we should ensure we have a correspending leave
            int leaveCount = batch.Count;


            try
            {
                //  all the messages in this batch should have the same properties - so just take the first one
                IBaseMessage firstMessage = (IBaseMessage)batch[0];
                KafkaTransmitProperties properties = TransmitHelper.CreateTransmitProperties(firstMessage, this._propertyNamespace);

                ConfluentKafkaAsyncBatch kafkaAsyncBatch = new ConfluentKafkaAsyncBatch(this._transportProxy, this._asyncTransmitter, properties);
                kafkaAsyncBatch.BatchTransmit(batch);


            }
            catch (Exception e)
            {
                Trace.Logger.TraceError(e, true);
                this._transportProxy.SetErrorInfo(e);
            }

            Trace.Logger.TraceEndScope($"{this._traceId}: Leaving KafkaAsyncTransmitterBatch.BatchWorker", ticks);
        }

        #endregion

        #region private methods...
        private Hashtable SortMessagesIntoBatches()
        {
            Hashtable batches = new Hashtable();

            foreach (IBaseMessage message in this.Messages)
            {

                string endpointKey = CreateEndpointKey(message);

                List<IBaseMessage> batch;
                if (!batches.ContainsKey(endpointKey))
                {
                    batch = new List<IBaseMessage>();
                    batches.Add(endpointKey, batch);
                }
                else
                {
                    batch = (List<IBaseMessage>)batches[endpointKey];
                }
                batch.Add(message);
            }

            return batches;
        }

        private string CreateEndpointKey(IBaseMessage message)
        {
            // if this is a dynamic send then we need to update the key
            string config = (string)message.Context.Read("AdapterConfig", this._propertyNamespace);
            if (config == null)
            {
                SystemMessageContext systemContext = new SystemMessageContext(message.Context);
                return systemContext.OutboundTransportLocation;
            }
            else
            {
                // for a static send port we could hash either the config or the spid - the latter is shorter
                string spid = (string)message.Context.Read("SPID", "http://schemas.microsoft.com/BizTalk/2003/system-properties");

                return spid;
            }
        }

        #endregion

    }
}
