/*
* TuningProfile.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
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
namespace beepcore.beep.core
{
	
	/// <summary>TuningProfiles change the security of a Session, either by
	/// negotiating a security layer (such as encryption or some integrity
	/// enhancing frame checksum), or by authenticating one or more of the
	/// individual peers involved in the session.
	/// 
	/// The TuningProfile class provides a nice place for constant data,
	/// shared routines used by its subclasses (blob manipulation, base64
	/// encoding), and so on.
	/// 
	/// SASL Authentication goes through about 4 states, two of them
	/// which are terminal
	/// (abort)    (terminal state)
	/// (complete) (terminal state)
	/// (begin)
	/// (continue)</summary>
	public abstract class TuningProfile
	{
		// Constants
		private const string TLS_URI = "http://iana.org/beep/TLS";
		
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		// Data
		// If the channel has begun tuning, it's in the table.
		// If it's been aborted or gets completed, it's removed.
		private ArrayList tuningChannels = new ArrayList();
		
		/// <summary>Constructor TuningProfile</summary>
		public TuningProfile()
		{
			log = LogFactory.getLog(this.GetType());
		}
		
		/// <summary>Method abort</summary>
		/// <param name="error"></param>
		/// <param name="channel"></param>
		/// <exception cref="BEEPException" />
		public virtual void  abort(BEEPError error, IChannel channel)
		{
			tuningChannels.Remove(channel);
			log.debug("TuningProfile.abort");
			
			// Log entry or something - throw an exception???
		}
		
		// Default Profile Methods
		
		/// <summary>Method begin</summary>
		/// <param name="channel"></param>
		/// <param name="profile"></param>
		/// <param name="data"></param>
		/// <exception cref="BEEPException" />
		public virtual void begin(IChannel channel, string profile, string data)
		{
			log.debug("TuningProfile.begin");
			
			SessionImpl session = (SessionImpl) channel.Session;
			
			try
			{
				if(!tuningChannels.Contains(channel))
					tuningChannels.Add(channel);
				((ChannelImpl) channel).setState(core.ChannelState.STATE_TUNING);
				session.sendProfile(profile, data, (ChannelImpl) channel);
				((ChannelImpl) channel).setState(core.ChannelState.STATE_ACTIVE);
				session.disableIO();
			}
			catch (System.Exception x)
			{
				
				// If we're here, the profile didn't succesfully send, so
				// send an error
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				BEEPError error = new BEEPError(BEEPStatusCode.REQUESTED_ACTION_ABORTED, "UnknownError" + x.Message);
				
				session.sendProfile(profile, error.createErrorMessage(), (ChannelImpl) channel);
				abort(error, channel);
			}
		}
		
		protected internal virtual void  begin(IChannel channel)
		{
			log.debug("TuningProfile.begin");
			
			SessionImpl session = (SessionImpl) channel.Session;
			
			((ChannelImpl) channel).setState(core.ChannelState.STATE_TUNING);
		}
		
		/// <summary>Method complete</summary>
		/// <exception cref="BEEPException" />
		public virtual void  complete(IChannel channel, SessionCredential localCred, SessionCredential peerCred, SessionTuningProperties tuning, ProfileRegistry registry, object argument)
		{
			try
			{
				log.debug("TuningProfile.complete");
				
				SessionImpl s = (SessionImpl) channel.Session;
				
				s.reset(localCred, peerCred, tuning, registry, argument);
				tuningChannels.Remove(channel);
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				abort(new BEEPError(BEEPStatusCode.REQUESTED_ACTION_ABORTED, "TuningProfile.complete failure\n" + x.Message), channel);
			}
		}
		
		// Session calls exposed only through the tuning profile
		
		/// <summary>Method disableIO</summary>
		/// <param name="session"></param>
		protected internal static void  disableIO(ISession session)
		{
			((SessionImpl) session).disableIO();
		}
		
		/// <summary>Method enableIO</summary>
		/// <param name="session"></param>
		protected internal static void  enableIO(ISession session)
		{
			((SessionImpl) session).enableIO();
		}
		
		/// <summary>Method setLocalCredential</summary>
		/// <param name="session"></param>
		/// <param name="credential"></param>
		protected internal static void  setLocalCredential(ISession session, SessionCredential credential)
		{
			((SessionImpl) session).setLocalCredential(credential);
		}
		
		/// <summary>Method setPeerCredential</summary>
		/// <param name="session"></param>
		/// <param name="credential"></param>
		protected internal static void  setPeerCredential(ISession session, SessionCredential credential)
		{
			((SessionImpl) session).setPeerCredential(credential);
		}
		
		/// <summary>Method reset
		/// </summary>
		/// <exception cref="BEEPException" />
		protected internal static ISession reset(ISession session, SessionCredential localCred, SessionCredential peerCred, SessionTuningProperties tuning, ProfileRegistry registry, object argument)
		{
			return ((SessionImpl) session).reset(localCred, peerCred, tuning, registry, argument);
		}
		
		/// <summary>Method sendProfile
		/// </summary>
		/// <exception cref="BEEPException" />
		protected internal static void  sendProfile(ISession session, string uri, string data, IChannel channel)
		{
			((SessionImpl) session).sendProfile(uri, data, (ChannelImpl) channel);
			((ChannelImpl) channel).setState(core.ChannelState.STATE_ACTIVE);
		}
		
#if MESSAGELISTENER
		public virtual Channel startChannel(Session session, string profile, bool base64Encoding, string data, MessageListener listener)
		{
			StartChannelProfile p = new StartChannelProfile(profile, base64Encoding, data);
			ArrayList l = new ArrayList();
			
			l.Add(p);
			
			return ((SessionImpl) session).startChannelRequest(l, listener, true);
		}
#endif

		public virtual IChannel startChannel(ISession session, string profile, bool base64Encoding, string data, IRequestHandler handler)
		{
			StartChannelProfile p = new StartChannelProfile(profile, base64Encoding, data);
			ArrayList l = new ArrayList();
			
			l.Add(p);
			
			return ((SessionImpl) session).startChannelRequest(l, handler, true);
		}
	}
}