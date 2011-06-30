/*
* BEEPException.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
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
namespace beepcore.beep.core
{
	/// <summary>An exception for representing BEEP related errors.
	/// <code>BEEPException</code> adds support for exception chaining
	/// similar to what is available in JDK 1.4.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class BEEPException:System.Exception
	{
		/// <summary>Constructs a new <code>BEEPException</code> with the specified
		/// detail message.</summary>
		public BEEPException(string message):base(message)
		{
		}
		
		/// <summary>Constructs a new <code>BEEPException</code> with the specified
		/// innerException </summary>
		public BEEPException(System.Exception innerException):base(innerException==null?null:innerException.Message,innerException)
		{
		}
		
		/// <summary>Constructs a new <code>BEEPException</code> with the specified
		/// cause and detailed message.</summary>
		public BEEPException(string message, System.Exception innerException):base(message, innerException)
		{
		}
	}
}