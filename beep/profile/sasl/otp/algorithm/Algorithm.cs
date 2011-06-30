/*
* Algorithm.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
using InvalidParameterException = org.beepcore.beep.profile.sasl.InvalidParameterException;
namespace org.beepcore.beep.profile.sasl.otp.algorithm
{
	
	/// <summary> </summary>
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
	public struct Algorithm_Fields{
		// Constants
		public readonly static System.String ERR_HASH_FAILURE;
		static Algorithm_Fields()
		{
			ERR_HASH_FAILURE = new System.Text.StringBuilder("Error during hash calculation").ToString();
		}
	}
	public interface Algorithm
		{
			//UPGRADE_NOTE: Members of interface 'Algorithm' were extracted into structure 'Algorithm_Fields'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1045"'
			System.String Name
			{
				// Methods
				
				get;
				
			}
			
			
			
			sbyte[] generateHash(System.String data);
			
			sbyte[] generateHash(sbyte[] data);
		}
}