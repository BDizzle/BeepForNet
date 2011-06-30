/*
* OutputDataStream.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
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
using beepcore.beep.util;

namespace beepcore.beep.core
{
	/// <summary><code>OutputDataStream</code> represents a BEEP message's payload as a
	/// stream.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class OutputDataStream
	{
		protected MimeHeaders mimeHeaders;
		
		private ArrayList buffers = new ArrayList();
		private bool complete = false;
		private bool headersSent = false;
		private int curOffset = 0;
		private ChannelImpl channel = null;

		virtual internal ChannelImpl Channel
		{
			set
			{
				this.channel = value;
			}
			
		}
		
		/// <summary>Creates an <code>OutputDataStream</code> without any mime
		/// headers. It is the responsibility of the application to ensure
		/// the mime headers exist in the first <code>BufferSegment</code>
		/// added.</summary>
		public OutputDataStream() : this(null, null)
		{
		}
		
		/// <summary>Creates an <code>OutputDataStream</code> using the specified
		/// mime headers.</summary>
		/// <param name="headers">Mime headers to be prepended to the buffers in
		/// the stream.</param>
		public OutputDataStream(MimeHeaders headers) : this(headers, null)
		{
		}
		
		/// <summary>Creates an <code>OutputDataStream</code> using the specified
		/// mime headers.</summary>
		/// <param name="headers">Mime headers to be prepended to the buffers in
		/// the stream.</param>
		public OutputDataStream(MimeHeaders headers, BufferSegment buf)
		{
			this.mimeHeaders = headers;
			if(buf != null) 
			{
				this.add(buf);
			}
		}
		
		public virtual void add(BufferSegment segment)
		{
			this.buffers.Add(segment);
			if (channel != null)
			{
				try
				{
					channel.sendQueuedMessages();
				}
				catch (BEEPException)
				{
				}
			}
		}
		
		/// <deprecated></deprecated>
		public virtual void close()
		{
			this.setComplete();
		}
		
		/// <summary>Returns <code>true</code> if no more bytes will be added to
		/// those currently available on this stream.  Returns
		/// <code>false</code> if more bytes are expected.</summary>
		public virtual bool isComplete()
		{
			return this.complete;
		}
		
		public virtual void setComplete()
		{
			this.complete = true;
			if (channel != null)
			{
				try
				{
					channel.sendQueuedMessages();
				}
				catch (BEEPException)
				{
				}
			}
		}
		
		internal virtual bool availableSegment()
		{
			return (buffers.Count > 0);
		}
		
		internal virtual BufferSegment getNextSegment(int maxLength)
		{
			if (this.headersSent == false)
			{
				if (this.mimeHeaders != null)
				{
					this.buffers.Insert(0, mimeHeaders.BufferSegment);
				}
				this.headersSent = true;
			}
			
			BufferSegment b = (BufferSegment) buffers[0];
			
			if (curOffset != 0 || maxLength < b.Length)
			{
				
				int origLength = b.Length;
				
				b = new BufferSegment(b.Data, b.Offset + curOffset, System.Math.Min(maxLength, origLength - curOffset));
				
				if (curOffset + b.Length != origLength)
				{
					curOffset += b.Length;
					return b;
				}
			}
			
			buffers.RemoveAt(0);
			curOffset = 0;
			
			return b;
		}
	}
}