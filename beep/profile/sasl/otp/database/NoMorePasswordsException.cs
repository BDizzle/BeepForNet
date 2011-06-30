/*
* NoMoreTokensException.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
	
	/// <summary> This Exception represents the error case in which a
	/// peer attempting to authenticate via SASL OTP finds
	/// that no more one-time passwords are available.
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
	public class NoMorePasswordsException:SASLException
	{
		
		// Constants    
		public const System.String MSG = "No remaining passwords available for ";
		
		/// <summary> Default Constructor
		/// 
		/// </summary>
		/// <param name="username">Denotes the identity of the peer
		/// wishing to authenticate via SASL OTP
		/// 
		/// </param>
		public NoMorePasswordsException(System.String username):base(MSG + username)
		{
		}
	}
}