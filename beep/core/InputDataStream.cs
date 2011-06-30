/*
* InputDataStream.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
using BufferSegment = beepcore.beep.util.BufferSegment;
namespace beepcore.beep.core
{
	
	/// <summary><code>InputDataStream</code> holds a stream of
	/// <code>BufferSegments</code>(s) and provides accessor methods to
	/// that stream.
	/// <p>
	/// <b>Note that this implementation is not synchronized.</b> If
	/// multiple threads access a <code>InputDataStream</code>
	/// concurrently, data may be inconsistent or lost.</summary>
	/// <seealso cref="beepcore.beep.util.BufferSegment"></seealso>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class InputDataStream
	{
		/// <summary>Returns an <code>InputStream</code> for reading the data in this stream.</summary>
		virtual public InputDataStreamAdapter InputStream
		{
			get
			{
				if (stream == null)
				{
					stream = new InputDataStreamAdapter(this);
				}
				
				return stream;
			}
			
		}
		/// <summary>Returns the next <code>BufferSegment</code> in this stream.</summary>
		virtual public BufferSegment NextSegment
		{
			get
			{
				BufferSegment b;
				
				lock (buffers)
				{
					b = (BufferSegment) buffers[0];
					buffers.RemoveAt(0);
				}
				
				if (this.channel != null)
				{
					this.channel.freeReceiveBufferBytes(b.Length);
				}
				
				this.availableBytes -= b.Length;
				
				return b;
			}
			
		}
		virtual public bool Closed
		{
			get
			{
				return closed;
			}
			
		}
		
		/// <summary>Creates a <code>InputDataStream</code>.</summary>
		internal InputDataStream()
		{
		}
		
		/// <summary>Creates a <code>InputDataStream</code>.
		/// For use with in <code>Channel</code>.</summary>
		/// <param name="channel">Is notified as BufferSegments are read this allows the
		/// Channel to update the receive window.</param>
		internal InputDataStream(ChannelImpl channel)
		{
			this.channel = channel;
		}
		
		/// <summary>Creates a <code>InputDataStream</code>.</summary>
		/// <param name="buf">The first buffer in the data stream.</param>
		internal InputDataStream(BufferSegment buf)
		{
			this.add(buf);
		}
		
		/// <summary>Creates a <code>InputDataStream</code>.</summary>
		/// <param name="buf">The first buffer in the data stream.</param>
		internal InputDataStream(BufferSegment buf, bool complete):this(buf)
		{
			if (complete)
			{
				this.setComplete();
			}
		}
		
		internal virtual void  add(BufferSegment segment)
		{
			if (this.closed)
			{
				if (this.channel != null)
				{
					this.channel.freeReceiveBufferBytes(segment.Length);
				}
				return ;
			}
			
			lock (this.buffers)
			{
				this.buffers.Add(segment);
				System.Threading.Monitor.Pulse(this.buffers);
			}
			this.availableBytes += segment.Length;
		}
		
		public virtual int available()
		{
			return this.availableBytes;
			//         int bytesAvailable = 0;
			
			//         synchronized (this.buffers) {
			//             Iterator i = this.buffers.iterator();
			//             while (i.hasNext()) {
			//                 bytesAvailable += ((BufferSegment) i.next()).getLength();
			//             }
			//         }
			
			//         return bytesAvailable;
		}
		
		/// <summary>Returns <code>true</code> if a <code>BufferSegment</code> is available
		/// to receive.</summary>
		public virtual bool availableSegment()
		{
			return (this.buffers.Count > 0);
		}
		
		/// <summary>Indicates that the application is finished receiving data from this
		/// stream. If there is more data available the data will be discarded.</summary>
		public virtual void  close()
		{
			this.closed = true;
			while (this.availableSegment())
			{
				beepcore.beep.util.BufferSegment generatedAux = this.NextSegment;
			}
		}
		
		/// <summary></summary>
		/// <returns>null if isComplete() is true.</returns>
		public virtual BufferSegment waitForNextSegment()
		{
			lock (buffers)
			{
				while (availableSegment() == false)
				{
					if (isComplete() == true)
					{
						return null;
					}
					System.Threading.Monitor.Wait(buffers);
				}
				return NextSegment;
			}
		}
		
		/// <summary>Returns <code>true</code> if no more bytes will be added to
		/// those currently available on this stream.  Returns
		/// <code>false</code> if more bytes are expected.</summary>
		public virtual bool isComplete()
		{
			return this.complete;
		}
		
		internal virtual void setComplete()
		{
			this.complete = true;
			lock (this.buffers)
			{
				System.Threading.Monitor.Pulse(this.buffers);
			}
		}
		
		internal ArrayList buffers = new ArrayList();
		private int availableBytes = 0;
		private ChannelImpl channel = null;
		private bool closed = false;
		private bool complete = false;
		private InputDataStreamAdapter stream = null;
	}
}