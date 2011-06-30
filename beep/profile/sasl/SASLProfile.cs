/*
* SASLProfile.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
namespace org.beepcore.beep.profile.sasl
{
	
	
	/// <summary> This class is the base SASL Profile implementation.  It's
	/// extended by ANONYMOUS and OTP and EXTERNAL
	/// 
	/// It provides a place for shared data and shared functionality
	/// for sending messages or replies, encoding data, etc.  Some of
	/// these support routine are provided merely because SASLProfile
	/// extends TuningProfile
	/// 
	/// It is anticipated that MECHANISM-specific state associated
	/// with an ongoing SASL connection will be provided by extensions
	/// as we don't want to mandate data structures, storage etc.
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
	public abstract class SASLProfile:TuningProfile
	{
		
		// Constants
		public const System.String COMPLETE = "<blob status='complete'>";
		public const System.String ENCODING_NONE = "none";
		public const System.String ENCODING_BASE64 = "base64";
		//UPGRADE_NOTE: Final was removed from the declaration of 'ENCODING_DEFAULT '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		public static readonly System.String ENCODING_DEFAULT = ENCODING_NONE;
		public const System.String LOCALIZE_DEFAULT = "i-default";
		public const System.String SASL = "sasl";
		public const System.String SASL_STATUS_ABORT = "abort";
		public const System.String SASL_STATUS_COMPLETE = "complete";
		public const System.String SASL_STATUS_CONTINUE = "continue";
		public const System.String SASL_STATUS_NONE = "none";
		
		// Data
		protected internal SASLSessionTable sessionTable;
		
		public SASLProfile()
		{
			sessionTable = new SASLSessionTable();
		}
		
		/// <summary> Method clearCredential simply clears the credentials associated
		/// with a given Session - this is typically done before a new
		/// authentication is attempted.  
		/// @todo make sure the spec allows for multiple authentications..
		/// I seem to remember it saying 'only once' or something like that.
		/// It certainly seems simpler, but if the lib can hack it, then
		/// maybe we should allow it?  Or maybe it's just a good security
		/// constraint.
		/// </summary>
		protected internal static void  clearCredential(Session s, SASLProfile profile)
		{
			
			// @todo bad toad - clean this up with tuning move
			try
			{
				setLocalCredential(s, null);
				setPeerCredential(s, null);
				if (profile != null)
					profile.sessionTable.removeEntry(s);
			}
			catch (System.Exception x)
			{
			}
		}
		
		/// <summary> Method finishInitiatorAuthentication basically says 'we've
		/// authenticated successfully' and calls the tuningprofile
		/// method (exposed by SASLProfile's extension of the core
		/// class TuningProfile) which sets the local credential.  The
		/// session has two credentials, one in each direction, so it's
		/// necessary to differentiate between local credentials and 
		/// credentials associated with the peer in a given session.
		/// </summary>
		protected internal virtual void  finishInitiatorAuthentication(SessionCredential cred, Session s)
		{
			
			// @todo incredibly lame -you'retired, go to bed
			setLocalCredential(s, cred);
		}
		
		/// <summary> Method finishListenerAuthentication basically says 'some peer has
		/// authenticated successfully' and calls the tuningprofile
		/// method (exposed by SASLProfile's extension of the core
		/// class TuningProfile) which sets the peer credential.  The
		/// session has two credentials, one in each direction, so it's
		/// necessary to differentiate between local credentials and 
		/// credentials associated with the peer in a given session.
		/// </summary>
		protected internal virtual void  finishListenerAuthentication(SessionCredential cred, Session s)
		{
			if (cred != null)
			{
				setPeerCredential(s, cred);
				sessionTable.addEntry(s);
			}
		}
		
		protected internal virtual void  failListenerAuthentication(Session session)
		{
			try
			{
				sessionTable.removeEntry(session);
				clearCredential(session, null);
			}
			catch (SASLException x)
			{
			}
		}
	}
}