/*
* Frame.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2001,2002 Huston Franklin.  All rights reserved.
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
using System.Collections;
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
using BufferSegment = beepcore.beep.util.BufferSegment;
using HeaderParser = beepcore.beep.util.HeaderParser;
namespace beepcore.beep.core
{
	
	/// <summary>Frame encapsulates a BEEP protocol frame for MSG, RPY, ERR, ANS and NUL
	/// BEEP message types.
	/// Contains a the <code>Channel</code> this frame belongs to, the BEEP Frame
	/// Payload which holds the BEEP Frames's Header, Trailer, and the message
	/// payload.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	/// <seealso cref="BufferSegment"></seealso>
	public class Frame
	{
		/// <summary>Returns an array of <code>BufferSegment</code> objects.</summary>
		virtual public BufferSegment[] getBytes()
		{
			BufferSegment[] b = new BufferSegment[this.payload.Count + 2];
			this.size = 0;
			
			int j = 1;
			foreach(BufferSegment bs in this.payload)
			{
				b[j] = bs;
				this.size += b[j].Length;
				++j;
			}
			
			b[0] = new BufferSegment(buildHeader());
			b[j] = trailerBufferSegment;
			
			return b;
		}
			
		/// <summary>Returns the <code>payload</code> of a <code>Frame</code>.
		/// A <code>BufferSegment</code> contains a BEEP Frames Payload.</summary>
		/// <seealso cref="BufferSegment"></seealso>
		virtual public System.Collections.ICollection Payload
		{
			get
			{
				return this.payload;
			}
			
		}
		/// <summary>Returns the message type of this <code>Frame</code>.</summary>
		virtual public string MessageTypeString
		{
			get
			{
				return MessageTypeStrings.GetMessageTypeString(this.messageType);
			}
			
		}
		/// <summary>Returns the <code>Channel</code> to which this <code>Frame</code>
		/// belongs.</summary>
		/// <seealso cref="Channel"></seealso>
		virtual public IChannel Channel
		{
			get
			{
				return this.channel;
			}
			
		}
		/// <summary>Returns the message number of this <code>Frame</code>.</summary>
		virtual public int Msgno
		{
			get
			{
				return this.msgno;
			}
			
		}
		/// <summary>Returns the <code>seqno</code> of this <code>Frame</code>.</summary>
		virtual public long Seqno
		{
			get
			{
				return this.seqno;
			}
			
		}
		/// <summary>Returns the <code>size</code> of the payload for this
		/// <code>Frame</code>.</summary>
		virtual public int Size
		{
			get
			{
				return this.size;
			}
			
		}
		/// <summary>Returns the answer number of this <code>Frame</code>.</summary>
		virtual public int Ansno
		{
			get
			{
				return this.ansno;
			}
			
		}
		
		private const string CRLF = "\r\n";
		
		public const string TRAILER = "END\r\n";

		public static readonly int MAX_HEADER_SIZE = (3 + 1 + 10 + 1 + 10 + 1 + 1 + 1 + 10 + 1 + 10 + 1 + 10 + CRLF.Length);
		public static readonly int MIN_HEADER_SIZE = (3 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + CRLF.Length);
		public static readonly int MIN_FRAME_SIZE = MIN_HEADER_SIZE + Frame.TRAILER.Length;
		public static readonly int MAX_ANS_NUMBER = System.Int32.MaxValue; // 2147483647;
		public static readonly int MAX_CHANNEL_NUMBER = System.Int32.MaxValue;
		public static readonly int MAX_MESSAGE_NUMBER = System.Int32.MaxValue;

		public const long MAX_SEQUENCE_NUMBER = 4294967295L;

		public static readonly int MAX_SIZE = System.Int32.MaxValue;
		
		private static readonly BufferSegment trailerBufferSegment = new BufferSegment(System.Text.Encoding.ASCII.GetBytes(TRAILER));
		
		private Log log = LogFactory.getLog(typeof(Frame));
		
		/// <summary>BEEP message type of  <code>Frame</code>. </summary>
		private core.MessageType messageType;
		
		/// <summary><code>Channel</code> to which <code>Frame</code> belongs. </summary>
		private ChannelImpl channel;
		
		/// <summary>Message number of <code>Frame</code>. </summary>
		private int msgno;
		
		/// <summary>Answer number of this BEEP <code>Frame</code>. </summary>
		private int ansno;
		
		/// <summary>Sequence number of a BEEP message.  <code>seqno</code> is equal to
		/// the 'seqno' of the BEEP message header.</summary>
		private long seqno;
		
		/// <summary>Size of the payload of a BEEP message.  <code>size</code> is equal to
		/// the 'size' of the BEEP message header.</summary>
		private int size;
		
		/// <summary>Specifies whether this is the final frame of the message (i.e. the
		/// continuation indicator 'more' is equal to a '.' and not a '*').</summary>
		private bool last;
		
		/// <summary>The payload of a BEEP message.</summary>
		private ArrayList payload = new ArrayList();
		
		internal Frame(core.MessageType messageType, ChannelImpl channel, int msgno, bool last, long seqno, int size, int ansno)
		{
			this.messageType = messageType;
			this.channel = channel;
			this.msgno = msgno;
			this.last = last;
			this.seqno = seqno;
			this.size = size;
			this.ansno = ansno;
		}
		
		/// <summary>Adds the <code>BufferSegment</code> to the list representing the
		/// payload for this frame.</summary>
		public virtual void  addPayload(BufferSegment buf)
		{
			this.payload.Add(buf);
		}
		
		/// <summary>Returns the message type of this <code>Frame</code>.</summary>
		public virtual core.MessageType MessageType
		{
			get 
			{
				return this.messageType;
			}
		}
		
		/// <summary>Indicates if this is the last <code>Frame</code> in a sequence of frames</summary>
		public virtual bool isLast()
		{
			return this.last;
		}
		
		internal virtual void  setLast()
		{
			this.last = true;
		}
		
		/// <summary>Builds a BEEP Header from the given <code>Frame</code> and returns it
		/// as a byte array.</summary>
		/// <param name="f"><code>Frame</code> from which we derive the BEEP Header.</param>
		internal virtual byte[] buildHeader()
		{
			System.Text.StringBuilder header = new System.Text.StringBuilder(Frame.MAX_HEADER_SIZE);
			
			// Create header
			header.Append(MessageTypeStrings.GetMessageTypeString(this.messageType));
			header.Append(' ');
			header.Append(this.channel.NumberAsString);
			header.Append(' ');
			header.Append(this.msgno);
			header.Append(' ');
			header.Append((this.last?'.':'*'));
			header.Append(' ');
			header.Append(this.seqno);
			header.Append(' ');
			header.Append(this.size);
			
			if (this.messageType == core.MessageType.MESSAGE_TYPE_ANS)
			{
				header.Append(' ');
				header.Append(this.ansno);
			}
			
			header.Append(Frame.CRLF);
			
			if (log.isTraceEnabled())
			{
				log.trace(header.ToString());
			}
			return System.Text.Encoding.ASCII.GetBytes(header.ToString());
		}
		
		internal static Frame parseHeader(SessionImpl session, byte[] headerBuffer, int length)
		{
			HeaderParser header = new HeaderParser(headerBuffer, length);
			
			core.MessageType msgType = MessageTypeStrings.GetMessageType(header.parseType());
			int channelNum = header.parseInt();
			int msgNum = header.parseInt();
			bool last = header.parseLast();
			long seqNum = header.parseUnsignedInt();
			int size = header.parseInt();
			
			int ansNum = - 1;
			if (header.hasMoreTokens())
			{
				ansNum = header.parseInt();
			}
			
			if (header.hasMoreTokens())
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			return new Frame(msgType, session.getValidChannel(channelNum), msgNum, last, seqNum, size, ansNum);
		}
		
		/// <summary>BEEP message type for utility only.</summary>
		private class MessageTypeStrings
		{
			private const int MESSAGE_TYPE_MAX = 6;
			
			static MessageTypeStrings()
			{
				types = new string[MESSAGE_TYPE_MAX];
				types[(int)core.MessageType.MESSAGE_TYPE_UNK] = "UNK";
				types[(int)core.MessageType.MESSAGE_TYPE_ANS] = "ANS";
				types[(int)core.MessageType.MESSAGE_TYPE_MSG] = "MSG";
				types[(int)core.MessageType.MESSAGE_TYPE_ERR] = "ERR";
				types[(int)core.MessageType.MESSAGE_TYPE_RPY] = "RPY";
				types[(int)core.MessageType.MESSAGE_TYPE_NUL] = "NUL";
			}

			//    public static LinkedList types = new LinkedList();
			public static string[] types;
			
			private MessageTypeStrings()
			{
			}
			
			internal static core.MessageType GetMessageType(string type)
			{
				int ret = 0;
				int i = 0;
				
				for (i = 0; i < MESSAGE_TYPE_MAX; i++)
				{
					if (type.Equals(types[i]))
					{
						ret = i;
						
						break;
					}
				}
				
				return (core.MessageType)ret;
			}
			
			internal static string GetMessageTypeString(core.MessageType type)
			{
				return types[(int)type];
			}

		}
	}
}