/*
* ReplyListener.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
namespace beepcore.beep.core
{
	
	
	/// <summary>Provided to allow for the registration of a <code>ReplyListener</code>
	/// per request.
	/// <p>
	/// A <code>ReplyListener</code> receives replies (RPY, ERR, ANS, NUL)
	/// corresponding to an MSG sent with <code>sendMSG</code>.</summary>
	/// <seealso cref="ReplyListener)"></seealso>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision, $Date: 2004/09/21 08:46:56 $</version>
	public interface ReplyListener
	{
		
		/// <summary>Called when the underlying BEEP framework receives a reply of
		/// type RPY.</summary>
		/// <param name="message">BEEP message</param>
		/// <seealso cref="beepcore.beep.core.Message"></seealso>
		/// <exception cref="AbortChannelException" />
		void  receiveRPY(IMessage message);
		
		/// <summary>Called when the underlying BEEP framework receives a reply of
		/// type ERR.</summary>
		/// <param name="message">BEEP message</param>
		/// <seealso cref="beepcore.beep.core.Message"></seealso>
		/// <exception cref="AbortChannelException" />
		void  receiveERR(IMessage message);
		
		/// <summary>Called when the underlying BEEP framework receives a reply of
		/// type ANS.</summary>
		/// <param name="message">BEEP message</param>
		/// <seealso cref="beepcore.beep.core.Message"></seealso>
		/// <exception cref="AbortChannelException" />
		void  receiveANS(IMessage message);
		
		/// <summary>Called when the underlying BEEP framework receives a reply of
		/// type NUL.</summary>
		/// <param name="message">BEEP message</param>
		/// <seealso cref="beepcore.beep.core.Message"></seealso>
		/// <exception cref="AbortChannelException" />
		void  receiveNUL(IMessage message);
		
		// @todo do any of the above need to throw BEEPException???
	}
}