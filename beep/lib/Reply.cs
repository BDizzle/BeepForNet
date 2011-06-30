/*
* Reply.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
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
using BEEPInterruptedException = beepcore.beep.core.BEEPInterruptedException;
using beepcore.beep.core;

namespace beepcore.beep.lib
{
	/// <summary>Provides a synchronous abstraction for receiving BEEP reply messages.
	/// The caller may block using <code>getNextReply</code> when as it waits
	/// for incoming messages.
	/// 
	/// <code>Reply</code> is produced by the <code>Channel.sendMSG</code>
	/// method.
	/// 
	/// Please note that the other Channel send operations do NOT
	/// return this class as a result.</summary>
	/// <seealso cref="beepcore.beep.core.Channel#sendMSG"></seealso>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision, $Date: 2004/09/21 08:46:57 $</version>
	public class Reply : ReplyListener
	{
		public Reply()
		{
		}

		/// <summary>Returns the reply corresponding to a <code>Channel.sendMSG</code>
		/// call as a <code>Message</code>.  Always call <code>hasNext</code>
		/// previous to calling <code>getNextReply</code> to discover whether or not
		/// <code>getNextReply</code> should be called again.
		/// If all messages for this reply have been returned, a subsequent call
		/// will throw a <code>NoSuchElementException</code>.</summary>
		/// <returns>Message Contains the reply to the previous request.</returns>
		/// <seealso cref="Message"></seealso>
		/// <seealso cref="#hasNext"></seealso>
		/// 
		/// <exception cref="BEEPException" />
		/// <exception cref="NoSuchElementException">If the reply is complete and no more
		/// <code>Message</code>s can be returned.</exception>
		virtual public IMessage getNextReply()
		{
			lock (this)
			{
				if (this.replies.Count == 0)
				{
					if (this.complete)
					{
						throw new System.ArgumentOutOfRangeException();
					}
					
					try
					{
						System.Threading.Monitor.Wait(this);
					}
					catch (System.Threading.ThreadInterruptedException x)
					{
						throw new BEEPInterruptedException(x.Message);
					}
				}
				
				IMessage message = (IMessage) this.replies[0];
				this.replies.RemoveAt(0);
				
				if (message.MessageType != core.MessageType.MESSAGE_TYPE_ANS)
				{
					this.complete = true;
				}
				
				return message;
			}
		}

		private void addReply(IMessage msg)
		{
			lock (this)
			{
				this.replies.Add(msg);
				System.Threading.Monitor.Pulse(this);
			}
		}

		// Constants
		private const int DEFAULT_ARRAY_LIST_SIZE = 4;
		
		// Data
		private bool complete = false;

		private ArrayList replies = new ArrayList();
		
		/// <summary>Indicates if there are more messages to retrive.  While
		/// <code>hasNext</code> returns true the reply to the previous
		/// <code>sendMSG</code> is not complete.  Call <code>getNextReply</code>
		/// to return unretrieved messages.</summary>
		/// <seealso cref="#getNextReply"></seealso>
		public virtual bool hasNext()
		{
			lock (this)
			{
				try
				{
					while (replies.Count == 0 && complete == false)
					{
						System.Threading.Monitor.Wait(this);
					}
				}
				catch (System.Threading.ThreadInterruptedException x)
				{
					throw new BEEPInterruptedException(x.Message);
				}
				
				return replies.Count > 0;
			}
		}
		
		// Implementation of method declared in ReplyListener
		public virtual void  receiveRPY(IMessage message)
		{
			addReply(message);
		}
		
		// Implementation of method declared in ReplyListener
		public virtual void  receiveERR(IMessage message)
		{
			addReply(message);
		}
		
		// Implementation of method declared in ReplyListener
		public virtual void  receiveANS(IMessage message)
		{
			addReply(message);
		}
		
		// Implementation of method declared in ReplyListener
		public virtual void  receiveNUL(IMessage message)
		{
			lock (this)
			{
				this.complete = true;
				System.Threading.Monitor.Pulse(this);
			}
		}
	}
}