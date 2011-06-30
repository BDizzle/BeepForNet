/*
* InvalidParameterException.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
namespace org.beepcore.beep.profile.sasl
{
	
	
	/// <summary> This exception is thrown when a peer uses invalid parameters.
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
	public class InvalidParameterException:SASLException
	{
		
		// Constants
		public const System.String MSG = "Invalid Parameter";
		public const System.String SEPARATOR = "=>";
		
		public InvalidParameterException():base(MSG)
		{
		}
		
		public InvalidParameterException(System.String msg):base(MSG + SEPARATOR + msg)
		{
		}
	}
}