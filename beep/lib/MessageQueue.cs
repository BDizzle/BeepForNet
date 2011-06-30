/*
* MessageQueue.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2002 Huston Franklin.  All rights reserved.
*
* The contents of this file are subject to the Blocks Public License (the
* "License"); You may not use this file except in compliance with the License.
*
* You may obtain a copy of the License at http://www.beepcore.org/
*
* Software distributed under the License is distributed on an "AS IS" basis,
* WITHOUT WARRANTY OF ANY KIND, either express or implied.  See the License
* for the specific language governing rights and limitations under the
* License.
*
*/
using System;
using System.Collections;
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
using beepcore.beep.core;

namespace beepcore.beep.lib
{
	/// <summary>Is a convience class that is registered with a <code>Channel</code> as a
	/// <code>MessageListener</code>. This receives messages and places them on a
	/// queue to be retrieved by calling <code>getNextMessage</code>. The same
	/// instance of <code>MessageQueue</code> can be registered with more than one
	/// <code>Channel</code> providing an easy mechanism to service the requests on
	/// several <code>Channel</code>s with the same thread(s).</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
#if MESSAGELISTENER
	public class MessageQueue : MessageListener
#else
	public class MessageQueue : core.IRequestHandler
#endif
	{
		private Log log;
		private ArrayList queue = new ArrayList();
		
		public MessageQueue()
		{
			log = LogFactory.getLog(this.GetType());
		}

		/// <summary>Gets the next message on the queue blocking if none are available.</summary>
		virtual public IMessage getNextMessage()
		{
			log.trace("getNextMessage: entry");
			lock (this)
			{
				if (queue.Count == 0)
				{
					System.Threading.Monitor.Wait(this);
				}
				IMessage msg = (IMessage) queue[0];
				queue.RemoveAt(0);
				return msg;
			}
		}
		
#if MESSAGELISTENER
		public virtual void ReceiveMSG(Message message)
#else
		public virtual void ReceiveMSG(core.IMessageMSG message)
#endif
		{
			log.trace("receiveMSG: entry");
			lock (this)
			{
				queue.Add(message);
				System.Threading.Monitor.Pulse(this);
			}
		}
	}
}