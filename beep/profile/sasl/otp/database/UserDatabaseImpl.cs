/*
* UserDatabaseImpl.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
using SASLOTPProfile = org.beepcore.beep.profile.sasl.otp.SASLOTPProfile;
namespace org.beepcore.beep.profile.sasl.otp.database
{
	
	
	/// <summary> This class is an implementation of UserDatabase interface.
	/// It uses the java.util.Property class, which uses the local
	/// filesystem, to store SASL OTP user databases.
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
	/// <version>  $Revision: 1.1 $, $Date: 2004/09/21 08:46:59 $
	/// 
	/// </version>
	public class UserDatabaseImpl : UserDatabase
	{
		/// <summary> Method getAlgorithmName
		/// 
		/// </summary>
		/// <returns> String the algorithm employed by the user of this database
		/// for SASL OTP authentication.
		/// 
		/// </returns>
		virtual public System.String AlgorithmName
		{
			get
			{
				return algorithm;
			}
			
		}
		/// <summary> Method getLastHash
		/// 
		/// </summary>
		/// <returns> byte[] the lastHash value generated the last time the
		/// user of this database performed SASL OTP authentication.  The
		/// hash value is represented in binary form.
		/// 
		/// </returns>
		virtual public sbyte[] LastHash
		{
			get
			{
				return SASLOTPProfile.convertHexToBytes(lastHash);
			}
			
		}
		/// <summary> Method getLastHash
		/// 
		/// </summary>
		/// <returns> String the lastHash value generated the last time the
		/// user of this database performed SASL OTP authentication.  The
		/// hash is represented in hexadecimal form.
		/// 
		/// </returns>
		virtual public System.String LastHashAsString
		{
			get
			{
				return lastHash;
			}
			
		}
		/// <summary> Method getLastHash
		/// 
		/// </summary>
		/// <returns> String the seed used by the 
		/// user of this database for SASL OTP authentication.
		/// 
		/// </returns>
		virtual public System.String Seed
		{
			get
			{
				return seed;
			}
			
		}
		/// <summary> Method getLastHash
		/// 
		/// </summary>
		/// <returns> int the sequence that should be used by the 
		/// user of this database for their next SASL OTP authentication.
		/// 
		/// </returns>
		virtual public int Sequence
		{
			get
			{
				return sequence;
			}
			
		}
		/// <summary> Method getLastHash
		/// 
		/// </summary>
		/// <returns> String the last hash value used by the 
		/// user of this database for SASL OTP authentication.
		/// 
		/// </returns>
		virtual public System.String Authenticator
		{
			get
			{
				return authenticator;
			}
			
		}
		// Data
		private System.String authenticator;
		private System.String algorithm;
		private System.String lastHash;
		private System.String seed;
		private int sequence;
		
		/// <summary> This constructor is the only one available, 
		/// and it requires everything.
		/// 
		/// </summary>
		/// <param name="String">algorithm indicates the Algorithm to be used by
		/// SASL OTP.  This library supports otp-md5 and otp-sha1.
		/// </param>
		/// <param name="String">authenticator the identity of the user wishing 
		/// to authenticate via SASL OTP.
		/// </param>
		/// <param name="String">lastHash the hexidecimal representation of the 
		/// sequence + 1 hash of the seed, passphrase combination.
		/// </param>
		/// <param name="String">seed the string that's prepended to the passphrase
		/// prior to the initial hash calculation.
		/// </param>
		/// <param name="int">sequence the current OTP sequence, which represents 
		/// the number of times the hash function is called with its own 
		/// product.  This value is decremented each time authentication
		/// occurs.
		/// 
		/// </param>
		internal UserDatabaseImpl(System.String algorithm, System.String lastHash, System.String seed, System.String authenticator, int sequence)
		{
			this.algorithm = algorithm;
			this.authenticator = authenticator;
			this.lastHash = lastHash;
			this.seed = seed;
			this.sequence = sequence;
		}
		
		/// <summary> Method updateLastHash
		/// 
		/// </summary>
		/// <param name="hash">String is the new hash value to be stored
		/// in the user database, for use in comparison the next time
		/// they try to authenticate.
		/// 
		/// @throws SASLException in the event that the update causes
		/// an exception to be thrown during the OTP database update.
		/// </param>
		public virtual void  updateLastHash(System.String hash)
		{
			sequence--;
			
			lastHash = hash;
		}
	}
}