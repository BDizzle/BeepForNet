/*
* SequenceZeroFailure.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
using AuthenticationFailureException = org.beepcore.beep.profile.sasl.AuthenticationFailureException;
namespace org.beepcore.beep.profile.sasl.otp
{
	
	/// <summary> This exception is used when a user attempts to authenticate via
	/// SASL OTP and it is determined that the user has no more valid
	/// hashes left in their OTP database.  It's distinct from the error
	/// used when an OTP database isn't found in that it allows the receiver
	/// to programmatically prompt the user to do an init-hex or init-word
	/// command to re-establish an OTP database.
	/// 
	/// @todo make sure the OTP db gets wiped clean off the disk after this
	/// gets thrown once - or something like that.  Think about that anyway.
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
	public class SequenceZeroFailure:AuthenticationFailureException
	{
		
		// Constants
		private const System.String MSG = "Authentication unable to proceed because the user's SASL OTP Sequence is 0.";
		
		public SequenceZeroFailure():base(MSG)
		{
		}
	}
}