/*
* MessageListener.java $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
	
#if MESSAGELISTENER	
	/// <summary>Provided to allow for the registration of a <code>MessageListener</code>
	/// on a given Channel.
	/// A <code>MessageListener</code> receives <code>Messages</code> from a
	/// <code>Channel</code>.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public interface MessageListener
	{
		
		/// <summary>Receives a BEEP message of type MSG.</summary>
		/// <param name="message"><code>Message</code></param>
		/// @throws BEEPError
		/// <seealso cref="<code>beepcore.beep.core.Message</code>"></seealso>
		void  receiveMSG(Message message);
	}
#endif
}
