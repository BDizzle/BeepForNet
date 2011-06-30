/*
* StringOutputDataStream.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
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
using BufferSegment = beepcore.beep.util.BufferSegment;

namespace beepcore.beep.core
{
	/// <summary><code>StringOutputDataStream</code> represents a BEEP message's
	/// payload.  Allows implementors to treat a <code>String</code> as a
	/// <code>DataSream</code>. The <code>String</code> is stored as a
	/// <code>byte[]</code> using UTF-8 encoding.  <p> <b>Note that this
	/// implementation is not synchronized.</b> If multiple threads access
	/// a <code>StringOutputDataStream</code> concurrently, data may be
	/// inconsistent or lost.</summary>
	/// <seealso cref="beepcore.beep.core.OutputDataStream"></seealso>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	public class StringOutputDataStream: OutputDataStream
	{
		/// <summary>The default <code>StringOutputDataStream</code> String encoding
		/// ("UTF-8").</summary>
		private const string DEFAULT_STRING_ENCODING = "utf-8";

		private string encName = DEFAULT_STRING_ENCODING;

		/// <summary>Returns the encoding used to convert the <code>String</code> to a
		/// <code>bytes[]</code>.</summary>
		virtual public string Encoding
		{
			get
			{
				return this.encName;
			}
		}

		/// <summary>Returns or sets the value of the MIME entity header <code>Content-Type</code>.</summary>
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

		/// <summary>Returns an <code>Enumeration</code> of all the names of the MIME entity
		/// headers in this data stream.
		/// Use this call in conjunction with <code>getHeaderValue</code> to iterate
		/// through all the corresponding MIME entity header <code>value</code>(s)
		/// in this data stream.</summary>
		/// <returns>An <code>Enumeration</code> of all the MIME entity header
		/// names.</returns>
		/// <exception cref="BEEPException" />
		virtual public System.Collections.IEnumerator HeaderNames // FIXME return ICollection instead
		{
			get
			{
				return this.mimeHeaders.HeaderNames;
			}			
		}

		/// <summary>Returns or sets the value of the MIME entity header
		/// <code>Content-Transfer-Encoding</code>.</summary>
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


		private StringOutputDataStream(string data, string enc, MimeHeaders headers)
		{
			encName = enc;

			add(new BufferSegment(System.Text.Encoding.GetEncoding(enc).GetBytes(data)));
		}

		/// <summary>Creates a <code>StringOutputDataStream</code> with a
		/// <code>String</code> and a <code>BEEP_XML_CONTENT_TYPE</code>
		/// content type and a transfer encoding of
		/// <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		/// <param name="data">A <code>String</code> representing a message's payload.</param>
		public StringOutputDataStream(string data): this(data, DEFAULT_STRING_ENCODING, new MimeHeaders())
		{
		}
		
		/// <summary>Creates a <code>StringOutputDataStream</code> with a
		/// <code>String</code> and a specified content type and a transfer
		/// encoding of <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		/// <param name="contentType">Content type of <code>data</code></param>
		/// <param name="data">A <code>String</code> representing a message's payload.</param>
		public StringOutputDataStream(string contentType, string data): this(data, DEFAULT_STRING_ENCODING, new MimeHeaders(contentType))
		{
		}
		
		/// <summary>Creates a <code>StringOutputDataStream</code> with a
		/// <code>String</code> and a specified content type and a transfer
		/// encoding of <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		/// <param name="contentType">Content type of <code>data</code></param>
		/// <param name="transferEncoding">Encoding Transfer encoding type of
		/// <code>data</code>.</param>
		/// <param name="data">A <code>String</code> representing a message's payload.</param>
		public StringOutputDataStream(string contentType, string transferEncoding, string data): this(data, DEFAULT_STRING_ENCODING, new MimeHeaders(contentType, transferEncoding))
		{
		}
		
		/// <summary>Creates a <code>StringOutputDataStream</code> with a
		/// <code>String</code> and a specified content type and encoding.</summary>
		/// <param name="contentType">Content type of <code>data</code></param>
		/// <param name="transferEncoding">Encoding Transfer encoding type of
		/// <code>data</code>.</param>
		/// <param name="data">A <code>String</code> representing a message's payload.</param>
		/// <param name="enc">The encoding used when converting <code>data</code> to a
		/// <code>byte[]</code> array.</param>
		public StringOutputDataStream(string contentType, string transferEncoding, string data, string enc): this(data, enc, new MimeHeaders(contentType, transferEncoding))
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
		public virtual void  setHeaderValue(string name, string hvalue)
		{
			this.mimeHeaders.setHeader(name, hvalue);
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