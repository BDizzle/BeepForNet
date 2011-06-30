/*
* MessageStatus.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
namespace beepcore.beep.core
{
	
	public enum MessageStatusCode 
	{
		/// <summary>Uninitialized BEEP status. </summary>
		MESSAGE_STATUS_UNK = 0,
			
		/// <summary>BEEP message status. </summary>
		MESSAGE_STATUS_NOT_SENT = 1,
			
		/// <summary>BEEP message status. </summary>
		MESSAGE_STATUS_SENT = 2,
			
		/// <summary>BEEP message status. </summary>
		MESSAGE_STATUS_RECEIVED_REPLY = 3,
			
		/// <summary>BEEP message status. </summary>
		MESSAGE_STATUS_RECEIVED_ERROR = 4
	}
	
	/// <summary></summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision, $Date: 2004/09/21 08:46:56 $</version>
	public class MessageStatus
	{
		/// <summary>Returns the answer number.</summary>
		virtual public int Ansno
		{
			get
			{
				return this.ansno;
			}
			
		}
		/// <summary>Returns the channel.</summary>
		virtual public IChannel Channel
		{
			get
			{
				return this.channel;
			}
			
		}
		/// <summary>Returns the message data.</summary>
		virtual public OutputDataStream MessageData
		{
			get
			{
				return this.data;
			}
			
		}
		/// <summary>Returns the message number.</summary>
		virtual public int Msgno
		{
			get
			{
				return this.msgno;
			}
			
		}
		virtual internal core.MessageType MessageType
		{
			get
			{
				return this.messageType;
			}
			
		}
		/// <summary>Method getListener</summary>
		virtual internal ReplyListener ReplyListener
		{
			get
			{
				return this.replyListener;
			}
			
		}
		
		/// <summary>Status of message. </summary>
		private core.MessageStatusCode messageStatusCode = core.MessageStatusCode.MESSAGE_STATUS_UNK;

		private IChannel channel;
		private MessageType messageType;
		private int msgno;
		private int ansno;
		private OutputDataStream data;
		private ReplyListener replyListener;
		
		internal MessageStatus(IChannel channel, MessageType messageType, int msgno, OutputDataStream data):this(channel, messageType, msgno, - 1, data, null)
		{
		}
		
		internal MessageStatus(IChannel channel, MessageType messageType, int msgno, OutputDataStream data, ReplyListener replyListener):this(channel, messageType, msgno, - 1, data, replyListener)
		{
		}
		
		internal MessageStatus(IChannel channel, MessageType messageType, int msgno, int ansno, OutputDataStream data):this(channel, messageType, msgno, ansno, data, null)
		{
		}
		
		internal MessageStatus(IChannel channel, MessageType messageType, int msgno, int ansno, OutputDataStream data, ReplyListener replyListener)
		{
			this.channel = channel;
			this.messageType = messageType;
			this.msgno = msgno;
			this.ansno = ansno;
			this.data = data;
			this.replyListener = replyListener;
		}
		
		/// <summary>Returns the message status.</summary>
		public virtual core.MessageStatusCode MessageStatusCode
		{
			get 
			{
				return this.messageStatusCode;
			}
			set 
			{
				this.messageStatusCode = value;
			}
		}
		
	}
}