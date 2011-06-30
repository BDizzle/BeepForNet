/*
* Channel.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
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
	
	
	/// <summary>This interface represents the operations available for all BEEP Channels.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public enum ChannelState
	{
		STATE_INITIALIZED = 0,
		STATE_STARTING = 1,
		STATE_ACTIVE = 2,
		STATE_TUNING_PENDING = 3,
		STATE_TUNING = 4,
		STATE_CLOSE_PENDING = 5,
		STATE_CLOSING = 6,
		STATE_CLOSED = 7,
		STATE_ABORTED = 8,
	}
	public interface IChannel
	{
		/// <summary>Returns or sets application context data</summary>
		object AppData
		{
			get;
			
			set;
		}

		/// <summary>Return the number of this <code>Channel</code>.</summary>
		int Number
		{
			get;
			
		}
		/// <summary>Returns the session for this channel.</summary>
		ISession Session
		{
			get;
		}

		/// <deprecated></deprecated>
		string StartData
		{
			get;
			set;
		}
		
		/// <summary>Closes the channel.</summary>
		/// <exception cref="BEEPException" />
		void  close();

#if MESSAGELISTENER
		/// <summary>Sets the <code>MessageListener</code> for this channel.</summary>
		/// <param name="listener"></param>
		/// <returns>The previous MessageListener or null if none was set.</returns>
		/// <deprecated></deprecated>
		MessageListener setMessageListener(MessageListener listener);
		
		/// <summary>Returns the message listener for this channel.</summary>
		/// <deprecated></deprecated>
		MessageListener getMessageListener();
#endif

		/// <summary>Returns the <code>RequestHandler</code> registered with this channel.</summary>
		IRequestHandler getRequestHandler();
		
		/// <summary>Sets the MSG handler for this <code>Channel</code>.</summary>
		/// <param name="handler"><code>RequestHandler</code> to handle received
		/// MSG messages.</param>
		/// <returns>The previous <code>RequestHandler</code> or <code>null</code> if
		/// one wasn't set.</returns>
		IRequestHandler setRequestHandler(IRequestHandler handler);
		
		/// <summary>Sets the MSG handler for this <code>Channel</code>.</summary>
		/// <param name="handler"><code>RequestHandler</code> to handle received
		/// MSG messages.</param>
		/// <param name="tuningReset">flag indicating that the profile will request a
		/// tuning reset.</param>
		/// <returns>The previous <code>RequestHandler</code> or <code>null</code> if
		/// one wasn't set.</returns>
		IRequestHandler setRequestHandler(IRequestHandler handler, bool tuningReset);
		
		/// <summary>Returns the state of this channel.</summary>
		core.ChannelState getState();
		
		/// <summary>Sends a MSG message.</summary>
		/// <param name="stream">Data contents of the MSG message to be sent.</param>
		/// <param name="replyListener">A listener to be notified when a reply to this
		/// MSG is received.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		MessageStatus sendMSG(OutputDataStream stream, ReplyListener replyListener);
		
		string getProfile();
	}
}