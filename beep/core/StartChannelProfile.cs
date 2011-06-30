/*
* StartChannelProfile.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
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
	/// <summary>Class StartChannelProfile</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	public class StartChannelProfile
	{
		internal string uri;
		internal bool base64Encoding;
		internal string data;
		
		/// <summary>Constructor StartChannelProfile</summary>
		/// <param name="uri"></param>
		/// <param name="base64Encoding"></param>
		/// <param name="data"></param>
		public StartChannelProfile(string uri, bool base64Encoding, string data)
		{
			this.uri = uri;
			this.base64Encoding = base64Encoding;
			this.data = data;
		}
		
		/// <summary>Constructor StartChannelProfile</summary>
		/// <param name="uri"></param>
		public StartChannelProfile(string uri) : this(uri, false, null)
		{
		}
	}
}