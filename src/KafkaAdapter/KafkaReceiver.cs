
//---------------------------------------------------------------------
// File: TransactionalReceiver.cs
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
using AdapterFramework;

namespace KafkaAdapter
{
    //  The KafkaReceiver class is a Singleton.
    //
    //  This is the class that is loaded by the BizTalk runtime and so must be public.
    //
    //  The instance of this class used by the BizTalk runtime is created at startup (or enable).
    //
    //  Through the BaseAdapter adapter Receiver class it acts as a Factory for endpoints. In this
    //  case TransactionalReceiverEndpoint instances. There will be an endpoint instance for every
    //  logical receive location configured and enabled. The base class is given the type of endpoint
    //  to create.
    //
    //  The Receiver maintains a hashtable of active receive locations and calls Update and Delete
    //  appropriately.
    //
    //  If you have any state that should be shared amongst all active endpoints of this adapter you can
    //  put it here as an alternative to making it static in the endpoint class.

    public class KafkaReceiver : Receiver 
	{
        public KafkaReceiver()
            : base(
            "Kafka Adapter",
			"1.0",
			"Kafka Message Consumer",
			"kafka",
            new Guid("C3E34184-237D-427B-BFD7-D6F0E36B044F"),
            "http://biztalk-adapter/kafka-properties/2020/1",
			typeof(KafkaReceiverEndpoint), System.Guid.NewGuid().ToString())
		{
		}
	}
}