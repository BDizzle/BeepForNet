/*
* TLSProfileHandshakeCompletedListener.java  $Revision: 1.1 $ $Date: 2004/09/21 08:47:00 $
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
using org.beepcore.beep.core;
namespace org.beepcore.beep.profile.tls
{
	
	
	/// <summary> receives handshake completed events.  This allows the application to
	/// examine the trust of the authentication and take appropriate action as
	/// necessary, such as alter the profiles on the session, or simply close it.
	/// </summary>
	/// <seealso cref="TLSProfile">
	/// </seealso>
	/// <seealso cref="org.beepcore.beep.core.Session">
	/// </seealso>
	/// <seealso cref="javax.net.ssl.HandshakeCompletedListener">
	/// </seealso>
	public interface TLSProfileHandshakeCompletedListener
		{
			
			/// <summary> called after the SSL handshake has completed to allow verification
			/// and fine tuning by the application.
			/// </summary>
			/// <param name="session">The BEEP Session on which the TLS profile channel was initiated
			/// </param>
			/// <param name="event">Event passed by the <code>SSLSocket</code> when the handshake was completed.
			/// </param>
			/// <seealso cref="javax.net.ssl.HandshakeCompletedEvent">
			/// </seealso>
			/// <seealso cref="org.beepcore.beep.core.Session">
			/// </seealso>
			//UPGRADE_TODO: Class 'javax.net.ssl.HandshakeCompletedEvent' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			bool handshakeCompleted(Session session, HandshakeCompletedEvent event_Renamed);
		}
}