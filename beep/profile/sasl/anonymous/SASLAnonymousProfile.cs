/*
* SASLAnonymousProfile.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
using Log = org.apache.commons.logging.Log;
using LogFactory = org.apache.commons.logging.LogFactory;
using org.beepcore.beep.core;
using org.beepcore.beep.profile;
using org.beepcore.beep.profile.sasl;
namespace org.beepcore.beep.profile.sasl.anonymous
{
	
	
	/// <summary> This class implements the Anonymous SASL mechanism
	/// as an extension of the base SASL profile.
	/// 
	/// It uses the TuningProfile methods begin, abort, complete
	/// and doesn't really have to do any SASL message exchange really
	/// But you could write one that did, and see, the nice thing
	/// is that with the CCLs, you can register whatever you want
	/// as the profile handler. It just expects (sort of) to find some
	/// user info in the blob...as who you're authenticating as.
	/// 
	/// </summary>
	/// <author>  Eric Dixon
	/// </author>
	/// <author>  Huston Franklin
	/// </author>
	/// <author>  Jay Kint
	/// </author>
	/// <author>  Scott Pead
	/// </author>
	/// <version>  $Revision: 1.1 $, $Date: 2004/09/21 08:46:58 $
	/// 
	/// </version>
	public class SASLAnonymousProfile:SASLProfile, Profile, StartChannelListener
	{
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
		}
		/// <summary> Method decodeData
		/// 
		/// 
		/// </summary>
		/// <param name="">data
		/// 
		/// @throws SASLException
		/// 
		/// </param>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'getInstance'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal static SASLAnonymousProfile Instance
		{
			get
			{
				lock (typeof(org.beepcore.beep.profile.sasl.anonymous.SASLAnonymousProfile))
				{
					if (instance == null)
					{
						instance = new SASLAnonymousProfile();
					}
					return instance;
				}
			}
			
		}
		
		// Constants
		public const System.String uri = "http://iana.org/beep/SASL/ANONYMOUS";
		public const System.String ANONYMOUS = "anonymous";
		public const System.String MECHANISM = "SASL/ANONYMOUS";
		private static SASLAnonymousProfile instance = null;
		
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		new private Log log;
		
		public SASLAnonymousProfile()
		{
			InitBlock();
			// @todo Probably do this slightly differently, although
			// the static routines make it tougher.
			instance = this;
		}
		
		public virtual StartChannelListener init(System.String uri, ProfileConfiguration config)
		{
			return this;
		}
		
		/// <summary> Our extension of Start Channel (see ChannelControlListener)
		/// does a lot of things.  It begins the authentication, and in
		/// some cases (if the user has packed data in <blob> form on
		/// the startChannel request) can actually finish the anonymous
		/// authentication.
		/// </summary>
		public virtual void  startChannel(Channel channel, System.String encoding, System.String data)
		{
			log.debug("SASLAnonymousProfile.startChannel");
			clearCredential(channel.Session, this);
			Session t = channel.Session;
			
			try
			{
				AnonymousAuthenticator auth = new AnonymousAuthenticator(this);
				auth.started(channel);
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				channel.Session.terminate(x.Message);
				return ;
			}
		}
		
		public virtual void  closeChannel(Channel channel)
		{
		}
		
		public virtual bool advertiseProfile(Session session)
		{
			return true;
		}
		
		/// <summary> Method authencitateSASLAnonymous is an Initiator routine designed
		/// to allow a peer to authenticate to another one. 
		/// 
		/// </summary>
		/// <param name="session">Session the current session
		/// </param>
		/// <param name="id">The identity of the peer withing to authenticate
		/// 
		/// @throws SASLException if any failure occurs.
		/// </param>
		public static Session AuthenticateSASLAnonymous(Session session, System.String id)
		{
			if ((System.Object) id == null)
			{
				id = ANONYMOUS;
			}
			
			clearCredential(session, null);
			
			AnonymousAuthenticator auth = new AnonymousAuthenticator(Instance);
			Channel ch = session.startChannel(SASLAnonymousProfile.uri, auth);
			auth.started(ch);
			auth.sendIdentity(id);
			lock (auth)
			{
				try
				{
					System.Threading.Monitor.Wait(auth);
					
					//FIX for bug 469725, if authentication fails no local Cred is
					//set and a AuthenticationFailureException is thrown
					if (ch.Session.getLocalCredential() == null)
					{
						throw new AuthenticationFailureException("Could not authenticate with SASL/ANON");
					}
					
					return ch.Session;
				}
				catch (System.Threading.ThreadInterruptedException x)
				{
				}
			}
			return null;
		}
		
		/// <summary> Method authencitateSASLAnonymousPiggyback is an Initiator
		/// routine designed to allow a peer to authenticate to another
		/// one.  It is distinct in that it piggybacks the data for the
		/// authentication request on the startChannel request.
		/// 
		/// </summary>
		/// <param name="session">Session the current session
		/// </param>
		/// <param name="id">The identity of the peer withing to authenticate
		/// 
		/// @throws SASLException if any failure occurs.
		/// </param>
		public static Session AuthenticateSASLAnonymousPiggyback(Session session, System.String id)
		{
			if ((System.Object) id == null)
			{
				id = ANONYMOUS;
			}
			
			clearCredential(session, null);
			
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
			Channel ch = session.startChannel(SASLAnonymousProfile.uri, false, new Blob(Blob.STATUS_NONE, id).ToString());
			
			if (((System.Object) ch.StartData != null) && (ch.StartData.IndexOf("<error ") != - 1))
			{
				throw new BEEPException(ch.StartData);
			}
			
			System.String goo = ch.StartData;
			
			if ((System.Object) goo != null)
			{
				Blob blob = new Blob(goo);
				if (blob.Status.Equals(Blob.COMPLETE))
				{
					Instance.finishInitiatorAuthentication(generateCredential(id), ch.Session);
					return session;
				}
			}
			// @todo allow non-piggybacked
			throw new BEEPException("Auth failed.");
		}
		
		/// <summary> Method generateCredential simply generates a SASLAnonymous
		/// style credential.
		/// </summary>
		internal static SessionCredential generateCredential()
		{
			return generateCredential(ANONYMOUS);
		}
		
		/// <summary> Method generateCredential simply generates a SASLAnonymous
		/// style credential.
		/// </summary>
		internal static SessionCredential generateCredential(System.String id)
		{
			if ((System.Object) id == null)
			{
				id = ANONYMOUS;
			}
			
			System.Collections.Hashtable ht = new System.Collections.Hashtable(4);
			
			SupportClass.PutElement(ht, SessionCredential.AUTHENTICATOR, id);
			SupportClass.PutElement(ht, SessionCredential.AUTHENTICATOR_TYPE, MECHANISM);
			
			return new SessionCredential(ht);
		}
		
		/// <summary> Method finishInitiatorAuthentication
		/// 
		/// see SASLProfile's version of this.
		/// </summary>
		protected internal override void  finishInitiatorAuthentication(SessionCredential c, Session s)
		{
			base.finishInitiatorAuthentication(c, s);
		}
		
		/// <summary> Method finishListenerAuthentication
		/// 
		/// see SASLProfile's version of this.
		/// </summary>
		protected internal override void  finishListenerAuthentication(SessionCredential c, Session s)
		{
			base.finishListenerAuthentication(c, s);
		}
	}
}