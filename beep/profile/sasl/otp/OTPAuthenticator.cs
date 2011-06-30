/*
* OTPAuthenticator.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2003 Huston Franklin.  All rights reserved.
*
* The contents of this file are subject to the Blocks License (the
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
using Log = org.apache.commons.logging.Log;
using LogFactory = org.apache.commons.logging.LogFactory;
using org.beepcore.beep.core;
using org.beepcore.beep.profile.sasl;
using org.beepcore.beep.profile.sasl.otp.algorithm;
using UserDatabase = org.beepcore.beep.profile.sasl.otp.database.UserDatabase;
namespace org.beepcore.beep.profile.sasl.otp
{
	
	/// <summary> This class encapsulates the state associated with
	/// an ongoing OTP Authentication, and provides methods
	/// to handle the exchange.
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
	public class OTPAuthenticator : RequestHandler, ReplyListener
	{
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
		}
		
		// Constants
		// Authentication States
		// Can be confusing..here are the sequences for state changes
		// It's not linear, the states only get changed here.
		// This prevents me from having to check if it's an 'initiator'
		// or 'listener' - the states imply that ;)
		// Initiator STARTED=>CHALLENGE=>COMPLETE
		// Listener STARTED=>ID=>RESPONSE=>COMPLETE
		internal const int STATE_UNKNOWN = 0;
		internal const int STATE_STARTED = 1;
		internal const int STATE_ID = 2;
		internal const int STATE_CHALLENGE = 3;
		internal const int STATE_RESPONSE = 4;
		internal const int STATE_COMPLETE = 5;
		internal const int STATE_ABORT = 6;
		
		// Err Messages
		internal const System.String ERR_PEER_ABORTED = "Our BEEP Peer has aborted this authentication sequence";
		internal const System.String ERR_IDENTITY_PARSE_FAILURE = "Invalid identity information submitted for OTP Authentication";
		internal const System.String ERR_NULL_ID = "Invalid Authentication Info Provided";
		internal const System.String ERR_OTP_AUTH_FAILURE = "Authentication Failure: Password hash doesn't match";
		internal const System.String ERR_OTP_STATE = "Authentication Failure: Illegal OTP State Transition";
		internal const System.String ERR_UNEXPECTED_MESSAGE = "Unexpected SASL-OTP Message";
		internal const System.String ERR_INIT = "Error while parsing init-hex or init-word=>";
		internal const System.String ERR_UNKNOWN_COMMAND = "Unknown SASL OTP Command=>";
		internal const System.String ERR_CONCURRENT = "Authentication for that user already in progress";
		private const System.String ERR_SEQUENCE_ZERO = "Authentication unable to proceed because the user's SASL OTP Sequence is 0.";
		private static System.String COLON = ":";
		
		// Other
		internal const System.String EXT = "ext";
		internal const System.String HEX = "hex:";
		internal const System.String INIT_HEX = "init-word:";
		internal const System.String INIT_WORD = "init-hex:";
		internal const System.String OTP_AUTH = "OTPAuthenticator";
		internal const System.String SPACE = " ";
		internal const System.String WORD = "word:";
		internal const char SPACE_CHAR = ' ';
		
		// Data
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		private int state;
		private Algorithm algorithm;
		private Channel channel;
		private System.Collections.Hashtable credential;
		private SASLOTPProfile profile;
		private System.String authenticated, authorized, initData = null, password;
		private UserDatabase database;
		
		/// <summary> Listener API
		/// 
		/// All of the routines below, but prior to the Initiator API,
		/// are the Listener calls
		/// </summary>
		internal OTPAuthenticator(SASLOTPProfile otpProfile)
		{
			InitBlock();
			log.debug("Creating Listener OTP Authenticator");
			
			authenticated = null;
			authorized = null;
			credential = new System.Collections.Hashtable();
			database = null;
			password = null;
			profile = otpProfile;
			state = STATE_UNKNOWN;
			
			SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR_TYPE, SASLOTPProfile.MECHANISM);
		}
		
		/// <summary> API for both Listener and Initiator</summary>
		internal virtual void  started(Channel ch)
		{
			log.debug("Starting OTP Authenticator");
			
			if (state != STATE_UNKNOWN)
			{
				throw new SASLException(ERR_OTP_STATE);
			}
			state = STATE_STARTED;
			channel = ch;
			channel.setRequestHandler(this);
		}
		
		/// <summary> Listener API
		/// 
		/// Receive IDs, respond with a Challenge or Exception
		/// </summary>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'receiveIDs'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual Blob receiveIDs(System.String data)
		{
			lock (this)
			{
				log.debug("OTP Authenticator Receiving IDs");
				
				// If we're listening, the last state we should
				// have gotten to was STATE_STARTED (after the channel start)
				if (state != STATE_STARTED)
				{
					abort(ERR_OTP_STATE);
				}
				
				if (log.isDebugEnabled())
				{
					log.debug("Data is" + data);
				}
				
				int i = data[0]; // data.indexOf(SPACE_CHAR);
				
				if (i == - 1)
				{
					abort(ERR_IDENTITY_PARSE_FAILURE);
				}
				else if (i == 0)
				{
					authorized = null;
				}
				else
				{
					int index = 0;
					try
					{
						index = data.IndexOf((System.Char) 0);
						authorized = data.Substring(0, (index) - (0));
					}
					catch (System.Exception x)
					//            catch(IndexOutOfBoundsException x)
					{
						authorized = null;
					}
				}
				
				authenticated = data.Substring(data.IndexOf((System.Char) 0) + 1);
				if (!profile.validateIdentity(authenticated, this))
				{
					abort(ERR_CONCURRENT);
				}
				
				if ((System.Object) authenticated == null)
				{
					abort(ERR_NULL_ID);
				}
				
				if (log.isDebugEnabled())
				{
					log.debug("Fetching DB for " + authenticated);
				}
				
				try
				{
					database = SASLOTPProfile.UserDatabase.getUser(authenticated);
					algorithm = SASLOTPProfile.getAlgorithm(database.AlgorithmName);
				}
				catch (SASLException x)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					abort(x.Message);
				}
				
				SupportClass.PutElement(credential, SessionCredential.ALGORITHM, algorithm.Name);
				
				// @todo we may want to clear the DB on file or something here,
				// consider it as we consider how an abstract library and more
				// implementation specific configurations play together, a
				// SASLOTPDatabase interface or something (such as what's in the
				// database package below sasl.otp might be used.  I've got
				// update and stuff, but we may want 'purge', and then extentions
				// in the init method of SASLOTPProfile to potentially load different
				// UserDatabase managing 'things'...so someone can load something
				// other than UserDictionaryPool at init/config time and use it.
				if ((database.Sequence) == 0)
				{
					abort(ERR_SEQUENCE_ZERO);
				}
				
				// Assign data
				state = STATE_ID;
				
				SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR, authenticated);
				
				if ((System.Object) authorized == null || authorized.Equals(""))
				{
					SupportClass.PutElement(credential, SessionCredential.AUTHORIZED, authenticated);
				}
				else
				{
					SupportClass.PutElement(credential, SessionCredential.AUTHORIZED, authorized);
				}
				
				SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR_TYPE, SASLOTPProfile.MECHANISM);
				
				System.Text.StringBuilder challenge = new System.Text.StringBuilder(128);
				
				challenge.Append(algorithm.Name);
				challenge.Append(SPACE);
				challenge.Append(database.Sequence);
				challenge.Append(SPACE);
				challenge.Append(database.Seed.ToLower());
				challenge.Append(SPACE);
				challenge.Append(EXT);
				if (log.isDebugEnabled())
				{
					log.debug("Generated Challenge=>" + challenge.ToString());
				}
				
				try
				{
					return new Blob(Blob.STATUS_NONE, challenge.ToString());
				}
				catch (System.Exception x)
				{
				}
				// This will throw a SASLException
				abort("Failed to issue SASL OTP challenge");
				return null;
			}
		}
		
		/// <summary> Listener API
		/// 
		/// Receive response to challenge, figure out if it
		/// works or throw an exception if it doesn't.
		/// 
		/// @todo must handle the init-hex and init-word responses
		/// if they're there.
		/// </summary>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'validateResponse'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual SessionCredential validateResponse(System.String response)
		{
			lock (this)
			{
				bool doInit = false;
				sbyte[] responseHash = null;
				
				log.debug("OTP Authenticator validating response");
				
				// If we're listening, the last state we should
				// have gotten to was STATE_ID (receiving the IDs)
				if (state != STATE_ID)
				{
					abort(ERR_OTP_STATE);
				}
				
				// Check results
				// The 'last hash' is the result of the hash function
				// on password+seed for sequence+1 times.
				// The pwd hash this time is for password_seed sequence
				// times.  By taking it and hashing it once, we should
				// get the last hash and that's how we verify.  Semi-slick.
				// So hooray for assymetric hash functions
				if (response.IndexOf(SASLOTPProfile.HEX_INIT) != - 1 || response.IndexOf(SASLOTPProfile.WORD_INIT) != - 1)
				{
					return validateInitResponse(response);
				}
				
				// Identify type of message, WORD or HEX
				if (response.IndexOf(WORD) != - 1)
				{
					response = response.Substring(WORD.Length);
					long l = OTPDictionary.convertWordsToHash(response);
					responseHash = SASLOTPProfile.convertLongToBytes(l);
					if (log.isDebugEnabled())
					{
						log.debug("Hacked response=>" + response);
					}
				}
				else if (response.IndexOf(HEX) != - 1)
				{
					response = response.Substring(HEX.Length);
					responseHash = SASLOTPProfile.convertHexToBytes(response);
					if (log.isDebugEnabled())
					{
						log.debug("Hacked response=>" + response);
					}
				}
				else
				{
					abort(ERR_UNEXPECTED_MESSAGE);
				}
				
				if ((database.Sequence) == 0)
				{
					throw new SequenceZeroFailure();
				}
				
				SessionCredential cred = validateHash(responseHash);
				database.updateLastHash(SASLOTPProfile.convertBytesToHex(responseHash));
				SASLOTPProfile.UserDatabase.updateUserDB(database);
				return cred;
			}
		}
		
		private SessionCredential validateHash(sbyte[] hash)
		{
			if ((database.Sequence) == 0)
			{
				throw new SequenceZeroFailure();
			}
			
			// Get the two hashes
			sbyte[] nextHash = database.LastHash;
			sbyte[] responseHash = algorithm.generateHash(hash);
			if (log.isTraceEnabled())
			{
				log.trace("Test====>" + SASLOTPProfile.convertBytesToHex(responseHash));
				log.trace("Control=>" + SASLOTPProfile.convertBytesToHex(nextHash));
			}
			bool match = true;
			for (int i = 0; i < 8; i++)
			///@TODO change to use array compare
			{
				if (nextHash[i] != responseHash[i])
					match = false; ///@TODO break;
			}
			if (!match)
				throw new SASLException(ERR_OTP_AUTH_FAILURE);
			
			// Success
			state = STATE_COMPLETE;
			if (credential.Count == 0)
				return null;
			return new SessionCredential(credential);
		}
		
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'validateInitResponse'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual SessionCredential validateInitResponse(System.String response)
		{
			lock (this)
			{
				log.debug("Validating init-* response");
				// Extract the various elements of the request
				System.String oldHashData;
				sbyte[] oldHash = null;
				System.String newHashData;
				System.String newParms;
				int i = response.IndexOf(COLON);
				System.String command;
				
				try
				{
					// Extract Chunks of data
					SupportClass.Tokenizer st = new SupportClass.Tokenizer(response, COLON);
					command = st.NextToken();
					oldHashData = st.NextToken();
					newParms = st.NextToken();
					newHashData = st.NextToken();
					if (log.isDebugEnabled())
					{
						log.debug("Command=>" + command);
						log.debug("OldHashData=>" + oldHashData);
						log.debug("newParms=>" + newParms);
						log.debug("newHashData=>" + newHashData);
					}
					
					// Validate login
					Algorithm a = SASLOTPProfile.getAlgorithm(database.AlgorithmName);
					if (SASLOTPProfile.HEX_INIT.StartsWith(command))
					{
						log.debug("CMD is " + SASLOTPProfile.HEX_INIT);
						oldHash = SASLOTPProfile.convertHexToBytes(oldHashData);
					}
					else if (SASLOTPProfile.WORD_INIT.StartsWith(command))
					{
						log.debug("CMD is " + SASLOTPProfile.WORD_INIT);
						// @todo obviate the 2nd step when you get a chance
						long l = OTPDictionary.convertWordsToHash(oldHashData);
						oldHash = SASLOTPProfile.convertLongToBytes(l);
					}
					else
					{
						abort(ERR_UNKNOWN_COMMAND + command);
					}
					
					if (log.isDebugEnabled())
					{
						log.debug("Retrieved from init-* oldHash=>" + SASLOTPProfile.convertBytesToHex(oldHash));
					}
					
					// Compare the hash and fail if it doesn't match
					SessionCredential cred = validateHash(oldHash);
					
					// Extract the stuff
					st = new SupportClass.Tokenizer(newParms);
					System.String algorithm = st.NextToken();
					System.String sequence = st.NextToken();
					System.String seed = st.NextToken().ToLower();
					if (!OTPGenerator.validateSeed(seed))
						abort("Invalid Seed");
					st = new SupportClass.Tokenizer(newParms);
					// Now do even weirder update of the db.
					if (log.isDebugEnabled())
					{
						log.debug("Auth=>" + authenticated);
						log.debug("Hash=>" + newHashData);
					}
					SASLOTPProfile.UserDatabase.addUser(authenticated, algorithm, newHashData, seed, sequence);
					log.debug("Successful Authentication!");
					return cred;
				}
				//UPGRADE_NOTE: Exception 'java.lang.Throwable' was converted to 'System.Exception' which has different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1100"'
				catch (System.Exception t)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					throw new SASLException(ERR_INIT + response + t.Message);
				}
			}
		}
		
		/// <summary> Initiator API
		/// 
		/// ALL of the routines below are the Initiator calls.
		/// </summary>
		internal OTPAuthenticator(SASLOTPProfile otpProfile, UserDatabase db, System.String pwd, System.String authorizedId, System.String authenticateId)
		{
			InitBlock();
			init(otpProfile, db, pwd, authorizedId, authenticateId);
		}
		
		private void  init(SASLOTPProfile otpProfile, UserDatabase db, System.String pwd, System.String authorizedId, System.String authenticateId)
		{
			log.debug("OTP Authenticator Initiator Construtor");
			
			authenticated = authenticateId;
			authorized = authorizedId;
			credential = new System.Collections.Hashtable();
			database = db;
			
			if (log.isDebugEnabled())
			{
				log.debug("Dict.getA()" + database.AlgorithmName);
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				log.debug("Dict.getA()" + SASLOTPProfile.getAlgorithm(database.AlgorithmName));
			}
			
			algorithm = SASLOTPProfile.getAlgorithm(database.AlgorithmName);
			profile = otpProfile;
			password = pwd;
			state = STATE_UNKNOWN;
			
			SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR_TYPE, SASLOTPProfile.MECHANISM);
			SupportClass.PutElement(credential, SessionCredential.ALGORITHM, algorithm.Name);
			SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR, authenticateId);
			
			if ((System.Object) authorizedId == null || authorizedId.Equals(""))
			{
				SupportClass.PutElement(credential, SessionCredential.AUTHORIZED, authenticateId);
			}
			else
			{
				SupportClass.PutElement(credential, SessionCredential.AUTHORIZED, authorizedId);
			}
		}
		
		// Weird init version
		internal OTPAuthenticator(SASLOTPProfile otpProfile, UserDatabase db, System.String pwd, System.String authorizedId, System.String authenticateId, System.String newAlgorithm, System.String newHash, System.String newSeed, System.String newSequence)
		{
			InitBlock();
			System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
			sb.Append(COLON);
			sb.Append(newAlgorithm);
			sb.Append(SPACE);
			sb.Append(newSequence);
			sb.Append(SPACE);
			sb.Append(newSeed.ToLower());
			sb.Append(COLON);
			sb.Append(newHash);
			initData = sb.ToString();
			init(otpProfile, db, pwd, authorizedId, authenticateId);
		}
		
		/// <summary> Initiator API used by SASL-OTP consumers that don't use
		/// the data on the startChannel option
		/// 
		/// If it works, we should get a challenge in our receiveRPY
		/// callback ;)
		/// </summary>
		internal virtual void  sendIdentity(System.String authorizeId, System.String authenticateId)
		{
			log.debug("OTP Authenticator sending Identities");
			
			// Grok and validate the parameters
			int limit = authenticateId.Length;
			
			if ((System.Object) authorizeId != null)
			{
				limit += authorizeId.Length;
			}
			
			System.Text.StringBuilder temp = new System.Text.StringBuilder(limit);
			
			if ((System.Object) authorizeId != null)
			{
				temp.Append(authorizeId);
			}
			
			temp.Append((char) 0);
			
			temp.Append(authenticateId);
			if (log.isDebugEnabled())
			{
				log.debug("AuthOTP Using=>" + temp.ToString() + "<=");
			}
			Blob blob = new Blob(Blob.STATUS_NONE, temp.ToString());
			if (log.isDebugEnabled())
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				log.debug("AuthOTP Using=>" + blob.ToString() + "<=");
			}
			try
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				channel.sendMSG(new StringOutputDataStream(blob.ToString()), (ReplyListener) this);
			}
			catch (BEEPException x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				abort(x.Message);
			}
		}
		
		/// <summary> Initiator API
		/// Receive Challenge, respond with a hash.
		/// </summary>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'receiveChallenge'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual void  receiveChallenge(Blob blob)
		{
			lock (this)
			{
				log.debug("OTP Authenticator received Challenge");
				
				// If we're initiating, the last state we should
				// have gotten to was STATE_STARTED
				if (state != STATE_STARTED)
				{
					abortNoThrow(ERR_OTP_STATE);
				}
				
				if (blob.Status.Equals(Blob.ABORT))
				{
					abort(ERR_PEER_ABORTED);
				}
				System.String challenge = blob.Data;
				
				// Parse Challenge, provide response
				state = STATE_CHALLENGE;
				
				int sequence = 0;
				System.String seed = null, algo = null;
				
				if (log.isDebugEnabled())
				{
					log.debug("Tokenizing=>" + challenge);
				}
				
				SupportClass.Tokenizer st = new SupportClass.Tokenizer(challenge);
				
				if (st.Count != 4)
				{
					abort("Failed to understand server's Challenge" + st.Count);
				}
				
				algo = st.NextToken();
				algorithm = SASLOTPProfile.getAlgorithm(algo);
				
				if (algorithm == null)
				{
					abort("Unrecognized algorithm in server challenge");
				}
				
				sequence = System.Int32.Parse(st.NextToken());
				seed = st.NextToken().ToLower();
				if (!OTPGenerator.validateSeed(seed))
					abort("Invalid Seed");
				
				if (log.isDebugEnabled())
				{
					log.debug("Algo is=>" + algo + " seed is=>" + seed + " seq=>" + sequence);
				}
				
				System.String phrase = new System.Text.StringBuilder(seed + password).ToString();
				password = null;
				sbyte[] response = null, temp;
				
				temp = SupportClass.ToSByteArray(SupportClass.ToByteArray(phrase));
				
				for (int i = 0; i < sequence; i++)
				{
					response = algorithm.generateHash(temp);
					temp = response;
				}
				
				if (log.isDebugEnabled())
				{
					log.debug(SASLOTPProfile.convertBytesToHex(temp));
				}
				long l = profile.convertBytesToLong(temp);
				phrase = new System.Text.StringBuilder(WORD + OTPDictionary.convertHashToWords(l)).ToString();
				
				if (log.isDebugEnabled())
				{
					log.debug("Prelim response is =>" + phrase + "<=");
				}
				
				// IF this is an init request   
				if ((System.Object) initData != null)
				{
					System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
					sb.Append(SASLOTPProfile.HEX_INIT);
					sb.Append(SASLOTPProfile.convertBytesToHex(temp));
					sb.Append(initData);
					phrase = sb.ToString();
					if (log.isDebugEnabled())
					{
						log.debug("Produced INIT response of " + phrase);
					}
				}
				try
				{
					blob = new Blob(Blob.STATUS_CONTINUE, phrase);
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					channel.sendMSG(new StringOutputDataStream(blob.ToString()), this);
				}
				catch (BEEPException x)
				{
					throw new SASLException("Unable to send response to challenge");
				}
			}
		}
		
		/// <summary> Initiator API
		/// Receive response to challenge, figure out if it
		/// works or throw an exception if it doesn't.
		/// </summary>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'receiveCompletion'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual SessionCredential receiveCompletion(System.String response)
		{
			lock (this)
			{
				log.debug("OTP Authenticator Completing!");
				
				// If we're initiating, the last state we should
				// have gotten to was STATE_CHALLENGE
				if (state != STATE_CHALLENGE)
				{
					abort(ERR_OTP_STATE);
				}
				
				state = STATE_COMPLETE;
				
				return new SessionCredential(credential);
			}
		}
		
		internal virtual void  abort(System.String message)
		{
			log.error("Aborting OTP Authenticator because " + message);
			state = STATE_ABORT;
			throw new SASLException(message);
		}
		
		/// <summary> This is designed to be called by initiator methods
		/// that want to abort the authentication by sending
		/// a MSG with a <blob status='abort'>reason</blob> 
		/// to their peer.
		/// </summary>
		internal virtual void  abortNoThrow(System.String message)
		{
			log.error("Aborting OTP Authenticator because " + message);
			state = STATE_ABORT;
		}
		
		/// <summary> Method receiveMSG
		/// Listener API
		/// 
		/// We receive MSGS - IDs, and extended responses (hash)
		/// in response to our challenges and stuff.
		/// 
		/// </summary>
		/// <param name="Message">message is the data we've received.
		/// We parse it to see if it's identity information, an
		/// abort, or otherwise.
		/// 
		/// @throws BEEPError if an ERR message is generated
		/// that's relative to the BEEP protocol is encountered.
		/// </param>
		public virtual void  receiveMSG(MessageMSG message)
		{
			try
			{
				log.debug("OTP Authenticator.receiveMSG");
				
				System.String data = null;
				Blob blob = null;
				
				if ((state != STATE_STARTED) && (state != STATE_ID))
				{
					abort(ERR_OTP_STATE);
				}
				
				// Read the data in the message and produce a Blob
				try
				{
					System.IO.Stream is_Renamed = message.DataStream.InputStream;
					long available;
					available = is_Renamed.Length - is_Renamed.Position;
					int limit = (int) available;
					sbyte[] buff = new sbyte[limit];
					SupportClass.ReadInput(is_Renamed, ref buff, 0, buff.Length);
					blob = new Blob(new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(buff))));
					data = blob.Data;
				}
				catch (System.IO.IOException x)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					abort(x.Message);
				}
				
				if (log.isDebugEnabled())
				{
					log.debug("MSG DATA=>" + data);
				}
				System.String status = blob.Status;
				
				if (((System.Object) status != null) && status.Equals(SASLProfile.SASL_STATUS_ABORT))
				{
					abort(ERR_PEER_ABORTED);
				}
				
				if (state == STATE_STARTED)
				{
					Blob reply = receiveIDs(data);
					try
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						message.sendRPY(new StringOutputDataStream(reply.ToString()));
					}
					catch (BEEPException x)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						throw new SASLException(x.Message);
					}
					return ;
				}
				
				// Process the user's password and stuff.
				SessionCredential cred = null;
				cred = validateResponse(data);
				
				if (cred != null)
				{
					profile.finishListenerAuthentication(cred, channel.Session);
					
					state = STATE_COMPLETE;
					
					if (log.isDebugEnabled())
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						log.debug("" + channel.Session + " is valid for\n" + cred.ToString());
					}
					try
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						message.sendRPY(new StringOutputDataStream(new Blob(Blob.STATUS_COMPLETE).ToString()));
						channel.setRequestHandler(null);
					}
					catch (BEEPException x)
					{
						profile.failListenerAuthentication(channel.Session, authenticated);
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						abortNoThrow(x.Message);
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						message.Channel.Session.terminate(x.Message);
						return ;
					}
				}
			}
			catch (SASLException s)
			{
				try
				{
					// Gotta reply, might as well send the abort, even
					// if it's redundant
					profile.failListenerAuthentication(channel.Session, authenticated);
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					Blob reply = new Blob(Blob.STATUS_ABORT, s.Message);
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					message.sendRPY(new StringOutputDataStream(reply.ToString()));
					//                channel.setDataListener(null);
				}
				catch (BEEPException x)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					message.Channel.Session.terminate(s.Message);
				}
			}
		}
		
		/// <summary> Method receiveRPY
		/// Initiator API
		/// 
		/// We receive replies to our ID messages, and to our extended responses
		/// Initiator API
		/// 
		/// </summary>
		/// <param name="Message">message is the data we've received.
		/// We parse it to see if it's identity information, an
		/// abort, or otherwise.
		/// 
		/// </param>
		public virtual void  receiveRPY(Message message)
		{
			log.debug("OTP Authenticator.receiveRPY");
			
			Blob blob = null;
			// Don't send an abort if we got one.
			bool sendAbort = true;
			
			try
			{
				if ((state != STATE_STARTED) && (state != STATE_CHALLENGE) && (state != STATE_COMPLETE))
				{
					sendAbort = true;
				}
				
				try
				{
					System.IO.Stream is_Renamed = message.DataStream.InputStream;
					long available;
					available = is_Renamed.Length - is_Renamed.Position;
					int limit = (int) available;
					sbyte[] buff = new sbyte[limit];
					SupportClass.ReadInput(is_Renamed, ref buff, 0, buff.Length);
					blob = new Blob(new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(buff))));
				}
				catch (System.IO.IOException x)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					abort(x.Message);
				}
				
				System.String status = blob.Status;
				
				if (((System.Object) status != null) && status.Equals(SASLProfile.SASL_STATUS_ABORT))
				{
					log.debug("OTPAuthenticator receiveRPY got an RPY=>" + blob.Data);
					sendAbort = false;
					abort(ERR_PEER_ABORTED + blob.Data);
				}
				
				// If this reply is a reply to our authenticate message
				if (state == STATE_STARTED)
				{
					receiveChallenge(blob);
					return ;
				}
				// If it's a reply to our authentication request
				else if ((System.Object) blob.Status != (System.Object) Blob.ABORT)
				{
					// Success case
					// Set creds...
					profile.finishInitiatorAuthentication(new SessionCredential(credential), channel.Session);
					
					lock (this)
					{
						System.Threading.Monitor.Pulse(this);
					}
					return ;
				}
				else
				{
					// Error case
					abort(ERR_UNKNOWN_COMMAND);
					return ;
				}
			}
			catch (System.Exception x)
			{
				log.error(x);
				lock (this)
				{
					System.Threading.Monitor.Pulse(this);
				}
				// Throw an error
				// Do a flag to indicate when this happens?
				if (sendAbort)
				{
					try
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						Blob a = new Blob(Blob.STATUS_ABORT, x.Message);
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						channel.sendMSG(new StringOutputDataStream(a.ToString()), this);
					}
					catch (BEEPException y)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						message.Channel.Session.terminate(y.Message);
					}
				}
			}
		}
		
		/// <summary> Method receiveERR
		/// 
		/// Initiator API
		/// 
		/// Generally we get this if our challenge fails or
		/// our authenticate identity is unacceptable or the
		/// hash we use isn't up to snuff etc.
		/// 
		/// </summary>
		/// <param name="Message">message is the data we've received.
		/// We parse it to see if it's identity information, an
		/// abort, or otherwise.
		/// 
		/// </param>
		public virtual void  receiveERR(Message message)
		{
			log.debug("OTP Authenticator.receiveERR");
			
			try
			{
				System.IO.Stream is_Renamed = message.DataStream.InputStream;
				long available;
				available = is_Renamed.Length - is_Renamed.Position;
				int limit = (int) available;
				sbyte[] buff = new sbyte[limit];
				
				SupportClass.ReadInput(is_Renamed, ref buff, 0, buff.Length);
				Blob b = new Blob(new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(buff))));
				if (log.isDebugEnabled())
				{
					log.debug("ERR received=>\n" + b.Data);
				}
				abort(new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(buff))));
				
				lock (this)
				{
					System.Threading.Monitor.Pulse(this);
				}
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				abortNoThrow(x.Message);
			}
		}
		
		/*
		* Method receiveANS
		* This method should never be called
		* 
		* @param Message message is the data we've received.
		* We parse it to see if it's identity information, an
		* abort, or otherwise.
		* 
		*/
		public virtual void  receiveANS(Message message)
		{
			message.Channel.Session.terminate(ERR_UNEXPECTED_MESSAGE);
		}
		
		/*
		* Method receiveNUL
		* This method should never be called
		* 
		* @param Message message is the data we've received.
		* We parse it to see if it's identity information, an
		* abort, or otherwise.
		* 
		* @throws TerminateException if some issue with the Message
		* that's relative to the BEEP protocol is encountered.
		*/
		public virtual void  receiveNUL(Message message)
		{
			message.Channel.Session.terminate(ERR_UNEXPECTED_MESSAGE);
		}
	}
}