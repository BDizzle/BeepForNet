/*
* SASLSessionTable.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
using Log = org.apache.commons.logging.Log;
using LogFactory = org.apache.commons.logging.LogFactory;
using Session = org.beepcore.beep.core.Session;
using SessionCredential = org.beepcore.beep.core.SessionCredential;
using SessionEvent = org.beepcore.beep.core.@event.SessionEvent;
using SessionResetEvent = org.beepcore.beep.core.@event.SessionResetEvent;
using SessionListener = org.beepcore.beep.core.@event.SessionListener;
using SASLAnonymousProfile = org.beepcore.beep.profile.sasl.anonymous.SASLAnonymousProfile;
namespace org.beepcore.beep.profile.sasl
{
	
	/// <summary> This class is provided to give the SASL profiles a way
	/// to record what other peers have authenticated to the peer
	/// they're serving.  
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
	public class SASLSessionTable : SessionListener
	{
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
		}
		private const int DEFAULT_SIZE = 4;
		
		private const System.String ERR_INVALID_PARAMETERS = "Invalid parameters to Session Table call";
		private const System.String MSG_SESSIONS_TABLE_HEADER = "===] BEEP Peer Session Table";
		private const System.String MSG_EMPTY = "===]  NONE";
		private const System.String MSG_MECHANISM_PREFIX = " Mechanism=>";
		private const System.String MSG_USER_PREFIX = "===]  User=>";
		private const System.String MSG_SESSIONS_TABLE_TRAILER = "===] End of Table";
		
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		private System.Collections.Hashtable nameToSession, sessionToName;
		
		internal SASLSessionTable()
		{
			InitBlock();
			sessionToName = new System.Collections.Hashtable(DEFAULT_SIZE);
			nameToSession = new System.Collections.Hashtable(DEFAULT_SIZE);
		}
		
		/// <summary> Method getSession locates a session based on the 
		/// authenticator
		/// 
		/// 
		/// 
		/// </summary>
		internal virtual Session getSession(System.String authenticator)
		{
			if ((System.Object) authenticator == null)
				throw new SASLException(ERR_INVALID_PARAMETERS);
			return (Session) nameToSession[authenticator];
		}
		
		/// <summary> Method isAuthenticated is used internally to find out
		/// whether or not a user has authenticated already with
		/// a given identity.
		/// </summary>
		internal virtual bool isAuthenticated(System.String authenticator)
		{
			if ((System.Object) authenticator == null)
				throw new SASLException(ERR_INVALID_PARAMETERS);
			return (nameToSession[authenticator] != null);
		}
		
		/// <summary> Method addEntry, adds information to the SASLSession
		/// table to track what sessions have been authenicated
		/// with what critera.
		/// </summary>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'addEntry'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		protected internal virtual void  addEntry(Session session)
		{
			lock (this)
			{
				bool anonymous = false;
				
				if (session == null)
					throw new SASLException(ERR_INVALID_PARAMETERS);
				
				SessionCredential cred = session.getPeerCredential();
				if (cred == null)
					throw new SASLException(ERR_INVALID_PARAMETERS);
				
				anonymous = cred.AuthenticatorType.Equals(SASLAnonymousProfile.MECHANISM);
				
				// If this session is mapped to another user, get rid of it.
				System.String authenticator = (System.String) sessionToName[session];
				
				// If it's OTP, store it both ways.
				if (anonymous)
				{
					// This will be in sessionToName by session
					SupportClass.HashtableRemove(sessionToName, session);
				}
				else if ((System.Object) authenticator != null)
				{
					SupportClass.HashtableRemove(sessionToName, session);
					SupportClass.HashtableRemove(nameToSession, authenticator);
				}
				
				// Get the new credential
				authenticator = session.getPeerCredential().Authenticator;
				if ((System.Object) authenticator == null)
					throw new SASLException(ERR_INVALID_PARAMETERS);
				session.addSessionListener(this);
				if (sessionToName.Contains(session))
					SupportClass.HashtableRemove(nameToSession, authenticator);
				SupportClass.PutElement(sessionToName, session, authenticator);
				if (!anonymous)
					SupportClass.PutElement(nameToSession, authenticator, session);
				printContents();
			}
		}
		
		/// <summary> Method removeEntry removes SASL/Authenticator data from
		/// the SASLSession table.  Called when sessions terminate,
		/// or when credentials are 'cleared'.  
		/// </summary>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'removeEntry'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		protected internal virtual void  removeEntry(Session session)
		{
			lock (this)
			{
				if (session == null)
					throw new SASLException(ERR_INVALID_PARAMETERS);
				
				if (session.getPeerCredential() != null)
				{
					System.String authenticator = session.getPeerCredential().Authenticator;
					if ((System.Object) authenticator == null)
						throw new SASLException(ERR_INVALID_PARAMETERS);
					SupportClass.HashtableRemove(nameToSession, authenticator);
				}
				SupportClass.HashtableRemove(sessionToName, session);
				session.removeSessionListener(this);
				printContents();
			}
		}
		
		public virtual void  greetingReceived(SessionEvent e)
		{
		}
		
		/// <summary> Method receiveEvent is implemented here so the SASLSessionTable
		/// can receive events when a session is terminated (so that it
		/// can update its information about what sessions are actively
		/// authenticated etc.
		/// 
		/// </summary>
		/// <param name="event">event the SessionEvent used.
		/// </param>
		public virtual void  sessionClosed(SessionEvent event_Renamed)
		{
			try
			{
				//UPGRADE_NOTE: The method 'java.util.EventObject.Source' needs to be in a event handling method in order to be properly converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1171"'
				Session s = (Session) event_Renamed.Source;
				removeEntry(s);
			}
			catch (System.InvalidCastException x)
			{
			}
			catch (SASLException x)
			{
				log.error("Error removing entry", x);
			}
		}
		
		/// <summary> Method receiveEvent is implemented here so the SASLSessionTable
		/// can receive events when a session is reset (so that it
		/// can update its information about what sessions are actively
		/// authenticated etc.
		/// 
		/// </summary>
		/// <param name="event">event the SessionResetEvent used.
		/// </param>
		public virtual void  sessionReset(SessionResetEvent event_Renamed)
		{
			try
			{
				//UPGRADE_NOTE: The method 'java.util.EventObject.Source' needs to be in a event handling method in order to be properly converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1171"'
				removeEntry((Session) event_Renamed.Source);
				addEntry(event_Renamed.NewSession);
			}
			catch (SASLException e)
			{
				log.error("Error replacing entry", e);
			}
		}
		
		/// <summary> Method printContents does a simple dump of the SASLSessionTable
		/// for the purposes of monitoring or debugging.
		/// </summary>
		internal virtual void  printContents()
		{
			log.debug(MSG_SESSIONS_TABLE_HEADER);
			if (sessionToName.Count == 0)
			{
				log.debug(MSG_EMPTY);
			}
			else
			{
				System.Collections.IEnumerator e = sessionToName.Keys.GetEnumerator();
				//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				while (e.MoveNext())
				{
					//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
					Session s = (Session) e.Current;
					System.String user = (System.String) sessionToName[s];
					System.String mech;
					if (s.getPeerCredential() != null)
						mech = s.getPeerCredential().AuthenticatorType;
					else
						mech = "UNKNOWN";
					
					if (log.isDebugEnabled())
					{
						log.debug(MSG_USER_PREFIX + user + MSG_MECHANISM_PREFIX + mech);
					}
				}
			}
			log.debug(MSG_SESSIONS_TABLE_TRAILER);
		}
	}
}