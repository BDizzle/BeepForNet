/*
* UserDatabase.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
	
	
	/// <summary> This interface represents all the information associated
	/// with a given user for SASL OTP, including password hashes, seeds,
	/// and the sequence number.  It is designed to be implemented by 
	/// classes that provide real-world storage for this information.
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
	public interface UserDatabase
		{
			/// <summary> Method getAlgorithmName returns the algorithm associated with this
			/// particular user and OTP database.  
			/// 
			/// </summary>
			/// <returns> String the algorithm employed by the user of this database
			/// for SASL OTP authentication.
			/// 
			/// </returns>
			System.String AlgorithmName
			{
				get;
				
			}
			/// <summary> Method getLastHash
			/// 
			/// </summary>
			/// <returns> byte[] the lastHash value generated the last time the
			/// user of this database performed SASL OTP authentication.  The
			/// hash value is represented in binary form.
			/// 
			/// </returns>
			sbyte[] LastHash
			{
				get;
				
			}
			/// <summary> Method getLastHashAsString
			/// 
			/// </summary>
			/// <returns> String the lastHash value generated the last time the
			/// user of this database performed SASL OTP authentication.  The
			/// hash is represented in hexadecimal form.
			/// 
			/// </returns>
			System.String LastHashAsString
			{
				get;
				
			}
			/// <summary> Method getSeed
			/// 
			/// </summary>
			/// <returns> String the seed used by the 
			/// user of this database for SASL OTP authentication.
			/// 
			/// </returns>
			System.String Seed
			{
				get;
				
			}
			/// <summary> Method getSequence
			/// 
			/// </summary>
			/// <returns> int the sequence to be used by the 
			/// user of this database for SASL OTP authentication.
			/// 
			/// </returns>
			int Sequence
			{
				get;
				
			}
			/// <summary> Method getAuthenticator
			/// 
			/// </summary>
			/// <returns> String the user of this database.
			/// 
			/// </returns>
			System.String Authenticator
			{
				get;
				
			}
			
			/// <summary> Method updateLastHash
			/// 
			/// </summary>
			/// <param name="hash">String is the new hash value to be stored
			/// in the user database.
			/// 
			/// </param>
			void  updateLastHash(System.String hash);
		}
}