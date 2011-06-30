/*
* TCPSession.java  $Revision: 1.1 $ $Date: 2004/09/21 08:47:00 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2001-2003 Huston Franklin.  All rights reserved.
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
using System.Threading;
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
using beepcore.beep.core;
using beepcore.beep.util;

namespace beepcore.beep.transport.tcp
{
	
	
	/// <summary>Provides the TCP transport mapping for BEEP according to RFC 3081.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:47:00 $</version>
	public class TCPSession:SessionImpl
	{
		private void  InitBlock()
		{
			outputBuf = new byte[0];
			headerBuffer = new byte[Frame.MAX_HEADER_SIZE];
			log = LogFactory.getLog(this.GetType());
		}
		virtual public System.Net.Sockets.TcpClient Socket
		{
			get
			{
				return this.socket;
			}
			
		}
		override protected internal int MaxFrameSize
		{
			// Implementation of method declared in Session
			
			get
			{
				/// <summary>@todo - test this and find an optimal frame size, key it up
				/// to approximate ethernet packet size, less header, trailer</summary>
				return 1400;
			}
			
		}
		
		// Constants
		private const string MESSAGE_TYPE_SEQ = "SEQ";
		private const int MAX_RECEIVE_BUFFER_SIZE = 64 * 1024;
		private const string TCP_MAPPING = "TCP Mapping";
		private const string CRLF = "\r\n";

		private static readonly int MIN_SEQ_HEADER_SIZE = (3 + 1 + 1 + 1 + 1 + 1 + 1 + CRLF.Length);
		
		private const int CHANNEL_START_ODD = 1;
		private const int CHANNEL_START_EVEN = 2;
		
		
		// Instance Data
		private Log log;
		
		private byte[] headerBuffer;
		private byte[] outputBuf;
		private object writerLock;
		private System.Net.Sockets.TcpClient socket;
		private bool running;
		private static int THREAD_COUNT = 0;
		private const string THREAD_NAME = "TCPSession Thread #";
		private Thread thread;
		
		/// <param name="sock">the Socket for this TCPConnection</param>
		/// <param name="registry">the ProfileRegistry (set of profiles) to be used on
		/// this Session</param>
		/// <param name="firstChannel">the integer indicating the
		/// ordinality for this Peer, which determines whether Channels
		/// started by this peer have odd or even numbers.</param>
		/// <param name="localCred"></param>
		/// <param name="peerCred"></param>
		/// <remarks>
		/// The hack-ish part of this is the credential parameter.  Here's
		/// the deal.  (1) TCPSessionCreator has public methods that don't
		/// expose the credential (2) Therefore if it's called by the User,
		/// it can only have a null credential.  (3) If it's called via a
		/// Tuning Profile reset though, it can have a real value.  (4)
		/// When that's the case, we call a 'different' init method that
		/// doesn't block the thread of the former (soon to die) session.
		/// </remarks>
		/// <exception cref="BEEPException" />
		private TCPSession(System.Net.Sockets.TcpClient sock, ProfileRegistry registry, int firstChannel, SessionCredential localCred, SessionCredential peerCred, SessionTuningProperties tuning, string servername):base(registry, firstChannel, localCred, peerCred, tuning, servername)
		{
			InitBlock();
			
			socket = sock;
			writerLock = new object();
			
			if ((peerCred != null) || (localCred != null) || (tuning != null))
			{
				tuningInit();
			}
			else
			{
				init();
			}
			
			try
			{
				socket.ReceiveBufferSize = MAX_RECEIVE_BUFFER_SIZE;
			}
			catch (System.Exception)
			{
				log.debug("Socket doesn't support setting receive buffer size");
			}
		}
		
		/// <summary>Creates a TCPSession for a Socket that was created by
		/// initiating a connection.</summary>
		/// <param name="sock"></param>
		/// <param name="registry"></param>
		/// <param name="servername"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession createInitiator(System.Net.Sockets.TcpClient sock, ProfileRegistry registry, string servername)
		{
			return new TCPSession(sock, (ProfileRegistry) registry.Clone(), CHANNEL_START_ODD, null, null, null, servername);
		}
		/// <summary>Creates a TCPSession for a Socket that was created by
		/// initiating a connection.</summary>
		/// <param name="sock"></param>
		/// <param name="registry"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession createInitiator(System.Net.Sockets.TcpClient sock, ProfileRegistry registry)
		{
			return createInitiator(sock, registry, null);
		}
		
		/// <summary>Creates a TCPSession for a Socket that was created by
		/// listening and accepting a connection.</summary>
		/// <param name="sock"></param>
		/// <param name="registry"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession createListener(System.Net.Sockets.TcpClient sock, ProfileRegistry registry)
		{
			return new TCPSession(sock, (ProfileRegistry) registry.Clone(), CHANNEL_START_EVEN, null, null, null, null);
		}
		
		// Overrides method in Session
		public override void close()
		{
			base.close();

			lock (this)
			{
				if (socket != null)
				{
					try
					{
						socket.GetStream().Close();
						socket.Close();
					}
					catch (System.Exception)
					{
					}
					
					Console.WriteLine(AppDomain.GetCurrentThreadId() + " setting socket to null");

					socket = null;
				}
			}
		}
		
		// Overrides method in Session
		public override void terminate(string reason)
		{
			base.terminate(reason);

			running = false;

			lock (this)
			{
				if (socket != null)
				{
					try
					{
						socket.GetStream().Close();
						socket.Close();
					}
					catch (System.Exception)
					{
					}
					
					Console.WriteLine(AppDomain.GetCurrentThreadId() + " setting socket to null");

					socket = null;
				}
			}
		}
		
		/* (non-Javadoc)
		* @see java.lang.Object#toString()
		*/
		public override string ToString()
		{
			return base.ToString(); // + " (" + socket.getLocalAddress() + ":" + socket.getLocalPort() + "-" + socket.getInetAddress() + ":" + socket.getPort() + ")";
		}
		
		protected internal override void disableIO()
		{
			running = false;
		}
		
		// BUG disableIO followed by enableIO probably does not work properly (enableIO does not wait for the
		// old thread to die before starting a new one)
		protected internal override void enableIO()
		{
			lock (this)
			{
				running = false;
				thread = null;
				
				// BUG thread is set to null on the preceding line... ?
				if (thread == null)
				{
					string threadName;
					
					// BUG locking string constants is a bad practice (they may be shared cross-appdomain)
					lock (THREAD_NAME)
					{
						threadName = THREAD_NAME + (THREAD_COUNT++).ToString();
					}
					
					thread = new Thread(new ThreadStart(this.processNextFrame));
					thread.Name = threadName;

					thread.IsBackground = true;
					thread.Start();
				}
			}
		}

		/// <summary>Generates a header, then writes the header, payload, and
		/// trailer to the wire.</summary>
		/// <param name="f">the Frame to send.</param>
		/// <returns>boolean true if the frame was sent, false otherwise.</returns>
		/// 
		/// <exception cref="BEEPException" />
		/// 
		/// @todo make this one write operation later.
		protected internal override void sendFrame(Frame f)
		{
			try
			{
				System.IO.Stream os;
				lock (this)
				{
					if(socket != null) 
					{
						os = socket.GetStream();
					} 
					else 
					{
						throw new SessionAbortedException();
					}
				}

				lock (writerLock)
				{
					/* Inspite of the extra data copy if is faster to have
					* a single call to write() (at least with the JVMs we
					* have tested with).
					*/
					BufferSegment[] bs = f.getBytes();
					
					int n = 0;
					for (int i = 0; i < bs.Length; ++i)
					{
						n += bs[i].Length;
					}
					
					if (n > outputBuf.Length)
					{
						outputBuf = new byte[n];
					}
					
					int off = 0;
					
					for (int i = 0; i < bs.Length; ++i)
					{
						Array.Copy(bs[i].Data, bs[i].Offset,  outputBuf, off, bs[i].Length);
						
						off += bs[i].Length;
					}
					
					if (log.isTraceEnabled())
					{
						log.trace("C:" + System.Text.Encoding.ASCII.GetString(outputBuf));
					}
					
					os.Write(outputBuf, 0, n);
					os.Flush(); // has no effect on NetworkStream !
				}
			}
			catch (System.Exception e)
			{
				throw new BEEPException(e);
			}
		}
		
		protected internal override ISession reset(SessionCredential localCred, SessionCredential peerCred, SessionTuningProperties tuning, ProfileRegistry reg, object argument)
		{
			lock(this) 
			{

				if (log.isTraceEnabled())
				{
					log.trace("Reset as " + (Initiator?"INITIATOR":"LISTENER"));
				}
				
				System.Net.Sockets.TcpClient s = argument as System.Net.Sockets.TcpClient;
				
				if(s == null) 
				{
					s = this.socket;
				}
				
				if (reg == null)
				{
					reg = this.ProfileRegistry;
				}
				
				ISession newSession = new TCPSession(s, reg, (Initiator?CHANNEL_START_ODD:CHANNEL_START_EVEN), localCred, peerCred, tuning, null);
				
				this.fireSessionReset(newSession);
				
				return newSession;
			}
		}
		
		/// <summary>Update the channel window size with the remote peer by sending
		/// SEQ frames as per RFC 3081.</summary>
		/// <returns>true if the Receive Buffer Size was updated</returns>
		/// 
		/// <exception cref="BEEPException">if a specified buffer size is larger
		/// than what's available on the Socket.</exception>
		protected internal override bool updateMyReceiveBufferSize(IChannel channel, long currentSeq, int currentAvail)
		{
			lock(this) 
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder(Frame.MAX_HEADER_SIZE);
				
				sb.Append(MESSAGE_TYPE_SEQ);
				sb.Append(' ');
				sb.Append(this.getChannelNumberAsString(channel));
				sb.Append(' ');
				sb.Append(System.Convert.ToString(currentSeq));
				sb.Append(' ');
				sb.Append(System.Convert.ToString(currentAvail));
				sb.Append(CRLF);
				
				try
				{
					if (log.isDebugEnabled())
					{
						log.debug("Wrote: " + sb.ToString());
					}
					
					System.IO.Stream os;
					if(socket != null) 
					{
						os = socket.GetStream();
					} 
					else 
					{
						throw new SessionAbortedException();
					}
					
					lock (writerLock)
					{
						byte[] temp_byteArray = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
						os.Write(temp_byteArray, 0, temp_byteArray.Length);
						os.Flush();
					}
				}
				catch (System.IO.IOException x)
				{
					throw new BEEPException("Unable to send SEQ", x);
				}
				
				return true;
			}
		}
		
		/// <summary>
		/// The thread procedure for the TCPSession. Blocks on the socket stream.
		/// </summary>
		private void processNextFrame()
		{
			running = true;
			
			if (socket == null)
			{
				running = false;
				return;
			}
			
			try
			{
				while (running)
				{
					if (log.isTraceEnabled())
					{
						log.trace("Processing next frame");
					}
					
					int amountRead;
					
					System.Net.Sockets.NetworkStream ins;
					
					lock(this)
					{
						if(socket != null) 
						{
							ins = socket.GetStream();
						} 
						else 
						{
							throw new SessionAbortedException();
						}
					}

					try
					{
						amountRead = ins.Read(headerBuffer, 0, MIN_SEQ_HEADER_SIZE);

						if (amountRead == 0)
						{
							throw new SessionAbortedException();
						}
					}
					catch (System.IO.IOException e)
					{
						if (running)
						{
							throw e;
						}
						
						// socket closed intentionally (session closing)

						// Even though the remote peer has closed its socket we need to close ours !
						ins.Close();

						// so just return
						return ;
					}
					
					if (headerBuffer[0] == (byte) MESSAGE_TYPE_SEQ[0])
					{
						processSEQFrame(headerBuffer, amountRead, ins);
						continue;
					}
					else
					{
						if (processCoreFrame(headerBuffer, amountRead, ins) == false)
						{
							// Even though the remote peer has closed its socket we need to close ours !
							// ins.Close();
							
							break;
						}
					}
				}
			}
			catch (System.IO.IOException e)
			{
				log.error(e);

				terminate(e.Message);
			}
			catch (SessionAbortedException)
			{
				terminate("Session aborted by remote peer.");
			}
			catch (System.Exception e)
			{
				log.error(e);

				terminate(e.Message);
			}
			
			if (log.isDebugEnabled())
			{
				log.debug("Session listener thread exiting.  State = " + this.State);
			}
		}
		
		private bool processCoreFrame(byte[] headerBuffer, int amountRead, System.IO.Stream ins)
		{
			int headerLength = 0;
			int amountToRead = Frame.MIN_FRAME_SIZE - amountRead;
			
			while (true)
			{
				int tokenCount = 6;
				
				try
				{
					int n = ins.Read(headerBuffer, amountRead, amountToRead);
					
					if (n == 0)
					{
						throw new SessionAbortedException();
					}
					
					amountRead += n;
				}
				catch (System.Net.Sockets.SocketException e)
				{
					if (running)
					{
						throw e;
					}
					
					// socket closed intentionally (session closing)
					// so just return
					return false;
				}
				
				while (headerLength < amountRead)
				{
					if (headerBuffer[headerLength] == '\n')
					{
						if (headerLength == 0 || headerBuffer[headerLength - 1] != '\r')
						{
							throw new BEEPException("Malformed BEEP header");
						}
						
						++headerLength;

						goto headerFound_brk;
					}
					
					if (headerBuffer[headerLength] == ' ')
					{
						if (tokenCount > 1)
						{
							// This is for ANS frames
							--tokenCount;
						}
					}
					
					++headerLength;
				}
				
				if (headerLength > Frame.MAX_HEADER_SIZE)
				{
					throw new BEEPException("Malformed BEEP header, no CRLF");
				}
				
				/* 2 = 1 for the min token size and 1 is for the separator ' '
				* or "\r\n"
				*/
				amountToRead = (tokenCount * 2) + Frame.TRAILER.Length;
			}

headerFound_brk: ;
			
			if (log.isTraceEnabled())
			{
				log.trace("S:" + System.Text.Encoding.ASCII.GetString(headerBuffer, 0, headerLength));
			}
			
			Frame f = base.createFrame(headerBuffer, headerLength - CRLF.Length);
			byte[] payload = new byte[f.Size];
			
			int count = amountRead - headerLength;
			if (count > payload.Length)
			{
				Array.Copy(headerBuffer, headerLength, payload, 0, payload.Length);
				count -= payload.Length;
				
				for (int i = 0; i < Frame.TRAILER.Length; ++i)
				{
					int b;
					if (count > 0)
					{
						b = headerBuffer[headerLength + payload.Length + i];
						--count;
					}
					else
					{
						b = ins.ReadByte();
						
						if (b == - 1)
						{
							throw new SessionAbortedException();
						}
					}
					
					if (((byte) b) != ((byte) Frame.TRAILER[i]))
					{
						throw new BEEPException("Malformed BEEP frame, " + "invalid trailer");
					}
				}
			}
			else
			{
				Array.Copy(headerBuffer, headerLength, payload, 0, count);
				
				while (count < payload.Length)
				{
					int n = ins.Read(payload, count, payload.Length - count);
					if (n == 0)
					{
						throw new SessionAbortedException();
					}
					count += n;
				}
				
				if (log.isTraceEnabled())
				{
					log.trace("S:Payload\n" + System.Text.Encoding.ASCII.GetString(payload));
				}
				
				for (int i = 0; i < Frame.TRAILER.Length; ++i)
				{
					int b = ins.ReadByte();
					
					if (b == - 1)
					{
						throw new SessionAbortedException();
					}
					
					if (((byte) b) != ((byte) Frame.TRAILER[i]))
					{
						throw new BEEPException("Malformed BEEP frame, " + "invalid trailer");
					}
				}
			}
			
			f.addPayload(new BufferSegment(payload));
			
			return base.postFrame(f);
		}
		
		private void  processSEQFrame(byte[] headerBuffer, int amountRead, System.IO.Stream is_Renamed)
		{
			int headerLength = 0;
			int tokenCount = 4;
			
			while (true)
			{
				
				while (headerLength < amountRead)
				{
					if (headerBuffer[headerLength] == '\n')
					{
						if (headerLength == 0 || headerBuffer[headerLength - 1] != '\r')
						{
							throw new BEEPException("Malformed BEEP header");
						}
						
						++headerLength;

						goto headerFound1_brk;
					}
					
					if (headerBuffer[headerLength] == ' ')
					{
						if (tokenCount > 1)
						{
							--tokenCount;
						}
					}
					
					++headerLength;
				}
				
				if (headerLength > Frame.MAX_HEADER_SIZE)
				{
					throw new BEEPException("Malformed BEEP header, no CRLF");
				}
				
				/* 2 = 1 for the min token size + 1 for the separator ' '
				 * or "\r\n"
				 */
				int amountToRead = headerBuffer[headerLength - 1] == '\r'?1:tokenCount * 2;
				try
				{
					int n = is_Renamed.Read(headerBuffer, amountRead, amountToRead);
					
					if (n == 0)
					{
						throw new SessionAbortedException();
					}

					amountRead += n;
				}
				catch (System.Net.Sockets.SocketException e)
				{
					if (running)
					{
						throw e;
					}
					
					// socket closed intentionally (session closing)

					is_Renamed.Close();

					// so just return
					return ;
				}
			}

headerFound1_brk: ;
			
			// Process the header
			HeaderParser header = new HeaderParser(headerBuffer, headerLength - CRLF.Length);
			
			string type = header.parseType();
			if (!type.Equals(MESSAGE_TYPE_SEQ))
			{
				throw new BEEPException("Malformed BEEP header");
			}
			
			int channelNum = header.parseInt();
			long ackNum = header.parseUnsignedInt();
			int window = header.parseInt();
			
			if (header.hasMoreTokens())
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			// update the channel with the new receive window size
			this.updatePeerReceiveBufferSize(channelNum, ackNum, window);
		}
		
		private class SessionAbortedException:System.Exception
		{
		}
	}
}