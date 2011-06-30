/*
* Message.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2003 Huston Franklin.  All rights reserved.
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
	
	
	/// <summary>Message encapsulates the BEEP MSG, RPY, ERR and NUL message types.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class MessageImpl : IMessage
	{
		/// <summary>Returns <code>InputDataStream</code> belonging to <code>Message</code>.</summary>
		/// <seealso cref="InputDataStream"></seealso>
		virtual public InputDataStream DataStream
		{
			get
			{
				return this.data;
			}
			
		}
		/// <summary>Returns the <code>Channel</code> to which this <code>Message</code>
		/// belongs.</summary>
		/// <seealso cref="Channel"></seealso>
		virtual public IChannel Channel
		{
			get
			{
				return this.channel;
			}
			
		}
		/// <summary>Returns the message number of this <code>Message</code>.</summary>
		virtual public int Msgno
		{
			get
			{
				return this.msgno;
			}
			
		}
		/// <summary>Returns the answer number of this <code>Message</code>.</summary>
		virtual public int Ansno
		{
			get
			{
				return this.ansno;
			}
			
		}
		/// <summary>Returns the message type of this <code>Message</code>.</summary>
		virtual public core.MessageType MessageType
		{
			get
			{
				return this.messageType;
			}
			
		}
		
		/// <summary>BEEP message type of  <code>Message</code>. </summary>
		internal core.MessageType messageType = core.MessageType.MESSAGE_TYPE_UNK;
		
		/// <summary><code>Channel</code> to which <code>Message</code> belongs. </summary>
		internal ChannelImpl channel;
		
		/// <summary>Message number of <code>Message</code>. </summary>
		internal int msgno;
		
		/// <summary>Answer number of this BEEP message. </summary>
		internal int ansno;
				
		private const string NOT_MESSAGE_TYPE_MSG = "Message is not of type MSG";
		
		private bool notified = false;
		
		/// <summary>Payload of the <code>Message</code> stored as a
		/// <code>InputDataStream</code></summary>
		/// <seealso cref="beepcore.beep.core.InputDataStream"></seealso>
		private InputDataStream data;
		
		/// <summary>Creates a new <code>Message</code>.</summary>
		/// <param name="channel"><code>Channel</code> to which this <code>Message</code>
		/// belongs.</param>
		/// <param name="msgno">Message number of the BEEP message.</param>
		/// <param name="data"><code>InputDataStream</code> containing the payload of the
		/// message.</param>
		/// <param name="messageType">Message type of the BEEP message.</param>
		/// <seealso cref="InputDataStream"></seealso>
		/// <seealso cref="Channel"></seealso>
		internal MessageImpl(ChannelImpl channel, int msgno, InputDataStream data, core.MessageType messageType)
		{
			this.channel = channel;
			this.msgno = msgno;
			this.ansno = -1;
			this.data = data;
			this.messageType = messageType;
		}
		
		/// <summary>Creates a BEEP message of type ANS</summary>
		/// <param name="channel"><code>Channel</code> to which the message belongs.</param>
		/// <param name="msgno">Message number of the message.</param>
		/// <param name="ansno"></param>
		/// <param name="data"><code>InputDataStream</code> contains the payload of the
		/// message.</param>
		/// <seealso cref="Channel"></seealso>
		/// <seealso cref="InputDataStream"></seealso>
		internal MessageImpl(ChannelImpl channel, int msgno, int ansno, InputDataStream data):this(channel, msgno, data, core.MessageType.MESSAGE_TYPE_ANS)
		{
			
			this.ansno = ansno;
		}
		
		/// <summary>Sends a message of type ANS.</summary>
		/// <param name="stream">Data to send in the form of <code>OutputDataStream</code>.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <seealso cref="#sendNUL"></seealso>
		/// <returns>MessageStatus</returns>
		/// 
		/// <exception cref="BEEPException">if an error is encoutered or if messageType is
		/// not MESSAGE_TYPE_MSG.</exception>
		public virtual MessageStatus sendANS(OutputDataStream stream)
		{
			throw new BEEPException(NOT_MESSAGE_TYPE_MSG);
		}
		
		/// <summary>Sends a message of type ERR.</summary>
		/// <param name="error">Error to send in the form of <code>BEEPError</code>.</param>
		/// <seealso cref="BEEPError"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// 
		/// <exception cref="BEEPException">if an error is encoutered or if messageType is
		/// not MESSAGE_TYPE_MSG.</exception>
		public virtual MessageStatus sendERR(BEEPError error)
		{
			throw new BEEPException(NOT_MESSAGE_TYPE_MSG);
		}
		
		/// <summary>Sends a message of type ERR.</summary>
		/// <param name="code"><code>code</code> attibute in <code>error</code> element.</param>
		/// <param name="diagnostic">Message for <code>error</code> element.</param>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// 
		/// <exception cref="BEEPException">if an error is encoutered or if messageType is
		/// not MESSAGE_TYPE_MSG.</exception>
		public virtual MessageStatus sendERR(BEEPStatusCode code, string diagnostic)
		{
			throw new BEEPException(NOT_MESSAGE_TYPE_MSG);
		}
		
		/// <summary>Sends a message of type ERR.</summary>
		/// <param name="code"><code>code</code> attibute in <code>error</code> element.</param>
		/// <param name="diagnostic">Message for <code>error</code> element.</param>
		/// <param name="xmlLang"><code>xml:lang</code> attibute in <code>error</code>
		/// element.</param>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// 
		/// <exception cref="BEEPException">if an error is encoutered or if messageType is
		/// not MESSAGE_TYPE_MSG.</exception>
		public virtual MessageStatus sendERR(BEEPStatusCode code, string diagnostic, string xmlLang)
		{
			throw new BEEPException(NOT_MESSAGE_TYPE_MSG);
		}
		
		/// <summary>Sends a message of type NUL.</summary>
		/// <seealso cref="MessageStatus"></seealso>
		/// <seealso cref="#sendANS"></seealso>
		/// <returns>MessageStatus</returns>
		/// 
		/// <exception cref="BEEPException">if an error is encoutered or if messageType is
		/// not MESSAGE_TYPE_MSG.</exception>
		public virtual MessageStatus sendNUL()
		{
			throw new BEEPException(NOT_MESSAGE_TYPE_MSG);
		}
		
		/// <summary>Sends a message of type RPY.</summary>
		/// <param name="stream">Data to send in the form of <code>OutputDataStream</code>.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// 
		/// <exception cref="BEEPException">if an error is encoutered or if messageType is
		/// not MESSAGE_TYPE_MSG.</exception>
		public virtual MessageStatus sendRPY(OutputDataStream stream)
		{
			throw new BEEPException(NOT_MESSAGE_TYPE_MSG);
		}
		
		internal virtual bool isNotified()
		{
			return this.notified;
		}
		
		internal virtual void  setNotified()
		{
			this.notified = true;
		}
	}
}