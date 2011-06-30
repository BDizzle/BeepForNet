/*
* MessageMSG.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
	
	/// <summary>Represents received BEEP MSG messages. Provides methods to reply to
	/// the MSG.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	class MessageMSGImpl:MessageImpl, IMessageMSG
	{
		internal MessageMSGImpl(ChannelImpl channel, int msgno, InputDataStream data):base(channel, msgno, data, core.MessageType.MESSAGE_TYPE_MSG)
		{
		}
		
		/// <summary>Sends a message of type ANS.</summary>
		/// <param name="stream">Data to send in the form of <code>OutputDataStream</code>.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <seealso cref="#sendNUL"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		public override MessageStatus sendANS(OutputDataStream stream)
		{
			MessageStatus m;
			
			lock (this)
			{
				// reusing ansno (initialized to -1) from Message since
				// this is a MSG
				++ansno;
				
				m = new MessageStatus(this.channel, core.MessageType.MESSAGE_TYPE_ANS, this.msgno, this.ansno, stream);
			}
			
			this.channel.sendMessage(m);
			return m;
		}
		
		/// <summary>Sends a message of type ERR.</summary>
		/// <param name="error">Error to send in the form of <code>BEEPError</code>.</param>
		/// <seealso cref="BEEPError"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		public override MessageStatus sendERR(BEEPError error)
		{
			OutputDataStream stream = new StringOutputDataStream(error.createErrorMessage());
			MessageStatus m = new MessageStatus(this.channel, core.MessageType.MESSAGE_TYPE_ERR, this.msgno, stream);
			this.channel.sendMessage(m);
			return m;
		}
		
		/// <summary>Sends a message of type ERR.</summary>
		/// <param name="code"><code>code</code> attibute in <code>error</code> element.</param>
		/// <param name="diagnostic">Message for <code>error</code> element.</param>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		public override MessageStatus sendERR(BEEPStatusCode code, string diagnostic)
		{
			string error = BEEPError.createErrorMessage(code, diagnostic);
			MessageStatus m = new MessageStatus(this.channel, core.MessageType.MESSAGE_TYPE_ERR, this.msgno, new StringOutputDataStream(error));
			this.channel.sendMessage(m);
			return m;
		}
		
		/// <summary>Sends a message of type ERR.</summary>
		/// <param name="code"><code>code</code> attibute in <code>error</code> element.</param>
		/// <param name="diagnostic">Message for <code>error</code> element.</param>
		/// <param name="xmlLang"><code>xml:lang</code> attibute in <code>error</code>
		/// element.</param>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		public override MessageStatus sendERR(BEEPStatusCode code, string diagnostic, string xmlLang)
		{
			string error = BEEPError.createErrorMessage(code, diagnostic, xmlLang);
			MessageStatus m = new MessageStatus(this.channel, core.MessageType.MESSAGE_TYPE_ERR, this.msgno, new StringOutputDataStream(error));
			this.channel.sendMessage(m);
			return m;
		}
		
		/// <summary>Sends a message of type NUL.</summary>
		/// <seealso cref="MessageStatus"></seealso>
		/// <seealso cref="#sendANS"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		public override MessageStatus sendNUL()
		{
			MessageStatus m = new MessageStatus(this.channel, core.MessageType.MESSAGE_TYPE_NUL, this.msgno, NULDataStream);
			this.channel.sendMessage(m);
			return m;
		}
		
		/// <summary>Sends a message of type RPY.</summary>
		/// <param name="stream">Data to send in the form of <code>OutputDataStream</code>.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		public override MessageStatus sendRPY(OutputDataStream stream)
		{
			MessageStatus m = new MessageStatus(this.channel, core.MessageType.MESSAGE_TYPE_RPY, this.msgno, stream);
			this.channel.sendMessage(m);
			return m;
		}
		
		private static OutputDataStream NULDataStream;
		static MessageMSGImpl()
		{
			{
				NULDataStream = new OutputDataStream();
				NULDataStream.setComplete();
			}
		}
	}
}