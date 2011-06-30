/*
* Session.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
using beepcore.beep.core.@event;

namespace beepcore.beep.core
{
	
	
	/// <summary>This interface represents the operations available for all BEEP Sessions.
	/// <p>
	/// A BEEP Session encapsulates a connection for a given transport.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	/// <seealso cref="Channel"></seealso>
	public enum SessionState
	{
		// Constants
		SESSION_STATE_INITIALIZED = 0,
		SESSION_STATE_GREETING_SENT = 1,
		SESSION_STATE_ACTIVE = 2,
		SESSION_STATE_TUNING_PENDING = 3,
		SESSION_STATE_TUNING = 4,
		SESSION_STATE_CLOSE_PENDING = 5,
		SESSION_STATE_CLOSING = 6,
		SESSION_STATE_CLOSED = 7,
		SESSION_STATE_ABORTED = 8
	}

	public interface ISession
	{
		/// <summary>Get the profiles supported by the remote peer.</summary>

		System.Collections.ICollection PeerSupportedProfiles
		{
			get;
		}

		/// <summary>Get the <code>ProfileRegistry</code> for this <code>Session</code>.</summary>
		/// <seealso cref="ProfileRegistry"></seealso>
		ProfileRegistry ProfileRegistry
		{
			get;
		}

		/// <summary>Returns the state of <code>Session</code>.</summary>
		/// <returns>Session state (see the SESSION_STATE_xxx constants defined
		/// in this interface).</returns>
		core.SessionState State
		{
			get;
		}

		/// <summary>Indicates whehter or not this peer is the initiator of this Session.</summary>
		bool Initiator
		{
			get;
		}
		
		/// <summary>Adds a listener to be notified of Channel events (start, close, etc.).</summary>
		/// <seealso cref="#removeChannelListener"></seealso>
		void  addChannelListener(IChannelListener listener);
		
		/// <summary>Adds a listener to be notified of Session events (greeting, close, etc.).</summary>
		/// <seealso cref="#removeSessionListener"></seealso>
		void  addSessionListener(ISessionListener listener);
		
		/// <summary>Request to close this <code>Session</code> gracefully. The profiles of
		/// the open <code>Channels</code> on this <code>Session</code> may veto
		/// the close request.</summary>
		/// <exception cref="BEEPException" />
		void close();
		
		/// <summary>Get the <code>SessionCredential</code> used to authenticate this peer
		/// of this Session.</summary>
		/// <returns><code>null</code> if this session has not been authenticated</returns>
		SessionCredential getLocalCredential();
		
		/// <summary>Get the <code>SessionCredential</code> used to authenticate the remote
		/// peer of this Session.</summary>
		/// <returns><code>null</code> if this session has not been authenticated</returns>
		SessionCredential getPeerCredential();
		
		/// <summary>Removes the listener from the list of listeners to be notified
		/// of future events. Note that the listener will be notified of
		/// events which have already happened and are in the process of
		/// being dispatched.</summary>
		/// <seealso cref="#addChannelListener"></seealso>
		void  removeChannelListener(IChannelListener l);
		
		/// <summary>Removes the listener from the list of listeners to be notified
		/// of future events. Note that the listener will be notified of
		/// events which have already happened and are in the process of
		/// being dispatched.</summary>
		/// <seealso cref="#addSessionListener"></seealso>
		void  removeSessionListener(ISessionListener l);
		
		/// <summary>Sends a request to start a new Channel on this Session for the
		/// specified profile.</summary>
		/// <param name="profile">The URI of the profile for the new Channel.
		/// 
		/// <exception cref="BEEPError">Thrown if the remote peer is unable or refuses to
		/// start a new Channel for the requested profile.
		/// </exception><exception cref="BEEPException">Thrown for errors other than those defined by
		/// the BEEP protocol (e.g. the Session is not in a
		/// state to create a new Channel).</exception>
		IChannel startChannel(string profile);
	
#if MESSAGELISTENER
		/// <summary>Sends a request to start a new Channel on this Session for the
		/// specified profile. This version of <code>startChannel</code> allows a
		/// <code>MessageListener</code> to be specified to be registered once the
		/// Channel is started. This is useful for profiles that are peer-to-peer in
		/// nature.</summary>
		/// <param name="profile">The URI of the profile for the new Channel.</param>
		/// <param name="listener">A <code>MessageListener</code> to receive MSG messages
		/// sent by the remote peer of this Session.
		/// 
		/// <exception cref="BEEPError">Thrown if the remote peer is unable or refuses to
		/// start a new Channel for the requested profile.
		/// </exception><exception cref="BEEPException">Thrown for errors other than those defined by
		/// the BEEP protocol (e.g. the Session is not in a
		/// state to create a new Channel).</exception>
		/// <seealso cref="MessageListener"></seealso>
		/// <deprecated></deprecated>
		Channel startChannel(string profile, MessageListener listener);
#endif
		
		/// <summary>Sends a request to start a new Channel on this Session for the
		/// specified profile. This version of <code>startChannel</code> allows a
		/// <code>RequestHandler</code> to be specified to be registered once the
		/// Channel is started. This is useful for profiles that are peer-to-peer in
		/// nature.</summary>
		/// <param name="profile">The URI of the profile for the new Channel.</param>
		/// <param name="handler">A <code>RequestHandler</code> to receive MSG messages
		/// sent by the remote peer of this Session.
		/// 
		/// <exception cref="BEEPError">Thrown if the remote peer is unable or refuses to
		/// start a new Channel for the requested profile.
		/// </exception><exception cref="BEEPException">Thrown for errors other than those defined by
		/// the BEEP protocol (e.g. the Session is not in a
		/// state to create a new Channel).</exception>
		IChannel startChannel(string profile, IRequestHandler handler);
		
		/// <summary>Sends a request to start a new Channel on this Session for the
		/// specified profile. This version of <code>startChannel</code> allows a
		/// <code>MessageListener</code> to be specified to be registered once the
		/// Channel is started. This is useful for profiles that are peer-to-peer in
		/// nature.</summary>
		/// <param name="profile">The URI of the profile for the new Channel.</param>
		/// <param name="base64Encoding">Indicates whether or not <code>data</code> is
		/// base64 encoded. <code>data</code> must be base64
		/// encoded if it is not valid XML CDATA.</param>
		/// <param name="data">An initial request to be sent piggyback'd along with the
		/// request to start the Channel. This request can be at most
		/// 4K in size.</param>
		/// 
		/// <exception cref="BEEPError">Thrown if the remote peer is unable or refuses to
		/// start a new Channel for the requested profile.
		/// </exception><exception cref="BEEPException">Thrown for errors other than those defined by
		/// the BEEP protocol (e.g. the Session is not in a
		/// state to create a new Channel).
		/// </exception><seealso cref="MessageListener"></seealso>
		IChannel startChannel(string profile, bool base64Encoding, string data);

#if MESSAGELISTENER
		/// <summary>Sends a request to start a new Channel on this Session for the
		/// specified profile. This version of <code>startChannel</code> allows a
		/// <code>MessageListener</code> to be specified to be registered once the
		/// Channel is started. This is useful for profiles that are peer-to-peer in
		/// nature.</summary>
		/// <param name="profile">The URI of the profile for the new Channel.</param>
		/// <param name="base64Encoding">Indicates whether or not <code>data</code> is
		/// base64 encoded. <code>data</code> must be base64
		/// encoded if it is not valid XML CDATA.</param>
		/// <param name="data">An initial request to be sent piggyback'd along with the
		/// request to start the Channel. This request can be at most
		/// 4K in size.</param>
		/// <param name="listener">A <code>MessageListener</code> to receive MSG messages
		/// sent by the remote peer of this Session.
		/// 
		/// <exception cref="BEEPError">Thrown if the remote peer is unable or refuses to
		/// start a new Channel for the requested profile.
		/// </exception><exception cref="BEEPException">Thrown for errors other than those defined by
		/// the BEEP protocol (e.g. the Session is not in a
		/// state to create a new Channel).</exception>
		/// <seealso cref="MessageListener"></seealso>
		/// <deprecated></deprecated>
		Channel startChannel(string profile, bool base64Encoding, string data, MessageListener listener);
#endif
		
		/// <summary>Sends a request to start a new Channel on this Session for the
		/// specified profile. This version of <code>startChannel</code> allows a
		/// <code>RequestHandler</code> to be specified to be registered once the
		/// Channel is started.</summary>
		/// <param name="profile"></param>
		/// <param name="handler">A <code>RequestHandler</code> to receive MSG messages
		/// sent by the remote peer of this Session.</param>
		/// 
		/// <exception cref="BEEPError">Thrown if the remote peer is unable or refuses to
		/// start a new Channel for the requested profile.
		/// </exception><exception cref="BEEPException">Thrown for errors other than those defined by
		/// the BEEP protocol (e.g. the Session is not in a
		/// state to create a new Channel).</exception>
		IChannel startChannel(StartChannelProfile profile, IRequestHandler handler);
		
		/// <summary>Sends a start channel request using the given list of profiles.</summary>
		/// <param name="profiles">A collection of <code>StartChannelProfile</code>(s).</param>
		/// <param name="handler">A <code>RequestHandler</code> to receive MSG messages
		/// sent by the remote peer of this Session.
		/// 
		/// <exception cref="BEEPError">Thrown if the remote peer is unable or refuses to
		/// start a new Channel for the requested profile.
		/// </exception><exception cref="BEEPException">Thrown for errors other than those defined by
		/// the BEEP protocol (e.g. the Session is not in a
		/// state to create a new Channel).</exception>
		/// <seealso cref="StartChannelProfile"></seealso>
		/// <seealso cref="RequestHandler"></seealso>

		/// <summary>This method is used to terminate the session when there is an
		/// non-recoverable error.</summary>
		/// <param name="reason"></param>
		void  terminate(string reason);
		
		SessionTuningProperties getTuningProperties();
	}
}