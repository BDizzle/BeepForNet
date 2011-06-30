/*
* SASLOTPProfile.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
using org.beepcore.beep.profile.sasl.otp.algorithm;
using MD5 = org.beepcore.beep.profile.sasl.otp.algorithm.md5.MD5;
using SHA1 = org.beepcore.beep.profile.sasl.otp.algorithm.sha1.SHA1;
using org.beepcore.beep.profile.sasl.otp.database;
namespace org.beepcore.beep.profile.sasl.otp
{
	
	
	/// <summary> This class implements the OTP (One-Time-Password) SASL mechanism
	/// as an extension of the base SASL profile.
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
	public class SASLOTPProfile:SASLProfile, StartChannelListener, Profile
	{
		public SASLOTPProfile()
		{
			InitBlock();
		}
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
		}
		internal static UserDatabasePool UserDatabase
		{
			get
			{
				return userDatabase;
			}
			
		}
		virtual public StartChannelListener StartChannelListener
		{
			get
			{
				return this;
			}
			
		}
		
		// Constants
		public const System.String URI = "http://iana.org/beep/SASL/OTP";
		
		// Message Constants
		public const System.String EXT = "ext";
		public const System.String HEX = "hex:";
		public const System.String SPACE = " ";
		public const System.String WORD = "word:";
		public const System.String HEX_INIT = "hex-init:";
		public const System.String OTP_DB_FILENAME = "OTP_DB";
		public const System.String SASL_OTP = "SASLOTPProfile";
		public const System.String WORD_INIT = "word-init:";
		public const System.String MECHANISM = "SASL/OTP";
		
		// Error Constants
		public const System.String ERR_PARSING_DB = "Error parsing OTP DB";
		public const System.String ERR_REJECTED = "Peer rejected SASL-OTP Start Channel Request";
		public const System.String ERR_INVALID_ID = "Invalid or improperly formatted Identity information";
		
		// Instance Data
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		new private Log log;
		
		private System.Collections.Hashtable authenticators;
		private MD5 md5;
		private SHA1 sha1;
		
		// Class Data
		private static System.Collections.Hashtable algorithms;
		private static SASLOTPProfile instance_Renamed_Field;
		private static UserDatabasePool userDatabase;
		
		/// <summary> Method init is used to construct various static data
		/// used in the SASL OTP profile.
		/// </summary>
		public virtual StartChannelListener init(System.String uri, ProfileConfiguration config)
		{
			md5 = new MD5();
			sha1 = new SHA1();
			authenticators = new System.Collections.Hashtable();
			
			if (instance_Renamed_Field == null)
			{
				instance_Renamed_Field = this;
				algorithms = new System.Collections.Hashtable();
				
				SupportClass.PutElement(algorithms, MD5.AlgorithmName, md5);
				SupportClass.PutElement(algorithms, SHA1.AlgorithmName, sha1);
				userDatabase = new UserDatabasePool();
			}
			
			return this;
		}
		
		internal static SASLOTPProfile instance()
		{
			if (instance_Renamed_Field == null)
			{
				throw new SASLException("SASLOTPProfile uninitialized");
			}
			
			return instance_Renamed_Field;
		}
		
		/// @TODO change this to be not static or add a static initializer for algoriths    
		internal static Algorithm getAlgorithm(System.String name)
		{
			System.Object algo = algorithms[name];
			
			if (algo != null)
			{
				return (Algorithm) algo;
			}
			
			return null;
		}
		
		public virtual void  startChannel(Channel channel, System.String encoding, System.String data)
		{
			log.debug("SASL-OTP Start Channel CCL");
			
			clearCredential(channel.Session, this);
			
			OTPAuthenticator temp = new OTPAuthenticator(this);
			try
			{
				channel.setRequestHandler(temp);
				temp.started(channel);
			}
			catch (SASLException x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				throw new StartChannelException(BEEPError.CODE_REQUESTED_ACTION_NOT_TAKEN, x.Message);
			}
			log.debug("Started an SASL-OTP Channel");
		}
		
		public virtual bool advertiseProfile(Session session)
		{
			return true;
		}
		
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'validateIdentity'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		protected internal virtual bool validateIdentity(System.String authenticateId, OTPAuthenticator a)
		{
			lock (this)
			{
				// Do a kind of mutually exclusive bit
				if (authenticators[authenticateId] != null)
				{
					return false;
				}
				SupportClass.PutElement(authenticators, authenticateId, a);
				return true;
			}
		}
		
		public virtual void  closeChannel(Channel channel)
		{
		}
		
		internal virtual OTPAuthenticator startAuthentication(System.String authorizeId, System.String authenticateId, System.String password)
		{
			UserDatabase db = userDatabase.getUser(authenticateId);
			
			if (db == null)
			{
				throw new SASLException("User OTP data not found");
			}
			
			OTPAuthenticator a = new OTPAuthenticator(this, db, password, authorizeId, authenticateId);
			return a;
		}
		
		// Used only for init calls
		internal virtual OTPAuthenticator startAuthentication(System.String authorizeId, System.String authenticateId, System.String password, System.String newAlgorithm, System.String newHash, System.String newSeed, System.String newSequence)
		{
			UserDatabase db = userDatabase.getUser(authenticateId);
			
			if (db == null)
			{
				throw new SASLException("User OTP data not found");
			}
			
			newSeed = newSeed.ToLower();
			OTPAuthenticator a = new OTPAuthenticator(this, db, password, authorizeId, authenticateId, newAlgorithm, newHash, newSeed, newSequence);
			return a;
		}
		
		protected internal override void  finishInitiatorAuthentication(SessionCredential cred, Session s)
		{
			base.finishInitiatorAuthentication(cred, s);
		}
		
		protected internal override void  finishListenerAuthentication(SessionCredential cred, Session s)
		{
			base.finishListenerAuthentication(cred, s);
			SupportClass.HashtableRemove(authenticators, cred.Authenticator);
		}
		
		protected internal virtual void  failListenerAuthentication(Session session, System.String authenticator)
		{
			SupportClass.HashtableRemove(authenticators, authenticator);
			base.failListenerAuthentication(session);
		}
		
		/// <summary> Method AuthenticateSASLOTP starts SASL OTP Authentication
		/// between two peers. This is the NON-Piggybacking version 
		/// (it doesn't send the initial identity information on the 
		/// startChannelRequest).
		/// 
		/// If you want to do that (I recommend it, then use the NEXT one).
		/// 
		/// </summary>
		/// <param name="session">Session is the session the user is authenticating on,
		/// in other words, represents the peer we want to
		/// authenticate to.
		/// </param>
		/// <param name="authorizeId">The identity this peer wants to be
		/// authorized to act as.
		/// </param>
		/// <param name="authenticateId">The identity this peer will
		/// authenticate as
		/// </param>
		/// <param name="pwd">The passphrase to authenticate with (it isn't stored or
		/// kept around very long at all, it's only used in computation).
		/// @throws SASLException if any issue is encountered (usually
		/// rejection by the other peer).
		/// </param>
		public static Session AuthenticateSASLOTP(Session session, System.String authorizeId, System.String authenticateId, System.String pwd)
		{
			bool success = false;
			
			if ((System.Object) authenticateId == null || session == null || (System.Object) pwd == null)
			{
				
				throw new InvalidParameterException(ERR_INVALID_ID + authenticateId);
			}
			
			// @todo bad toad - clean this up with tuning move
			clearCredential(session, null);
			
			OTPAuthenticator auth = SASLOTPProfile.instance().startAuthentication(authorizeId, authenticateId, pwd);
			Channel ch = null;
			System.String startData = null;
			try
			{
				ch = session.startChannel(SASLOTPProfile.URI, auth);
				startData = ch.StartData;
			}
			catch (BEEPException x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				auth.abort(x.Message);
			}
			
			// @todo EITHER use the Session Event Mechanism once it's
			// detached from the session I/O thread via the Channel
			// Message (EVENT) Queues...
			// OR
			// Embed some tuning profile logic in ChannelZero (hacky) to
			// address stuff.  For now, this is ok, the only thread
			// blocked is the users, and it waits until the Authentication
			// succeeds or fails.
			Blob blob = null;
			
			if ((System.Object) startData != null)
			{
				blob = new Blob(startData);
				if (blob.Status.Equals(SASL_STATUS_ABORT))
					throw new SASLException(ERR_REJECTED);
			}
			
			auth.started(ch);
			auth.sendIdentity(authorizeId, authenticateId);
			
			// We'll get notified when either
			// (a) An Error Occurs
			// (b) The authentication succeeds
			// Everything else from here on out is basically on autopilot
			try
			{
				lock (auth)
				{
					System.Threading.Monitor.Wait(auth);
				}
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				auth.abort(x.Message);
			}
			
			SessionCredential cred = session.getLocalCredential();
			
			if (cred == null)
			{
				auth.abort("Authentication Failed");
			}
			else
			{
				success = true;
			}
			
			return session;
		}
		
		/*
		* Method AuthenticateSASLOTP starts SASL OTP Authentication
		* between two peers.  This is the Piggybacking version (it does send the 
		* initial identity information on the startChannelRequest).
		* 
		* If you want to do that (I recommend it, then use the NEXT one).
		* 
		* @param Session session is the session the user is authenticating on,
		* in other words, represents the peer we want to authenticate to.
		* @param String authorizeId is the identity this peer wants to be
		*               authorized to act as.
		* @param String authenticateId is the identity this peer will
		*               authenticate as
		* @param String pwd is the passphrase to authenticate with (it isn't
		* stored or kept around very long at all, it's only used in computation).
		* @throws SASLException if any issue is encountered (usually
		* rejection by the other peer).
		*/
		public static Session AuthenticateSASLOTPPiggybacked(Session session, System.String authorizeId, System.String authenticateId, System.String pwd)
		{
			bool success = false;
			
			if ((System.Object) authenticateId == null)
			{
				throw new InvalidParameterException(ERR_INVALID_ID + authenticateId);
			}
			
			// @todo bad toad - clean this up with tuning move
			clearCredential(session, null);
			
			OTPAuthenticator auth = SASLOTPProfile.instance().startAuthentication(authorizeId, authenticateId, pwd);
			Channel ch = null;
			System.String startData = null;
			try
			{
				ch = session.startChannel(SASLOTPProfile.URI, auth);
				startData = ch.StartData;
			}
			catch (BEEPException x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				auth.abort(x.Message);
			}
			
			// @todo EITHER use the Session Event Mechanism once it's
			// detached from the session I/O thread via the Channel
			// Message (EVENT) Queues...
			// OR
			// Embed some tuning profile logic in ChannelZero (hacky) to
			// address stuff.  For now, this is ok, the only thread
			// blocked is the users, and it waits until the Authentication
			// succeeds or fails.
			Blob blob = null;
			
			if ((System.Object) startData != null)
			{
				blob = new Blob(startData);
				if (blob.Status.Equals(SASL_STATUS_ABORT))
					throw new SASLException(ERR_REJECTED);
			}
			
			auth.started(ch);
			auth.sendIdentity(authorizeId, authenticateId);
			
			// We'll get notified when either
			// (a) An Error Occurs
			// (b) The authentication succeeds
			// Everything else from here on out is basically on autopilot
			try
			{
				lock (auth)
				{
					System.Threading.Monitor.Wait(auth);
					
					SessionCredential cred = session.getLocalCredential();
					
					if (cred == null)
					{
						auth.abort("Authentication Failed");
					}
					else
					{
						success = true;
					}
				}
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				auth.abort(x.Message);
			}
			
			return session;
		}
		
		/*
		* Method AuthenticateSASLOTPWithInit is like the others, except
		* that is actually forces the server (if authentication succeeds)
		* to update the OTP db for this user.
		* 
		* If you want to do that (I recommend it, then use the NEXT one).
		* 
		* @param Session session is the session the user is authenticating on,
		* in other words, represents the peer we want to authenticate to.
		* @param String authorizeId is the identity this peer wants to be
		*               authorized to act as.
		* @param String authenticateId is the identity this peer will
		*               authenticate as
		* @param String pwd is the passphrase to authenticate with (it isn't
		* stored or kept around very long at all, it's only used in computation).
		*
		* @param String newSequence String representation of the new Sequence
		*               integer
		* @param String newAlgorithm name of the algorithm in the new OTP DB
		* @param String newSeed value of the seed in the new OTP DB
		* @param String newHas value of the lastHash in the new OTP DB
		*
		*
		* @throws SASLException if any issue is encountered (usually
		* rejection by the other peer).
		*/
		public static Session AuthenticateSASLOTPWithInit(Session session, System.String authorizeId, System.String authenticateId, System.String pwd, System.String newAlgorithm, System.String newHash, System.String newSeed, System.String newSequence)
		{
			bool success = false;
			
			// Test the values
			convertHexToBytes(newHash);
			newSeed = newSeed.ToLower();
			
			if (!OTPGenerator.validateSeed(newSeed) || !OTPGenerator.validatePassphrase(pwd) || !OTPGenerator.validateSequence(newSequence))
				throw new SASLException("Unsuitable values for the parameters to init-hex");
			
			// @todo bad toad - clean this up with tuning move
			clearCredential(session, null);
			
			OTPAuthenticator auth = SASLOTPProfile.instance().startAuthentication(authorizeId, authenticateId, pwd, newAlgorithm, newHash, newSeed, newSequence);
			Channel ch = null;
			System.String startData = null;
			try
			{
				ch = session.startChannel(SASLOTPProfile.URI, auth);
				startData = ch.StartData;
			}
			catch (BEEPException x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				auth.abort(x.Message);
			}
			
			// @todo EITHER use the Session Event Mechanism once it's detached from
			// the session I/O thread via the Channel Message (EVENT) Queues...
			// OR
			// Embed some tuning profile logic in ChannelZero (hacky) to address stuff.
			// For now, this is ok, the only thread blocked is the users, and it waits
			// until the Authentication succeeds or fails.
			Blob blob = null;
			
			if ((System.Object) startData != null)
			{
				blob = new Blob(startData);
				if (blob.Status.Equals(SASL_STATUS_ABORT))
					throw new SASLException(ERR_REJECTED);
			}
			
			auth.started(ch);
			auth.sendIdentity(authorizeId, authenticateId);
			
			// We'll get notified when either
			// (a) An Error Occurs
			// (b) The authentication succeeds
			// Everything else from here on out is basically on autopilot
			try
			{
				lock (auth)
				{
					System.Threading.Monitor.Wait(auth);
					
					SessionCredential cred = session.getLocalCredential();
					
					if (cred == null)
					{
						auth.abort("Authentication Failed");
					}
					else
					{
						success = true;
					}
				}
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				auth.abort(x.Message);
			}
			
			return session;
		}
		
		public static sbyte[] convertLongToBytes(long l)
		{
			sbyte[] hash = new sbyte[8];
			
			for (int i = 7; i >= 0; i--)
			{
				hash[i] = (sbyte) (l & 0xff);
				l >>= 8;
			}
			
			return hash;
		}
		
		protected internal virtual long convertBytesToLong(sbyte[] hash)
		{
			long l = 0L;
			
			for (int k = 0; k < 8; ++k)
			{
				l = (l << 8) | (hash[k] & 0xff);
			}
			
			return (l);
		}
		
		public static long convertHexToLong(System.String hash)
		{
			if (hash.Length != 16)
			{
				throw new SASLException("Illegal hash" + hash.Length);
			}
			long result = 0L;
			result = (long) System.Convert.ToInt64(hash, 16);
			return result;
		}
		
		public static System.String convertBytesToHex(sbyte[] hash)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(16);
			
			for (int i = 0; i < 8; i++)
			{
				int val = hash[i] & 0xFF;
				
				if (val < 16)
				{
					sb.Append('0');
				}
				
				sb.Append(System.Convert.ToString(val, 16));
			}
			
			return sb.ToString();
		}
		
		public static sbyte[] convertHexToBytes(System.String hash)
		{
			sbyte[] result = new sbyte[8];
			
			if (hash.Length != 16)
			{
				throw new SASLException("Illegal hash" + hash.Length);
			}
			
			for (int i = 0; i < 16; i += 2)
			{
				//UPGRADE_TODO: Method 'java.lang.Integer.parseInt' was converted to 'System.Convert.ToInt32' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				result[i / 2] = (sbyte) System.Convert.ToInt32(hash.Substring(i, (i + 2) - (i)), 16);
			}
			
			return result;
		}
	}
}