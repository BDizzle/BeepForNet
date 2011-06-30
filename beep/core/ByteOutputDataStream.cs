/*
* ByteOutputDataStream.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
using BufferSegment = beepcore.beep.util.BufferSegment;
namespace beepcore.beep.core
{
	
	
	/// <summary><code>ByteOutputDataStream</code> represents a BEEP message's
	/// payload.  Allows the implementor to treat a <code>byte[]</code> as
	/// a <code>DataStream</code>.  <p> <b>Note that this implementation is
	/// not synchronized.</b> If multiple threads access a
	/// <code>ByteOutputDataStream</code> concurrently, data may be
	/// inconsistent or lost.</summary>
	/// <seealso cref="beepcore.beep.core.OutputDataStream"></seealso>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class ByteOutputDataStream:OutputDataStream
	{
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions.
		/// <summary>Returns the value of the MIME entity header <code>Content-Type</code>.</summary>
		/// <summary>Sets the content type of a <code>DataStream</code>.</summary>
		/// <param name="contentType"></param>
		virtual public string ContentType
		{
			get
			{
				return this.mimeHeaders.ContentType;
			}
			
			set
			{
				this.mimeHeaders.ContentType = value;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions.
		/// <summary>Returns the value of the MIME entity header
		/// <code>Content-Transfer-Encoding</code>.</summary>
		/// <summary>Sets the content transfer encoding of a <code>DataStream</code></summary>
		/// <param name="transferEncoding"></param>
		virtual public string TransferEncoding
		{
			get
			{
				return this.mimeHeaders.TransferEncoding;
			}
			
			set
			{
				this.mimeHeaders.TransferEncoding = value;
			}
			
		}
		/// <summary>Returns an <code>Enumeration</code> of all the names of the MIME entity
		/// headers in this data stream.
		/// Use this call in conjunction with <code>getHeaderValue</code> to iterate
		/// through all the corresponding MIME entity header <code>value</code>(s)
		/// in this data stream.</summary>
		/// <returns>An <code>Enumeration</code> of all the MIME entity header
		/// names.</returns>
		/// <exception cref="BEEPException" />
		virtual public System.Collections.IEnumerator HeaderNames
		{
			get
			{
				return this.mimeHeaders.HeaderNames;
			}
			
		}
		
		/// <summary>Creates a <code>ByteOutputDataStream</code> from a
		/// <code>byte[]</code> with a content type of
		/// <code>DEFAULT_CONTENT_TYPE</code> and a transfer encoding of
		/// <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		/// <param name="data">A <code>byte[]</code> representing a message's payload.</param>
		public ByteOutputDataStream(byte[] data):this(data, 0, data.Length)
		{
		}
		
		/// <summary>Creates a <code>ByteOutputDataStream</code> from a <code>byte[]</code>
		/// with a specified content type and a transfer encoding
		/// of <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		/// <param name="contentType">Content type of <code>data</code>.</param>
		/// <param name="data">A <code>byte[]</code> representing a message's payload.</param>
		public ByteOutputDataStream(string contentType, byte[] data):this(contentType, data, 0, data.Length)
		{
		}
		
		/// <summary>Creates a <code>ByteOutputDataStream</code> from a <code>byte[]</code>
		/// with a specified content type and a specified transfer encoding.</summary>
		/// <param name="contentType">Content type of <code>data</code>.</param>
		/// <param name="transferEncoding">Encoding Transfer encoding type of
		/// <code>data</code>.</param>
		/// <param name="data">A <code>byte[]</code> representing a message's payload.</param>
		public ByteOutputDataStream(string contentType, string transferEncoding, byte[] data):this(contentType, transferEncoding, data, 0, data.Length)
		{
		}
		
		/// <summary>Creates a <code>ByteOutputDataStream</code> from a
		/// <code>byte[]</code> using the specified length and offset with
		/// a content type of <code>DEFAULT_CONTENT_TYPE</code> and a
		/// transfer encoding of
		/// <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		/// <param name="data">A <code>byte[]</code> representing a message's payload.</param>
		/// <param name="offset">The start offset in array <code>data</code> at which the
		/// data is written.</param>
		/// <param name="length">The maximum number of bytes to read.</param>
		public ByteOutputDataStream(byte[] data, int offset, int length):base(new MimeHeaders(), new BufferSegment(data, offset, length))
		{
		}
		
		/// <summary>Creates a <code>ByteOutputDataStream</code> from a
		/// <code>byte[]</code> using the specified length and offset and
		/// with a specified content type and a transfer encoding of
		/// <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		/// <param name="contentType">Content type of <code>byte[]</code>.</param>
		/// <param name="data">A <code>byte[]</code> representing a message's payload.</param>
		/// <param name="offset">The start offset in array <code>data</code> at which the
		/// data is written.</param>
		/// <param name="length">The maximum number of bytes to read.</param>
		public ByteOutputDataStream(string contentType, byte[] data, int offset, int length):base(new MimeHeaders(contentType), new BufferSegment(data, offset, length))
		{
		}
		
		/// <summary>Creates a <code>ByteOutputDataStream</code> from a
		/// <code>byte[]</code> using the specified length and offset and
		/// with a specified content type and a specified transfer
		/// encoding.</summary>
		/// <param name="contentType">Content type of <code>byte[]</code>.</param>
		/// <param name="transferEncoding">Encoding Transfer encoding type of
		/// <code>data</code>.</param>
		/// <param name="data">A <code>byte[]</code> representing a message's payload.</param>
		/// <param name="offset">The start offset in array <code>data</code> at which the
		/// data is written.</param>
		/// <param name="length">The maximum number of bytes to read.</param>
		public ByteOutputDataStream(string contentType, string transferEncoding, byte[] data, int offset, int length):base(new MimeHeaders(contentType, transferEncoding), new BufferSegment(data, offset, length))
		{
		}
		
		/// <summary>Returns <code>true</code> if no more bytes will be added to those
		/// currently available, if any, on this stream.  Returns
		/// <code>false</code> if more bytes are expected.</summary>
		public override bool isComplete()
		{
			return true;
		}
		
		/// <summary>Retrieves the correspoding <code>value</code> to a given a MIME entity
		/// header <code>name</code>.</summary>
		/// <param name="name">Name of the MIME entity header.</param>
		/// <returns>The <code>value</code> of the MIME entity header.</returns>
		/// <exception cref="BEEPException" />
		public virtual string getHeaderValue(string name)
		{
			return this.mimeHeaders.getHeaderValue(name);
		}
		
		/// <summary>Adds a MIME entity header to this data stream.</summary>
		/// <param name="name">Name of the MIME enitity header.</param>
		/// <param name="value">Value of the MIME entity header.</param>
		public virtual void  setHeaderValue(string name, string value_Renamed)
		{
			this.mimeHeaders.setHeader(name, value_Renamed);
		}
		
		/// <summary>Removes the <code>name</code> and <code>value</code> of a MIME entity
		/// header from the data stream.  Returns <code>true</code> if the
		/// <code>name</code> was successfully removed.</summary>
		/// <param name="name">Name of the header to be removed from the data stream.</param>
		/// <returns>Returns </code>true<code> if header was removed.  Otherwise,
		/// returns <code>false</code>.</returns>
		public virtual bool removeHeader(string name)
		{
			return this.mimeHeaders.removeHeader(name);
		}
	}
}