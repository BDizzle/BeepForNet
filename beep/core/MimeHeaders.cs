using System;
using beepcore.beep.util;

namespace beepcore.beep.core
{
	public class MimeHeaders
	{
		/// <summary>The default <code>DataStream</code> content type
		/// ("application/octet-stream").</summary>
		public const string DEFAULT_CONTENT_TYPE = "application/octet-stream";
		
		/// <summary>The default <code>DataStream</code> content transfer encoding
		/// ("binary").</summary>
		public const string DEFAULT_CONTENT_TRANSFER_ENCODING = "binary";
		
		/// <summary><code>DataStream</code> content type ("application/beep+xml");</summary>
		public const string BEEP_XML_CONTENT_TYPE = "application/beep+xml";
		
		/// <summary></summary>
		public const string CONTENT_TYPE = "Content-Type";
		
		/// <summary></summary>
		public const string CONTENT_TRANSFER_ENCODING = "Content-Transfer-Encoding";
		
		private const string NAME_VALUE_SEPARATOR = ": ";
		private static readonly byte[] NAME_VALUE_SEPARATOR_bytes = System.Text.Encoding.ASCII.GetBytes(NAME_VALUE_SEPARATOR);

		private const string HEADER_SUFFIX = "\r\n";
		private static readonly byte[] HEADER_SUFFIX_bytes = System.Text.Encoding.ASCII.GetBytes(HEADER_SUFFIX);

		private static readonly int HEADER_FORMAT_LENGTH = NAME_VALUE_SEPARATOR_bytes.Length + HEADER_SUFFIX_bytes.Length;
		
		private const int MAX_BUFFER_SIZE = 128;
		
		// guessing most hashtables will only contain the 2 default values
		// private const int DEFAULT_HEADER_TABLE_SIZE = 2;

		// length of header portion of data stream
		private int lenHeaders;
		
		// a case-insensitive comparer used for all comparisons (header names are not case-sensitive)
		private System.Collections.IComparer comparer;
		private System.Collections.IDictionary mimeHeadersTable;

		/// <summary>Returns or sets the value of the MIME entity header <code>Content-Type</code>.</summary>
		virtual public string ContentType
		{
			get
			{
				return this.getHeaderValue(CONTENT_TYPE);
			}
			
			set
			{
				this.setHeader(CONTENT_TYPE, value);
			}
			
		}
		/// <summary>Returns an <code>Enumeration</code> of all the names of the MIME entity
		/// headers.
		/// Use this call in conjunction with <code>getHeaderValue</code> to iterate
		/// through all the corresponding MIME entity header <code>value</code>(s).</summary>
		/// <returns>An <code>Enumeration</code> of all the MIME entity header
		/// names.</returns>
		virtual public System.Collections.IEnumerator HeaderNames
		{
			get
			{
				return this.mimeHeadersTable.Keys.GetEnumerator();
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
				return this.getHeaderValue(CONTENT_TRANSFER_ENCODING);
			}
			
			set
			{
				this.setHeader(CONTENT_TRANSFER_ENCODING, value);
			}	
		}

		virtual public BufferSegment BufferSegment
		{
			get
			{
				byte[] headersBytes = new byte[this.lenHeaders];

				// this is just to keep track of the position in the buffer
				System.IO.MemoryStream s = new System.IO.MemoryStream(headersBytes);

				// write the headers
				foreach(string name in mimeHeadersTable.Keys) {
					string hvalue = (string) mimeHeadersTable[name];
					
					if (!isDefaultHeaderValue(name, hvalue))
					{
						// copy name
						byte[] name_bytes = System.Text.Encoding.ASCII.GetBytes(name);
						s.Write(name_bytes, 0, name_bytes.Length);
						
						// ": "
						s.Write(NAME_VALUE_SEPARATOR_bytes, 0, NAME_VALUE_SEPARATOR_bytes.Length);
						
						// copy value
						byte[] hvalue_bytes = System.Text.Encoding.ASCII.GetBytes(hvalue);
						s.Write(hvalue_bytes, 0, hvalue_bytes.Length);

						// CRLF
						s.Write(HEADER_SUFFIX_bytes, 0, HEADER_SUFFIX_bytes.Length);
					}
				}
				
				// write the CRLF that separates the headers and the data
				s.Write(HEADER_SUFFIX_bytes, 0, HEADER_SUFFIX_bytes.Length);
				
				return new BufferSegment(headersBytes);
			}
			
		}
		/// <summary>Creates <code>MimeHeaders</code> using the default content type
		/// <code>DEFAULT_CONTENT_TYPE</code> and default content transfre encoding
		/// <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code>.</summary>
		public MimeHeaders()
		{
			lenHeaders = HEADER_SUFFIX.Length;
			comparer = System.Collections.CaseInsensitiveComparer.DefaultInvariant;

			// most messages will have around 2 headers... no need to waste
			// time and memory on a Hashtable
			mimeHeadersTable = new System.Collections.Specialized.ListDictionary(comparer);

			//mimeHeadersTable = new System.Collections.Hashtable(DEFAULT_HEADER_TABLE_SIZE,
			//	System.Collections.CaseInsensitiveHashCodeProvider.DefaultInvariant,
			//	comparer);
		}
		
		/// <summary>Creates <code>MimeHeaders</code> using the specified content type and
		/// the <code>DEFAULT_CONTENT_TRANSFER_ENCODING</code> content transfer
		/// encoding.</summary>
		public MimeHeaders(string contentType): this()
		{
			this.ContentType = contentType;
		}
		
		/// <summary>Creates <code>MimeHeaders</code> using the specified content type and
		/// content transfer encoding.</summary>
		public MimeHeaders(string contentType, string transferEncoding): this(contentType)
		{
			this.TransferEncoding = transferEncoding;
		}
		
		/// <summary>Retrieves the correspoding <code>value</code> to a given a MIME entity
		/// header <code>name</code>.</summary>
		/// <param name="name">Name of the MIME entity header.</param>
		/// <returns>The <code>value</code> of the MIME entity header.</returns>
		/// <exception cref="BEEPException" />
		public virtual string getHeaderValue(string name)
		{
			return (string) this.mimeHeadersTable[name];
		}
		
		/// <summary>Removes the <code>name</code> and <code>value</code> of a MIME entity
		/// header from the data stream.  Returns <code>true</code> if the
		/// <code>name</code> was successfully removed.</summary>
		/// <param name="name">Name of the header to be removed from the data stream.</param>
		/// <returns>Returns </code>true<code> if header was removed.  Otherwise,
		/// returns <code>false</code>.</returns>
		public virtual bool removeHeader(string name)
		{
			string hvalue = (string) mimeHeadersTable[name];
			
			/// <summary>@todo change to not allow the removal of content-type and
			/// transfer-encoding.</summary>
			if ((object) hvalue != null)
			{
				if (this.mimeHeadersTable.Contains(name))
				{
					this.mimeHeadersTable.Remove(name);

					if (!isDefaultHeaderValue(name, hvalue))
					{
						this.lenHeaders -= (name.Length + hvalue.Length + MimeHeaders.HEADER_FORMAT_LENGTH);
					}
					
					return true;
				}
			}
			
			return false;
		}
		
		/// <summary>Adds a MIME entity header to this data stream.</summary>
		/// <param name="name">Name of the MIME enitity header.</param>
		/// <param name="hvalue">Value of the MIME entity header.</param>
		public virtual void setHeader(string name, string hvalue)
		{
			if (this.mimeHeadersTable.Contains(name))
			{
				// must removeHeader before replacing it, for lenHeaders bookkeeping
				removeHeader(name);
			}
			
			this.mimeHeadersTable[name] = hvalue;
			
			if(!isDefaultHeaderValue(name, hvalue))
			{
				this.lenHeaders += name.Length + hvalue.Length + MimeHeaders.HEADER_FORMAT_LENGTH;
			}
		}

		/// <summary>
		/// Returns true if the header name and value provided are the default and therefore do not need
		/// to be sent.
		/// </summary>
		/// <param name="name">Name of the MIME enitity header.</param>
		/// <param name="hvalue">Value of the MIME entity header.</param>
		/// <returns></returns>
		private bool isDefaultHeaderValue(string name, string hvalue)
		{
			return (comparer.Compare(name, CONTENT_TYPE)==0 && comparer.Compare(hvalue, DEFAULT_CONTENT_TYPE)==0) 
				|| (comparer.Compare(name, CONTENT_TRANSFER_ENCODING)==0 && comparer.Compare(hvalue, DEFAULT_CONTENT_TRANSFER_ENCODING)==0);
		}
	}
}