/*
* HeaderParser.java            $Revision: 1.1 $ $Date: 2004/09/21 08:47:00 $
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
using BEEPException = beepcore.beep.core.BEEPException;
namespace beepcore.beep.util
{
	
	/// <summary></summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:47:00 $</version>
	public class HeaderParser
	{
		internal const int MAX_INT_STRING_LENGTH = 10;
		internal const long UNSIGNED_INT_MAX_VALUE = 4294967295L;
		
		private byte[] buf;
		private int off = 0;
		private int len;
		
		public HeaderParser(byte[] buf, int len)
		{
			this.buf = buf;
			this.len = len;
		}
		
		public bool hasMoreTokens()
		{
			return off < len;
		}
		
		public bool parseLast()
		{
			if (hasMoreTokens() == false)
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			int tl = tokenLength();
			if (tl != 1)
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			char c = (char) (buf[off] & 0x7f);
			
			if (c != '*' && c != '.')
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			findNextToken(tl);
			
			return c == '.';
		}
		
		public int parseInt()
		{
			if (hasMoreTokens() == false)
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			int tl = tokenLength();
			
			int i = atoi(buf, off, tl);
			
			findNextToken(tl);
			
			return i;
		}
		
		public long parseUnsignedInt()
		{
			if (hasMoreTokens() == false)
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			int tl = tokenLength();
			
			long l = atoui(buf, off, tl);
			
			findNextToken(tl);
			
			return l;
		}
		
		public string parseType()
		{
			if (hasMoreTokens() == false)
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			int tl = tokenLength();
			if (tl != 3)
			{
				throw new BEEPException("Malformed BEEP Header");
			}
			
			string s = System.Text.Encoding.ASCII.GetString(buf,off,3);

//			char[] c = new char[3];
//			
//			c[0] = (char) (buf[off] & 0xff);
//			c[1] = (char) (buf[off + 1] & 0xff);
//			c[2] = (char) (buf[off + 2] & 0xff);
			
			findNextToken(tl);
			
			return s;
		}
		
		private void findNextToken(int previousLength)
		{
			off += previousLength + 1;
			
			if (off > len)
				return ;
			
			if (off == len || buf[off - 1] != ' ' || buf[off] == ' ')
			{
				throw new BEEPException("Malformed BEEP Header");
			}
		}
		
		private int tokenLength()
		{
			for (int i = off; i < len; ++i)
			{
				if (buf[i] == ' ')
				{
					return i - off;
				}
			}
			return len - off;
		}
		
		private static int atoi(byte[] b, int off, int len)
		{
			long res = atoui(b, off, len);
			if (res > System.Int32.MaxValue)
			{
				throw new System.FormatException();
			}
			
			return (int) res;
		}
		
		private static long atoui(byte[] b, int off, int len)
		{
			if (len > MAX_INT_STRING_LENGTH)
			{
				throw new System.FormatException();
			}
			
			long res = 0;
			for (int i = off; i < len + off; ++i)
			{
				if (b[i] < (byte) '0' || b[i] > (byte) '9')
				{
					System.Console.Out.WriteLine("b[" + i + "] = " + b[i]);
					throw new System.FormatException();
				}
				
				res *= 10;
				res += (b[i] - (byte) '0');
			}
			
			if (res > UNSIGNED_INT_MAX_VALUE)
			{
				throw new System.FormatException();
			}

			return res;
		}
	}
}