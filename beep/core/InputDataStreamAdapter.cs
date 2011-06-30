/*
* InputDataStreamAdapter.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2002 Huston Franklin.  All rights reserved.
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
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
using BufferSegment = beepcore.beep.util.BufferSegment;
namespace beepcore.beep.core
{
	public class InputDataStreamAdapter:System.IO.Stream
	{
		public override bool CanRead
		{
			get 
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override void Flush()
		{
			throw new System.NotSupportedException();
		}

		public override long Length
		{
			get
			{
				throw new System.NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				return this.pos;
			}
			set
			{
				throw new System.NotSupportedException();
			}
		}

		public int Read(byte[] b)
		{
			return Read(b, 0, b.Length);
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (offset == - 1 || count == - 1 || offset + count > buffer.Length)
				throw new System.IndexOutOfRangeException();
			
			int bytesRead = 0;
			while (bytesRead < count)
			{
				if (waitAvailable() == - 1)
				{
					break;
				}
				int n = internalRead(buffer, offset + bytesRead, count - bytesRead);
				if (n == 0 && ids.Closed)
				{
					return bytesRead != 0?bytesRead:- 1;
				}
				bytesRead += n;
			}
			
			return bytesRead;
		}

		public override int ReadByte()
		{
			if (waitAvailable() == -1)
			{
				return -1;
			}
			
			return internalReadByte();
		}


		public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			throw new System.NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new System.NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new System.NotSupportedException();
		}

		private void  InitBlock()
		{
			curBuf = zeroLength;
			log = LogFactory.getLog(this.GetType());
			state = STATE_INIT;
			name = new System.IO.MemoryStream(32);
			value_Renamed = new System.IO.MemoryStream(32);
			mimeHeaders = new System.Collections.Hashtable(DEFAULT_HEADER_TABLE_SIZE);
		}
		/// <summary>Returns the content type of a <code>FrameDataStrea</code>.  If the
		/// <code>Frame</code> containing the content type hasn't been received yet,
		/// the method blocks until it is received.</summary>
		/// <returns>Content type.</returns>
		/// <exception cref="BEEPException" />
		virtual public string ContentType
		{
			get
			{
				return this.getHeaderValue(MimeHeaders.CONTENT_TYPE);
			}
			
		}
		/// <summary>Returns an <code>Enumeration</code> of all the MIME entity header names
		/// belonging to this <code>FrameDataStream</code>.  If the
		/// <code>Frame</code> containing the content type hasn't been received yet,
		/// the method blocks until it is received.</summary>
		/// <exception cref="BEEPException" />
		virtual public System.Collections.IEnumerator HeaderNames
		{
			get
			{
				waitAvailable();
				
				return this.mimeHeaders.Keys.GetEnumerator();
			}
			
		}
		/// <summary>Returns the transfer encoding of a <code>FrameDataStrea</code>.  If the
		/// <code>Frame</code> containing the content type hasn't been received yet,
		/// the method blocks until it is received.</summary>
		/// <returns>Content type.</returns>
		/// <exception cref="BEEPException" />
		virtual public string TransferEncoding
		{
			get
			{
				return this.getHeaderValue(MimeHeaders.CONTENT_TRANSFER_ENCODING);
			}
			
		}
		
		internal InputDataStreamAdapter(InputDataStream ids)
		{
			InitBlock();
			this.ids = ids;
		}
		
		public int available()
		{
			if (this.state != STATE_HEADERS_PARSED)
			{
				parseHeaders();
				if (this.state != STATE_HEADERS_PARSED)
				{
					return 0;
				}
			}
			
			if (this.pos == this.curBuf.Length)
			{
				setNextBuffer();
			}
			return (this.curBuf.Length - this.pos) + this.ids.available();
		}
		
		public override void Close()
		{
			ids.close();
		}
		
		/// <summary>Returns the value of the MIME entity header which corresponds
		/// to the given <code>name</code>. If the <code>Frame</code>
		/// containing the content type hasn't been received yet, the
		/// method blocks until it is received.</summary>
		/// <param name="name">Name of the entity header.</param>
		/// <returns>String Value of the entity header.</returns>
		/// <exception cref="BEEPException" />
		public virtual string getHeaderValue(string name)
		{
			waitAvailable();
			
			return (string) this.mimeHeaders[name];
		}
		
		
		//UPGRADE_TODO: The equivalent of method 'java.io.InputStream.skip' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143"'
		public long skip(long n)
		{
			if (n > System.Int32.MaxValue)
			{
				return 0;
			}
			
			int bytesSkipped = 0;
			while (bytesSkipped < n && ids.Closed == false)
			{
				if (waitAvailable() == - 1)
				{
					break;
				}
				int i = System.Math.Min(((int) n) - bytesSkipped, curBuf.Length - pos);
				pos += i;
				bytesSkipped += i;
			}
			
			return bytesSkipped;
		}
		
		/// <summary>This version of read() does not block if there are no bytes
		/// available.</summary>
		private int internalReadByte()
		{
			if (setNextBuffer() == false)
			{
				return -1;
			}
			
			int b = curBuf.Data[curBuf.Offset + pos];
			
			pos++;
			
			return b;
		}
		
		/// <summary>This version of read(byte[], int, int) does not block if
		/// there are no bytes available.</summary>
		private int internalRead(byte[] b, int off, int len)
		{
			int bytesRead = 0;
			
			while (bytesRead < len)
			{
				if (setNextBuffer() == false)
				{
					break;
				}
				
				int n = System.Math.Min(len - bytesRead, curBuf.Length - this.pos);
				Array.Copy(curBuf.Data, curBuf.Offset + this.pos, b, off + bytesRead, n);
				
				this.pos += n;
				bytesRead += n;
			}
			
			return bytesRead;
		}
		
		private void  parseHeaders()
		{
			while (true)
			{
				switch (state)
				{
					
					case STATE_INIT: 
						state = STATE_PARSING_NAME;
						break;
					
					case STATE_PARSING_NAME: 
						parseName();
						if (state == STATE_PARSING_NAME)
						{
							return ;
						}
						break;
					
					case STATE_PARSING_NAME_TERMINATOR: 
						parseNameTerminator();
						if (state == STATE_PARSING_NAME_TERMINATOR)
						{
							return ;
						}
						break;
					
					case STATE_PARSING_VALUE: 
						parseValue();
						if (state == STATE_PARSING_VALUE)
						{
							return ;
						}
						break;
					
					case STATE_PARSING_VALUE_TERMINATOR: 
						parseValueTerminator();
						if (state == STATE_PARSING_VALUE_TERMINATOR)
						{
							return ;
						}
						break;
					
					case STATE_PARSING_HEADERS_TERMINATOR: 
						parseHeadersTerminator();
						if (state == STATE_PARSING_HEADERS_TERMINATOR)
						{
							return ;
						}
						break;
					
					case STATE_HEADERS_PARSED: 
						return ;
					}
			}
		}
		
		private void  parseHeadersTerminator()
		{
			int b = internalReadByte(); // move off of the LF
			if (b == - 1)
			{
				return ;
			}
			
			if (this.mimeHeaders[MimeHeaders.CONTENT_TYPE] == null)
			{
				this.mimeHeaders[MimeHeaders.CONTENT_TYPE] = MimeHeaders.DEFAULT_CONTENT_TYPE;
			}
			
			if (mimeHeaders[MimeHeaders.CONTENT_TRANSFER_ENCODING] == null)
			{
				this.mimeHeaders[MimeHeaders.CONTENT_TRANSFER_ENCODING] = MimeHeaders.DEFAULT_CONTENT_TRANSFER_ENCODING;
			}
			
			state = STATE_HEADERS_PARSED;
		}
		
		private void parseName()
		{
			int b = internalReadByte();
			if (b == - 1)
			{
				return ;
			}
			
			if (b == CR)
			{
				state = STATE_PARSING_HEADERS_TERMINATOR;
				return ;
			}
			
			name.WriteByte((byte) b);
			
			// find the ':'
			while (true)
			{
				b = internalReadByte();
				if (b == - 1)
				{
					return ;
				}
				
				if (b == COLON)
				{
					state = STATE_PARSING_NAME_TERMINATOR;
					return ;
				}
				
				name.WriteByte((byte) b);
			}
		}
		
		private void  parseNameTerminator()
		{
			int b = internalReadByte(); // we are pointing to ':' move off of ' '
			if (b == - 1)
			{
				return ;
			}
			state = STATE_PARSING_VALUE;
		}
		
		private void  parseValue()
		{
			while (true)
			{
				int b = internalReadByte();
				if (b == - 1)
				{
					return ;
				}
				
				if (b == CR)
				{
					state = STATE_PARSING_VALUE_TERMINATOR;
					return ;
				}
				
				value_Renamed.WriteByte((byte) b);
			}
		}
		
		private void  parseValueTerminator()
		{
			int b = internalReadByte(); // move off of LF
			if (b == - 1)
			{
				return ;
			}
			
			this.mimeHeaders[System.Text.Encoding.UTF8.GetString(name.ToArray())] =
				System.Text.Encoding.UTF8.GetString(value_Renamed.ToArray());

			name.SetLength(0);
			value_Renamed.SetLength(0);
			state = STATE_PARSING_NAME;
		}
		
		/// <summary>If there are no bytes remaining in the current buffer move to
		/// the next one if it exists.</summary>
		private bool setNextBuffer()
		{
			while (pos == curBuf.Length)
			{
				if (ids.availableSegment() == false)
				{
					return false;
				}
				
				curBuf = ids.NextSegment;
				pos = 0;
			}
			
			return true;
		}
		
		/// <summary>Wait until available() != 0</summary>
		private int waitAvailable()
		{
			int n;
			
			if ((n = available()) > 0) 
			{
				return n;
			}
			
			lock (ids.buffers)
			{
				while ((n = available()) == 0)
				{
					
					if (ids.isComplete() == true)
					{
						// no more bytes to read() and none are
						// expected, return -1
						return -1;
					}
					
					// no bytes available to read, but more are
					// expected... block
					try
					{
						System.Threading.Monitor.Wait(ids.buffers);
					}
					catch (System.Threading.ThreadInterruptedException e)
					{
						log.error("waiting for buffer", e);
					}
				}
				
				return n;
			}
		}
		//UPGRADE_NOTE: Final was removed from the declaration of 'zeroLength '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		private static BufferSegment zeroLength =	new BufferSegment(new byte[0]);
		
		//UPGRADE_NOTE: There is an untranslated Statement.  Please refer to original code. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1153"'
		private int pos = 0;
		//UPGRADE_NOTE: The initialization of  'curBuf' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private BufferSegment curBuf;
		
		private const int LF = 10;
		private const int CR = 13;
		private const int COLON = 58;
		
		private const int STATE_INIT = 0;
		private const int STATE_PARSING_NAME = 1;
		private const int STATE_PARSING_NAME_TERMINATOR = 2;
		private const int STATE_PARSING_VALUE = 3;
		private const int STATE_PARSING_VALUE_TERMINATOR = 4;
		private const int STATE_PARSING_HEADERS_TERMINATOR = 5;
		private const int STATE_HEADERS_PARSED = 6;
		
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		//UPGRADE_NOTE: The initialization of  'state' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private int state;
		//UPGRADE_NOTE: The initialization of  'name' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private System.IO.MemoryStream name;
		//UPGRADE_NOTE: The initialization of  'value_Renamed' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private System.IO.MemoryStream value_Renamed;
		private const int DEFAULT_HEADER_TABLE_SIZE = 2;
		//UPGRADE_NOTE: The initialization of  'mimeHeaders' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private System.Collections.Hashtable mimeHeaders;
		private InputDataStream ids;
	}
}