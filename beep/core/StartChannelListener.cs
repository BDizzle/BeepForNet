/*
* StartChannelListener.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
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
	
	
	/// <summary>Interface StartChannelListener
	/// 
	/// StartChannelListener is an interface specifying the methods that must
	/// be implemented by any class that implements logic managing the start and
	/// close events on a channel, as well as any other events that may eventually
	/// be associated with profiles.</summary>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	public interface IStartChannelListener
	{
		
		/// <summary>Callback that determines if a profile should be advertised or not.  The
		/// {@link SessionTuningProperties} can be used to determine if
		/// certain attributes are in effect before a profile is advertised,
		/// such as encryption or authentication.</summary>
		/// <param name="session">to check other properties such as the credentials of
		/// of the session</param>
		/// <seealso cref="for standard properties and their values"></seealso>
		bool AdvertiseProfile(ISession session);
		
		/// <summary>Called when the underlying BEEP framework receives
		/// a "start" element.</summary>
		/// <param name="channel">A <code>Channel</code> object which represents a channel
		/// in this <code>Session</code>.</param>
		/// <param name="data">The content of the "profile" element selected for this
		/// channel (may be <code>null</code>).</param>
		/// <param name="encoding">specifies whether the content of the "profile" element
		/// selected for this channel is represented as a base64-encoded string.
		/// The <code>encoding</code> is only valid if <code>data</code> is not
		/// <code>null</code>.
		/// 
		/// <exception cref="StartChannelException">Throwing this exception will cause an
		/// error to be returned to the BEEP peer requesting to start a channel.
		/// The channel is then discarded.</exception>
		void StartChannel(IChannel channel, string encoding, string data);
		
		/// <summary>Called when the underlying BEEP framework receives
		/// a "close" element.</summary>
		/// <param name="channel"><code>Channel</code> which received the close request.
		/// 
		/// <exception cref="CloseChannelException">Throwing this exception will return an
		/// error to the BEEP peer requesting the close.  The channel will remain
		/// open.</exception>
		void CloseChannel(IChannel channel);
	}
}