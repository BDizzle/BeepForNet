/*
* BufferSegment.java  $Revision: 1.1 $ $Date: 2004/09/21 08:47:00 $
*
* Copyright (c) 2001 Huston Franklin.  All rights reserved.
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
namespace beepcore.beep.util
{
	
	/// <summary>A <code>BufferSegment</code> represents a BEEP Frame payload and holds
	/// the BEEP Frames's Header, Trailer and the message payload.
	/// 
	/// It contains a byte array an offset into the array and the
	/// length from the offset.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:47:00 $</version>
	public class BufferSegment
	{
		virtual public byte[] Data
		{
			get
			{
				return this.data;
			}
			
		}
		virtual public int Offset
		{
			get
			{
				return this.offset;
			}
			
		}
		virtual public int Length
		{
			get
			{
				return this.length;
			}
			
		}
		/// <summary>Constructor BufferSegment</summary>
		/// <param name="data">A byte array containing a BEEP Frame payload.</param>
		public BufferSegment(byte[] data)
		{
			this.data = data;
			this.offset = 0;
			this.length = data.Length;
		}
		
		/// <summary>Constructor BufferSegment</summary>
		/// <param name="data">A byte array containing a BEEP Frame payload.</param>
		/// <param name="offset">Indicates the begining position of the BEEP Frame
		/// payload in the byte array <code>data</code>.</param>
		/// <param name="length">Number of valid bytes in the byte array starting from
		/// <code>offset</code>.</param>
		public BufferSegment(byte[] data, int offset, int length)
		{
			this.data = data;
			this.offset = offset;
			this.length = length;
		}
		
		private byte[] data;
		private int offset;
		private int length;
	}
}