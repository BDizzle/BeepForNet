/*
* StartChannelException.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
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
namespace beepcore.beep.core
{
	
	
	/// <summary>A subclass of BEEPError designed to return
	/// information about the errors associated with a close
	/// channel request.  Call <code>getCode</code> and
	/// <code>getMessage</code> to retrieve error info ;)</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision, $Date: 2004/09/21 08:46:57 $</version>
	public class StartChannelException:BEEPError
	{
		
		/// <summary>Constructor StartChannelException</summary>
		/// <param name="code"></param>
		/// <param name="diagnostic"></param>
		/// <param name="xmlLang"></param>
		public StartChannelException(BEEPStatusCode code, string diagnostic, string xmlLang):base(code, diagnostic, xmlLang)
		{
		}
		
		/// <summary>Constructor StartChannelException</summary>
		/// <param name="code"></param>
		/// <param name="diagnostic"></param>
		public StartChannelException(BEEPStatusCode code, string diagnostic):base(code, diagnostic)
		{
		}
		
		/// <summary>Constructor StartChannelException</summary>
		/// <param name="code"></param>
		public StartChannelException(BEEPStatusCode code):base(code)
		{
		}
	}
}