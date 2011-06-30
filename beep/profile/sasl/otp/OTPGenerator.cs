/*
* OTPGenerator.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
using BEEPException = org.beepcore.beep.core.BEEPException;
using ProfileConfiguration = org.beepcore.beep.profile.ProfileConfiguration;
using SASLException = org.beepcore.beep.profile.sasl.SASLException;
using Algorithm = org.beepcore.beep.profile.sasl.otp.algorithm.Algorithm;
using UserDatabasePool = org.beepcore.beep.profile.sasl.otp.database.UserDatabasePool;
namespace org.beepcore.beep.profile.sasl.otp
{
	
	
	/// <summary> This class serves as a utility that generates OTP information
	/// (in the form of a java.util.Properties that can be written to
	/// network or disk. 
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
	public class OTPGenerator
	{
		private static System.String UserInput
		{
			get
			{
				try
				{
					//UPGRADE_TODO: Expected value of parameters of constructor 'java.io.BufferedReader.BufferedReader' are different in the equivalent in .NET. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1092"'
					//UPGRADE_ISSUE: 'java.lang.System.in' was converted to 'System.Console.In' which is not valid in this expression. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1109"'
					System.IO.StreamReader br = new System.IO.StreamReader(new System.IO.StreamReader(System.Console.In).BaseStream, System.Text.Encoding.UTF7);
					
					return br.ReadLine();
				}
				catch (System.IO.IOException ioe)
				{
				}
				
				return null;
			}
			
		}
		// Constants
		// Prompts used during OTP database generation
		private const System.String PROMPT_ALGORITHM = "Please enter the hash algorithm to use";
		private const System.String PROMPT_ALGORITHM_SUGGESTIONS = " [ Please use 'otp-md5' or 'otp-sha1' ]";
		private const System.String PROMPT_PASSPHRASE = "Please enter the passphrase to be used (this will NOT be stored)";
		private const System.String PROMPT_RETURN = " and hit return when done\n=>";
		private const System.String PROMPT_SEED = "Please enter some string to serve as your password seed:";
		private const System.String PROMPT_SEQUENCE = "Please enter an integer corresponding to the OTP sequence you wish to use:";
		private const System.String PROMPT_USERNAME = "Please enter the username to be used in authentication";
		
		// Error cases
		private const System.String ERR_USER_DB_EXISTS = "User database exists for ";
		private const System.String ERR_ALGO_UNSUPPORTED = "Algorithm not supported=>";
		private const System.String ERR_SEED_ALPHANUM = "Seed must be composed of alpha-numeric characters only.";
		private const System.String ERR_SEED_SIZE = "Seed must 1 to 16 characters in length.";
		private const System.String ERR_PASSPHRASE_SIZE = "Passphrase must be between 10 and 63 characters in length";
		
		private static System.String promptForAlgorithm()
		{
			bool done = false;
			System.String algo = null;
			
			while (!done)
			{
				System.Console.Out.WriteLine(PROMPT_ALGORITHM);
				System.Console.Out.WriteLine(PROMPT_ALGORITHM_SUGGESTIONS);
				System.Console.Out.Write(PROMPT_RETURN);
				
				algo = UserInput;
				
				Algorithm a = SASLOTPProfile.getAlgorithm(algo);
				
				if (a == null)
				{
					System.Console.Out.WriteLine(ERR_ALGO_UNSUPPORTED + algo);
				}
				else
				{
					done = true;
				}
			}
			
			return algo;
		}
		
		private static System.String promptForPassphrase()
		{
			bool done = false;
			System.String pass = null;
			
			while (!done)
			{
				System.Console.Out.WriteLine(PROMPT_PASSPHRASE);
				System.Console.Out.Write(PROMPT_RETURN);
				
				pass = UserInput;
				try
				{
					done = validatePassphrase(pass);
				}
				catch (System.Exception x)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					System.Console.Out.WriteLine(x.Message);
				}
			}
			return pass;
		}
		
		private static System.String promptForSeed()
		{
			bool done = false;
			System.String seed = null;
			
			while (!done)
			{
				System.Console.Out.WriteLine(PROMPT_SEED);
				System.Console.Out.Write(PROMPT_RETURN);
				
				seed = UserInput;
				try
				{
					done = validateSeed(seed);
				}
				catch (System.Exception x)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					System.Console.Out.WriteLine(x.Message);
				}
			}
			
			return seed.ToLower();
		}
		
		private static System.String promptForSequence()
		{
			bool done = false;
			System.String seq = null;
			
			while (!done)
			{
				System.Console.Out.WriteLine(PROMPT_SEQUENCE);
				System.Console.Out.Write(PROMPT_RETURN);
				
				seq = UserInput;
				done = validateSequence(seq);
			}
			
			return seq;
		}
		
		private static System.String promptForUsername()
		{
			System.Console.Out.WriteLine(PROMPT_USERNAME);
			System.Console.Out.Write(PROMPT_RETURN);
			
			return UserInput;
		}
		
		private static bool validateUserName(System.String user)
		{
			bool result = false;
			
			if ((System.Object) user == null)
			{
				return false;
			}
			
			try
			{
				//UPGRADE_TODO: Format of property file may need to be changed. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1089"'
				System.Collections.Specialized.NameValueCollection test = new System.Collections.Specialized.NameValueCollection();
				
				new System.IO.FileStream(user + UserDatabasePool.OTP_SUFFIX, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				test = new System.Collections.Specialized.NameValueCollection(System.Configuration.ConfigurationSettings.AppSettings);
			}
			catch (System.Exception x)
			{
				return true;
			}
			
			return false;
		}
		
		internal static bool validatePassphrase(System.String pp)
		{
			int length = pp.Length;
			if (length < 10 || length > 63)
			{
				System.Console.Out.WriteLine("The length of " + length + " is invalid.");
				throw new SASLException(ERR_PASSPHRASE_SIZE);
			}
			return true;
		}
		
		internal static bool validateSeed(System.String seed)
		{
			int length = seed.Length;
			if (length <= 0 || length > 10)
			{
				System.Console.Out.WriteLine("The length of " + length + " is invalid.");
				throw new SASLException(ERR_SEED_SIZE);
			}
			for (int i = 0; i < length; i++)
			{
				char c = seed[i];
				
				// Unfortunately, they allow '$' and '_'
				if (!System.Char.IsDigit(c) && !System.Char.IsLetter(c))
				{
					System.Console.Out.WriteLine("The character=>" + seed[i] + "<= is invalid.");
					throw new SASLException(ERR_SEED_ALPHANUM);
				}
			}
			return true;
		}
		
		internal static bool validateSequence(System.String seq)
		{
			int i = 0;
			
			try
			{
				i = System.Int32.Parse(seq);
				
				if (i < 0)
				{
					return false;
				}
				
				return true;
			}
			catch (System.Exception x)
			{
				return false;
			}
		}
		
		internal static void  printHex(sbyte[] buff)
		{
			System.Console.Out.WriteLine(SASLOTPProfile.convertBytesToHex(buff));
		}
		
		/// <summary> Method main is the method used to run the OTP generator.
		/// IT prompts the users for information necessary to create
		/// the OTP database and validates as it goes.  Once it has
		/// valid input, it produces the file.
		/// </summary>
		[STAThread]
		public static void  Main(System.String[] argv)
		{
			SASLOTPProfile sop = new SASLOTPProfile();
			
			try
			{
				sop.init(SASLOTPProfile.URI, new ProfileConfiguration());
			}
			catch (BEEPException x)
			{
			}
			
			//UPGRADE_TODO: Format of property file may need to be changed. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1089"'
			System.Collections.Specialized.NameValueCollection p = new System.Collections.Specialized.NameValueCollection();
			
			// Get username and check to make sure this won't clobber
			// existing OTP database information.
			System.String username = null;
			
			while (!validateUserName((username = promptForUsername())))
				;
			
			// Get the passphrase, to only be used transiently
			System.String passphrase = null;
			
			passphrase = promptForPassphrase();
			
			// Get the seed, algorith, and sequence
			System.String seed = promptForSeed();
			System.String algo = promptForAlgorithm();
			System.String sequence = promptForSequence();
			
			// Generate the hash
			Algorithm a = SASLOTPProfile.getAlgorithm(algo);
			int limit = System.Int32.Parse(sequence);
			sbyte[] hash = a.generateHash(seed + passphrase), temp;
			
			for (int i = 0; i < limit; i++)
			{
				temp = a.generateHash(hash);
				hash = temp;
			}
			
			printHex(hash);
			
			passphrase = SASLOTPProfile.convertBytesToHex(hash);
			
			// Store the OTP db information
			try
			{
				SupportClass.PutElement(p, UserDatabasePool.OTP_AUTHENTICATOR, username);
				SupportClass.PutElement(p, UserDatabasePool.OTP_ALGO, algo);
				SupportClass.PutElement(p, UserDatabasePool.OTP_LAST_HASH, passphrase);
				SupportClass.PutElement(p, UserDatabasePool.OTP_SEQUENCE, sequence);
				SupportClass.PutElement(p, UserDatabasePool.OTP_SEED, seed);
				//UPGRADE_ISSUE: Method 'java.util.Properties.store' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000"'
				p.store(new System.IO.FileStream(username + UserDatabasePool.OTP_SUFFIX, System.IO.FileMode.Create), UserDatabasePool.OTP_HEADER);
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				throw new SASLException(x.Message);
			}
		}
	}
}