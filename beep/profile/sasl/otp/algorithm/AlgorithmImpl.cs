/*
* AlgorithmImpl.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:59 $
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
	public abstract class AlgorithmImpl : Algorithm
	{
		/// <summary> Method getName</summary>
		public abstract System.String Name{get;}
		
		// Data
		// We use two types of algorithm names - one for challenges
		// and one for the java.security.MessageDigest
		private System.String internalAlgorithmName;
		
		/// <summary> Method AlgorithmImpl
		/// 
		/// </summary>
		/// <param name="internal">The data used by the JVM internally to represent
		/// a certain MessageDigest hash algorithm.   This is
		/// defined in JVM documentation and in constants in
		/// SASLOTPProfile.
		/// 
		/// </param>
		public AlgorithmImpl(System.String internal_Renamed)
		{
			internalAlgorithmName = internal_Renamed;
		}
		
		/// <summary> Method generateHash generate a hash value using the appropriate
		/// hash function.
		/// 
		/// </summary>
		/// <param name="s">The data to be hashed
		/// </param>
		/// <returns> byte[] the hash value in binary form.
		/// 
		/// @throws SASLException if an error is encountered during the 
		/// generation of hte hash.
		/// 
		/// </returns>
		public virtual sbyte[] generateHash(System.String s)
		{
			return generateHash(SupportClass.ToSByteArray(SupportClass.ToByteArray(s.ToLower()))); ///@TODO use encoding
		}
		
		/// <summary> Method generateHash generate a hash value using the appropriate
		/// hash function.
		/// 
		/// </summary>
		/// <param name="data">The data to be hashed
		/// </param>
		/// <returns> byte[] the hash value in binary form.
		/// 
		/// @throws SASLException if an error is encountered during the 
		/// generation of hte hash.
		/// 
		/// </returns>
		public virtual sbyte[] generateHash(sbyte[] data)
		{
			SupportClass.MessageDigestSupport digest = null;
			
			try
			{
				digest = SupportClass.MessageDigestSupport.GetInstance(internalAlgorithmName);
			}
			catch (System.Exception x)
			{
				throw new System.SystemException(internalAlgorithmName + " hash algorithm not found");
			}
			return SupportClass.ToSByteArray(digest.DigestData(SupportClass.ToByteArray(data)));
		}
		
		/// <summary> Method foldHash is provided for implementations, as the value
		/// of the message digest hash must be folding into 64 bits before 
		/// it can be used by the SASLOTPProfile and its supporting classes.
		/// 
		/// </summary>
		/// <param name="hash">The hash value to be folded
		/// </param>
		/// <returns> byte[] is the folded hash.
		/// 
		/// @throws InvalidParameterException of the has provided is
		/// somehow improper or invalid.
		/// 
		/// </returns>
		protected internal abstract sbyte[] foldHash(sbyte[] hash);
	}
}