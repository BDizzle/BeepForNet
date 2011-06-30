/*
* AnonymousAuthenticator.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2003 Huston Franklin.  All rights reserved.
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
using Log = org.apache.commons.logging.Log;
using LogFactory = org.apache.commons.logging.LogFactory;
using org.beepcore.beep.core;
using org.beepcore.beep.profile.sasl;
namespace org.beepcore.beep.profile.sasl.anonymous
{
	
	/// <summary> This class encapsulates the state associated with
	/// an ongoing SASL-Anonymous Authentication, and
	/// provides methods to handle the exchange.  The
	/// AnonymousAuthenticator provides inter-message
	/// state for the exchange, which is normally
	/// quite simple, and can in fact be handled complete
	/// in the start channel exchange.  This isn't mandatory
	/// however, and so this class has been provided to
	/// support that non-piggybacked start channel case.
	/// 
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
	class AnonymousAuthenticator : RequestHandler, ReplyListener
	{
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
		}
		
		// Constants
		// Authentication States
		public const int STATE_UNKNOWN = 0;
		public const int STATE_STARTED = 1;
		public const int STATE_ID = 2;
		public const int STATE_COMPLETE = 3;
		public const int STATE_ABORT = 4;
		
		// Err Messages
		public const System.String ERR_ANON_STATE = "Illegal state transition";
		public const System.String ERR_PEER_ABORTED = "Our BEEP Peer has aborted this authentication sequence";
		public const System.String ERR_IDENTITY_PARSE_FAILURE = "Invalid identity information submitted for Anonymous Authentication";
		public const System.String ERR_UNEXPECTED_MESSAGE = "Unexpected SASL-Anonymous Message";
		
		// Data
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		private int state;
		private Channel channel;
		private System.Collections.Hashtable credential;
		private SASLAnonymousProfile profile;
		private System.String authenticated;
		
		/// <summary> Listener API
		/// 
		/// All of the routines below, but prior to the Initiator API,
		/// are the Listener calls
		/// </summary>
		
		/// <summary> AnonymouAuthenticator is the constructor used by the Listener.
		/// It means someone has started a SASL anon channel and hasn't
		/// yet authenticated and this object has been constructed to
		/// track that.
		/// 
		/// </summary>
		/// <param name="SASLAnonymousProfile">the instance of the profile used
		/// in the authentication.
		/// 
		/// @throws SASLException
		/// 
		/// </param>
		internal AnonymousAuthenticator(SASLAnonymousProfile anonymousProfile)
		{
			InitBlock();
			log.debug("Creating Listener ANONYMOUS Authenticator");
			
			credential = new System.Collections.Hashtable();
			profile = anonymousProfile;
			state = STATE_UNKNOWN;
			
			SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR_TYPE, SASLAnonymousProfile.MECHANISM);
		}
		
		/// <summary> API for both Listener and Initiator</summary>
		/// <summary> Method started is called when the channel has been
		/// 'started', and basically modifies the authenticators
		/// state appropriately (including setting the Authenticator
		/// as the replyListener for the Channel used).
		/// 
		/// </summary>
		/// <param name="Channel">is used to set the data member, so we
		/// know what channel is used for this authentication.
		/// 
		/// @throws SASLException
		/// 
		/// </param>
		internal virtual void  started(Channel ch)
		{
			log.debug("Starting Anonymous Authenticator");
			
			if (state != STATE_UNKNOWN)
			{
				throw new SASLException(ERR_ANON_STATE);
			}
			state = STATE_STARTED;
			ch.setRequestHandler(this);
			channel = ch;
		}
		
		/// <summary> Listener API
		/// 
		/// Receive IDs, respond with a Challenge or Exception
		/// </summary>
		/// <summary> Method receiveID is called when the Initiator of the
		/// authentication sends its information (identity).
		/// @todo make the inbound parameter a blob instead..or
		/// does that break piggybacking?..no, it shouldn't, the
		/// piggybacked authentication can deal with it and just
		/// catch the exception.
		/// 
		/// </summary>
		/// <param name="String">data, the user's information
		/// </param>
		/// <param name="">Channel
		/// 
		/// @throws SASLException
		/// 
		/// </param>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'receiveID'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual Blob receiveID(System.String data)
		{
			lock (this)
			{
				log.debug("Anonymous Authenticator Receiving ID");
				
				// If we're listening, the last state we should
				// have gotten to was STATE_STARTED (after the channel start)
				if (state != STATE_STARTED)
				{
					abort(ERR_ANON_STATE);
				}
				
				if ((System.Object) data == null)
				{
					abort(ERR_IDENTITY_PARSE_FAILURE);
				}
				
				// Assign data
				state = STATE_ID;
				
				SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR, data);
				SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR_TYPE, SASLAnonymousProfile.MECHANISM);
				
				try
				{
					return new Blob(Blob.STATUS_COMPLETE);
				}
				catch (System.Exception x)
				{
				}
				abort("Failed to complete SASL Anonymous authentication");
				return null;
			}
		}
		
		
		/// <summary> Initiator API used by SASL-ANON consumers that don't use
		/// the data on the startChannel option
		/// 
		/// If it works, we should get a challenge in our receiveRPY
		/// callback ;)
		/// </summary>
		internal virtual void  sendIdentity(System.String authenticateId)
		{
			log.debug("Anonymous Authenticator sending Identity");
			
			if ((System.Object) authenticateId == null)
				throw new SASLException(ERR_IDENTITY_PARSE_FAILURE);
			
			if (log.isDebugEnabled())
			{
				log.debug("Using=>" + authenticateId + "<=");
			}
			Blob blob = new Blob(Blob.STATUS_NONE, authenticateId);
			if (log.isDebugEnabled())
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				log.debug("Using=>" + blob.ToString() + "<=");
			}
			try
			{
				SupportClass.PutElement(credential, SessionCredential.AUTHENTICATOR, authenticateId);
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				channel.sendMSG(new StringOutputDataStream(blob.ToString()), this);
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				abort(x.Message);
			}
			state = STATE_ID;
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
				log.debug("Anonymous Authenticator Completing!");
				
				// If we're initiating, the last state we should
				// have gotten to was STATE_CHALLENGE
				if (state != STATE_ID)
				{
					abort(ERR_ANON_STATE);
				}
				state = STATE_COMPLETE;
				return new SessionCredential(credential);
			}
		}
		
		/// <summary> Cheat here, if we don't want to send anything back, then
		/// we don't do a damn thing...just abort.
		/// 
		/// The params are a bit complex.  The reply boolean indicates
		/// whether or not to send a reply or a message.
		/// 
		/// The channel parameter is non-null if we are to send ANYTHING
		/// AT ALL.  If it's null, we don't send.  This is kind of
		/// kludgey.
		/// @todo make it cleaner.
		/// </summary>
		internal virtual void  abort(System.String msg)
		{
			log.debug("Aborting Anonymous Authenticator");
			log.debug(msg);
			state = STATE_ABORT;
			throw new SASLException(msg);
		}
		
		internal virtual void  abortNoThrow(System.String msg)
		{
			log.debug("Aborting Anonymous Authenticator");
			log.debug(msg);
			state = STATE_ABORT;
		}
		
		/// <summary> Method receiveMSG
		/// Listener API
		/// 
		/// We receive MSGS - IDs and stuff.
		/// 
		/// </summary>
		/// <param name="Message">message is the data we've received.
		/// We parse it to see if it's identity information, an
		/// abort, or otherwise.
		/// 
		/// @throws BEEPError if an ERR message is generated
		/// </param>
		public virtual void  receiveMSG(MessageMSG message)
		{
			try
			{
				log.debug("Anonymous Authenticator.receiveMSG");
				System.String data = null;
				Blob blob = null;
				
				if (state != STATE_STARTED)
				{
					abort(ERR_ANON_STATE);
				}
				try
				{
					// Read the data in the message and produce a Blob
					System.IO.Stream is_Renamed = message.DataStream.InputStream;
					long available;
					available = is_Renamed.Length - is_Renamed.Position;
					int limit = (int) available;
					byte[] buff = new byte[limit];
					SupportClass.ReadInput(is_Renamed, ref buff, 0, buff.Length);
					blob = new Blob(new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(buff))));
					data = blob.Data;
				}
				catch (System.IO.IOException x)
				{
					log.error("", x);
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
					try
					{
						Blob reply = receiveID(data);
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						message.sendRPY(new StringOutputDataStream(reply.ToString()));
					}
					catch (BEEPException x)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						abort(x.Message);
						// @todo weird and iffy, cuz we may have sent...
						// Unsolicited message is probably better than
						// a waiting peer, so let's abort and blow it up...
					}
					profile.finishListenerAuthentication(new SessionCredential(credential), channel.Session);
				}
			}
			catch (SASLException s)
			{
				try
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					Blob reply = new Blob(Blob.STATUS_ABORT, s.Message);
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					message.sendRPY(new StringOutputDataStream(reply.ToString()));
				}
				catch (BEEPException t)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					message.Channel.Session.terminate(t.Message);
				}
			}
		}
		
		/// <summary> Method receiveRPY
		/// Initiator API
		/// 
		/// We receive replies to our ID messages
		/// 
		/// </summary>
		/// <param name="Message">message is the data we've received.
		/// We parse it to see if it's identity information, an
		/// abort, or otherwise.
		/// 
		/// </param>
		public virtual void  receiveRPY(Message message)
		{
			log.debug("Anonymous Authenticator.receiveRPY");
			
			Blob blob = null;
			bool sendAbort = true;
			
			try
			{
				if (state != STATE_ID)
				{
					abort(ERR_ANON_STATE);
				}
				
				try
				{
					System.IO.Stream is_Renamed = message.DataStream.InputStream;
					long available;
					available = is_Renamed.Length - is_Renamed.Position;
					int limit = (int) available;
					byte[] buff = new byte[limit];
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
					log.debug("Anonymous Authenticator receiveRPY=>" + blob.Data);
					sendAbort = false;
					abort(ERR_PEER_ABORTED);
				}
				
				// If it's a reply to our authentication request
				if (!status.Equals(Blob.ABORT))
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
					abort(blob.Data);
				}
			}
			catch (SASLException x)
			{
				log.error(x);
				lock (this)
				{
					System.Threading.Monitor.Pulse(this);
				}
				try
				{
					if (sendAbort)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						Blob reply = new Blob(Blob.STATUS_ABORT, x.Message);
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						channel.sendMSG(new StringOutputDataStream(blob.ToString()), this);
					}
				}
				catch (System.Exception q)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					message.Channel.Session.terminate(q.Message);
				}
			}
		}
		
		/// <summary> Method receiveERR
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
			log.debug("Anonymous Authenticator.receiveERR");
			
			try
			{
				System.IO.Stream is_Renamed = message.DataStream.InputStream;
				long available;
				available = is_Renamed.Length - is_Renamed.Position;
				int limit = (int) available;
				byte[] buff = new byte[limit];
				
				SupportClass.ReadInput(is_Renamed, ref buff, 0, buff.Length);
				if (log.isDebugEnabled())
				{
					log.debug("SASL-Anonymous Authentication ERR received=>\n" + new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(buff))));
				}
				abortNoThrow(new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(buff))));
				
				lock (this)
				{
					System.Threading.Monitor.Pulse(this);
				}
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				message.Channel.Session.terminate(x.Message);
			}
		}
		
		/// <summary> Method receiveANS
		/// This method should never be called
		/// 
		/// </summary>
		/// <param name="Message">message is the data we've received.
		/// We parse it to see if it's identity information, an
		/// abort, or otherwise.
		/// 
		/// </param>
		public virtual void  receiveANS(Message message)
		{
			message.Channel.Session.terminate(ERR_UNEXPECTED_MESSAGE);
		}
		
		/// <summary> Method receiveNUL
		/// This method should never be called
		/// 
		/// </summary>
		/// <param name="Message">message is the data we've received.
		/// We parse it to see if it's identity information, an
		/// abort, or otherwise.
		/// 
		/// </param>
		public virtual void  receiveNUL(Message message)
		{
			message.Channel.Session.terminate(ERR_UNEXPECTED_MESSAGE);
		}
	}
}