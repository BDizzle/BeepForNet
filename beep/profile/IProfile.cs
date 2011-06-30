/*
* Profile.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
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
using beepcore.beep.core;

namespace beepcore.beep.profile
{
	
	
	/// <summary>All profile implementations that register with this
	/// beep library must implement these methods.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	public interface IProfile
	{
		
		/// <summary>Initializes the profile and returns the
		/// <code>StartChannelListener</code> for <code>uri</code>.</summary>
		/// <param name="uri"></param>
		/// <param name="config"></param>
		/// <returns>The corresponding <code>StartChannelListener</code> for
		/// the specified uri.</returns>
		/// <exception cref="BEEPException" />
		IStartChannelListener Init(string uri, ProfileConfiguration config);
		
		/// <summary>Called when the <code>StartChannelListener</code> is removed from
		/// the <code>ProfileRegistry</code>.</summary>
		//    public void Shutdown();
	}
}