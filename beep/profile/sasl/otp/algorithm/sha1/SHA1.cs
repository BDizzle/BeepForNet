/*
* SHA1.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
using AlgorithmImpl = org.beepcore.beep.profile.sasl.otp.algorithm.AlgorithmImpl;
namespace org.beepcore.beep.profile.sasl.otp.algorithm.sha1
{
	
	
	/// <summary> This class does the 'funky OTP stuff' to the SHA1 hash
	/// (basically folds it to 64 bits and makes it little endian).
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
	public class SHA1:AlgorithmImpl
	{
		override public System.String Name
		{
			get
			{
				return AlgorithmName;
			}
			
		}
		public static System.String AlgorithmName
		{
			get
			{
				return SHA1_NAME;
			}
			
		}
		
		private const System.String SHA1_INTERNAL = "SHA";
		private const System.String SHA1_NAME = "otp-sha1";
		
		public SHA1():base(SHA1_INTERNAL)
		{
		}
		
		public override sbyte[] generateHash(sbyte[] hash)
		{
			if (hash == null)
			{
				throw new InvalidParameterException();
			}
			
			hash = base.generateHash(hash);
			return foldHash(hash);
		}
		
		protected internal override sbyte[] foldHash(sbyte[] hash)
		{
			if (hash == null)
			{
				throw new InvalidParameterException();
			}
			
			for (int i = 0; i < 4; i++)
			{
				hash[i] ^= (0xff & hash[i + 8]);
			}
			
			for (int i = 4; i < 8; i++)
			{
				hash[i] ^= (0xff & hash[i + 8]);
			}
			
			for (int i = 0; i < 4; i++)
			{
				hash[i] ^= (0xff & hash[i + 16]);
			}
			
			sbyte[] newHash = new sbyte[8];
			
			for (int i = 0; i < 4; i++)
			{
				newHash[3 - i] = hash[i];
				newHash[7 - i] = hash[i + 4];
			}
			
			return (newHash);
		}
	}
}