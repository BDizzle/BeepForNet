/*
* UserDatabasePool.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
using SASLException = org.beepcore.beep.profile.sasl.SASLException;
namespace org.beepcore.beep.profile.sasl.otp.database
{
	
	
	/// <summary> This class provides several routines through which one
	/// can retrieve implementations of SASL OTP User Databases.
	/// Implementors of any other UserDatabase implementations 
	/// will want to expose them through this, or provide similar
	/// functionality elsewhere.
	/// 
	/// </summary>
	public struct UserDatabaseManager_Fields{
		// Constants
		public readonly static System.String OTP_ALGO = "OTP Algorithm";
		public readonly static System.String OTP_AUTHENTICATOR = "OTP Authenticator";
		public readonly static System.String OTP_HEADER = "OTP Properties Header";
		public readonly static System.String OTP_LAST_HASH = "OTP Last Hash";
		public readonly static System.String OTP_MECH = "OTP Mechanism";
		public readonly static System.String OTP_SEED = "OTP Seed";
		public readonly static System.String OTP_SEQUENCE = "OTP Sequence";
		public readonly static System.String OTP_SUFFIX = ".otp";
		public readonly static System.String ERR_DB_PARSE = "Unable to parse OTP DB";
	}
	public interface UserDatabaseManager
		{
			//UPGRADE_NOTE: Members of interface 'UserDatabaseManager' were extracted into structure 'UserDatabaseManager_Fields'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1045"'
			
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
			UserDatabase getUser(System.String username);
			
			/// <summary> Method addUser
			/// 
			/// </summary>
			/// <param name="username">The identity of the user for whom this OTP
			/// database is used.
			/// 
			/// </param>
			void  addUser(System.String username, System.String algorithm, System.String hash, System.String seed, System.String sequence);
			
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
			void  updateUserDB(UserDatabase ud);
			
			/// <summary> Method purgeUserDatabase is a routine designed to allow
			/// for the removal of a user db.
			/// 
			/// </summary>
			/// <param name="username">The username associated with a given db.
			/// 
			/// @throws SASLException if any errors are encountered in the
			/// removal of the data (such as it not being there in the first place
			/// or encountering some rights issue, it can't be removed right now
			/// cuz it's being used etc.)
			/// </param>
			void  removeUserDB(System.String username);
		}
}