/*
* UserDatabasePool.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
using SASLException = org.beepcore.beep.profile.sasl.SASLException;
namespace org.beepcore.beep.profile.sasl.otp.database
{
	
	
	/// <summary> This class implements UserDatabaseManager with a simple
	/// implementation.  More sophisticated implementations will
	/// use the ProfileConfiguration to tell SASLOTPProfile to
	/// load different classes to do this.  WARNING:  I have some
	/// serious concerns about the public nature of these methods.
	/// This won't stay this way, but the changes aren't cooked
	/// enough to shove into the first beta.
	/// 
	/// @todo do something TuningProfile-ish to minimize the public
	/// availability of these methods so that fake OTP DBs can't
	/// be pushed in so damn easily.  There has to be a 'validation'
	/// step and preferably, it'd have to happen through SASLOTPProfile
	/// extending some class here or something.  Granted, if some
	/// freak can execute code on your box then you're dead anyway,
	/// but this makes it tougher...somewhat.
	/// 
	/// </summary>
	public class UserDatabasePool : UserDatabaseManager
	{
		public UserDatabasePool()
		{
			InitBlock();
		}
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
			userpool = new System.Collections.Hashtable(4);
		}
		// Data 
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		//UPGRADE_NOTE: The initialization of  'userpool' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private System.Collections.Hashtable userpool;
		
		/// <summary> Method getUser This method is provided as a means
		/// for users of the OTP databases to retrieve the information
		/// contained in them, in the form of an instance of
		/// UserDatabase.  Please note that ALGORITHM should in time be
		/// added - to be part of how one looks up an OTP database (using
		/// both the username and the algorithm).  The init-word and init-hex
		/// commands, in their nature, don't really allow for it, so this'll
		/// do for now, but in time it should be that way.  It certainly
		/// wouldn't be a difficult thing to do.  This would also entail
		/// evolving the way init-hex/word are processed, as well...which
		/// is slightly trickier than doing a dual parameter lookup.
		/// 
		/// </summary>
		/// <param name="username">Indicates which OTP database should 
		/// be retrieved, based on who wishes to authenticate using it.
		/// 
		/// </param>
		/// <returns> UserDatabase the OTP database for the user specified.
		/// 
		/// @throws SASLException is thrown if the parameter is null or 
		/// some error is encountered during the reading or processing
		/// of the user's OTP database file.
		/// 
		/// </returns>
		public virtual UserDatabase getUser(System.String username)
		{
			if ((System.Object) username == null)
			{
				throw new SASLException("Must supply a username to get an OTP DB");
			}
			
			if (userpool == null)
			{
				userpool = new System.Collections.Hashtable();
			}
			
			UserDatabase ud = (UserDatabase) userpool[username];
			
			if (ud == null)
			{
				//UPGRADE_TODO: Format of property file may need to be changed. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1089"'
				System.Collections.Specialized.NameValueCollection p = new System.Collections.Specialized.NameValueCollection();
				
				try
				{
					if (log.isDebugEnabled())
					{
						log.debug("Loading otp property file " + username + org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SUFFIX);
					}
					new System.IO.FileStream(username + org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SUFFIX, System.IO.FileMode.Open, System.IO.FileAccess.Read);
					p = new System.Collections.Specialized.NameValueCollection(System.Configuration.ConfigurationSettings.AppSettings);
				}
				catch (System.IO.IOException x)
				{
					log.error(UserDBNotFoundException.MSG + username);
					
					throw new UserDBNotFoundException(username);
				}
				
				try
				{
					ud = new UserDatabaseImpl(p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_ALGO], p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_LAST_HASH], p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEED], p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_AUTHENTICATOR], System.Int32.Parse(p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEQUENCE]));
				}
				catch (System.Exception x)
				{
					throw new SASLException("OTP DB for " + username + "is corrupted");
				}
				SupportClass.PutElement(userpool, username, ud);
				if (log.isDebugEnabled())
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.util.Hashtable.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					log.debug(p.ToString());
					log.debug("Stored otp settings for " + username);
				}
			}
			if (log.isDebugEnabled())
			{
				log.debug("Fetching User Database for " + username);
			}
			return ud;
		}
		
		/// <summary> Method addUser
		/// 
		/// </summary>
		/// <param name="String">username is the identity of the user for
		/// whom this OTP database is used.
		/// </param>
		/// <param name="UserDatabase">ud is the user database to be associated
		/// with the user identified by 'username'.
		/// 
		/// </param>
		private void  addUser(System.String username, UserDatabase ud)
		{
			SupportClass.PutElement(userpool, username, ud);
			updateUserDB(ud);
		}
		
		public virtual void  addUser(System.String username, System.String algorithm, System.String hash, System.String seed, System.String sequence)
		{
			UserDatabaseImpl udi = new UserDatabaseImpl(algorithm, hash, seed, username, System.Int32.Parse(sequence));
			addUser(username, udi);
		}
		
		/// <summary> Method updateUserDB causes the long-term representation
		/// (e.g. file) of the user's OTP database to be updated
		/// after a successful authentication.  This entails a
		/// decrementation of the sequence, and a storage of a new
		/// 'last hash' value.
		/// 
		/// 
		/// </summary>
		/// <param name="ud">The updated form of the OTP database.
		/// 
		/// @throws SASLException if any issues are encountered during the
		/// storage of the user's OTP DB.
		/// 
		/// </param>
		public virtual void  updateUserDB(UserDatabase ud)
		{
			try
			{
				//UPGRADE_TODO: Format of property file may need to be changed. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1089"'
				System.Collections.Specialized.NameValueCollection p = new System.Collections.Specialized.NameValueCollection();
				System.String authenticator = ud.Authenticator;
				
				if (log.isDebugEnabled())
				{
					log.debug("Updating User DB on for=>" + authenticator);
				}
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_AUTHENTICATOR] = authenticator;
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_LAST_HASH] = ud.LastHashAsString;
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEED] = ud.Seed;
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_MECH] = "otp";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_ALGO] = ud.AlgorithmName;
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEQUENCE] = System.Convert.ToString(ud.Sequence);
				//UPGRADE_ISSUE: Method 'java.util.Properties.store' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000"'
				p.store(new System.IO.FileStream(authenticator + org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SUFFIX, System.IO.FileMode.Create), org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_HEADER);
			}
			catch (System.IO.IOException x)
			{
				log.error(x);
				
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				throw new SASLException(x.Message);
			}
		}
		
		/// <summary> Method removeUserDB causes the long-term representation
		/// (e.g. file) of the user's OTP database to be removed.
		/// 
		/// </summary>
		/// <param name="authenticator">The user of the OTP database.
		/// 
		/// @throws SASLException if any issues are encountered during the
		/// removal of the user's OTP DB.
		/// 
		/// </param>
		public virtual void  removeUserDB(System.String authenticator)
		{
			// @todo implement pending thinking about the issue.        
		}
		
		/// <summary> Method populateUserDatabases is a simple stub routine used
		/// to test the library.  It simply creates two temporary OTP
		/// databases.     
		/// 
		/// @throws SASLException if it encounters any issues storing
		/// the OTP databases.
		/// 
		/// </summary>
		public virtual void  populateUserDatabases()
		{
			try
			{
				//UPGRADE_TODO: Format of property file may need to be changed. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1089"'
				System.Collections.Specialized.NameValueCollection p = new System.Collections.Specialized.NameValueCollection();
				
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_AUTHENTICATOR] = "IW_User";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_LAST_HASH] = "7CD34C1040ADD14B";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEED] = "alpha1";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_MECH] = "otp";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_ALGO] = "otp-md5";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEQUENCE] = "1";
				//UPGRADE_ISSUE: Method 'java.util.Properties.store' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000"'
				p.store(new System.IO.FileStream("IW_User.otp", System.IO.FileMode.Create), org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_HEADER);
				
				//UPGRADE_TODO: Format of property file may need to be changed. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1089"'
				p = new System.Collections.Specialized.NameValueCollection();
				
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_AUTHENTICATOR] = "IW_User2";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_LAST_HASH] = "82AEB52D943774E4";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEED] = "correct";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_MECH] = "otp";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_ALGO] = "otp-sha1";
				//UPGRADE_TODO: Method 'java.util.Properties.setProperty' was converted to 'System.Collections.Specialized.NameValueCollection.Item' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				p[org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_SEQUENCE] = "1";
				//UPGRADE_ISSUE: Method 'java.util.Properties.store' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000"'
				p.store(new System.IO.FileStream("IW_User2.otp", System.IO.FileMode.Create), org.beepcore.beep.profile.sasl.otp.database.UserDatabaseManager_Fields.OTP_HEADER);
			}
			catch (System.IO.IOException x)
			{
				log.error(x);
			}
		}
	}
}